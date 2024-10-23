/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 */

//#define ENABLE_DEBUG_STATS_SERVICE

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Stats;
    
    using Samples;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public class StatsService : EOSService
    {
        #region Singleton Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the StatsService
        /// </summary>
        private static readonly Lazy<StatsService> s_LazyInstance = new(() => new StatsService());

        /// <summary>
        /// Accessor for the instance.
        /// </summary>
        public static StatsService Instance
        {
            get
            {
                return s_LazyInstance.Value;
            }
        }

        /// <summary>
        /// Private constructor guarantees adherence to thread-safe singleton
        /// pattern.
        /// </summary>
        private StatsService() { }

        ~StatsService()
        {
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// Maps a given user to a list of player statistics.
        /// </summary>
        private readonly ConcurrentDictionary<ProductUserId, List<Stat>> _playerStats = new();

        protected override void Reset()
        {
            // Clear any player stats that may have been locally cached.
            _playerStats.Clear();

            // Call the base implementation.
            base.Reset();
        }

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [Conditional("ENABLE_DEBUG_STATS_SERVICE")]
        private static void Log(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }

        protected async override void OnLoggedIn(AuthenticationListener.LoginChangeKind changeType)
        {
            if (TryGetProductUserId(out ProductUserId userId))
            {
                await RefreshPlayerStatsAsync(userId);
            }
        }

        /// <summary>
        /// Gets the Stats Interface from the EOS SDK.
        /// </summary>
        /// <returns>
        /// A references to the StatsInterface from the EOS SDK
        /// </returns>
        private static StatsInterface GetEOSStatsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
        }

        protected async override Task InternalRefreshAsync()
        {
            // Add all the refresh player stats tasks to an array so we can
            // await the completion of all of them, and allow them to happen
            // concurrently.
            List<Task> refreshPlayerStatsTasks = new();
            foreach (var playerId in _playerStats.Keys)
            {
                refreshPlayerStatsTasks.Add(RefreshPlayerStatsAsync(playerId));
            }

            await Task.WhenAll(refreshPlayerStatsTasks);

            NotifyUpdated();
        }

        /// <summary>
        /// Refresh the locally cached data pertaining to Stats for the given
        /// user.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId of the player whose stats should be refreshed.
        /// </param>
        private async Task RefreshPlayerStatsAsync(ProductUserId productUserId)
        {
            List<Stat> playerStats = await QueryPlayerStatsAsync(productUserId);

            // TODO: In the lambda function to update stats, the stats could be
            //       inspected for equality. If it is determined they have not
            //       changed, then refreshing the achievement manager could be
            //       skipped, as could invoking NotifyUpdated()
            _playerStats.AddOrUpdate(productUserId, playerStats, (id, previousStats) => playerStats);
            
            // Because statistics can change achievements, refresh the 
            // achievements service as well.
            await AchievementsService.Instance.RefreshAsync();
        }

        /// <summary>
        /// Ingest a given stat for the current player.
        /// </summary>
        /// <param name="statName">The stat name.</param>
        /// <param name="ingestAmount">
        /// The amount to "ingest" for the stat.
        /// </param>
        /// <returns>
        /// A task so that this function can be awaitable.
        /// </returns>
        public Task IngestStatAsync(string statName, int ingestAmount)
        {
            ProductUserId userId = EOSManager.Instance.GetProductUserId();
            IngestStatOptions ingestOptions = new()
            {
                LocalUserId = userId,
                TargetUserId = userId,
                Stats = new[] { new IngestData() { StatName = statName, IngestAmount = ingestAmount } }
            };

            TaskCompletionSource<object> taskCompletionSource = new();

            GetEOSStatsInterface().IngestStat(ref ingestOptions, null, (ref IngestStatCompleteCallbackInfo data) =>
            {
                taskCompletionSource.SetResult(null);
                _ = AchievementsService.Instance.RefreshAsync();
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Queries from the server the stats pertaining to the user associated
        /// to the given ProductUserId.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId associated with the player to get the statistics
        /// for.
        /// </param>
        /// <returns>
        /// List of stats for the indicated player.
        /// </returns>
        private static Task<List<Stat>> QueryPlayerStatsAsync(ProductUserId productUserId)
        {
            if (!productUserId.IsValid())
            {
                Log("Invalid product user id sent in!");
                return Task.FromResult(new List<Stat>());
            }
            var statInterface = GetEOSStatsInterface();

            QueryStatsOptions statsOptions = new()
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };

            TaskCompletionSource<List<Stat>> tcs = new();

            statInterface.QueryStats(ref statsOptions, null, (ref OnQueryStatsCompleteCallbackInfo queryStatsCompleteCallbackInfo) =>
            {
                if (queryStatsCompleteCallbackInfo.ResultCode != Result.Success)
                {
                    tcs.SetException(new Exception($"Failed to query stats, result code: {queryStatsCompleteCallbackInfo.ResultCode}"));
                }
                else
                {
                    tcs.SetResult(GetCachedPlayerStats(productUserId));
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Returns the list of statistics for a given player that have been
        /// locally cached. Note that reading the cached statistics is the only
        /// means by which statistics for a player can be accessed.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId for a given player.
        /// </param>
        /// <returns>
        /// A list of statistics pertaining to the player represented by the
        /// given ProductUserId
        /// </returns>
        private static List<Stat> GetCachedPlayerStats(ProductUserId productUserId)
        {
            var statInterface = GetEOSStatsInterface();
            GetStatCountOptions countOptions = new()
            {
                TargetUserId = productUserId
            };
            uint statsCountForProductUserId = statInterface.GetStatsCount(ref countOptions);

            List<Stat> collectedStats = new();
            CopyStatByIndexOptions copyStatsByIndexOptions = new()
            {
                TargetUserId = productUserId,
                StatIndex = 0
            };

            for (uint i = 0; i < statsCountForProductUserId; ++i)
            {
                copyStatsByIndexOptions.StatIndex = i;

                Result copyStatResult = statInterface.CopyStatByIndex(ref copyStatsByIndexOptions, out Stat? stat);

                if (copyStatResult == Result.Success && stat.HasValue)
                {
                    collectedStats.Add(stat.Value);
                }
            }

            return collectedStats;
        }
    }
}
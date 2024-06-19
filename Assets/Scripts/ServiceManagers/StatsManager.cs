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
 * Author: Paul Hazen (paul.hazen@playeveryware.com)
 *
 * Date:   2024-06-17
 *
 * Notes: This file contains implementation of a manager for the stats interface
 *        of the EOS SDK, extracted from where it previously existed within the
 *        EOSAchievementManager class.
 *
 */

//#define ENABLE_DEBUG_EOSSTATSMANAGER

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

    public class StatsManager : ServiceManager
    {
        #region Singleton Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the StatsManager
        /// </summary>
        private static readonly Lazy<StatsManager> s_LazyInstance = new(() => new StatsManager());

        /// <summary>
        /// Accessor for the instance.
        /// </summary>
        public static StatsManager Instance
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
        private StatsManager() { }

        #endregion

        /// <summary>
        /// Maps a given user to a list of player statistics.
        /// </summary>
        private ConcurrentDictionary<ProductUserId, List<Stat>> _playerStats = new();

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [Conditional("ENABLE_DEBUG_EOSSTATSMANAGER")]
        private static void Log(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }

        protected async override void OnPlayerLogin(ProductUserId productUserId)
        {
            await RefreshPlayerStatsAsync(productUserId);
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

        public async override Task RefreshAsync()
        {
            foreach (var playerId in _playerStats.Keys)
            {
                await RefreshPlayerStatsAsync(playerId);
            }
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
            await EOSAchievementManager.Instance.RefreshAsync();

            NotifyUpdated();
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
                _ = EOSAchievementManager.Instance.RefreshAsync();
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
                    // TODO: handle error
                    Log($"Failed to query stats, result code: {queryStatsCompleteCallbackInfo.ResultCode}");
                    tcs.SetResult(new List<Stat>());
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
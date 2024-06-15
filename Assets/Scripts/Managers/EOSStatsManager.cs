/*
 * Copyright (c) 2021 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#define ENABLE_DEBUG_EOSSTATSMANAGER

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Stats;
using System;
using Epic.OnlineServices;
using System.Diagnostics;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using Epic.OnlineServices.Presence;
    using System.Threading.Tasks;

    public class EOSStatsManager : Service
    {
        /// <summary>
        /// Maps a given user to a list of player statistics.
        /// </summary>
        private IDictionary<ProductUserId, List<Stat>> _playerStats = new Dictionary<ProductUserId, List<Stat>>();

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [Conditional("ENABLE_DEBUG_EOSSTATSMANAGER")]
        private static void Log(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
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

        /// <summary>
        /// Queries from the server the stats pertaining to the user associated
        /// to the given ProductUserId.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId associated with the player to get the statistics
        /// for.
        /// </param>
        /// <param name="callback">
        /// Invoked when the query has completed (successfully or otherwise).
        /// </param>
        private static void QueryPlayerStats(ProductUserId productUserId, OnQueryStatsCompleteCallback callback)
        {
            if (!productUserId.IsValid())
            {
                Log("Invalid product user id sent in!");
                return;
            }
            var statInterface = GetEOSStatsInterface();

            QueryStatsOptions statsOptions = new()
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };

            statInterface.QueryStats(ref statsOptions, null, (ref OnQueryStatsCompleteCallbackInfo queryStatsCompleteCallbackInfo) =>
            {
                if (queryStatsCompleteCallbackInfo.ResultCode != Result.Success)
                {
                    // TODO: handle error
                    Log($"Failed to query stats, result code: {queryStatsCompleteCallbackInfo.ResultCode}");
                }
                callback?.Invoke(ref queryStatsCompleteCallbackInfo);
            });
        }

        public void IngestStat(ProductUserId productUserId, string statName, int ingestAmount, Action callback)
        {
            // TODO: Make all of this async awaitable
            IngestStatOptions options = new()
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId,
                Stats = new[] { new IngestData() { StatName = statName, IngestAmount = ingestAmount } }
            };

            GetEOSStatsInterface().IngestStat(ref options, null, (ref IngestStatCompleteCallbackInfo data) =>
            {
                UnityEngine.Debug.Log($"Stat ingest result: {data.ResultCode}.");

                // TODO: Make refresh async / awaitable
                Refresh();

                callback.Invoke();
            });
        }

        protected override void PostLogin(ProductUserId productUserId)
        {
            RefreshPlayerStats(productUserId);
        }

        protected override void RefreshLocalData()
        {
            foreach (ProductUserId productUserId in _playerStats.Keys)
            {
                RefreshPlayerStats(productUserId);
            }
        }

        private void RefreshPlayerStats(ProductUserId productUserId)
        {
            QueryPlayerStats(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsInfo) =>
            {
                _playerStats[productUserId] = GetCachedPlayerStats(productUserId);
            });
        }
    }
}
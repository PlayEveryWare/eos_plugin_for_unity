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

//#define ENABLE_DEBUG_EOSACHIEVEMENTMANAGER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Http;
using System.Collections.Concurrent;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.UserInfo;
using Epic.OnlineServices.Stats;
using System;
using Epic.OnlineServices;
using System.Diagnostics;
using Epic.OnlineServices.Auth;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using Epic.OnlineServices.Connect;

    /// <summary>
    /// Class <c>EOSAchievementManager</c> is a simplified wrapper for EOS [Stats Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Achievements/index.html).
    /// </summary>

    public class EOSStatsManager : IEOSSubManager, IEOSOnConnectLogin
    {
        /// <summary>
        /// Cache of the map between the product user id and the stats for that user.
        /// </summary>
        private Dictionary<ProductUserId, List<Stat>> _productUserIdToStatsCache = new();

        public void OnConnectLogin(LoginCallbackInfo loginCallbackInfo)
        {
            ProductUserId productUserId = loginCallbackInfo.LocalUserId;
            QueryStatsForProductUserId(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
            {
                CacheStatsForProductUserId(productUserId);
            });
        }

        /// <summary>
        /// Gets the StatsInterface for EOS
        /// </summary>
        /// <returns>The stats interface.</returns>
        private StatsInterface GetEOSStatsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
        }

        private void CacheStatsForProductUserId(ProductUserId productUserId)
        {
            var statInterface = GetEOSStatsInterface();
            GetStatCountOptions countOptions = new()
            {
                TargetUserId = productUserId
            };
            uint statsCountForProductUserId = statInterface.GetStatsCount(ref countOptions);

            List<Stat> collectedStats = new();
            var copyStatsByIndexOptions = new CopyStatByIndexOptions
            {
                TargetUserId = productUserId,
                StatIndex = 0
            };

            for (uint i = 0; i < statsCountForProductUserId; ++i)
            {
                copyStatsByIndexOptions.StatIndex = i;

                Result copyStatResult = statInterface.CopyStatByIndex(ref copyStatsByIndexOptions, out Stat? stat);

                if (copyStatResult == Result.Success)
                {
                    collectedStats.Add(stat.Value);
                }
            }

            _productUserIdToStatsCache[productUserId] = collectedStats;
        }

        private void QueryStatsForProductUserId(ProductUserId productUserId, OnQueryStatsCompleteCallback callback)
        {
            if (!productUserId.IsValid())
            {
                print("Invalid product user id sent in!");
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
                    print("Failed to query stats: " + queryStatsCompleteCallbackInfo.ResultCode);
                }
                if (callback != null)
                {
                    callback(ref queryStatsCompleteCallbackInfo);
                }
            });
        }

        public void RefreshData()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            QueryStatsForProductUserId(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
            {
                CacheStatsForProductUserId(productUserId);
            });
        }

        public void IngestStat(string statName, int statDelta)
        {
            var statsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
            var userId = EOSManager.Instance.GetProductUserId();
            IngestStatOptions ingestOptions = new()
            {
                LocalUserId = userId,
                TargetUserId = userId,
                Stats = new IngestData[] { new() { StatName = statName, IngestAmount = statDelta } }
            };

            statsInterface.IngestStat(ref ingestOptions, null, (ref IngestStatCompleteCallbackInfo info) =>
            {
                UnityEngine.Debug.LogFormat("Stat ingest result: {0}", info.ResultCode.ToString());
                RefreshData();
            });
        }

        public void SetStat(string statName, int statValue)
        {
            // First refresh so that the value we have is up-to-date
            RefreshData();

            var userStats = _productUserIdToStatsCache[EOSManager.Instance.GetProductUserId()];
            
            int requiredDelta = statValue;

            foreach (var stat in userStats)
            {
                // skip if it's not the stat we want to update
                if (stat.Name != statName)
                    continue;

                requiredDelta = statValue - stat.Value;

                break;
            }

            IngestStat(statName, requiredDelta);
        }

        [Conditional("ENABLE_DEBUG_EOSSTATSMANAGER")]
        static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }
    }
}
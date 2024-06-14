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
    /// <summary>
    /// Class <c>EOSAchievementManager</c> is a simplified wrapper for EOS [Achievements Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Achievements/index.html).
    /// </summary>

    public class EOSAchievementManager : IEOSSubManager, IEOSOnConnectLogin
    {
        private ConcurrentDictionary<string, byte[]> _downloadCache = new();
        private Dictionary<string, DefinitionV2> _achievementDefinitionCache = new();

        private ConcurrentDictionary<ProductUserId, List<PlayerAchievement>> _productUserIdToPlayerAchievement = new();
        private Dictionary<ProductUserId, List<Stat>> _productUserIdToStatsCache = new();

        private List<Action> _achievementDataUpdatedCallbacks = new();

        [Conditional("ENABLE_DEBUG_EOSACHIEVEMENTMANAGER")]
        private static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }
        
        private static string ProductUserIdToString(ProductUserId productUserId)
        {
            productUserId.ToString(out Utf8String buffer);
            return buffer;
        }

        #region Private

        private void CacheStatsForProductUserId()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
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

            if (collectedStats.Count > 0)
            {
                _productUserIdToStatsCache[productUserId] = collectedStats;
            }
        }

        private void QueryStatsForProductUserId(OnQueryStatsCompleteCallback callback)
        {
            QueryStatsOptions statsOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetUserId = EOSManager.Instance.GetProductUserId()
            };

            GetEOSStatsInterface().QueryStats(ref statsOptions, null, (ref OnQueryStatsCompleteCallbackInfo queryStatsCompleteCallbackInfo) =>
            {
                if (queryStatsCompleteCallbackInfo.ResultCode != Result.Success)
                {
                    // TODO: handle error
                    print("Failed to query stats: " + queryStatsCompleteCallbackInfo.ResultCode);
                }

                callback?.Invoke(ref queryStatsCompleteCallbackInfo);
            });
        }

        private StatsInterface GetEOSStatsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
        }

        private AchievementsInterface GetEOSAchievementsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetAchievementsInterface();
        }

        private void QueryAchievementDefinitions(OnQueryDefinitionsCompleteCallback callback = null)
        {
            var options = new QueryDefinitionsOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            GetEOSAchievementsInterface().QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    print($"Unable to query achievement definitions: {data.ResultCode}");
                }

                callback?.Invoke(ref data);
            });
        }

        private void QueryPlayerAchievements(OnQueryPlayerAchievementsCompleteCallback callback)
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();

            if (!productUserId.IsValid())
            {
                return;
            }

            print("Begin query player achievements for " + ProductUserIdToString(productUserId));

            QueryPlayerAchievementsOptions options = new()
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };

            GetEOSAchievementsInterface().QueryPlayerAchievements(ref options, new WeakReference(this), (ref OnQueryPlayerAchievementsCompleteCallbackInfo data) =>
            {
                if (((WeakReference)data.ClientData).Target is not EOSAchievementManager strongThis)
                {
                    return;
                }

                if (data.ResultCode != Epic.OnlineServices.Result.Success)
                {
                    print("Error after query player achievements: " + data.ResultCode);
                }

                callback?.Invoke(ref data);
            });
        }

        private void CachePlayerAchievements()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            var eosAchievementInterface = GetEOSAchievementsInterface();
            var achievementCountOptions = new GetPlayerAchievementCountOptions
            {
                UserId = productUserId
            };

            uint achievementCount = eosAchievementInterface.GetPlayerAchievementCount(ref achievementCountOptions);
            CopyPlayerAchievementByIndexOptions playerAchievementByIndexOptions = new()
            {
                AchievementIndex = 0,
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };

            print("Fetching achievements");
            var collectedAchievements = new List<PlayerAchievement>();
            for (uint i = 0; i < achievementCount; ++i)
            {
                playerAchievementByIndexOptions.AchievementIndex = i;
                var copyResult = eosAchievementInterface.CopyPlayerAchievementByIndex(ref playerAchievementByIndexOptions, out PlayerAchievement? playerAchievement);
                if (copyResult != Result.Success)
                {
                    print("Failed to copy player achievement : " + copyResult);
                    continue; // TODO handle error
                }
                if (playerAchievement.HasValue)
                {
                    collectedAchievements.Add(playerAchievement.Value);
                }
            }

            _productUserIdToPlayerAchievement[productUserId] = collectedAchievements;

            foreach (var callback in _achievementDataUpdatedCallbacks)
            {
                callback?.Invoke();
            }
        }

        private void CacheAchievementDefinitions()
        {
            uint achievementDefinitionCount = GetAchievementDefinitionCount();
            var options = new CopyAchievementDefinitionV2ByIndexOptions
            {
                AchievementIndex = 0
            };

            for (uint i = 0; i < achievementDefinitionCount; ++i)
            {
                options.AchievementIndex = i;
                GetEOSAchievementsInterface().CopyAchievementDefinitionV2ByIndex(ref options, out DefinitionV2? definition);

                if (!definition.HasValue) continue;

                _achievementDefinitionCache[definition.Value.AchievementId] = definition.Value;

                CacheIconDataFromURI(definition.Value.LockedIconURL);
                CacheIconDataFromURI(definition.Value.UnlockedIconURL);
            }
        }

        private async void CacheIconDataFromURI(string uri)
        {
            if (_downloadCache.ContainsKey(uri))
            {
                return;
            }

            using DownloadHandlerBuffer downloadHandler = new();
            using UnityWebRequest request = UnityWebRequest.Get(uri);
            request.downloadHandler = downloadHandler;

            UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
            }

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.Success)
#else
                if (!request.isNetworkError && !request.isHttpError)
#endif
            {
                _downloadCache[uri] = downloadHandler.data;
            }
        }

        #endregion

        #region Public

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            ProductUserId productUserId = loginCallbackInfo.LocalUserId;
            QueryStatsForProductUserId((ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
            {
                CacheStatsForProductUserId();
                QueryAchievementDefinitions((ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
                {
                    CacheAchievementDefinitions();
                    QueryPlayerAchievements((ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                    {
                        CachePlayerAchievements();
                    });
                });
            });

        }

        public IEnumerable<DefinitionV2> EnumerateCachedAchievementDefinitions()
        {
            foreach (var achievementDefinition in _achievementDefinitionCache.Values)
            {
                yield return achievementDefinition;
            }
        }

        public void RefreshData()
        {
            QueryAchievementDefinitions((ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                CacheAchievementDefinitions();
                foreach (var userId in _productUserIdToStatsCache.Keys)
                {
                    QueryStatsForProductUserId((ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
                    {
                        CacheStatsForProductUserId();
                        QueryPlayerAchievements((ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                        {
                            CachePlayerAchievements();
                        });
                    });
                }

                QueryStatsForProductUserId((ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
                {
                    CacheStatsForProductUserId();
                    QueryPlayerAchievements((ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                    {
                        CachePlayerAchievements();
                    });
                });

            });
        }

        public Texture2D GetAchievementUnlockedIconTexture(string achievementId)
        {
            return GetAchievementIconTexture(achievementId, (definition) => definition.UnlockedIconURL);
        }

        public Texture2D GetAchievementLockedIconTexture(string achievementId)
        {
            return GetAchievementIconTexture(achievementId, (definition) => definition.LockedIconURL);
        }

        private Texture2D GetAchievementIconTexture(string achievementId, Func<DefinitionV2,string> uriSelector)
        {
            // Return null if the achievement id given isn't cached.
            if (!_achievementDefinitionCache.TryGetValue(achievementId, out DefinitionV2 definition))
                return null;
            
            // Return null if the icon bytes aren't cached
            if (!_downloadCache.TryGetValue(uriSelector.Invoke(definition), out byte[] iconBytes))
                return null;

            // Return null if the image cannot be loaded from the bytes.
            Texture2D iconTexture = new(2, 2);
            if (!iconTexture.LoadImage(iconBytes))
                return null;

            // Otherwise return the texture
            return iconTexture;
        }

        public DefinitionV2 GetAchievementDefinition(string achievementId)
        {
            return _achievementDefinitionCache[achievementId];
        }

        public IEnumerable<PlayerAchievement> EnumerateCachedPlayerAchievement(ProductUserId productUserId)
        {
            if (!_productUserIdToPlayerAchievement.ContainsKey(productUserId))
            {
                yield break;
            }

            foreach (var playerAchievement in _productUserIdToPlayerAchievement[productUserId])
            {
                yield return playerAchievement;
            }
        }

        public uint GetAchievementDefinitionCount()
        {
            var getAchievementDefinitionCountOptions = new GetAchievementDefinitionCountOptions();

            uint achievementDefinitionCount = GetEOSAchievementsInterface().GetAchievementDefinitionCount(ref getAchievementDefinitionCountOptions);

            UnityEngine.Debug.LogFormat("Achievements (GetAchievementDefinitionCount): Count={0}", achievementDefinitionCount);

            return achievementDefinitionCount;
        }

        // TODO: Create a callback version of this method
        public void UnlockAchievement(string achievementId, OnUnlockAchievementsCompleteCallback callback = null)
        {
            var eosAchievementOption = new UnlockAchievementsOptions
            {
                UserId = EOSManager.Instance.GetProductUserId(),
                AchievementIds = new Utf8String[] { achievementId }
            };

            GetEOSAchievementsInterface().UnlockAchievements(ref eosAchievementOption, null, callback);
        }

        public void AddNotifyAchievementDataUpdated(Action callback)
        {
            _achievementDataUpdatedCallbacks.Add(callback);
            if (_productUserIdToPlayerAchievement.Count > 0)
            {
                callback?.Invoke();
            }
        }

        public void RemoveNotifyAchievementDataUpdated(Action callback)
        {
            _achievementDataUpdatedCallbacks.Remove(callback);
        }

        #endregion
    }
}
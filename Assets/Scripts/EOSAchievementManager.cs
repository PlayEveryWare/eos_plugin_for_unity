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
            Utf8String buffer;
            productUserId.ToString(out buffer);
            return buffer;
        }




        #region Private

        //-------------------------------------------------------------------------
        private void CacheStatsForProductUserId(ProductUserId productUserId)
        {
            var statInterface = GetEOSStatsInterface();
            GetStatCountOptions countOptions = new GetStatCountOptions
            {
                TargetUserId = productUserId
            };
            uint statsCountForProductUserId = statInterface.GetStatsCount(ref countOptions);

            List<Stat> collectedStats = new List<Stat>();
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

        //-------------------------------------------------------------------------
        // may only be called after logging in with connect
        private void QueryStatsForProductUserId(ProductUserId productUserId, OnQueryStatsCompleteCallback callback)
        {
            if (!productUserId.IsValid())
            {
                print("Invalid product user id sent in!");
                return;
            }
            var statInterface = GetEOSStatsInterface();

            QueryStatsOptions statsOptions = new QueryStatsOptions
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


        private StatsInterface GetEOSStatsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
        }

        private AchievementsInterface GetEOSAchievementsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetAchievementsInterface();
        }


        private void QueryAchievementDefinitions(Epic.OnlineServices.ProductUserId productUserId = null, OnQueryDefinitionsCompleteCallback callback = null)
        {
            var options = new QueryDefinitionsOptions
            {
                LocalUserId = productUserId
            };

            GetEOSAchievementsInterface().QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    print("unable to query achievement definitions: " + data.ResultCode.ToString());
                }

                callback?.Invoke(ref data);
            });
        }

        private void QueryPlayerAchievements(Epic.OnlineServices.ProductUserId productUserId, OnQueryPlayerAchievementsCompleteCallback callback)
        {
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

        private Texture2D GetTextureFromBytes(byte[] achievementIconBytes)
        {
            if (achievementIconBytes != null)
            {
                var toReturn = new Texture2D(2, 2);
                if (toReturn.LoadImage(achievementIconBytes))
                {
                    return toReturn;
                }
            }
            return null;
        }

        private byte[] GetAchievementUnlockedIconBytes(string achievementId)
        {
            if (!_achievementDefinitionCache.TryGetValue(achievementId, out DefinitionV2 definition))
            {
                return null;
            }

            return _downloadCache.TryGetValue(definition.UnlockedIconURL, out byte[] toReturn) ? toReturn : null;
        }

        private byte[] GetAchievementLockedIconBytes(string achievementId)
        {
            if (!_achievementDefinitionCache.TryGetValue(achievementId, out DefinitionV2 definition))
            {
                return null;
            }

            return _downloadCache.TryGetValue(definition.LockedIconURL, out byte[] toReturn) ? toReturn : null;
        }

        private IEnumerator<DefinitionV2> EnumeratorForCachedAchievementDefinitions()
        {
            foreach (var achievementDefinition in _achievementDefinitionCache.Values)
            {
                yield return achievementDefinition;
            }
        }

        
        private IEnumerator<PlayerAchievement> EnumeratorForCachedPlayerAchievement(ProductUserId productUserId)
        {
            if (_productUserIdToPlayerAchievement.ContainsKey(productUserId))
            {
                foreach (var playerAchievement in _productUserIdToPlayerAchievement[productUserId])
                {
                    yield return playerAchievement;
                }
            }
        }

        private void CacheAllPlayerAchievements(ProductUserId productUserId)
        {
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

            print("Fetching achievments");
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

        //-------------------------------------------------------------------------
        private void CacheAllAchievementDefinitions()
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

                CacheAchievementDef(definition.Value);
            }
        }

        private void CacheAchievementDef(DefinitionV2 achievementDef)
        {
            _achievementDefinitionCache[achievementDef.AchievementId] = achievementDef;

            DownloadIconDataFromURI(achievementDef.LockedIconURL);
            DownloadIconDataFromURI(achievementDef.UnlockedIconURL);
        }

        //-------------------------------------------------------------------------
        private async void DownloadIconDataFromURI(string uri)
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

        //-------------------------------------------------------------------------
        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            ProductUserId productUserId = loginCallbackInfo.LocalUserId;
            QueryStatsForProductUserId(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
            {
                CacheStatsForProductUserId(productUserId);
                QueryAchievementDefinitions(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
                {
                    CacheAllAchievementDefinitions();
                    QueryPlayerAchievements(productUserId, (ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                    {
                        CacheAllPlayerAchievements(productUserId);
                    });
                });
            });

        }

        public IEnumerable<DefinitionV2> EnumerateCachedAchievementDefinitions()
        {
            return new EnumerableWrapper<DefinitionV2>(EnumeratorForCachedAchievementDefinitions());
        }


        public void RefreshData()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            QueryAchievementDefinitions(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                CacheAllAchievementDefinitions();
                foreach (var userId in _productUserIdToStatsCache.Keys)
                {
                    QueryStatsForProductUserId(userId, (ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
                    {
                        CacheStatsForProductUserId(userId);
                        QueryPlayerAchievements(userId, (ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                        {
                            CacheAllPlayerAchievements(userId);
                        });
                    });
                }
                //A user with empty stats would not be added to "productUserIdToStatsCache"
                //This chunk of code makes up for that and welcomes the newcomers.
                if (!_productUserIdToStatsCache.ContainsKey(productUserId))
                {
                    QueryStatsForProductUserId(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
                    {
                        CacheStatsForProductUserId(productUserId);
                        QueryPlayerAchievements(productUserId, (ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                        {
                            CacheAllPlayerAchievements(productUserId);
                        });
                    });
                }
            });
        }

        public Texture2D GetAchievementUnlockedIconTexture(string achievementId)
        {
            byte[] achievementIconBytes = GetAchievementUnlockedIconBytes(achievementId);
            return GetTextureFromBytes(achievementIconBytes);
        }

        public Texture2D GetAchievementLockedIconTexture(string achievementId)
        {
            byte[] achievementIconBytes = GetAchievementLockedIconBytes(achievementId);
            return GetTextureFromBytes(achievementIconBytes);
        }

        public DefinitionV2 GetAchievementDefinition(string achievementId)
        {
            return _achievementDefinitionCache[achievementId];
        }

        public IEnumerable<PlayerAchievement> EnumerateCachedPlayerAchievement(ProductUserId productUserId)
        {
            return new EnumerableWrapper<PlayerAchievement>(EnumeratorForCachedPlayerAchievement(productUserId));
        }

        public uint GetAchievementDefinitionCount()
        {
            var getAchievementDefinitionCountOptions = new GetAchievementDefinitionCountOptions();

            uint achievementDefinitionCount = GetEOSAchievementsInterface().GetAchievementDefinitionCount(ref getAchievementDefinitionCountOptions);

            UnityEngine.Debug.LogFormat("Achievements (GetAchievementDefinitionCount): Count={0}", achievementDefinitionCount);

            return achievementDefinitionCount;
        }

        //-------------------------------------------------------------------------
        // TODO: Create a callback version of this method
        public void UnlockAchievementManually(string achievementId, OnUnlockAchievementsCompleteCallback callback = null)
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
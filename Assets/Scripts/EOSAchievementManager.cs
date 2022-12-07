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
        private ConcurrentDictionary<string, byte[]> downloadCache;
        private List<DefinitionV2> achievementDefinitionCache;
        //private ConcurrentDictionary<ProductUserId, UserInfoData> productUserIdToUserInfoData;
        private ConcurrentDictionary<ProductUserId, List<PlayerAchievement>> productUserIdToPlayerAchievement;
        private AchievementsInterface eosAchievementInterface;

        private Dictionary<ProductUserId, List<Stat>> productUserIdToStatsCache = new Dictionary<ProductUserId, List<Stat>>();

        private List<Action> achievementDataUpdatedCallbacks;

        public EOSAchievementManager()
        {
            downloadCache = new ConcurrentDictionary<string, byte[]>();
            achievementDefinitionCache = new List<DefinitionV2>();
            //productUserIdToUserInfoData = new ConcurrentDictionary<ProductUserId, UserInfoData>();
            productUserIdToPlayerAchievement = new ConcurrentDictionary<ProductUserId, List<PlayerAchievement>>();
            achievementDataUpdatedCallbacks = new List<Action>();
        }


        [Conditional("ENABLE_DEBUG_EOSACHIEVEMENTMANAGER")]
        static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }

        //-------------------------------------------------------------------------
        private PlatformInterface GetEOSPlatformInterface()
        {
            var eosPlatformInterface = EOSManager.Instance.GetEOSPlatformInterface();
            return eosPlatformInterface;
        }

        //-------------------------------------------------------------------------
        private Epic.OnlineServices.Stats.StatsInterface GetEOSStatsInterface()
        {
            return GetEOSPlatformInterface().GetStatsInterface();
        }

        private string ProductUserIdToString(ProductUserId productUserId)
        {
            Utf8String buffer;
            productUserId.ToString(out buffer);
            return buffer;
        }

        private WeakReference NewWeakThis()
        {
            return new WeakReference(this);
        }

        // Assumes that the data is actually a weak reference
        private EOSAchievementManager StrongThisFromClientData(object data)
        {
            return ((data as WeakReference).Target as EOSAchievementManager);
        }

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
                productUserIdToStatsCache[productUserId] = collectedStats;
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

        //-------------------------------------------------------------------------
        private Epic.OnlineServices.UserInfo.UserInfoInterface GetEOSUserInfoInterface()
        {
            return GetEOSPlatformInterface().GetUserInfoInterface();
        }

        //-------------------------------------------------------------------------
        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            ProductUserId productUserId = loginCallbackInfo.LocalUserId;
            QueryStatsForProductUserId(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsQueryData) =>
            {
                CacheStatsForProductUserId(productUserId);
                QueryAchievementDefinitions(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
                {
                    CacheAllAchievementDefinitions(productUserId);
                    QueryPlayerAchievements(productUserId, (ref OnQueryPlayerAchievementsCompleteCallbackInfo playerAchiQueryData) =>
                    {
                        CacheAllPlayerAchievements(productUserId);
                    });
                });
            });

        }

        public void RefreshData()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            QueryAchievementDefinitions(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                CacheAllAchievementDefinitions(productUserId);
                foreach (var userId in productUserIdToStatsCache.Keys)
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
                if (!productUserIdToStatsCache.ContainsKey(productUserId))
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

        //-------------------------------------------------------------------------
        /*public void QueryUserInformation(EpicAccountId localEpicAccountId, EpicAccountId targetEpicAccountId)
        {
            var options = new QueryUserInfoOptions
            {
                LocalUserId = localEpicAccountId,
                TargetUserId = targetEpicAccountId
            };

            GetEOSUserInfoInterface().QueryUserInfo(ref options, null, (ref QueryUserInfoCallbackInfo data) =>
            {
                var copyUserInfoOptions = new CopyUserInfoOptions
                {
                    LocalUserId = localEpicAccountId,
                    TargetUserId = targetEpicAccountId
                };
                GetEOSUserInfoInterface().CopyUserInfo(ref copyUserInfoOptions, out UserInfoData? userInfoData);

            });
        }*/

        //-------------------------------------------------------------------------
        AchievementsInterface GetEOSAchievementInterface()
        {
            if (eosAchievementInterface == null)
            {
                var eosPlatformInterface = EOSManager.Instance.GetEOSPlatformInterface();
                eosAchievementInterface = eosPlatformInterface.GetAchievementsInterface();
            }
            return eosAchievementInterface;
        }

        //-------------------------------------------------------------------------
        public void QueryAchievementDefinitions(Epic.OnlineServices.ProductUserId productUserId = null, OnQueryDefinitionsCompleteCallback callback = null)
        {
            var options = new QueryDefinitionsOptions
            {
                LocalUserId = productUserId
            };

            GetEOSAchievementInterface().QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    print("unable to query achievement definitions: " + data.ResultCode.ToString());
                }

                callback?.Invoke(ref data);
            });
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Helper to wrap up differences between different versions of the EOS SDK
        /// </summary>
        private QueryPlayerAchievementsOptions MakeQueryPlayerAchievementsOptions(Epic.OnlineServices.ProductUserId productUserId)
        {
            return new QueryPlayerAchievementsOptions
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Get the latest stats and player progress towards unlocking achievements
        /// </summary>
        /// <param name="productUserId"></param>
        public void QueryPlayerAchievements(Epic.OnlineServices.ProductUserId productUserId, OnQueryPlayerAchievementsCompleteCallback callback)
        {
            if (!productUserId.IsValid())
            {
                return;
            }

            print("Begin query player achievements for " + ProductUserIdToString(productUserId));

            QueryPlayerAchievementsOptions options = MakeQueryPlayerAchievementsOptions(productUserId);

            GetEOSAchievementInterface().QueryPlayerAchievements(ref options, NewWeakThis(), (ref OnQueryPlayerAchievementsCompleteCallbackInfo data) =>
            {
                var strongThis = StrongThisFromClientData(data.ClientData);
                if (strongThis != null)
                {
                    if (data.ResultCode != Epic.OnlineServices.Result.Success)
                    {
                        print("Error after query player achievements: " + data.ResultCode);
                    }

                    callback?.Invoke(ref data);
                }
            });
        }

        //-------------------------------------------------------------------------
        public void OnShutdown()
        {
            // Do things to clean up stuff
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

        //-------------------------------------------------------------------------
        public byte[] GetAchievementUnlockedIconBytes(string achievementId)
        {
            foreach (var achievementDef in achievementDefinitionCache)
            {
                if (achievementDef.AchievementId == achievementId)
                {
                    if (downloadCache.TryGetValue(achievementDef.UnlockedIconURL, out byte[] toReturn))
                    {
                        return toReturn;
                    }

                }
            }

            return null;
        }

        //-------------------------------------------------------------------------
        public byte[] GetAchievementLockedIconBytes(string achievementId)
        {
            foreach (var achievementDef in achievementDefinitionCache)
            {
                if (achievementDef.AchievementId == achievementId)
                {
                    if (downloadCache.TryGetValue(achievementDef.LockedIconURL, out byte[] toReturn))
                    {
                        return toReturn;
                    }

                }
            }

            return null;
        }

        public IEnumerator<DefinitionV2> EnumeratorForCachedAchievementDefinitions()
        {
            foreach (var achievementDefinition in achievementDefinitionCache)
            {
                yield return achievementDefinition;
            }
        }

        public IEnumerable<DefinitionV2> EnumerateCachedAchievementDefinitions()
        {
            return new EnumerableWrapper<DefinitionV2>(EnumeratorForCachedAchievementDefinitions());
        }

        public DefinitionV2 GetAchievementDefinitionAtIndex(int idx)
        {
            return achievementDefinitionCache[idx];
        }

        public IEnumerator<PlayerAchievement> EnumeratorForCachedPlayerAchievement(ProductUserId productUserId)
        {
            if (productUserIdToPlayerAchievement.ContainsKey(productUserId))
            {
                foreach (var playerAchievement in productUserIdToPlayerAchievement[productUserId])
                {
                    yield return playerAchievement;
                }
            }
        }
        public IEnumerable<PlayerAchievement> EnumerateCachedPlayerAchievement(ProductUserId productUserId)
        {
            return new EnumerableWrapper<PlayerAchievement>(EnumeratorForCachedPlayerAchievement(productUserId));
        }

        public uint GetAchievementDefinitionCount()
        {
            var getAchievementDefinitionCountOptions = new GetAchievementDefinitionCountOptions();

            uint achievementDefinitionCount = GetEOSAchievementInterface().GetAchievementDefinitionCount(ref getAchievementDefinitionCountOptions);

            UnityEngine.Debug.LogFormat("Achievements (GetAchievementDefinitionCount): Count={0}", achievementDefinitionCount);

            return achievementDefinitionCount;
        }

        //-------------------------------------------------------------------------
        // TODO: Create a callback version of this method
        public void UnlockAchievementManually(string achievementId,OnUnlockAchievementsCompleteCallback callback)
        {
            var eosAchievementInterface = GetEOSAchievementInterface();
            var localUserId = EOSManager.Instance.GetProductUserId();
            var eosAchievementOption = new UnlockAchievementsOptions
            {
                UserId = localUserId,
                AchievementIds = new Utf8String[] { achievementId }
            };

            eosAchievementInterface.UnlockAchievements(ref eosAchievementOption, null, callback);
        }
        // TODO: Create a debug mode to check if the achievement is valid?
        public void UnlockAchievementManually(string achievementId)
        {
            var eosAchievementInterface = GetEOSAchievementInterface();
            var localUserId = EOSManager.Instance.GetProductUserId();
            var eosAchievementOption = new UnlockAchievementsOptions
            {
                UserId = localUserId,
                AchievementIds = new Utf8String[] { achievementId }
            };

            eosAchievementInterface.UnlockAchievements(ref eosAchievementOption, null, null);
        }

        //-------------------------------------------------------------------------
        private CopyPlayerAchievementByIndexOptions MakeCopyPlayerAchievementByIndexOptions(ProductUserId productUserId)
        {
            return new CopyPlayerAchievementByIndexOptions
            {
                AchievementIndex = 0,
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };
        }

        //-------------------------------------------------------------------------
        private void CacheAllPlayerAchievements(ProductUserId productUserId)
        {
            var eosAchievementInterface = GetEOSAchievementInterface();
            var achievementCountOptions = new GetPlayerAchievementCountOptions
            {
                UserId = productUserId
            };

            uint achievementCount = eosAchievementInterface.GetPlayerAchievementCount(ref achievementCountOptions);
            var playerAchievementByIndexOptions = MakeCopyPlayerAchievementByIndexOptions(productUserId);

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

            productUserIdToPlayerAchievement[productUserId] = collectedAchievements;

            foreach (var callback in achievementDataUpdatedCallbacks)
            {
                callback?.Invoke();
            }
        }

        public void AddNotifyAchievementDataUpdated(Action Callback)
        {
            achievementDataUpdatedCallbacks.Add(Callback);
            if (productUserIdToPlayerAchievement.Count > 0)
            {
                Callback?.Invoke();
            }
        }

        public void RemoveNotifyAchievementDataUpdated(Action Callback)
        {
            achievementDataUpdatedCallbacks.Remove(Callback);
        }

        //-------------------------------------------------------------------------
        private void CacheAllAchievementDefinitions(ProductUserId productUserId)
        {
            uint achievementDefinitionCount = GetAchievementDefinitionCount();
            var options = new CopyAchievementDefinitionV2ByIndexOptions
            {
                AchievementIndex = 0
            };

            for (uint i = 0; i < achievementDefinitionCount; ++i)
            {
                options.AchievementIndex = i;
                GetEOSAchievementInterface().CopyAchievementDefinitionV2ByIndex(ref options, out DefinitionV2? definition);
                cacheAchievementDef(definition);

            }
        }

        //-------------------------------------------------------------------------
        private void CacheAchievementDefById(string achievementId)
        {
            var eosPlatformInterface = EOSManager.Instance.GetEOSPlatformInterface();
            var eosAchievementInterface = eosPlatformInterface.GetAchievementsInterface();

            var options = new CopyAchievementDefinitionV2ByAchievementIdOptions();

            options.AchievementId = achievementId;

            eosAchievementInterface.CopyAchievementDefinitionV2ByAchievementId(ref options, out DefinitionV2? achievementDef);
            cacheAchievementDef(achievementDef);
        }

        private bool DoesCacheContainAchievementWithID(Utf8String achievementID)
        {
            //TODO: make this not copy by value when enumerating
            foreach(DefinitionV2 achievementDef in achievementDefinitionCache)
            {
                if(achievementID == achievementDef.AchievementId)
                {
                    return true;
                }
            }

            return false;
        }

        private void cacheAchievementDef(DefinitionV2? achievementDef)
        {
            Utf8String achievementID = achievementDef.Value.AchievementId;

            if (!DoesCacheContainAchievementWithID(achievementID))
            {
                achievementDefinitionCache.Add(achievementDef.Value);
            }
            UnityEngine.Debug.LogFormat("Achievements (cacheAchievementDef): Id={0}, LockedDisplayName={1}", achievementDef?.AchievementId, achievementDef?.LockedDisplayName);

            DownloadIconDataFromURI(achievementDef?.LockedIconURL);
            DownloadIconDataFromURI(achievementDef?.UnlockedIconURL);
        }

        //-------------------------------------------------------------------------
        private async void DownloadIconDataFromURI(string uri)
        {
            if (downloadCache.ContainsKey(uri))
            {
                return;
            }

            using (DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer())
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                request.downloadHandler = downloadHandler;

                UnityEngine.Networking.UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
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
                    downloadCache[uri] = downloadHandler.data;
                }
            }
        }
    }
}
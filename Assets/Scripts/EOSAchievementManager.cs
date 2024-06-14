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
    /// <summary>
    /// Class <c>EOSAchievementManager</c> is a simplified wrapper for
    /// EOS [Achievements Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Achievements/index.html).
    /// </summary>
    public class EOSAchievementManager : IEOSSubManager, IEOSOnConnectLogin
    {
        /// <summary>
        /// Stores data that has been cached, where the string key is a URI,
        /// and the value is the bytes of the resource at that URI.
        /// </summary>
        private static ConcurrentDictionary<string, byte[]> _downloadCache = new();

        /// <summary>
        /// Contains a list of the achievements that exist for the game.
        /// </summary>
        private IList<DefinitionV2> _achievements = new List<DefinitionV2>();
        
        /// <summary>
        /// Maps a given user to a list of player achievements.
        /// </summary>
        private ConcurrentDictionary<ProductUserId, List<PlayerAchievement>> _playerAchievements = new();
        
        /// <summary>
        /// Maps a given user to a list of player statistics.
        /// </summary>
        private IDictionary<ProductUserId, List<Stat>> _playerStats = new Dictionary<ProductUserId, List<Stat>>();

        /// <summary>
        /// Stores a list of functions to be called whenever data related to
        /// Achievements has been updated locally.
        /// </summary>
        private IList<Action> _dataUpdatedNotifiers = new List<Action>();

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint"></param>
        [Conditional("ENABLE_DEBUG_EOSACHIEVEMENTMANAGER")]
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
        /// Helper function to convert a ProductUserId to string.
        /// </summary>
        /// <param name="productUserId">The ProductUserId to convert.</param>
        /// <returns>String representation of the given ProductUserId.</returns>
        private static string ProductUserIdToString(ProductUserId productUserId)
        {
            productUserId.ToString(out Utf8String buffer);
            return buffer;
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
                    Log("Failed to query stats: " + queryStatsCompleteCallbackInfo.ResultCode);
                }
                callback?.Invoke(ref queryStatsCompleteCallbackInfo);
            });
        }

        /// <summary>
        /// This method is called by EOSManager when login has happened.
        /// TODO: It may not be necessary or wise to call this upon login. it
        /// might make more sense to call it on demand.
        /// </summary>
        /// <param name="loginCallbackInfo">
        /// Information pertaining to the login that took place.
        /// </param>
        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            ProductUserId productUserId = loginCallbackInfo.LocalUserId;

            QueryAchievements(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                _achievements = GetCachedAchievements();
                RefreshPlayerStatsAndAchievements(productUserId);
            });
        }

        /// <summary>
        /// Refreshes all the data related to the achievements and statistics.
        /// This encompasses updating the achievement definitions in addition to
        /// updating the details of the achievements and statistics that relate
        /// to any player for whom that information has been requested
        /// previously.
        /// </summary>
        public void Refresh()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            QueryAchievements(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                _achievements = GetCachedAchievements();
                foreach (var userId in _playerStats.Keys)
                {
                    RefreshPlayerStatsAndAchievements(userId);
                }
                //A user with empty stats would not be added to "productUserIdToStatsCache"
                //This chunk of code makes up for that and welcomes the newcomers.
                if (!_playerStats.ContainsKey(productUserId))
                {
                    RefreshPlayerStatsAndAchievements(productUserId);    
                }
            });
        }

        /// <summary>
        /// Replaces the current set of statistics and achievements that
        /// correspond to the user indicated by the given ProductUserId with
        /// values from the server.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId associated with the player for whom the statistics
        /// and achievements are to be updated for.
        /// </param>
        private void RefreshPlayerStatsAndAchievements(ProductUserId productUserId)
        {
            QueryPlayerStats(productUserId, (ref OnQueryStatsCompleteCallbackInfo statsInfo) =>
            {
                _playerStats[productUserId] = GetCachedPlayerStats(productUserId);
            });

            QueryPlayerAchievements(productUserId, (ref OnQueryPlayerAchievementsCompleteCallbackInfo achievementsInfo) =>
            {
                _playerAchievements[productUserId] = GetCachedPlayerAchievements(productUserId);
            });

            NotifyAchievementDataUpdated();
        }

        /// <summary>
        /// Gets a reference to the AchievementsInterface from the EOS SDK.
        /// </summary>
        /// <returns>
        /// Reference to the AchievementsInterface within the EOS SDK.
        /// </returns>
        private static AchievementsInterface GetEOSAchievementInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetAchievementsInterface();
        }

        /// <summary>
        /// Queries the server for the achievement definitions for the game.
        /// Note that while a ProductUserId is a required parameter, it is used
        /// exclusively for the purpose of getting Locale-specific versions of
        /// the achievement strings.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId that corresponds to the player. This is used
        /// exclusively for the purposes of determining which Locale should be
        /// used when determining on the server side what string properties to
        /// assign to the Achievements that will be returned.
        /// </param>
        /// <param name="callback">
        /// Invoked when the request from the server has completed - whether
        /// successful or not.
        /// </param>
        private static void QueryAchievements(ProductUserId productUserId, OnQueryDefinitionsCompleteCallback callback)
        {
            var options = new QueryDefinitionsOptions
            {
                LocalUserId = productUserId
            };

            GetEOSAchievementInterface().QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    Log("unable to query achievement definitions: " + data.ResultCode.ToString());
                }

                callback?.Invoke(ref data);
            });
        }

        /// <summary>
        /// Requests from the server the set of achievements that are associated
        /// with the player that corresponds to the given ProductUserId.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId that corresponds to the player for whom the
        /// achievements are being queried.
        /// </param>
        /// <param name="callback">
        /// Invoked when the request from the server has completed - whether
        /// successful or not.
        /// </param>
        private static void QueryPlayerAchievements(ProductUserId productUserId, OnQueryPlayerAchievementsCompleteCallback callback)
        {
            if (!productUserId.IsValid())
            {
                return;
            }

            Log($"Begin query player achievements for {ProductUserIdToString(productUserId)}");

            QueryPlayerAchievementsOptions options = new()
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId
            }; 

            GetEOSAchievementInterface().QueryPlayerAchievements(ref options, null, (ref OnQueryPlayerAchievementsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    Log("Error after query player achievements: " + data.ResultCode);
                }

                callback?.Invoke(ref data);
            });
        }

        /// <summary>
        /// Retrieves the texture for the achievement to be used when a player
        /// has unlocked the achievement.
        /// </summary>
        /// <param name="achievementId">
        /// The id of the achievement to get the unlocked icon texture of.
        /// </param>
        /// <returns>
        /// The texture to be used when a player has unlocked the achievement.
        /// </returns>
        public Texture2D GetAchievementUnlockedIconTexture(string achievementId)
        {
            return GetAchievementIconTexture(achievementId, v2 => v2.UnlockedIconURL);
        }

        /// <summary>
        /// Retrieves the texture for the achievement to be used when a player
        /// has not unlocked the achievement.
        /// </summary>
        /// <param name="achievementId">
        /// The id of the achievement to get the locked icon texture of.
        /// </param>
        /// <returns>
        /// The texture to be used when a player has not unlocked the
        /// achievement.
        /// </returns>
        public Texture2D GetAchievementLockedIconTexture(string achievementId)
        {
            return GetAchievementIconTexture(achievementId, v2 => v2.LockedIconURL);
        }

        /// <summary>
        /// Retrieves the icon texture associated with the given achievement ID
        /// given the provided uri selector.
        /// </summary>
        /// <param name="achievementId">
        /// The id of the achievement to get the icon texture of.
        /// </param>
        /// <param name="uriSelector">
        /// Function that takes the achievement definition and determines which
        /// URI corresponds to the texture being requested.
        /// </param>
        /// <returns>
        /// The icon texture associated with the given achievement ID and
        /// determined by the given URI selector.
        /// </returns>
        private Texture2D GetAchievementIconTexture(string achievementId, Func<DefinitionV2, string> uriSelector)
        {
            Texture2D iconTexture = null;
            foreach (var achievementDef in _achievements)
            {
                if (achievementDef.AchievementId != achievementId)
                    continue;

                if (_downloadCache.TryGetValue(uriSelector(achievementDef), out byte[] iconBytes) && null != iconBytes)
                {
                    Texture2D textureFromBytes = new(2, 2);
                    if (textureFromBytes.LoadImage(iconBytes))
                    {
                        iconTexture = textureFromBytes;
                    }
                    else
                    {
                        // TODO: Deal with circumstances where image could not be loaded for some reason
                    }
                    break;
                }
                else
                {
                    // TODO: Deal with circumstance where icon bytes are not cached, or are null
                }
            }

            return iconTexture;
        }

        /// <summary>
        /// Returns an IEnumerable of the achievement definitions that have been
        /// cached by the EOS SDK. Should be called after QueryAchievements()
        /// TODO: Provide a guarantee that the QueryAchievements function has
        ///       been called at least once before this is called - failing that
        ///       provide a descriptive warning to users.
        /// </summary>
        /// <returns>The achievements for the game.</returns>
        public IEnumerable<DefinitionV2> CachedAchievements()
        {
            foreach (var achievementDefinition in _achievements)
            {
                yield return achievementDefinition;
            }
        }

        /// <summary>
        /// Returns the achievement definition at the given index. The index is
        /// the index of the achievement as it exists in the collection of
        /// achievements on the server side - so it is not a particularly
        /// meaningful means to get the achievement definition, unless you are
        /// enumerating all of them.
        /// </summary>
        /// <param name="idx">
        /// The achievement definition at the index - where the index is the
        /// index of the achievement on the server side.
        /// </param>
        /// <returns>
        /// The definition of an achievement from the dev portal.
        /// </returns>
        public DefinitionV2 GetAchievementDefinitionAtIndex(int idx)
        {
            return _achievements[idx];
        }

        /// <summary>
        /// Enumerates the collection of cached achievements for the player
        /// associated with the provided ProductUserId.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId that corresponds to the player for whom the
        /// achievements are to be enumerated.
        /// </param>
        /// <returns>
        /// The cached achievements for the given player.
        /// </returns>
        public IEnumerable<PlayerAchievement> CachedPlayerAchievements(ProductUserId productUserId)
        {
            if (!_playerAchievements.TryGetValue(productUserId, out List<PlayerAchievement> achievements))
            {
                yield break;
            }

            foreach (PlayerAchievement achievement in achievements)
            {
                yield return achievement;
            }
        }

        /// <summary>
        /// Gets the number of achievement definitions that have been cached
        /// locally.
        /// </summary>
        /// <returns>Number of cached achievement definitions.</returns>
        public static uint GetAchievementsCount()
        {
            var getAchievementDefinitionCountOptions = new GetAchievementDefinitionCountOptions();

            uint achievementDefinitionCount = GetEOSAchievementInterface().GetAchievementDefinitionCount(ref getAchievementDefinitionCountOptions);

            UnityEngine.Debug.LogFormat("Achievements (GetAchievementDefinitionCount): Count={0}", achievementDefinitionCount);

            return achievementDefinitionCount;
        }

        /// <summary>
        /// Unlocks the achievement for the current player.
        /// TODO: Create a callback version of this method
        /// </summary>
        /// <param name="achievementId">
        /// The id of the achievement to unlock for the current player.
        /// </param>
        /// <param name="callback">
        /// Invoked upon the completion of the attempt to unlock the achievement
        /// for the given player.
        /// </param>
        public void UnlockAchievement(string achievementId, OnUnlockAchievementsCompleteCallback callback)
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

        /// <summary>
        /// Gets the locally cached collection of achievements for the player
        /// that corresponds to the provided ProductUserId.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId associated with the player for whom the cached
        /// achievements are being requested.
        /// </param>
        /// <returns>
        /// The cached list of achievements that the player has.
        /// </returns>
        private static List<PlayerAchievement> GetCachedPlayerAchievements(ProductUserId productUserId)
        {
            GetPlayerAchievementCountOptions achievementCountOptions = new()
            {
                UserId = productUserId
            };

            uint achievementCount = GetEOSAchievementInterface().GetPlayerAchievementCount(ref achievementCountOptions);
            CopyPlayerAchievementByIndexOptions playerAchievementByIndexOptions = new()
            {
                AchievementIndex = 0,
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };

            Log("Fetching achievements");
            var collectedAchievements = new List<PlayerAchievement>();
            for (uint i = 0; i < achievementCount; ++i)
            {
                playerAchievementByIndexOptions.AchievementIndex = i;
                var copyResult = GetEOSAchievementInterface().CopyPlayerAchievementByIndex(ref playerAchievementByIndexOptions, out PlayerAchievement? playerAchievement);
                if (copyResult != Result.Success)
                {
                    Log("Failed to copy player achievement : " + copyResult);
                    continue; // TODO handle error
                }
                if (playerAchievement.HasValue)
                {
                    collectedAchievements.Add(playerAchievement.Value);
                }
            }

            return collectedAchievements;
        }

        /// <summary>
        /// Trigger the notification callbacks that have been registered,
        /// letting any consumers know that the data base been updated.
        /// </summary>
        private void NotifyAchievementDataUpdated()
        {
            foreach (Action callback in _dataUpdatedNotifiers)
            {
                callback?.Invoke();
            }
        }

        /// <summary>
        /// Adds a given callback - to be invoked by the manager whenever
        /// data pertaining to the manager has been updated.
        /// </summary>
        /// <param name="callback">
        /// The callback to invoke when data pertaining to this manager has been
        /// updated locally.
        /// </param>
        public void AddNotifyAchievementDataUpdated(Action callback)
        {
            _dataUpdatedNotifiers.Add(callback);
            if (_playerAchievements.Count > 0)
            {
                callback?.Invoke();
            }
        }

        /// <summary>
        /// Removes a specific callback from the list of callbacks to call when
        /// data for achievements and stats has been updated locally.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void RemoveNotifyAchievementDataUpdated(Action callback)
        {
            _dataUpdatedNotifiers.Remove(callback);
        }

        /// <summary>
        /// Retrieves the list of achievement definitions for the game that have
        /// been cached locally.
        /// </summary>
        /// <returns>A list of achievement definitions.</returns>
        private static List<DefinitionV2> GetCachedAchievements()
        {
            uint achievementDefinitionCount = GetAchievementsCount();
            var options = new CopyAchievementDefinitionV2ByIndexOptions
            {
                AchievementIndex = 0
            };

            List<DefinitionV2> achievements = new();

            for (uint i = 0; i < achievementDefinitionCount; ++i)
            {
                options.AchievementIndex = i;
                GetEOSAchievementInterface().CopyAchievementDefinitionV2ByIndex(ref options, out DefinitionV2? definition);

                // Move on if the definition is empty
                if (!definition.HasValue)
                    continue;

                achievements.Add(definition.Value);

                UnityEngine.Debug.LogFormat("Achievements (CacheAchievementDef): Id={0}, LockedDisplayName={1}", definition.Value.AchievementId, definition.Value.LockedDisplayName);

                GetAndCacheData(definition.Value.LockedIconURL);
                GetAndCacheData(definition.Value.UnlockedIconURL);
            }

            return achievements;
        }

        /// <summary>
        /// Gets and subsequently caches the data at the given URI.
        /// </summary>
        /// <param name="uri">
        /// The URI to get bytes from.
        /// </param>
        private static async void GetAndCacheData(string uri)
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
    }
}
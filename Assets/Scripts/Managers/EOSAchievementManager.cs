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

#define ENABLE_DEBUG_EOSACHIEVEMENTMANAGER

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
    using Debug = System.Diagnostics.Debug;

    /// <summary>
    /// Class <c>EOSAchievementManager</c> is a simplified wrapper for
    /// EOS [Achievements Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Achievements/index.html).
    /// </summary>
    public class EOSAchievementManager : Service
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
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [Conditional("ENABLE_DEBUG_EOSACHIEVEMENTMANAGER")]
        private static void Log(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
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
        /// This method is called by EOSManager when login has happened.
        /// TODO: It may not be necessary or wise to call this upon login. it
        /// might make more sense to call it on demand.
        /// </summary>
        /// <param name="productUserId">
        /// The product user id for the logged in player. Might be null if the
        /// login was unsuccessful.
        /// </param>
        protected override void PostLogin(ProductUserId productUserId)
        {
            QueryAchievements(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                _achievements = GetCachedAchievements();
                RefreshPlayerAchievements(productUserId);
            });
        }

        /// <summary>
        /// Refreshes all the data related to the achievements and statistics.
        /// This encompasses updating the achievement definitions in addition to
        /// updating the details of the achievements and statistics that relate
        /// to any player for whom that information has been requested
        /// previously.
        /// </summary>
        protected override void RefreshLocalData()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            QueryAchievements(productUserId, (ref OnQueryDefinitionsCompleteCallbackInfo defQueryData) =>
            {
                _achievements = GetCachedAchievements();
                foreach (var userId in _playerAchievements.Keys)
                {
                    RefreshPlayerAchievements(userId);
                }
                //A user with empty stats would not be added to "productUserIdToStatsCache"
                //This chunk of code makes up for that and welcomes the newcomers.
                if (!_playerAchievements.ContainsKey(productUserId))
                {
                    RefreshPlayerAchievements(productUserId);    
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
        private void RefreshPlayerAchievements(ProductUserId productUserId)
        {
            QueryPlayerAchievements(productUserId, (ref OnQueryPlayerAchievementsCompleteCallbackInfo achievementsInfo) =>
            {
                _playerAchievements[productUserId] = GetCachedPlayerAchievements(productUserId);
            });

            NotifyListeners();
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
        /// the achievements.
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
                    Log($"Unable to query achievement definitions. Result code: {data.ResultCode}");
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
                    Log($"Error querying player achievements. Result code: {data.ResultCode}");
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
                    Log($"Failed to copy player achievement from the cache. Result code: {copyResult}");
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
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

//#define ENABLE_DEBUG_ACHIEVEMENTS_SERVICE

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    using UnityEngine;
    using UnityEngine.Networking;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Achievements;
    using System.Threading.Tasks;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Class <c>AchievementsService</c> is a simplified wrapper for
    /// EOS [Achievements Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Achievements/index.html).
    /// </summary>
    public class AchievementsService : EOSService
    {
        /// <summary>
        /// Stores data that has been cached, where the string key is a URI,
        /// and the value is the bytes of the resource at that URI.
        /// </summary>
        private ConcurrentDictionary<string, byte[]> _downloadCache = new();

        /// <summary>
        /// Contains a list of the achievements that exist for the game.
        /// </summary>
        private IList<DefinitionV2> _achievements = new List<DefinitionV2>();
        
        /// <summary>
        /// Maps a given user to a list of player achievements.
        /// </summary>
        private ConcurrentDictionary<ProductUserId, List<PlayerAchievement>> _playerAchievements = new();

        #region Singleton Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the AchievementsService
        /// </summary>
        private static readonly Lazy<AchievementsService> s_LazyInstance = new(() => new AchievementsService());

        /// <summary>
        /// Accessor for the instance.
        /// </summary>
        public static AchievementsService Instance
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
        private AchievementsService() { }

        #endregion

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [Conditional("ENABLE_DEBUG_ACHIEVEMENTS_SERVICE")]
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

        protected async override void OnLoggedIn(AuthenticationListener.LoginChangeKind changeType)
        {
            await RefreshAsync();
        }

        protected override void Reset()
        {
            _downloadCache.Clear();
            _achievements.Clear();
            _playerAchievements.Clear();

            base.Reset();
        }

        /// <summary>
        /// Refreshes all the data related to the achievements and statistics.
        /// This encompasses updating the achievement definitions in addition to
        /// updating the details of the achievements and statistics that relate
        /// to any player for whom that information has been requested
        /// previously.
        /// </summary>
        protected async override Task InternalRefreshAsync()
        {
            if (!TryGetProductUserId(out ProductUserId productUser)) 
            {
                return;
            }
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            _achievements = await QueryAchievementsAsync(productUserId);

            // If the user is not in the list, then add it.
            if (!_playerAchievements.ContainsKey(productUserId))
            {
                _playerAchievements.AddOrUpdate(productUserId, new List<PlayerAchievement>(),
                    (id, list) => new List<PlayerAchievement>());
            }

            List<Task> refreshPlayerAchievementsTasks = new();
            foreach (var userId in _playerAchievements.Keys)
            {
                refreshPlayerAchievementsTasks.Add(RefreshPlayerAchievementsAsync(userId));
            }

            await Task.WhenAll(refreshPlayerAchievementsTasks);

            // NOTE: Because there is no check in the above code to determine if
            //       any achievements have actually changed, EVERY call to this
            //       Refresh function will trigger a data updated callback to
            //       all listeners, regardless of whether anything has changed.
            //       Possible improvement would be to evaluate each achievement
            //       definition, and the achievement for each player and only 
            //       trigger this callback if a change is detected. However,
            //       comparing equality of the PlayerAchievement struct (defined
            //       in the EOS SDK) is not be default supported, so that would
            //       need to be implemented.
            NotifyUpdated();
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
        private async Task RefreshPlayerAchievementsAsync(ProductUserId productUserId)
        {
            var playerAchievements = await QueryPlayerAchievementsAsync(productUserId);

            // TODO: In the lambda function to update achievements, the
            //       achievements could be inspected for equality. If it is
            //       determined they have not changed, then NotifyUpdated()
            //       would not need to be called.
            _playerAchievements.AddOrUpdate(productUserId, playerAchievements, 
                (id, previousPlayerAchievements) => playerAchievements);
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
        private Task<List<DefinitionV2>> QueryAchievementsAsync(ProductUserId productUserId)
        {
            var options = new QueryDefinitionsOptions
            {
                LocalUserId = productUserId
            };

            TaskCompletionSource<List<DefinitionV2>> tcs = new();

            GetEOSAchievementInterface().QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    Log($"Unable to query achievement definitions. Result code: {data.ResultCode}");
                }

                tcs.SetResult(GetCachedAchievements());
            });

            return tcs.Task;
        }

        /// <summary>
        /// Retrieves the list of achievement definitions for the game that have
        /// been cached locally.
        /// </summary>
        /// <returns>A list of achievement definitions.</returns>
        private List<DefinitionV2> GetCachedAchievements()
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
                Result copyResult = GetEOSAchievementInterface().CopyAchievementDefinitionV2ByIndex(ref options, out DefinitionV2? definition);

                // Log a warning and continue if the achievement was not copied
                if (copyResult != Result.Success)
                {
                    UnityEngine.Debug.LogWarning($"Could not copy achievement definition from cache. Result code: {Enum.GetName(typeof(Result), copyResult)}");
                    continue;
                }

                // Move on if the definition is empty
                if (!definition.HasValue)
                    continue;

                achievements.Add(definition.Value);

                Log($"Achievements (CacheAchievementDef): Id={definition.Value.AchievementId}, LockedDisplayName={definition.Value.LockedDisplayName}.");
            }

            return achievements;
        }

        /// <summary>
        /// Requests from the server the set of achievements that are associated
        /// with the player that corresponds to the given ProductUserId.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId that corresponds to the player for whom the
        /// achievements are being queried.
        /// </param>
        /// <returns>
        /// List of PlayerAchievement objects.
        /// </returns>
        private Task<List<PlayerAchievement>> QueryPlayerAchievementsAsync(ProductUserId productUserId)
        {
            Log($"Begin query player achievements for {ProductUserIdToString(productUserId)}");

            QueryPlayerAchievementsOptions options = new()
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId
            };

            TaskCompletionSource<List<PlayerAchievement>> tcs = new();

            GetEOSAchievementInterface().QueryPlayerAchievements(ref options, null, (ref OnQueryPlayerAchievementsCompleteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    Log($"Error querying player achievements. Result code: {data.ResultCode}");
                    tcs.SetResult(new List<PlayerAchievement>());
                }
                else
                {
                    tcs.SetResult(GetCachedPlayerAchievements(productUserId));
                }
            });

            return tcs.Task;
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
        public async Task<Texture2D> GetAchievementUnlockedIconTexture(string achievementId)
        {
            return await GetAchievementIconTexture(achievementId, v2 => v2.UnlockedIconURL);
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
        public async Task<Texture2D> GetAchievementLockedIconTexture(string achievementId)
        {
            return await GetAchievementIconTexture(achievementId, v2 => v2.LockedIconURL);
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
        private async Task<Texture2D> GetAchievementIconTexture(string achievementId, Func<DefinitionV2, string> uriSelector)
        {
            Texture2D textureFromBytes = null;
            
            foreach (var achievementDef in _achievements)
            {
                if (achievementDef.AchievementId != achievementId)
                    continue;

                var uri = uriSelector(achievementDef);
                byte[] iconBytes = null;

                // Download the data
                if (!_downloadCache.ContainsKey(uri))
                {
                    TaskCompletionSource<byte[]> downloadTcs = new();

                    GetAndCacheData(uri, data =>
                    {
                        if (data.result == UnityWebRequest.Result.Success)
                        {
                            downloadTcs.SetResult(data.data);
                            return;
                        }

                        Debug.LogWarning($"Could not download achievement icon: {data.result}.");
                        downloadTcs.SetResult(null);
                    });

                    iconBytes = await downloadTcs.Task;

                    if (null != iconBytes)
                    {
                        _downloadCache[uri] = iconBytes;
                    }
                }
                else
                {
                    _downloadCache.TryGetValue(uri, out iconBytes);
                }


                if (null != iconBytes)
                {
                    textureFromBytes = new Texture2D(2, 2);

                    if (!textureFromBytes.LoadImage(iconBytes))
                    {
                        Debug.LogWarning("Could not load achievement icon bytes into texture.");
                        textureFromBytes = null;
                    }
                }

                break;
            }

            return textureFromBytes;
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

            Log($"Achievements (GetAchievementDefinitionCount): Count={achievementDefinitionCount}");

            return achievementDefinitionCount;
        }

        /// <summary>
        /// Unlocks the achievement for the current player.
        /// TODO: Create a callback version of this method
        /// </summary>
        /// <param name="achievementId">
        /// The id of the achievement to unlock for the current player.
        /// </param>
        public Task UnlockAchievementAsync(string achievementId)
        {
            var localUserId = EOSManager.Instance.GetProductUserId();
            var eosAchievementOption = new UnlockAchievementsOptions
            {
                UserId = localUserId,
                AchievementIds = new Utf8String[] { achievementId }
            };

            TaskCompletionSource<object> tcs = new();

            GetEOSAchievementInterface().UnlockAchievements(ref eosAchievementOption, null,
                (ref OnUnlockAchievementsCompleteCallbackInfo data) =>
                {
                    if (data.ResultCode != Result.Success)
                    {
                        tcs.SetException(new Exception($"Could not unlock achievement. Error code: {Enum.GetName(typeof(Result), data.ResultCode)}"));
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                });

            return tcs.Task;
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


        protected struct DownloadDataCallback
        {
            public byte[] data;
            public UnityWebRequest.Result result;
        }

        /// <summary>
        /// Gets and subsequently caches the data at the given URI.
        /// </summary>
        /// <param name="uri">
        /// The URI to get bytes from.
        /// </param>
        /// <param name="callback">
        /// Action to invoke when the data has been retrieved and cached.
        /// </param>
        private void GetAndCacheData(string uri, Action<DownloadDataCallback> callback)
        {
            if (_downloadCache.ContainsKey(uri))
            {
                return;
            }

            UnityWebRequest request = UnityWebRequest.Get(uri);

            UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();

            asyncOp.completed += operation =>
            {
                DownloadDataCallback callbackInfo = new()
                {
                    result = request.result
                };

#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.Success)
#else
                if (!request.isNetworkError && !request.isHttpError)
#endif
                {
                    _downloadCache[uri] = request.downloadHandler.data;
                    callbackInfo.data = request.downloadHandler.data;
                }

                callback(callbackInfo);

                request.Dispose();
            };
        }
    }
}
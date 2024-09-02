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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Leaderboards;
    using Epic.OnlineServices.Stats;

    /// <summary>
    /// Class <c>EOSLeaderboardManager</c> is a simplified wrapper for EOS [Leaderboard Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Leaderboards/index.html).
    /// </summary>
    public class EOSLeaderboardManager : IEOSSubManager
    {
        private LeaderboardsInterface LeaderboardsHandle;
        private StatsInterface StatsHandle;

        private Dictionary<string, Definition> CachedLeaderboardDefinitions;
        private bool CachedLeaderboardDefinitionsDirty;

        private List<LeaderboardRecord> CachedLeaderboardRecords;
        private bool CachedLeaderboardRecordsDirty;

        private Dictionary<string, List<LeaderboardUserScore>> CachedLeaderboardUserScores;
        private bool CachedLeaderboardUserScoresDirty;

        private DateTimeOffset? EOS_LEADERBOARDS_TIME_UNDEFINED = null;

        // Manager Callbacks
        private OnLeaderboardCallback QueryDefinitionsCallback;
        private OnLeaderboardCallback QueryRanksCallback;
        private OnLeaderboardCallback QueryUserScoresCallback;

        public delegate void OnLeaderboardCallback(Result result);

        public EOSLeaderboardManager()
        {
            CachedLeaderboardDefinitions = new Dictionary<string, Definition>();
            CachedLeaderboardDefinitionsDirty = true;

            CachedLeaderboardRecords = new List<LeaderboardRecord>();
            CachedLeaderboardRecordsDirty = true;

            CachedLeaderboardUserScores = new Dictionary<string, List<LeaderboardUserScore>>();
            CachedLeaderboardUserScoresDirty = true;

            LeaderboardsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetLeaderboardsInterface();
            StatsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();

            QueryDefinitionsCallback = null;
            QueryRanksCallback = null;
            QueryUserScoresCallback = null;
        }

        /// <summary>User Logged In actions</summary>
        /// <list type="bullet">
        ///     <item><description><c>QueryDefinitions()</c></description></item>
        /// </list>
        public void OnLoggedIn()
        {
            QueryDefinitions(null);
        }

        /// <summary>User Logged Out actions</summary>
        /// <list type="bullet">
        ///     <item><description>Clear Cache</description></item>
        /// </list>
        public void OnLoggedOut()
        {
            CachedLeaderboardDefinitions.Clear();
            CachedLeaderboardDefinitionsDirty = true;

            CachedLeaderboardRecords.Clear();
            CachedLeaderboardRecordsDirty = true;

            CachedLeaderboardUserScores.Clear();
            CachedLeaderboardUserScoresDirty = true;
        }

        /// <summary>Returns cached Leaderboard Dictionary where LeaderboardId is the key and <c>Definition</c> is the value.</summary>
        /// <param name="LeaderboardDefinitions">Out parameter returns Dictionary where LeaderboardId is the key and <c>LeaderboardDefinitions</c> is the value.</param>
        /// <returns>True if <c>LeaderboardDefinitions</c> has changed since last call.</returns>
        public bool GetCachedLeaderboardDefinitions(out Dictionary<string, Definition> LeaderboardDefinitions)
        {
            LeaderboardDefinitions = CachedLeaderboardDefinitions;

            bool returnDirty = CachedLeaderboardDefinitionsDirty;

            CachedLeaderboardDefinitionsDirty = false;

            return returnDirty;
        }

        /// <summary>Returns cached <c>LeaderboardRecord</c> List.</summary>
        /// <param name="LeaderboardsRecords">Out parameter returns List of <c>LeaderboardRecord</c>.</param>
        /// <returns>True if <c>LeaderboardRecords</c> has changed since last call.</returns>
        public bool GetCachedLeaderboardRecords(out List<LeaderboardRecord> LeaderboardsRecords)
        {
            LeaderboardsRecords = CachedLeaderboardRecords;

            bool returnDirty = CachedLeaderboardRecordsDirty;

            CachedLeaderboardRecordsDirty = false;

            return returnDirty;
        }

        /// <summary>Returns cached <c>LeaderboardUserScore</c> Dictionary where LeaderboardId is the key.</summary>
        /// <param name="LeaderboardUserScores">Out parameter returns Dictionary where LeaderboardId is the key and <c>LeaderboardUserScore</c> is the value.</param>
        /// <returns>True if <c>LeaderboardUserScores</c> has changed since last call.</returns>
        public bool GetCachedLeaderboardUserScores(out Dictionary<string, List<LeaderboardUserScore>> LeaderboardUserScores)
        {
            LeaderboardUserScores = CachedLeaderboardUserScores;

            bool returnDirty = CachedLeaderboardUserScoresDirty;

            CachedLeaderboardUserScoresDirty = false;

            return returnDirty;
        }

        /// <summary>(async) Initiate query for Leaderboard <c>Definitions</c>.</summary>
        public void QueryDefinitions(OnLeaderboardCallback QueryDefinitionsCompleted)
        {
            QueryLeaderboardDefinitionsOptions options = new QueryLeaderboardDefinitionsOptions()
            {
                StartTime = DateTimeOffset.MinValue,
                EndTime = DateTimeOffset.MaxValue,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            QueryDefinitionsCallback = QueryDefinitionsCompleted;

            LeaderboardsHandle.QueryLeaderboardDefinitions(ref options, null, LeaderboardDefinitionsReceivedCallbackFn); //OnQueryLeaderboardDefinitionsComplete
        }

        private void CacheLeaderboardDefinitions()
        {
            var getLeaderboardDefinitionCountOptions = new GetLeaderboardDefinitionCountOptions();
            uint leaderboardDefinitionsCount = LeaderboardsHandle.GetLeaderboardDefinitionCount(ref getLeaderboardDefinitionCountOptions);

            CachedLeaderboardDefinitions.Clear();

            for (uint definitionIndex = 0; definitionIndex < leaderboardDefinitionsCount; definitionIndex++)
            {
                CopyLeaderboardDefinitionByIndexOptions options = new CopyLeaderboardDefinitionByIndexOptions()
                {
                    LeaderboardIndex = definitionIndex
                };

                Result result = LeaderboardsHandle.CopyLeaderboardDefinitionByIndex(ref options, out Definition? leaderboardDefinition);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Leaderboard (CacheLeaderboardDefinitions): CopyLeaderboardDefinitionByIndex failed '{0}'", result);
                    break;
                }

                CachedLeaderboardDefinitions.Add(leaderboardDefinition?.LeaderboardId, leaderboardDefinition.Value);
            }

            CachedLeaderboardDefinitionsDirty = true;
        }

        /// <summary>Get list of cached Leaderboard <c>Definitions</c></summary>
        /// <returns>List(string) cached DefinitionIds</returns>
        public List<string> GetCachedDefinitionIds()
        {
            string[] keys = new string[CachedLeaderboardDefinitions.Keys.Count];
            CachedLeaderboardDefinitions.Keys.CopyTo(keys, 0);

            return new List<string>(keys);
        }

        /// <summary>Get cached Leaderboard <c>Defintion</c> given a LeaderboardId.</summary>
        /// <param name="leaderboardId">Specified LeaderboardID</param>
        /// <returns>Leaderboard <c>Defintion</c></returns>
        public Definition? GetCachedDefinitionFromId(string leaderboardId)
        {

            if(CachedLeaderboardDefinitions.TryGetValue(leaderboardId, out Definition defintion))
            {
                return defintion;
            }

            return null;
        }

        private void LeaderboardDefinitionsReceivedCallbackFn(ref OnQueryLeaderboardDefinitionsCompleteCallbackInfo data) // OnQueryLeaderboardDefinitionsComplete
        {
            //if (data == null)
            //{
            //    Debug.LogError("Leaderboard (LeaderboardDefinitionsReceivedCallbackFn): data is null");
            //    QueryDefinitionsCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Leaderboard (LeaderboardDefinitionsReceivedCallbackFn): QueryDefinitions error: {0}", data.ResultCode);
                QueryDefinitionsCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Leaderboard (LeaderboardDefinitionsReceivedCallbackFn): Query Definitions Complete");

            CacheLeaderboardDefinitions();

            QueryDefinitionsCallback?.Invoke(Result.Success);
        }

        /// <summary>(async) Initiate query for Leaderboard <c>Ranks</c> given a Leaderboard ID.</summary>
        /// <param name="leaderboardId">Leaderboard ID</param>
        public void QueryRanks(string leaderboardId, OnLeaderboardCallback QueryRanksCompleted)
        {
            QueryLeaderboardRanksOptions options = new QueryLeaderboardRanksOptions()
            {
                LeaderboardId = leaderboardId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            QueryRanksCallback = QueryRanksCompleted;

            LeaderboardsHandle.QueryLeaderboardRanks(ref options, null, LeaderboardRanksReceivedCallbackFn);
        }

        private void CacheLeaderboardRecords()
        {
            var GetLeaderboardRecordCountOptions = new GetLeaderboardRecordCountOptions();
            uint leaderboardRecordsCount = LeaderboardsHandle.GetLeaderboardRecordCount(ref GetLeaderboardRecordCountOptions);

            CachedLeaderboardRecords.Clear();

            for (uint recordIndex = 0; recordIndex < leaderboardRecordsCount; recordIndex++)
            {
                CopyLeaderboardRecordByIndexOptions options = new CopyLeaderboardRecordByIndexOptions()
                {
                    LeaderboardRecordIndex = recordIndex
                };

                Result result = LeaderboardsHandle.CopyLeaderboardRecordByIndex(ref options, out LeaderboardRecord? leaderboardRecord);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Leaderboard (CacheLeaderboardRecords): CopyLeaderboardRecordByIndex failed '{0}'", result);
                    break;
                }

                CachedLeaderboardRecords.Add(leaderboardRecord.Value);
            }

            CachedLeaderboardRecordsDirty = true;
        }

        private void LeaderboardRanksReceivedCallbackFn(ref OnQueryLeaderboardRanksCompleteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Leaderboard (LeaderboardRanksReceivedCallbackFn): data is null");
            //    QueryRanksCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Leaderboard (LeaderboardRanksReceivedCallbackFn): QueryRanks error: {0}", data.ResultCode);
                QueryRanksCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Leaderboard (LeaderboardRanksReceivedCallbackFn): Query Ranks Complete");

            CacheLeaderboardRecords();

            QueryRanksCallback?.Invoke(Result.Success);
        }

        /// <summary>(async) Initiate query for Leaderboard <c>UserScores</c> given a list of LeaderboardIds and list of Users.</summary>
        /// <param name="leaderboardIds">List of LeaderboardIds to query.</param>
        /// <param name="userIds">List of UserId to query.</param>
        public void QueryUserScores(List<string> leaderboardIds, List<ProductUserId> userIds, OnLeaderboardCallback QueryUserScoresCompleted)
        {
            if (leaderboardIds.Count == 0 || userIds.Count == 0)
            {
                Debug.LogError("Leaderboard (QueryUserScores): leaderboardIds or userIds is null");

                return;
            }

            List<UserScoresQueryStatInfo> statsInfoList = new List<UserScoresQueryStatInfo>();

            foreach (string leaderboardId in leaderboardIds)
            {
                Definition? definition = GetCachedDefinitionFromId(leaderboardId);

                if (definition != null)
                {
                    UserScoresQueryStatInfo statInfo = new UserScoresQueryStatInfo()
                    {
                        StatName = definition?.StatName,
                        Aggregation = (LeaderboardAggregation)(definition?.Aggregation)
                    };
                    statsInfoList.Add(statInfo);
                }
            }

            // Query User Score

            QueryLeaderboardUserScoresOptions options = new QueryLeaderboardUserScoresOptions()
            {
                UserIds = userIds.ToArray(),
                StatInfo = statsInfoList.ToArray(),
                StartTime = EOS_LEADERBOARDS_TIME_UNDEFINED,
                EndTime = EOS_LEADERBOARDS_TIME_UNDEFINED,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            QueryUserScoresCallback = QueryUserScoresCompleted;

            LeaderboardsHandle.QueryLeaderboardUserScores(ref options, null, LeaderboardUserScoresReceivedCallbackFn);
        }

        /// <summary>Retrieves the leaderboard record for the specified user id.</summary>
        /// <param name="userId">UserId to query.</param>
        /// <param name="record">LeaderboardRecord output from the query.</param>
        /// <returns>Result of the copying the record for the user id.</returns>
        public Result CopyUserScore(ProductUserId userId, out LeaderboardRecord? record)
        {
            record = null;
            var options = new CopyLeaderboardRecordByUserIdOptions
            {
                UserId = userId
            };
            return LeaderboardsHandle.CopyLeaderboardRecordByUserId(ref options, out record);
        }

        private void CacheLeaderboardUserScores()
        {
            CachedLeaderboardUserScores.Clear();

            List<Definition> cachedLeaderboardDefinitions = new List<Definition>();
            cachedLeaderboardDefinitions.AddRange(CachedLeaderboardDefinitions.Values);

            for (int leaderboardIndex = 0; leaderboardIndex < cachedLeaderboardDefinitions.Count; leaderboardIndex++)
            {
                // Leaderboard

                List<LeaderboardUserScore> currentLeaderboardScores = new List<LeaderboardUserScore>();

                Definition leaderboardDefinition = cachedLeaderboardDefinitions[leaderboardIndex];

                if (!string.IsNullOrEmpty(leaderboardDefinition.StatName) && !string.IsNullOrEmpty(leaderboardDefinition.LeaderboardId))
                {
                    GetLeaderboardUserScoreCountOptions options = new GetLeaderboardUserScoreCountOptions()
                    {
                        StatName = leaderboardDefinition.StatName
                    };

                    uint userScoresCount = LeaderboardsHandle.GetLeaderboardUserScoreCount(ref options);

                    CopyLeaderboardUserScoreByIndexOptions userScoreOptions = new CopyLeaderboardUserScoreByIndexOptions()
                    {
                        StatName = leaderboardDefinition.StatName
                    };

                    for (uint userScoreIndex = 0; userScoreIndex < userScoresCount; userScoreIndex++)
                    {
                        // User Scores

                        userScoreOptions.LeaderboardUserScoreIndex = userScoreIndex;

                        Result result = LeaderboardsHandle.CopyLeaderboardUserScoreByIndex(ref userScoreOptions, out LeaderboardUserScore? leaderboardUserScore);

                        if (result != Result.Success)
                        {
                            Debug.LogErrorFormat("Leaderboard (CacheLeaderboardUserScores): CopyLeaderboardUserScoreByIndex {0} failed with result {1}", userScoreIndex, result);
                            break;
                        }

                        currentLeaderboardScores.Add(leaderboardUserScore.Value);
                    }
                }

                CachedLeaderboardUserScores.Add(leaderboardDefinition.LeaderboardId, currentLeaderboardScores);
            }

            CachedLeaderboardUserScoresDirty = true;
        }

        /// <summary>Get cached Leaderboard <c>UserScores</c> given a LeaderboardId.</summary>
        /// <param name="leaderboardId">Leaderboard ID</param>
        /// <returns>Leaderboard <c>UserScores</c></returns>
        public List<LeaderboardUserScore> GetCachedUserScoresFromLeaderboardId(string leaderboardId)
        {
            CachedLeaderboardUserScores.TryGetValue(leaderboardId, out List<LeaderboardUserScore> userScores);

            return userScores;
        }

        private void LeaderboardUserScoresReceivedCallbackFn(ref OnQueryLeaderboardUserScoresCompleteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Leaderboard (LeaderboardUserScoresReceivedCallbackFn): data is null");
            //    QueryUserScoresCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Leaderboard (LeaderboardUserScoresReceivedCallbackFn): Query User Scores error: {0}", data.ResultCode);
                QueryUserScoresCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Leaderboard (LeaderboardUserScoresReceivedCallbackFn): Query User Scores Complete");

            CacheLeaderboardUserScores();

            QueryUserScoresCallback?.Invoke(Result.Success);
        }
    }
}
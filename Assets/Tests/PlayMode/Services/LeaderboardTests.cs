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
 */

namespace PlayEveryWare.EpicOnlineServices.Tests.Services.Leaderboard
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Leaderboards;
    using Epic.OnlineServices.Stats;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Integration tests for leaderboard related calls.
    /// </summary>
    public partial class LeaderboardTests : EOSTestBase
    {
        private LeaderboardsInterface _leaderboardsInterface;
        private StatsInterface statsInterface;

        /// <summary>
        /// Initializes the leaderboard and stats interface for use in the tests.
        /// </summary>
        [SetUp]
        public void SetupLeaderboard()
        {
            _leaderboardsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetLeaderboardsInterface();
            statsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
        }

        /// <summary>
        /// Queries the leaderboard list and makes sure it can complete it.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryLeaderboardList()
        {
            QueryLeaderboardDefinitionsOptions options = new()
            {
                StartTime = DateTimeOffset.MinValue,
                EndTime = DateTimeOffset.MaxValue,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            OnQueryLeaderboardDefinitionsCompleteCallbackInfo? queryResult = null;
            _leaderboardsInterface.QueryLeaderboardDefinitions(ref options, null, (ref OnQueryLeaderboardDefinitionsCompleteCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);
            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
            }

            var countOptions = new GetLeaderboardDefinitionCountOptions();
            uint leaderboardDefinitionsCount = _leaderboardsInterface.GetLeaderboardDefinitionCount(ref countOptions);

            // For now, just verify that the definitions can be returned
            for (uint definitionIndex = 0; definitionIndex < leaderboardDefinitionsCount; definitionIndex++)
            {
                CopyLeaderboardDefinitionByIndexOptions defOptions = new()
                {
                    LeaderboardIndex = definitionIndex
                };

                Result result = _leaderboardsInterface.CopyLeaderboardDefinitionByIndex(ref defOptions, out Definition? leaderboardDefinition);
                Assert.AreEqual(Result.Success, result);
                Assert.IsNotNull(leaderboardDefinition, "Null leaderboard definition received");
                Assert.IsNotEmpty(leaderboardDefinition.Value.LeaderboardId, $"Empty leaderboard id found in leaderboard list at index {definitionIndex}.");
                Assert.IsNotEmpty(leaderboardDefinition.Value.StatName, $"Empty stat name found in the leaderboard list at index {definitionIndex}");
            }
        }

        /// <summary>
        /// Queries an invalid leaderboard id when getting rankings.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryInvalidLeaderboardId()
        {
            QueryLeaderboardRanksOptions rankOptions = new()
            {
                LeaderboardId = "0123456789abc",
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            OnQueryLeaderboardRanksCompleteCallbackInfo? queryResult = null;
            _leaderboardsInterface.QueryLeaderboardRanks(ref rankOptions, null, (ref OnQueryLeaderboardRanksCompleteCallbackInfo data) => { queryResult = data; });

            // This part should fail as the id shouldn't exist
            yield return new WaitUntil(() => queryResult != null);
            Assert.AreNotEqual(Result.Success, queryResult.Value.ResultCode);
        }

        /// <summary>
        /// Queries the ranks of a leaderboard for the first leaderboard item.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryRankings()
        {
            QueryLeaderboardDefinitionsOptions queryDefOptions = new()
            {
                StartTime = DateTimeOffset.MinValue,
                EndTime = DateTimeOffset.MaxValue,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            OnQueryLeaderboardDefinitionsCompleteCallbackInfo? queryResult = null;
            _leaderboardsInterface.QueryLeaderboardDefinitions(ref queryDefOptions, null, (ref OnQueryLeaderboardDefinitionsCompleteCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);
            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
            }

            var countOptions = new GetLeaderboardDefinitionCountOptions();
            uint leaderboardDefinitionsCount = _leaderboardsInterface.GetLeaderboardDefinitionCount(ref countOptions);
            Assert.GreaterOrEqual(leaderboardDefinitionsCount, 1, "Leaderboard should have at least one category.");

            // Get the first leaderboard item and makes sure the query succeeds
            // TODO: Can do more tests when there's something known/static with the leaderboard information
            CopyLeaderboardDefinitionByIndexOptions defOptions = new()
            {
                LeaderboardIndex = 0
            };

            Result result = _leaderboardsInterface.CopyLeaderboardDefinitionByIndex(ref defOptions, out Definition? leaderboardDefinition);
            Assert.AreEqual(Result.Success, result);
            Assert.IsNotNull(leaderboardDefinition);

            QueryLeaderboardRanksOptions rankOptions = new()
            {
                LeaderboardId = leaderboardDefinition.Value.LeaderboardId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            OnQueryLeaderboardRanksCompleteCallbackInfo? queryRankResult = null;
            _leaderboardsInterface.QueryLeaderboardRanks(ref rankOptions, null, (ref OnQueryLeaderboardRanksCompleteCallbackInfo data) => { queryRankResult = data; });

            yield return new WaitUntil(() => queryRankResult != null);
            if (queryRankResult != null)
            {
                Assert.AreEqual(Result.Success, queryRankResult.Value.ResultCode);
            }

            // Loops through the rankings and makes sure it can retrieve the results
            var recordCountOptions = new GetLeaderboardRecordCountOptions();
            uint leaderboardRecordsCount = _leaderboardsInterface.GetLeaderboardRecordCount(ref recordCountOptions);

            for (uint recordIndex = 0; recordIndex < leaderboardRecordsCount; recordIndex++)
            {
                CopyLeaderboardRecordByIndexOptions options = new()
                {
                    LeaderboardRecordIndex = recordIndex
                };

                result = _leaderboardsInterface.CopyLeaderboardRecordByIndex(ref options, out LeaderboardRecord? leaderboardRecord);
                Assert.AreEqual(Result.Success, result);
                Assert.IsNotNull(leaderboardRecord);
                Assert.IsNotNull(leaderboardRecord.Value.UserId, "Ranking has no id.");
                Assert.IsTrue(leaderboardRecord.Value.UserId.IsValid(), "Ranking has an invalid user id.");
                Assert.Greater(leaderboardRecord.Value.Rank, 0, "Ranking should start at 1.");
            }
        }
    }
}

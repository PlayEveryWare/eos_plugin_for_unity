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

namespace PlayEveryWare.EpicOnlineServices.Tests.IntegrationTests
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Achievements;
    using Epic.OnlineServices.Stats;
    using NUnit.Framework;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Integration tests for achievements.
    /// </summary>
    public class EOSAchievementTests : EOSTestBase
    {
        /// <summary>
        /// Checks to see if we can get a count for the number of achievements for the product user.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator GetStatCountForProductUser()
        {
            StatsInterface statInterface = EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
            AchievementsInterface achievementInterface = EOSManager.Instance.GetEOSPlatformInterface().GetAchievementsInterface();

            // Get the stats in order to enable querying the definitions
            var statsOptions = new QueryStatsOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetUserId = EOSManager.Instance.GetProductUserId()
            };

            OnQueryStatsCompleteCallbackInfo? queryResult = null;
            statInterface.QueryStats(ref statsOptions, null, (ref OnQueryStatsCompleteCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);

            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
            }

            OnQueryDefinitionsCompleteCallbackInfo? queryDefResult = null;
            var options = new QueryDefinitionsOptions { LocalUserId = EOSManager.Instance.GetProductUserId() };
            achievementInterface.QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo data) => { queryDefResult = data; });

            yield return new WaitUntil(() => queryDefResult != null);

            if (queryDefResult != null)
            {
                Assert.AreEqual(Result.Success, queryDefResult.Value.ResultCode);
            }

            var getAchievementDefinitionCountOptions = new GetAchievementDefinitionCountOptions();
            uint achievementDefinitionCount = achievementInterface.GetAchievementDefinitionCount(ref getAchievementDefinitionCountOptions);

            Assert.Greater(achievementDefinitionCount, 0, "There should be some achievements for the current product user.");
        }

        /// <summary>
        /// Retrieves the achievement definitions for the product user id and checks some of the definitions to make sure they're valid.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator GetAchievementsDefinitionsForProductUser()
        {
            StatsInterface statInterface = EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
            AchievementsInterface achievementInterface = EOSManager.Instance.GetEOSPlatformInterface().GetAchievementsInterface();

            // Get the stats in order to enable querying the definitions
            var statsOptions = new QueryStatsOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetUserId = EOSManager.Instance.GetProductUserId()
            };

            OnQueryDefinitionsCompleteCallbackInfo? queryDefResult = null;
            statInterface.QueryStats(ref statsOptions, null, (ref OnQueryStatsCompleteCallbackInfo data) =>
            {
                var options = new QueryDefinitionsOptions { LocalUserId = EOSManager.Instance.GetProductUserId() };
                achievementInterface.QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo callbackData) => { queryDefResult = callbackData; }); ;
            });

            yield return new WaitUntilDone(GlobalTestTimeout, () => queryDefResult != null);

            if (queryDefResult != null)
            {
                Assert.AreEqual(Result.Success, queryDefResult.Value.ResultCode);
            }

            var getAchievementDefinitionCountOptions = new GetAchievementDefinitionCountOptions();
            uint achievementDefinitionCount = achievementInterface.GetAchievementDefinitionCount(ref getAchievementDefinitionCountOptions);
            Assert.Greater(achievementDefinitionCount, 0, "There should be some achievements from the product user.");

            var achievementDefOptions = new CopyAchievementDefinitionV2ByIndexOptions();

            for (uint i = 0; i < achievementDefinitionCount; ++i)
            {
                achievementDefOptions.AchievementIndex = i;
                Result achievementResult = achievementInterface.CopyAchievementDefinitionV2ByIndex(ref achievementDefOptions, out DefinitionV2? definition);

                Assert.AreEqual(Result.Success, achievementResult, $"Failed to copy achievement index {i}.");
                Assert.IsNotNull(definition, "Null definition received");
                Assert.IsNotEmpty(definition.Value.LockedDescription, $"Locked description is empty for achievement index {i}.");
                Assert.IsNotEmpty(definition.Value.LockedDisplayName, $"Locked display name is empty for achievement index {i}.");
                Assert.IsNotEmpty(definition.Value.LockedIconURL, $"Locked icon is empty for achievement index {i}.");
                Assert.IsNotEmpty(definition.Value.UnlockedDescription, $"Unlocked description is empty for achievement index {i}.");
                Assert.IsNotEmpty(definition.Value.UnlockedDisplayName, $"Unlocked display name is empty for achievement index {i}.");
                Assert.IsNotEmpty(definition.Value.UnlockedIconURL, $"Unlocked icon is empty for achievement index {i}.");
                Assert.IsNotEmpty(definition.Value.AchievementId, $"Achievement id for index {i} is empty.");

                // Check to see that the same achievement is retrieved if it's retrieved by achievement id or by index
                CopyAchievementDefinitionV2ByAchievementIdOptions achievementDefOptionsById = new()
                {
                    AchievementId = definition.Value.AchievementId
                };
                Result checkedAchievement = achievementInterface.CopyAchievementDefinitionV2ByAchievementId(ref achievementDefOptionsById, out DefinitionV2? checkDefinition);

                Assert.AreEqual(
                    Result.Success, checkedAchievement, 
                    $"Failed to copy achievement by AchievementId {achievementDefOptionsById.AchievementId}");

                if (checkDefinition != null)
                {
                    Assert.AreEqual(
                        checkDefinition.Value,
                        definition.Value,
                        $"The achievement retrieved by AchievementId {achievementDefOptionsById.AchievementId} is not equal to the achievement retrieved by index {achievementDefOptions.AchievementIndex}");
                }
            }
        }
    }
}

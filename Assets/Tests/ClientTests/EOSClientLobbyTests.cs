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

namespace PlayEveryWare.EpicOnlineServices.Tests.ClientTests
{
    using Tests;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Lobby;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;
    using JsonUtility = EpicOnlineServices.Utility.JsonUtility;

    /// <summary>
    /// Lobby connection tests that test connecting to an existing lobby.
    /// </summary>
    public class EOSClientLobbyTests : EOSTestBase
    {
        private string lobbyId;
        private NotifyEventHandle lobbyInviteNotification;

        [SetUp]
        public void Initialize()
        {
            lobbyId = null;
            lobbyInviteNotification = null;
        }

        /// <summary>
        /// Leaves the lobby once the test case ends.
        /// </summary>
        [UnityTearDown]
        public IEnumerator CleanupLobby()
        {
            if (!string.IsNullOrWhiteSpace(lobbyId))
            {
                // Leave the lobby room
                LeaveLobbyOptions options = new()
                {
                    LobbyId = lobbyId,
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                };

                LeaveLobbyCallbackInfo? leaveLobbyResult = null;
                EOSManager.Instance.GetEOSLobbyInterface().LeaveLobby(ref options, null, (ref LeaveLobbyCallbackInfo data) => { leaveLobbyResult = data; });

                yield return new WaitUntilDone(GlobalTestTimeout, () => leaveLobbyResult != null);

                Assert.IsNotNull(leaveLobbyResult);
                Assert.That(leaveLobbyResult.Value.ResultCode == Result.Success, $"Leave Lobby did not succeed: error code {leaveLobbyResult.Value.ResultCode}");
            }

            lobbyInviteNotification?.Dispose();
        }

        /// <summary>
        /// Search by the bucket id for the server preset and join it.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.ClientCategory)]
        public IEnumerator FindByBucketIdAndJoin()
        {
            // Find the bucket id that we recently created
            var searchOptions = new CreateLobbySearchOptions() { MaxResults = 10 };
            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref searchOptions, out LobbySearch outLobbySearchHandle);
            Assert.AreEqual(Result.Success, result, $"Could not create lobby search. Error code: {result}");

            LobbySearchSetParameterOptions paramOptions = new()
            {
                ComparisonOp = ComparisonOp.Equal
            };

            // Turn SearchString into AttributeData
            AttributeData attrData = new()
            {
                Key = TestCommon.SearchBucketIdKey,
                Value = new AttributeDataValue { AsUtf8 = TestCommon.LobbyBucketId }
            };
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to update search with the bucket id. Error code: {result}");

            LobbySearchFindCallbackInfo? searchLobbyResult = null;
            var findOptions = new LobbySearchFindOptions() { LocalUserId = EOSManager.Instance.GetProductUserId() };
            outLobbySearchHandle.Find(ref findOptions,
                null,
                (ref LobbySearchFindCallbackInfo data) => { searchLobbyResult = data; });

            yield return new WaitUntil(() => searchLobbyResult != null);

            if (searchLobbyResult != null)
            {
                Assert.AreEqual(Result.Success, searchLobbyResult.Value.ResultCode,
                    $"Search lobby failed. Error code: {searchLobbyResult.Value.ResultCode}");
            }

            // With the search results, verify that there's only one lobby and it matches with the one created before
            var countOptions = new LobbySearchGetSearchResultCountOptions();
            uint searchResultCount = outLobbySearchHandle.GetSearchResultCount(ref countOptions);
            Assert.AreEqual(1, searchResultCount, $"There should be only one result, got {searchResultCount} instead.");

            LobbySearchCopySearchResultByIndexOptions indexOptions = new() { LobbyIndex = 0 };
            result = outLobbySearchHandle.CopySearchResultByIndex(ref indexOptions, out LobbyDetails outLobbyDetailsHandle);
            Assert.AreEqual(Result.Success, result, "Could not copy search results from index 0.");

            // Now that we have the lobby we're looking for, join it
            JoinLobbyOptions joinOptions = new()
            {
                LobbyDetailsHandle = outLobbyDetailsHandle,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                PresenceEnabled = false
            };

            JoinLobbyCallbackInfo? joinResult = null;
            EOSManager.Instance.GetEOSLobbyInterface().JoinLobby(ref joinOptions, null, (ref JoinLobbyCallbackInfo data) => { joinResult = data; });

            yield return new WaitUntil(() => joinResult != null);
            if (joinResult == null)
            {
                yield break;
            }

            Assert.AreEqual(Result.Success, joinResult.Value.ResultCode,
                $"Could not join the server lobby. Error code: {joinResult.Value.ResultCode}");

            lobbyId = joinResult.Value.LobbyId;
        }

        /// <summary>
        /// Search by the bucket id for the private server and shouldn't be able to find it.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.ClientCategory)]
        public IEnumerator TryToFindPrivateLobby()
        {
            // Find the bucket id that we recently created
            var searchOptions = new CreateLobbySearchOptions() { MaxResults = 10 };
            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref searchOptions, out LobbySearch outLobbySearchHandle);
            Assert.AreEqual(Result.Success, result, $"Could not create lobby search. Error code: {result}");

            LobbySearchSetParameterOptions paramOptions = new()
            {
                ComparisonOp = ComparisonOp.Equal
            };

            // Turn SearchString into AttributeData
            AttributeData attrData = new()
            {
                Key = TestCommon.SearchBucketIdKey,
                Value = new AttributeDataValue { AsUtf8 = TestCommon.LobbyPrivateBucketId }
            };
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to update search with the bucket id. Error code: {result}");

            LobbySearchFindCallbackInfo? searchLobbyResult = null;
            var findOptions = new LobbySearchFindOptions() { LocalUserId = EOSManager.Instance.GetProductUserId() };
            outLobbySearchHandle.Find(ref findOptions,
                null,
                (ref LobbySearchFindCallbackInfo data) => { searchLobbyResult = data; });

            yield return new WaitUntil(() => searchLobbyResult != null);

            if (searchLobbyResult != null)
            {
                Assert.AreEqual(Result.Success, searchLobbyResult.Value.ResultCode,
                    $"Search lobby failed. Error code: {searchLobbyResult.Value.ResultCode}");
            }

            // With the search results, verify there are no results.
            var countOptions = new LobbySearchGetSearchResultCountOptions();
            uint searchResultCount = outLobbySearchHandle.GetSearchResultCount(ref countOptions);
            Assert.AreEqual(0, searchResultCount, $"There should not be any result, got {searchResultCount} instead.");
        }
    }
}

﻿/*
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

namespace PlayEveryWare.EpicOnlineServices.Tests.Services.Sessions
{
    using Tests;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Sessions;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Session connection tests that test connecting to an existing session.
    /// </summary>
    public class ClientSessionTests : EOSTestBase
    {
        private const ulong InvalidNotificationId = 0;

        private SessionsInterface _sessionInterface;
        public ulong SessionInviteNotificationHandle = InvalidNotificationId;
        private string _sessionName;

        [SetUp]
        public void SessionStartup()
        {
            _sessionName = null;
            SessionInviteNotificationHandle = InvalidNotificationId;

            _sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Assert.IsNotNull(_sessionInterface);
        }

        /// <summary>
        /// Leaves the session once the test case ends.
        /// </summary>
        [UnityTearDown]
        public IEnumerator CleanupSession()
        {
            if (!string.IsNullOrWhiteSpace(_sessionName))
            {
                DestroySessionOptions destroyOptions = new()
                {
                    SessionName = _sessionName
                };

                DestroySessionCallbackInfo? result = null;
                _sessionInterface.DestroySession(ref destroyOptions, _sessionName, (ref DestroySessionCallbackInfo data) => { result = data; });

                yield return new WaitUntil(() => result != null);
            }

            if (SessionInviteNotificationHandle != InvalidNotificationId)
            {
                _sessionInterface.RemoveNotifySessionInviteReceived(SessionInviteNotificationHandle);
                SessionInviteNotificationHandle = InvalidNotificationId;
            }
        }
        private SessionSearch CreateSessionSearch(uint maxSearchResults)
        {
            CreateSessionSearchOptions searchOptions = new() { MaxSearchResults = maxSearchResults };
            Result result = _sessionInterface.CreateSessionSearch(ref searchOptions, out SessionSearch sessionSearchHandle);
            Assert.AreEqual(Result.Success, result, "Could not create a session search.");
            return sessionSearchHandle;
        }

        private static void SetSearchParameter(SessionSearch sessionSearchHandle, string key, string value)
        {
            AttributeData attrData = new()
            {
                Key = key,
                Value = new AttributeDataValue() { AsUtf8 = value }
            };

            SessionSearchSetParameterOptions paramOptions = new()
            {
                ComparisonOp = ComparisonOp.Equal,
                Parameter = attrData
            };
            Result result = sessionSearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, "Could not set the search parameter for bucket id.");
        }

        private static IEnumerator FindSession(SessionSearch sessionSearchHandle, Action<SessionSearchFindCallbackInfo?> callback)
        {
            SessionSearchFindOptions findOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            SessionSearchFindCallbackInfo? searchResult = null;
            sessionSearchHandle.Find(ref findOptions, null, (ref SessionSearchFindCallbackInfo data) => { searchResult = data; });

            yield return new WaitUntil(() => searchResult != null);
            callback(searchResult);
        }
        /// <summary>
        /// Search by the bucket id for the server preset and join it.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.ClientCategory)]
        public IEnumerator FindByBucketIdAndJoin()
        {
            SessionSearch sessionSearchHandle = CreateSessionSearch(10);
            SetSearchParameter(sessionSearchHandle, TestCommon.SearchBucketIdKey, TestCommon.SessionBucketId);

            SessionSearchFindCallbackInfo? searchResult = null;
            yield return FindSession(sessionSearchHandle, result => searchResult = result);

            Assert.IsNotNull(searchResult);
            Assert.AreEqual(Result.Success, searchResult.Value.ResultCode, "Failed to search for the session.");

            var countOptions = new SessionSearchGetSearchResultCountOptions();
            uint numSearchResult = sessionSearchHandle.GetSearchResultCount(ref countOptions);
            Assert.AreEqual(1, numSearchResult, "Should find one result for the session search.");

            // Grab information about the session and verify it's the one created
            SessionSearchCopySearchResultByIndexOptions indexOptions = new() { SessionIndex = 0 };
            Result result = sessionSearchHandle.CopySearchResultByIndex(ref indexOptions, out SessionDetails sessionHandle);
            Assert.AreEqual(Result.Success, result, "Could not copy search results.");
            Assert.IsNotNull(sessionHandle, "Session details shouldn't be null.");

            var infoOptions = new SessionDetailsCopyInfoOptions();
            result = sessionHandle.CopyInfo(ref infoOptions, out SessionDetailsInfo? sessionInfo);
            Assert.AreEqual(Result.Success, result, "Could not copy info from the session.");
            Assert.IsNotNull(sessionInfo, "Info from the session is null.");
            if (sessionInfo.Value.Settings != null)
            {
                Assert.AreEqual(TestCommon.SessionBucketId, sessionInfo.Value.Settings.Value.BucketId.ToString(),
                    "Bucket id from result doesn't match.");
            }

            // Join the session on the server
            JoinSessionOptions joinOptions = new()
            {
                SessionHandle = sessionHandle,
                SessionName = "Session#1",
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                PresenceEnabled = false
            };

            JoinSessionCallbackInfo? joinResult = null;
            _sessionInterface.JoinSession(ref joinOptions, null, (ref JoinSessionCallbackInfo data) => { joinResult = data; });

            yield return new WaitUntil(() => joinResult != null);
            if (joinResult != null)
            {
                Assert.AreEqual(Result.Success, joinResult.Value.ResultCode, "Failed to join the server session.");
            }

            _sessionName = TestCommon.SessionName;
        }

        /// <summary>
        /// Search by the bucket id for the private server.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.ClientCategory)]
        public IEnumerator TryToFindPrivateBucketId()
        {
            SessionSearch sessionSearchHandle = CreateSessionSearch(10);
            SetSearchParameter(sessionSearchHandle, TestCommon.SearchBucketIdKey, TestCommon.SessionPrivateBucketId);

            SessionSearchFindCallbackInfo? searchResult = null;
            yield return FindSession(sessionSearchHandle, result => searchResult = result);

            Assert.IsNotNull(searchResult);
            Assert.AreEqual(Result.Success, searchResult.Value.ResultCode, "Failed to search for the session.");

            var countOptions = new SessionSearchGetSearchResultCountOptions();
            uint numSearchResult = sessionSearchHandle.GetSearchResultCount(ref countOptions);
            Assert.AreEqual(0, numSearchResult, "Should not find any sessions in the result.");
        }
    }
}

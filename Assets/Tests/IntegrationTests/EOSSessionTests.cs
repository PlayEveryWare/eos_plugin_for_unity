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
    using Epic.OnlineServices.Sessions;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Integration tests for sessions and matchmaking.
    /// </summary>
    public class EOSSessionTests : EOSTestBase
    {
        private const string SessionName = "IntegrationTestSession";
        private const string LevelName = "CASTLE";

        private SessionModification _sessionModificationHandle;
        private SessionsInterface _sessionInterface;

        /// <summary>
        /// Class of parameters for easily testing multiple cases in a single function.
        /// </summary>
        public class SessionTestParameters
        {
            public string BucketId { get; set; } = string.Empty;
            public uint MaxPlayers { get; set; } = 2;
            public bool AllowJoinInProgress { get; set; } = false;
            public bool PresenceEnabled { get; set; } = false;
            public bool InvitesAllowed { get; set; } = true;
            public OnlineSessionPermissionLevel PermissionLevel { get; set; } = OnlineSessionPermissionLevel.PublicAdvertised;
        }

        // Provides different test cases by changing one of the parameters in each test case.
        public static SessionTestParameters[] sessionParameters = {
            new() { BucketId = "IntegrationTests:Defaults" },
            new() { BucketId = "IntegrationTests:4Players", MaxPlayers = 4 },
            new() { BucketId = "IntegrationTests:AllowJoinInProgress", AllowJoinInProgress = true },
            new() { BucketId = "IntegrationTests:Presence", PresenceEnabled = true },
            new() { BucketId = "IntegrationTests:InvitesDenied", InvitesAllowed = false },
            new() { BucketId = "IntegrationTests:JoinViaPresence", PermissionLevel = OnlineSessionPermissionLevel.JoinViaPresence },
            new() { BucketId = "IntegrationTests:JoinInviteOnly", PermissionLevel = OnlineSessionPermissionLevel.InviteOnly },
        };

        [SetUp]
        public void SessionStartup()
        {
            _sessionModificationHandle = null;

            _sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Assert.IsNotNull(_sessionInterface);
        }

        /// <summary>
        /// Cleans up an handles for the sessions.
        /// </summary>
        [UnityTearDown]
        public IEnumerator CleanupHandles()
        {
            if (_sessionModificationHandle != null)
            {
                _sessionModificationHandle.Release();
            }

            DestroySessionOptions destroyOptions = new()
            {
                SessionName = SessionName
            };

            DestroySessionCallbackInfo? result = null;
            _sessionInterface.DestroySession(ref destroyOptions, SessionName, (ref DestroySessionCallbackInfo data) => { result = data; });

            yield return new WaitUntil(() => result != null);
        }

        /// <summary>
        /// Starts a new session and updates it with different parameters.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator StartAndUpdateNewSession([ValueSource(nameof(sessionParameters))] SessionTestParameters parameters)
        {
            // Small delay to make sure sessions are cleared before creating a new one
            yield return new WaitForSeconds(1f);

            CreateSession(SessionName, parameters);

            UpdateSessionCallbackInfo? updateResult = null;
            UpdateSessionOptions updateOptions = new()
            {
                SessionModificationHandle = _sessionModificationHandle
            };
            _sessionInterface.UpdateSession(ref updateOptions, null, (ref UpdateSessionCallbackInfo data) => { updateResult = data; });

            yield return new WaitUntil(() => updateResult != null);
            if (updateResult != null)
            {
                Assert.AreEqual(Result.Success, updateResult.Value.ResultCode,
                    $"Failed to update the session for bucket id {parameters.BucketId}.");
            }
        }

        /// <summary>
        /// Starts a new session and makes sure it shows up in the search.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator StartAndSearchSession()
        {
            // Minor delay to hopefully reduce timing issues with a previous session remaining active
            yield return new WaitForSeconds(1f);

            const string bucketId = "IntegrationTests:SearchDefaults";
            CreateSession(SessionName, new SessionTestParameters { BucketId = bucketId });

            // Adding additonal attributes that aren't advertised (if not provided, by default, attributes are not advertised)
            AttributeData additionalAttrData = new()
            {
                Key = "Additional",
                Value = new AttributeDataValue { AsUtf8 = "Additional Test Key" },
            };

            SessionModificationAddAttributeOptions attrOptions = new()
            {
                SessionAttribute = additionalAttrData
            };
            Result result = _sessionModificationHandle.AddAttribute(ref attrOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set session attributes for Additional.");

            additionalAttrData.Key = "Another Key";
            additionalAttrData.Value = new AttributeDataValue { AsUtf8 = "More keys" };
            attrOptions.SessionAttribute = additionalAttrData;
            result = _sessionModificationHandle.AddAttribute(ref attrOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set session attributes for Another Key.");

            UpdateSessionCallbackInfo? updateResult = null;
            UpdateSessionOptions updateOptions = new()
            {
                SessionModificationHandle = _sessionModificationHandle
            };
            _sessionInterface.UpdateSession(ref updateOptions, null, (ref UpdateSessionCallbackInfo data) => { updateResult = data; });

            yield return new WaitUntil(() => updateResult != null);
            if (updateResult != null)
            {
                Assert.AreEqual(Result.Success, updateResult.Value.ResultCode,
                    $"Failed to update the session for bucket id {bucketId}.");
            }

            // Wait for the session to update before attempting to search
            yield return new WaitForSecondsRealtime(5f);

            CreateSessionSearchOptions searchOptions = new()
            {
                MaxSearchResults = 10
            };
            result = _sessionInterface.CreateSessionSearch(ref searchOptions, out SessionSearch sessionSearchHandle);
            Assert.AreEqual(Result.Success, result, "Could not create a session search.");

            // Find the session based on the bucket id
            AttributeData attrData = new()
            {
                Key = TestCommon.SearchBucketIdKey,
                Value = new AttributeDataValue() { AsUtf8 = bucketId }
            };

            SessionSearchSetParameterOptions paramOptions = new()
            {
                ComparisonOp = ComparisonOp.Equal,
                Parameter = attrData
            };
            result = sessionSearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, "Could not set the search parameter for bucket id.");

            SessionSearchFindOptions findOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            SessionSearchFindCallbackInfo? searchResult = null;
            sessionSearchHandle.Find(ref findOptions, null, (ref SessionSearchFindCallbackInfo data) => { searchResult = data; });

            yield return new WaitUntil(() => searchResult != null);
            if (searchResult != null)
            {
                Assert.AreEqual(Result.Success, searchResult.Value.ResultCode, "Failed to search for the session.");
            }

            var countOptions = new SessionSearchGetSearchResultCountOptions();
            uint numSearchResult = sessionSearchHandle.GetSearchResultCount(ref countOptions);
            Assert.AreEqual(1, numSearchResult, $"Should find one result for the session search, got {numSearchResult} instead.");

            // Grab information about the session and verify it's the one created
            SessionSearchCopySearchResultByIndexOptions indexOptions = new()
            {
                SessionIndex = 0
            };

            result = sessionSearchHandle.CopySearchResultByIndex(ref indexOptions, out SessionDetails sessionHandle);
            Assert.AreEqual(Result.Success, result, "Could not copy search results.");
            Assert.IsNotNull(sessionHandle, "Session details shouldn't be null.");

            var infoOptions = new SessionDetailsCopyInfoOptions();
            result = sessionHandle.CopyInfo(ref infoOptions, out SessionDetailsInfo? sessionInfo);
            Assert.AreEqual(Result.Success, result, "Could not copy info from the session.");
            Assert.IsNotNull(sessionInfo, "Info from the session is null.");
            if (sessionInfo.Value.Settings != null)
            {
                Assert.AreEqual(bucketId, sessionInfo.Value.Settings.Value.BucketId.ToString(),
                    "Bucket id from result doesn't match.");
            }

            // The count should only show 2 advertised attributes and ignore the 2 extra attributes that were added.
            var sessionDetailsGetSessionAttributeCountOptions = new SessionDetailsGetSessionAttributeCountOptions();
            uint attributeCount = sessionHandle.GetSessionAttributeCount(ref sessionDetailsGetSessionAttributeCountOptions);
            Assert.AreEqual(2, attributeCount, "There should be 2 attributes (bucket id and level) for the session.");

            for (uint attribIndex = 0; attribIndex < attributeCount; attribIndex++)
            {
                SessionDetailsCopySessionAttributeByIndexOptions attributeOptions = new()
                {
                    AttrIndex = attribIndex
                };

                Result attrResult = sessionHandle.CopySessionAttributeByIndex(ref attributeOptions, out SessionDetailsAttribute? sessionAttribute);
                Assert.AreEqual(Result.Success, attrResult, $"Could not copy session attribute index {attribIndex}");
                Assert.IsNotNull(sessionAttribute);
                Assert.IsNotNull(sessionAttribute?.Data);
                Assert.AreEqual(AttributeType.String, sessionAttribute?.Data.Value.Value.ValueType);
            }
        }

        /// <summary>
        /// Helper method to create a session.
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="parameters"></param>
        private void CreateSession(string sessionName, SessionTestParameters parameters)
        {
            CreateSessionModificationOptions createOptions = new()
            {
                BucketId = parameters.BucketId,
                MaxPlayers = parameters.MaxPlayers,
                SessionName = sessionName,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                PresenceEnabled = parameters.PresenceEnabled,
            };

            Result result = _sessionInterface.CreateSessionModification(ref createOptions, out _sessionModificationHandle);
            Assert.AreEqual(Result.Success, result, $"Failed to create session modification for bucket id {parameters.BucketId}.");

            SessionModificationSetPermissionLevelOptions permissionOptions = new()
            {
                PermissionLevel = parameters.PermissionLevel
            };
            result = _sessionModificationHandle.SetPermissionLevel(ref permissionOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set permission level for bucket id {parameters.BucketId}.");

            SessionModificationSetJoinInProgressAllowedOptions jipOptions = new()
            {
                AllowJoinInProgress = parameters.AllowJoinInProgress
            };
            result = _sessionModificationHandle.SetJoinInProgressAllowed(ref jipOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set join in progress for bucket id {parameters.BucketId}.");

            SessionModificationSetInvitesAllowedOptions iaOptions = new()
            {
                InvitesAllowed = parameters.InvitesAllowed
            };
            result = _sessionModificationHandle.SetInvitesAllowed(ref iaOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set allowing invites for bucket id {parameters.BucketId}.");

            // Set Bucket Id
            AttributeData attrData = new()
            {
                Key = TestCommon.SearchBucketIdKey,
                Value = new AttributeDataValue { AsUtf8 = parameters.BucketId },
            };

            SessionModificationAddAttributeOptions attrOptions = new()
            {
                SessionAttribute = attrData,
                AdvertisementType = SessionAttributeAdvertisementType.Advertise
            };
            result = _sessionModificationHandle.AddAttribute(ref attrOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set session attributes for bucket id {parameters.BucketId}.");

            // Set the level name
            attrData.Key = TestCommon.LevelKey;
            attrData.Value = new AttributeDataValue { AsUtf8 = LevelName };
            attrOptions.SessionAttribute = attrData;
            result = _sessionModificationHandle.AddAttribute(ref attrOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to set session attributes for level id.");
        }
    }
}

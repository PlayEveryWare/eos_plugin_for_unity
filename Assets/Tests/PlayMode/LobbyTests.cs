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

namespace PlayEveryWare.EpicOnlineServices.Tests.Services.Lobby
{
    using Tests;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Lobby;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Lobby related tests.
    /// </summary>
    public class LobbyTests : EOSTestBase
    {
        private CreateLobbyCallbackInfo? createLobbyResult = null;
        private LobbySearchFindCallbackInfo? searchLobbyResult = null;

        /// <summary>
        /// Class of parameters for easily testing multiple cases in a single function.
        /// </summary>
        public class LobbyTestParameters
        {
            public string BucketId { get; set; }
            public uint MaxLobbyMembers { get; set; } = 2;
            public LobbyPermissionLevel PermissionLevel { get; set; } = LobbyPermissionLevel.Publicadvertised;
            public bool PresenceEnabled { get; set; } = true;
            public bool AllowInvites { get; set; } = true;
        }

        // Provides different test cases by changing one of the parameters in each test case.
        public static LobbyTestParameters[] lobbyParameters = {
            new() { BucketId = "LobbyTestDefaults" },
            new() { BucketId = "LobbyTest4Members", MaxLobbyMembers = 4 },
            new() { BucketId = "LobbyTestJoinViaPresence", PermissionLevel = LobbyPermissionLevel.Joinviapresence },
            new() { BucketId = "LobbyTestInviteOnly", PermissionLevel = LobbyPermissionLevel.Inviteonly },
            new() { BucketId = "LobbyTestPresenceFalse", PresenceEnabled = false },
            new() { BucketId = "LobbyTestInviteFalse", AllowInvites = false },
        };

        /// <summary>
        /// Reset the results in the beginning of every run.
        /// </summary>
        [SetUp]
        public void LobbySetup()
        {
            createLobbyResult = null;
            searchLobbyResult = null;
        }

        /// <summary>
        /// Cleans up the lobby once the test case ends.
        /// </summary>
        [UnityTearDown]
        public IEnumerator CleanupLobby()
        {
            // Leave the newly created lobby room
            LeaveLobbyOptions options = new LeaveLobbyOptions
            {
                LobbyId = createLobbyResult.Value.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
            };

            LeaveLobbyCallbackInfo? leaveLobbyResult = null;
            EOSManager.Instance.GetEOSLobbyInterface().LeaveLobby(ref options, null, (ref LeaveLobbyCallbackInfo data) => { leaveLobbyResult = data; });

            yield return new WaitUntilDone(GlobalTestTimeout, () => leaveLobbyResult != null);

            Assert.IsNotNull(leaveLobbyResult);
            Assert.That(leaveLobbyResult.Value.ResultCode == Result.Success, $"Leave Lobby did not succeed: error code {leaveLobbyResult.Value.ResultCode}");
        }

        /// <summary>
        /// Create a new lobby without RTC enabled. This generates 2 x lobbyParameters.Length number of tests with different
        /// combinations of on/off voice and a different test parameter from lobbyParameters.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator CreateNewLobby(
            [ValueSource(nameof(lobbyParameters))] LobbyTestParameters parameters,
            [Values(false, true)] bool enableRtc)
        {
            Debug.Log($"=== Test case: bucketId = {parameters.BucketId}, enableRtc = {enableRtc} ===");

            CreateLobbyOptions createLobbyOptions = GenerateLobbyOptions(parameters, enableRtc);
            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(ref createLobbyOptions, null, OnCreateLobbyCompleted);

            yield return new WaitUntilDone(GlobalTestTimeout, () => createLobbyResult != null);

            Assert.IsNotNull(createLobbyResult);
            Assert.AreEqual(Result.Success, createLobbyResult.Value.ResultCode, $"Create lobby {parameters.BucketId} failed. Error code: {createLobbyResult.Value.ResultCode}");
            Assert.That(!string.IsNullOrEmpty(createLobbyResult.Value.LobbyId), $"No lobby id returned on successful create for bucketId {parameters.BucketId} and enableRtc is {enableRtc}.");
        }

        /// <summary>
        /// Creates a lobby and checks to see if it appears in the search by the lobby id.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator CreateLobbyAndFindByLobbyId()
        {
            const string searchBucketId = "LobbyTestLobbyIdSearch";

            // Create a lobby with defaults
            LobbyTestParameters testParameters = new() { BucketId = searchBucketId };
            CreateLobbyOptions createLobbyOptions = GenerateLobbyOptions(testParameters, false);
            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(ref createLobbyOptions, null, OnCreateLobbyCompleted);

            yield return new WaitUntilDone(GlobalTestTimeout, () => createLobbyResult != null);

            Assert.IsNotNull(createLobbyResult);
            Assert.That(createLobbyResult.Value.ResultCode == Result.Success, $"Create lobby failed. Error code: {createLobbyResult.Value.ResultCode}");
            Assert.That(!string.IsNullOrEmpty(createLobbyResult.Value.LobbyId), $"No lobby id returned on successful create.");

            // Find the lobby id that we recently created
            var searchOptions = new CreateLobbySearchOptions() { MaxResults = 10 };
            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref searchOptions, out LobbySearch outLobbySearchHandle);
            Assert.AreEqual(Result.Success, result, $"Could not create lobby search. Error code: {result}");

            var lobbyIdOptions = new LobbySearchSetLobbyIdOptions { LobbyId = createLobbyResult.Value.LobbyId };
            result = outLobbySearchHandle.SetLobbyId(ref lobbyIdOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to update search with the lobby id. Error code: {result}");

            var findOptions = new LobbySearchFindOptions() { LocalUserId = EOSManager.Instance.GetProductUserId() };
            outLobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

            yield return new WaitUntilDone(GlobalTestTimeout, () => searchLobbyResult != null);

            Assert.IsNotNull(searchLobbyResult);
            Assert.AreEqual(Result.Success, searchLobbyResult.Value.ResultCode, $"Search lobby failed. Error code: {searchLobbyResult.Value.ResultCode}");

            // With the search results, verify that there's only one lobby and it matches with the one created before
            var countOptions = new LobbySearchGetSearchResultCountOptions();
            uint searchResultCount = outLobbySearchHandle.GetSearchResultCount(ref countOptions);
            Assert.AreEqual(1, searchResultCount, $"There should be only one result, got {searchResultCount} instead.");

            LobbySearchCopySearchResultByIndexOptions indexOptions = new() { LobbyIndex = 0 };
            result = outLobbySearchHandle.CopySearchResultByIndex(ref indexOptions, out LobbyDetails outLobbyDetailsHandle);
            Assert.AreEqual(Result.Success, result, "Could not copy search results from index 0.");

            var lobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref lobbyOwnerOptions);
            Assert.AreEqual(EOSManager.Instance.GetProductUserId(), newLobbyOwner, "Lobby owner is different than the current test user.");

            // Check a few of the parameters to make sure things are matching. Shouldn't need to check all of them.
            var copyInfoOptions = new LobbyDetailsCopyInfoOptions();
            result = outLobbyDetailsHandle.CopyInfo(ref copyInfoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            Assert.AreEqual(Result.Success, result, $"Could not copy the lobby details. Error code: {result}");
            Assert.IsNotNull(outLobbyDetailsInfo);
            Assert.AreEqual(createLobbyResult.Value.LobbyId, outLobbyDetailsInfo.Value.LobbyId);
            Assert.AreEqual(testParameters.MaxLobbyMembers, outLobbyDetailsInfo.Value.MaxMembers);
            Assert.IsFalse(outLobbyDetailsInfo.Value.RTCRoomEnabled);
        }

        /// <summary>
        /// Creates a lobby and checks to see if it appears in the search by the bucket id.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator CreateLobbyAndFindByBucketId()
        {
            const string searchBucketId = "LobbyTestBucketIdSearch";

            // Create a lobby with defaults
            LobbyTestParameters testParameters = new() { BucketId = searchBucketId };
            CreateLobbyOptions createLobbyOptions = GenerateLobbyOptions(testParameters, false);
            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(ref createLobbyOptions, null, OnCreateLobbyCompleted);

            yield return new WaitUntilDone(GlobalTestTimeout, () => createLobbyResult != null);

            Assert.IsNotNull(createLobbyResult);
            Assert.AreEqual(Result.Success, createLobbyResult.Value.ResultCode, $"Create lobby failed. Error code: {createLobbyResult.Value.ResultCode}");
            Assert.That(!string.IsNullOrEmpty(createLobbyResult.Value.LobbyId), $"No lobby id returned on successful create.");

            // Delay trying to search for a few seconds while the lobby finishes creating on the EOS side. If you search immediately after
            // creating the lobby, then it won't find anything. Searching directly by lobby id doesn't have this delay though.
            yield return new WaitForSecondsRealtime(5f);

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
                Key = "bucket",
                Value = new AttributeDataValue() { AsUtf8 = searchBucketId }
            };
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to update search with the bucket id. Error code: {result}");

            var findOptions = new LobbySearchFindOptions() { LocalUserId = EOSManager.Instance.GetProductUserId() };
            outLobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

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

            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref ownerOptions);
            Assert.AreEqual(EOSManager.Instance.GetProductUserId(), newLobbyOwner, "Lobby owner is different than the current test user.");

            // Check a few of the parameters to make sure things are matching. Shouldn't need to check all of them.
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            result = outLobbyDetailsHandle.CopyInfo(ref infoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            Assert.AreEqual(Result.Success, result, $"Could not copy the lobby details. Error code: {result}");
            Assert.IsNotNull(outLobbyDetailsInfo);
            Assert.AreEqual(createLobbyResult.Value.LobbyId, outLobbyDetailsInfo.Value.LobbyId);
            Assert.AreEqual(searchBucketId, outLobbyDetailsInfo.Value.BucketId.ToString());
            Assert.AreEqual(testParameters.MaxLobbyMembers, outLobbyDetailsInfo.Value.MaxMembers);
            Assert.IsFalse(outLobbyDetailsInfo.Value.RTCRoomEnabled);
        }

        /// <summary>
        /// Creates a lobby and checks to see if it appears in the search by the custom level name.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator CreateLobbyAndFindByLevel()
        {
            const string searchBucketId = "LobbyTestLevelSearch";
            const string levelName = "TEST_LEVEL";

            // Create a lobby with defaults
            LobbyTestParameters testParameters = new() { BucketId = searchBucketId };
            CreateLobbyOptions createLobbyOptions = GenerateLobbyOptions(testParameters, false);
            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(ref createLobbyOptions, null, OnCreateLobbyCompleted);

            yield return new WaitUntilDone(GlobalTestTimeout, () => createLobbyResult != null);

            Assert.IsNotNull(createLobbyResult);
            Assert.AreEqual(Result.Success, createLobbyResult.Value.ResultCode, $"Create lobby failed. Error code: {createLobbyResult.Value.ResultCode}");
            Assert.That(!string.IsNullOrEmpty(createLobbyResult.Value.LobbyId), $"No lobby id returned on successful create.");

            UpdateLobbyModificationOptions options = new()
            {
                LobbyId = createLobbyResult.Value.LobbyId, LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            // Get LobbyModification object handle
            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(ref options, out LobbyModification outLobbyModificationHandle);
            Assert.AreEqual(Result.Success, result, $"Could not create lobby modification. Error code: {result}");

            LobbyModificationAddAttributeOptions addAttributeOptions = new()
            {
                Attribute = new AttributeData { Key = "LEVEL", Value = new AttributeDataValue { AsUtf8 = levelName } },
                Visibility = LobbyAttributeVisibility.Public
            };
            result = outLobbyModificationHandle.AddAttribute(ref addAttributeOptions);
            Assert.AreEqual(Result.Success, result, $"Could not add attribute. Error code: {result}");

            UpdateLobbyCallbackInfo? updateLobbyResult = null;
            var lobbyOptions = new UpdateLobbyOptions() { LobbyModificationHandle = outLobbyModificationHandle };
            EOSManager.Instance.GetEOSLobbyInterface().UpdateLobby(
                ref lobbyOptions,
                null,
                (ref UpdateLobbyCallbackInfo data) => { updateLobbyResult = data; });

            yield return new WaitUntil(() => updateLobbyResult != null);
            Assert.AreEqual(Result.Success, updateLobbyResult.Value.ResultCode, $"UpdateLobby failed with error code: {updateLobbyResult.Value.ResultCode}");

            // Delay trying to search for a few seconds while the lobby finishes creating on the EOS side. If you search immediately after
            // creating the lobby, then it won't find anything. Searching directly by lobby id doesn't have this delay though.
            yield return new WaitForSecondsRealtime(5f);

            // Find the level that we recently created
            var searchOptions = new CreateLobbySearchOptions() { MaxResults = 10 };
            result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref searchOptions, out LobbySearch outLobbySearchHandle);
            Assert.AreEqual(Result.Success, result, $"Could not create lobby search. Error code: {result}");

            LobbySearchSetParameterOptions paramOptions = new();
            paramOptions.ComparisonOp = ComparisonOp.Equal;

            // Turn SearchString into AttributeData
            AttributeData attrData = new();
            attrData.Key = "LEVEL";
            attrData.Value = new AttributeDataValue() { AsUtf8 = levelName };
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to update search with the bucket id. Error code: {result}");

            var findOptions = new LobbySearchFindOptions() { LocalUserId = EOSManager.Instance.GetProductUserId() };
            outLobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

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

            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref ownerOptions);
            Assert.AreEqual(EOSManager.Instance.GetProductUserId(), newLobbyOwner, "Lobby owner is different than the current test user.");

            // Check a few of the parameters to make sure things are matching. Shouldn't need to check all of them.
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            result = outLobbyDetailsHandle.CopyInfo(ref infoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            Assert.AreEqual(Result.Success, result, $"Could not copy the lobby details. Error code: {result}");
            Assert.IsNotNull(outLobbyDetailsInfo);
            Assert.AreEqual(createLobbyResult.Value.LobbyId, outLobbyDetailsInfo.Value.LobbyId);
            Assert.AreEqual(searchBucketId, outLobbyDetailsInfo.Value.BucketId.ToString());
            Assert.AreEqual(testParameters.MaxLobbyMembers, outLobbyDetailsInfo.Value.MaxMembers);
            Assert.IsFalse(outLobbyDetailsInfo.Value.RTCRoomEnabled);
        }

        /// <summary>
        /// Creates a lobby and modifies settings.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator ModifyLobbySettings()
        {
            const string searchBucketId = "LobbyTestModifying";

            // Create a lobby with defaults
            LobbyTestParameters testParameters = new() { BucketId = searchBucketId };
            CreateLobbyOptions createLobbyOptions = GenerateLobbyOptions(testParameters, false);
            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(ref createLobbyOptions, null, OnCreateLobbyCompleted);

            yield return new WaitUntilDone(GlobalTestTimeout, () => createLobbyResult != null);

            Assert.IsNotNull(createLobbyResult);
            Assert.AreEqual(Result.Success, createLobbyResult.Value.ResultCode, $"Create lobby failed. Error code: {createLobbyResult.Value.ResultCode}");
            Assert.That(!string.IsNullOrEmpty(createLobbyResult.Value.LobbyId), $"No lobby id returned on successful create.");

            UpdateLobbyModificationOptions options = new();
            options.LobbyId = createLobbyResult.Value.LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            // Get LobbyModification object handle
            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(ref options, out LobbyModification outLobbyModificationHandle);
            Assert.AreEqual(Result.Success, result, $"Could not create lobby modification. Error code: {result}");

            // Modify all the different parameters
            LobbyModificationSetBucketIdOptions bucketOptions = new() { BucketId = "LobbyTestNewBucket" };
            result = outLobbyModificationHandle.SetBucketId(ref bucketOptions);
            Assert.AreEqual(Result.Success, result, $"Could not set bucket id. Error code: {result}");

            LobbyModificationSetMaxMembersOptions memberOptions = new() { MaxMembers = 5 };
            result = outLobbyModificationHandle.SetMaxMembers(ref memberOptions);
            Assert.AreEqual(Result.Success, result, $"Could not set max members. Error code: {result}");

            LobbyModificationSetPermissionLevelOptions permissionOptions = new() { PermissionLevel = LobbyPermissionLevel.Inviteonly };
            result = outLobbyModificationHandle.SetPermissionLevel(ref permissionOptions);
            Assert.AreEqual(Result.Success, result, $"Could not set permission level. Error code: {result}");

            LobbyModificationSetInvitesAllowedOptions inviteOptions = new() { InvitesAllowed = false };
            result = outLobbyModificationHandle.SetInvitesAllowed(ref inviteOptions);
            Assert.AreEqual(Result.Success, result, $"Could not set permission level. Error code: {result}");

            LobbyModificationAddAttributeOptions addAttributeOptions = new()
            {
                Attribute = new AttributeData { Key = "test", Value = new AttributeDataValue { AsInt64 = 8 } },
                Visibility = LobbyAttributeVisibility.Public
            };
            result = outLobbyModificationHandle.AddAttribute(ref addAttributeOptions);
            Assert.AreEqual(Result.Success, result, $"Could not add attribute. Error code: {result}");

            UpdateLobbyCallbackInfo? updateLobbyResult = null;
            var lobbyOptions = new UpdateLobbyOptions() { LobbyModificationHandle = outLobbyModificationHandle };
            EOSManager.Instance.GetEOSLobbyInterface().UpdateLobby(
                ref lobbyOptions,
                null,
                (ref UpdateLobbyCallbackInfo data) => { updateLobbyResult = data; });

            yield return new WaitUntil(() => updateLobbyResult != null);
            if (updateLobbyResult != null)
            {
                Assert.AreEqual(Result.Success, updateLobbyResult.Value.ResultCode,
                    $"UpdateLobby failed with error code: {updateLobbyResult.Value.ResultCode}");
            }
        }

        /// <summary>
        /// Helper method to create the <see cref="CreateLobbyOptions"/> for the test.
        /// </summary>
        /// <param name="parameters"><see cref="LobbyTestParameters"/> to use when generating the lobby options.</param>
        /// <param name="enableRtc">True if voice is enabled for the lobby, false otherwise.</param>
        /// <returns>A <see cref="CreateLobbyOptions"/> to use for creating a lobby.</returns>
        private static CreateLobbyOptions GenerateLobbyOptions(LobbyTestParameters parameters, bool enableRtc)
        {
            LocalRTCOptions? rtcOptions = null;
            if (enableRtc)
            {
                // Create the lobby room with voice chat
                rtcOptions = new LocalRTCOptions()
                {
                    Flags = 0, //EOS_RTC_JOINROOMFLAGS_ENABLE_ECHO;
                    UseManualAudioInput = false,
                    UseManualAudioOutput = false,
                    LocalAudioDeviceInputStartsMuted = false
                };
            }

            return new CreateLobbyOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxLobbyMembers = parameters.MaxLobbyMembers,
                PermissionLevel = parameters.PermissionLevel,
                PresenceEnabled = parameters.PresenceEnabled,
                AllowInvites = parameters.AllowInvites,
                BucketId = parameters.BucketId,
                EnableRTCRoom = enableRtc,
                LocalRTCOptions = rtcOptions,
            };
        }

        private void OnCreateLobbyCompleted(ref CreateLobbyCallbackInfo createLobbyCallbackInfo)
        {
            createLobbyResult = createLobbyCallbackInfo;
        }

        private void OnLobbySearchCompleted(ref LobbySearchFindCallbackInfo data)
        {
            searchLobbyResult = data;
        }
    }
}

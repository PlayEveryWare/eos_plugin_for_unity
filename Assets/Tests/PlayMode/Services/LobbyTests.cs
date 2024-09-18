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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Lobby related tests.
    /// </summary>
    public class LobbyTests : LobbyTestBase
    {
        /// <summary>
        /// Class of parameters for easily testing multiple cases in a single 
        /// function.
        /// </summary>
        public class LobbyTestParameters
        {
            public string BucketId { get; set; }
            public uint MaxLobbyMembers { get; set; } = 2;
            public LobbyPermissionLevel PermissionLevel { get; set; } = 
                LobbyPermissionLevel.Publicadvertised;
            public bool PresenceEnabled { get; set; } = true;
            public bool AllowInvites { get; set; } = true;
        }

        // Provides different test cases by changing one of the parameters in
        // each test case.
        private static readonly LobbyTestParameters[] s_lobbyParameters = {
            new() { BucketId = "LobbyTestDefaults" },
            new() { BucketId = "LobbyTest4Members", MaxLobbyMembers = 4 },
            new() { BucketId = "LobbyTestJoinViaPresence", 
                PermissionLevel = LobbyPermissionLevel.Joinviapresence },
            new() { BucketId = "LobbyTestInviteOnly", 
                PermissionLevel = LobbyPermissionLevel.Inviteonly },
            new() { BucketId = "LobbyTestPresenceFalse", 
                PresenceEnabled = false },
            new() { BucketId = "LobbyTestInviteFalse", AllowInvites = false },
        };

        /// <summary>
        /// Reset the results in the beginning of every run.
        /// </summary>
        [SetUp]
        public void LobbySetup()
        {
        }

        [TearDown]
        public void LobbyTests_TearDown()
        {
        }

        /// <summary>
        /// Create a new lobby without RTC enabled. This generates 2 x 
        /// s_lobbyParameters.Length number of tests with different combinations
        /// of on/off voice and a different test parameter from 
        /// s_lobbyParameters.
        /// </summary>
        [Test]
        [Category(TestCategories.SoloCategory)]
        public void CreateNewLobbies(
            [ValueSource(nameof(s_lobbyParameters))] LobbyTestParameters parameters,
            [Values(false, true)] bool enableRtc)
        {
            Utf8String lobbyId = CreateNewLobby(parameters, enableRtc);
            LeaveLobby(lobbyId);
        }

        protected Utf8String CreateNewLobby(
            LobbyTestParameters parameters, 
            bool enableRtc = false)
        {
            Debug.Log($"=== Test case: bucketId = {parameters.BucketId}, " +
                $"enableRtc = {enableRtc} ===");

            CreateLobbyOptions createLobbyOptions = GenerateLobbyOptions(
                parameters, 
                enableRtc
                );

            CreateLobbyCallbackInfo? callbackInfo = null;

            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(
                ref createLobbyOptions, 
                null, 
                (ref CreateLobbyCallbackInfo data) =>
                {
                    Assert.AreEqual(
                        data.ResultCode, 
                        Result.Success, 
                        $"Result code for creating new lobby was not " +
                        $"success, it was \"{data.ResultCode}\".");
                    callbackInfo = data;
                });

            // Wait for lobby to be created 
            Task.Run(
                () => new WaitUntilDone(
                    GlobalTestTimeout, 
                    () => callbackInfo != null)
            ).Wait();

            // Delay trying to search for a few seconds while the lobby finishes
            // creating on the EOS side. If you search immediately after
            // creating the lobby, then it won't find anything. Searching
            // directly by lobby id doesn't have this delay though.
            Task.Run(() => new WaitForSecondsRealtime(5f)).Wait();

            if (callbackInfo.HasValue)
            {
                return callbackInfo.Value.LobbyId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a lobby and checks to see if it appears in the search by the
        /// lobby id.
        /// </summary>
        [Test]
        [Category(TestCategories.SoloCategory)]
        public async void CreateLobbyAndFindByLobbyId()
        {
            LobbyTestParameters lobbyParameters = new() { 
                BucketId = "LobbyTestLobbyIdSearch" 
            };

            Utf8String lobbyId = CreateNewLobby(lobbyParameters, false);

            // Find the lobby id that we recently created
            _ = TryCreateLobbySearch(out LobbySearch lobbySearchHandle);

            LobbyDetails lobbyDetails = await FindLobby(lobbyId);

            LobbyDetailsGetLobbyOwnerOptions lobbyOwnerOptions = new();
            ProductUserId newLobbyOwner = lobbyDetails.GetLobbyOwner(
                ref lobbyOwnerOptions
                );

            Assert.AreEqual(
                EOSManager.Instance.GetProductUserId(), 
                newLobbyOwner, 
                "Lobby owner is different than the current test user.");

            // Check a few of the parameters to make sure things are matching.
            // Shouldn't need to check all of them.
            var copyInfoOptions = new LobbyDetailsCopyInfoOptions();

            Result result = lobbyDetails.CopyInfo(
                ref copyInfoOptions, 
                out LobbyDetailsInfo? outLobbyDetailsInfo);

            Assert.AreEqual(
                Result.Success, 
                result, 
                $"Could not copy the lobby details. Error code: {result}");
            Assert.IsNotNull(outLobbyDetailsInfo);
            Assert.AreEqual(lobbyId, outLobbyDetailsInfo.Value.LobbyId);
            Assert.AreEqual(
                lobbyParameters.MaxLobbyMembers, 
                outLobbyDetailsInfo.Value.MaxMembers);
            Assert.IsFalse(outLobbyDetailsInfo.Value.RTCRoomEnabled);

            LeaveLobby(lobbyId);
        }

        /// <summary>
        /// Creates a lobby and checks to see if it appears in the search by the
        /// bucket id.
        /// </summary>
        [Test]
        [Category(TestCategories.SoloCategory)]
        public async void CreateLobbyAndFindByBucketId()
        {
            const string searchBucketId = "LobbyTestBucketIdSearch";

            // Create a lobby with defaults
            LobbyTestParameters testParameters = new() { 
                BucketId = searchBucketId 
            };

            Utf8String lobbyId = CreateNewLobby(testParameters, false);

            IList<LobbyDetails> lobbiesFound = await FindLobbies(
                "bucket", 
                searchBucketId
                );

            // With the search results, verify that there's only one lobby and
            // it matches with the one created before
            Assert.AreEqual(1, lobbiesFound.Count, $"There should be only " +
                $"one result, got {lobbiesFound.Count} instead.");

            LobbyDetailsGetLobbyOwnerOptions ownerOptions = new();
            ProductUserId newLobbyOwner = lobbiesFound[0]?.GetLobbyOwner(ref ownerOptions);
            Assert.AreEqual(EOSManager.Instance.GetProductUserId(), newLobbyOwner, "Lobby owner is different than the current test user.");

            // Check a few of the parameters to make sure things are matching. Shouldn't need to check all of them.
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            Result result = lobbiesFound[0].CopyInfo(ref infoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            Assert.AreEqual(Result.Success, result, $"Could not copy the lobby details. Error code: {result}");
            Assert.IsNotNull(outLobbyDetailsInfo);
            Assert.AreEqual(lobbyId, outLobbyDetailsInfo.Value.LobbyId);
            Assert.AreEqual(searchBucketId, outLobbyDetailsInfo.Value.BucketId.ToString());
            Assert.AreEqual(testParameters.MaxLobbyMembers, outLobbyDetailsInfo.Value.MaxMembers);
            Assert.IsFalse(outLobbyDetailsInfo.Value.RTCRoomEnabled);

            LeaveLobby(lobbyId);
        }

        /// <summary>
        /// Creates a lobby and checks to see if it appears in the search by the custom level name.
        /// </summary>
        [Test]
        [Category(TestCategories.SoloCategory)]
        public async void CreateLobbyAndFindByLevel()
        {
            const string searchBucketId = "LobbyTestLevelSearch";
            const string levelName = "TEST_LEVEL";

            // Create a lobby with defaults
            LobbyTestParameters testParameters = new() { BucketId = searchBucketId };

            Utf8String lobbyId = CreateNewLobby(testParameters);

            UpdateLobbyModificationOptions options = new()
            {
                LobbyId = lobbyId, LocalUserId = EOSManager.Instance.GetProductUserId()
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

            Task.Run(() => new WaitUntil(() => updateLobbyResult != null)).Wait();

            Assert.AreEqual(Result.Success, updateLobbyResult.Value.ResultCode, $"UpdateLobby failed with error code: {updateLobbyResult.Value.ResultCode}");

            IList<LobbyDetails> lobbies = await FindLobbies("LEVEL", levelName);
            
            Assert.AreEqual(1, lobbies.Count, $"There should be only one result, got {lobbies.Count} instead.");

            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = lobbies[0].GetLobbyOwner(ref ownerOptions);
            Assert.AreEqual(EOSManager.Instance.GetProductUserId(), newLobbyOwner, "Lobby owner is different than the current test user.");

            // Check a few of the parameters to make sure things are matching. Shouldn't need to check all of them.
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            result = lobbies[0].CopyInfo(ref infoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            Assert.AreEqual(Result.Success, result, $"Could not copy the lobby details. Error code: {result}");
            Assert.IsNotNull(outLobbyDetailsInfo);
            Assert.AreEqual(lobbyId, outLobbyDetailsInfo.Value.LobbyId);
            Assert.AreEqual(searchBucketId, outLobbyDetailsInfo.Value.BucketId.ToString());
            Assert.AreEqual(testParameters.MaxLobbyMembers, outLobbyDetailsInfo.Value.MaxMembers);
            Assert.IsFalse(outLobbyDetailsInfo.Value.RTCRoomEnabled);

            LeaveLobby(lobbyId);
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
            Utf8String lobbyId = CreateNewLobby(testParameters);

            UpdateLobbyModificationOptions options = new();
            options.LobbyId = lobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            // Get LobbyModification object handle
            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(
                ref options, out LobbyModification outLobbyModificationHandle);
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

            LeaveLobby(lobbyId);
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
    }
}

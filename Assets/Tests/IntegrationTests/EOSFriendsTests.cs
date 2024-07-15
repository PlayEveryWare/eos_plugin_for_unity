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
    using Epic.OnlineServices.Connect;
    using Epic.OnlineServices.Friends;
    using Epic.OnlineServices.Presence;
    using Epic.OnlineServices.Reports;
    using Epic.OnlineServices.UI;
    using Epic.OnlineServices.UserInfo;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Integration tests for friend related calls.
    /// </summary>
    public class EOSFriendsTests : EOSTestBase
    {
        private FriendsInterface _friendsInterface;
        private UserInfoInterface _userInfoInterface;
        private PresenceInterface _presenceInterface;

        /// <summary>
        /// Initializes the interfaces needed to get information related to friends.
        /// </summary>
        [SetUp]
        public void InitializeInterfaces()
        {
            _friendsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetFriendsInterface();
            _userInfoInterface = EOSManager.Instance.GetEOSPlatformInterface().GetUserInfoInterface();
            _presenceInterface = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();
        }

        /// <summary>
        /// Queries the friends list and makes sure it can complete it.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryFriendsList()
        {
            QueryFriendsOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId()
            };

            QueryFriendsCallbackInfo? result = null;
            _friendsInterface.QueryFriends(ref options, null, (ref QueryFriendsCallbackInfo data) => { result = data; });

            yield return new WaitUntil(() => result != null);
            if (result != null)
			{
				Assert.AreEqual(Result.Success, result.Value.ResultCode, "Could not query the friends list.");
			}

            GetFriendsCountOptions countOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId()
            };

            // TODO: Until we have set test accounts, can't determine if the number is correct or not.
            // All we can do is make sure it doesn't throw an exception.
            Assert.DoesNotThrow(() => _friendsInterface.GetFriendsCount(ref countOptions));
        }

        /// <summary>
        /// Searches the friends list with a test display name.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator SearchFriendsListByDisplayName()
        {
            QueryUserInfoByDisplayNameOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                DisplayName = "abdxyz"
            };

            QueryUserInfoByDisplayNameCallbackInfo? queryResult = null;
            _userInfoInterface.QueryUserInfoByDisplayName(ref options, null, (ref QueryUserInfoByDisplayNameCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);
            Assert.AreEqual(Result.NotFound, queryResult.Value.ResultCode);

            // TODO: Will need set test accounts to verify that the results are correct. For now,
            // as long as the function doesn't error out, consider it okay.
        }

        /// <summary>
        /// Simple test to make sure the plugin can get friend updates.
        /// </summary>
        [Test]
        [Category(TestCategories.SoloCategory)]
        public void SubscribeToFriendUpdates()
        {
            AddNotifyFriendsUpdateOptions updateOptions = new();
            ulong notificationId = _friendsInterface.AddNotifyFriendsUpdate(ref updateOptions, null, (ref OnFriendsUpdateInfo x) => { });
            Assert.AreNotEqual(Common.InvalidNotificationid, notificationId, "Could not subscribe to friends update.");

            _friendsInterface.RemoveNotifyFriendsUpdate(notificationId);

            AddNotifyOnPresenceChangedOptions changedOptions = new();
            ulong presenceNotificationId = _presenceInterface.AddNotifyOnPresenceChanged(ref changedOptions, null, (ref PresenceChangedCallbackInfo x) => { });
            Assert.AreNotEqual(Common.InvalidNotificationid, presenceNotificationId, "Could not subscribe to friends presence change.");

            _presenceInterface.RemoveNotifyOnPresenceChanged(presenceNotificationId);
        }

        /// <summary>
        /// Queries the local account and get information about it.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QuerySelfInfo()
        {
            QueryUserInfoOptions queryOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = EOSManager.Instance.GetLocalUserId()
            };

            QueryUserInfoCallbackInfo? queryResult = null;
            _userInfoInterface.QueryUserInfo(ref queryOptions, null, (ref QueryUserInfoCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);
            if (queryResult != null)
			{
				Assert.AreEqual(Result.Success, queryResult.Value.ResultCode, "Could not query for self.");
			}

            // Copy the user info and verify it's valid
            CopyUserInfoOptions copyOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = EOSManager.Instance.GetLocalUserId()
            };

            Result copyResult = _userInfoInterface.CopyUserInfo(ref copyOptions, out UserInfoData? userInfo);
            Assert.AreEqual(Result.Success, copyResult, "Could not copy info for self.");
            Assert.IsNotNull(userInfo, "Got a null UserInfoData");
            Assert.IsNotNull(userInfo.Value.UserId, "The user id should not be null.");
            Assert.IsNotEmpty(userInfo.Value.DisplayName, "The display name should not be empty.");
        }

        /// <summary>
        /// Sends a report to the current test user. This should fail since you can't report yourself.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator SendReportToSelf()
        {
            SendPlayerBehaviorReportOptions reportOptions = new()
            {
                ReporterUserId = EOSManager.Instance.GetProductUserId(),
                ReportedUserId = EOSManager.Instance.GetProductUserId(),
                Category = PlayerReportsCategory.Cheating,
                Message = "Automated test report"
            };

            // There's an expected error log for this since you can't report yourself
            LogAssert.Expect(LogType.Error, new Regex("Failed to send a player behavior report"));

            ReportsInterface reportsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetReportsInterface();
            SendPlayerBehaviorReportCompleteCallbackInfo? reportResult = null;
            reportsHandle.SendPlayerBehaviorReport(ref reportOptions, null, (ref SendPlayerBehaviorReportCompleteCallbackInfo data) => { reportResult = data; });

            yield return new WaitUntil(() => reportResult != null);

            if (reportResult != null)
            {
                Assert.AreEqual(Result.InvalidRequest, reportResult.Value.ResultCode,
                    $"Did not get InvalidRequest when reporting user id {EOSManager.Instance.GetProductUserId()}.");
            }
        }

        /// <summary>
        /// Sends a report to the first friend of the test user. This assumes the user has at least one friend.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator SendReportToFirstFriend()
        {
            QueryFriendsOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId()
            };

            QueryFriendsCallbackInfo? result = null;
            _friendsInterface.QueryFriends(ref options, null, (ref QueryFriendsCallbackInfo data) => { result = data; });

            yield return new WaitUntil(() => result != null);
            if (result != null)
			{
				Assert.AreEqual(Result.Success, result.Value.ResultCode, "Could not query the friends list.");
			}

            // Get the first friend from the list
            GetFriendAtIndexOptions friendOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                Index = 0,
            };

            EpicAccountId friendAccount = _friendsInterface.GetFriendAtIndex(ref friendOptions);
            Result stringResult = friendAccount.ToString(out Utf8String friendAccountString);
            Assert.AreEqual(Result.Success, stringResult, "Could not convert friendAccount into a string version.");

            // Get the product user id of the friend account
            ConnectInterface connectHandle = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();

            Utf8String[] externalAccountIds = { friendAccountString };
            QueryExternalAccountMappingsOptions queryOptions = new()
            {
                AccountIdType = ExternalAccountType.Epic,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                ExternalAccountIds = externalAccountIds
            };

            QueryExternalAccountMappingsCallbackInfo? externalData = null;
            connectHandle.QueryExternalAccountMappings(ref queryOptions, null, (ref QueryExternalAccountMappingsCallbackInfo data) => { externalData = data; });

            yield return new WaitUntil(() => externalData != null);
            if (externalData != null)
            {
                Assert.AreEqual(Result.Success, externalData.Value.ResultCode,
                    "Failed to query external account mapping");
            }

            GetExternalAccountMappingsOptions accountOptions = new()
            {
                AccountIdType = ExternalAccountType.Epic,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetExternalUserId = friendAccountString
            };

            ProductUserId newProductUserId = connectHandle.GetExternalAccountMapping(ref accountOptions);
            Assert.IsNotNull(newProductUserId, $"Could not retrieve the product user id of friend local user id {friendAccount}");

            // Send the report against the first friend
            SendPlayerBehaviorReportOptions reportOptions = new()
            {
                ReporterUserId = EOSManager.Instance.GetProductUserId(),
                ReportedUserId = newProductUserId,
                Category = PlayerReportsCategory.Cheating,
                Message = "Automated test report"
            };

            ReportsInterface reportsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetReportsInterface();
            SendPlayerBehaviorReportCompleteCallbackInfo? reportResult = null;
            reportsHandle.SendPlayerBehaviorReport(ref reportOptions, null, (ref SendPlayerBehaviorReportCompleteCallbackInfo data) => { reportResult = data; });

            yield return new WaitUntil(() => reportResult != null);

            if (reportResult != null)
            {
                Assert.AreEqual(Result.Success, reportResult.Value.ResultCode,
                    $"Error when reporting first friend with user id {EOSManager.Instance.GetProductUserId()}.");
            }
        }

        /// <summary>
        /// Tests to make sure the friends overlay doesn't throw any errors.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator ShowHideFriendsOverlay()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR
            Assert.Ignore("Skipping test when run in Unity Editor.");
#endif
            ShowFriendsCallbackInfo? overlayResult = null;
            ShowFriendsOptions showOptions = new() { LocalUserId = EOSManager.Instance.GetLocalUserId() };
            EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().ShowFriends(
                ref showOptions,
                null,
                (ref ShowFriendsCallbackInfo data) => { overlayResult = data; });

            yield return new WaitUntil(() => overlayResult != null);
            if (overlayResult != null)
			{
				Assert.AreEqual(Result.Success, overlayResult.Value.ResultCode, "Could not notify overlay to show friends");
			}

            // Wait a bit before closing the overlay for any UI delays
            yield return new WaitForSecondsRealtime(2f);

            HideFriendsCallbackInfo? hideResult = null;
            var hideOptions = new HideFriendsOptions() { LocalUserId = EOSManager.Instance.GetLocalUserId() };
            EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().HideFriends(
                ref hideOptions,
                null,
                (ref HideFriendsCallbackInfo data) => { hideResult = data; });

            yield return new WaitUntil(() => hideResult != null);
            if (hideResult != null)
			{
				Assert.AreEqual(Result.Success, hideResult.Value.ResultCode, "Could not notify overlay to hide friends");
			}
        }
    }
}

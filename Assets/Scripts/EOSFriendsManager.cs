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
    using UnityEngine;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using Epic.OnlineServices.Friends;
    using Epic.OnlineServices.Presence;
    using Epic.OnlineServices.UserInfo;
    using System.Collections.Generic;
    using Epic.OnlineServices.UI;

    /// <summary>Class <c>FriendData</c> stores Friend data.</summary>
    public class FriendData
    {
        public EpicAccountId LocalUserId;
        public EpicAccountId UserId;
        public ProductUserId UserProductUserId;
        public string Name;
        public FriendsStatus Status = FriendsStatus.NotFriends;
        public PresenceInfo Presence;

        public bool IsFriend()
        {
            return Status == FriendsStatus.Friends;
        }

        public bool IsOnline()
        {
            return Presence?.Status == Epic.OnlineServices.Presence.Status.Online;
        }
    }

    /// <summary>Class <c>PresenceInfo</c> stores Player Presence data.</summary>
    public class PresenceInfo
    {
        //Status
        public Status Status = Status.Offline;
        public string RichText;
        public string Application;
        public string Platform;
        public string JoinInfo;
    }

    /// <summary>
    /// Class <c>EOSFriendsManager</c> is a simplified wrapper for EOS [FriendsInterface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Friends/index.html).
    /// </summary>
    public class EOSFriendsManager : IEOSSubManager
    {
        private Dictionary<EpicAccountId, FriendData> CachedFriends;
        private bool CachedFriendsDirty;

        private Dictionary<EpicAccountId, FriendData> CachedSearchResults;
        private bool CachedSearchResultsDirty;

        // Manager Callbacks
        private OnFriendsCallback AddFriendCallback;
        private OnFriendsCallback QueryFriendCallback;
        private OnFriendsCallback AcceptInviteCallback;
        private OnFriendsCallback RejectInviteCallback;
        private OnFriendsCallback ShowFriendsOverlayCallback;
        private OnFriendsCallback HideFriendsOverlayCallback;

        private OnFriendsCallback QueryUserInfoCallback;

        public delegate void OnFriendsCallback(Result result);

        private FriendsInterface FriendsHandle;
        private UserInfoInterface UserInfoHandle;
        private PresenceInterface PresenceHandle;
        private ConnectInterface ConnectHandle;

        private Dictionary<EpicAccountId, ulong> FriendNotifications = new Dictionary<EpicAccountId, ulong>();
        private Dictionary<EpicAccountId, ulong> PresenceNotifications = new Dictionary<EpicAccountId, ulong>();

        public EOSFriendsManager()
        {
            CachedFriends = new Dictionary<EpicAccountId, FriendData>();
            CachedFriendsDirty = true;

            CachedSearchResults = new Dictionary<EpicAccountId, FriendData>();
            CachedSearchResultsDirty = true;

            FriendsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetFriendsInterface();
            UserInfoHandle = EOSManager.Instance.GetEOSPlatformInterface().GetUserInfoInterface();
            PresenceHandle = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();
            ConnectHandle = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
        }

        /// <summary>Returns cached Friends list.</summary>
        /// <returns>True if cache has changed since last call.</returns>
        public bool GetCachedFriends(out Dictionary<EpicAccountId, FriendData> Friends)
        {
            Friends = CachedFriends;

            bool returnDirty = CachedFriendsDirty;

            CachedFriendsDirty = false;

            return returnDirty;
        }

        public EpicAccountId GetAccountMapping(ProductUserId targetUserId)
        {
            foreach(FriendData friendData in CachedFriends.Values)
            {
                if(targetUserId == friendData.UserProductUserId)
                {
                    return friendData.UserId;
                }
            }

            return new EpicAccountId();
        }

        public string GetDisplayName(EpicAccountId targetAccountId)
        {
            if(CachedFriends.TryGetValue( targetAccountId, out FriendData friend ))
            {
                return friend.Name;
            }

            return string.Empty;
        }

        /// <summary>Returns cached Search Results.</summary>
        /// <returns>True if cache has changed since last call.</returns>
        public bool GetCachedSearchResults(out Dictionary<EpicAccountId, FriendData> SearchResults)
        {
            SearchResults = CachedSearchResults;

            bool returnDirty = CachedSearchResultsDirty;

            CachedSearchResultsDirty = false;

            return returnDirty;
        }

        public void ClearCachedSearchResults()
        {
            CachedSearchResults.Clear();
            CachedSearchResultsDirty = true;
        }

        [Obsolete("SendInvite is obsolete.  ErrorCode=NotImplemented")]
        public void SendInvite(EpicAccountId friendUserId, OnFriendsCallback AddFriendCompleted)
        {
            if(friendUserId.IsValid())
            {
                Debug.LogError("Friends (AddFriend): friendUserId parameter is invalid!");
                AddFriendCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            SendInviteOptions options = new SendInviteOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = friendUserId
            };

            AddFriendCallback = AddFriendCompleted;

            FriendsHandle.SendInvite(ref options, null, OnSendInviteCompleted);
        }

        [Obsolete("OnSendInviteCompleted is obsolete.  ErrorCode=NotImplemented")]
        private void OnSendInviteCompleted(ref SendInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (SendInviteCallback): data is null");
            //    AddFriendCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (SendInviteCallback): SendInvite error: {0}", data.ResultCode);
                AddFriendCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Friends (SendInviteCallback): SendInvite Complete for user id: {0}", data.LocalUserId);
            AddFriendCallback?.Invoke(Result.Success);
        }

        public void AcceptInvite(EpicAccountId friendUserId, OnFriendsCallback AcceptInviteCompleted)
        {
            if (friendUserId.IsValid())
            {
                Debug.LogError("Friends (AcceptInvite): friendUserId parameter is invalid!");
                AcceptInviteCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            AcceptInviteOptions options = new AcceptInviteOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = friendUserId
            };

            AcceptInviteCallback = AcceptInviteCompleted;

            FriendsHandle.AcceptInvite(ref options, null, OnAcceptInviteCompleted);
        }

        private void OnAcceptInviteCompleted(ref AcceptInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnAcceptInviteCompleted): data is null");
            //    AcceptInviteCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnAcceptInviteCompleted): AcceptInvite error: {0}", data.ResultCode);
                AcceptInviteCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Friends (OnAcceptInviteCompleted): Accept Invite Complete for user id: {0}", data.LocalUserId);
            AcceptInviteCallback?.Invoke(Result.Success);
        }

        public void RejectInvite(EpicAccountId friendUserId, OnFriendsCallback RejectInviteCompleted)
        {
            if (friendUserId.IsValid())
            {
                Debug.LogError("Friends (RejectInvite): friendUserId parameter is invalid!");
                RejectInviteCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            RejectInviteOptions options = new RejectInviteOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = friendUserId
            };

            RejectInviteCallback = RejectInviteCompleted;

            FriendsHandle.RejectInvite(ref options, null, OnRejectInviteCompleted);
        }

        private void OnRejectInviteCompleted(ref RejectInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnRejectInviteCompleted): data is null");
            //    RejectInviteCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnRejectInviteCompleted): RejectInvite error: {0}", data.ResultCode);
                RejectInviteCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Friends (OnRejectInviteCompleted): Reject Invite Complete for user id: {0}", data.LocalUserId);
            RejectInviteCallback?.Invoke(Result.Success);
        }

        /// <summary>(async) Query for friends.</summary>
        public void QueryFriends(OnFriendsCallback QueryFriendsCompleted)
        {
            QueryFriends(EOSManager.Instance.GetLocalUserId(), QueryFriendsCompleted);
        }

        public void QueryFriends(EpicAccountId userId, OnFriendsCallback QueryFriendsCompleted)
        {
            QueryFriendsOptions options = new QueryFriendsOptions()
            {
                LocalUserId = userId
            };

            QueryFriendCallback = QueryFriendsCompleted;

            FriendsHandle.QueryFriends(ref options, null, QueryFriendsCallback);
        }

        private void QueryFriendsCallback(ref QueryFriendsCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (QueryFriendsCallback): data is null");
            //    QueryFriendCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (QueryFriendsCallback): QueryFriends error: {0}", data.ResultCode);
                QueryFriendCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Friends (QueryFriendsCallback): Query Friends Complete for user id: {0}", data.LocalUserId);

            GetFriendsCountOptions countOptions = new GetFriendsCountOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId()
            };

            int friendsCount = FriendsHandle.GetFriendsCount(ref countOptions);

            Debug.LogFormat("Friends (QueryFriendsCallback): Number of friends: {0}", friendsCount);

            GetFriendAtIndexOptions options = new GetFriendAtIndexOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId()
            };

            Dictionary<EpicAccountId, FriendData> friendsList = new Dictionary<EpicAccountId, FriendData>();

            for (int friendIndex = 0; friendIndex < friendsCount; friendIndex++)
            {
                options.Index = friendIndex;

                EpicAccountId friendUserId = FriendsHandle.GetFriendAtIndex(ref options);

                if (friendUserId.IsValid())
                {
                    GetStatusOptions statusOptions = new GetStatusOptions()
                    {
                        LocalUserId = EOSManager.Instance.GetLocalUserId(),
                        TargetUserId = friendUserId
                    };

                    FriendsStatus friendStatus = FriendsHandle.GetStatus(ref statusOptions);

                    Debug.LogFormat("Friends (QueryFriendsCallback): Friend Status {0} => {1}", friendUserId, friendStatus);

                    FriendData friendEntry = new FriendData()
                    {
                        LocalUserId = data.LocalUserId,
                        UserId = friendUserId,
                        Name = "Pending...",
                        Status = friendStatus
                    };

                    friendsList.Add(friendEntry.UserId, friendEntry);
                }
                else
                {
                    Debug.LogWarning("Friends (QueryFriendsCallback): Invalid friend found in friends list");
                }
            }

            CachedFriends = friendsList;
            CachedFriendsDirty = true;

            foreach (FriendData friend in CachedFriends.Values)
            {
                QueryPresenceInfo(EOSManager.Instance.GetLocalUserId(), friend.UserId);
                QueryUserInfo(friend.UserId, null);
                QueryFriendsConnectMappings();
            }

            QueryFriendCallback?.Invoke(Result.Success);
        }

        private void QueryPresenceInfo(EpicAccountId localUserId, EpicAccountId targetUserId)
        {
            QueryPresenceOptions options = new QueryPresenceOptions()
            {
                LocalUserId = localUserId,
                TargetUserId = targetUserId
            };

            PresenceHandle.QueryPresence(ref options, null, OnQueryPresenceCompleted);
        }

        private void OnQueryPresenceCompleted(ref QueryPresenceCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnQueryPresenceCompleted): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnQueryPresenceCompleted): Error calling QueryPresence: " + data.ResultCode);
                return;
            }

            Debug.Log("Friends (OnQueryPresenceCompleted): QueryPresence successful");

            CopyPresenceOptions presenceOptions = new CopyPresenceOptions()
            {
                LocalUserId = data.LocalUserId,
                TargetUserId = data.TargetUserId
            };

            Result result = PresenceHandle.CopyPresence(ref presenceOptions, out Info? presence);

            if(result != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnQueryPresenceCompleted): CopyPresence error: {0}", result);
                return;
            }

            GetJoinInfoOptions joinInfoOptions = new GetJoinInfoOptions()
            {
                LocalUserId = data.LocalUserId,
                TargetUserId = data.TargetUserId
            };

            // If the user has a Presence JoinInfo, set that in this incoming Presence.
            PresenceInfo presenceInfo = new PresenceInfo()
            {
                Application = presence?.ProductId,
                Platform = presence?.Platform,
                Status = (Status)(presence?.Status),
                RichText = presence?.RichText,
                JoinInfo = null // Default to null until the value is determined
            };

            Result joinInfoResult = PresenceHandle.GetJoinInfo(ref joinInfoOptions, out Utf8String joinInfo);

            // If the result of getting join info is "Result.NotFound", that implies the user does not have an active joinable activity.
            // Only on a Success can the JoinInfo be used, but otherwise the JoinInfo is to be ignored.
            if (joinInfoResult == Result.Success)
            {
                presenceInfo.JoinInfo = joinInfo;
            }

            if(CachedFriends.TryGetValue(data.TargetUserId, out FriendData friendData))
            {
                friendData.Presence = presenceInfo;
                CachedFriendsDirty = true;

                Debug.LogFormat("Friends (OnQueryPresenceCompleted): PresenceInfo (Status) updated for target user: {0}", data.TargetUserId);
            }
            else
            {
                data.TargetUserId.ToString(out Utf8String targetUserIdString);

                if(string.IsNullOrEmpty(targetUserIdString))

                Debug.LogWarningFormat("Friends (OnQueryPresenceCompleted): PresenceInfo not stored, couldn't find target user in friends cache: {0}, ", data.TargetUserId);
            }
        }

        private void QueryFriendsConnectMappings()
        {
            if(CachedFriends.Count == 0)
            {
                Debug.LogError("Friends (QueryFriendsConnectMappings): No friend cache.");
                return;
            }

            var externalAccountIds = new Utf8String[CachedFriends.Count];

            int i = 0;
            foreach(EpicAccountId account in CachedFriends.Keys)
            {
                Result result = account.ToString(out Utf8String accountAsString);

                if(result != Result.Success)
                {
                    Debug.LogErrorFormat("Friends (QueryFriendsConnectMappings): Couldn't convert EpicAccountId to string: {0}", result);
                    return;
                }

                externalAccountIds[i] = accountAsString;
                i++;
            }

            QueryExternalAccountMappingsOptions options = new QueryExternalAccountMappingsOptions()
            {
                AccountIdType = ExternalAccountType.Epic,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                ExternalAccountIds = externalAccountIds
            };

            ConnectHandle.QueryExternalAccountMappings(ref options, null, OnQueryExternalAccountMappingsCompleted);
        }

        private void OnQueryExternalAccountMappingsCompleted(ref QueryExternalAccountMappingsCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnQueryExternalAccountMappingsCompleted): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnQueryExternalAccountMappingsCompleted): Error calling QueryExternalAccountMappings: " + data.ResultCode);
                return;
            }

            Debug.Log("Friends (OnQueryExternalAccountMappingsCompleted): QueryExternalAccountMappings successful");

            Dictionary<EpicAccountId, ProductUserId> mappingsToUpdate = new Dictionary<EpicAccountId, ProductUserId>();

            foreach (FriendData friend in CachedFriends.Values)
            {
                if(friend.UserProductUserId == null || !friend.UserProductUserId.IsValid())
                {
                    Result result = friend.UserId.ToString(out Utf8String epidAccountIdString);

                    if(result == Result.Success)
                    {
                        GetExternalAccountMappingsOptions options = new GetExternalAccountMappingsOptions()
                        {
                            AccountIdType = ExternalAccountType.Epic,
                            LocalUserId = data.LocalUserId,
                            TargetExternalUserId = epidAccountIdString
                        };

                        ProductUserId newProductUserId = ConnectHandle.GetExternalAccountMapping(ref options);
                        if(newProductUserId != null && newProductUserId.IsValid())
                        {
                            mappingsToUpdate.Add(friend.UserId, newProductUserId);
                        }
                        else
                        {
                            Debug.LogWarningFormat("Friends (OnQueryExternalAccountMappingsCompleted): No connected Epic Account associated with EpicAccountId = ({0})", epidAccountIdString);
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("Friends (OnQueryExternalAccountMappingsCompleted): ToString of FriendData.UserId failed with result = {0}", result);
                    }
                }
            }

            foreach(KeyValuePair<EpicAccountId, ProductUserId> kvp in mappingsToUpdate)
            {
                if(CachedFriends.TryGetValue(kvp.Key, out FriendData friend))
                {
                    friend.UserProductUserId = kvp.Value;
                }
                else
                {
                    Debug.LogErrorFormat("Friends (OnQueryExternalAccountMappingsCompleted): Error updating ProductUserId for friend {0}", kvp.Key);
                }
            }
        }

        // Search Results
        public void SearchFriendList(string displayName)
        {
            CachedSearchResults.Clear();
            CachedSearchResultsDirty = true;

            foreach (FriendData data in CachedFriends.Values)
            {
                if (data.Name.Contains(displayName))
                {
                    CachedSearchResults.Add(data.UserId, data);
                }
            }
        }

        public void QueryUserInfo(string displayName, OnFriendsCallback QueryUserInfoCompleted)
        {
            CachedSearchResults.Clear();
            CachedSearchResultsDirty = true;

            QueryUserInfo(EOSManager.Instance.GetLocalUserId(), displayName, QueryUserInfoCompleted);
        }

        public void QueryUserInfo(EpicAccountId localUserId, string displayName, OnFriendsCallback QueryUserInfoCompleted)
        {
            QueryUserInfoByDisplayNameOptions options = new QueryUserInfoByDisplayNameOptions()
            {
                LocalUserId = localUserId,
                DisplayName = displayName
            };

            UserInfoHandle.QueryUserInfoByDisplayName(ref options, null, QueryUserInfoByDisplaynameCompleted);
        }

        private void QueryUserInfoByDisplaynameCompleted(ref QueryUserInfoByDisplayNameCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (QueryUserInfoByDisplaynameCompleted): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (QueryUserInfoByDisplaynameCompleted): ResultCode error: {0}", data.ResultCode);
                return;
            }

            Debug.LogFormat("Friends (QueryUserInfoByDisplaynameCompleted): Query User Info Complete - UserId: {0}", data.TargetUserId);

            FriendData foundUser = new FriendData()
            {
                Name = data.DisplayName,
                Status = FriendsStatus.NotFriends,
                LocalUserId = data.LocalUserId,
                UserId = data.TargetUserId
            };

            CachedSearchResults.Add(foundUser.UserId, foundUser);
            CachedSearchResultsDirty = true;
        }

        public void AddFriend(EpicAccountId friendUserId)
        {
            if(!friendUserId.IsValid())
            {
                Debug.LogError("EOSFriendManager (QueryUserInfoByDisplaynameCompleted): friendUserId parameter is invalid.");
                return;
            }

            SendInviteOptions options = new SendInviteOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = friendUserId
            };

            FriendsHandle.SendInvite(ref options, null, SendInviteCompleted);
        }

        private void SendInviteCompleted(ref SendInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (SendInviteCompleted): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (SendInviteCompleted): ResultCode error: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Friends (SendInviteCompleted): Invite Sent.");

            QueryFriends(data.LocalUserId, null);
        }

        // Friends List

        public void QueryUserInfo(EpicAccountId targetUserId, OnFriendsCallback QueryUserInfoCompleted)
        {
            if (!targetUserId.IsValid())
            {
                Debug.LogError("Friends (QueryUserInfo): targetUserId parameter is invalid!");
                QueryUserInfoCompleted?.Invoke(Result.InvalidParameters);
                return;
            }

            QueryUserInfoOptions options = new QueryUserInfoOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = targetUserId
            };

            QueryUserInfoCallback = QueryUserInfoCompleted;

            UserInfoHandle.QueryUserInfo(ref options, null, OnQueryUserInfoCompleted);
        }

        private void OnQueryUserInfoCompleted(ref QueryUserInfoCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnQueryUserInfoCompleted): data is null");
            //    QueryUserInfoCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnQueryUserInfoCompleted): Error calling QueryUserInfo: " + data.ResultCode);
                QueryUserInfoCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Friends (OnQueryUserInfoCompleted): QueryUserInfo successful");
            QueryUserInfoCallback?.Invoke(Result.Success);

            CopyUserInfoOptions options = new CopyUserInfoOptions()
            {
                LocalUserId = data.LocalUserId,
                TargetUserId = data.TargetUserId
            };

            Result result = UserInfoHandle.CopyUserInfo(ref options, out UserInfoData? userInfo);

            if(result != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnQueryUserInfoCompleted): CopyUserInfo error: {0}", result);
                QueryUserInfoCallback?.Invoke(result);
                return;
            }
            FriendData retrievedFriendData = new FriendData()
            {
                LocalUserId = data.LocalUserId,
                UserId = data.TargetUserId,
                Name = userInfo?.DisplayName
            };

            if (CachedFriends.TryGetValue(retrievedFriendData.UserId, out FriendData friend))
            {
                Debug.Log("Friends (OnQueryUserInfoCompleted): FriendData (LocalUserId, Name) Updated");
                friend.LocalUserId = retrievedFriendData.LocalUserId;
                friend.Name = retrievedFriendData.Name;

                CachedFriendsDirty = true;
            }
            else
            {
                CachedFriends.Add(retrievedFriendData.UserId, retrievedFriendData);
                CachedFriendsDirty = true;
                Debug.Log("Friends (OnQueryUserInfoCompleted): FriendData Added");
            }

            QueryUserInfoCallback?.Invoke(Result.Success);
        }


        /// <summary>Display Social Overlay</summary>
        public void ShowFriendsOverlay(OnFriendsCallback ShowFriendsOverlayCompleted)
        {
            ShowFriendsOverlayCallback = ShowFriendsOverlayCompleted;
            var showFriendsOptions = new ShowFriendsOptions() { LocalUserId = EOSManager.Instance.GetLocalUserId() };
            EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().ShowFriends(ref showFriendsOptions, null, OnShowFriendsCallback);
        }

        private void OnShowFriendsCallback(ref ShowFriendsCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnShowFriendsCallback): data is null");
            //    ShowFriendsOverlayCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnShowFriendsCallback): Error calling ShowFriends: " + data.ResultCode);
                ShowFriendsOverlayCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Friends (OnShowFriendsCallback): ShowFriends successful");
            ShowFriendsOverlayCallback?.Invoke(Result.Success);
        }

        //-------------------------------------------------------------------------
        /// <summary>Hides Social Overlay</summary>
        public void HideFriendsOverlay(OnFriendsCallback HideFriendsOverlayCompleted)
        {
            HideFriendsOverlayCallback = HideFriendsOverlayCompleted;
            var hideFriendsOptions = new HideFriendsOptions() { LocalUserId = EOSManager.Instance.GetLocalUserId() };
            EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().HideFriends(ref hideFriendsOptions, null, OnHideFriendsCallback);
        }

        //-------------------------------------------------------------------------
        private void OnHideFriendsCallback(ref HideFriendsCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnHideFriendsCallback): data is null");
            //    HideFriendsOverlayCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Friends (OnHideFriendsCallback): Error calling HideFriends: " + data.ResultCode);
                HideFriendsOverlayCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Friends (OnHideFriendsCallback): HideFriends successful");
            HideFriendsOverlayCallback?.Invoke(Result.Success);
        }

        public void SubscribeToFriendUpdates(EpicAccountId userId)
        {
            if (userId == null || !userId.IsValid())
            {
                Debug.LogWarning("Friends (SubscribeToFriendUpdates): userId parameter is not valid.");
                return;
            }

            UnsubscribeFromFriendUpdates(userId);

            // Status
            var addNotifyFriendsUpdateOptions = new AddNotifyFriendsUpdateOptions();
            ulong notificationId = FriendsHandle.AddNotifyFriendsUpdate(ref addNotifyFriendsUpdateOptions, null, OnFriendsUpdateCallbackHandler);

            if(notificationId == Common.InvalidNotificationid)
            {
                Debug.LogError("Friends (SubscribeToFriendUpdates): Could not subscribe to friend update notifications.");
            }
            else
            {
                FriendNotifications[userId] = notificationId;
            }

            // Presence
            var addNotifyOnPresenceChangedOptions = new AddNotifyOnPresenceChangedOptions();
            ulong presenceNotificationId = PresenceHandle.AddNotifyOnPresenceChanged(ref addNotifyOnPresenceChangedOptions , null, OnPresenceChangedCallbackHandler);

            if(presenceNotificationId == Common.InvalidNotificationid)
            {
                Debug.LogError("Friends (SubscribeToFriendUpdates): Could not subscribe to presence changed notifications.");
            }
            else
            {
                PresenceNotifications[userId] = presenceNotificationId;
            }
        }

        private void UnsubscribeFromFriendUpdates(EpicAccountId userId)
        {
            if(userId == null || !userId.IsValid())
            {
                Debug.LogWarning("Friends (UnsubscribeFromFriendUpdates): userId parameter is not valid.");
                return;
            }

            if(FriendNotifications == null || PresenceNotifications == null)
            {
                Debug.LogWarning("Friends (UnsubscribeFromFriendUpdates): Not initialized yet, try again.");
                return;
            }

            if(FriendNotifications.TryGetValue(userId, out ulong friendNotificationId))
            {
                FriendsHandle.RemoveNotifyFriendsUpdate(friendNotificationId);
                FriendNotifications.Remove(userId);
            }

            if(PresenceNotifications.TryGetValue(userId, out ulong presenceNotificationId))
            {
                PresenceHandle.RemoveNotifyOnPresenceChanged(presenceNotificationId);
                PresenceNotifications.Remove(userId);
            }
        }

        private void OnFriendsUpdateCallbackHandler(ref OnFriendsUpdateInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnFriendsUpdateCallbackHandler): data is null");
            //    return;
            //}

            // Friend Status Changed

            if(EOSManager.Instance.GetLocalUserId() == data.LocalUserId)
            {
                if(data.CurrentStatus == FriendsStatus.NotFriends)
                {
                    if(CachedFriends.ContainsKey(data.TargetUserId)) //UserId
                    {
                        CachedFriends.Remove(data.TargetUserId);
                        CachedFriendsDirty = true;
                    }
                }
                else
                {
                    if(CachedFriends.TryGetValue(data.TargetUserId, out FriendData friend))
                    {
                        if(friend.Status != data.CurrentStatus)
                        {
                            // Status changed
                            friend.Status = data.CurrentStatus;
                            CachedFriendsDirty = true;
                        }
                    }
                    else
                    {
                        // New Friend
                        QueryFriends(null);
                    }
                }
            }
        }

        private void OnPresenceChangedCallbackHandler(ref PresenceChangedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Friends (OnPresenceChangedCallbackHandler): data is null");
            //    return;
            //}

            QueryPresenceInfo(data.LocalUserId, data.PresenceUserId);
        }

        /// <summary>User Logged In actions</summary>
        /// <list type="bullet">
        ///     <item><description><c>QueryFriends()</c></description></item>
        /// </list>
        public void OnLoggedIn()
        {
            QueryFriends(null);
            SubscribeToFriendUpdates(EOSManager.Instance.GetLocalUserId());
        }

        /// <summary>User Logged Out actions</summary>
        /// <list type="bullet">
        ///     <item><description>NONE</description></item>
        /// </list>
        public void OnLoggedOut()
        {
            EpicAccountId localUser = EOSManager.Instance.GetLocalUserId();

            if(localUser != null && localUser.IsValid())
            {
                UnsubscribeFromFriendUpdates(localUser);
            }
        }
    }
}
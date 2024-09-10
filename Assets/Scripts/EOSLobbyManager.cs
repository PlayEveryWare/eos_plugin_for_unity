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
    using Epic.OnlineServices.Lobby;
    using Epic.OnlineServices.RTC;
    using Epic.OnlineServices.RTCAudio;
	
    public enum LobbyChangeType
    { 
        Create, 
        Join, 
        Leave,
        Kicked
    }

    /// <summary>
    /// Class represents all Lobby properties
    /// </summary>
    public class Lobby
    {
        public string Id;
        public ProductUserId LobbyOwner = new ProductUserId();
        public EpicAccountId LobbyOwnerAccountId = new EpicAccountId();
        public string LobbyOwnerDisplayName;
        public string BucketId;
        public uint MaxNumLobbyMembers = 0;
        public LobbyPermissionLevel LobbyPermissionLevel = LobbyPermissionLevel.Publicadvertised;
        public uint AvailableSlots = 0;
        public bool AllowInvites = true;
        public bool? DisableHostMigration;

        /// <summary>
        /// Indicates whether the local user has deafened themselves
        /// (this doesn't carry over between rooms).
        /// </summary>
        public bool IsLocalUserDeafened;

        // Cached copy of the RoomName of the RTC room that our lobby has, if any
        public string RTCRoomName = string.Empty;
        // Are we currently connected to an RTC room?
        public bool RTCRoomConnected = false;
        /** Notification for RTC connection status changes */
        public NotifyEventHandle RTCRoomConnectionChanged; // EOS_INVALID_NOTIFICATIONID;
        /** Notification for RTC room participant updates (new players or players leaving) */
        public NotifyEventHandle RTCRoomParticipantUpdate; // EOS_INVALID_NOTIFICATIONID;
        /** Notification for RTC audio updates (talking status or mute changes) */
        public NotifyEventHandle RTCRoomParticipantAudioUpdate; // EOS_INVALID_NOTIFICATIONID;

        public bool PresenceEnabled = false;
        public bool RTCRoomEnabled = false;

        public List<LobbyAttribute> Attributes = new List<LobbyAttribute>();
        public List<LobbyMember> Members = new List<LobbyMember>();

        // Utility data

        public bool _SearchResult = false;
        public bool _BeingCreated = false;

        /// <summary>
        /// Checks if Lobby Id is valid
        /// </summary>
        /// <returns>True if valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }

        /// <summary>
        /// Checks if the specified <c>ProductUserId</c> is the current owner
        /// </summary>
        /// <param name="userProductId">Specified <c>ProductUserId</c></param>
        /// <returns>True if specified user is owner</returns>
        public bool IsOwner(ProductUserId userProductId)
        {
            return userProductId == LobbyOwner;
        }

        /// <summary>
        /// Clears local cache of Lobby Id, owner, attributes and members
        /// </summary>
        public void Clear()
        {
            Id = string.Empty;
            LobbyOwner = new ProductUserId();
            Attributes.Clear();
            Members.Clear();
        }

        /// <summary>
        /// Initializing the given Lobby Id and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified Lobby Id</param>
        public void InitFromLobbyHandle(string lobbyId)
        {
            if (string.IsNullOrEmpty(lobbyId))
            {
                return;
            }

            Id = lobbyId;

            CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions();
            options.LobbyId = Id;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandle(ref options, out LobbyDetails outLobbyDetailsHandle);
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (InitFromLobbyHandle): can't get lobby info handle. Error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (InitFromLobbyHandle): can't get lobby info handle. outLobbyDetailsHandle is null");
                return;
            }

            InitFromLobbyDetails(outLobbyDetailsHandle);
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Initializing the given <c>LobbyDetails</c> handle and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified <c>LobbyDetails</c> handle</param>
        public void InitFromLobbyDetails(LobbyDetails outLobbyDetailsHandle)
        {
            // get owner
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if (newLobbyOwner != LobbyOwner)
            {
                LobbyOwner = newLobbyOwner;
                LobbyOwnerAccountId = new EpicAccountId();
                LobbyOwnerDisplayName = string.Empty;
            }

            // copy lobby info
            var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
            Result infoResult = outLobbyDetailsHandle.CopyInfo(ref lobbyDetailsCopyInfoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            if (infoResult != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (InitFromLobbyDetails): can't copy lobby info. Error code: {0}", infoResult);
                return;
            }
            if (outLobbyDetailsInfo == null)
            {
                Debug.LogError("Lobbies: (InitFromLobbyDetails) could not copy info: outLobbyDetailsInfo is null.");
                return;
            }

            Id = outLobbyDetailsInfo?.LobbyId;
            MaxNumLobbyMembers = (uint)(outLobbyDetailsInfo?.MaxMembers);
            LobbyPermissionLevel = (LobbyPermissionLevel)(outLobbyDetailsInfo?.PermissionLevel);
            AllowInvites = (bool)(outLobbyDetailsInfo?.AllowInvites);
            AvailableSlots = (uint)(outLobbyDetailsInfo?.AvailableSlots);
            BucketId = outLobbyDetailsInfo?.BucketId;
            RTCRoomEnabled = (bool)(outLobbyDetailsInfo?.RTCRoomEnabled);

            // get attributes
            Attributes.Clear();
            var lobbyDetailsGetAttributeCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = outLobbyDetailsHandle.GetAttributeCount(ref lobbyDetailsGetAttributeCountOptions);
            for (uint i = 0; i < attrCount; i++)
            {
                LobbyDetailsCopyAttributeByIndexOptions attrOptions = new LobbyDetailsCopyAttributeByIndexOptions();
                attrOptions.AttrIndex = i;
                Result copyAttrResult = outLobbyDetailsHandle.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                if (copyAttrResult == Result.Success && outAttribute != null && outAttribute?.Data != null)
                {
                    LobbyAttribute attr = new LobbyAttribute();
                    attr.InitFromAttribute(outAttribute);
                    Attributes.Add(attr);
                }
            }

            // get members
            List<LobbyMember> OldMembers = new List<LobbyMember>(Members);
            Members.Clear();

            var lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = outLobbyDetailsHandle.GetMemberCount(ref lobbyDetailsGetMemberCountOptions);

            for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                var lobbyDetailsGetMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions() { MemberIndex = (uint)memberIndex };
                ProductUserId memberId = outLobbyDetailsHandle.GetMemberByIndex(ref lobbyDetailsGetMemberByIndexOptions);
                Members.Insert((int)memberIndex, new LobbyMember() { ProductId = memberId });

                // member attributes
                var lobbyDetailsGetMemberAttributeCountOptions = new LobbyDetailsGetMemberAttributeCountOptions() { TargetUserId = memberId };
                int memberAttributeCount = (int)outLobbyDetailsHandle.GetMemberAttributeCount(ref lobbyDetailsGetMemberAttributeCountOptions);

                for (int attributeIndex = 0; attributeIndex < memberAttributeCount; attributeIndex++)
                {
                    var lobbyDetailsCopyMemberAttributeByIndexOptions = new LobbyDetailsCopyMemberAttributeByIndexOptions() { AttrIndex = (uint)attributeIndex, TargetUserId = memberId };
                    Result memberAttributeResult = outLobbyDetailsHandle.CopyMemberAttributeByIndex(ref lobbyDetailsCopyMemberAttributeByIndexOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);

                    if (memberAttributeResult != Result.Success)
                    {
                        Debug.LogFormat("Lobbies (InitFromLobbyDetails): can't copy member attribute. Error code: {0}", memberAttributeResult);
                        continue;
                    }

                    LobbyAttribute newAttribute = new LobbyAttribute();
                    newAttribute.InitFromAttribute(outAttribute);
 
                    Members[memberIndex].MemberAttributes.Add(newAttribute.Key, newAttribute);
                }

                // Copy RTC Status from old members
                foreach(LobbyMember oldLobbyMember in OldMembers)
                {
                    LobbyMember newMember = Members[memberIndex];
                    if(oldLobbyMember.ProductId != newMember.ProductId)
                    {
                        continue;
                    }

                    // Copy RTC status to new object
                    newMember.RTCState = oldLobbyMember.RTCState;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Class represents all Lobby Invite properties
    /// </summary>
    public class LobbyInvite
    {
        public Lobby Lobby = new Lobby();
        public LobbyDetails LobbyInfo = new LobbyDetails();
        public ProductUserId FriendId;
        public EpicAccountId FriendEpicId;
        public string FriendDisplayName;
        public string InviteId;

        public bool IsValid()
        {
            return Lobby.IsValid();
        }

        public void Clear()
        {
            Lobby.Clear();
            LobbyInfo.Release();
            FriendId = new ProductUserId();
            FriendEpicId = new EpicAccountId();
            FriendDisplayName = string.Empty;
            InviteId = string.Empty;
        }
    }

    /// <summary>
    /// Class represents all Lobby Attribute properties
    /// </summary>
    public class LobbyAttribute
    {
        public LobbyAttributeVisibility Visibility = LobbyAttributeVisibility.Public;

        public AttributeType ValueType = AttributeType.String;

        //Key is uppercased when transmitted, so this should be uppercase
        public string Key;

        //Only one of the following properties will have valid data (depending on 'ValueType')
        public long? AsInt64 = 0;
        public double? AsDouble = 0.0;
        public bool? AsBool = false;
        public string AsString;

        public AttributeData AsAttribute
        {
            get
            {
                AttributeData attrData = new AttributeData();
                attrData.Key = Key;
                attrData.Value = new AttributeDataValue();

                switch (ValueType)
                {
                    case AttributeType.String:
                        attrData.Value = AsString;
                        break;
                    case AttributeType.Int64:
                        attrData.Value = (AttributeDataValue)AsInt64;
                        break;
                    case AttributeType.Double:
                        attrData.Value = (AttributeDataValue)AsDouble;
                        break;
                    case AttributeType.Boolean:
                        attrData.Value = (AttributeDataValue)AsBool;
                        break;
                }

                return attrData;
            }
        }

        public override bool Equals(object other)
        {
            LobbyAttribute lobbyAttr = (LobbyAttribute)other;

            return ValueType == lobbyAttr.ValueType &&
                AsInt64 == lobbyAttr.AsInt64 &&
                AsDouble == lobbyAttr.AsDouble &&
                AsBool == lobbyAttr.AsBool &&
                AsString == lobbyAttr.AsString &&
                Key == lobbyAttr.Key &&
                Visibility == lobbyAttr.Visibility;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void InitFromAttribute(Epic.OnlineServices.Lobby.Attribute? attributeParam)
        {
            AttributeData attributeData = (AttributeData)(attributeParam?.Data);

            Key = attributeData.Key;
            ValueType = attributeData.Value.ValueType;

            switch (attributeData.Value.ValueType)
            {
                case AttributeType.Boolean:
                    AsBool = attributeData.Value.AsBool;
                    break;
                case AttributeType.Int64:
                    AsInt64 = attributeData.Value.AsInt64;
                    break;
                case AttributeType.Double:
                    AsDouble = attributeData.Value.AsDouble;
                    break;
                case AttributeType.String:
                    AsString = attributeData.Value.AsUtf8;
                    break;
            }
        }
    }

    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    public class LobbyMember
    {
        //public EpicAccountId AccountId;
        public ProductUserId ProductId;

        public string DisplayName
        {
            get
            {
                MemberAttributes.TryGetValue(DisplayNameKey, out LobbyAttribute nameAttrib);
                return nameAttrib?.AsString ?? string.Empty;
            }
        }

        public const string DisplayNameKey = "DISPLAYNAME";

        public Dictionary<string, LobbyAttribute> MemberAttributes = new Dictionary<string, LobbyAttribute>();

        public LobbyRTCState RTCState = new LobbyRTCState();
    }

    /// <summary>
    /// Class represents RTC State (Voice) of a Lobby
    /// </summary>

    public class LobbyRTCState
    {
        // Is this person currently connected to the RTC room?
        public bool IsInRTCRoom = false;

        // Is this person currently talking (audible sounds from their audio output)
        public bool IsTalking = false;

        // We have locally muted this person (others can still hear them)
        public bool IsLocalMuted = false;

        // Has this person muted their own audio output (nobody can hear them)
        public bool IsAudioOutputDisabled = false;

        // Are we currently muting this person?
        public bool MuteActionInProgress = false;

        // Has this person enabled press to talk
        public bool PressToTalkEnabled = false;

        // We have locally blocked this person from receiving and sending audio
        public bool IsBlocked = false;
    }

    /// <summary>
    /// Class represents a request to Join a lobby
    /// </summary>
    public class LobbyJoinRequest
    {
        string Id = string.Empty;
        LobbyDetails LobbyInfo = new LobbyDetails();

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }

        public void Clear()
        {
            Id = string.Empty;
            LobbyInfo = new LobbyDetails();
        }
    }

    /// <summary>
    /// Class <c>EOSLobbyManager</c> is a simplified wrapper for EOS [Lobby Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Lobby/index.html).
    /// </summary>

    public class EOSLobbyManager : IEOSSubManager
    {
        private Lobby CurrentLobby;
        private LobbyJoinRequest ActiveJoin;

        // Pending invites (up to one invite per friend)
        private Dictionary<ProductUserId, LobbyInvite> Invites;
        private LobbyInvite CurrentInvite;

        // Search 
        private LobbySearch CurrentSearch;
        private Dictionary<Lobby, LobbyDetails> SearchResults;


        //NotificationId
        private NotifyEventHandle LobbyUpdateNotification;
        private NotifyEventHandle LobbyMemberUpdateNotification;
        private NotifyEventHandle LobbyMemberStatusNotification;
        private NotifyEventHandle LobbyInviteNotification;
        private NotifyEventHandle LobbyInviteAcceptedNotification;
        private NotifyEventHandle JoinLobbyAcceptedNotification;
        private NotifyEventHandle LeaveLobbyRequestedNotification;

        // TODO: Does this constant exist in the EOS SDK C# Wrapper?
        private const ulong EOS_INVALID_NOTIFICATIONID = 0;

        public bool _Dirty = true;

        // Manager Callbacks
        private OnLobbySearchCallback LobbySearchCallback;

        public delegate void OnLobbyCallback(Result result);
        public delegate void OnLobbySearchCallback(Result result);

        public delegate void OnMemberUpdateCallback(string LobbyId, ProductUserId MemberId);

        private List<OnMemberUpdateCallback> MemberUpdateCallbacks;

        public class LobbyChangeEventArgs
        {
            public string LobbyId { get; }
            public LobbyChangeType LobbyChangeType { get; }

            public LobbyChangeEventArgs(string lobbyId, LobbyChangeType changeType)
            {
                LobbyId = lobbyId;
                LobbyChangeType = changeType;
            }
        }

        public delegate void LobbyChangeEventHandler(object sender, LobbyChangeEventArgs e);

        public event LobbyChangeEventHandler LobbyChanged;

        protected virtual void OnLobbyChanged(LobbyChangeEventArgs args)
        {
            LobbyChangeEventHandler handler = LobbyChanged;
            handler?.Invoke(this, args);
        }

        private List<Action> LobbyUpdateCallbacks;

        private EOSUserInfoManager UserInfoManager;
        
        public LocalRTCOptions? customLocalRTCOptions;

        // Init
        public EOSLobbyManager()
        {
            UserInfoManager = EOSManager.Instance.GetOrCreateManager<EOSUserInfoManager>();

            CurrentLobby = new Lobby();
            ActiveJoin = new LobbyJoinRequest();

            Invites = new Dictionary<ProductUserId, LobbyInvite>();
            CurrentInvite = null;

            CurrentSearch = new LobbySearch();
            SearchResults = new Dictionary<Lobby, LobbyDetails>();

            LobbySearchCallback = null;

            MemberUpdateCallbacks = new List<OnMemberUpdateCallback>();
            LobbyUpdateCallbacks = new List<Action>();
        }

        public Lobby GetCurrentLobby()
        {
            return CurrentLobby;
        }

        public Dictionary<ProductUserId, LobbyInvite> GetInvites()
        {
            return Invites;
        }

        public LobbyInvite GetCurrentInvite()
        {
            return CurrentInvite;
        }

        public LobbySearch GetCurrentSearch()
        {
            return CurrentSearch;
        }

        public Dictionary<Lobby, LobbyDetails> GetSearchResults()
        {
            return SearchResults;
        }

        private bool IsLobbyNotificationValid(NotifyEventHandle handle)
        {
            return handle != null && handle.IsValid();
        }

        // Helper method to keep the code cleaner

        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLobbyUpdateReceived](https://dev.epicgames.com/docs/api-ref/functions/eos-lobby-add-notify-lobby-update-received)
        /// The callback will only run if a listener is subscribed, which is done in <see cref="SubscribeToLobbyUpdates"/>.
        /// </summary>
        /// <param name="lobbyInterface">Handle to the lobby interface.</param>
        /// <param name="notificationFn">Callback to receive notification when lobby update is received.</param>
        /// <returns>Handle representing the registered callback</returns>
        private ulong AddNotifyLobbyUpdateReceived(LobbyInterface lobbyInterface, OnLobbyUpdateReceivedCallback notificationFn)
        {
            var options = new AddNotifyLobbyUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyUpdateReceived(ref options, null, notificationFn);
        }

        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLobbyMemberUpdateReceived](https://dev.epicgames.com/docs/api-ref/functions/eos-lobby-add-notify-lobby-member-update-received)
        /// The callback will only run if a listener is subscribed, which is done in <see cref="SubscribeToLobbyUpdates"/>.
        /// </summary>
        /// <param name="lobbyInterface">Handle to the lobby interface.</param>
        /// <param name="notificationFn">Callback to receive notification when lobby member update is received.</param>
        /// <returns>Handle representing the registered callback</returns>
        private ulong AddNotifyLobbyMemberUpdateReceived(LobbyInterface lobbyInterface, OnLobbyMemberUpdateReceivedCallback notificationFn)
        {
            var options = new AddNotifyLobbyMemberUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberUpdateReceived(ref options, null, notificationFn);
        }

        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLobbyMemberStatusReceived](https://dev.epicgames.com/docs/api-ref/functions/eos-lobby-add-notify-lobby-member-status-received)
        /// The callback will only run if a listener is subscribed, which is done in <see cref="SubscribeToLobbyUpdates"/>.
        /// </summary>
        /// <param name="lobbyInterface">Handle to the lobby interface.</param>
        /// <param name="notificationFn">Callback to receive notification when lobby member status is received.</param>
        /// <returns>Handle representing the registered callback</returns>
        private ulong AddNotifyLobbyMemberStatusReceived(LobbyInterface lobbyInterface, OnLobbyMemberStatusReceivedCallback notificationFn)
        {
            var options = new AddNotifyLobbyMemberStatusReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberStatusReceived(ref options, null, notificationFn);
        }

        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLeaveLobbyRequested](https://dev.epicgames.com/docs/api-ref/functions/eos-lobby-add-notify-leave-lobby-requested)
        /// The callback will only run if a listener is subscribed, which is done in <see cref="SubscribeToLobbyUpdates"/>.
        /// </summary>
        /// <param name="lobbyInterface">Handle to the lobby interface.</param>
        /// <param name="notificationFn">Callback to receive notification when lobby member leave request is received.</param>
        /// <returns>Handle representing the registered callback</returns>
        private ulong AddNotifyLeaveLobbyRequested(LobbyInterface lobbyInterface, OnLeaveLobbyRequestedCallback notificationFn)
        {
            var options = new AddNotifyLeaveLobbyRequestedOptions();
            return lobbyInterface.AddNotifyLeaveLobbyRequested(ref options, null, notificationFn);
        }

        /// <summary>
        /// Subscribes to Lobby notifications.
        /// This method must be run in order to receive any updates from other user's Lobby changes.
        /// </summary>
        private void SubscribeToLobbyUpdates()
        {
            if(IsLobbyNotificationValid(LobbyUpdateNotification) ||
                IsLobbyNotificationValid(LobbyMemberUpdateNotification) || 
                IsLobbyNotificationValid(LobbyMemberStatusNotification) ||
                IsLobbyNotificationValid(LeaveLobbyRequestedNotification))
            {
                Debug.LogError("Lobbies (SubscribeToLobbyUpdates): SubscribeToLobbyUpdates called but already subscribed!");
                return;
            }
            

            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            LobbyUpdateNotification = new NotifyEventHandle(AddNotifyLobbyUpdateReceived(lobbyInterface, OnLobbyUpdateReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyUpdateReceived(handle);
            });

            LobbyMemberUpdateNotification = new NotifyEventHandle(AddNotifyLobbyMemberUpdateReceived(lobbyInterface, OnMemberUpdateReceived), (ulong handle) => 
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberUpdateReceived(handle);
            });

            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnMemberStatusReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });

            LeaveLobbyRequestedNotification = new NotifyEventHandle(AddNotifyLeaveLobbyRequested(lobbyInterface, OnLeaveLobbyRequested), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLeaveLobbyRequested(handle);
            });
        }

        private void UnsubscribeFromLobbyUpdates()
        {
            LobbyUpdateNotification.Dispose();
            LobbyMemberUpdateNotification.Dispose();
            LobbyMemberStatusNotification.Dispose();
            LeaveLobbyRequestedNotification.Dispose();
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Subscribes to Lobby invites.
        /// This method must be run in order to receive messages relating to lobby invitations.
        /// </summary>
        private void SubscribeToLobbyInvites()
        {
            if (IsLobbyNotificationValid(LobbyInviteNotification) || 
                IsLobbyNotificationValid(LobbyInviteAcceptedNotification) || 
                IsLobbyNotificationValid(JoinLobbyAcceptedNotification) )
            {
                Debug.LogError("Lobbies (SubscribeToLobbyInvites): SubscribeToLobbyInvites called but already subscribed!");
                return;
            }

            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            var addNotifyLobbyInviteReceivedOptions = new AddNotifyLobbyInviteReceivedOptions();
            LobbyInviteNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyInviteReceived(ref addNotifyLobbyInviteReceivedOptions, null, OnLobbyInviteReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyInviteReceived(handle);
            });

            var addNotifyLobbyInviteAcceptedOptions = new AddNotifyLobbyInviteAcceptedOptions();
            LobbyInviteAcceptedNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyInviteAccepted(ref addNotifyLobbyInviteAcceptedOptions, null, OnLobbyInviteAccepted), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyInviteAccepted(handle);
            });

            var addNotifyJoinLobbyAcceptedOptions = new AddNotifyJoinLobbyAcceptedOptions();
            JoinLobbyAcceptedNotification = new NotifyEventHandle(lobbyInterface.AddNotifyJoinLobbyAccepted(ref addNotifyJoinLobbyAcceptedOptions, null, OnJoinLobbyAccepted), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyJoinLobbyAccepted(handle);
            });
        }

        //-------------------------------------------------------------------------
        private void UnsubscribeFromLobbyInvites()
        {
            LobbyInviteNotification.Dispose(); 
            LobbyInviteAcceptedNotification.Dispose(); 
            JoinLobbyAcceptedNotification.Dispose();
        }

        private string GetRTCRoomName()
        {
            GetRTCRoomNameOptions options = new GetRTCRoomNameOptions()
            {
                LobbyId = CurrentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            Result result = EOSManager.Instance.GetEOSLobbyInterface().GetRTCRoomName(ref options, out Utf8String roomName);

            if(result != Result.Success)
            {
                Debug.LogFormat("Lobbies (GetRTCRoomName): Could not get RTC Room Name. Error Code: {0}", result);
                return string.Empty;
            }

            Debug.LogFormat("Lobbies (GetRTCRoomName): Found RTC Room Name for lobby. RooName={0}", roomName);

            return roomName;
        }

        private void UnsubscribeFromRTCEvents()
        {
            if(!CurrentLobby.RTCRoomEnabled)
            {
                return;
            }

            CurrentLobby.RTCRoomParticipantAudioUpdate.Dispose();
            CurrentLobby.RTCRoomParticipantUpdate.Dispose();
            CurrentLobby.RTCRoomConnectionChanged.Dispose();

            CurrentLobby.RTCRoomName = string.Empty;
        }

        private void SubscribeToRTCEvents()
        {
            if(!CurrentLobby.RTCRoomEnabled)
            {
                Debug.LogWarning("Lobbies (SubscribeToRTCEvents): RTC Room is disabled.");
                return;
            }

            CurrentLobby.RTCRoomName = GetRTCRoomName();

            if(string.IsNullOrEmpty(CurrentLobby.RTCRoomName))
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Unable to bind to RTC Room Name, failing to bind delegates.");
                return;
            }

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();

            // Register for connection status changes
            AddNotifyRTCRoomConnectionChangedOptions addNotifyRTCRoomConnectionChangedOptions = new AddNotifyRTCRoomConnectionChangedOptions();
            CurrentLobby.RTCRoomConnectionChanged = new NotifyEventHandle(lobbyInterface.AddNotifyRTCRoomConnectionChanged(ref addNotifyRTCRoomConnectionChangedOptions, null, OnRTCRoomConnectionChangedReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyRTCRoomConnectionChanged(handle);
            });

            if(!CurrentLobby.RTCRoomConnectionChanged.IsValid())
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Failed to bind to Lobby NotifyRTCRoomConnectionChanged notification.");
                return;
            }

            // Get the current room connection status now that we're listening for changes
            IsRTCRoomConnectedOptions isRTCRoomConnectedOptions = new IsRTCRoomConnectedOptions()
            {
                LobbyId = CurrentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            Result result = lobbyInterface.IsRTCRoomConnected(ref isRTCRoomConnectedOptions, out bool isConnected);

            if (result != Result.Success)
            {
                Debug.LogFormat("Lobbies (SubscribeToRTCEvents): Failed to get RTC Room connection status:. Error Code: {0}", result);
                return;
            }
            else
            {
                CurrentLobby.RTCRoomConnected = isConnected;
            }

            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            // Register for RTC Room participant changes
            AddNotifyParticipantStatusChangedOptions addNotifyParticipantsStatusChangedOptions = new AddNotifyParticipantStatusChangedOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName
            };

            CurrentLobby.RTCRoomParticipantUpdate = new NotifyEventHandle(rtcHandle.AddNotifyParticipantStatusChanged(ref addNotifyParticipantsStatusChangedOptions, null, OnRTCRoomParticipantStatusChanged), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSRTCInterface().RemoveNotifyParticipantStatusChanged(handle);
            });

            if(!CurrentLobby.RTCRoomParticipantUpdate.IsValid())
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Failed to bind to RTC AddNotifyParticipantStatusChanged notification.");
            }

            // Register for talking changes
            AddNotifyParticipantUpdatedOptions addNotifyParticipantUpdatedOptions = new AddNotifyParticipantUpdatedOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName
            };

            CurrentLobby.RTCRoomParticipantAudioUpdate = new NotifyEventHandle(rtcAudioHandle.AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions, null, OnRTCRoomParticipantAudioUpdateRecieved), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().RemoveNotifyParticipantUpdated(handle);
            });

            // Allow Manual Gain Control
            var setSetting = new SetSettingOptions();
            setSetting.SettingName = "DisableAutoGainControl";
            setSetting.SettingValue = "True";
            var disableAutoGainControlResult = rtcHandle.SetSetting(ref setSetting);
        }

        private void OnRTCRoomConnectionChangedReceived(ref RTCRoomConnectionChangedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomConnectionChangedReceived): RTCRoomConnectionChangedCallbackInfo data is null");
            //    return;
            //}

            Debug.LogFormat("Lobbies (OnRTCRoomConnectionChangedReceived): connection status changed. LobbyId={0}, IsConnected={1}, DisconnectReason={2}", data.LobbyId, data.IsConnected, data.DisconnectReason);

            // OnRTCRoomConnectionChanged

            if(!CurrentLobby.IsValid() || CurrentLobby.Id != data.LobbyId)
            {
                return;
            }

            if(EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                return;
            }

            CurrentLobby.RTCRoomConnected = data.IsConnected;

            foreach(LobbyMember lobbyMember in CurrentLobby.Members)
            {
                if(lobbyMember.ProductId == EOSManager.Instance.GetProductUserId())
                {
                    lobbyMember.RTCState.IsInRTCRoom = data.IsConnected;
                    if(!data.IsConnected)
                    {
                        lobbyMember.RTCState.IsTalking = false;
                    }
                    break;
                }
            }

            _Dirty = true;
        }

        private void OnRTCRoomParticipantStatusChanged(ref ParticipantStatusChangedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomParticipantStatusChanged): ParticipantStatusChangedCallbackInfo data is null");
            //    return;
            //}

            int metadataCount = 0;
            if (data.ParticipantMetadata != null)
            {
                metadataCount = data.ParticipantMetadata.Length;
            }

            Debug.LogFormat("Lobbies (OnRTCRoomParticipantStatusChanged): LocalUserId={0}, Room={1}, ParticipantUserId={2}, ParticipantStatus={3}, MetadataCount={4}",
                data.LocalUserId, 
                data.RoomName, 
                data.ParticipantId, 
                data.ParticipantStatus == RTCParticipantStatus.Joined ? "Joined" : "Left",
                metadataCount);

            // Ensure this update is for our room
            if (string.IsNullOrEmpty(CurrentLobby.RTCRoomName) || !CurrentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //OnRTCRoomParticipantJoined / OnRTCRoomParticipantLeft

            // Find this participant in our list
            foreach (LobbyMember lobbyMember in CurrentLobby.Members)
            {
                if(lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                // Update in-room status
                if (data.ParticipantStatus == RTCParticipantStatus.Joined)
                {
                    lobbyMember.RTCState.IsInRTCRoom = true;
                }
                else
                {
                    lobbyMember.RTCState.IsInRTCRoom = false;
                    lobbyMember.RTCState.IsTalking = false;
                }

                _Dirty = true;
                break;
            }
        }

        private void OnRTCRoomParticipantAudioUpdateRecieved(ref ParticipantUpdatedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomParticipantAudioUpdateRecieved): ParticipantUpdatedCallbackInfo data is null");
            //    return;
            //}

            /* Verbose Logging: Uncomment to print each time audio is received.
            Debug.LogFormat("Lobbies (OnRTCRoomParticipantAudioUpdateRecieved): participant audio updated. LocalUserId={0}, Room={1}, ParticipantUserId={2}, IsTalking={3}, IsAudioDisabled={4}",
                data.LocalUserId,
                data.RoomName,
                data.ParticipantId,
                data.Speaking,
                data.AudioStatus != RTCAudioStatus.Enabled);
            */

            // OnRTCRoomParticipantAudioUpdated

            // Ensure this update is for our room
            if (string.IsNullOrEmpty(CurrentLobby.RTCRoomName) || !CurrentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Find this participant in our list
            foreach(LobbyMember lobbyMember in CurrentLobby.Members)
            {
                if(lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                // Update talking status
                if(lobbyMember.RTCState.IsTalking != data.Speaking)
                {
                    lobbyMember.RTCState.IsTalking = data.Speaking;
                }

                // Only update the audio status for other players (we control their own status)
                if(lobbyMember.ProductId != EOSManager.Instance.GetProductUserId())
                {
                    lobbyMember.RTCState.IsAudioOutputDisabled = data.AudioStatus != RTCAudioStatus.Enabled;
                }

                _Dirty = true;
                break;
            }
        }

        /// <summary>User Logged In actions</summary>
        /// <list type="bullet">
        ///     <item><description>Reset local cache for Invites</description></item>
        /// </list>
        public void OnLoggedIn()
        {
            _Dirty = true;
            CurrentInvite = null;
            CurrentLobby = new Lobby();

            SubscribeToLobbyUpdates();
            SubscribeToLobbyInvites();

            LobbySearchCallback = null;

            EOSManagerPlatformSpecificsSingleton.Instance.SetDefaultAudioSession();
        }

        /// <summary>User Logged Out actions</summary>
        /// <list type="bullet">
        ///     <item><description>Leaves current lobby</description></item>
        ///     <item><description>Unsubscribe from Lobby invites and updates</description></item>
        ///     <item><description>Reset local cache for <c>Lobby</c>, <c>LobbyJoinRequest</c>, Invites, <c>LobbySearch</c> and </description></item>
        /// </list>
        public void OnLoggedOut()
        {
            LeaveLobby(null);
            UnsubscribeFromLobbyInvites();
            UnsubscribeFromLobbyUpdates();

            CurrentLobby = new Lobby();
            ActiveJoin = new LobbyJoinRequest();

            Invites.Clear();
            CurrentInvite = null;

            CurrentSearch = new LobbySearch();
            SearchResults.Clear();
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_CreateLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_CreateLobby/index.html)
        /// </summary>
        /// <param name="lobbyProperties"><b>Lobby</b> properties used to create new lobby</param>
        /// <param name="CreateLobbyCompleted">Callback when create lobby is completed</param>
        public void CreateLobby(Lobby lobbyProperties, OnLobbyCallback CreateLobbyCompleted)
        {
            ProductUserId currentUserProductId = EOSManager.Instance.GetProductUserId();
            if (!currentUserProductId.IsValid())
            {
                Debug.LogError("Lobbies (CreateLobby): Current player is invalid!");

                CreateLobbyCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            // Check if there is current session. Leave it.
            if (CurrentLobby.IsValid())
            {
                Debug.LogWarningFormat("Lobbies (Create Lobby): Leaving Current Lobby '{0}'", CurrentLobby.Id);
                LeaveLobby(null);
            }

            // Create new lobby

            // Max Players
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.LocalUserId = currentUserProductId;
            createLobbyOptions.MaxLobbyMembers = lobbyProperties.MaxNumLobbyMembers;
            createLobbyOptions.PermissionLevel = lobbyProperties.LobbyPermissionLevel;
            createLobbyOptions.PresenceEnabled = lobbyProperties.PresenceEnabled;
            createLobbyOptions.AllowInvites = lobbyProperties.AllowInvites;
            createLobbyOptions.BucketId = lobbyProperties.BucketId;

            if (lobbyProperties.DisableHostMigration != null)
            {
                createLobbyOptions.DisableHostMigration = (bool)lobbyProperties.DisableHostMigration;
            }
            // Voice Chat
            if(lobbyProperties.RTCRoomEnabled)
            {
                if (customLocalRTCOptions != null)
                {
                    createLobbyOptions.LocalRTCOptions = customLocalRTCOptions;
                }

                createLobbyOptions.EnableRTCRoom = true;      
            }
            else
            {
                createLobbyOptions.EnableRTCRoom = false;
            }

            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(ref createLobbyOptions, CreateLobbyCompleted, OnCreateLobbyCompleted);

            // Save lobby data for modification
            CurrentLobby = lobbyProperties;
            CurrentLobby._BeingCreated = true;
            CurrentLobby.LobbyOwner = currentUserProductId;
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_UpdateLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_UpdateLobby/index.html)
        /// </summary>
        /// <param name="lobbyUpdates"><b>Lobby</b> properties used to update current lobby</param>
        /// <param name="ModififyLobbyCompleted">Callback when modify lobby is completed</param>
        public void ModifyLobby(Lobby lobbyUpdates, OnLobbyCallback ModififyLobbyCompleted)
        {
            // Validate current lobby
            if (!CurrentLobby.IsValid())
            {
                Debug.LogError("Lobbies (ModifyLobby): Current Lobby {0} is invalid!");
                ModififyLobbyCompleted?.Invoke(Result.InvalidState);
                return;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (ModifyLobby): Current player is invalid!");
                ModififyLobbyCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            if (!CurrentLobby.IsOwner(currentProductUserId))
            {
                Debug.LogError("Lobbies (ModifyLobby): Current player is not lobby owner!");
                ModififyLobbyCompleted?.Invoke(Result.LobbyNotOwner);
                return;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions();
            options.LobbyId = CurrentLobby.Id;
            options.LocalUserId = currentProductUserId;

            // Get LobbyModification object handle
            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(ref options, out LobbyModification outLobbyModificationHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not create lobby modification. Error code: {0}", result);
                ModififyLobbyCompleted?.Invoke(result);
                return;
            }

            // Bucket Id
            if(!string.Equals(lobbyUpdates.BucketId, CurrentLobby.BucketId))
            {
                var lobbyModificationSetBucketIdOptions = new LobbyModificationSetBucketIdOptions() { BucketId = lobbyUpdates.BucketId };
                result = outLobbyModificationHandle.SetBucketId(ref lobbyModificationSetBucketIdOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set bucket id. Error code: {0}", result);
                    ModififyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Max Players
            if (lobbyUpdates.MaxNumLobbyMembers > 0)
            {
                var lobbyModificationSetMaxMembersOptions = new LobbyModificationSetMaxMembersOptions() { MaxMembers = lobbyUpdates.MaxNumLobbyMembers };
                result = outLobbyModificationHandle.SetMaxMembers(ref lobbyModificationSetMaxMembersOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set max players. Error code: {0}", result);
                    ModififyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Add Lobby Attributes
            for (int attributeIndex = 0; attributeIndex < lobbyUpdates.Attributes.Count; attributeIndex++)
            {
                AttributeData attributeData = lobbyUpdates.Attributes[attributeIndex].AsAttribute;

                LobbyModificationAddAttributeOptions addAttributeOptions = new LobbyModificationAddAttributeOptions();
                addAttributeOptions.Attribute = attributeData;
                addAttributeOptions.Visibility = lobbyUpdates.Attributes[attributeIndex].Visibility;

                //if (attributeData.Key == null)
                //{
                //    Debug.LogWarning("Lobbies (ModifyLobby): Attributes with null key! Do not add!");
                //    continue;
                //}

                //if (attributeData.Value == null)
                //{
                //    Debug.LogWarningFormat("Lobbies (ModifyLobby): Attributes with key '{0}' has null value! Do not add!", attributeData.Key);
                //    continue;
                //}

                result = outLobbyModificationHandle.AddAttribute(ref addAttributeOptions);
                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not add attribute. Error code: {0}", result);
                    ModififyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Permission
            if (lobbyUpdates.LobbyPermissionLevel != CurrentLobby.LobbyPermissionLevel)
            {
                var lobbyModificationSetPermissionLevelOptions = new LobbyModificationSetPermissionLevelOptions() { PermissionLevel = lobbyUpdates.LobbyPermissionLevel };
                result = outLobbyModificationHandle.SetPermissionLevel(ref lobbyModificationSetPermissionLevelOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set permission level. Error code: {0}", result);
                    ModififyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Allow Invites
            if (lobbyUpdates.AllowInvites != CurrentLobby.AllowInvites)
            {
                var lobbyModificationSetInvitesAllowedOptions = new LobbyModificationSetInvitesAllowedOptions() { InvitesAllowed = lobbyUpdates.AllowInvites };
                result = outLobbyModificationHandle.SetInvitesAllowed(ref lobbyModificationSetInvitesAllowedOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set allow invites. Error code: {0}", result);
                    ModififyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            //Trigger lobby update
            var updateLobbyOptions = new UpdateLobbyOptions() { LobbyModificationHandle = outLobbyModificationHandle };
            EOSManager.Instance.GetEOSLobbyInterface().UpdateLobby(ref updateLobbyOptions, ModififyLobbyCompleted, OnUpdateLobbyCallBack);
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_LeaveLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_LeaveLobby/index.html)
        /// </summary>
        /// <param name="LeaveLobbyCompleted">Callback when leave lobby is completed</param>
        public void LeaveLobby(OnLobbyCallback LeaveLobbyCompleted)
        {
            if (CurrentLobby == null || string.IsNullOrEmpty(CurrentLobby.Id) || !EOSManager.Instance.GetProductUserId().IsValid())
            {
                Debug.LogWarning("Lobbies (LeaveLobby): Not currently in a lobby.");
                LeaveLobbyCompleted?.Invoke(Result.NotFound);
                return;
            }

            UnsubscribeFromRTCEvents();

            LeaveLobbyOptions options = new LeaveLobbyOptions();
            options.LobbyId = CurrentLobby.Id;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            Debug.LogFormat("Lobbies (LeaveLobby): Attempting to leave lobby: Id='{0}', LocalUserId='{1}'", options.LobbyId, options.LocalUserId);

            EOSManager.Instance.GetEOSLobbyInterface().LeaveLobby(ref options, LeaveLobbyCompleted, OnLeaveLobbyCompleted);
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_SendInvite](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_SendInvite/index.html)
        /// </summary>
        /// <param name="targetUserId">Target <c>ProductUserId</c> to send invite</param>
        public void SendInvite(ProductUserId targetUserId)
        {
            if (!targetUserId.IsValid())
            {
                Debug.LogWarning("Lobbies (SendInvite): targetUserId parameter is not valid.");
                return;
            }

            if (!CurrentLobby.IsValid())
            {
                Debug.LogWarning("Lobbies (SendInvite): CurrentLobby is not valid.");
                return;
            }

            SendInviteOptions options = new SendInviteOptions()
            {
                LobbyId = CurrentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetUserId = targetUserId
            };

            EOSManager.Instance.GetEOSLobbyInterface().SendInvite(ref options, null, OnSendInviteCompleted);
        }

        /// <summary>
        /// Wrapper for calling [EOS_LobbyModification_AddMemberAttribute](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_LobbyModification_AddMemberAttribute/index.html)
        /// </summary>
        /// <param name="memberAttribute"><c>LobbyAttribute</c> to be added to the current lobby</param>
        public void SetMemberAttribute(LobbyAttribute memberAttribute)
        {
            if(!CurrentLobby.IsValid())
            {
                Debug.LogError("Lobbies (SetMemberAttribute): CurrentLobby is not valid.");
                return;
            }

            // Modify Lobby

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions()
            {
                LobbyId = CurrentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(ref options, out LobbyModification lobbyModificationHandle);

            if(result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SetMemberAttribute): Could not create lobby modification: Error code: {0}", result);
                return;
            }

            // Update member attribute

            AttributeData attributeData = memberAttribute.AsAttribute;

            LobbyModificationAddMemberAttributeOptions attrOptions = new LobbyModificationAddMemberAttributeOptions()
            {
                Attribute = attributeData,
                Visibility = LobbyAttributeVisibility.Public
            };

            result = lobbyModificationHandle.AddMemberAttribute(ref attrOptions);

            if(result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SetMemberAttribute): Could not add member attribute: Error code: {0}", result);
                return;
            }

            // Trigger lobby update

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions()
            {
                LobbyModificationHandle = lobbyModificationHandle
            };

            EOSManager.Instance.GetEOSLobbyInterface().UpdateLobby(ref updateOptions, null, OnUpdateLobbyCallBack);
        }

        private void OnSendInviteCompleted(ref SendInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnSendInviteCompleted): SendInviteCallbackInfo data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnSendInviteCompleted): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnSendInviteCompleted): invite sent.");
        }

        private void OnCreateLobbyCompleted(ref CreateLobbyCallbackInfo createLobbyCallbackInfo)
        {
            OnLobbyCallback LobbyCreatedCallback = createLobbyCallbackInfo.ClientData as OnLobbyCallback;

            //if (createLobbyCallbackInfo == null)
            //{
            //    Debug.Log("Lobbies (OnCreateLobbyCompleted): createLobbyCallbackInfo is null");
            //    LobbyCreatedCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (createLobbyCallbackInfo.ResultCode != Result.Success)
            {
                Debug.LogFormat("Lobbies (OnCreateLobbyCompleted): error code: {0}", createLobbyCallbackInfo.ResultCode);
                LobbyCreatedCallback?.Invoke(createLobbyCallbackInfo.ResultCode);
            }
            else
            {
                Debug.Log("Lobbies (OnCreateLobbyCompleted): Lobby created.");

                // OnLobbyCreated
                if (!string.IsNullOrEmpty(createLobbyCallbackInfo.LobbyId) && CurrentLobby._BeingCreated)
                {
                    CurrentLobby.Id = createLobbyCallbackInfo.LobbyId;
                    ModifyLobby(CurrentLobby, null);

                    if(CurrentLobby.RTCRoomEnabled)
                    {
                        SubscribeToRTCEvents();
                    }
                }

                _Dirty = true;

                LobbyCreatedCallback?.Invoke(Result.Success);

                OnCurrentLobbyChanged(LobbyChangeType.Create);
            }
        }

        private void OnCurrentLobbyChanged(LobbyChangeType lobbyChangedEvent)
        {
            if (CurrentLobby.IsValid())
            {
                AddLocalUserAttributes();
            }

            OnLobbyChanged(new LobbyChangeEventArgs(CurrentLobby?.Id, lobbyChangedEvent));
        }

        private void OnUpdateLobbyCallBack(ref UpdateLobbyCallbackInfo data)
        {
            OnLobbyCallback LobbyModifyCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnUpdateLobbyCallBack): UpdateLobbyCallbackInfo data is null");
            //    LobbyModifyCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnUpdateLobbyCallBack): error code: {0}", data.ResultCode);
                LobbyModifyCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnUpdateLobbyCallBack): lobby updated.");

            OnLobbyUpdated(data.LobbyId, LobbyModifyCallback);
        }

        private void OnLobbyUpdated(string lobbyId, OnLobbyCallback LobbyUpdateCompleted)
        {
            // Update Lobby
            if (!string.IsNullOrEmpty(lobbyId) && CurrentLobby.Id == lobbyId)
            {
                CurrentLobby.InitFromLobbyHandle(lobbyId);

                LobbyUpdateCompleted?.Invoke(Result.Success);

                foreach (var callback in LobbyUpdateCallbacks)
                {
                    callback?.Invoke();
                }
            }
        }

        private void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo data)
        {
            // Callback for LobbyUpdateNotification

            //if (data != null)
            //{
                Debug.Log("Lobbies (OnLobbyUpdateReceived): lobby update received.");
                OnLobbyUpdated(data.LobbyId, null);
            //}
            //else
            //{
            //    Debug.LogError("Lobbies (OnLobbyUpdateReceived): EOS_Lobby_LobbyUpdateReceivedCallbackInfo is null");
            //}
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_DestroyLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_DestroyLobby/index.html)
        /// </summary>
        /// <param name="DestroyCurrentLobbyCompleted">Callback when destroy lobby is completed</param>
        public void DestroyCurrentLobby(ref OnLobbyCallback DestroyCurrentLobbyCompleted)
        {
            if (!CurrentLobby.IsValid())
            {
                Debug.LogError("Lobbies (DestroyCurrentLobby): CurrentLobby is invalid!");
                DestroyCurrentLobbyCompleted?.Invoke(Result.InvalidState);
                return;
            }

            UnsubscribeFromRTCEvents();

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (DestroyCurrentLobby): Current player is invalid!");
                DestroyCurrentLobbyCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            if (!CurrentLobby.IsOwner(currentProductUserId))
            {
                Debug.LogError("Lobbies (DestroyCurrentLobby): Current player is now lobby owner!");
                DestroyCurrentLobbyCompleted?.Invoke(Result.LobbyNotOwner);
                return;
            }

            DestroyLobbyOptions options = new DestroyLobbyOptions();
            options.LocalUserId = currentProductUserId;
            options.LobbyId = CurrentLobby.Id;

            EOSManager.Instance.GetEOSLobbyInterface().DestroyLobby(ref options, DestroyCurrentLobbyCompleted, OnDestroyLobbyCompleted);

            // Clear current lobby
            CurrentLobby.Clear();
        }

        private void OnDestroyLobbyCompleted(ref DestroyLobbyCallbackInfo data)
        {
            OnLobbyCallback DestroyLobbyCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnDestroyLobbyCompleted): DestroyLobbyCallbackInfo data is null");
            //    DestroyLobbyCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnDestroyLobbyCompleted): error code: {0}", data.ResultCode);
                DestroyLobbyCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnDestroyLobbyCompleted): Lobby destroyed.");

            //_LobbyLeaveInProgress = false;
            if (ActiveJoin.IsValid())
            {
                LobbyJoinRequest lobbyToJoin = ActiveJoin;
                ActiveJoin.Clear();

            }

            DestroyLobbyCallback?.Invoke(Result.Success);
        }

        public void EnablePressToTalk(ProductUserId targetUserId, OnLobbyCallback EnablePressToTalkCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            foreach (LobbyMember lobbyMember in CurrentLobby.Members)
            {
                // Find the correct lobby member
                if (lobbyMember.ProductId != targetUserId)
                {
                    continue;
                }

                lobbyMember.RTCState.PressToTalkEnabled = !lobbyMember.RTCState.PressToTalkEnabled;

                UpdateSendingVolumeOptions sendVolumeOptions = new UpdateSendingVolumeOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = CurrentLobby.RTCRoomName,
                    Volume = lobbyMember.RTCState.PressToTalkEnabled ? 0 : 50
                };

                Debug.LogFormat("Press To Talk Enabled : {0} : Current self Audio output volume is {1}", lobbyMember.RTCState.PressToTalkEnabled, sendVolumeOptions.Volume);

                rtcAudioHandle.UpdateSendingVolume(ref sendVolumeOptions, EnablePressToTalkCompleted, null);
            }
        }
        // Member Events
        public void PressToTalk(KeyCode PTTKeyCode, OnLobbyCallback TogglePressToTalkCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();
            ProductUserId targetUserId = EOSManager.Instance.GetProductUserId();

            foreach (LobbyMember lobbyMember in CurrentLobby.Members)
            {
                // Find the correct lobby member
                if (lobbyMember.ProductId != targetUserId)
                {
                    continue;
                }

                UpdateSendingVolumeOptions sendVolumeOptions = new UpdateSendingVolumeOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = CurrentLobby.RTCRoomName,
                    Volume = Input.GetKey(PTTKeyCode) ? 50 : 0
                };

                Debug.LogFormat("Lobbies (TogglePressToTalk): Setting self audio output volume to {0}", sendVolumeOptions.Volume);

                rtcAudioHandle.UpdateSendingVolume(ref sendVolumeOptions, TogglePressToTalkCompleted, null);
            }
        }

        /// <summary>
        /// Wrapper for calling [EOS_RTCAudio_UpdateReceiving](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/NoInterface/EOS_RTCAudio_UpdateReceiving/index.html)
        /// This attempts to locate a user with id <paramref name="targetUserId"/> inside the <see cref="CurrentLobby"/>.
        /// If CurrentLobby has populated its <see cref="Lobby.Members"/> list,
        /// the local user's current mute setting is reversed, and then applied.
        /// If the Members list isn't available, this action will attempt to mute the target.
        /// If the local user has another user in the Lobby muted, the local user won't hear the muted user, but other members may still be able to.
        /// If the local user mutes themself, other users won't be able to hear the local user.
        /// </summary>
        /// <param name="targetUserId">Target <c>ProductUserId</c> to mute or unmute</param>
        /// <param name="ToggleMuteCompleted">Callback when toggle mute is completed</param>
        public void ToggleMute(ProductUserId targetUserId, OnLobbyCallback ToggleMuteCompleted)
        {
            // If the user can be found in CurrentLobby.Members, set this value to the opposite of the user's current mute value
            // Otherwise, always attempt to mute
            bool shouldUserBecomeMuted = false;

            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            LobbyMember lobbyMember = GetCurrentLobby()?.Members?.Find(x => x.ProductId == targetUserId);
            if (lobbyMember != null)
            {
                // If the target is muted, "IsLocalMuted" is true,
                // since this is a toggle, we want to take the opposite of that
                shouldUserBecomeMuted = !lobbyMember.RTCState.IsLocalMuted;
            }

            SetMemberMuteStatus(shouldUserBecomeMuted, targetUserId, ToggleMuteCompleted);
        }

        /// <summary>
        /// Set the local mute status of a member in the lobby.
        /// If the local user has another user in the Lobby muted, the local user won't hear the muted user, but other members may still be able to.
        /// If the local user mutes themself, other users won't be able to hear the local user.
        /// </summary>
        /// <param name="shouldUserBeMuted">
        /// If true, audio "becomes disabled", muting the target.
        /// If false, audio "becomes enabled", unmuting the target.
        /// </param>
        /// </param>
        /// <param name="targetProductUserId">The Product User ID of the target member to mute</param>
        /// <param name="MuteMemberCompleted">Callback when MuteMember is completed</param>
        public void SetMemberMuteStatus(bool shouldUserBeMuted, ProductUserId targetProductUserId, OnLobbyCallback MuteMemberCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            LobbyMember lobbyMember = GetCurrentLobby().Members.Find(x => x.ProductId == targetProductUserId);
            if (lobbyMember != null)
            {
                // Do not allow multiple local mute toggles at the same time
                if (lobbyMember.RTCState.MuteActionInProgress)
                {
                    Debug.LogWarningFormat("Lobbies (MuteMember): 'MuteActionInProgress' for productUserId {0}.", targetProductUserId);
                    MuteMemberCompleted?.Invoke(Result.RequestInProgress);
                    return;
                }

                // Set mute action as in progress
                lobbyMember.RTCState.MuteActionInProgress = true;
            }

            // Check if muting ourselves vs other member
            if (EOSManager.Instance.GetProductUserId() == targetProductUserId)
            {
                // Toggle our mute status
                SetLocalMemberMute(shouldUserBeMuted, MuteMemberCompleted);
            }
            else
            {
                // Toggle mute for remote member (this is a local-only action and does not block the other user from receiving your audio stream)

                UpdateReceivingOptions recevingOptions = new UpdateReceivingOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = GetCurrentLobby().RTCRoomName,
                    ParticipantId = targetProductUserId,
                    AudioEnabled = shouldUserBeMuted
                };

                Debug.LogFormat("Lobbies (MuteMember): {0} remote player {1}", recevingOptions.AudioEnabled ? "Unmuting" : "Muting", targetProductUserId);

                rtcAudioHandle.UpdateReceiving(ref recevingOptions, MuteMemberCompleted, OnRTCRoomUpdateReceivingCompleted);
            }
        }

        /// <summary>
        /// Set the Mute Status of the Local Member.
        /// The local user sets their mute status to <paramref name="micOn"/>,
        /// either becoming muted or unmuting themself.
        /// While muted, other users in the lobby will not be able to hear the local user.
        /// </summary>
        /// <param name="shouldSelfBeMuted">
        /// If true, the local user mutes themself, and no longer sends audio.
        /// If false, the local user unmutes themself.
        /// </param>
        /// <param name="MuteLocalMemberCompleted">Callback when MuteLocalMember is completed</param>
        public void SetLocalMemberMute(bool shouldSelfBeMuted, OnLobbyCallback MuteLocalMemberCompleted)
        {
            UpdateSendingOptions sendOptions = new UpdateSendingOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = GetCurrentLobby().RTCRoomName,
                AudioStatus = shouldSelfBeMuted ? RTCAudioStatus.Disabled : RTCAudioStatus.Enabled
            };

            Debug.LogFormat("Lobbies (MuteLocalMember): Setting self audio output status to {0}", sendOptions.AudioStatus == RTCAudioStatus.Enabled ? "Unmuted" : "Muted");

            EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().UpdateSending(ref sendOptions, MuteLocalMemberCompleted, OnRTCRoomUpdateSendingCompleted);
        }

        /// <summary>
        /// Sets the Deafen Status of the Local Member.
        /// If <paramref name="shouldBeDefeaned"/> is true, then the user won't be able to hear other users.
        /// </summary>
        /// <param name="shouldBeDefeaned">
        /// If true, the local user should become defeaned.
        /// Otherwise, undeafen the user.
        /// </param>
        /// <param name="DeafenCompleted">Callback when DeafenLocalMember is completed</param>
        public void SetLocalMemberDeafen(bool shouldBeDefeaned, OnLobbyCallback DeafenCompleted)
        {
            UpdateReceivingOptions recevingOptions = new UpdateReceivingOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = GetCurrentLobby().RTCRoomName,
                AudioEnabled = !shouldBeDefeaned
            };
            Debug.LogFormat("Lobbies (DeafenLocalMember): {0}", recevingOptions.AudioEnabled ? "Hearing" : "Deafened");

            EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().UpdateReceiving(ref recevingOptions, DeafenCompleted, OnRTCRoomUpdateReceivingCompleted);
        }

        private void OnRTCRoomUpdateSendingCompleted(ref UpdateSendingCallbackInfo data)
        {
            OnLobbyCallback ToggleMuteCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomUpdateSendingCompleted): UpdateSendingCallbackInfo data is null");
            //    ToggleMuteCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): error code: {0}", data.ResultCode);
                ToggleMuteCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Updated sending status successfully. Room={0}, AudioStatus={1}", data.RoomName, data.AudioStatus);

            // Ensure this update is for our room
            if (!CurrentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Incorrect Room! CurrentLobby.RTCRoomName={0} != data.RoomName", CurrentLobby.RTCRoomName, data.RoomName);
                return;
            }

            // Ensure this update is for us
            if(EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Incorrect LocalUserId! LocalProductId={0} != data.LocalUserId", EOSManager.Instance.GetProductUserId(), data.LocalUserId);
                return;
            }

            // Update our mute status
            foreach(LobbyMember lobbyMember in CurrentLobby.Members)
            {
                // Find ourselves
                if(lobbyMember.ProductId != data.LocalUserId)
                {
                    continue;
                }

                lobbyMember.RTCState.IsAudioOutputDisabled = data.AudioStatus == RTCAudioStatus.Disabled;
                lobbyMember.RTCState.MuteActionInProgress = false;

                Debug.LogFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Cache updated for '{0}'", lobbyMember.ProductId);

                _Dirty = true;
                break;
            }

            ToggleMuteCallback?.Invoke(data.ResultCode);
        }

        private void OnRTCRoomUpdateReceivingCompleted(ref UpdateReceivingCallbackInfo data)
        {
            OnLobbyCallback ToggleMuteCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomUpdateReceivingCompleted): UpdateSendingCallbackInfo data is null");
            //    ToggleMuteCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): error code: {0}", data.ResultCode);
                ToggleMuteCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Updated receiving status successfully. LocalUserId={0} Room={1}, IsMuted={2}", data.LocalUserId, data.RoomName, data.AudioEnabled == false);

            // Ensure this update is for our room
            if (!CurrentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Incorrect Room! CurrentLobby.RTCRoomName={0} != data.RoomName", CurrentLobby.RTCRoomName, data.RoomName);
                return;
            }

            // Update should be for remote user
            if (EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Incorrect call for local member.");
                return;
            }

            // If the participantId is null, then this must be referencing the local user
            if (data.ParticipantId == null)
            {
                LobbyMember selfMember = CurrentLobby.Members.Find(x => x.ProductId == EOSManager.Instance.GetProductUserId());
                if (selfMember != null)
                {
                    selfMember.RTCState.MuteActionInProgress = false;
                }

                CurrentLobby.IsLocalUserDeafened = !data.AudioEnabled;
                _Dirty = true;

                Debug.LogFormat($"Lobbies (OnRTCRoomUpdateReceivingCompleted): Self-deafen cache updated for '{EOSManager.Instance.GetProductUserId()}' (now {CurrentLobby.IsLocalUserDeafened})");
                return;
            }

            // This must be about another user, find that user and set the status
            foreach (LobbyMember lobbyMember in CurrentLobby.Members)
            { 
                if(lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                lobbyMember.RTCState.MuteActionInProgress = false;
                lobbyMember.RTCState.IsLocalMuted = data.AudioEnabled == false;

                Debug.LogFormat($"Lobbies (OnRTCRoomUpdateReceivingCompleted): Mute cache updated for '{lobbyMember.ProductId}'. (now {lobbyMember.RTCState.IsLocalMuted})");

                _Dirty = true;
                break;
            }
        }

        /// <summary>
        /// Blocks the Local Lobby Member from receiving/sending audio from/to a Target Lobby Member. Only affects this Local Member. Does not affect other Lobby Members.
        /// </summary>
        /// <param name="targetUserId">The Target Member's ProductUserID</param>
        /// <param name="status">To block or to unblock? True to Block, False to Unblock</param>
        /// <param name="blockRTCParticipantComplete">Callback when BlockTargetRTCParticipant is completed</param>
        public void UpdateBlockStatusForRTCParticipant(ProductUserId targetUserId, bool status, OnLobbyCallback blockRTCParticipantComplete)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            BlockParticipantOptions blockParticipantOptions = new BlockParticipantOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName,
                ParticipantId = targetUserId,
                Blocked = status
            };

            rtcHandle.BlockParticipant(ref blockParticipantOptions, blockRTCParticipantComplete, OnRTCBlockParticipantCompleted);
        }

        private void OnRTCBlockParticipantCompleted(ref BlockParticipantCallbackInfo data)
        {
            OnLobbyCallback BlockParticipantCallback = data.ClientData as OnLobbyCallback;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCBlockParticipantCompleted): error code: {0}", data.ResultCode);
                BlockParticipantCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Lobbies (OnRTCBlockParticipantCompleted): Blocked Participant successfully. Participant={0}, Room={1}, Blocked={2}", data.ParticipantId, data.RoomName, data.Blocked);

            // Ensure this update is for our room
            if (!CurrentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogErrorFormat("Lobbies (OnRTCBlockParticipantCompleted): Incorrect Room! CurrentLobby.RTCRoomName={0} != data.RoomName", CurrentLobby.RTCRoomName, data.RoomName);
                return;
            }

            // Ensure this update is for us
            if (EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCBlockParticipantCompleted): Incorrect LocalUserId! LocalProductId={0} != data.LocalUserId", EOSManager.Instance.GetProductUserId(), data.LocalUserId);
                return;
            }

            // Update our mute status
            foreach (LobbyMember lobbyMember in CurrentLobby.Members)
            {
                // Find the ParticipantId
                if (lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                lobbyMember.RTCState.IsBlocked = data.Blocked;

                Debug.LogFormat("Lobbies (OnRTCBlockParticipantCompleted): Cache updated for '{0}'", data.ParticipantId);

                _Dirty = true;
                break;
            }

            BlockParticipantCallback?.Invoke(data.ResultCode);
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_KickMember](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_KickMember/index.html)
        /// </summary>
        /// <param name="productUserId">Target <c>ProductUserId</c> to kick from current lobby</param>
        /// <param name="KickMemberCompleted">Callback when kick member is completed</param>
        public void KickMember(ProductUserId productUserId, OnLobbyCallback KickMemberCompleted)
        {
            if (!productUserId.IsValid())
            {
                Debug.LogError("Lobbies (KickMember): productUserId is invalid!");
                KickMemberCompleted?.Invoke(Result.InvalidState);
                return;
            }

            ProductUserId currentUserId = EOSManager.Instance.GetProductUserId();
            if (!currentUserId.IsValid())
            {
                Debug.LogError("Lobbies (KickMember): Current player is invalid!");
                KickMemberCompleted?.Invoke(Result.InvalidState);
                return;
            }

            KickMemberOptions kickOptions = new KickMemberOptions();
            kickOptions.TargetUserId = productUserId;
            kickOptions.LobbyId = CurrentLobby.Id;
            kickOptions.LocalUserId = currentUserId;

            EOSManager.Instance.GetEOSLobbyInterface().KickMember(ref kickOptions, KickMemberCompleted, OnKickMemberCompleted);
        }

        private void OnKickMemberCompleted(ref KickMemberCallbackInfo data)
        {
            OnLobbyCallback KickMemberCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnKickMemberCompleted): KickMemberCallbackInfo data is null");
            //    KickMemberCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnKickMemberFinished): error code: {0}", data.ResultCode);
                KickMemberCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnKickMemberFinished): Member kicked.");
            KickMemberCallback?.Invoke(Result.Success);
        }

        private void OnKickedFromLobby(string lobbyId)
        {
            Debug.LogFormat("Lobbies (OnKickedFromLobby):  Kicked from lobby: {0}", lobbyId);
            if (CurrentLobby.IsValid() && CurrentLobby.Id.Equals(lobbyId, StringComparison.OrdinalIgnoreCase))
            {
                CurrentLobby.Clear();
                _Dirty = true;

                OnCurrentLobbyChanged(LobbyChangeType.Kicked);
            }
        }


        /// <summary>
        /// Wrapper for calling [EOS_Lobby_PromoteMember](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_PromoteMember/index.html)
        /// </summary>
        /// <param name="productUserId">Target <c>ProductUserId</c> to promote</param>
        /// <param name="PromoteMemberCompleted">Callback when promote member is completed</param>
        public void PromoteMember(ProductUserId productUserId, OnLobbyCallback PromoteMemberCompleted)
        {
            if (!productUserId.IsValid())
            {
                Debug.LogError("Lobbies (PromoteMember): productUserId is invalid!");
                PromoteMemberCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            ProductUserId currentUserId = EOSManager.Instance.GetProductUserId();
            if (!currentUserId.IsValid())
            {
                Debug.LogError("Lobbies (PromoteMember): Current player is invalid!");
                PromoteMemberCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            PromoteMemberOptions promoteOptions = new PromoteMemberOptions();
            promoteOptions.TargetUserId = productUserId;
            promoteOptions.LocalUserId = currentUserId;
            promoteOptions.LobbyId = CurrentLobby.Id;

            EOSManager.Instance.GetEOSLobbyInterface().PromoteMember(ref promoteOptions, PromoteMemberCompleted, OnPromoteMemberCompleted);
        }

        private void OnPromoteMemberCompleted(ref PromoteMemberCallbackInfo data)
        {
            OnLobbyCallback PromoteMemberCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnPromoteMemberCompleted): PromoteMemberCallbackInfo data is null");
            //    PromoteMemberCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnPromoteMemberFinished): error code: {0}", data.ResultCode);
                PromoteMemberCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnPromoteMemberFinished): Member promoted.");

            PromoteMemberCallback?.Invoke(Result.Success);
        }

        private void OnLeaveLobbyRequested(ref LeaveLobbyRequestedCallbackInfo data)
        {
            Debug.Log("Lobbies (OnLeaveLobbyRequested): Leave Lobby Requested via Overlay.");
            LeaveLobby(null);
        }
        private void OnMemberStatusReceived(ref LobbyMemberStatusReceivedCallbackInfo data)
        {
            // Callback for LobbyMemberStatusNotification

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnMemberStatusReceived): LobbyMemberStatusReceivedCallbackInfo data is null");
            //    return;
            //}

            Debug.Log("Lobbies (OnMemberStatusReceived): Member status update received.");

            if (!data.TargetUserId.IsValid())
            {
                Debug.Log("Lobbies  (OnMemberStatusReceived): Current player is invalid!");

                //Simply update the whole lobby
                OnLobbyUpdated(data.LobbyId, null);
                return;
            }

            bool updateLobby = true;

            // Current player updates need special handing
            ProductUserId currentPlayer = EOSManager.Instance.GetProductUserId();

            if (data.TargetUserId == currentPlayer)
            {
                if (data.CurrentStatus == LobbyMemberStatus.Closed ||
                    data.CurrentStatus == LobbyMemberStatus.Kicked ||
                    data.CurrentStatus == LobbyMemberStatus.Disconnected)
                {
                    OnKickedFromLobby(data.LobbyId);
                    updateLobby = false;
                }
            }

            if (updateLobby)
            {
                // Update lobby
                OnLobbyUpdated(data.LobbyId, null);
            }
        }

        private void OnMemberUpdateReceived(ref LobbyMemberUpdateReceivedCallbackInfo data)
        {
            // Callback for LobbyMemberUpdateNotification

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnMemberUpdateReceived): LobbyMemberUpdateReceivedCallbackInfo data is null");
            //    return;
            //}

            Debug.Log("Lobbies (OnMemberUpdateReceived): Member update received.");
            OnLobbyUpdated(data.LobbyId, null);

            foreach (var callback in MemberUpdateCallbacks)
            {
                callback?.Invoke(data.LobbyId, data.TargetUserId);
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLobbyMemberUpdateReceived](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_AddNotifyLobbyMemberUpdateReceived/index.html)
        /// The callback will only run if a listener is subscribed, which is done in <see cref="SubscribeToLobbyUpdates"/>.
        /// </summary>
        /// <param name="Callback">Callback to receive notification when lobby member update is received</param>
        public void AddNotifyMemberUpdateReceived(OnMemberUpdateCallback Callback)
        {
            MemberUpdateCallbacks.Add(Callback);
        }

        public void RemoveNotifyMemberUpdate(OnMemberUpdateCallback Callback)
        {
            MemberUpdateCallbacks.Remove(Callback);
        }

        /// <summary>
        /// Subscribe to event callback for when the current lobby data has been updated
        /// The callback will only run if a listener is subscribed, which is done in <see cref="SubscribeToLobbyUpdates"/>.
        /// </summary>
        /// <param name="Callback">Callback to receive notification when lobby data is updated</param>
        public void AddNotifyLobbyUpdate(Action Callback)
        {
            LobbyUpdateCallbacks.Add(Callback);
        }

        public void RemoveNotifyLobbyUpdate(Action Callback)
        {
            LobbyUpdateCallbacks.Remove(Callback);
        }
        // Search Events

        /// <summary>
        /// Wrapper for calling [EOS_LobbySearch_Find](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_LobbySearch_Find/index.html)
        /// </summary>
        /// <param name="lobbyId"><c>string</c> of lobbyId to search</param>
        /// <param name="SearchCompleted">Callback when search is completed</param>
        public void SearchByLobbyId(string lobbyId, OnLobbySearchCallback SearchCompleted)
        {
            Debug.LogFormat("Lobbies (SearchByLobbyId): lobbyId='{0}'", lobbyId);

            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogWarning("Lobbies (SearchByLobbyId): lobbyId is null or empty!");
                SearchCompleted?.Invoke(Result.InvalidParameters);
                return;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (SearchByLobbyId): Current player is invalid!");
                SearchCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }

            var createLobbySearchOptions = new CreateLobbySearchOptions() { MaxResults = 10 };
            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref createLobbySearchOptions, out LobbySearch outLobbySearchHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByLobbyId): could not create SearchByLobbyId. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            CurrentSearch = outLobbySearchHandle;

            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions();
            setLobbyOptions.LobbyId = lobbyId;

            result = outLobbySearchHandle.SetLobbyId(ref setLobbyOptions);
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByLobbyId): failed to update SearchByLobbyId with lobby id. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            LobbySearchCallback = SearchCompleted;
            var lobbySearchFindOptions = new LobbySearchFindOptions() { LocalUserId = currentProductUserId };
            outLobbySearchHandle.Find(ref lobbySearchFindOptions, null, OnLobbySearchCompleted);
        }

        /// <summary>
        /// Wrapper for calling [EOS_LobbySearch_Find](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_LobbySearch_Find/index.html)
        /// </summary>
        /// <param name="attributeKey"><c>string</c> of attributeKey to search</param>
        /// <param name="attributeValue"><c>string</c> of attributeValue to search</param>
        /// <param name="SearchCompleted">Callback when search is completed</param>
        public void SearchByAttribute(string attributeKey, string attributeValue, OnLobbySearchCallback SearchCompleted)
        {
            Debug.LogFormat("Lobbies (SearchByAttribute): searchString='{0}'", attributeKey);

            if (string.IsNullOrEmpty(attributeKey))
            {
                Debug.LogError("Lobbies (SearchByAttribute): searchString is null or empty!");
                SearchCompleted?.Invoke(Result.InvalidParameters);
                return;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (SearchByAttribute): Current player is invalid!");
                SearchCompleted?.Invoke(Result.InvalidProductUserID);
                return;
            }
            var createLobbySearchOptions = new CreateLobbySearchOptions() { MaxResults = 10}; 
            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref createLobbySearchOptions, out LobbySearch outLobbySearchHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByAttribute): could not create SearchByAttribute. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            CurrentSearch = outLobbySearchHandle;

            LobbySearchSetParameterOptions paramOptions = new LobbySearchSetParameterOptions();
            paramOptions.ComparisonOp = ComparisonOp.Equal;

            // Turn SearchString into AttributeData
            AttributeData attrData = new AttributeData();
            attrData.Key = attributeKey.Trim();
            attrData.Value = new AttributeDataValue();
            attrData.Value = attributeValue.Trim();
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(ref paramOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByAttribute): failed to update SearchByAttribute with parameter. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            LobbySearchCallback = SearchCompleted;
            var lobbySearchFindOptions = new LobbySearchFindOptions() { LocalUserId = currentProductUserId };
            outLobbySearchHandle.Find(ref lobbySearchFindOptions, null, OnLobbySearchCompleted);
        }

        private void OnLobbySearchCompleted(ref LobbySearchFindCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnLobbySearchCompleted): LobbySearchFindCallbackInfo data is null");
            //    LobbySearchCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                if (data.ResultCode == Result.NotFound)
                {
                    // It's not an error if there's no results found when searching
                    Debug.Log("Lobbies (OnLobbySearchCompleted): No results found.");
                }
                else
                {
                    Debug.LogErrorFormat("Lobbies (OnLobbySearchCompleted): error code: {0}", data.ResultCode);
                }

                LobbySearchCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnLobbySearchCompleted): Search finished.");

            // Process Search Results
            var lobbySearchGetSearchResultCountOptions = new LobbySearchGetSearchResultCountOptions(); 
            uint searchResultCount = CurrentSearch.GetSearchResultCount(ref lobbySearchGetSearchResultCountOptions);

            Debug.LogFormat("Lobbies (OnLobbySearchCompleted): searchResultCount = {0}", searchResultCount);

            SearchResults.Clear();

            LobbySearchCopySearchResultByIndexOptions indexOptions = new LobbySearchCopySearchResultByIndexOptions();

            for (uint i = 0; i < searchResultCount; i++)
            {
                Lobby lobbyObj = new Lobby();

                indexOptions.LobbyIndex = i;

                Result result = CurrentSearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails outLobbyDetailsHandle);

                if (result == Result.Success && outLobbyDetailsHandle != null)
                {
                    lobbyObj.InitFromLobbyDetails(outLobbyDetailsHandle);

                    if (lobbyObj == null)
                    {
                        Debug.LogWarning("Lobbies (OnLobbySearchCompleted): lobbyObj is null!");
                        continue;
                    }

                    if (!lobbyObj.IsValid())
                    {
                        Debug.LogWarning("Lobbies (OnLobbySearchCompleted): Lobby is invalid, skip.");
                        continue;
                    }

                    if (outLobbyDetailsHandle == null)
                    {
                        Debug.LogWarning("Lobbies (OnLobbySearchCompleted): outLobbyDetailsHandle is null!");
                        continue;
                    }

                    SearchResults.Add(lobbyObj, outLobbyDetailsHandle);

                    Debug.LogFormat("Lobbies (OnLobbySearchCompleted): Added lobbyId: '{0}'", lobbyObj.Id);
                }
            }

            Debug.Log("Lobbies  (OnLobbySearchCompleted):  SearchResults Lobby objects = " + SearchResults.Count);

            LobbySearchCallback?.Invoke(Result.Success);
        }

        private void OnLobbyJoinFailed(string lobbyId)
        {
            _Dirty = true;

            PopLobbyInvite();
        }

        // Invite

        private void OnLobbyInvite(string inviteId, ProductUserId senderUserId)
        {
            LobbyInvite newLobbyInvite = new LobbyInvite();

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions();
            options.InviteId = inviteId;

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByInviteId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbyInvite): could not get lobby details: error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnLobbyInvite): could not get lobby details: null details handle.");
                return;
            }

            newLobbyInvite.Lobby.InitFromLobbyDetails(outLobbyDetailsHandle);
            newLobbyInvite.LobbyInfo = outLobbyDetailsHandle;
            newLobbyInvite.InviteId = inviteId;

            newLobbyInvite.FriendId = senderUserId;
            //newLobbyInvite.FriendEpicId = new EpicAccountId(); // TODO!!!!

            // If there's already an invite, check to see if the sender is the same as the current.
            // If it is, then update the current invite with the new invite.
            // If not, then add/update the new invite for the new sender.
            if (CurrentInvite == null ||
                (CurrentInvite != null && senderUserId == CurrentInvite.FriendId))
            {
                CurrentInvite = newLobbyInvite;
            }
            else
            {
                // This is not the current invite by the invite sender. Add it to the dictionary
                // if it doesn't exist, or update it if there's already an invite from this sender.
                Invites.TryGetValue(senderUserId, out LobbyInvite invite);

                if (invite == null)
                {
                    Invites.Add(senderUserId, newLobbyInvite);
                }
                else
                {
                    Invites[senderUserId] = newLobbyInvite;
                }
            }

            _Dirty = true;
        }

        private void OnJoinLobbyAccepted(ref JoinLobbyAcceptedCallbackInfo data)
        {
            // Callback for JoinLobbyAcceptedNotification

            CopyLobbyDetailsHandleByUiEventIdOptions options = new CopyLobbyDetailsHandleByUiEventIdOptions();
            options.UiEventId = data.UiEventId;

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByUiEventId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnJoinLobbyAccepted): could not get lobby details: error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnJoinLobbyAccepted): could not get lobby details: null details handle.");
                return;
            }

            Lobby newLobby = new Lobby();
            newLobby.InitFromLobbyDetails(outLobbyDetailsHandle);

            JoinLobby(newLobby.Id, outLobbyDetailsHandle, true, null);
            PopLobbyInvite();
        }

        private void OnLobbyInviteReceived(ref LobbyInviteReceivedCallbackInfo data)
        {
            // Callback for LobbyInviteNotification

            //if (data == null)
            //{
            //    Debug.LogFormat("Lobbies (OnLobbyInviteReceived): LobbyInviteReceivedCallbackInfo data is null");
            //    return;
            //}

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions();
            options.InviteId = data.InviteId;

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByInviteId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbyInvite): could not get lobby details: error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnLobbyInvite): could not get lobby details: null details handle.");
                return;
            }

            OnLobbyInvite(data.InviteId, data.TargetUserId);
        }

        private void OnLobbyInviteAccepted(ref LobbyInviteAcceptedCallbackInfo data)
        {
            // Callback for LobbyInviteAcceptedNotification

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies  (OnLobbyInviteAccepted): LobbyInviteAcceptedCallbackInfo data is null");
            //    return;
            //}

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions()
            {
                InviteId = data.InviteId
            };

            Result lobbyDetailsResult = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByInviteId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (lobbyDetailsResult != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbyInviteAccepted) could not get lobby details: error code: {0}", lobbyDetailsResult);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnLobbyInviteAccepted) could not get lobby details: null details handle.");
                return;
            }

            Lobby lobby = new Lobby();
            lobby.InitFromLobbyDetails(outLobbyDetailsHandle);

            JoinLobby(lobby.Id, outLobbyDetailsHandle, true, null);
        }

        private void PopLobbyInvite()
        {
            if (Invites.Count > 0)
            {
                var nextInvite = Invites.GetEnumerator();
                nextInvite.MoveNext();
                CurrentInvite = nextInvite.Current.Value;
                Invites.Remove(nextInvite.Current.Key);
            }
            else
            {
                CurrentInvite = null;
            }
        }

        // CanInviteToCurrentLobby()

        //-------------------------------------------------------------------------
        // It appears that to get RTC to work on some platforms, the RTC API needs to be
        // 'kicked' to enumerate the input and output devices.
        //
        private void HackWorkaroundRTCInitIssues()
        {
            // Hack to get RTC working
            var audioOptions = new Epic.OnlineServices.RTCAudio.GetAudioInputDevicesCountOptions();
            EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().GetAudioInputDevicesCount(ref audioOptions);

            var audioOutputOptions = new Epic.OnlineServices.RTCAudio.GetAudioOutputDevicesCountOptions();
            EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().GetAudioOutputDevicesCount(ref audioOutputOptions);
        }

        private void AddLocalUserAttributes()
        {
            string localUserDisplayName = UserInfoManager.GetLocalUserInfo().DisplayName;

            if (!string.IsNullOrEmpty(localUserDisplayName))
            {
                Debug.Log("Lobbies (AddLocalUserAttributes): adding displayname attribute.");
                LobbyAttribute nameAttrib = new LobbyAttribute() { Key = LobbyMember.DisplayNameKey, AsString = localUserDisplayName, ValueType = AttributeType.String };
                SetMemberAttribute(nameAttrib);
            }
        }

        // Join Events

        //-------------------------------------------------------------------------
        /// <summary>
        /// Wrapper for calling [EOS_Lobby_JoinLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_JoinLobby/index.html)
        /// </summary>
        /// <param name="lobbyId">Target lobbyId of lobby to join</param>
        /// <param name="lobbyDetails">Reference to <c>LobbyDetails</c> of lobby to join</param>
        /// <param name="presenceEnabled">Presence Enabled if <c>true</c></param>
        /// <param name="JoinLobbyCompleted">Callback when join lobby is completed</param>
        public void JoinLobby(string lobbyId, LobbyDetails lobbyDetails, bool presenceEnabled, OnLobbyCallback JoinLobbyCompleted)
        {
            HackWorkaroundRTCInitIssues();

            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogError("Lobbies (JoinButtonOnClick): lobbyId is null or empty!");
                JoinLobbyCompleted?.Invoke(Result.InvalidParameters);
                return;
            }

            if (lobbyDetails == null)
            {
                Debug.LogError("Lobbies (JoinButtonOnClick): lobbyDetails is null!");
                JoinLobbyCompleted?.Invoke(Result.InvalidParameters);
                return;
            }

            if (CurrentLobby.IsValid())
            {
                if (string.Equals(CurrentLobby.Id, lobbyId, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("Lobbies (JoinLobby): Already in the same lobby!");
                    return;
                }

                // TODO Active Join
                //ActiveJoin = 
                LeaveLobby(null);
                Debug.LogError("Lobbies (JoinLobby): Leaving lobby now (must Join again, Active Join Not Implemented)!");
                JoinLobbyCompleted?.Invoke(Result.InvalidState);

                return;
            }

            JoinLobbyOptions joinOptions = new JoinLobbyOptions();
            joinOptions.LobbyDetailsHandle = lobbyDetails;
            joinOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            joinOptions.PresenceEnabled = presenceEnabled;
            if (customLocalRTCOptions != null)
            {
                joinOptions.LocalRTCOptions = customLocalRTCOptions;
            }
            EOSManager.Instance.GetEOSLobbyInterface().JoinLobby(ref joinOptions, JoinLobbyCompleted, OnJoinLobbyCompleted);
        }

        private void OnJoinLobbyCompleted(ref JoinLobbyCallbackInfo data)
        {
            OnLobbyCallback JoinLobbyCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnJoinLobbyCompleted): JoinLobbyCallbackInfo data is null");
            //    JoinLobbyCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnJoinLobbyCompleted): error code: {0}", data.ResultCode);
                OnLobbyJoinFailed(data.LobbyId);
                JoinLobbyCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnJoinLobbyCompleted): Lobby join finished.");

            // OnLobbyJoined
            if (CurrentLobby.IsValid() && !string.Equals(CurrentLobby.Id, data.LobbyId))
            {
                LeaveLobby(null);
            }

            CurrentLobby.InitFromLobbyHandle(data.LobbyId);

            if(CurrentLobby.RTCRoomEnabled)
            {
                SubscribeToRTCEvents();
            }

            _Dirty = true;

            PopLobbyInvite();

            JoinLobbyCallback?.Invoke(Result.Success);

            OnCurrentLobbyChanged(LobbyChangeType.Join);
        }

        private void OnLeaveLobbyCompleted(ref LeaveLobbyCallbackInfo data)
        {
            OnLobbyCallback LeaveLobbyCallback = data.ClientData as OnLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogFormat("Lobbies (OnLeaveLobbyCompleted): LobbyInviteReceivedCallbackInfo data is null");
            //    LeaveLobbyCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogFormat("Lobbies (OnLeaveLobbyCompleted): error code: {0}", data.ResultCode);
                LeaveLobbyCallback?.Invoke(data.ResultCode);
            }
            else
            {
                Debug.Log("Lobbies (OnLeaveLobbyCompleted): Successfully left lobby: " + data.LobbyId);

                CurrentLobby.Clear();

                LeaveLobbyCallback?.Invoke(Result.Success);

                OnCurrentLobbyChanged(LobbyChangeType.Leave);
            }
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Wrapper for calling [EOS_Lobby_RejectInvite](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_RejectInvite/index.html)
        /// </summary>
        public void DeclineLobbyInvite()
        {
            if (CurrentInvite != null && CurrentInvite.IsValid())
            {
                ProductUserId currentUserProductId = EOSManager.Instance.GetProductUserId();
                if (!currentUserProductId.IsValid())
                {
                    Debug.LogError("Lobbies (DeclineLobbyInvite): Current player is invalid!");
                    return;
                }

                RejectInviteOptions rejectOptions = new RejectInviteOptions();
                rejectOptions.InviteId = CurrentInvite.InviteId;
                rejectOptions.LocalUserId = currentUserProductId;

                EOSManager.Instance.GetEOSLobbyInterface().RejectInvite(ref rejectOptions, null, OnDeclineInviteCompleted);
            }

            // LobbyId does not match current invite, reject can be ignored
        }

        private void OnDeclineInviteCompleted(ref RejectInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnDeclineInviteCompleted): RejectInviteCallbackInfo data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnDeclineInviteCompleted): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnDeclineInviteCompleted): Invite rejected");

            PopLobbyInvite();
        }

        /// <summary>
        /// If there is a current invite, calls <c>JoinLobby</c>
        /// </summary>
        /// <param name="enablePresence">Presence Enabled if <c>true</c></param>
        /// <param name="AcceptLobbyInviteCompleted">Callback when join lobby is completed</param>
        public void AcceptCurrentLobbyInvite(bool enablePresence, OnLobbyCallback AcceptLobbyInviteCompleted)
        {
            if (CurrentInvite != null && CurrentInvite.IsValid())
            {
                Debug.Log("Lobbies (AcceptCurrentLobbyInvite): Accepted invite, joining lobby.");

                JoinLobby(CurrentInvite.Lobby.Id, CurrentInvite.LobbyInfo, enablePresence, AcceptLobbyInviteCompleted);
            }
            else
            {
                Debug.LogError("Lobbies (AcceptCurrentLobbyInvite): Current invite is null or invalid!");

                AcceptLobbyInviteCompleted(Result.InvalidState);
            }
        }

        /// <summary>
        /// Calls <c>JoinLobby</c> on specified <c>LobbyInvite</c>
        /// </summary>
        /// <param name="lobbyInvite">Specified invite to accept</param>
        /// <param name="enablePresence">Presence Enabled if <c>true</c></param>
        /// <param name="AcceptLobbyInviteCompleted">Callback when join lobby is completed</param>
        public void AcceptLobbyInvite(LobbyInvite lobbyInvite, bool enablePresence, OnLobbyCallback AcceptLobbyInviteCompleted)
        {
            if (lobbyInvite != null && lobbyInvite.IsValid())
            {
                Debug.Log("Lobbies (AcceptLobbyInvite): Accepted invite, joining lobby.");

                JoinLobby(lobbyInvite.Lobby.Id, lobbyInvite.LobbyInfo, enablePresence, AcceptLobbyInviteCompleted);
            }
            else
            {
                Debug.LogError("Lobbies (AcceptLobbyInvite): lobbyInvite parameter is null or invalid!");

                AcceptLobbyInviteCompleted(Result.InvalidState);
            }
        }
    }
}

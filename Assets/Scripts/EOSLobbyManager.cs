﻿using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
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

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }

        public bool IsOwner(ProductUserId userProductId)
        {
            return userProductId == LobbyOwner;
        }

        public void Clear()
        {
            Id = string.Empty;
            LobbyOwner = new ProductUserId();
            Attributes.Clear();
            Members.Clear();
        }

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

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandle(options, out LobbyDetails outLobbyDetailsHandle);
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

        public void InitFromLobbyDetails(LobbyDetails outLobbyDetailsHandle)
        {
            // get owner
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions());
            if (newLobbyOwner != LobbyOwner)
            {
                LobbyOwner = newLobbyOwner;
                LobbyOwnerAccountId = new EpicAccountId();
                LobbyOwnerDisplayName = string.Empty;
            }

            // copy lobby info
            Result infoResult = outLobbyDetailsHandle.CopyInfo(new LobbyDetailsCopyInfoOptions(), out LobbyDetailsInfo outLobbyDetailsInfo);
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

            Id = outLobbyDetailsInfo.LobbyId;
            MaxNumLobbyMembers = outLobbyDetailsInfo.MaxMembers;
            LobbyPermissionLevel = outLobbyDetailsInfo.PermissionLevel;
            AllowInvites = outLobbyDetailsInfo.AllowInvites;
            AvailableSlots = outLobbyDetailsInfo.AvailableSlots;
            BucketId = outLobbyDetailsInfo.BucketId;
            RTCRoomEnabled = outLobbyDetailsInfo.RTCRoomEnabled;

            // get attributes
            Attributes.Clear();
            uint attrCount = outLobbyDetailsHandle.GetAttributeCount(new LobbyDetailsGetAttributeCountOptions());
            for (uint i = 0; i < attrCount; i++)
            {
                LobbyDetailsCopyAttributeByIndexOptions attrOptions = new LobbyDetailsCopyAttributeByIndexOptions();
                attrOptions.AttrIndex = i;
                Result copyAttrResult = outLobbyDetailsHandle.CopyAttributeByIndex(attrOptions, out Epic.OnlineServices.Lobby.Attribute outAttribute);
                if (copyAttrResult == Result.Success && outAttribute != null && outAttribute.Data != null)
                {
                    LobbyAttribute attr = new LobbyAttribute();
                    attr.InitFromAttribute(outAttribute);
                    Attributes.Add(attr);
                }
            }

            // get members
            List<LobbyMember> OldMembers = new List<LobbyMember>(Members);
            Members.Clear();

            uint memberCount = outLobbyDetailsHandle.GetMemberCount(new LobbyDetailsGetMemberCountOptions());

            for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                ProductUserId memberId = outLobbyDetailsHandle.GetMemberByIndex(new LobbyDetailsGetMemberByIndexOptions() { MemberIndex = (uint)memberIndex });
                Members.Insert((int)memberIndex, new LobbyMember() { ProductId = memberId });

                // member attributes
                int memberAttributeCount = (int)outLobbyDetailsHandle.GetMemberAttributeCount(new LobbyDetailsGetMemberAttributeCountOptions() { TargetUserId = memberId });

                for (int attributeIndex = 0; attributeIndex < memberAttributeCount; attributeIndex++)
                {
                    Result memberAttributeResult = outLobbyDetailsHandle.CopyMemberAttributeByIndex(new LobbyDetailsCopyMemberAttributeByIndexOptions() { AttrIndex = (uint)attributeIndex, TargetUserId = memberId }, out Epic.OnlineServices.Lobby.Attribute outAttribute);

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

    public class LobbyAttribute
    {
        public LobbyAttributeVisibility Visibility = LobbyAttributeVisibility.Public;

        public AttributeType ValueType = AttributeType.String;
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
                        attrData.Value.AsUtf8 = AsString;
                        break;
                    case AttributeType.Int64:
                        attrData.Value.AsInt64 = AsInt64;
                        break;
                    case AttributeType.Double:
                        attrData.Value.AsDouble = AsDouble;
                        break;
                    case AttributeType.Boolean:
                        attrData.Value.AsBool = AsBool;
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

        public void InitFromAttribute(Epic.OnlineServices.Lobby.Attribute attributeParam)
        {
            Key = attributeParam.Data.Key;
            ValueType = attributeParam.Data.Value.ValueType;

            switch (attributeParam.Data.Value.ValueType)
            {
                case AttributeType.Boolean:
                    AsBool = attributeParam.Data.Value.AsBool;
                    break;
                case AttributeType.Int64:
                    AsInt64 = attributeParam.Data.Value.AsInt64;
                    break;
                case AttributeType.Double:
                    AsDouble = attributeParam.Data.Value.AsDouble;
                    break;
                case AttributeType.String:
                    AsString = attributeParam.Data.Value.AsUtf8;
                    break;
            }
        }
    }

    public class LobbyMember
    {
        public EpicAccountId AccountId;
        public ProductUserId ProductId;

        public string DisplayName;
        public Dictionary<string, LobbyAttribute> MemberAttributes = new Dictionary<string, LobbyAttribute>();

        public LobbyRTCState RTCState = new LobbyRTCState();
    }

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
    }

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

        // TODO: Does this constant exist in the EOS SDK C# Wrapper?
        private const ulong EOS_INVALID_NOTIFICATIONID = 0;

        public bool _Dirty = true;

        // Manager Callbacks
        private OnLobbyCallback LobbyCreatedCallback;
        private OnLobbyCallback LobbyModifyCallback;
        private OnLobbyCallback JoinLobbyCallback;
        private OnLobbyCallback LeaveLobbyCallback;
        private OnLobbyCallback DestroyLobbyCallback;
        private OnLobbyCallback ToggleMuteCallback;
        private OnLobbyCallback KickMemberCallback;
        private OnLobbyCallback PromoteMemberCallback;
        private OnLobbySearchCallback LobbySearchCallback;

        public delegate void OnLobbyCallback(Result result);
        public delegate void OnLobbySearchCallback(Result result);

        // Init

        public EOSLobbyManager()
        {
            CurrentLobby = new Lobby();
            ActiveJoin = new LobbyJoinRequest();

            Invites = new Dictionary<ProductUserId, LobbyInvite>();
            CurrentInvite = null;

            CurrentSearch = new LobbySearch();
            SearchResults = new Dictionary<Lobby, LobbyDetails>();

            SubscribeToLobbyUpdates();
            SubscribeToLobbyInvites();

            LobbyCreatedCallback = null;
            LobbyModifyCallback = null;
            JoinLobbyCallback = null;
            LeaveLobbyCallback = null;
            LobbySearchCallback = null;
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

        private void SubscribeToLobbyUpdates()
        {
            if(IsLobbyNotificationValid(LobbyUpdateNotification) ||
                IsLobbyNotificationValid(LobbyMemberUpdateNotification) || 
                IsLobbyNotificationValid(LobbyMemberStatusNotification))
            {
                Debug.LogError("Lobbies (SubscribeToLobbyUpdates): SubscribeToLobbyUpdates called but already subscribed!");
                return;
            }
            

            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            LobbyUpdateNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyUpdateReceived(new AddNotifyLobbyUpdateReceivedOptions(), null, OnLobbyUpdateReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyUpdateReceived(handle);
            });

            LobbyMemberUpdateNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyMemberUpdateReceived(new AddNotifyLobbyMemberUpdateReceivedOptions(), null, OnMemberUpdateReceived), (ulong handle) => 
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberUpdateReceived(handle);
            });

            LobbyMemberStatusNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyMemberStatusReceived(new AddNotifyLobbyMemberStatusReceivedOptions(), null, OnMemberStatusReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });
        }

        private void UnsubscribeFromLobbyUpdates()
        {
            LobbyUpdateNotification.Dispose();
            LobbyMemberUpdateNotification.Dispose();
            LobbyMemberStatusNotification.Dispose();
        }

        //-------------------------------------------------------------------------
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
            LobbyInviteNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyInviteReceived(new AddNotifyLobbyInviteReceivedOptions(), null, OnLobbyInviteReceived), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyInviteReceived(handle);
            });

            LobbyInviteAcceptedNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyInviteAccepted(new AddNotifyLobbyInviteAcceptedOptions(), null, OnLobbyInviteAccepted), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyInviteAccepted(handle);
            });

            JoinLobbyAcceptedNotification = new NotifyEventHandle(lobbyInterface.AddNotifyJoinLobbyAccepted(new AddNotifyJoinLobbyAcceptedOptions(), null, OnJoinLobbyAccepted), (ulong handle) =>
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

            Result result = EOSManager.Instance.GetEOSLobbyInterface().GetRTCRoomName(options, out string roomName);

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
            AddNotifyRTCRoomConnectionChangedOptions addNotifyRTCRoomConnectionChangedOptions = new AddNotifyRTCRoomConnectionChangedOptions()
            {
                LobbyId = CurrentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            CurrentLobby.RTCRoomConnectionChanged = new NotifyEventHandle(lobbyInterface.AddNotifyRTCRoomConnectionChanged(addNotifyRTCRoomConnectionChangedOptions, null, OnRTCRoomConnectionChangedCompleted), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyRTCRoomConnectionChanged(handle);
            });

            if(!CurrentLobby.RTCRoomConnectionChanged.IsValid())
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Failed to bind to Lobby NotifyRTCRoomConnectionChanged notification.");
            }

            // Get the current room connection status now that we're listening for changes
            IsRTCRoomConnectedOptions isRTCRoomConnectedOptions = new IsRTCRoomConnectedOptions()
            {
                LobbyId = CurrentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            Result result = lobbyInterface.IsRTCRoomConnected(isRTCRoomConnectedOptions, out bool isConnected);

            if (result != Result.Success)
            {
                Debug.LogFormat("Lobbies (SubscribeToRTCEvents): Failed to get RTC Room connection status:. Error Code: {0}", result);
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

            CurrentLobby.RTCRoomParticipantUpdate = new NotifyEventHandle(rtcHandle.AddNotifyParticipantStatusChanged(addNotifyParticipantsStatusChangedOptions, null, OnParticipantStatusChangedCompleted), (ulong handle) =>
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

            CurrentLobby.RTCRoomParticipantAudioUpdate = new NotifyEventHandle(rtcAudioHandle.AddNotifyParticipantUpdated(addNotifyParticipantUpdatedOptions, null, OnParticipantUpdatedCompleted), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().RemoveNotifyParticipantUpdated(handle);
            });
        }

        private void OnParticipantUpdatedCompleted(ParticipantUpdatedCallbackInfo data)
        {
            throw new NotImplementedException();
        }

        private void OnParticipantStatusChangedCompleted(ParticipantStatusChangedCallbackInfo data)
        {
            throw new NotImplementedException();
        }

        private void OnRTCRoomConnectionChangedCompleted(RTCRoomConnectionChangedCallbackInfo data)
        {
            throw new NotImplementedException();
        }

        // User Events
        public void OnLoggedIn()
        {
            _Dirty = true;
            CurrentInvite = null;
        }

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

        // Lobby Create/Leave/Modify Events

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

            // Voice Chat
            if(lobbyProperties.RTCRoomEnabled)
            {
                LocalRTCOptions rtcOptions = new LocalRTCOptions()
                {
                    Flags = 0, //EOS_RTC_JOINROOMFLAGS_ENABLE_ECHO;
                    UseManualAudioInput = false,
                    UseManualAudioOutput = false,
                    AudioOutputStartsMuted = false
                };

                createLobbyOptions.EnableRTCRoom = true;
                createLobbyOptions.LocalRTCOptions = rtcOptions;
            }
            else
            {
                createLobbyOptions.EnableRTCRoom = false;
                createLobbyOptions.LocalRTCOptions = null;
            }

            // Note: Attributes are handled in ModifyLobby
            LobbyCreatedCallback = CreateLobbyCompleted;

            EOSManager.Instance.GetEOSLobbyInterface().CreateLobby(createLobbyOptions, null, OnCreateLobbyCompleted);

            // Save lobby data for modification
            CurrentLobby = lobbyProperties;
            CurrentLobby._BeingCreated = true;
            CurrentLobby.LobbyOwner = currentUserProductId;
        }

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
            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(options, out LobbyModification outLobbyModificationHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not create lobby modification. Error code: {0}", result);
                ModififyLobbyCompleted?.Invoke(result);
                return;
            }

            // Bucket Id
            if(!string.Equals(lobbyUpdates.BucketId, CurrentLobby.BucketId))
            {
                outLobbyModificationHandle.SetBucketId(new LobbyModificationSetBucketIdOptions() { BucketId = lobbyUpdates.BucketId });

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
                result = outLobbyModificationHandle.SetMaxMembers(new LobbyModificationSetMaxMembersOptions() { MaxMembers = lobbyUpdates.MaxNumLobbyMembers });

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

                if (attributeData.Key == null)
                {
                    Debug.LogWarning("Lobbies (ModifyLobby): Attributes with null key! Do not add!");
                    continue;
                }

                if (attributeData.Value == null)
                {
                    Debug.LogWarningFormat("Lobbies (ModifyLobby): Attributes with key '{0}' has null value! Do not add!", attributeData.Key);
                    continue;
                }

                result = outLobbyModificationHandle.AddAttribute(addAttributeOptions);
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
                result = outLobbyModificationHandle.SetPermissionLevel(new LobbyModificationSetPermissionLevelOptions() { PermissionLevel = lobbyUpdates.LobbyPermissionLevel });

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
                outLobbyModificationHandle.SetInvitesAllowed(new LobbyModificationSetInvitesAllowedOptions() { InvitesAllowed = lobbyUpdates.AllowInvites });

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set allow invites. Error code: {0}", result);
                    ModififyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            LobbyModifyCallback = ModififyLobbyCompleted;

            //Trigger lobby update
            EOSManager.Instance.GetEOSLobbyInterface().UpdateLobby(new UpdateLobbyOptions() { LobbyModificationHandle = outLobbyModificationHandle }, null, OnUpdateLobbyCallBack);
        }

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

            LeaveLobbyCallback = LeaveLobbyCompleted;

            EOSManager.Instance.GetEOSLobbyInterface().LeaveLobby(options, null, OnLeaveLobbyCompleted);
        }

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

            EOSManager.Instance.GetEOSLobbyInterface().SendInvite(options, null, OnSendInviteCompleted);
        }

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

            Result result = EOSManager.Instance.GetEOSLobbyInterface().UpdateLobbyModification(options, out LobbyModification lobbyModificationHandle);

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

            result = lobbyModificationHandle.AddMemberAttribute(attrOptions);

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

            EOSManager.Instance.GetEOSLobbyInterface().UpdateLobby(updateOptions, null, OnUpdateLobbyCallBack);
        }

        private void OnSendInviteCompleted(SendInviteCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnSendInviteCompleted): SendInviteCallbackInfo data is null");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnSendInviteCompleted): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnSendInviteCompleted): invite sent.");
        }

        private void OnCreateLobbyCompleted(CreateLobbyCallbackInfo createLobbyCallbackInfo)
        {
            if (createLobbyCallbackInfo == null)
            {
                Debug.Log("Lobbies (OnCreateLobbyCompleted): createLobbyCallbackInfo is null");
                LobbyCreatedCallback?.Invoke(Result.InvalidState);
                return;
            }

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
                    ModifyLobby(CurrentLobby, LobbyCreatedCallback);

                    if(CurrentLobby.RTCRoomEnabled)
                    {
                        SubscribeToRTCEvents();
                    }
                }

                _Dirty = true;

                LobbyCreatedCallback?.Invoke(Result.Success);
            }
        }

        private void OnUpdateLobbyCallBack(UpdateLobbyCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnUpdateLobbyCallBack): UpdateLobbyCallbackInfo data is null");
                LobbyModifyCallback?.Invoke(Result.InvalidState);
                return;
            }

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
            }
        }

        private void OnLobbyUpdateReceived(LobbyUpdateReceivedCallbackInfo data)
        {
            // Callback for LobbyUpdateNotification

            if (data != null)
            {
                Debug.Log("Lobbies (OnLobbyUpdateReceived): lobby update received.");
                OnLobbyUpdated(data.LobbyId, null);
            }
            else
            {
                Debug.LogError("Lobbies (OnLobbyUpdateReceived): EOS_Lobby_LobbyUpdateReceivedCallbackInfo is null");
            }
        }

        public void DestroyCurrentLobby(OnLobbyCallback DestroyCurrentLobbyCompleted)
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

            DestroyLobbyCallback = DestroyCurrentLobbyCompleted;

            EOSManager.Instance.GetEOSLobbyInterface().DestroyLobby(options, null, OnDestroyLobbyCompleted);

            // Clear current lobby
            CurrentLobby.Clear();
        }

        private void OnDestroyLobbyCompleted(DestroyLobbyCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnDestroyLobbyCompleted): DestroyLobbyCallbackInfo data is null");
                DestroyLobbyCallback?.Invoke(Result.InvalidState);
                return;
            }

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

        // Member Events

        public void ToggleMute(ProductUserId targetUserId, OnLobbyCallback ToggleMuteCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            foreach(LobbyMember lobbyMember in CurrentLobby.Members)
            {
                // Find the correct lobby member
                if(lobbyMember.ProductId != targetUserId)
                {
                    continue;
                }

                // Do not allow multiple local mute toggles at the same time
                if(lobbyMember.RTCState.MuteActionInProgress)
                {
                    Debug.LogWarningFormat("Lobbies (ToggleMute): 'MuteActionInProgress' for productUserId {0}.", targetUserId);
                    ToggleMuteCompleted?.Invoke(Result.RequestInProgress);
                    return;
                }

                // Set mute action as in progress
                lobbyMember.RTCState.MuteActionInProgress = true;

                // Check if muting ourselves vs other member
                if(EOSManager.Instance.GetProductUserId() == targetUserId)
                {
                    // Toggle our mute status
                    UpdateSendingOptions sendOptions = new UpdateSendingOptions()
                    {
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        RoomName = CurrentLobby.RTCRoomName,
                        AudioStatus = lobbyMember.RTCState.IsAudioOutputDisabled ? RTCAudioStatus.Enabled : RTCAudioStatus.Disabled
                    };

                    Debug.LogFormat("Lobbies (ToggleMute): Setting self audio output status to {0}", sendOptions.AudioStatus == RTCAudioStatus.Enabled ? "Unmuted" : "Muted");

                    ToggleMuteCallback = ToggleMuteCompleted;

                    rtcAudioHandle.UpdateSending(sendOptions, null, OnRTCRoomUpdateSendingCompleted);
                }
                else
                {
                    // Toggle mute for remote member (this is a local-only action and does not block the other user from receiving your audio stream)

                    UpdateReceivingOptions recevingOptions = new UpdateReceivingOptions()
                    {
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        RoomName = CurrentLobby.RTCRoomName,
                        ParticipantId = targetUserId,
                        AudioEnabled = lobbyMember.RTCState.IsLocalMuted
                    };

                    Debug.LogFormat("Lobbies (ToggleMute): {0} remote player {1}", recevingOptions.AudioEnabled ? "Unmuting" : "Muting", targetUserId);

                    ToggleMuteCallback = ToggleMuteCompleted;

                    rtcAudioHandle.UpdateReceiving(recevingOptions, null, OnRTCRoomUpdateReceivingCompleted);
                }
            }
        }

        private void OnRTCRoomUpdateSendingCompleted(UpdateSendingCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnRTCRoomUpdateSendingCompleted): UpdateSendingCallbackInfo data is null");
                ToggleMuteCallback?.Invoke(Result.InvalidState);
                return;
            }

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

                lobbyMember.RTCState.IsAudioOutputDisabled = data.AudioStatus == RTCAudioStatus.AdminDisabled;
                lobbyMember.RTCState.MuteActionInProgress = false;

                Debug.LogFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Cache updated for '{0}'", lobbyMember.DisplayName);

                _Dirty = true;
                break;
            }
        }

        private void OnRTCRoomUpdateReceivingCompleted(UpdateReceivingCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnRTCRoomUpdateReceivingCompleted): UpdateSendingCallbackInfo data is null");
                ToggleMuteCallback?.Invoke(Result.InvalidState);
                return;
            }

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
            if (EOSManager.Instance.GetProductUserId() == data.LocalUserId)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Incorrect call for local member.");
                return;
            }

            foreach(LobbyMember lobbyMember in CurrentLobby.Members)
            { 
                if(lobbyMember.ProductId == data.LocalUserId)
                {
                    continue;
                }

                lobbyMember.RTCState.IsLocalMuted = data.AudioEnabled == false;
                lobbyMember.RTCState.MuteActionInProgress = false;

                Debug.LogFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Cache updated for '{0}'", lobbyMember.DisplayName);

                _Dirty = true;
                break;
            }
        }

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

            KickMemberCallback = KickMemberCompleted;

            EOSManager.Instance.GetEOSLobbyInterface().KickMember(kickOptions, null, OnKickMemberCompleted);
        }

        private void OnKickMemberCompleted(KickMemberCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnKickMemberCompleted): KickMemberCallbackInfo data is null");
                KickMemberCallback?.Invoke(Result.InvalidState);
                return;
            }

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
            }
        }

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

            PromoteMemberCallback = PromoteMemberCompleted;

            EOSManager.Instance.GetEOSLobbyInterface().PromoteMember(promoteOptions, null, OnPromoteMemberCompleted);
        }

        private void OnPromoteMemberCompleted(PromoteMemberCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnPromoteMemberCompleted): PromoteMemberCallbackInfo data is null");
                PromoteMemberCallback?.Invoke(Result.InvalidState);
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnPromoteMemberFinished): error code: {0}", data.ResultCode);
                PromoteMemberCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnPromoteMemberFinished): Member promoted.");

            PromoteMemberCallback?.Invoke(Result.Success);
        }

        private void OnMemberStatusReceived(LobbyMemberStatusReceivedCallbackInfo data)
        {
            // Callback for LobbyMemberStatusNotification

            if (data == null)
            {
                Debug.LogError("Lobbies (OnMemberStatusReceived): LobbyMemberStatusReceivedCallbackInfo data is null");
                return;
            }

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

        private void OnMemberUpdateReceived(LobbyMemberUpdateReceivedCallbackInfo data)
        {
            // Callback for LobbyMemberUpdateNotification

            if (data == null)
            {
                Debug.LogError("Lobbies (OnMemberUpdateReceived): LobbyMemberUpdateReceivedCallbackInfo data is null");
                return;
            }

            Debug.Log("Lobbies (OnMemberUpdateReceived): Member update received.");
            OnLobbyUpdated(data.LobbyId, null);
        }


        // Search Events

        public void SearchByLobbyId(string lobbyId, OnLobbySearchCallback SearchCompleted)
        {
            Debug.LogFormat("Lobbies (SearchByLobbyId): lobbyId='{0}'", lobbyId);

            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogError("Lobbies (SearchByLobbyId): lobbyId is null or empty!");
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

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(new CreateLobbySearchOptions() { MaxResults = 10 }, out LobbySearch outLobbySearchHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByLobbyId): could not create SearchByLobbyId. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            CurrentSearch = outLobbySearchHandle;

            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions();
            setLobbyOptions.LobbyId = lobbyId;

            result = outLobbySearchHandle.SetLobbyId(setLobbyOptions);
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByLobbyId): failed to update SearchByLobbyId with lobby id. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            LobbySearchCallback = SearchCompleted;

            outLobbySearchHandle.Find(new LobbySearchFindOptions() { LocalUserId = currentProductUserId }, null, OnLobbySearchCompleted);
        }

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

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(new CreateLobbySearchOptions() { MaxResults = 10 }, out LobbySearch outLobbySearchHandle);

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
            attrData.Value.AsUtf8 = attributeValue.Trim();
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(paramOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByAttribute): failed to update SearchByAttribute with parameter. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            LobbySearchCallback = SearchCompleted;

            outLobbySearchHandle.Find(new LobbySearchFindOptions() { LocalUserId = currentProductUserId }, null, OnLobbySearchCompleted);
        }

        private void OnLobbySearchCompleted(LobbySearchFindCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnLobbySearchCompleted): LobbySearchFindCallbackInfo data is null");
                LobbySearchCallback?.Invoke(Result.InvalidState);
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbySearchCompleted): error code: {0}", data.ResultCode);
                LobbySearchCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnLobbySearchCompleted): Search finished.");

            // Process Search Results

            uint searchResultCount = CurrentSearch.GetSearchResultCount(new LobbySearchGetSearchResultCountOptions());

            Debug.LogFormat("Lobbies (OnLobbySearchCompleted): searchResultCount = {0}", searchResultCount);

            SearchResults.Clear();

            LobbySearchCopySearchResultByIndexOptions indexOptions = new LobbySearchCopySearchResultByIndexOptions();

            for (uint i = 0; i < searchResultCount; i++)
            {
                Lobby lobbyObj = new Lobby();

                indexOptions.LobbyIndex = i;

                Result result = CurrentSearch.CopySearchResultByIndex(indexOptions, out LobbyDetails outLobbyDetailsHandle);

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

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByInviteId(options, out LobbyDetails outLobbyDetailsHandle);

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

            Invites.TryGetValue(senderUserId, out LobbyInvite invite);

            if (invite == null)
            {
                Invites.Add(senderUserId, newLobbyInvite);
            }
            else
            {
                //Add new invite
                Invites[senderUserId] = newLobbyInvite;
            }

            if (CurrentInvite != null)
            {
                PopLobbyInvite();
            }
            else
            {
                CurrentInvite = newLobbyInvite;
            }

            _Dirty = true;
        }

        private void OnJoinLobbyAccepted(JoinLobbyAcceptedCallbackInfo data)
        {
            // Callback for JoinLobbyAcceptedNotification

            CopyLobbyDetailsHandleByUiEventIdOptions options = new CopyLobbyDetailsHandleByUiEventIdOptions();
            options.UiEventId = data.UiEventId;

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByUiEventId(options, out LobbyDetails outLobbyDetailsHandle);

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
            CurrentInvite = null;
        }

        private void OnLobbyInviteReceived(LobbyInviteReceivedCallbackInfo data)
        {
            // Callback for LobbyInviteNotification

            if (data == null)
            {
                Debug.LogFormat("Lobbies (OnLobbyInviteReceived): LobbyInviteReceivedCallbackInfo data is null");
                return;
            }

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions();
            options.InviteId = data.InviteId;

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByInviteId(options, out LobbyDetails outLobbyDetailsHandle);

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

        private void OnLobbyInviteAccepted(LobbyInviteAcceptedCallbackInfo data)
        {
            // Callback for LobbyInviteAcceptedNotification

            if (data == null)
            {
                Debug.LogError("Lobbies  (OnLobbyInviteAccepted): LobbyInviteAcceptedCallbackInfo data is null");
                return;
            }

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions()
            {
                InviteId = data.InviteId
            };

            Result lobbyDetailsResult = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandleByInviteId(options, out LobbyDetails outLobbyDetailsHandle);

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
            CurrentInvite = null;
        }

        private void PopLobbyInvite()
        {
            if (CurrentInvite != null)
            {
                Invites.Remove(CurrentInvite.FriendId);
                CurrentInvite = null;
            }

            if (Invites.Count > 0)
            {
                var nextInvite = Invites.GetEnumerator();
                nextInvite.MoveNext();
                CurrentInvite = nextInvite.Current.Value;
            }
        }

        // CanInviteToCurrentLobby()


        // Join Events

        public void JoinLobby(string lobbyId, LobbyDetails lobbyDetails, bool presenceEnabled, OnLobbyCallback JoinLobbyCompleted)
        {
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
                    //return;
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

            JoinLobbyCallback = JoinLobbyCompleted;

            EOSManager.Instance.GetEOSLobbyInterface().JoinLobby(joinOptions, null, OnJoinLobbyCompleted);
        }

        private void OnJoinLobbyCompleted(JoinLobbyCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnJoinLobbyCompleted): JoinLobbyCallbackInfo data is null");
                JoinLobbyCallback?.Invoke(Result.InvalidState);
                return;
            }

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
        }

        private void OnLeaveLobbyCompleted(LeaveLobbyCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogFormat("Lobbies (OnLeaveLobbyCompleted): LobbyInviteReceivedCallbackInfo data is null");
                LeaveLobbyCallback?.Invoke(Result.InvalidState);
                return;
            }

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
            }
        }

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

                EOSManager.Instance.GetEOSLobbyInterface().RejectInvite(rejectOptions, null, OnDeclineInviteCompleted);
            }

            // LobbyId does not match current invite, reject can be ignored
        }

        private void OnDeclineInviteCompleted(RejectInviteCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Lobbies (OnDeclineInviteCompleted): RejectInviteCallbackInfo data is null");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnDeclineInviteCompleted): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnDeclineInviteCompleted): Invite rejected");

            CurrentInvite = null;
        }

        public void AcceptCurrentLobbyInvite(bool enablePresence, OnLobbyCallback AcceptLobbyInviteCompleted)
        {
            if (CurrentInvite != null && CurrentInvite.IsValid())
            {
                Debug.Log("Lobbies (AcceptCurrentLobbyInvite): Accepted invite, joining lobby.");

                JoinLobby(CurrentInvite.Lobby.Id, CurrentInvite.LobbyInfo, enablePresence, AcceptLobbyInviteCompleted);
                CurrentInvite = null;
            }
            else
            {
                Debug.LogError("Lobbies (AcceptCurrentLobbyInvite): Current invite is null or invalid!");

                AcceptLobbyInviteCompleted(Result.InvalidState);
            }
        }

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
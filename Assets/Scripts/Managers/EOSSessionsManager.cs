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

using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Class represents a session search and search results
    /// </summary>
    public class SessionSearch
    {
        public Epic.OnlineServices.Sessions.SessionSearch SearchHandle;
        private Dictionary<Session, SessionDetails> SearchResults = new Dictionary<Session, SessionDetails>();

        public SessionSearch()
        {
            Release();
        }

        public void Release()
        {
            SearchResults.Clear();

            if (SearchHandle != null)
            {
                SearchHandle.Release();
                SearchHandle = null;
            }
        }

        public void SetNewSearch(Epic.OnlineServices.Sessions.SessionSearch handle)
        {
            Release();
            SearchHandle = handle;
        }

        public Epic.OnlineServices.Sessions.SessionSearch GetSearchHandle()
        {
            return SearchHandle;
        }

        public Dictionary<Session, SessionDetails> GetResults()
        {
            return SearchResults;
        }

        public SessionDetails GetSessionHandleById(string sessionId)
        {
            foreach (KeyValuePair<Session, SessionDetails> kvp in SearchResults)
            {
                if (kvp.Key.Id.Equals(sessionId, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        public void OnSearchResultReceived(Dictionary<Session, SessionDetails> results)
        {
            SearchResults = results;
        }
    }

    /// <summary>
    /// Class represents a single session attribute
    /// </summary>
    public class SessionAttribute
    {
        public AttributeType ValueType = AttributeType.String;
        public string Key;

        //Only one of the following properties will have valid data (depending on 'ValueType')
        public long? AsInt64 = 0;
        public double? AsDouble = 0.0;
        public bool? AsBool = false;
        public string AsString;

        public SessionAttributeAdvertisementType Advertisement = SessionAttributeAdvertisementType.DontAdvertise;

        public AttributeData AsAttribute
        {
            get
            {
                AttributeData attrData = new AttributeData();
                attrData.Key = Key;

                switch (ValueType)
                {
                    case AttributeType.String:
                        attrData.Value = new AttributeDataValue()
                        {
                            AsUtf8 = AsString
                        };
                        break;
                    case AttributeType.Int64:
                        attrData.Value = new AttributeDataValue()
                        {
                            AsInt64 = AsInt64
                        };
                        break;
                    case AttributeType.Double:
                        attrData.Value = new AttributeDataValue()
                        {
                            AsDouble = AsDouble
                        };
                        break;
                    case AttributeType.Boolean:
                        attrData.Value = new AttributeDataValue()
                        {
                            AsBool = AsBool
                        };
                        break;
                }

                return attrData;
            }
        }

        public override bool Equals(object other)
        {
            SessionAttribute sessionAttr = (SessionAttribute)other;

            return ValueType == sessionAttr.ValueType &&
                AsInt64 == sessionAttr.AsInt64 &&
                AsDouble == sessionAttr.AsDouble &&
                AsBool == sessionAttr.AsBool &&
                AsString == sessionAttr.AsString &&
                Key == sessionAttr.Key &&
                Advertisement == sessionAttr.Advertisement;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Class represents a single session attribute
    /// </summary>
    public class Session
    {
        public string Name = string.Empty;
        public string Id = string.Empty;
        public string BucketId = string.Empty;
        public uint MaxPlayers;
        public uint NumConnections = 1;
        public bool AllowJoinInProgress;
        public bool InvitesAllowed = true;
        public bool SanctionsEnabled = true;
        public OnlineSessionPermissionLevel PermissionLevel;
        public ActiveSession ActiveSession;

        public List<SessionAttribute> Attributes = new List<SessionAttribute>();
        public bool SearchResults = false;
        public bool UpdateInProgress = true;
        public OnlineSessionState SessionState = OnlineSessionState.NoSession;

        //private Session InvalidSession;

        //-------------------------------------------------------------------------
        public bool InitFromInfoOfSessionDetails(SessionDetails session)
        {
            //SessionDetails

            SessionDetailsCopyInfoOptions copyOptions = new SessionDetailsCopyInfoOptions();
            Result result = session.CopyInfo(ref copyOptions, out SessionDetailsInfo? outSessionInfo);

            if (result != Result.Success)
            {
                return false;
            }

            InitFromSessionInfo(session, outSessionInfo);
            //session.Release();  // Crashes EOS on JoinSession if session is released here
            return true;
        }

        //-------------------------------------------------------------------------
        public void InitFromSessionInfo(SessionDetails session, SessionDetailsInfo? sessionDetailsInfo)
        {
            if (sessionDetailsInfo != null && sessionDetailsInfo?.Settings != null)
            {
                // Copy session info
                AllowJoinInProgress = (bool)(sessionDetailsInfo?.Settings?.AllowJoinInProgress);
                BucketId = sessionDetailsInfo?.Settings?.BucketId;
                PermissionLevel = (OnlineSessionPermissionLevel)(sessionDetailsInfo?.Settings?.PermissionLevel);
                MaxPlayers = (uint)(sessionDetailsInfo?.Settings?.NumPublicConnections);
                Id = sessionDetailsInfo?.SessionId;
                //PresenceSession = // TODO
            }

            // Get Attributes
            Attributes.Clear();
            var sessionDetailsGetSessionAttributeCountOptions = new SessionDetailsGetSessionAttributeCountOptions();
            uint attributeCount = session.GetSessionAttributeCount(ref sessionDetailsGetSessionAttributeCountOptions);

            for (uint attribIndex = 0; attribIndex < attributeCount; attribIndex++)
            {
                SessionDetailsCopySessionAttributeByIndexOptions attributeOptions = new SessionDetailsCopySessionAttributeByIndexOptions();
                attributeOptions.AttrIndex = attribIndex;

                Result result = session.CopySessionAttributeByIndex(ref attributeOptions, out SessionDetailsAttribute? sessionAttribute);
                if (result == Result.Success && sessionAttribute != null && sessionAttribute?.Data != null)
                {
                    SessionAttribute nextAttribute = new SessionAttribute();
                    nextAttribute.Advertisement = (SessionAttributeAdvertisementType)(sessionAttribute?.AdvertisementType);
                    nextAttribute.Key = sessionAttribute?.Data?.Key;

                    var sessionAttributeDataValue = (AttributeDataValue)sessionAttribute?.Data?.Value;
                    switch (sessionAttributeDataValue.ValueType)
                    {
                        case AttributeType.Boolean:
                            nextAttribute.ValueType = AttributeType.Boolean;
                            nextAttribute.AsBool = sessionAttributeDataValue.AsBool;
                            break;
                        case AttributeType.Int64:
                            nextAttribute.ValueType = AttributeType.Int64;
                            nextAttribute.AsInt64 = sessionAttributeDataValue.AsInt64;
                            break;
                        case AttributeType.Double:
                            nextAttribute.ValueType = AttributeType.Double;
                            nextAttribute.AsDouble = sessionAttributeDataValue.AsDouble;
                            break;
                        case AttributeType.String:
                            nextAttribute.ValueType = AttributeType.String;
                            nextAttribute.AsString = sessionAttributeDataValue.AsUtf8;
                            break;
                    }

                    Attributes.Add(nextAttribute);
                }
            }

            InitActiveSession();

            UpdateInProgress = false;
        }

        //-------------------------------------------------------------------------
        public void InitActiveSession()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                CopyActiveSessionHandleOptions copyOptions = new CopyActiveSessionHandleOptions();
                copyOptions.SessionName = Name;

                SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
                Result result = sessionInterface.CopyActiveSessionHandle(ref copyOptions, out ActiveSession sessionHandle);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: could not get ActiveSession for name: {0}", Name);
                    return;
                }

                ActiveSession = sessionHandle;
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Id);
        }

        public override bool Equals(object other)
        {
            Session session = other as Session;

            return other != null && 
                Name.Equals(session.Name, StringComparison.OrdinalIgnoreCase) &&
                Id.Equals(session.Id, StringComparison.OrdinalIgnoreCase) &&
                BucketId.Equals(session.BucketId, StringComparison.OrdinalIgnoreCase) &&
                MaxPlayers == session.MaxPlayers &&
                NumConnections == session.NumConnections &&
                AllowJoinInProgress == session.AllowJoinInProgress &&
                InvitesAllowed == session.InvitesAllowed &&
                PermissionLevel == session.PermissionLevel &&
                Attributes == session.Attributes;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Class <c>EOSSessionsManager</c> is a simplified wrapper for EOS [Sessions Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Sessions/index.html).
    /// </summary>
    public class EOSSessionsManager : IEOSSubManager
    {
        private Dictionary<string, Session> CurrentSessions;

        private SessionSearch CurrentSearch;
        private string JoinPresenceSessionId = string.Empty;
        private ulong JoinUiEvent;
        private string KnownPresenceSessionId = string.Empty;

        private Dictionary<Session, SessionDetails> Invites;
        private Session CurrentInvite;

        private SessionDetails JoiningSessionDetails = null;
        private ulong JoinedSessionIndex = 0;

        private const string JOINED_SESSION_NAME = "Session#";
        private const uint JOINED_SESSION_NAME_ROTATION_NUM = 9;
        private const int JOINED_SESSION_NAME_ROTATION = 9;
        private const string BUCKET_ID = "SessionSample:Region";

        private const string EOS_SESSIONS_SEARCH_BUCKET_ID = "bucket";
        private const string EOS_SESSIONS_SEARCH_EMPTY_SERVERS_ONLY = "emptyonly";
        private const string EOS_SESSIONS_SEARCH_NONEMPTY_SERVERS_ONLY = "nonemptyonly";
        private const string EOS_SESSIONS_SEARCH_MINSLOTSAVAILABLE = "minslotsavailable";

        // UI Parameterized Callbacks
        Queue<Action> UIOnSessionCreated;
        Queue<Action> UIOnSessionModified;
        Queue<Action> UIOnJoinSession;
        Queue<Action> UIOnLeaveSession;
        Queue<Action> UIOnSessionSearchCompleted;

        public const ulong INVALID_NOTIFICATIONID = 0;

        public ulong SessionInviteNotificationHandle = INVALID_NOTIFICATIONID;
        public ulong SessionInviteAcceptedNotificationHandle = INVALID_NOTIFICATIONID;
        public ulong JoinGameNotificationHandle = INVALID_NOTIFICATIONID;
        public ulong SessionJoinGameNotificationHandle = INVALID_NOTIFICATIONID;

        private bool subscribtedToGameInvites = false;
        private bool userLoggedIn = false;
        public bool IsUserLoggedIn
        {
            get { return userLoggedIn; }
        }

        public EOSSessionsManager()
        {
            UIOnSessionCreated = new Queue<Action>();
            UIOnSessionModified = new Queue<Action>();
            UIOnJoinSession = new Queue<Action>();
            UIOnLeaveSession = new Queue<Action>();
            UIOnSessionSearchCompleted = new Queue<Action>();

            CurrentSessions = new Dictionary<string, Session>();
            CurrentSearch = new SessionSearch();
            Invites = new Dictionary<Session, SessionDetails>();
            CurrentInvite = null;
        }

        public Dictionary<Session, SessionDetails> GetInvites()
        {
            return Invites;
        }

        public Session GetCurrentInvite()
        {
            return CurrentInvite;
        }

        public SessionSearch GetCurrentSearch()
        {
            return CurrentSearch;
        }

        public Dictionary<string, Session> GetCurrentSessions()
        {
            return CurrentSessions;
        }

        //-------------------------------------------------------------------------
        public void SubscribteToGameInvites()
        {
            if (subscribtedToGameInvites)
            {
                Debug.LogWarning("Session Matchmaking (SubscribteToGameInvites): Already subscribed.");
                return;
            }

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSSessionsInterface();
            PresenceInterface presenceInterface = EOSManager.Instance.GetEOSPresenceInterface();

            var addNotifySessionInviteReceivedOptions = new AddNotifySessionInviteReceivedOptions();
            var addNotifySessionInviteAcceptedOptions = new AddNotifySessionInviteAcceptedOptions();
            var addNotifyJoinSessionAcceptedOptions = new AddNotifyJoinSessionAcceptedOptions();
            var addNotifyJoinGameAcceptedOptions = new AddNotifyJoinGameAcceptedOptions();

            SessionInviteNotificationHandle = sessionInterface.AddNotifySessionInviteReceived(ref addNotifySessionInviteReceivedOptions, null, OnSessionInviteReceivedListener);
            SessionInviteAcceptedNotificationHandle = sessionInterface.AddNotifySessionInviteAccepted(ref addNotifySessionInviteAcceptedOptions, null, OnSessionInviteAcceptedListener);
            JoinGameNotificationHandle = presenceInterface.AddNotifyJoinGameAccepted(ref addNotifyJoinGameAcceptedOptions, null, OnJoinGameAcceptedListener);
            SessionJoinGameNotificationHandle = sessionInterface.AddNotifyJoinSessionAccepted(ref addNotifyJoinSessionAcceptedOptions, null, OnJoinSessionAcceptedListener);

            subscribtedToGameInvites = true;
        }

        public void UnsubscribeFromGameInvites()
        {
            if (!subscribtedToGameInvites)
            {
                Debug.LogWarning("Session Matchmaking (UnsubscribeFromGameInvites): Not subscribed yet.");
                return;
            }

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            PresenceInterface presenceInterface = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();

            if (SessionInviteNotificationHandle != INVALID_NOTIFICATIONID)
            {
                sessionInterface.RemoveNotifySessionInviteReceived(SessionInviteNotificationHandle);
                SessionInviteNotificationHandle = INVALID_NOTIFICATIONID;
            }

            if (SessionInviteAcceptedNotificationHandle != INVALID_NOTIFICATIONID)
            {
                sessionInterface.RemoveNotifySessionInviteAccepted(SessionInviteAcceptedNotificationHandle);
                SessionInviteAcceptedNotificationHandle = INVALID_NOTIFICATIONID;
            }

            if (JoinGameNotificationHandle != INVALID_NOTIFICATIONID)
            {
                presenceInterface.RemoveNotifyJoinGameAccepted(JoinGameNotificationHandle);
                JoinGameNotificationHandle = INVALID_NOTIFICATIONID;
            }

            if (SessionJoinGameNotificationHandle != INVALID_NOTIFICATIONID)
            {
                sessionInterface.RemoveNotifyJoinSessionAccepted(SessionJoinGameNotificationHandle);
                SessionJoinGameNotificationHandle = INVALID_NOTIFICATIONID;
            }

            subscribtedToGameInvites = false;
        }

        private void OnShutDown()
        {
            DestroyAllSessions();
            UnsubscribeFromGameInvites();
        }

        //-------------------------------------------------------------------------
        public bool Update()
        {
            bool stateUpdates = false;

            //Update active session from time to time
            foreach (KeyValuePair<string, Session> kvp in CurrentSessions)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value.IsValid())
                {
                    Session session = kvp.Value;
                    if (session.UpdateInProgress)
                    {
                        continue;
                    }

                    // Update Settings State
                    if (session.ActiveSession != null)
                    {
                        SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
                        var activeSessionCopyInfoOptions = new ActiveSessionCopyInfoOptions();

                        Result result = session.ActiveSession.CopyInfo(ref activeSessionCopyInfoOptions, out ActiveSessionInfo? activeSession);
                        if (result == Result.Success)
                        {
                            if (activeSession != null && session.SessionState != activeSession?.State)
                            {
                                session.SessionState = (OnlineSessionState)(activeSession?.State);
                                stateUpdates = true;
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat("Session Matchmaking: ActiveSessionCopyInfo failed. Errors code: {0}", result); ;
                        }
                    }
                }
            }

            return stateUpdates;
        }

        public void OnLoggedIn()
        {
            if (userLoggedIn)
            {
                Debug.LogWarning("Session Matchmaking (OnLoggedIn): Already logged in.");
                return;
            }

            SubscribteToGameInvites();

            CurrentInvite = null;

            userLoggedIn = true;
        }

        public void OnLoggedOut()
        {
            if (!userLoggedIn)
            {
                Debug.LogWarning("Session Matchmaking (OnLoggedOut): Not logged in.");
                return;
            }

            UnsubscribeFromGameInvites();

            LeaveAllSessions();

            CurrentSearch.Release();
            CurrentSessions.Clear();
            Invites.Clear();
            CurrentInvite = null;
            JoiningSessionDetails = null;

            userLoggedIn = false;
        }

        private void LeaveAllSessions()
        {
            // Enumerate session entries in UI
            foreach (KeyValuePair<string, Session> kvp in GetCurrentSessions())
            {
                DestroySession(kvp.Key);
            }
        }

        //-------------------------------------------------------------------------
        public bool CreateSession(Session session, bool presence = false, Action callback = null)
        {
            if (session == null)
            {
                Debug.LogErrorFormat("Session Matchmaking: parameter 'session' cannot be null!");
                return false;
            }

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            if (sessionInterface == null)
            {
                Debug.LogErrorFormat("Session Matchmaking: can't get sessions interface.");
                return false;
            }

            CreateSessionModificationOptions createOptions = new CreateSessionModificationOptions();
            createOptions.BucketId = BUCKET_ID;
            createOptions.MaxPlayers = session.MaxPlayers;
            createOptions.SessionName = session.Name;
            createOptions.SanctionsEnabled = session.SanctionsEnabled;
            createOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            createOptions.PresenceEnabled = presence;

            Result result = sessionInterface.CreateSessionModification(ref createOptions, out SessionModification sessionModificationHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: could not create session modification. Error code: {0}", result);
                return false;
            }

            SessionModificationSetPermissionLevelOptions permisionOptions = new SessionModificationSetPermissionLevelOptions();
            permisionOptions.PermissionLevel = session.PermissionLevel;

            result = sessionModificationHandle.SetPermissionLevel(ref permisionOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed to set permissions. Error code: {0}", result);
                sessionModificationHandle.Release();
                return false;
            }

            SessionModificationSetJoinInProgressAllowedOptions jipOptions = new SessionModificationSetJoinInProgressAllowedOptions();
            jipOptions.AllowJoinInProgress = session.AllowJoinInProgress;

            result = sessionModificationHandle.SetJoinInProgressAllowed(ref jipOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed to set 'join in progress allowed' flag. Error code: {0}", result);
                sessionModificationHandle.Release();
                return false;
            }

            SessionModificationSetInvitesAllowedOptions iaOptions = new SessionModificationSetInvitesAllowedOptions();
            iaOptions.InvitesAllowed = session.InvitesAllowed;

            result = sessionModificationHandle.SetInvitesAllowed(ref iaOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed to set invites allowed. Error code: {0}", result);
                sessionModificationHandle.Release();
                return false;
            }

            // Set Bucket Id
            AttributeData attrData = new AttributeData();
            attrData.Key = EOS_SESSIONS_SEARCH_BUCKET_ID;
            attrData.Value = new AttributeDataValue()
            {
                AsUtf8 = BUCKET_ID
            };
            
            SessionModificationAddAttributeOptions attrOptions = new SessionModificationAddAttributeOptions();
            attrOptions.SessionAttribute = attrData;

            result = sessionModificationHandle.AddAttribute(ref attrOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed to set a bucket id attribute. Error code: {0}", result);
                sessionModificationHandle.Release();
                return false;
            }

            // Set Other Attributes
            foreach (SessionAttribute sdAttrib in session.Attributes)
            {
                attrData.Key = sdAttrib.Key;
                
                switch (sdAttrib.ValueType)
                {
                    case AttributeType.Boolean:
                        attrData.Value = (AttributeDataValue)sdAttrib.AsBool;
                        break;
                    case AttributeType.Double:
                        attrData.Value = (AttributeDataValue)sdAttrib.AsDouble;
                        break;
                    case AttributeType.Int64:
                        attrData.Value = (AttributeDataValue)sdAttrib.AsInt64;
                        break;
                    case AttributeType.String:
                        attrData.Value = sdAttrib.AsString;
                        break;
                }

                attrOptions.AdvertisementType = sdAttrib.Advertisement;
                attrOptions.SessionAttribute = attrData;

                result = sessionModificationHandle.AddAttribute(ref attrOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to set an attribute: {0}. Error code: {1}", sdAttrib.Key, result);
                    sessionModificationHandle.Release();
                    return false;
                }
            }

            if (callback != null)
            {
                UIOnSessionCreated.Enqueue(callback);
            }

            UpdateSessionOptions updateOptions = new UpdateSessionOptions();
            updateOptions.SessionModificationHandle = sessionModificationHandle;
            sessionInterface.UpdateSession(ref updateOptions, null, OnUpdateSessionCompleteCallback_ForCreate);

            sessionModificationHandle.Release();

            if (CurrentSessions.ContainsKey(session.Name))
            {
                CurrentSessions[session.Name] = session;
            }
            else
            {
                CurrentSessions.Add(session.Name, session);
            }
            CurrentSessions[session.Name].UpdateInProgress = true;

            return true;
        }

        //-------------------------------------------------------------------------
        public void DestroySession(string name)
        {
            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();

            DestroySessionOptions destroyOptions = new DestroySessionOptions();
            destroyOptions.SessionName = name;

            sessionInterface.DestroySession(ref destroyOptions, name, OnDestroySessionCompleteCallback);
        }

        public void DestroyAllSessions()
        {
            foreach (KeyValuePair<string, Session> session in CurrentSessions)
            {
                if (!session.Key.Contains(JOINED_SESSION_NAME))
                {
                    DestroySession(session.Key);
                }
            }
        }

        public bool HasActiveLocalSessions()
        {
            foreach (KeyValuePair<string, Session> session in CurrentSessions)
            {
                if (session.Key.Contains(JOINED_SESSION_NAME))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPresenceSession()
        {
            if (KnownPresenceSessionId.Length > 0)
            {
                if (CurrentSessions.ContainsKey(KnownPresenceSessionId)) // TODO: validate
                {
                    return true;
                }
                KnownPresenceSessionId = string.Empty;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                return false;
            }

            CopySessionHandleForPresenceOptions copyOptions = new CopySessionHandleForPresenceOptions();
            copyOptions.LocalUserId = currentProductUserId;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Result result = sessionInterface.CopySessionHandleForPresence(ref copyOptions, out SessionDetails sessionHandle);
            if (result != Result.Success)
            {
                return false;
            }

            if (sessionHandle == null)
            {
                return false;
            }

            SessionDetailsCopyInfoOptions copyInfoOptions = new SessionDetailsCopyInfoOptions();
            result = sessionHandle.CopyInfo(ref copyInfoOptions, out SessionDetailsInfo? sessioninfo);
            if (result != Result.Success)
            {
                return false;
            }

            KnownPresenceSessionId = sessioninfo?.SessionId;
            return true;
        }

        public bool IsPresenceSession(string id)
        {
            return HasPresenceSession() && id.Equals(KnownPresenceSessionId, StringComparison.OrdinalIgnoreCase);
        }

        public Session GetSession(string name)
        {
            if (CurrentSessions.TryGetValue(name, out Session session))
            {
                return session;
            }

            return null;
        }

        public void StartSession(string name)
        {
            if (CurrentSessions.TryGetValue(name, out Session session))
            {
                StartSessionOptions sessionOptions = new StartSessionOptions();
                sessionOptions.SessionName = name;

                SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();

                sessionInterface.StartSession(ref sessionOptions, name, OnStartSessionCompleteCallBack);
            }
            else
            {
                Debug.LogErrorFormat("Session Matchmaking: can't start session: no active session with specified name.");
                return;
            }
        }

        public void EndSession(string name)
        {
            if (!CurrentSessions.TryGetValue(name, out Session session))
            {
                Debug.LogErrorFormat("Session Matchmaking: can't end session: no active session with specified name: {0}", name);
                return;
            }

            EndSessionOptions endOptions = new EndSessionOptions();
            endOptions.SessionName = name;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.EndSession(ref endOptions, name, OnEndSessionCompleteCallback);
        }

        public void Register(string sessionName, ProductUserId friendId)
        {
            RegisterPlayersOptions registerOptions = new RegisterPlayersOptions();
            registerOptions.SessionName = sessionName;
            registerOptions.PlayersToRegister = new ProductUserId[] { friendId };

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.RegisterPlayers(ref registerOptions, null, OnRegisterCompleteCallback);
        }

        public void UnRegister(string sessionName, ProductUserId friendId)
        {
            UnregisterPlayersOptions unregisterOptions = new UnregisterPlayersOptions();
            unregisterOptions.SessionName = sessionName;
            unregisterOptions.PlayersToUnregister = new ProductUserId[] { friendId };

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.UnregisterPlayers(ref unregisterOptions, null, OnUnregisterCompleteCallback);
        }

        public void InviteToSession(string sessionName, ProductUserId friendId)
        {
            if (!friendId.IsValid())
            {
                Debug.LogError("Session Matchmaking - InviteToSession: friend's product user id is invalid!");
                return;
            }

            ProductUserId currentUserId = EOSManager.Instance.GetProductUserId();
            if (!currentUserId.IsValid())
            {
                Debug.LogError("Session Matchmaking - InviteToSession: current user's product user id is invalid!");
                return;
            }

            SendInviteOptions sendInviteOptions = new SendInviteOptions();
            sendInviteOptions.LocalUserId = currentUserId;
            sendInviteOptions.TargetUserId = friendId;
            sendInviteOptions.SessionName = sessionName;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.SendInvite(ref sendInviteOptions, null, OnSendInviteCompleteCallback);
        }

        public void SetInviteSession(Session session, SessionDetails sessionDetails)
        {
            // Add invite
            Invites.Add(session, sessionDetails);

            if (CurrentInvite != null)
            {
                PopLobbyInvite();
            }
            else
            {
                CurrentInvite = session;
            }
        }

        public void Search(List<SessionAttribute> attributes)
        {
            // Clear previous search
            CurrentSearch.Release();

            CreateSessionSearchOptions searchOptions = new CreateSessionSearchOptions();
            searchOptions.MaxSearchResults = 10;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Result result = sessionInterface.CreateSessionSearch(ref searchOptions, out Epic.OnlineServices.Sessions.SessionSearch sessionSearchHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed to create session search. Error code: {0}", result);
                return;
            }

            CurrentSearch.SetNewSearch(sessionSearchHandle);

            AttributeData attrData = new AttributeData();
            attrData.Key = EOS_SESSIONS_SEARCH_BUCKET_ID;
            attrData.Value = new AttributeDataValue()
            {
                AsUtf8 = BUCKET_ID
            };
            
            SessionSearchSetParameterOptions paramOptions = new SessionSearchSetParameterOptions();
            paramOptions.ComparisonOp = ComparisonOp.Equal;
            paramOptions.Parameter = attrData;

            result = sessionSearchHandle.SetParameter(ref paramOptions);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed to update session search with bucket id parameter. Error code: {0}", result);
                return;
            }

            // Set other attributes
            foreach (SessionAttribute attr in attributes)
            {
                attrData.Key = attr.Key;

                switch (attr.ValueType)
                {
                    case AttributeType.Boolean:
                        attrData.Value = (AttributeDataValue)attr.AsBool;
                        break;
                    case AttributeType.Int64:
                        attrData.Value = (AttributeDataValue)attr.AsInt64;
                        break;
                    case AttributeType.Double:
                        attrData.Value = (AttributeDataValue)attr.AsDouble;
                        break;
                    case AttributeType.String:
                        attrData.Value = attr.AsString;
                        break;
                }

                paramOptions.Parameter = attrData; // Needed or is by ref work?

                result = sessionSearchHandle.SetParameter(ref paramOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to update session search with parameter. Error code: {0}", result);
                    return;
                }
            }
            
            SessionSearchFindOptions findOptions = new SessionSearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            sessionSearchHandle.Find(ref findOptions, null, OnFindSessionsCompleteCallback);
        }

        public void SearchById(string sessionId)
        {
            // Clear previous search
            CurrentSearch.Release();

            CreateSessionSearchOptions searchOptions = new CreateSessionSearchOptions();
            searchOptions.MaxSearchResults = 10;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Result result = sessionInterface.CreateSessionSearch(ref searchOptions, out Epic.OnlineServices.Sessions.SessionSearch sessionSearchHandle);

            if (result != Result.Success)
            {
                AcknowledgeEventId(result);
                Debug.LogErrorFormat("Session Matchmaking: failed create session search. Error code: {0}", result);
                return;
            }

            CurrentSearch.SetNewSearch(sessionSearchHandle);

            SessionSearchSetSessionIdOptions sessionIdOptions = new SessionSearchSetSessionIdOptions();
            sessionIdOptions.SessionId = sessionId;

            result = sessionSearchHandle.SetSessionId(ref sessionIdOptions);

            if (result != Result.Success)
            {
                AcknowledgeEventId(result);
                Debug.LogErrorFormat("Session Matchmaking: failed to update session search with session ID. Error code: {0}", result);
                return;
            }

            SessionSearchFindOptions findOptions = new SessionSearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            sessionSearchHandle.Find(ref findOptions, null, OnFindSessionsCompleteCallback);
        }

        public SessionDetails MakeSessionHandleByInviteId(string inviteId)
        {
            CopySessionHandleByInviteIdOptions options = new CopySessionHandleByInviteIdOptions();
            options.InviteId = inviteId;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Result result = sessionInterface.CopySessionHandleByInviteId(ref options, out SessionDetails sessionHandle);

            if (result == Result.Success)
            {
                return sessionHandle;
            }

            return null;
        }

        public SessionDetails MakeSessionHandleByEventId(ulong uiEventId)
        {
            CopySessionHandleByUiEventIdOptions copyOptions = new CopySessionHandleByUiEventIdOptions();
            copyOptions.UiEventId = uiEventId;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Result result = sessionInterface.CopySessionHandleByUiEventId(ref copyOptions, out SessionDetails sessionHandle);
            if (result == Result.Success && sessionHandle != null)
            {
                return sessionHandle;
            }

            return null;
        }

        public SessionDetails MakeSessionHandleFromSearch(string sessionId)
        {
            // TODO if needed
            return null;
        }

        //-------------------------------------------------------------------------
        public void JoinSession(SessionDetails sessionHandle, bool presenceSession, Action<Result> callback = null)
        {
            JoinSessionOptions joinOptions = new JoinSessionOptions();
            joinOptions.SessionHandle = sessionHandle;
            joinOptions.SessionName = GenerateJoinedSessionName();
            joinOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            joinOptions.PresenceEnabled = presenceSession;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.JoinSession(ref joinOptions, callback, OnJoinSessionListener);

            //SetJoinSessionDetails
            JoiningSessionDetails = sessionHandle;
        }

        //-------------------------------------------------------------------------
        public bool ModifySession(Session session, Action callback = null)
        {
            if (session == null)
            {
                Debug.LogError("Session Matchmaking: pamater session is null.");
                return false;
            }

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();

            if (sessionInterface == null)
            {
                Debug.LogError("Session Matchmaking: can't get sessions interface.");
                return false;
            }

            if (!CurrentSessions.TryGetValue(session.Name, out Session currentSession))
            {
                Debug.LogError("Session Matchmaking: can't modify session: no active session with specified name.");
                return false;
            }

            UpdateSessionModificationOptions updateModOptions = new UpdateSessionModificationOptions();
            updateModOptions.SessionName = session.Name;

            Result result = sessionInterface.UpdateSessionModification(ref updateModOptions, out SessionModification sessionModificationHandle);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking: failed create session modification. Error code: {0}", result);
                return false;
            }

            // BucketId
            /*
            if (session.BucketId != currentSession.BucketId)
            {
                SessionModificationSetBucketIdOptions bucketOptions = new SessionModificationSetBucketIdOptions();
                bucketOptions.BucketId = session.BucketId;

                result = sessionModificationHandle.SetBucketId(bucketOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to set bucket id. Error code: {0}", result);
                    sessionModificationHandle.Release();
                    return false;
                }

                //Update local cache
                currentSession.BucketId = session.BucketId;
            }
            */

            // Max Players
            if (session.MaxPlayers != currentSession.MaxPlayers)
            {
                SessionModificationSetMaxPlayersOptions maxPlayerOptions = new SessionModificationSetMaxPlayersOptions();
                maxPlayerOptions.MaxPlayers = session.MaxPlayers;

                result = sessionModificationHandle.SetMaxPlayers(ref maxPlayerOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to set maxp layers. Error code:: {0}", result);
                    sessionModificationHandle.Release();
                    return false;
                }

                //Update local cache
                currentSession.MaxPlayers = session.MaxPlayers;
            }

            // Modify Permissions
            if (session.PermissionLevel != currentSession.PermissionLevel)
            {
                SessionModificationSetPermissionLevelOptions permisionOptions = new SessionModificationSetPermissionLevelOptions();
                permisionOptions.PermissionLevel = session.PermissionLevel;

                result = sessionModificationHandle.SetPermissionLevel(ref permisionOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to set permission level. Error code:: {0}", result);
                    sessionModificationHandle.Release();
                    return false;
                }

                //Update local cache
                currentSession.PermissionLevel = session.PermissionLevel;
            }

            // Join In Progress
            if (session.AllowJoinInProgress != currentSession.AllowJoinInProgress)
            {
                SessionModificationSetJoinInProgressAllowedOptions jipOptions = new SessionModificationSetJoinInProgressAllowedOptions();
                jipOptions.AllowJoinInProgress = session.AllowJoinInProgress;

                result = sessionModificationHandle.SetJoinInProgressAllowed(ref jipOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to set 'join in progress allowed' flag. Error code:: {0}", result);
                    sessionModificationHandle.Release();
                    return false;
                }

                //Update local cache
                currentSession.AllowJoinInProgress = session.AllowJoinInProgress;
            }

            AttributeData attributeData = new AttributeData();

            SessionModificationAddAttributeOptions attrOptions = new SessionModificationAddAttributeOptions();

            foreach (SessionAttribute nextAttribute in session.Attributes)
            {
                // Check if attribute changed

                SessionAttribute attributeFound = currentSession.Attributes.Find(x => string.Equals(x.Key, nextAttribute.Key, StringComparison.OrdinalIgnoreCase));

                if (attributeFound != null && attributeFound == nextAttribute)
                {
                    // attributes are equal, skip
                    continue;
                }

                attributeData.Key = nextAttribute.Key;

                switch (nextAttribute.ValueType)
                {
                    case AttributeType.Boolean:
                        attributeData.Value = new AttributeDataValue()
                        {
                            AsBool = nextAttribute.AsBool
                        };
                        break;
                    case AttributeType.Double:
                        attributeData.Value = new AttributeDataValue()
                        {
                            AsDouble = nextAttribute.AsDouble
                        };
                        break;
                    case AttributeType.Int64:
                        attributeData.Value = new AttributeDataValue()
                        {
                            AsInt64 = nextAttribute.AsInt64
                        };
                        break;
                    case AttributeType.String:
                        attributeData.Value = new AttributeDataValue()
                        {
                            AsUtf8 = nextAttribute.AsString
                        };
                        break;
                }

                attrOptions.SessionAttribute = attributeData;
                attrOptions.AdvertisementType = nextAttribute.Advertisement;

                result = sessionModificationHandle.AddAttribute(ref attrOptions);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Session Matchmaking: failed to set an attribute: {0}. Error code: {1}", nextAttribute.Key, result);
                    sessionModificationHandle.Release();
                    return false;
                }

                // Update local cache
                attributeFound.AsString = nextAttribute.AsString;
                attributeFound.Advertisement = nextAttribute.Advertisement;
            }

            if (callback != null)
            {
                UIOnSessionModified.Enqueue(callback);
            }

            UpdateSessionOptions updateOptions = new UpdateSessionOptions();
            updateOptions.SessionModificationHandle = sessionModificationHandle;
            sessionInterface.UpdateSession(ref updateOptions, null, OnUpdateSessionCompleteCallback);

            currentSession = session;
            currentSession.UpdateInProgress = true;

            sessionModificationHandle.Release();
            return true;
        }

        private void OnSessionDestroyed(string sessionName)
        {
            if (!string.IsNullOrEmpty(sessionName))
            {
                if (CurrentSessions.TryGetValue(sessionName, out Session session))
                {
                    CurrentSessions.Remove(sessionName);
                }
            }
        }

        private void OnSessionUpdateFinished(bool success, string sessionName, string sessionId, bool removeSessionOnFailure = false)
        {
            if (CurrentSessions.TryGetValue(sessionName, out Session session))
            {
                session.Name = sessionName;
                session.InitActiveSession();
                session.UpdateInProgress = false;

                if (success)
                {
                    session.Id = sessionId;
                }
                else
                {
                    if (removeSessionOnFailure)
                    {
                        CurrentSessions.Remove(sessionName);
                    }
                }
            }
        }

        private void OnSearchResultsReceived()
        {
            if (CurrentSearch == null)
            {
                Debug.LogError("Session Matchmaking (OnSearchResultsReceived): CurrentSearch is null");
                return;
            }

            Epic.OnlineServices.Sessions.SessionSearch searchHandle = CurrentSearch.GetSearchHandle();

            if (searchHandle == null)
            {
                Debug.LogError("Session Matchmaking (OnSearchResultsReceived): searchHandle is null");
                return;
            }

            var sessionSearchGetSearchResultCountOptions = new SessionSearchGetSearchResultCountOptions();
            uint numSearchResult = searchHandle.GetSearchResultCount(ref sessionSearchGetSearchResultCountOptions);

            Dictionary<Session, SessionDetails> searchResults = new Dictionary<Session, SessionDetails>();

            SessionSearchCopySearchResultByIndexOptions indexOptions = new SessionSearchCopySearchResultByIndexOptions();

            for (uint i = 0; i < numSearchResult; i++)
            {
                indexOptions.SessionIndex = i;

                Result result = searchHandle.CopySearchResultByIndex(ref indexOptions, out SessionDetails sessionHandle);

                if (result == Result.Success && sessionHandle != null)
                {
                    var sessionDetailsCopyInfoOptions = new SessionDetailsCopyInfoOptions();
                    result = sessionHandle.CopyInfo(ref sessionDetailsCopyInfoOptions, out SessionDetailsInfo? sessionInfo);

                    Session nextSession = new Session();
                    if (result == Result.Success)
                    {
                        nextSession.InitFromSessionInfo(sessionHandle, sessionInfo);
                    }
                    nextSession.SearchResults = true;
                    searchResults.Add(nextSession, sessionHandle);


                    foreach (KeyValuePair<string, Session> kvp in CurrentSessions)
                    {
                        if (kvp.Value.Id == nextSession.Id)
                        {
                            nextSession.Name = kvp.Key;
                            break;
                        }
                    }
                }
            }

            CurrentSearch.OnSearchResultReceived(searchResults);
            if (JoinPresenceSessionId.Length > 0)
            {
                SessionDetails sessionHandle = CurrentSearch.GetSessionHandleById(JoinPresenceSessionId);
                if (sessionHandle != null)
                {
                    // Clear session Id
                    JoinPresenceSessionId = string.Empty;
                    JoinSession(sessionHandle, true);
                }
                else
                {
                    AcknowledgeEventId(Result.NotFound);
                }
            }
            else
            {
                AcknowledgeEventId(Result.NotFound);
            }
        }

        private void OnJoinSessionFinished(Action<Result> callback)
        {
            if (JoiningSessionDetails != null)
            {
                var sessionDetailsCopyInfoOptions = new SessionDetailsCopyInfoOptions();
                Result result = JoiningSessionDetails.CopyInfo(ref sessionDetailsCopyInfoOptions, out SessionDetailsInfo? sessionInfo);

                if (result == Result.Success)
                {
                    Session session = new Session();
                    session.Name = GenerateJoinedSessionName(true);
                    session.InitFromSessionInfo(JoiningSessionDetails, sessionInfo);

                    // Check if we have a local session with same ID
                    bool localSessionFound = false;
                    foreach (Session currentSession in CurrentSessions.Values)
                    {
                        if (currentSession.Id == session.Id)
                        {
                            localSessionFound = true;
                            break;
                        }
                    }

                    if (!localSessionFound)
                    {
                        CurrentSessions[session.Name] = session;
                    }
                }
                callback?.Invoke(result);
            }
        }

        // private void OnSessionStarted(string name) // Not needed for C# Wrapper

        // private void OnSessionEnded(string name) // Not needed for C# Wrapper
        //TODO: move this somewhere more general purpose
        public static void SetJoinInfo(string joinInfo, bool onLoggingOut = false)
        {
            EpicAccountId userId = EOSManager.Instance.GetLocalUserId();

            if (userId?.IsValid() != true)
            {
                Debug.LogError("Session Matchmaking (SetJoinInfo): Current player is invalid");
                return;
            }

            PresenceInterface presenceInterface = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();

            CreatePresenceModificationOptions createModOptions = new CreatePresenceModificationOptions();
            createModOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();

            Result result = presenceInterface.CreatePresenceModification(ref createModOptions, out PresenceModification presenceModification);
            if (result != Result.Success)
            {
                if(onLoggingOut)
                {
                    Debug.LogWarning("EOSSessionsManager.SetJoininfo: Create presence modification during logOut, ignore.");
                    return;
                }
                else
                {
                    Debug.LogErrorFormat("EOSSessionsManager.SetJoininfo: Create presence modification failed: {0}", result);
                    return;
                }
            }

            PresenceModificationSetJoinInfoOptions joinOptions = new PresenceModificationSetJoinInfoOptions();
            if (string.IsNullOrEmpty(joinInfo))
            {
                // Clear JoinInfo string if there is no local sessionId
                joinOptions.JoinInfo = null;
            }
            else
            {
                // Use local sessionId to build JoinInfo string to share with friends
                joinOptions.JoinInfo = joinInfo;
            }

            result = presenceModification.SetJoinInfo(ref joinOptions);
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (SetJoinInfo): SetJoinInfo failed: {0}", result);
                return;
            }

            SetPresenceOptions setOptions = new SetPresenceOptions();
            setOptions.LocalUserId = userId;
            setOptions.PresenceModificationHandle = presenceModification;

            presenceInterface.SetPresence(ref setOptions, null, OnSetPresenceCompleteCallback);

            presenceModification.Release();
        }

        private void OnJoinGameAcceptedByJoinInfo(string joinInfo, ulong uiEventId)
        {
            JoinUiEvent = uiEventId;

            if (joinInfo.Contains("SessionId")) // TODO: Validate with Regex, this probably won't work
            {
                if (joinInfo.Length == 2)
                {
                    JoinPresenceSessionById(joinInfo.Substring(1, 1));
                    return;
                }
            }

            AcknowledgeEventId(Result.UnexpectedError);
            Debug.LogErrorFormat("Session Matchmaking (OnJoinGameAccepted): unable to parse location string: {0}", joinInfo);
        }

        private void OnJoinGameAcceptedByEventId(ulong uiEventId)
        {
            SessionDetails eventSession = MakeSessionHandleByEventId(uiEventId);
            if (eventSession != null)
            {
                JoinSession(eventSession, true);
            }
            else
            {
                JoinUiEvent = uiEventId;
                AcknowledgeEventId(Result.UnexpectedError);
                Debug.LogErrorFormat("Session Matchmaking (OnJoinGameAcceptedByEventId): unable to get details for event ID: {0}", uiEventId);
            }
        }

        private void JoinPresenceSessionById(string sessionId)
        {
            JoinPresenceSessionId = sessionId;
            Debug.LogFormat("Session Matchmaking (JoinPresenceSessionById): looking for session ID: {0}", JoinPresenceSessionId);
            SearchById(JoinPresenceSessionId);
        }

        private void AcknowledgeEventId(Result result)
        {
            if (JoinUiEvent != 0)
            {
                AcknowledgeEventIdOptions options = new AcknowledgeEventIdOptions();
                options.UiEventId = JoinUiEvent;
                options.Result = result;

                UIInterface uiInterface = EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface();
                uiInterface.AcknowledgeEventId(ref options);

                JoinUiEvent = 0;
            }
        }

        private string GenerateJoinedSessionName(bool noIncrement = false)
        {
            if (!noIncrement)
            {
                JoinedSessionIndex = (JoinedSessionIndex + 1) & JOINED_SESSION_NAME_ROTATION_NUM;
            }

            return string.Format("{0}{1}", JOINED_SESSION_NAME, JoinedSessionIndex);
        }

        private void OnUpdateSessionCompleteCallback(ref UpdateSessionCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnStartSessionCompleteCallback): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                OnSessionUpdateFinished(false, data.SessionName, data.SessionId);
                Debug.LogErrorFormat("Session Matchmaking (OnUpdateSessionCompleteCallback): error code: {0}", data.ResultCode);
            }
            else
            {
                OnSessionUpdateFinished(true, data.SessionName, data.SessionId);
                Debug.Log("Session Matchmaking: game session updated successfully.");

                if (UIOnSessionModified.Count > 0)
                {
                    UIOnSessionModified.Dequeue().Invoke();
                }
            }
        }

        private void OnUpdateSessionCompleteCallback_ForCreate(ref UpdateSessionCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnUpdateSessionCompleteCallback_ForCreate): data is null");
            //    return;
            //}

            bool removeSession = true;
            bool success = (data.ResultCode == Result.Success);

            if (success)
            {
                ProductUserId prodUserId = EOSManager.Instance.GetProductUserId();

                if (prodUserId != null)
                {
                    // Register session owner
                    Register(data.SessionName, prodUserId);
                    removeSession = false;
                }
                else
                {
                    Debug.LogError("Session Matchmaking (OnUpdateSessionCompleteCallback_ForCreate): player is null, can't register yourself in created session.");
                }
            }
            else
            {
                Debug.LogErrorFormat("Session Matchmaking (OnUpdateSessionCompleteCallback): error code: {0}", data.ResultCode);
            }

            if (UIOnSessionCreated.Count > 0)
            {
                UIOnSessionCreated.Dequeue().Invoke();
            }

            OnSessionUpdateFinished(success, data.SessionName, data.SessionId, removeSession);
        }

        private void OnStartSessionCompleteCallBack(ref StartSessionCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnStartSessionCompleteCallback): data is null");
            //    return;
            //}

            if (data.ClientData == null)
            {
                Debug.LogError("Session Matchmaking (OnStartSessionCompleteCallback): data.ClientData is null");
                return;
            }

            string sessionName = (string)data.ClientData;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnStartSessionCompleteCallback): session name: '{0}' error code: {1}", sessionName, data.ResultCode);
                return;
            }

            Debug.LogFormat("Session Matchmaking(OnStartSessionCompleteCallback): Started session: {0}", sessionName);

            //OnSessionStarted(sessionName); // Needed for C# wrapper?
        }

        private void OnEndSessionCompleteCallback(ref EndSessionCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnEndSessionCompleteCallback): data is null!");
            //    return;
            //}

            string sessionName = (string)data.ClientData;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnEndSessionCompleteCallback): session name: '{0}' error code: {1}", sessionName, data.ResultCode);
                return;
            }

            Debug.LogFormat("Session Matchmaking(OnEndSessionCompleteCallback): Ended session: {0}", sessionName);

            //OnSessionEnded(sessionName); // Not used in C# wrapper
        }

        private void OnDestroySessionCompleteCallback(ref DestroySessionCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnDestroySessionCompleteCallback): data is null!");
            //    return;
            //}

            if (data.ClientData == null)
            {
                Debug.LogError("Session Matchmaking (OnDestroySessionCompleteCallback): data.ClientData is null!");
                return;
            }

            string sessionName = (string)data.ClientData;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnDestroySessionCompleteCallback): error code: {0}", data.ResultCode);
                return;
            }

            if (!string.IsNullOrEmpty(sessionName))
            {
                OnSessionDestroyed(sessionName);
            }
        }

        private void OnRegisterCompleteCallback(ref RegisterPlayersCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnRegisterCompleteCallback): data is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnRegisterCompleteCallback): error code: {0}", data.ResultCode);
                return;
            }
        }

        private void OnUnregisterCompleteCallback(ref UnregisterPlayersCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnUnregisterCompleteCallback): data is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnUnregisterCompleteCallback): error code: {0}", data.ResultCode);
                return;
            }
        }

        private void OnFindSessionsCompleteCallback(ref SessionSearchFindCallbackInfo data)
        {
            //if (data == null)
            //{
            //    AcknowledgeEventId(Result.UnexpectedError);
            //    Debug.LogError("Session Matchmaking (OnFindSessionsCompleteCallback): data is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                AcknowledgeEventId(data.ResultCode);
                Debug.LogErrorFormat("Session Matchmaking (OnFindSessionsCompleteCallback): error code: {0}", data.ResultCode);
                return;
            }

            OnSearchResultsReceived();
        }

        private void OnSendInviteCompleteCallback(ref SendInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnSendInviteCompleteCallback): data is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnSendInviteCompleteCallback): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Session Matchmaking: invite to session sent successfully.");
        }

        public void OnSessionInviteReceivedListener(ref SessionInviteReceivedCallbackInfo data) // OnSessionInviteReceivedCallback
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnSessionInviteReceivedListener): data is null!");
            //    return;
            //}

            Debug.LogFormat("Session Matchmaking: invite to session received. Invite id: {0}", data.InviteId);

            SessionDetails sessionDetails = MakeSessionHandleByInviteId(data.InviteId);

            if (sessionDetails == null)
            {
                Debug.LogErrorFormat("Session Matchmaking (OnSessionInviteReceivedListener): Could not copy session information for invite id {0}", data.InviteId);
                return;
            }

            Session inviteSession = new Session();
            if (inviteSession.InitFromInfoOfSessionDetails(sessionDetails))
            {
                SetInviteSession(inviteSession, sessionDetails);

                // Show invite popup
                Debug.LogFormat("Session Matchmaking (OnSessionInviteReceivedListener): Invite received id = {0}", data.InviteId);
            }
            else
            {
                Debug.LogErrorFormat("Session Matchmaking (OnSessionInviteReceivedListener): Could not copy session information for invite id {0}", data.InviteId);
            }
        }

        private void PopLobbyInvite()
        {
            if (CurrentInvite != null)
            {
                Invites.Remove(CurrentInvite);
                CurrentInvite = null;
            }

            if (Invites.Count > 0)
            {
                var nextInvite = Invites.GetEnumerator();
                nextInvite.MoveNext();
                CurrentInvite = nextInvite.Current.Key;
            }
        }

        public void OnSessionInviteAcceptedListener(ref SessionInviteAcceptedCallbackInfo data) // OnSessionInviteAcceptedCallback
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnSessionInviteAcceptedListener): data is null!");
            //    return;
            //}

            Debug.Log("Session Matchmaking: joined session successfully.");

            OnJoinSessionFinished(null);
        }

        private void OnJoinSessionListener(ref JoinSessionCallbackInfo data) // OnJoinSessionCallback
        {
            var callback = data.ClientData as Action<Result>;
            //if (data == null)
            //{
            //    AcknowledgeEventId(Result.UnexpectedError);
            //    Debug.LogError("Session Matchmaking (OnJoinSessionListener): data is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                AcknowledgeEventId(data.ResultCode);
                Debug.LogErrorFormat("Session Matchmaking (OnJoinSessionListener): error code: {0}", data.ResultCode);
                callback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Session Matchmaking: joined session successfully.");

            // Add joined session to list of current sessions
            OnJoinSessionFinished(callback);

            AcknowledgeEventId(data.ResultCode);
        }

        public void OnJoinGameAcceptedListener(ref JoinGameAcceptedCallbackInfo data) // OnPresenceJoinGameAcceptedListener
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnJoinGameAcceptedListener): data is null!");
            //    return;
            //}

            Debug.Log("Session Matchmaking: join game accepted successfully.");

            OnJoinGameAcceptedByJoinInfo(data.JoinInfo, data.UiEventId);
        }

        public void OnJoinSessionAcceptedListener(ref JoinSessionAcceptedCallbackInfo data) // OnSessionsJoinSessionAcceptedCallback
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnJoinSessionAcceptedListener): data is null!");
            //    return;
            //}

            Debug.Log("Session Matchmaking: join game accepted successfully.");

            OnJoinGameAcceptedByEventId(data.UiEventId);
        }

        private static void OnSetPresenceCompleteCallback(ref SetPresenceCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Session Matchmaking (OnSetPresenceCallback): EOS_Presence_SetPresenceCallbackInfo is null");
            //}
            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("EOSSessionsManager.OnSetPresenceCallback: error code: {0}", data.ResultCode);
            }
            else
            {
                Debug.Log("EOSSessionsManager.OnSetPresenceCallback: set presence successfully.");
            }
        }

        public void AcceptLobbyInvite(bool invitePresenceToggled)
        {
            if (CurrentInvite != null && Invites.TryGetValue(CurrentInvite, out SessionDetails sessionHandle))
            {
                JoinSession(sessionHandle, invitePresenceToggled);
                PopLobbyInvite();
            }
            else
            {
                Debug.LogError("Session Matchmaking (AcceptLobbyInvite): CurrentInvite not found.");
            }
        }

        public void DeclineLobbyInvite()
        {
            PopLobbyInvite();
        }
    }
}
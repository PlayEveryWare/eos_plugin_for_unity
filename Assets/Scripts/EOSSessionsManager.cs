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

// Uncomment this define, or define it in build symbols, to get non-error debugging messages from this Manager
// #define ENABLE_DEBUG_SESSIONS_MANAGER

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Sessions;
    using Epic.OnlineServices.Presence;
    using Epic.OnlineServices.UI;
    using Epic.OnlineServices.P2P;
    using System.Text.RegularExpressions;
    using System.Text;

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
    /// Class represents a single session attribute.
    /// 
    /// TODO: When used for Search, this structure only supports Equality options.
    /// Should use all comparisons available in https://dev.epicgames.com/docs/game-services/lobbies-and-sessions/sessions#configuring-for-attribute-data
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

            if (sessionDetailsInfo.HasValue)
            {
                NumConnections = MaxPlayers - sessionDetailsInfo.Value.NumOpenPublicConnections;
            }
            else
            {
                EOSSessionsManager.Log($"{nameof(Session)} ({nameof(InitFromSessionInfo)}): SessionDetailsInfo was null, therefore unable to determine current player count.");
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
                    Debug.LogError($"{nameof(Session)} ({nameof(InitActiveSession)}): could not get ActiveSession for name: {Name}");
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

        /// <summary>
        /// Determines if the local user owns this Session.
        /// This information can be used to gate some Session modifying actions.
        /// </summary>
        /// <returns>
        /// True if the Session is owned by the local user.
        /// False if either it was determined that the user doesn't own this Session,
        /// or if the operation to query that information failed.
        /// </returns>
        public bool IsLocalUserOwnerOfSession()
        {
            ProductUserId localUserId = EOSManager.Instance?.GetProductUserId();

            // If we can't even get the local user's id, that state suggests they shouldn't be owning any Sessions
            if (localUserId == null)
            {
                return false;
            }

            // If the ActiveSession isn't set, then this user isn't a part of the Session
            // Can't possibly own it
            if (ActiveSession == null)
            {
                return false;
            }

            ActiveSessionCopyInfoOptions options = new ActiveSessionCopyInfoOptions() { };
            Result copyResult = ActiveSession.CopyInfo(ref options, out ActiveSessionInfo? sessionInfo);

            if (copyResult != Result.Success || !sessionInfo.HasValue || !sessionInfo.Value.SessionDetails.HasValue)
            {
                return false;
            }

            return sessionInfo.Value.SessionDetails.Value.OwnerUserId.Equals(localUserId);
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

        /// <summary>
        /// When joining a Session you did not create, the Session's name is not shared with you.
        /// Instead the Session is given a local name, starting with this, when <see cref="GenerateJoinedSessionName(bool)"/> is called.
        /// This local name doesn't have to follow any particular formatting, but does need to be locally unique and consistent.
        /// </summary>
        private const string JOINED_SESSION_NAME = "Session#";
        private const uint JOINED_SESSION_NAME_ROTATION_NUM = 9;
        private const int JOINED_SESSION_NAME_ROTATION = 9;

        /// <summary>
        /// The Bucket Id to create Sessions under, and to search by when finding Sessions.
        /// Bucket Ids are used to separate search results, operating as a major filter option.
        /// Your application could use different Bucket Ids for Sessions. If you do,
        /// then you will need to search by those Bucket Ids in order to find those Sessions online.
        /// 
        /// https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-session-modification-set-bucket-id
        /// </summary>
        private const string BUCKET_ID = "SessionSample:Region";

        /// <summary>
        /// When creating a Session or searching for one, this is the attribute used to filter for buckets.
        /// In these sample functions, <see cref="CreateSession(Session, bool, Action)"/> sets the BucketId
        /// in <see cref="CreateSessionModificationOptions"/> as well as an <see cref="SessionAttribute"/>.
        /// </summary>
        private const string EOS_SESSIONS_SEARCH_BUCKET_ID = "bucket";

        // TODO: All three of these are unused, remove?
        private const string EOS_SESSIONS_SEARCH_EMPTY_SERVERS_ONLY = "emptyonly";
        private const string EOS_SESSIONS_SEARCH_NONEMPTY_SERVERS_ONLY = "nonemptyonly";
        private const string EOS_SESSIONS_SEARCH_MINSLOTSAVAILABLE = "minslotsavailable";

        // UI Parameterized Callbacks
        // TODO: None of these are subscribed to, several of them aren't implemented fully. Remove? Implement more fully?
        // UIOnSessionCreated is actually utilized
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

        #region Peer2Peer Messaging Variables

        /* The Sessions Interface does not provide a way to notify Session Owners and Members when things change,
         * such as users joining, leaving, or the session starting, ending, being modified, or bieng destroyed.
         * This set of constants is used for sending Peer2Peer messages to update users.
         * In order to receive Peer 2 Peer messages, the Notifications for requests must be subscribed to, and cleaned up when exiting Peer 2 Peer messaging areas.
         * */

        /// <summary>
        /// When sending or receiving Peer2Peer messages relating to Session Status upkeep, always use this same socket name.
        /// The combination of this and <see cref="P2P_SESSION_STATUS_UPDATE_CHANNEL"/> is used to receive and send messages.
        /// This should be distinct from any other sockets used in the program, because the socket id is used while accepting incoming peer requests.
        /// </summary>
        private const string P2P_SESSION_STATUS_SOCKET_NAME = "SESSIONSTATUS";

        /// <summary>
        /// When sending or receiving Peer2Peer messages relating to Session status upkeep, always use this same channel.
        /// This should be a distinct and unique channel, different than any other channel utilized in the program, if possible.
        /// Because of the nature of <see cref="P2PInterface.GetNextReceivedPacketSize(ref GetNextReceivedPacketSizeOptions, out uint)"/>,
        /// collisions in using the same channel in another point in the program would require you to filter out things not meant for the appropriate socket.
        /// </summary>
        private const byte P2P_SESSION_STATUS_UPDATE_CHANNEL = 0xF;

        /// <summary>
        /// Messages about sessions should all start with this message, so that parsing the message for information is easy.
        /// This is part of <see cref="P2P_INFORM_SESSION_MESSAGE_FORMAT"/>, which is then formatted to form messages that are sent.
        /// The start of received messages are checked for starting with this text to know that it's something intended for managing session updates.
        /// </summary>
        private const string P2P_INFORM_SESSION_MESSAGE_BASE = "SESSIONINFORMATION";

        /// <summary>
        /// Messages about sessions follow this format, starting with <see cref="P2P_INFORM_SESSION_MESSAGE_BASE"/>.
        /// {0} - The EOS Game Services <see cref="Session.Id"/> of the session to message about.
        /// {1} - The <see cref="ProductUserId"/> of the user sending the message.
        /// {2} - Additional information about the message.
        /// <see cref="P2P_JOINING_SESSION_MESSAGE_ELEMENT"/>
        /// <see cref="P2P_LEAVING_SESSION_MESSAGE_ELEMENT"/>
        /// <see cref="P2P_REFRESH_SESSION_MESSAGE_ELEMENT"/>
        /// <see cref="P2P_SESSION_OWNER_DESTROYED_SESSION_MESSAGE_ELEMENT"/>
        /// </summary>
        private const string P2P_INFORM_SESSION_MESSAGE_FORMAT = P2P_INFORM_SESSION_MESSAGE_BASE + " ({0}) ({1}) ({2})";

        /// <summary>
        /// Messages with this as the {2} parameter of <see cref="InformSessionOwnerMessageFormat"/> indicate a user is joining the session.
        /// </summary>
        private const string P2P_JOINING_SESSION_MESSAGE_ELEMENT = "JOIN";

        /// <summary>
        /// Messages with this as the {2} parameter of <see cref="P2P_INFORM_SESSION_MESSAGE_FORMAT"/> indicate a user is leaving the session.
        /// </summary>
        private const string P2P_LEAVING_SESSION_MESSAGE_ELEMENT = "LEAVE";

        /// <summary>
        /// Messages with this as the {2} parameter of <see cref="P2P_INFORM_SESSION_MESSAGE_FORMAT"/> indicate that a user should re-acquire and refresh session information.
        /// </summary>
        private const string P2P_REFRESH_SESSION_MESSAGE_ELEMENT = "REFRESH";

        /// <summary>
        /// Messages with this as the {2} parameter of <see cref="P2P_INFORM_SESSION_MESSAGE_FORMAT"/> indicate that the owner of a session has destroyed the session,
        /// so members of the session should also remove themselves from the session.
        /// </summary>
        private const string P2P_SESSION_OWNER_DESTROYED_SESSION_MESSAGE_ELEMENT = "DESTROY";

        /// <summary>
        /// When subscribing to peer request connection messages, this Id is held as a way to later remove the subscription.
        /// This one notification id will handle all incoming peer requests on the <see cref="P2P_SESSION_STATUS_SOCKET_NAME"/> socket.
        /// You should only be subscribed once for these notifications.
        /// </summary>
        private ulong P2PSessionPeerRequestConnectionNotificationId { get; set; }

        /// <summary>
        /// When subscribing to peer disconnection messages, this Id is held as a way to later remove the subscription.
        /// This one notification id will handle all incoming peer disconnections on the <see cref="P2P_SESSION_STATUS_SOCKET_NAME"/> socket.
        /// You should only be subscribed once for these notifications.
        /// </summary>
        private ulong P2PSessionPeerDisconnectConnectionNotificationId { get; set; }

        #endregion

        #region Peer2Peer UX Variables

        /* In order to update a local Session, it must be "Searched" for.
         * Then when retrieving the updated Session from Epic Online Services, the Action in this region can be used to inform the UI.
         * */

        /// <summary>
        /// When using <see cref="RefreshSession(string)"/>, this is the SessionSearch used.
        /// The search is contained inside here instead of <see cref="CurrentSearch"/> to minimize collisions with any other searches.
        /// </summary>
        private SessionSearch P2PSessionRefreshSessionSearch { get; set; }

        /// <summary>
        /// After a successful <see cref="RefreshSession(string)"/> and <see cref="OnRefreshSessionFindSessionsCompleteCallback(ref SessionSearchFindCallbackInfo)"/> callback call,
        /// this is run to inform UI of the updated Session information. This should be subscribed to by your program's UI.
        /// </summary>
        public Action<Session, SessionDetails> UIOnSessionRefresh { get; set; }

        #endregion

        #region Manager Set Up And Utility

        /* This region contains meta-management for this Manager class.
         * This Manager isn't a MonoBehaviour, so its state management needs to be called by the program using it.
         * OnLoggedIn should be run *after* a successful authentication, and either OnLoggedOut or OnShutDown should be called when it's time to clean up the class,
         * and Update must be called every frame to receive messages.
         * If you implement your own manager, the actions and broadstrokes of the implementation within should be considered.
         * */

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

            P2PSessionRefreshSessionSearch = new SessionSearch();
        }

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// Errors are logged using <see cref="UnityEngine.Debug.LogError(object)"/>.
        /// If this is included, it is used to log out non-error debug messages.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [System.Diagnostics.Conditional("ENABLE_DEBUG_SESSIONS_MANAGER")]
        internal static void Log(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }

        /// <summary>
        /// Checks for new Peer2Peer messages for Session management, as well as updating the OnlineSessionState of ActiveSessions.
        /// This Manager is not a MonoBehaviour, so this function should be run by the program every update.
        /// </summary>
        /// <returns>Returns true if there are any detected changes to the Active Sessions. This can inform the UI to update.</returns>
        public bool Update()
        {
            bool stateUpdates = false;
            HandleReceivedP2PMessages();

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
                            Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(Update)}): ActiveSessionCopyInfo failed. Errors code: {result}");
                        }
                    }
                }
            }

            return stateUpdates;
        }

        /// <summary>
        /// Subscribes to invitations and Peer2Peer message requests and prepares internal states for this manager to be used.
        /// This should be called after a successful Connect login.
        /// </summary>
        public void OnLoggedIn()
        {
            if (userLoggedIn)
            {
                Debug.LogWarning($"{nameof(EOSSessionsManager)} ({nameof(OnLoggedIn)}): Already logged in.");
                return;
            }

            SubscribeToGameInvites();
            SubscribeToSessionMessageConnectionRequests();

            CurrentInvite = null;

            userLoggedIn = true;
        }

        /// <summary>
        /// Unsubscribes from invitations and Peer2Peer message requests and clears internal states.
        /// This can be run before or after a successful logout, or preparing to shut down the application.
        /// This must be called in order to remove Native notifications.
        /// After calling this function, you must call <see cref="OnLoggedIn"/> to resume full Sessions functionality.
        /// </summary>
        public void OnLoggedOut()
        {
            if (!userLoggedIn)
            {
                Debug.LogWarning($"{nameof(EOSSessionsManager)} ({nameof(OnLoggedOut)}): Not logged in.");
                return;
            }

            UnsubscribeFromGameInvites();
            UnsubscribeToSessionMessageConnectionRequests();

            LeaveAllSessions();

            CurrentSearch.Release();
            CurrentSessions.Clear();
            Invites.Clear();
            CurrentInvite = null;
            JoiningSessionDetails = null;

            userLoggedIn = false;
        }

        /// <summary>
        /// Unsubscribes from invitations and Peer2Peer message requests.
        /// This should be called before the application finishes shutting down in order to properly remove Native notifications.
        /// TODO: This function has no references, and is incomplete in its implementation. This will likely be removed.
        /// </summary>
        [Obsolete]
        private void OnShutDown()
        {
            DestroyAllSessions();
            UnsubscribeFromGameInvites();
            UnsubscribeToSessionMessageConnectionRequests();
        }

        /// <summary>
        /// Informs the <see cref="UIInterface"/> that a UI Event has been processed.
        /// Some functions in the EOS SDK return a UIEventId. When this happens, it is required that this function be called to released by the SDK.
        /// For example, if you are subscribed to <see cref="OnJoinSessionAcceptedListener(ref JoinSessionAcceptedCallbackInfo)"/>, after a successful call to <see cref="SessionsInterface.JoinSession(ref JoinSessionOptions, object, OnJoinSessionCallback)"/>,
        /// the notification function will receive a <see cref="JoinSessionAcceptedCallbackInfo.UiEventId"/>.
        /// You can use this event id to create a <see cref="SessionDetails"/> object by calling <see cref="MakeSessionHandleByEventId(ulong)"/>.
        /// After you've done so, or if you aren't going to use this handle, you must call this function to release the handle in the EOS SDK.
        /// Most of the functions in the EOS SDK that require this will mention it in the comments of the callback function. Anything with a UiEventId should have this called.
        /// 
        /// TODO: This is hard coded to only handle the <see cref="JoinUiEvent"/> case. It must be changed to also accept the event's id in order to function properly.
        /// </summary>
        /// <param name="result">A result indicating the success or failure of the event id to acknowledge.</param>
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

        /// <summary>
        /// Indicates if <see cref="OnLoggedIn"/> has been called since last not being logged in.
        /// </summary>
        public bool IsUserLoggedIn
        {
            get { return userLoggedIn; }
        }

        /// <summary>
        /// Sets the current user's Social Presence.
        /// 
        /// TODO: This is not called by this manager. It either should be utilized, or moved to another manager, perhaps some sort of EOSPresenceManager.
        /// </summary>
        /// <param name="joinInfo">If provided, this string will be displayed to users in the Social Overlay as your current presence.
        /// This can be used to indicate things like current level, looking for party status, etc.</param>
        /// <param name="onLoggingOut">Indicates if the user is currently attempting or has already logged out.
        /// If true, then if there's an error while trying to change your presence, the error will be ignored.
        /// 
        /// TODO: The error handling in both scenarios is almost identical. Perhaps an error state should be returned in the situation where this is false and an error is encountered.</param>
        public static void SetJoinInfo(string joinInfo, bool onLoggingOut = false)
        {
            EpicAccountId userId = EOSManager.Instance.GetLocalUserId();

            if (userId?.IsValid() != true)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SetJoinInfo)}): Current player is invalid");
                return;
            }

            PresenceInterface presenceInterface = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();

            CreatePresenceModificationOptions createModOptions = new CreatePresenceModificationOptions();
            createModOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();

            Result result = presenceInterface.CreatePresenceModification(ref createModOptions, out PresenceModification presenceModification);
            if (result != Result.Success)
            {
                if (onLoggingOut)
                {
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SetJoinInfo)}): Create presence modification during logOut, ignore.");
                    return;
                }
                else
                {
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SetJoinInfo)}): Create presence modification failed: {result}");
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SetJoinInfo)}): SetJoinInfo failed: {result}");
                return;
            }

            SetPresenceOptions setOptions = new SetPresenceOptions();
            setOptions.LocalUserId = userId;
            setOptions.PresenceModificationHandle = presenceModification;

            presenceInterface.SetPresence(ref setOptions, null, OnSetPresenceCompleteCallback);

            presenceModification.Release();
        }

        /// <summary>
        /// Callback for after attempting to set Presence.
        /// This is only called if <see cref="SetJoinInfo(string, bool)"/> succeeds in proceeding to setting the Presence.
        /// </summary>
        /// <param name="data"></param>
        private static void OnSetPresenceCompleteCallback(ref SetPresenceCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnSetPresenceCompleteCallback)}): error code: {data.ResultCode}");
            }
            else
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnSetPresenceCompleteCallback)}): set presence successfully.");
            }
        }

        #endregion

        #region Session Creation

        /* This section is clearly separated from others to show the entry point for creating a Session, and handling the callback.
         * */

        /// <summary>
        /// Creates a Session in Epic Online Services and makes a local copy.
        /// If all local modifications and handle retrieving succeed, <see cref="OnUpdateSessionCompleteCallback_ForCreate"/> will be called with results.
        /// Even before getting a response for creating the Session, this function sets local information about the Session for use in displaying UI in <see cref="CurrentSessions"/>.
        /// This should only be called to create a Session on Epic Online Services, not to create a local copy of a joined Session.
        /// </summary>
        /// <param name="session">An object containing information about the Session to create.
        /// Some values are expected to be set by the caller, and some values are updated when the Session is actually created.
        /// You should set the following:
        /// - <see cref="Session.MaxPlayers"/>
        /// - <see cref="Session.Name"/>
        /// - <see cref="Session.SanctionsEnabled"/>
        /// - <see cref="Session.PermissionLevel"/>
        /// - <see cref="Session.AllowJoinInProgress"/>
        /// - <see cref="Session.InvitesAllowed"/>
        /// You should also set any <see cref="SessionAttribute"/>s inside <see cref="Session.Attributes"/> that should be in the initial Session creation.
        /// <param name="presence">Determines if this created session should be tied with the local user's presence information.
        /// Only one local Session can be Presence enabled at one time.
        /// <seealso cref="CreateSessionModificationOptions.PresenceEnabled"/></param>
        /// <param name="callback">Callback to add to <see cref="UIOnSessionCreated"/>. Invoked in <see cref="OnUpdateSessionCompleteCallback_ForCreate"/>.</param>
        /// <returns>True if the configuration and attempt to create a Session succeed. Does not indicate that the Session was created successfully in Epic Online Services,
        /// only that the program was able to attempt its creation and without running in to an error.</returns>
        public bool CreateSession(Session session, bool presence = false, Action callback = null)
        {
            if (session == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): parameter 'session' cannot be null!");
                return false;
            }

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            if (sessionInterface == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): can't get sessions interface.");
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): could not create session modification. Error code: {result}");
                return false;
            }

            SessionModificationSetPermissionLevelOptions permisionOptions = new SessionModificationSetPermissionLevelOptions();
            permisionOptions.PermissionLevel = session.PermissionLevel;

            result = sessionModificationHandle.SetPermissionLevel(ref permisionOptions);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): failed to set permissions. Error code: {result}");
                sessionModificationHandle.Release();
                return false;
            }

            SessionModificationSetJoinInProgressAllowedOptions jipOptions = new SessionModificationSetJoinInProgressAllowedOptions();
            jipOptions.AllowJoinInProgress = session.AllowJoinInProgress;

            result = sessionModificationHandle.SetJoinInProgressAllowed(ref jipOptions);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): failed to set 'join in progress allowed' flag. Error code: {result}");
                sessionModificationHandle.Release();
                return false;
            }

            SessionModificationSetInvitesAllowedOptions iaOptions = new SessionModificationSetInvitesAllowedOptions();
            iaOptions.InvitesAllowed = session.InvitesAllowed;

            result = sessionModificationHandle.SetInvitesAllowed(ref iaOptions);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): failed to set invites allowed. Error code: {result}");
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): failed to set a bucket id attribute. Error code: {result}");
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
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(CreateSession)}): failed to set an attribute: {sdAttrib.Key}. Error code: {result}");
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

        /// <summary>
        /// Callback that handles Session creation.
        /// If the callback info indicates failure, the local Session information is cleared out.
        /// If it succeeds, the local user has <see cref="Register(string, ProductUserId)"/> called for it.
        /// Either way the most recent <see cref="UIOnSessionCreated"/> is called, as well as <see cref="OnSessionUpdateFinished(bool, string, string, bool)"/>.
        /// 
        /// TODO: It's strange that <see cref="UIOnSessionCreated"/> is called without indicating success, failure, or which Session was created.
        /// </summary>
        /// <param name="data">Callback information provided by Epic Online Services about the success of the operation.</param>
        private void OnUpdateSessionCompleteCallback_ForCreate(ref UpdateSessionCallbackInfo data)
        {
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
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnUpdateSessionCompleteCallback_ForCreate)}): player is null, can't register yourself in created session.");
                }
            }
            else
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnUpdateSessionCompleteCallback_ForCreate)}): error code: {data.ResultCode}");
            }

            if (UIOnSessionCreated.Count > 0)
            {
                UIOnSessionCreated.Dequeue().Invoke();
            }

            OnSessionUpdateFinished(success, data.SessionName, data.SessionId, removeSession);
        }

        #endregion

        #region Local Session Lookup and Search

        /* These functions are separated out to highlight that these functions are entirely local.
         * While they may call in to the EOS SDK, none of these functions call the EOS Game Services backend.
         * */

        /// <summary>
        /// Accessor for the most recent search executed by the UI.
        /// This is separate from <see cref="P2PSessionRefreshSessionSearch"/>.
        /// </summary>
        /// <returns>The most recent search.</returns>
        public SessionSearch GetCurrentSearch()
        {
            return CurrentSearch;
        }

        /// <summary>
        /// Accessor for all Current Session.
        /// These are all Sessions that are locally joined.
        /// </summary>
        /// <returns>A dictionary lookup of "local session name" to Session object.</returns>
        public Dictionary<string, Session> GetCurrentSessions()
        {
            return CurrentSessions;
        }

        /// <summary>
        /// Determines if any Sessions are joined.
        /// 
        /// TODO: This is unused, only detects Sessions that aren't locally owned. This should likely be removed. Ironically it doesn't determine if there are any <see cref="ActiveSession"/>s.
        /// </summary>
        /// <returns>True if a local Session has been joined.</returns>
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

        /// <summary>
        /// Determines if any local Sessions are Presence enabled.
        /// If <see cref="KnownPresenceSessionId"/> is set and the Session is still available, returns true.
        /// Otherwise uses the <see cref="SessionsInterface.CopySessionHandleForPresence(ref CopySessionHandleForPresenceOptions, out SessionDetails)"/> to attempt to find a local Presence enabled Session.
        /// If found, caches the Session's Id in <see cref="KnownPresenceSessionId"/>.
        /// 
        /// TODO: This doesn't actually function properly in the KnownPresenceSessionId scenario. The returned value is an Id, not the local name of the Session. The cache look up will never succeed, rework.
        /// </summary>
        /// <returns>Returns true if there is a local Session with Presence enabled.</returns>
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

        /// <summary>
        /// Determines if a local Session with the provided id is a Presence Session.
        /// </summary>
        /// <param name="id">The Session Id to check.</param>
        /// <returns>True if a local session is found with the id, and it is a Presence Session.</returns>
        public bool IsPresenceSession(string id)
        {
            return HasPresenceSession() && id.Equals(KnownPresenceSessionId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Tries to get a local Session that is joined with the provided name.
        /// </summary>
        /// <param name="name">The local name of the Session to try to get. This name is unique to the user.</param>
        /// <param name="session">Out parameter with the Session, if found.</param>
        /// <returns>True if a Session with the provided name is found.</returns>
        public bool TryGetSession(string name, out Session session)
        {
            if (CurrentSessions.TryGetValue(name, out session))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to get a local Session that is joined with the provided id.
        /// </summary>
        /// <param name="id">The id of the Session to try to get. This is the EOS Game Services id, shared for all users.</param>
        /// <param name="session">Out parameter with the Session, if found.</param>
        /// <returns>True if a Session with the provided name is found.</returns>
        public bool TryGetSessionById(string id, out Session session)
        {
            foreach (Session curSession in CurrentSessions.Values)
            {
                if (!curSession.Id.Equals(id))
                {
                    continue;
                }

                session = curSession;
                return true;
            }

            session = null;
            return false;
        }

        /// <summary>
        /// Gets a Session from the most recent <see cref="CurrentSearch"/> with the provided id.
        /// 
        /// TODO: This is unimplemented. Either implement fully or remove.
        /// </summary>
        /// <param name="sessionId">
        /// The id of the Session to try to get.
        /// This is the EOS Game Services id, shared for all users.
        /// Can only get results from the most recent search.
        /// </param>
        /// <returns>Session Details handle, if found.</returns>
        public SessionDetails MakeSessionHandleFromSearch(string sessionId)
        {
            // TODO if needed
            return null;
        }

        #endregion

        #region Online Session Lookup and Search

        /// <summary>
        /// Performs an online Search to look up a Session, where all attributes provided are met.
        /// <see cref="OnFindSessionsCompleteCallback"/> is called with the results.
        /// A handle for retriving results will be stored in <see cref="CurrentSearch"/>.
        /// 
        /// TODO: I feel like this should run a callback or return a Result in case of errors when trying to set up the search.
        /// TODO: Only finds up to 10 results. Should have some method for finding next sets of results.
        /// </summary>
        /// <param name="attributes">
        /// A list of Session Attributes to act as filters.
        /// </param>
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(Search)}): failed to create session search. Error code: {result}");
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(Search)}): failed to update session search with bucket id parameter. Error code: {result}");
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
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(Search)}): failed to update session search with parameter. Error code: {result}");
                    return;
                }
            }

            SessionSearchFindOptions findOptions = new SessionSearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            sessionSearchHandle.Find(ref findOptions, null, OnFindSessionsCompleteCallback);
        }

        /// <summary>
        /// Searches online for a Session with the exact Id. This can only return zero or one results.
        /// Like other online searching methods, this can only find Sessions that are marked as either Public or JoinViaPresence.
        /// It won't be able to find private invite only Sessions.
        /// <see cref="OnFindSessionsCompleteCallback"/> is called with the results.
        /// A handle for retriving results will be stored in <see cref="CurrentSearch"/>.
        /// This is the EOS Game Services id, shared for all users.
        /// 
        /// TODO: This should also have a callback or return type to indicate successful search attempt.
        /// </summary>
        /// <param name="sessionId">The session Id to search for.</param>
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SearchById)}): failed create session search. Error code: {result}");
                return;
            }

            CurrentSearch.SetNewSearch(sessionSearchHandle);

            SessionSearchSetSessionIdOptions sessionIdOptions = new SessionSearchSetSessionIdOptions();
            sessionIdOptions.SessionId = sessionId;

            result = sessionSearchHandle.SetSessionId(ref sessionIdOptions);

            if (result != Result.Success)
            {
                AcknowledgeEventId(result);
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SearchById)}): failed to update session search with session ID. Error code: {result}");
                return;
            }

            SessionSearchFindOptions findOptions = new SessionSearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            sessionSearchHandle.Find(ref findOptions, null, OnFindSessionsCompleteCallback);
        }

        /// <summary>
        /// Callback function to dispatch search results success and failure.
        /// This function doesn't access the returned Sessions from the Search,
        /// instead calling <see cref="OnSearchResultsReceived"/> to indicate search should be updated.
        /// This currently uses <see cref="CurrentSearch"/> only.
        /// </summary>
        /// <param name="data">Callback information about the success.</param>
        private void OnFindSessionsCompleteCallback(ref SessionSearchFindCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                AcknowledgeEventId(data.ResultCode);
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnFindSessionsCompleteCallback)}): error code: {data.ResultCode}");
                return;
            }

            OnSearchResultsReceived();
        }

        /// <summary>
        /// Handler method for dispatching search results for use.
        /// A handle is received from <see cref="CurrentSearch"/>, which was set when calling a Search-starting function.
        /// If <see cref="JoinPresenceSessionId"/> is set, and one of the Search results is that Id, it will be immediately joined.
        /// 
        /// TODO: This should handle any <see cref="SessionSearch"/>, not just <see cref="CurrentSearch"/>.
        /// TODO: What happens if a Presence Session is already joined, and it gets rejoined?
        /// TODO: Shouldn't log errors if no Presence Session is set.
        /// </summary>
        private void OnSearchResultsReceived()
        {
            if (CurrentSearch == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnSearchResultsReceived)}): CurrentSearch is null");
                return;
            }

            Epic.OnlineServices.Sessions.SessionSearch searchHandle = CurrentSearch.GetSearchHandle();

            if (searchHandle == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnSearchResultsReceived)}): searchHandle is null");
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

        /// <summary>
        /// Identifies a local session by its <paramref name="localSessionName"/>, gets its EOS Game Services <see cref="Session.Id"/>,
        /// and then attempts to use the Session search API to look for this Session on the Epic Online Services back end.
        /// If it is able to find it, then a UI refresh action is called to inform the UI to update the Session's displayed information.
        /// While similar to <see cref="SearchById(string)"/>, this function uses <see cref="P2PSessionRefreshSessionSearch"/> instead of <see cref="CurrentSearch"/>,
        /// and uses <see cref="OnRefreshSessionFindSessionsCompleteCallback"/> as the callback to handle the results.
        /// 
        /// TODO: This should be designed to work regardless of what the "public" status of the Session is.
        /// Currently only functions for Public and Join by Presence. Must also function for invites.
        /// </summary>
        /// <param name="localSessionName"></param>
        public void RefreshSession(string localSessionName)
        {
            // First ensure that we have this local session
            if (!TryGetSession(localSessionName, out Session localSession))
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(RefreshSession)}): Asked to refresh a Session with {nameof(localSessionName)} \"{localSessionName}\", but could not find a local Session with that name. Unable to refresh.");
                return;
            }

            if (string.IsNullOrEmpty(localSession.Id))
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(RefreshSession)}): Asked to refresh a Session with {nameof(localSessionName)} \"{localSessionName}\", but the found local Session did not have an {nameof(Session.Id)} assigned. Unable to refresh.");
                return;
            }

            Log($"{nameof(EOSSessionsManager)} ({nameof(RefreshSession)}): Requested to refresh session with local name {localSessionName} and {nameof(Session.Id)} {localSession.Id}.");

            // Clear previous search
            P2PSessionRefreshSessionSearch.Release();

            // There should be exactly one or zero results
            CreateSessionSearchOptions searchOptions = new CreateSessionSearchOptions();
            searchOptions.MaxSearchResults = 1;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            Result result = sessionInterface.CreateSessionSearch(ref searchOptions, out Epic.OnlineServices.Sessions.SessionSearch sessionSearchHandle);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(RefreshSession)}): Failed create Session search. Error code: {result}");
                AcknowledgeEventId(result);
                return;
            }

            P2PSessionRefreshSessionSearch.SetNewSearch(sessionSearchHandle);

            SessionSearchSetSessionIdOptions sessionIdOptions = new SessionSearchSetSessionIdOptions();
            sessionIdOptions.SessionId = localSession.Id;

            result = sessionSearchHandle.SetSessionId(ref sessionIdOptions);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(RefreshSession)}): Failed to update Session search with Session ID. Error code: {result}");
                AcknowledgeEventId(result);
                return;
            }

            SessionSearchFindOptions findOptions = new SessionSearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            sessionSearchHandle.Find(ref findOptions, localSessionName, OnRefreshSessionFindSessionsCompleteCallback);
        }

        /// <summary>
        /// Handles the Session search results from <see cref="P2PSessionRefreshSessionSearch"/>.
        /// Similar to <see cref="OnFindSessionsCompleteCallback(ref SessionSearchFindCallbackInfo)"/>, but tailored explicitly for refreshing existing Sessions.
        /// </summary>
        /// <param name="info">Callback information indicating success. The <see cref="SessionSearchFindCallbackInfo.ClientData"/> should contain the local Session name.</param>
        private void OnRefreshSessionFindSessionsCompleteCallback(ref SessionSearchFindCallbackInfo info)
        {
            if (info.ClientData is not string localSessionName)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): When constructing the search, the local Session name should be included in the ClientData of the Find method. Without it, the Session that should be updated cannot be determined.");
                return;
            }

            if (P2PSessionRefreshSessionSearch == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): {nameof(P2PSessionRefreshSessionSearch)} is null. This callback should not be run without this search being set.");
                return;
            }

            Epic.OnlineServices.Sessions.SessionSearch searchHandle = P2PSessionRefreshSessionSearch.GetSearchHandle();

            if (searchHandle == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): searchHandle is null");
                return;
            }

            var sessionSearchGetSearchResultCountOptions = new SessionSearchGetSearchResultCountOptions();
            uint numSearchResult = searchHandle.GetSearchResultCount(ref sessionSearchGetSearchResultCountOptions);

            if (numSearchResult == 0)
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): Search for refresh completed successfully, but found no sessions with the associated id.");
                return;
            }

            if (numSearchResult > 1)
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): Search for refresh completed successfully, but somehow found multiple Sessions. Only the first Session in the list will be used.");
            }


            SessionSearchCopySearchResultByIndexOptions indexOptions = new SessionSearchCopySearchResultByIndexOptions()
            {
                SessionIndex = 0
            };

            Result result = searchHandle.CopySearchResultByIndex(ref indexOptions, out SessionDetails sessionDetails);

            if (result != Result.Success || sessionDetails == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): Failed to copy search results. Result code {result}.");
                return;
            }

            var sessionDetailsCopyInfoOptions = new SessionDetailsCopyInfoOptions();
            result = sessionDetails.CopyInfo(ref sessionDetailsCopyInfoOptions, out SessionDetailsInfo? sessionInfo);

            if (result != Result.Success || !sessionInfo.HasValue)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): Failed to copy information out of the Session handle. Result code {result}.");
                return;
            }

            // Now that we have the EOS Game Services session information, update the existing session
            if (!TryGetSessionById(sessionInfo.Value.SessionId, out Session existingLocalSession))
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): Successfully queried Epic Online Services for Session, but was unable to find a local session with {nameof(Session.Id)} \"{sessionInfo.Value.SessionId}\".");
                return;
            }

            Log($"{nameof(EOSSessionsManager)} ({nameof(OnRefreshSessionFindSessionsCompleteCallback)}): Successfully queried Epic Online Services for Session. Attempting to update found local Session with {nameof(Session.Name)} \"{existingLocalSession.Name}\".");
            existingLocalSession.InitFromSessionInfo(sessionDetails, sessionInfo);

            UIOnSessionRefresh?.Invoke(existingLocalSession, sessionDetails);
        }

        #endregion

        #region Session Leaving

        /// <summary>
        /// Indicates that a Session should be "destroyed".
        /// This informs Epic Game Services that this local user is leaving a Session.
        /// <see cref="OnDestroySessionCompleteCallback"/> runs with results, which should then locally remove all of the Session's information.
        /// If this user is the Owner of the Session, the Session is destroyed with EOS Game Services, and every member should leave it.
        /// </summary>
        /// <param name="name">The local name of the Session to destroy.</param>
        public void DestroySession(string name)
        {
            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();

            DestroySessionOptions destroyOptions = new DestroySessionOptions();
            destroyOptions.SessionName = name;

            sessionInterface.DestroySession(ref destroyOptions, name, OnDestroySessionCompleteCallback);
        }

        /// <summary>
        /// Handles the results of a Session destruction.
        /// Uses Peer2Peer messaging to inform the Owner of a Session that this user left it,
        /// or if this user is the owner of the Session, inform the members that they should leave it.
        /// The local Session information for the Session should then be cleaned up and removed.
        /// </summary>
        /// <param name="data">Callback information about the operation.</param>
        private void OnDestroySessionCompleteCallback(ref DestroySessionCallbackInfo data)
        {
            if (data.ClientData == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnDestroySessionCompleteCallback)}): data.ClientData is null!");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnDestroySessionCompleteCallback)}): error code: {data.ResultCode}");
                return;
            }

            // Before removing the session from our local data, we need to inform the owner of the session that we've left the session, if we're not the owner
            // TODO: Validate that this gets to the members/owners of the session in time, and that we haven't already deleted the local information needed to get session information
            string sessionName = (string)data.ClientData;
            Session localSession;

            if (!TryGetSession(sessionName, out localSession) || localSession.ActiveSession == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnDestroySessionCompleteCallback)}): Could not find local Session and associated ActiveSession, so could not inform owner/members of destruction.");
                return;
            }

            ActiveSessionCopyInfoOptions copyOptions = new ActiveSessionCopyInfoOptions() { };
            Result localCopyResult = localSession.ActiveSession.CopyInfo(ref copyOptions, out ActiveSessionInfo? outActiveSessionInfo);

            // If we were unable to copy the active session's information, or it failed to populate the SessionDetails inside the ActiveSessionInfo, then we can't get the Owner
            // If we can't get the Owner, we can't determine who should be messaged, or if we are the owner of this Session
            if (localCopyResult != Result.Success || !outActiveSessionInfo.HasValue || !outActiveSessionInfo.Value.SessionDetails.HasValue)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnDestroySessionCompleteCallback)}): Failed to copy local information for session {sessionName}, so could not inform owner/members of destruction. Result code {localCopyResult}");
                return;
            }

            if (outActiveSessionInfo.Value.SessionDetails.Value.OwnerUserId.Equals(EOSManager.Instance.GetProductUserId()))
            {
                // We're the owner of the session, inform everyone that it was destroyed
                InformSessionMembers(sessionName, P2P_SESSION_OWNER_DESTROYED_SESSION_MESSAGE_ELEMENT);
            }
            else
            {
                // Inform the owner that we've left the session
                InformSessionOwnerWithMessage(sessionName, P2P_LEAVING_SESSION_MESSAGE_ELEMENT);
            }

            if (!string.IsNullOrEmpty(sessionName))
            {
                OnSessionDestroyed(sessionName);
            }
        }

        /// <summary>
        /// Removes local information about the Session.
        /// </summary>
        /// <param name="sessionName">The local Session's name to remove.</param>
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

        /// <summary>
        /// Leaves all Sessions currently joined by using <see cref="DestroySession(string)"/>.
        /// </summary>
        private void LeaveAllSessions()
        {
            // Enumerate session entries in UI
            foreach (KeyValuePair<string, Session> kvp in GetCurrentSessions())
            {
                DestroySession(kvp.Key);
            }
        }

        /// <summary>
        /// Leaves all Sessions currently joined by using <see cref="DestroySession(string)"/>.
        /// Only applies to Sessions not owned.
        /// 
        /// TODO: This seems redundant and misleading. Should remove.
        /// </summary>
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

        #endregion

        #region Session Joining

        /// <summary>
        /// Attempts to join an online Session.
        /// The results are handled by <see cref="OnJoinSessionListener(ref JoinSessionCallbackInfo)"/>.
        /// Sets <see cref="JoiningSessionDetails"/> with the handle, so that it can be used to inform which Session was joined.
        /// </summary>
        /// <param name="sessionHandle">A handle to the Session that you want to join.</param>
        /// <param name="presenceSession">
        /// If true, then when the Session is joined it'll be set to Presence enabled.
        /// You can only have one Presence enabled Session at a time.
        /// </param>
        /// <param name="callback">
        /// Additional callback to run with the results of the operation.
        /// Executed as ClientData in <see cref="OnJoinSessionListener(ref JoinSessionCallbackInfo)"/>.
        /// </param>
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

        /// <summary>
        /// Callback handler for joining a Session.
        /// If the joining failed, invokes the <paramref name="data"/>'s ClientData as an Action<Result>.
        /// If it succeeds, calls <see cref="OnJoinSessionFinished(Action{Result})"/> with the above callback.
        /// </summary>
        /// <param name="data">Callback information indicating success.</param>
        private void OnJoinSessionListener(ref JoinSessionCallbackInfo data) // OnJoinSessionCallback
        {
            var callback = data.ClientData as Action<Result>;

            if (data.ResultCode != Result.Success)
            {
                AcknowledgeEventId(data.ResultCode);
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnJoinSessionListener)}): error code: {data.ResultCode}");
                callback?.Invoke(data.ResultCode);
                return;
            }

            Log($"{nameof(EOSSessionsManager)} ({nameof(OnJoinSessionListener)}): joined session successfully.");

            // Add joined session to list of current sessions
            OnJoinSessionFinished(callback);

            AcknowledgeEventId(data.ResultCode);
        }

        /// <summary>
        /// Run when a Session has been successfully joined.
        /// Uses the handle in <see cref="JoiningSessionDetails"/> to determine the Session that was joined.
        /// Uses Peer2Peer messages to inform the Owner that this user has joined the Session.
        /// </summary>
        /// <param name="callback">Callback to run with the status, perhaps used to inform the UI to update.</param>
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

                    InformSessionOwnerWithMessage(session.Name, P2P_JOINING_SESSION_MESSAGE_ELEMENT);
                }
                callback?.Invoke(result);
            }
        }

        /// <summary>
        /// Returns a valid Session Name for use as the local Session's name.
        /// Names are local to each user for Sessions.
        /// Manipulates <see cref="JoinedSessionIndex"/> if <paramref name="noIncrement"/> is false,
        /// then uses that to determine a Session name.
        /// 
        /// TODO: This should do validation to make sure the returned name is actually unused.
        /// </summary>
        /// <param name="noIncrement">
        /// If false, <see cref="JoinedSessionIndex"/> is manipulated before returning the name.
        /// If true, then this will return the current value of <see cref="JoinedSessionIndex"/> when generating the name.
        /// </param>
        /// <returns>A usable Session name for local use.</returns>
        private string GenerateJoinedSessionName(bool noIncrement = false)
        {
            if (!noIncrement)
            {
                JoinedSessionIndex = (JoinedSessionIndex + 1) & JOINED_SESSION_NAME_ROTATION_NUM;
            }

            return string.Format("{0}{1}", JOINED_SESSION_NAME, JoinedSessionIndex);
        }

        /// <summary>
        /// Upon successfully joining a Session by Presence, handles joining the local Session and setting Presence.
        /// 
        /// TODO: This is incorrectly implemented and will not function properly. Need to change how <paramref name="joinInfo"/> is utilized.
        /// </summary>
        /// <param name="joinInfo">
        /// Information containing a Session id to join off of.
        /// TODO: Determine an example string and post in this comment.
        /// </param>
        /// <param name="uiEventId">
        /// An Id that the EOS SDK can use to create a handle.
        /// Once used or no longer needed, call <see cref="AcknowledgeEventId(Result)"/>.
        /// <see cref="MakeSessionHandleByEventId(ulong)"/>
        /// </param>
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
            Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnJoinGameAcceptedByJoinInfo)}): unable to parse location string: {joinInfo}");
        }

        /// <summary>
        /// Sets the local <see cref="JoinPresenceSessionId"/>, and then attempts to <see cref="SearchById(string)"/>.
        /// If the indicated Session is found, it'll join it, and then set the <see cref="KnownPresenceSessionId"/> to that Id.
        /// This can only find Sessions that have their <see cref="Session.PermissionLevel"/> set to either
        /// <see cref="OnlineSessionPermissionLevel.JoinViaPresence"/> or <see cref="OnlineSessionPermissionLevel.PublicAdvertised"/>.
        /// </summary>
        /// <param name="sessionId">The EOS Game Services Session Id.</param>
        private void JoinPresenceSessionById(string sessionId)
        {
            JoinPresenceSessionId = sessionId;
            Log($"{nameof(EOSSessionsManager)} ({nameof(JoinPresenceSessionById)}): looking for session ID: {JoinPresenceSessionId}");
            SearchById(JoinPresenceSessionId);
        }

        /// <summary>
        /// After a Session is successfully joined, try to set local information about the Session,
        /// then join it.
        /// 
        /// TODO: This should probably call <see cref="AcknowledgeEventId(Result)"/> after using the handle.
        /// </summary>
        /// <param name="uiEventId">The EOS SDK's Event Id to use for creating a Session Handle.</param>
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnJoinGameAcceptedByEventId)}): unable to get details for event ID: {uiEventId}");
            }
        }

        /// <summary>
        /// Uses a provided <paramref name="uiEventId"/> to get a <see cref="SessionDetails"/> object.
        /// This will only return a value if the provided Id has not yet been called with <see cref="AcknowledgeEventId(Result)"/>.
        /// After making the Session handle, <see cref="AcknowledgeEventId(Result)"/> should be called with this id.
        /// </summary>
        /// <param name="uiEventId">The id to create off of.</param>
        /// <returns>
        /// If an event is available with the provided id, returns <see cref="SessionDetails"/> for the Session.
        /// Otherwise returns null.
        /// </returns>
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

        #region Notifications

        /// <summary>
        /// Listener notification method for when a game is joined.
        /// Upon firing, this calls <see cref="OnJoinGameAcceptedByJoinInfo(string, ulong)"/> to join the Game.
        /// 
        /// TODO: When is this called versus <see cref="OnJoinSessionAcceptedListener(ref JoinSessionAcceptedCallbackInfo)"/>?
        /// TODO: At what point does this get called in joining life cycle?
        /// TODO: What is a Game versus a Session?
        /// </summary>
        /// <param name="data">Callback information from the attempted join.</param>
        public void OnJoinGameAcceptedListener(ref JoinGameAcceptedCallbackInfo data) // OnPresenceJoinGameAcceptedListener
        {
            Debug.Log($"{nameof(EOSSessionsManager)} ({nameof(OnJoinGameAcceptedListener)}): join game accepted successfully.");

            OnJoinGameAcceptedByJoinInfo(data.JoinInfo, data.UiEventId);
        }

        /// <summary>
        /// Listener notification method for when a Session is joined.
        /// UPon firing, this calls <see cref="OnJoinGameAcceptedByEventId(ulong)"/> to join the Session.
        /// 
        /// TODO: When is this called versus <see cref="OnJoinGameAcceptedListener(ref JoinGameAcceptedCallbackInfo)"/>?
        /// TODO: At what point does this get called in a joining life cycle?
        /// </summary>
        /// <param name="data">Callback information from the attempted join.</param>
        public void OnJoinSessionAcceptedListener(ref JoinSessionAcceptedCallbackInfo data) // OnSessionsJoinSessionAcceptedCallback
        {
            Log($"{nameof(EOSSessionsManager)} ({nameof(OnJoinSessionAcceptedListener)}): join game accepted successfully.");

            OnJoinGameAcceptedByEventId(data.UiEventId);
        }

        #endregion

        #endregion

        #region Session Other State Management

        /* These methods manage the state of the Session, other than Destroying it.
         * All of the public methods will call out to EOS Game Services to make the modification,
         * then a local callback is run to make the modification.
         * Only the owner of a Session are able to modify its state.
         * 
         * With P2P Messaging enabled, these modifications will call out to members of the Session to update their state.
         * */

        /// <summary>
        /// Starts an existing Session with EOS Game Services. This is not used to create new Sessions.
        /// This is mostly used for standardizing a state across clients,
        /// but if <see cref="Session.AllowJoinInProgress"/> is set to false, then users can only join it if it hasn't been started.
        /// This will set the <see cref="Session.SessionState"/> to <see cref="OnlineSessionState.Starting"/>,
        /// and then soon after <see cref="OnlineSessionState.InProgress"/>.
        /// <see cref="Update"/> will notice the change in State and inform the UI to update.
        /// Calls to <see cref="SessionsInterface.StartSession(ref StartSessionOptions, object, OnStartSessionCallback)"/>,
        /// with the ClientData param containing the local name of the Session to start.
        /// <see cref="OnStartSessionCompleteCallBack(ref StartSessionCallbackInfo)"/> handles the callback.
        /// 
        /// TODO: Answer - can users see this if it has been ended and <see cref="Session.AllowJoinInProgress"/> is false?
        /// </summary>
        /// <param name="name">The local name of the Session to start.</param>
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
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(StartSession)}): can't start session: no active session with specified name.");
                return;
            }
        }

        /// <summary>
        /// Callback after an attempted Session start.
        /// When successful, the Session owner will attempt to inform Session members of the state change.
        /// </summary>
        /// <param name="data">
        /// Callback information about the attempted start.
        /// <see cref="StartSessionCallbackInfo.ClientData"/> must contain the local name of the Session that started.
        /// </param>
        private void OnStartSessionCompleteCallBack(ref StartSessionCallbackInfo data)
        {
            if (data.ClientData == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnStartSessionCompleteCallBack)}): data.ClientData is null");
                return;
            }

            string sessionName = (string)data.ClientData;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnStartSessionCompleteCallBack)}): session name: '{sessionName}' error code: {data.ResultCode}");
                return;
            }

            Log($"{nameof(EOSSessionsManager)} ({nameof(OnStartSessionCompleteCallBack)}): Started session: {sessionName}");

            //OnSessionStarted(sessionName); // Needed for C# wrapper?

            // ClientData should contain the local sessionName
            if (data.ClientData is string localSessionName)
            {
                InformSessionMembers(localSessionName, P2P_REFRESH_SESSION_MESSAGE_ELEMENT);
            }
        }

        /// <summary>
        /// Ends an existing Session with EOS Game Services. This is not used to leave or destroy Sessions.
        /// A Session does not need to "End" in order to be left or destroyed.
        /// This is mostly used for standardizing a state across clients,
        /// but if <see cref="Session.AllowJoinInProgress"/> is set to false, then users can only join it if it hasn't been started.
        /// This will set the <see cref="Session.SessionState"/> to <see cref="OnlineSessionState.Ending"/>,
        /// and then soon after <see cref="OnlineSessionState.Ended"/>.
        /// <see cref="Update"/> will notice the change in State and inform the UI to update.
        /// Calls to <see cref="SessionsInterface.EndSession(ref EndSessionOptions, object, OnEndSessionCallback)"/>,
        /// with the ClientData param containing the local name of the Session to end.
        /// <see cref="OnEndSessionCompleteCallback(ref EndSessionCallbackInfo)"/> handles the callback.
        /// 
        /// TODO: Answer - can users see this if it has been ended and <see cref="Session.AllowJoinInProgress"/> is false?
        /// </summary>
        /// <param name="name">The local name of the Session to end.</param>
        public void EndSession(string name)
        {
            if (!CurrentSessions.TryGetValue(name, out Session session))
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(EndSession)}): can't end session: no active session with specified name: {name}");
                return;
            }

            EndSessionOptions endOptions = new EndSessionOptions();
            endOptions.SessionName = name;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.EndSession(ref endOptions, name, OnEndSessionCompleteCallback);
        }

        /// <summary>
        /// Callback after an attempted Session ending.
        /// When successful, the Session owner will attempt to inform Session members of the state change.
        /// </summary>
        /// <param name="data">
        /// Callback information about the attempted end operation.
        /// <see cref="EndSessionCallbackInfo.ClientData"/> must contain the local name of the Session that ended.
        /// </param>
        private void OnEndSessionCompleteCallback(ref EndSessionCallbackInfo data)
        {
            string sessionName = (string)data.ClientData;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnEndSessionCompleteCallback)}): session name: '{sessionName}' error code: {data.ResultCode}");
                return;
            }

            Log($"{nameof(EOSSessionsManager)} ({nameof(OnEndSessionCompleteCallback)}): Ended session: {sessionName}");

            //OnSessionEnded(sessionName); // Not used in C# wrapper

            // ClientData should contain the local sessionName
            if (data.ClientData is string localSessionName)
            {
                InformSessionMembers(localSessionName, P2P_REFRESH_SESSION_MESSAGE_ELEMENT);
            }
        }

        /// <summary>
        /// Registers a user to the Session.
        /// Users will be able to query the identities of Registered members in a Session,
        /// and Registering is required for EOS Game Services to manage Maximum Users configurations.
        /// This should be called by the owner of a Session. They are informed to do so with P2P messages.
        /// </summary>
        /// <param name="sessionName">The name of the local session to register the user to.</param>
        /// <param name="userIdToRegister">
        /// The ProductUserId of the user to Register to the Session.
        /// Uses <paramref name="sessionName"/> for ClientData to identify the Session to register the user to.
        /// </param>
        public void Register(string sessionName, ProductUserId userIdToRegister)
        {
            RegisterPlayersOptions registerOptions = new RegisterPlayersOptions();
            registerOptions.SessionName = sessionName;
            registerOptions.PlayersToRegister = new ProductUserId[] { userIdToRegister };

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.RegisterPlayers(ref registerOptions, sessionName, OnRegisterCompleteCallback);
        }

        /// <summary>
        /// Callback handler for an attempt to register a user to the Session.
        /// </summary>
        /// <param name="data">
        /// Callback info from the attempt.
        /// ClientData should contain the local Session name.
        /// </param>
        private void OnRegisterCompleteCallback(ref RegisterPlayersCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnRegisterCompleteCallback)}): error code: {data.ResultCode}");
                return;
            }

            // ClientData should contain the local sessionName
            if (data.ClientData is string localSessionName)
            {
                // Refresh the owner's local UI, and also inform members
                RefreshSession(localSessionName);
                InformSessionMembers(localSessionName, P2P_REFRESH_SESSION_MESSAGE_ELEMENT);
            }
        }

        /// <summary>
        /// Unregisters a user from a Session.
        /// This will then inform the user to leave the room with P2P messages.
        /// An unregistered user will no longer be part of the Session with EOS Game Services.
        /// Uses <paramref name="sessionName"/> as the ClientData to identify the local Session name to unregister the user from.
        /// </summary>
        /// <param name="sessionName">Local session name to unregister the user from.</param>
        /// <param name="userIdToUnRegister">
        /// The ProductUserId of the user to unregister.
        /// </param>
        public void UnRegister(string sessionName, ProductUserId userIdToUnRegister)
        {
            UnregisterPlayersOptions unregisterOptions = new UnregisterPlayersOptions();
            unregisterOptions.SessionName = sessionName;
            unregisterOptions.PlayersToUnregister = new ProductUserId[] { userIdToUnRegister };

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.UnregisterPlayers(ref unregisterOptions, sessionName, OnUnregisterCompleteCallback);
        }

        /// <summary>
        /// Callback handler for an attempt to uregister a user to the Session.
        /// </summary>
        /// <param name="data">
        /// Callback info from the attempt.
        /// ClientData should contain the local Session name.
        /// </param>
        private void OnUnregisterCompleteCallback(ref UnregisterPlayersCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnUnregisterCompleteCallback)}): error code: {data.ResultCode}");
                return;
            }

            // ClientData should contain the local sessionName
            if (data.ClientData is string localSessionName)
            {
                // Refresh the owner's local UI, and also inform members
                RefreshSession(localSessionName);
                InformSessionMembers(localSessionName, P2P_REFRESH_SESSION_MESSAGE_ELEMENT);
            }
        }

        /// <summary>
        /// Saturates an attempt to modify a Session that this user is the owner of.
        /// Assumes there is a local Session present in <see cref="CurrentSessions"/>,
        /// which is a different object than the <paramref name="session"/>.
        /// By looking up the Session in the EOS SDK with a matching local Session name,
        /// and comparing it to the changes in <paramref name="session"/>,
        /// this calls to <see cref="SessionsInterface.UpdateSession(ref UpdateSessionOptions, object, OnUpdateSessionCallback)"/>.
        /// 
        /// TODO: I really don't like that this assumes you have an entirely different Session object,
        /// or that it's unsafe to modify the Session object if you ever have a reference to it.
        /// This should probably take in a different method for modifying the Session.
        /// </summary>
        /// <param name="session">
        /// A new Session object that can be used to compare to the local Session to determine changes.
        /// Should be a different object than the existing local Session.
        /// </param>
        /// <param name="callback">Optional callback to run after an update.</param>
        /// <returns>
        /// True if successfully attempting to change the Session.
        /// Does not indicate that the modification was successful, just that the attempt to create a modification was successful.
        /// </returns>
        public bool ModifySession(Session session, Action callback = null)
        {
            if (session == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): pamater session is null.");
                return false;
            }

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();

            if (sessionInterface == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): can't get sessions interface.");
                return false;
            }

            if (!CurrentSessions.TryGetValue(session.Name, out Session currentSession))
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): can't modify session: no active session with specified name.");
                return false;
            }

            UpdateSessionModificationOptions updateModOptions = new UpdateSessionModificationOptions();
            updateModOptions.SessionName = session.Name;

            Result result = sessionInterface.UpdateSessionModification(ref updateModOptions, out SessionModification sessionModificationHandle);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): failed create session modification. Error code: {result}");
                return false;
            }

            // Max Players
            if (session.MaxPlayers != currentSession.MaxPlayers)
            {
                SessionModificationSetMaxPlayersOptions maxPlayerOptions = new SessionModificationSetMaxPlayersOptions();
                maxPlayerOptions.MaxPlayers = session.MaxPlayers;

                result = sessionModificationHandle.SetMaxPlayers(ref maxPlayerOptions);

                if (result != Result.Success)
                {
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): failed to set maxp layers. Error code: {result}");
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
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): failed to set permission level. Error code: {result}");
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
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): failed to set 'join in progress allowed' flag. Error code: {result}");
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
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(ModifySession)}): failed to set an attribute: {nextAttribute.Key}. Error code: {result}");
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

        /// <summary>
        /// Callback after an attempting a Session modification.
        /// Dispatches the call to <see cref="OnSessionUpdateFinished(bool, string, string, bool)"/>.
        /// 
        /// TODO: This pops <see cref="UIOnSessionModified"/> if successful, but not if in failure state.
        /// This can easily get that queue in to an error state. Pop either way with a result?
        /// TODO: Rename to something more clearly tying to <see cref="ModifySession(Session, Action)"/>.
        /// </summary>
        /// <param name="data">Callback information about the update attempt.</param>
        private void OnUpdateSessionCompleteCallback(ref UpdateSessionCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                OnSessionUpdateFinished(false, data.SessionName, data.SessionId);
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnUpdateSessionCompleteCallback)}): error code: {data.ResultCode}");
            }
            else
            {
                OnSessionUpdateFinished(true, data.SessionName, data.SessionId);
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnUpdateSessionCompleteCallback)}): game session updated successfully.");

                if (UIOnSessionModified.Count > 0)
                {
                    UIOnSessionModified.Dequeue().Invoke();
                }
            }
        }

        /// <summary>
        /// Called after an attempted Session update or creation.
        /// Informs the Session to re-update itself after the change.
        /// <see cref="ModifySession(Session, Action)"/>
        /// <see cref="CreateSession(Session, bool, Action)"/>
        /// 
        /// TODO: Should only inform Session Members on a successful edit.
        /// TODO: This is pretty unclear in what it is actually doing or updating...
        /// Can it possibly change the SessionId? Why is that passed in at all at the same time as SessionName?
        /// </summary>
        /// <param name="success">Whether the update was successful.</param>
        /// <param name="sessionName">The local Session name.</param>
        /// <param name="sessionId">The Session id that was updated.</param>
        /// <param name="removeSessionOnFailure">If true, and success is false, remove the local Session.</param>
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

                InformSessionMembers(sessionName, P2P_REFRESH_SESSION_MESSAGE_ELEMENT);
            }
        }

        #endregion

        #region Invite Management

        /* Methods in this area relate to sending and receiving Invites, which are explicit messages from users asking them to join the Session.
         * 
         * TODO: The object management in this region is pretty rough, with several easily disrupted carrier objects.
         * Variables relating to it should be reorganized and renamed.
         * */

        /// <summary>
        /// Subscribes to notifications for Game/Session invitations.
        /// This must be called for the EOS Game Services to know to send messages to you regarding being invited.
        /// You should call <see cref="UnsubscribeFromGameInvites"/> before the application finishes shutting down,
        /// or when you no longer want to listen to and respond to game invites.
        /// </summary>
        public void SubscribeToGameInvites()
        {
            if (subscribtedToGameInvites)
            {
                Debug.LogWarning($"{nameof(EOSSessionsManager)} ({nameof(SubscribeToGameInvites)}): Already subscribed.");
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

        /// <summary>
        /// Removes all notification handles relating to Game/Session Invites.
        /// </summary>
        public void UnsubscribeFromGameInvites()
        {
            if (!subscribtedToGameInvites)
            {
                Debug.LogWarning($"{nameof(EOSSessionsManager)} ({nameof(UnsubscribeFromGameInvites)}): Not subscribed yet.");
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

        /// <summary>
        /// Accessor for retrieving <see cref="Invites"/>, which are invites sent to the user.
        /// </summary>
        /// <returns>All Invites sent to the user.</returns>
        public Dictionary<Session, SessionDetails> GetInvites()
        {
            return Invites;
        }

        /// <summary>
        /// Accessor for retriving the Session that is currently being operated on in regards to invitation.
        /// </summary>
        /// <returns>Currently acted upon Session, in regards to invitation.</returns>
        public Session GetCurrentInvite()
        {
            return CurrentInvite;
        }

        /// <summary>
        /// Sends an invitation to a user with EOS Game Services to join a Session.
        /// </summary>
        /// <param name="sessionName">The local Session name to invite the user to.</param>
        /// <param name="friendId">
        /// The ProductUserId of the user to invite.
        /// TODO: Not necessarily a friend.
        /// </param>
        public void InviteToSession(string sessionName, ProductUserId friendId)
        {
            if (!friendId.IsValid())
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InviteToSession)}): friend's product user id is invalid!");
                return;
            }

            ProductUserId currentUserId = EOSManager.Instance.GetProductUserId();
            if (!currentUserId.IsValid())
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InviteToSession)}): current user's product user id is invalid!");
                return;
            }

            SendInviteOptions sendInviteOptions = new SendInviteOptions();
            sendInviteOptions.LocalUserId = currentUserId;
            sendInviteOptions.TargetUserId = friendId;
            sendInviteOptions.SessionName = sessionName;

            SessionsInterface sessionInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
            sessionInterface.SendInvite(ref sendInviteOptions, null, OnSendInviteCompleteCallback);
        }

        /// <summary>
        /// Callback handling the results of sending an invite.
        /// </summary>
        /// <param name="data">Callback information about the success of the invitation.</param>
        private void OnSendInviteCompleteCallback(ref SendInviteCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnSendInviteCompleteCallback)}): error code: {data.ResultCode}");
                return;
            }

            Log($"{nameof(EOSSessionsManager)} ({nameof(OnSendInviteCompleteCallback)}): invite to session sent successfully.");
        }

        /// <summary>
        /// Sets a specific set of Session information as the currently acted upon Session.
        /// If there are any existing Session invites, bring the most recent one up by calling <see cref="PopLobbyInvite"/>.
        /// Otherwise sets the current invitation to the one passed in to this function.
        /// The <see cref="CurrentInvite"/> is used to automatically join if a joined Session has this id.
        /// 
        /// TODO: This assumes a user is only invited once to a Session, which is not necessarily true.
        /// TODO: This has awkward management of the most recent invitation object.
        /// </summary>
        /// <param name="session">Session information to join. Mostly used for its name and id.</param>
        /// <param name="sessionDetails">
        /// Handle to additional details.
        /// 
        /// TODO: This should be readily available for any Session you've been invited to, and probably shouldn't be passed as an argument.
        /// </param>
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

        /// <summary>
        /// Creates a <see cref="SessionDetails"/> handle based on an invitation Id.
        /// This can be used to translate the provided information from an invitation to a more detailed form.
        /// After the returned object is no longer useful, call <see cref="SessionDetails.Release"/> to free the memory.
        /// </summary>
        /// <param name="inviteId">The invitation id to create a Session handle from.</param>
        /// <returns>
        /// If an invitation Id that hasn't been released yet is found, then return that.
        /// Otherwise returns null.
        /// </returns>
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

        /// <summary>
        /// Accepts the <see cref="CurrentInvite"/> and attempts to <see cref="JoinSession(SessionDetails, bool, Action{Result})"/>.
        /// 
        /// TODO: It's weird that this doesn't accept an invitation object, just directly joining the last Session to be pointed at.
        /// </summary>
        /// <param name="invitePresenceToggled">If true, the Session will be joined as the Presence Session for this user.</param>
        public void AcceptLobbyInvite(bool invitePresenceToggled)
        {
            if (CurrentInvite != null && Invites.TryGetValue(CurrentInvite, out SessionDetails sessionHandle))
            {
                JoinSession(sessionHandle, invitePresenceToggled);
                PopLobbyInvite();
            }
            else
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(AcceptLobbyInvite)}): CurrentInvite not found.");
            }
        }

        /// <summary>
        /// Declines the most recent invitation.
        /// 
        /// TODO: It's weird that this doesn't accept a specific invitation to respond to.
        /// If you're invited to two things, this will only pop the most recent one, which might not be the thing you want to decline.
        /// </summary>
        public void DeclineLobbyInvite()
        {
            PopLobbyInvite();
        }

        /// <summary>
        /// If there is a <see cref="CurrentInvite"/>, unsets and removes it from <see cref="Invites"/>.
        /// If there are any <see cref="Invites"/> remaining, sets the next one to <see cref="CurrentInvite"/>.
        /// Should be called after the <see cref="CurrentInvite"/> is "dealt with",
        /// either by accepting or rejecting it.
        /// </summary>
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

        #region Notifications

        /// <summary>
        /// Notification handler for a Session Invite being received by the local user.
        /// </summary>
        /// <param name="data">Callback information about the most recent invitation.</param>
        public void OnSessionInviteReceivedListener(ref SessionInviteReceivedCallbackInfo data) // OnSessionInviteReceivedCallback
        {
            Log($"{nameof(EOSSessionsManager)} ({nameof(OnSessionInviteReceivedListener)}): invite to session received. Invite id: {data.InviteId}");

            SessionDetails sessionDetails = MakeSessionHandleByInviteId(data.InviteId);

            if (sessionDetails == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnSessionInviteReceivedListener)}): Could not copy session information for invite id {data.InviteId}");
                return;
            }

            Session inviteSession = new Session();
            if (inviteSession.InitFromInfoOfSessionDetails(sessionDetails))
            {
                SetInviteSession(inviteSession, sessionDetails);

                // Show invite popup
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnSessionInviteReceivedListener)}): Invite received id =  {data.InviteId}");
            }
            else
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnSessionInviteReceivedListener)}): Could not copy session information for invite id {data.InviteId}");
            }
        }

        /// <summary>
        /// Notification handler for a Session Invite being accepted by the local user.
        /// </summary>
        /// <param name="data">Callback informationa bout the most recent invitation.</param>
        public void OnSessionInviteAcceptedListener(ref SessionInviteAcceptedCallbackInfo data) // OnSessionInviteAcceptedCallback
        {
            Log($"{nameof(EOSSessionsManager)} ({nameof(OnSessionInviteAcceptedListener)}): joined session successfully.");

            OnJoinSessionFinished(null);
        }

        #endregion

        #endregion

        #region Peer2Peer Messaging Functions

        /// <summary>
        /// This method subscribes to the notification channels for receiving P2P connection requests.
        /// By subscribing to the connection requests, we are able to accept those requests, and then start receiving P2P messages from the requesting user.
        /// This method must be called first, before P2P Messages can be received.
        /// The subscribed socket is <see cref="P2P_SESSION_STATUS_SOCKET_NAME"/>.
        /// This subscription should be open as long as any P2P Session management is desired.
        /// When finished, unsubscribe using <see cref="UnsubscribeToSessionMessageConnectionRequests"/>.
        /// </summary>
        private void SubscribeToSessionMessageConnectionRequests()
        {
            SocketId socketId = new SocketId()
            {
                SocketName = P2P_SESSION_STATUS_SOCKET_NAME
            };

            AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = socketId
            };

            P2PSessionPeerRequestConnectionNotificationId = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().AddNotifyPeerConnectionRequest(ref options, null, OnIncomingSessionsConnectionRequest);

            if (P2PSessionPeerRequestConnectionNotificationId == 0)
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(SubscribeToSessionMessageConnectionRequests)}): Failed to subscribe to P2P Messages, bad Notification Id was returned.");
            }

            AddNotifyPeerConnectionClosedOptions closedOptions = new AddNotifyPeerConnectionClosedOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = socketId
            };
            P2PSessionPeerDisconnectConnectionNotificationId = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().AddNotifyPeerConnectionClosed(ref closedOptions, null, OnIncomingSessionsDisconnect);
        }

        /// <summary>
        /// If subscribed, unsubscribe to P2P Session management messages.
        /// The ability to receive P2P connection requests is unsubscribed from.
        /// Closes all connections relating to the <see cref="P2P_SESSION_STATUS_SOCKET_NAME"/> socket.
        /// </summary>
        private void UnsubscribeToSessionMessageConnectionRequests()
        {
            if (P2PSessionPeerRequestConnectionNotificationId != 0)
            {
                EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().RemoveNotifyPeerConnectionRequest(P2PSessionPeerRequestConnectionNotificationId);
                P2PSessionPeerRequestConnectionNotificationId = 0;
            }

            if (P2PSessionPeerDisconnectConnectionNotificationId != 0)
            {
                EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().RemoveNotifyPeerConnectionClosed(P2PSessionPeerDisconnectConnectionNotificationId);
                P2PSessionPeerDisconnectConnectionNotificationId = 0;
            }

            CloseConnectionsOptions closeOptions = new CloseConnectionsOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(), 
                SocketId = new SocketId() 
                { 
                    SocketName = P2P_SESSION_STATUS_SOCKET_NAME 
                } 
            };

            EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().CloseConnections(ref closeOptions);
        }
        
        /// <summary>
        /// Utility function to send a message to the owner of a session.
        /// </summary>
        /// <param name="localSessionName">The local name for the session.</param>
        /// <param name="messageDetail">The message detail to send. This message detail is used to inform the owner of what action to take.</param>
        private void InformSessionOwnerWithMessage(string localSessionName, string messageDetail)
        {
            // Find the session with this name
            // Identify the owner of the session
            // Send them a packet informing them of joining status

            Session localSession;

            if (!TryGetSession(localSessionName, out localSession))
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionOwnerWithMessage)}): No local session with name \"{localSessionName}\" was found.");
                return;
            }

            ActiveSession activeSession = localSession.ActiveSession;

            if (activeSession == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionOwnerWithMessage)}): Found local session with name \"{localSessionName}\", but there was no corresponding ActiveSession. An ActiveSession should be populated in InitActiveSession.");
                return;
            }

            // Copy over the session information
            // This should contain SessionDetails, which will include the user id of the owner of the room 
            ActiveSessionCopyInfoOptions copyInfoOptions = new ActiveSessionCopyInfoOptions() { };
            ActiveSessionInfo? copiedInfo;
            Result activeSessionInfoCopyResult = activeSession.CopyInfo(ref copyInfoOptions, out copiedInfo);

            if (activeSessionInfoCopyResult != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionOwnerWithMessage)}): Found local and active session with name \"{localSessionName}\", but failed to copy its info. Result code {activeSessionInfoCopyResult}");
                return;
            }

            if (!copiedInfo.Value.SessionDetails.HasValue)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionOwnerWithMessage)}): Found local and active session with name \"{localSessionName}\", but the SessionDetails in the object is null. Cannot determine session owner or session id.");
                return;
            }

            ProductUserId ownerUserId = copiedInfo.Value.SessionDetails.Value.OwnerUserId;

            if (!ownerUserId.IsValid())
            {
                // This session is owned by a server, so we can't message it using a product user id.
                // TODO: How do we message it?
                // This isn't exactly an error state, so we won't log a message, just going to return.
                return;
            }

            // Quickly check; is this local user the owner of this session? If so, we shouldn't be informing ourself
            if (EOSManager.Instance.GetProductUserId().Equals(ownerUserId))
            {
                // Oh nevermind, then! This user is the owner of this, and shouldn't message themself.
                return;
            }

            string formattedMessage = string.Format(P2P_INFORM_SESSION_MESSAGE_FORMAT, localSession.Id, EOSManager.Instance.GetProductUserId(), messageDetail);
            SendP2PMessage(formattedMessage, ownerUserId);
        }

        /// <summary>
        /// Whenever a session owner has a reason to update the session, inform all members of the session to refresh.
        /// There should already be a connection between the owner and the members of the session, but one will open if needed.
        /// The session members should then refresh their session information.
        /// </summary>
        /// <param name="localSessionName">The local name for the session.</param>.
        /// <param name="messageDetail">The message detail to send. This message detail is used to inform the members of what action to take.</param>
        private void InformSessionMembers(string localSessionName, string messageDetail)
        {
            // First find a local session with this name
            Session localSession;

            if (!TryGetSession(localSessionName, out localSession))
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionMembers)}): No local session with name \"{localSessionName}\" was found.");
                return;
            }

            ActiveSession activeSession = localSession.ActiveSession;

            if (activeSession == null)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionMembers)}): Found local session with name \"{localSessionName}\", but there was no corresponding ActiveSession. An ActiveSession should be populated in InitActiveSession.");
                return;
            }

            // Copy over the session information
            // This should contain SessionDetails, which will include the user id of the owner of the room 
            ActiveSessionCopyInfoOptions copyInfoOptions = new ActiveSessionCopyInfoOptions() { };
            ActiveSessionInfo? copiedInfo;
            Result activeSessionInfoCopyResult = activeSession.CopyInfo(ref copyInfoOptions, out copiedInfo);

            if (activeSessionInfoCopyResult != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionMembers)}): Found local and active session with name \"{localSessionName}\", but failed to copy its info. Result code {activeSessionInfoCopyResult}.");
                return;
            }

            if (!copiedInfo.Value.SessionDetails.HasValue)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(InformSessionMembers)}): Found local and active session with name \"{localSessionName}\", but the SessionDetails in the object is null. Cannot determine session id.");
                return;
            }

            // Are we the owner of this session? We should only be messaging people if we are, inside this function
            if (!copiedInfo.Value.SessionDetails.Value.OwnerUserId.Equals(EOSManager.Instance.GetProductUserId()))
            {
                // We're not the owner! This is not an error state, but don't send any messages.
                return;
            }

            ActiveSessionGetRegisteredPlayerCountOptions countOptions = new ActiveSessionGetRegisteredPlayerCountOptions() { };
            uint playerCount = activeSession.GetRegisteredPlayerCount(ref countOptions);

            Log($"{nameof(EOSSessionsManager)} ({nameof(InformSessionMembers)}): There are {playerCount} registered members in {localSession.Name}, informing users of {messageDetail} (excluding self)");

            string messageToSend = string.Format(P2P_INFORM_SESSION_MESSAGE_FORMAT, copiedInfo.Value.SessionDetails.Value.SessionId, EOSManager.Instance.GetProductUserId(), messageDetail);
            for (uint ii = 0; ii < playerCount; ii++)
            {
                ActiveSessionGetRegisteredPlayerByIndexOptions getPlayerIndexOption = new ActiveSessionGetRegisteredPlayerByIndexOptions() { PlayerIndex = ii };
                ProductUserId registeredPlayer = activeSession.GetRegisteredPlayerByIndex(ref getPlayerIndexOption);

                // We don't need to message ourself, so skip over if this is the local user
                if (EOSManager.Instance.GetProductUserId().Equals(registeredPlayer))
                {
                    continue;
                }

                SendP2PMessage(messageToSend, registeredPlayer);
            }
        }

        /// <summary>
        /// Utility function for sending P2P messages.
        /// </summary>
        /// <param name="message">The message to send. Should be a formatted string of <see cref="P2P_INFORM_SESSION_MESSAGE_FORMAT"/>.</param>
        /// <param name="userToSendTo">The ProductUserId to send the message to.</param>
        private void SendP2PMessage(string message, ProductUserId userToSendTo)
        {
            SocketId socketId = new SocketId()
            {
                SocketName = P2P_SESSION_STATUS_SOCKET_NAME
            };

            SendPacketOptions options = new SendPacketOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = userToSendTo,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = P2P_SESSION_STATUS_UPDATE_CHANNEL,
                Reliability = PacketReliability.ReliableOrdered,
                Data = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message))
            };

            // The result code of this indicates that we've managed to send the message successfully,
            // it does not guarantee that the message was received
            Result result = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().SendPacket(ref options);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(SendP2PMessage)}): Error while sending data, code: {result}.");
            }
            else
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(SendP2PMessage)}): Sending \"{message}\" to {userToSendTo}");
            }
        }

        /// <summary>
        /// Every update, or frequently, this method gathers P2P messages directed at the local user.
        /// This method determines what kind of message is incoming, and dispatches the function appropriately.
        /// </summary>
        private void HandleReceivedP2PMessages()
        {
            ReceivePacketOptions options = new ReceivePacketOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxDataSizeBytes = 4096,
                RequestedChannel = P2P_SESSION_STATUS_UPDATE_CHANNEL
            };

            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RequestedChannel = P2P_SESSION_STATUS_UPDATE_CHANNEL
            };

            Result nextPacketSizeResult = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);

            if (nextPacketSizeResult == Result.NotFound)
            {
                // There was no packet to receive. This isn't an error, there's just no news.
                return;
            }

            if (nextPacketSizeResult != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): error while reading received packet data, code: {nextPacketSizeResult}.");
                return;
            }

            if (nextPacketSizeBytes == 0)
            {
                return;
            }

            byte[] data = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(data);
            ProductUserId peerId = null;
            SocketId socketId = new SocketId() { SocketName = P2P_SESSION_STATUS_SOCKET_NAME };

            Result receivePacketResult = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().ReceivePacket(ref options, ref peerId, ref socketId, out byte outChannel, dataSegment, out uint bytesWritten);

            if (receivePacketResult != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): error while reading received packet data, code: {nextPacketSizeResult}.");
                return;
            }

            if (!peerId.IsValid())
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): ProductUserId peerId is not valid!");
                return;
            }

            string message = System.Text.Encoding.UTF8.GetString(data);

            Log($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): Received a message: {message}");

            if (!message.StartsWith(P2P_INFORM_SESSION_MESSAGE_BASE))
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): This function is handling a received message that it wasn't intended to. Perhaps there's a socket or channel conflict? Message: {message}");
                return;
            }

            // Match the message to look for (sessionid) (userid) (message element)
            Match regexMatchOfMessage = Regex.Match(message, @"\(([^\)]*)\) \(([^\)]*)\) \(([^\)]*)\)");

            if (regexMatchOfMessage.Groups.Count != 4)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): Message received, but it didn't contain the expected three parts. Message: {message}");
                return;
            }

            string sessionId = regexMatchOfMessage.Groups[1].Value;
            string userId = regexMatchOfMessage.Groups[2].Value;
            string messageElement = regexMatchOfMessage.Groups[3].Value;

            Log($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): Message parts: [{nameof(sessionId)}: {sessionId}] [{nameof(userId)}: {userId}] [{nameof(messageElement)}: {messageElement}]");

            if (!TryGetSessionById(sessionId, out Session session))
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): Message received regarding sessionId {sessionId}, but no local sessions have that id. Message: {message}");
                return;
            }

            ProductUserId messagingUserId = ProductUserId.FromString(userId);

            switch (messageElement)
            {
                case P2P_JOINING_SESSION_MESSAGE_ELEMENT:
                    Register(session.Name, messagingUserId);
                    break;
                case P2P_LEAVING_SESSION_MESSAGE_ELEMENT:
                    UnRegister(session.Name, messagingUserId);
                    break;
                case P2P_REFRESH_SESSION_MESSAGE_ELEMENT:
                    RefreshSession(session.Name);
                    break;
                case P2P_SESSION_OWNER_DESTROYED_SESSION_MESSAGE_ELEMENT:
                    DestroySession(session.Name);
                    break;
                default:
                    Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(HandleReceivedP2PMessages)}): Unrecognized message element, unclear what action to take. Message: {message}");
                    break;
            }
        }

        #region Notifications

        /// <summary>
        /// Whenever a user attempts to create a connection, this method handles their connection request.
        /// By default, accept all incoming connections.
        /// </summary>
        /// <param name="data">Data, containing the product user id of the connecting request.</param>
        private void OnIncomingSessionsConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            if (data.SocketId?.SocketName != P2P_SESSION_STATUS_SOCKET_NAME)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnIncomingSessionsConnectionRequest)}): This function should not be handling this message, its socket is not '{P2P_SESSION_STATUS_SOCKET_NAME}'. Socket name is '{(data.SocketId?.SocketName)}'.");
                return;
            }

            SocketId socketId = new SocketId()
            {
                SocketName = P2P_SESSION_STATUS_SOCKET_NAME
            };

            AcceptConnectionOptions options = new AcceptConnectionOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = data.RemoteUserId,
                SocketId = socketId
            };

            Result result = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().AcceptConnection(ref options);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnIncomingSessionsConnectionRequest)}): Error while accepting connection, code: {result}");
            }
            else
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnIncomingSessionsConnectionRequest)}): Successfully accepted connection from {options.RemoteUserId} on socket {P2P_SESSION_STATUS_SOCKET_NAME}");
            }
        }

        /// <summary>
        /// Upon a user that is connected disconnecting, this method closes out the connection.
        /// </summary>
        /// <param name="data">Data, containing the product user id of the user to disconnect from.</param>
        private void OnIncomingSessionsDisconnect(ref OnRemoteConnectionClosedInfo data)
        {
            SocketId socketId = new SocketId()
            {
                SocketName = P2P_SESSION_STATUS_SOCKET_NAME
            };

            CloseConnectionOptions closeOptions = new CloseConnectionOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = data.RemoteUserId,
                SocketId = socketId
            };

            Result result = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().CloseConnection(ref closeOptions);

            if (result != Result.Success)
            {
                Debug.LogError($"{nameof(EOSSessionsManager)} ({nameof(OnIncomingSessionsDisconnect)}): Error while closing connection, code: {result}");
            }
            else
            {
                Log($"{nameof(EOSSessionsManager)} ({nameof(OnIncomingSessionsDisconnect)}): Successfully closed connection with {closeOptions.RemoteUserId} on socket {P2P_SESSION_STATUS_SOCKET_NAME}");
            }
        }

        #endregion

        #endregion
    }
}
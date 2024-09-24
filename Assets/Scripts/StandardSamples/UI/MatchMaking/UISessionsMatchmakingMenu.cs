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
    using UnityEngine.UI;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Sessions;

    public class UISessionsMatchmakingMenu : SampleMenuWithFriends
    {
        /// <summary>
        /// An enum to record the local state of whether the local user can invite users to their Session.
        /// <see cref="OwnInvitationState"/>
        /// </summary>
        protected enum OwnSessionInvitationAbilityState
        {
            /// <summary>
            /// Indicates the user has not joined any Session, therefore can't invite anyone.
            /// </summary>
            NoSessionToInviteTo,

            /// <summary>
            /// Indicates the user has joined a Session, but for some reason it can't have invitations sent for it.
            /// </summary>
            InvalidSessionToInviteTo,

            /// <summary>
            /// Indicates the user has joined a Session that can have invitations sent for it correctly.
            /// </summary>
            ValidSessionToInviteTo
        }

        /// <summary>
        /// Cached state that indicates if the local user can send invitations for a Session.
        /// Processed in <see cref="OnFriendStateChanged"/>,
        /// and utilized in <see cref="GetFriendInteractionState(FriendData)"/>.
        /// </summary>
        protected OwnSessionInvitationAbilityState OwnInvitationState { get; set; } = OwnSessionInvitationAbilityState.NoSessionToInviteTo;

        [Header("Sessions/Matchmaking UI - Create Options")]
        public Text SessionNameVal;
        public Dropdown MaxPlayersVal;
        public Dropdown LevelVal;
        public Dropdown PermissionVal;
        public Toggle PresenceVal;
        public Toggle JoinInProgressVal;
        public Toggle InvitesAllowedVal;
        public Toggle SanctionsVal;

        [Header("Sessions/Matchmaking UI - Session Members")]
        public GameObject UISessionEntryPrefab;
        public GameObject SessionContentParent;
        public Text CurrentSessionsHeader;

        [Header("Sessions/Matchmaking UI - Search")]
        public UIConsoleInputField SearchByLevelBox;

        private bool ShowSearchResults = false;

        [Header("Sessions/Matchmaking UI - Invite PopUp")]
        public GameObject UIInvitePanel;
        public Text InviteFromVal;
        public Toggle InvitePresence;

        // TODO: It's unclear why this only works when startsHidden is set to
        //       false, but only for this menu. Some investigation needs to
        //       happen, after which it's possible that the constructor 
        //       parameter might be able to be removed altogether.
        public UISessionsMatchmakingMenu() : base(false)
        {

        }

        private EOSSessionsManager GetEOSSessionsManager
        {
            get { return EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>(); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Hide Invite Pop-up (Default)
            UIInvitePanel.SetActive(false);
            InviteFromVal.text = string.Empty;

            GetEOSSessionsManager.UIOnSessionRefresh = OnSessionRefresh;
        }

        private int previousFrameSessionCount = 0;
        private int previousFrameResultCount = 0;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Unity crashes if you try to access EOSSinglton OnDestroy
            EOSManager.Instance.RemoveManager<EOSSessionsManager>();
        }

        protected override void Update()
        {
            base.Update();

            EOSSessionsManager sessionsManager = GetEOSSessionsManager;
            bool stateUpdates = sessionsManager.Update();
            
            // Invites UI Prompt
            if (sessionsManager.GetCurrentInvite() != null)
            {
                UIInvitePanel.SetActive(true);

                if (string.IsNullOrEmpty(InviteFromVal.text))
                {
                    SessionAttribute attributeFound = sessionsManager.GetCurrentInvite().Attributes.Find(x => string.Equals(x.Key, "Level", StringComparison.OrdinalIgnoreCase));

                    if (attributeFound != null)
                    {
                        InviteFromVal.text = attributeFound.AsString;
                    }
                }
            }
            else
            {
                UIInvitePanel.SetActive(false);
                InviteFromVal.text = string.Empty;
            }

            if (ShowSearchResults)
            {
                previousFrameSessionCount = 0;

                if (sessionsManager.GetCurrentSearch() == null)
                {
                    Debug.LogError("Sessions Matchmaking (Update): ShowSearchResults is true, but CurrentSearch is null!");
                    ShowSearchResults = false;
                }

                CurrentSessionsHeader.text = "Search Results:";

                Dictionary<Session, SessionDetails> results = sessionsManager.GetCurrentSearch().GetResults();

                if (previousFrameResultCount == results.Count)
                {
                    if (results.Count == 0)
                    {
                        // Destroy current UI member list
                        foreach (Transform child in SessionContentParent.transform)
                        {
                            Destroy(child.gameObject);
                        }
                    }

                    // no new results count
                    return;
                }

                // Render Sessions State changes
                previousFrameResultCount = results.Count;

                foreach (KeyValuePair<Session, SessionDetails> kvp in sessionsManager.GetCurrentSearch().GetResults())
                {
                    Session sessionResult = kvp.Key;

                    GameObject sessionUiObj = Instantiate(UISessionEntryPrefab, SessionContentParent.transform);
                    UISessionEntry uiEntry = sessionUiObj.GetComponent<UISessionEntry>();

                    if (uiEntry != null)
                    {
                        uiEntry.SetUIElementsFromSessionAndDetails(sessionResult, kvp.Value, this);
                    }
                }
            }
            else
            {
                previousFrameResultCount = 0;

                CurrentSessionsHeader.text = "Current Sessions:";

                if (!stateUpdates && previousFrameSessionCount == sessionsManager.GetCurrentSessions().Count)
                {
                    if (sessionsManager.GetCurrentSessions().Count == 0)
                    {
                        // Destroy current UI member list
                        foreach (Transform child in SessionContentParent.transform)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }

                    // no state updates and count hasn't changed;
                    return;
                }

                // Render Sessions State changes
                previousFrameSessionCount = sessionsManager.GetCurrentSessions().Count;

                // Destroy current UI member list
                foreach (Transform child in SessionContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                // Enumerate session entries in UI
                foreach (KeyValuePair<string, Session> kvp in sessionsManager.GetCurrentSessions())
                {
                    Session session = kvp.Value;

                    GameObject sessionUiObj = Instantiate(UISessionEntryPrefab, SessionContentParent.transform);
                    UISessionEntry uiEntry = sessionUiObj.GetComponent<UISessionEntry>();

                    if (uiEntry != null)
                    {
                        uiEntry.SetUIElementsFromSession(session, this);
                    }
                }
            }
        }

        public void CreateNewSessionOnClick()
        {
            Session session = new Session();
            session.AllowJoinInProgress = JoinInProgressVal.isOn;
            session.InvitesAllowed = InvitesAllowedVal.isOn;
            session.SanctionsEnabled = SanctionsVal.isOn;
            session.MaxPlayers = (uint)Int32.Parse(MaxPlayersVal.options[MaxPlayersVal.value].text);
            session.Name = SessionNameVal.text;
            session.PermissionLevel = (OnlineSessionPermissionLevel)PermissionVal.value;

            SessionAttribute attribute = new SessionAttribute();
            attribute.Key = "Level";
            attribute.AsString = LevelVal.options[LevelVal.value].text;
            attribute.ValueType = AttributeType.String;
            attribute.Advertisement = SessionAttributeAdvertisementType.Advertise;

            session.Attributes.Add(attribute);

            GetEOSSessionsManager.CreateSession(session, UIOnSessionCreated);
        }

        private void UIOnSessionCreated(SessionsManagerCreateSessionCallbackInfo info)
        {
            // Update() already enumerates ActiveSessions.  Here you can do any UI related calls after session is created.
        }

        //Search Result
        public void JoinButtonOnClick(SessionDetails sessionHandle)
        {
            GetEOSSessionsManager.JoinSession(sessionHandle, true, OnJoinSessionFinished); // Default Presence True
        }

        private void OnJoinSessionFinished(Result result)
        {
            if (result != Result.Success)
            {
                RefreshSearch();
            }
            else
            {
                ShowSearchResults = false;
            }
        }

        // Session Member
        public void StartButtonOnClick(string sessionName)
        {
            GetEOSSessionsManager.StartSession(sessionName);
        }

        public void EndButtonOnClick(string sessionName)
        {
            GetEOSSessionsManager.EndSession(sessionName);
        }

        public void ModifyButtonOnClick(string sessionName)
        {
            // Only modify Max Players and Level
            Session session = new Session(); //GetEOSSessionsManager.GetSession(sessionName);
            session.Name = sessionName;
            session.MaxPlayers = (uint)Int32.Parse(MaxPlayersVal.options[MaxPlayersVal.value].text);
            session.AllowJoinInProgress = JoinInProgressVal.isOn;
            session.InvitesAllowed = InvitesAllowedVal.isOn;
            session.PermissionLevel = (OnlineSessionPermissionLevel)PermissionVal.value;

            SessionAttribute attr = new SessionAttribute();
            attr.Key = "Level";
            attr.ValueType = AttributeType.String;
            attr.AsString = LevelVal.options[LevelVal.value].text;
            attr.Advertisement = SessionAttributeAdvertisementType.Advertise;
            session.Attributes.Add(attr);

            GetEOSSessionsManager.ModifySession(session, OnModifySessionCompleted);
        }

        private void OnModifySessionCompleted()
        {
            previousFrameSessionCount = 0;
        }

        public void LeaveButtonOnClick(string sessionName)
        {
            GetEOSSessionsManager.DestroySession(sessionName);
        }

        public void RefreshSearch()
        {
            SearchByLevelEndEdit(SearchByLevelBox.InputField.text);
        }

        // Search
        public void SearchByLevelEndEdit(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                ShowSearchResults = false;
                return;
            }

            SessionAttribute levelAttribute = new SessionAttribute();
            levelAttribute.Key = "Level";
            levelAttribute.ValueType = AttributeType.String;
            levelAttribute.AsString = searchPattern.ToUpper();
            levelAttribute.Advertisement = SessionAttributeAdvertisementType.Advertise;

            List<SessionAttribute> attributes = new List<SessionAttribute>() { levelAttribute };

            GetEOSSessionsManager.Search(attributes);

            previousFrameResultCount = 0;
            ShowSearchResults = true;
        }

        // Invite
        public void AcceptInviteButtonOnClick()
        {
            bool invitePresenceToggled = InvitePresence.isOn;

            GetEOSSessionsManager.AcceptLobbyInvite(invitePresenceToggled);

            // Make sure UI is showing current sessions
            ShowSearchResults = false;
        }

        public void DeclineInviteButtonOnClick()
        {
            GetEOSSessionsManager.DeclineLobbyInvite();
        }

        public override void Show()
        {
            base.Show();
            GetEOSSessionsManager.OnLoggedIn();
            GetEOSSessionsManager.OnPresenceChange.AddListener(SetDirtyFlagAction);

            EOSManager.Instance.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, Epic.OnlineServices.Logging.LogLevel.Warning);
            EOSManager.Instance.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.Sessions, Epic.OnlineServices.Logging.LogLevel.Verbose);
        }

        public override void Hide()
        {
            base.Hide();
            if (GetEOSSessionsManager.IsUserLoggedIn)//check to prevent warnings when done unnecessarily during Sessions & Matchmaking startup
            {
                GetEOSSessionsManager.OnPresenceChange.RemoveListener(SetDirtyFlagAction);
                GetEOSSessionsManager.OnLoggedOut();
            }
        }

        public bool TryGetExistingUISessionEntryById(string sessionId, out UISessionEntry entry)
        {
            foreach (Transform childTransform in SessionContentParent.transform)
            {
                UISessionEntry thisEntry = childTransform.GetComponent<UISessionEntry>();

                if (null == thisEntry || null == thisEntry.RepresentedSession || thisEntry.RepresentedSession.Id != sessionId)
                {
                    continue;
                }

                entry = thisEntry;
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// After a Session is successfully refreshed, this is run to update the UI with the new information about the local Session.
        /// </summary>
        /// <param name="session">Information about the Session from the EOS C# SDK.</param>
        /// <param name="details">Additional information about the Session from the EOS C SDK.</param>
        public void OnSessionRefresh(Session session, SessionDetails details)
        {
            if (!TryGetExistingUISessionEntryById(session.Id, out UISessionEntry uiEntry))
            {
                Debug.Log($"UISessionsMatchmakingMenu (OnSessionRefresh): Requested refresh of a Session with {nameof(Session.Id)} \"{session.Id}\", but did not have a UI Entry for that currently. Cannot refresh it.");
                return;
            }

            Debug.Log($"{nameof(UISessionsMatchmakingMenu)} ({nameof(OnSessionRefresh)} Instructed to refresh Session with {nameof(Session.Id)} \"{session.Id}\". Found local UI element. Refreshing now.");

            uiEntry.SetUIElementsFromSessionAndDetails(session, details, this);
        }

        #region UIFriendInteractionSource Implementations

        /* 
         * REGION NOTES:
         * 
         * The UIFriendsMenu popout uses the base class UIFriendInteractionSource to facilitate interactions.
         * 
         * NOTE: This current implementation is not complete.
         * There is functionality suggested by the existance of this window that needs to be added.
         * 
         * - Users should be able to Join Sessions their friends are in, if that friend is in a Session, and that Session has invites enabled.
         * - Users that are in a Session who want to interact with a friend that is also in a Session should have UX for choosing Invite or Join
         * - Users in multiple Sessions should be able to choose which Session to invite their friend to. Currently it uses the most recently joined Session only.
         * - Users who have friends in multiple Sessions should be able to choose which Session to join
         * 
         */

        public override string GetFriendInteractButtonText()
        {
            return "Invite";
        }

        public override void OnFriendInteractButtonClicked(FriendData friendData)
        {
            // Get the local Presence Session to invite to
            if (!GetEOSSessionsManager.TryGetPresenceSession(out Session foundSession) || foundSession.ActiveSession == null)
            {
                // Didn't find a presence session, so nothing to invite to
                Debug.LogError($"{nameof(UISessionsMatchmakingMenu)} ({nameof(OnFriendInteractButtonClicked)}): A friend was chosen to invite to a Session, but no local Presence-enabled Session detected.");
                return;
            }

            GetEOSSessionsManager.InviteToSession(foundSession.Name, friendData.UserProductUserId);
        }

        public override FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            // First determine if the user is both a friend and online
            if (!friendData.IsFriend() || !friendData.IsOnline())
            {
                return FriendInteractionState.Hidden;
            }

            if (OwnInvitationState == OwnSessionInvitationAbilityState.NoSessionToInviteTo)
            {
                return FriendInteractionState.Hidden;
            }

            if (OwnInvitationState == OwnSessionInvitationAbilityState.InvalidSessionToInviteTo)
            {
                return FriendInteractionState.Disabled;
            }

            // The only thing remaining is yes, this user can be interacted with
            return FriendInteractionState.Enabled;
        }

        public override void OnFriendStateChanged()
        {
            // Determine if the local user has an active, presence-enabled Session
            if (!GetEOSSessionsManager.TryGetPresenceSession(out Session foundSession) || foundSession.ActiveSession == null)
            {
                // Didn't find a presence session, so nothing to invite to
                OwnInvitationState = OwnSessionInvitationAbilityState.NoSessionToInviteTo;
                return;
            }

            // Does this Session allow for invites? If not, you can't invite users to it.
            if (!foundSession.InvitesAllowed)
            {
                OwnInvitationState = OwnSessionInvitationAbilityState.InvalidSessionToInviteTo;
                return;
            }

            // Is this Session in a state that can accept more users?
            // To answer questions about this, fetch the ActiveSessionInfo
            ActiveSessionCopyInfoOptions copyInfoOption = new();
            Result copyResult = foundSession.ActiveSession.CopyInfo(ref copyInfoOption, out ActiveSessionInfo? foundInfo);

            if (copyResult != Result.Success || !foundInfo.HasValue || !foundInfo.Value.SessionDetails.HasValue)
            {
                OwnInvitationState = OwnSessionInvitationAbilityState.InvalidSessionToInviteTo;
                return;
            }

            // If users can't join an in-progress Session, then check the status of the Session
            if (!foundSession.AllowJoinInProgress && (foundInfo.Value.State == OnlineSessionState.Starting || foundInfo.Value.State == OnlineSessionState.InProgress))
            {
                EOSSessionsManager.Log($"{nameof(UISessionsMatchmakingMenu)} ({nameof(GetFriendInteractionState)}): The current Presence-enabled Session cannot be invited to because it is {foundInfo.Value.State} and {nameof(Session.AllowJoinInProgress)} is false.");
                OwnInvitationState = OwnSessionInvitationAbilityState.InvalidSessionToInviteTo;
                return;
            }

            // Check that the Session doesn't already have the maximum number of users
            if (foundInfo.Value.SessionDetails.Value.NumOpenPublicConnections == 0)
            {
                EOSSessionsManager.Log($"{nameof(UISessionsMatchmakingMenu)} ({nameof(GetFriendInteractionState)}): The current Presence-enabled Session cannot be invited to because the Session already has reached its {nameof(Session.MaxPlayers)} count.");
                OwnInvitationState = OwnSessionInvitationAbilityState.InvalidSessionToInviteTo;
                return;
            }

            OwnInvitationState = OwnSessionInvitationAbilityState.ValidSessionToInviteTo;
            return;
        }

        /// <summary>
        /// <see cref="EOSSessionsManager.OnPresenceChange"/> accepts a function with zero arguments.
        /// This methods gives a consistent method to AddListener and RemoveListener to that event.
        /// </summary>
        private void SetDirtyFlagAction()
        {
            SetDirtyFlag();
        }

        #endregion
    }
}
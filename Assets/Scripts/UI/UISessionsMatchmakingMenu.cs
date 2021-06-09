using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UISessionsMatchmakingMenu : MonoBehaviour
    {
        public GameObject SessionsMatchmakingUIParent;

        [Header("Sessions/Matchmaking UI - Create Options")]
        public Text SessionNameVal;
        public Dropdown MaxPlayersVal;
        public Dropdown LevelVal;
        public Toggle PublicVal;
        public Toggle PresenceVal;
        public Toggle JoinInProgressVal;
        public Toggle InvitesAllowedVal;

        [Header("Sessions/Matchmaking UI - Session Members")]
        public GameObject UISessionEntryPrefab;
        public GameObject SessionContentParent;
        public Text CurrentSessionsHeader;

        [Header("Sessions/Matchmaking UI - Search")]
        public InputField SearchByLevelBox;

        private bool ShowSearchResults = false;

        [Header("Sessions/Matchmaking UI - Invite PopUp")]
        public GameObject UIInvitePanel;
        public Text InviteFromVal;
        public Toggle InvitePresence;

        public void Awake()
        {
            // Hide Invite Pop-up (Default)
            UIInvitePanel.SetActive(false);
            InviteFromVal.text = string.Empty;

            HideMenu();
        }

        private void Start()
        {
            SearchByLevelBox.onEndEdit.AddListener(SearchByLevelEnterPressed);
        }

        private int previousFrameSessionCount = 0;
        private int previousFrameResultCount = 0;

        public void Update()
        {
            EOSSessionsManager sessionsManager = EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>();
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
                            GameObject.Destroy(child.gameObject);
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
                        uiEntry.NameTxt.text = sessionResult.Name;

                        if (sessionResult.UpdateInProgress)
                        {
                            uiEntry.StatusTxt.text = "*Updating*";
                        }
                        else
                        {
                            uiEntry.StatusTxt.text = sessionResult.SessionState.ToString();
                        }

                        uiEntry.PlayersTxt.text = string.Format("{0}/{1}", sessionResult.NumConnections, sessionResult.MaxPlayers);
                        uiEntry.PresenceTxt.text = sessionResult.PresenceSession.ToString();
                        uiEntry.JIPTxt.text = sessionResult.AllowJoinInProgress.ToString();
                        uiEntry.PublicTxt.text = sessionResult.AllowJoinInProgress.ToString();
                        uiEntry.InvitesTxt.text = sessionResult.InvitesAllowed.ToString();

                        uiEntry.JoinOnClick = JoinButtonOnClick;
                        uiEntry.JoinSessionDetails = kvp.Value;

                        uiEntry.OnlyEnableSearchResultButtons();

                        bool levelAttributeFound = false;
                        foreach (SessionAttribute sessionAttr in sessionResult.Attributes)
                        {
                            if (sessionAttr.Key.Equals("Level", StringComparison.OrdinalIgnoreCase))
                            {
                                uiEntry.LevelTxt.text = sessionAttr.AsString;
                                levelAttributeFound = true;
                                break;
                            }
                        }

                        if (!levelAttributeFound)
                        {
                            Debug.LogErrorFormat("UISessionsMatchmakingMenu: Attribute 'Level' not found for session '{0}'", sessionResult.Name);
                            uiEntry.LevelTxt.text = "-NA-";
                        }
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
                        uiEntry.NameTxt.text = kvp.Key;

                        if (session.UpdateInProgress)
                        {
                            uiEntry.StatusTxt.text = "*Updating*";
                        }
                        else
                        {
                            uiEntry.StatusTxt.text = session.SessionState.ToString();
                        }

                        uiEntry.PlayersTxt.text = string.Format("{0}/{1}", session.NumConnections, session.MaxPlayers);
                        uiEntry.PresenceTxt.text = session.PresenceSession.ToString();
                        uiEntry.JIPTxt.text = session.AllowJoinInProgress.ToString();
                        uiEntry.PublicTxt.text = session.AllowJoinInProgress.ToString();
                        uiEntry.InvitesTxt.text = session.InvitesAllowed.ToString();

                        uiEntry.StartOnClick = StartButtonOnClick;
                        uiEntry.EndOnClick = EndButtonOnClick;
                        uiEntry.ModifyOnClick = ModifyButtonOnClick;
                        uiEntry.LeaveOnClick = LeaveButtonOnClick;

                        bool levelAttributeFound = false;
                        foreach (SessionAttribute sessionAttr in session.Attributes)
                        {
                            if (sessionAttr.Key.Equals("Level", StringComparison.OrdinalIgnoreCase))
                            {
                                uiEntry.LevelTxt.text = sessionAttr.AsString;
                                levelAttributeFound = true;
                                break;
                            }
                        }

                        if (!levelAttributeFound)
                        {
                            Debug.LogErrorFormat("UISessionsMatchmakingMenu: Attribute 'Level' not found for session '{0}'", session.Name);
                            uiEntry.LevelTxt.text = "-NA-";
                        }
                    }
                }
            }
        }

        public void CreateNewSessionOnClick()
        {
            Session session = new Session();
            session.AllowJoinInProgress = JoinInProgressVal.enabled;
            session.PresenceSession = PresenceVal.enabled;
            session.InvitesAllowed = InvitesAllowedVal.enabled;
            session.MaxPlayers = (uint)Int32.Parse(MaxPlayersVal.options[MaxPlayersVal.value].text);
            session.Name = SessionNameVal.text;
            session.PermissionLevel = PublicVal.enabled ? OnlineSessionPermissionLevel.PublicAdvertised : OnlineSessionPermissionLevel.InviteOnly;

            SessionAttribute attribute = new SessionAttribute();
            attribute.Key = "Level";
            attribute.AsString = LevelVal.options[LevelVal.value].text;
            attribute.ValueType = AttributeType.String;
            attribute.Advertisement = SessionAttributeAdvertisementType.Advertise;

            session.Attributes.Add(attribute);

            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().CreateSession(session, UIOnSessionCreated);
        }

        private void UIOnSessionCreated()
        {
            // Update() already enumerates ActiveSessions.  Here you can do any UI related calls after session is created.
        }

        //Search Result
        private void JoinButtonOnClick(SessionDetails sessionHandle)
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().JoinSession(sessionHandle, true, OnJoinSessionFinished); // Default Presence True
        }

        private void OnJoinSessionFinished()
        {
            ShowSearchResults = false;
        }

        // Session Member
        public void StartButtonOnClick(string sessionName)
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().StartSession(sessionName);
        }

        public void EndButtonOnClick(string sessionName)
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().EndSession(sessionName);
        }

        public void ModifyButtonOnClick(string sessionName)
        {
            // Only modify Max Players and Level
            Session session = new Session(); //EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().GetSession(sessionName);
            session.Name = sessionName;
            session.MaxPlayers = (uint)Int32.Parse(MaxPlayersVal.options[MaxPlayersVal.value].text);
            session.AllowJoinInProgress = JoinInProgressVal.enabled;
            session.InvitesAllowed = InvitesAllowedVal.enabled;
            session.PermissionLevel = PublicVal.enabled ? OnlineSessionPermissionLevel.PublicAdvertised : OnlineSessionPermissionLevel.InviteOnly;

            SessionAttribute attr = new SessionAttribute();
            attr.Key = "Level";
            attr.AsString = LevelVal.options[LevelVal.value].text;
            session.Attributes.Add(attr);

            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().ModifySession(session, OnModifySessionCompleted);
        }

        private void OnModifySessionCompleted()
        {
            previousFrameSessionCount = 0;
        }

        public void LeaveButtonOnClick(string sessionName)
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().DestroySession(sessionName);
        }

        // Search
        private void SearchByLevelEnterPressed(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                ShowSearchResults = false;
                return;
            }

            SessionAttribute levelAttribute = new SessionAttribute();
            levelAttribute.Key = "Level";
            levelAttribute.ValueType = AttributeType.String;
            levelAttribute.AsString = searchPattern;
            levelAttribute.Advertisement = SessionAttributeAdvertisementType.Advertise;

            List<SessionAttribute> attributes = new List<SessionAttribute>() { levelAttribute };

            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().Search(attributes);

            previousFrameResultCount = 0;
            ShowSearchResults = true;
        }

        private void OnDestroy()
        {
            //HideMenu();
            // Unity crashes if you try to access EOSSinglton OnDestroy
        }

        // Invite
        public void AcceptInviteButtonOnClick()
        {
            bool invitePresenceToggled = InvitePresence.isOn;

            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().AcceptLobbyInvite(invitePresenceToggled);

            // Make sure UI is showing current sessions
            ShowSearchResults = false;
        }

        public void DeclineInviteButtonOnClick()
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().DeclineLobbyInvite();
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().OnLoggedIn();

            SessionsMatchmakingUIParent.gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>()?.OnLoggedOut();

            SessionsMatchmakingUIParent.gameObject.SetActive(false);
        }
    }
}
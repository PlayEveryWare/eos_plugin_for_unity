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
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using Epic.OnlineServices;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.UI;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIFriendsMenu : MonoBehaviour
    {
        [Header("Friends UI")]
        public GameObject FriendsUIParent;

        public GameObject FriendsPanel;
        public Button FriendsTabButton_Closed;
        private bool collapsed = false;

        public ConsoleInputField SearchFriendsInput;

        public GameObject FriendsListContentParent;
        public GameObject UIFriendEntryPrefab;

        public bool CollapseOnStart = false;

        [Header("Controller")]
        public GameObject UIFirstSelected;
        public GameObject[] ControllerUIObjects;

        private EOSFriendsManager FriendsManager;

        private bool isSearching;

        // P2P
        [Header("P2P Options (Optional)")]
        public bool EnableP2PChat = false;
        public UIPeer2PeerMenu UIPeer2PeerMenu;

        // Lobbies
        [Header("Lobbies Options (Optional)")]
        public bool EnableLobbyInvites = false;
        public UILobbiesMenu UILobbiesMenu;

        // Player Report
        [Header("Player Report Options (Optional)")]
        public bool EnablePlayerReport = false;
        public UIPlayerReportMenu UIPlayerReportMenu;


#if !ENABLE_INPUT_SYSTEM
        private void Awake()
        {
            // Ensure Disable Controller UI
            foreach(GameObject o in ControllerUIObjects)
            {
                o.SetActive(false);
            }
        }
#endif

        public void Start()
        {
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();

            if (CollapseOnStart)
            {
                CollapseFriendsTab();
            }
            else
            {
                FriendsTabButton_Closed.gameObject.SetActive(false);
            }

            SearchFriendsInput.InputField.onEndEdit.AddListener(SearchFriendsInputEnterPressed);
            isSearching = false;
        }

        private void SearchFriendsInputEnterPressed(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                isSearching = false;
                FriendsManager.ClearCachedSearchResults();
            }
            else
            {
                FriendsManager.QueryUserInfo(searchString, null);
                isSearching = true;
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var gamepad = Gamepad.current;
            if (gamepad != null && gamepad.rightShoulder.wasPressedThisFrame)
            {
                ToggleFriendsTab();
            }
#endif

            if (isSearching)
            {
                if (FriendsManager.GetCachedSearchResults(out Dictionary<EpicAccountId, FriendData> searchResults))
                {
                    // Destroy current UI member list
                    foreach (Transform child in FriendsListContentParent.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }

                    foreach (FriendData friend in searchResults.Values)
                    {
                        GameObject friendUIObj = Instantiate(UIFriendEntryPrefab, FriendsListContentParent.transform);
                        UIFriendEntry uiEntry = friendUIObj.GetComponent<UIFriendEntry>();

                        uiEntry.SetEpicAccount(friend.UserId, friend.UserProductUserId);

                        uiEntry.DisplayName.text = friend.Name;

                        if (friend.Status == FriendsStatus.Friends && friend.Presence != null)
                        {
                            uiEntry.Status.text = friend.Presence.Status.ToString();
                        }
                        else
                        {
                            uiEntry.Status.text = friend.Status.ToString();
                        }

                        // Report offline/online players
                        if (EnablePlayerReport)
                        {
                            uiEntry.ReportOnClick = UIPlayerReportMenu.ReportButtonOnClick;
                            uiEntry.EnableReportButton();
                        }

                        // AddFriends is Deprecated
                        // uiEntry.AddFriendOnClick = AddFriendButtonOnClick;
                        // uiEntry.EnableAddButton();
                    }
                }

                return;
            }

            if (FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friendList))
            {
                // Destroy current UI member list
                foreach (Transform child in FriendsListContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (FriendData friend in friendList.Values)
                {
                    GameObject friendUIObj = Instantiate(UIFriendEntryPrefab, FriendsListContentParent.transform);
                    UIFriendEntry uiEntry = friendUIObj.GetComponent<UIFriendEntry>();

                    uiEntry.SetEpicAccount(friend.UserId, friend.UserProductUserId);

                    uiEntry.DisplayName.text = friend.Name;

                    // Report offline/online players
                    if (EnablePlayerReport)
                    {
                        uiEntry.ReportOnClick = UIPlayerReportMenu.ReportButtonOnClick;
                        uiEntry.EnableReportButton();
                    }

                    if (friend.Status == FriendsStatus.Friends && friend.Presence != null)
                    {
                        uiEntry.Status.text = friend.Presence.Status.ToString();

                        if (friend.Presence.Status == Status.Online)
                        {
                            if (EnableP2PChat)
                            {
                                uiEntry.ChatOnClick = UIPeer2PeerMenu.ChatButtonOnClick;
                                uiEntry.EnableChatButton();
                            }

                            if (EnableLobbyInvites && UILobbiesMenu.IsCurrentLobbyValid())
                            {
                                uiEntry.InviteFriendsOnClick = UILobbiesMenu.LobbyInviteButtonOnClick;
                                uiEntry.EnableInviteButton();
                            }
                        }
                    }
                    else
                    {
                        uiEntry.Status.text = friend.Status.ToString();
                    }
                }
            }
        }

        private void AddFriendButtonOnClick(EpicAccountId searchResultEntry)
        {
            FriendsManager.AddFriend(searchResultEntry);
        }

        public void ToggleFriendsTab()
        {
            // Toggle Friends List UI
            if (collapsed)
            {
                ExpandFriendsTab();
            }
            else
            {
                CollapseFriendsTab();
            }
        }

        public void CollapseFriendsTab()
        {
            FriendsTabButton_Closed.gameObject.SetActive(true);

            FriendsPanel.SetActive(false);
            collapsed = true;
        }

        public void ExpandFriendsTab()
        {
            FriendsTabButton_Closed.gameObject.SetActive(false);

            FriendsPanel.SetActive(true);

            collapsed = false;
        }


        // Friends
        public void FriendsOverlayOnClick()
        {
            Debug.Log("FriendsOverlayOnClick: IsValid=" + EOSManager.Instance.GetLocalUserId().IsValid() + ", accountId" + EOSManager.Instance.GetLocalUserId().ToString());
            FriendsManager.ShowFriendsOverlay(null);
        }

        public void RefreshFriendsOnClick()
        {
            FriendsManager.QueryFriends(null);
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>().OnLoggedIn();

            FriendsUIParent.SetActive(true);

            // Controller
            if(UIFirstSelected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(UIFirstSelected);
            }
        }

        public void HideMenu()
        {
            FriendsManager?.OnLoggedOut();

            FriendsUIParent.SetActive(false);
        }
    }
}
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
    public class UIFriendsMenu : MonoBehaviour, ISampleSceneUI
    {
        [Header("Friends UI")]
        public GameObject FriendsUIParent;

        public GameObject FriendsPanel;
        private bool collapsed = false;

        public GameObject FriendOverlayContent;

        public UIConsoleInputField SearchFriendsInput;

        public GameObject FriendsListContentParent;
        public GameObject UIFriendEntryPrefab;

        public bool CollapseOnStart = false;

        [Header("Controller")]
        public GameObject UIFirstSelected;
        public GameObject[] ControllerUIObjects;

        private EOSFriendsManager FriendsManager;

        private bool isSearching;

        // Lobbies, P2P Chat, etc.
        [Header("Friend Interaction Source (Optional)")]
        [Tooltip("UI for Lobbies, P2P Chat, Reports, etc.")]
        public UIFriendInteractionSource UIFriendInteractionSource;

        private float initialPanelAnchoredPosX;


#if !ENABLE_INPUT_SYSTEM
        private void Awake()
        {
            // Ensure Disable Controller UI
            foreach (GameObject o in ControllerUIObjects)
            {
                o.SetActive(false);
            }
        }
#endif

        public void Start()
        {
            initialPanelAnchoredPosX = (FriendsPanel.transform as RectTransform).anchoredPosition.x;
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();

            if (CollapseOnStart)
            {
                CollapseFriendsTab();
            }

            isSearching = false;
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
        }

        public void SearchFriendsEndEdit(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                isSearching = false;
                FriendsManager.ClearCachedSearchResults();
                RenderFriendsList(true);
            }
            else
            {
                isSearching = true;
                FriendsManager.SearchFriendList(searchString);
            }
        }

        // Use when friend button states need to be updated without querying friend list
        public void RefreshFriendUI()
        {
            RenderFriendsList(true);
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
                RenderSearchResults();
            }
            else
            {
                if (UIFriendInteractionSource != null && UIFriendInteractionSource.IsDirty())
                {
                    RenderFriendsList(true);
                    UIFriendInteractionSource.ResetDirtyFlag();
                }
                else
                {
                    RenderFriendsList(false);
                }
            }
        }

        private void RenderFriendsList(bool forceUpdate)
        {
            if (FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friendList) || forceUpdate)
            {
                RefreshUIList(friendList.Values);
            }
        }

        private void RenderSearchResults()
        {
            if (FriendsManager.GetCachedSearchResults(out Dictionary<EpicAccountId, FriendData> searchResults))
            {
                RefreshUIList(searchResults.Values);
            }
        }

        private void RefreshUIList(Dictionary<EpicAccountId, FriendData>.ValueCollection friendDataList)
        {
            // Destroy current UI member list
            foreach (Transform child in FriendsListContentParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (FriendData friend in friendDataList)
            {
                GameObject friendUIObj = Instantiate(UIFriendEntryPrefab, FriendsListContentParent.transform);
                UIFriendEntry uiEntry = friendUIObj.GetComponent<UIFriendEntry>();
                uiEntry.EnableFriendButton(false);

                uiEntry.SetFriendData(friend);

                if (UIFriendInteractionSource != null)
                {
                    var friendButtonState = UIFriendInteractionSource.GetFriendInteractionState(friend);
                    uiEntry.SetFriendbuttonText(UIFriendInteractionSource.GetFriendInteractButtonText());
                    uiEntry.FriendInteractOnClick = UIFriendInteractionSource.OnFriendInteractButtonClicked;
                    switch (friendButtonState)
                    {
                        case UIFriendInteractionSource.FriendInteractionState.Hidden:
                            uiEntry.EnableFriendButton(false);
                            break;

                        case UIFriendInteractionSource.FriendInteractionState.Disabled:
                            uiEntry.EnableFriendButtonInteraction(false);
                            uiEntry.EnableFriendButton(true);
                            break;

                        case UIFriendInteractionSource.FriendInteractionState.Enabled:
                            uiEntry.EnableFriendButtonInteraction(true);
                            uiEntry.EnableFriendButton(true);
                            break;
                    }
                }

                if (friend.Status == FriendsStatus.Friends && friend.Presence != null)
                {
                    uiEntry.Status.text = friend.Presence.Status.ToString();
                    switch (friend.Presence.Status)
                    {
                        case Status.Away:
                            uiEntry.Status.color = Color.yellow;
                            break;

                        case Status.Online:
                            uiEntry.Status.color = Color.green;
                            break;

                        case Status.Offline:
                            uiEntry.Status.color = Color.gray;
                            break;

                        case Status.DoNotDisturb:
                            uiEntry.Status.color = Color.red;
                            break;

                        case Status.ExtendedAway:
                            uiEntry.Status.color = new Vector4(1, .05f, 0, 1); //orange
                            break;


                    }
                }
                else
                {
                    uiEntry.Status.text = friend.Status.ToString();
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
            var panelRT = FriendsPanel.transform as RectTransform;
            var newPos = panelRT.anchoredPosition;
            newPos.x = 0;
            panelRT.anchoredPosition = newPos;

            FriendOverlayContent.SetActive(false);
            UIActions.OnCollapseFriendsTab?.Invoke();

            collapsed = true;
        }

        public void ExpandFriendsTab()
        {  
            FriendOverlayContent.SetActive(true);
            UIActions.OnExpandFriendsTab?.Invoke();

            var panelRT = FriendsPanel.transform as RectTransform;
            var newPos = panelRT.anchoredPosition;
            newPos.x = initialPanelAnchoredPosX;
            panelRT.anchoredPosition = newPos;
         
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
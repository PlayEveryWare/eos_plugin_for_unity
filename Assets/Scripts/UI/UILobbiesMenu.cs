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
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Lobby;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UILobbiesMenu : MonoBehaviour
    {
        [Header("Lobbies UI - Create Options")]
        public GameObject LobbiesUIParent;
        public ConsoleInputField BucketIdVal;
        public Dropdown MaxPlayersVal;
        public Dropdown LevelVal;
        public Dropdown PermissionVal;
        public Toggle AllowInvitesVal;
        public Toggle PresenceEnabledVal;
        public Toggle RTCVoiceRoomEnabledVal;

        // Create/Modify/Leave UI
        public Button CreateLobbyButton;
        public Button LeaveLobbyButton;
        public Button ModifyLobbyButton;
        public Button AddMemberAttributeButton;

        // Current Lobby
        public Text LobbyIdVal;
        public Text OwnerIdVal;

        [Header("Lobbies UI - Lobby Members")]
        public GameObject UIMemberEntryPrefab;
        public GameObject MemberContentParent;

        [Header("Lobbies UI - Search")]
        public GameObject UILobbyEntryPrefab;
        public GameObject SearchContentParent;

        public ConsoleInputField SearchByBucketIdBox;
        public ConsoleInputField SearchByLevelBox;
        public ConsoleInputField SearchByLobbyIdBox;

        [Header("Lobbies UI - Invite PopUp")]
        public GameObject UIInvitePanel;
        public Text InviteFromVal;
        public Text InviteLevelVal;
        public Toggle InvitePresence;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        // UI Cache
        private int lastMemberCount = 0;
        private ProductUserId currentLobbyOwnerCache;
        private bool lastCurrentLobbyIsValid = false;

        private List<UIMemberEntry> UIMemberEntries = new List<UIMemberEntry>();

        private EOSLobbyManager LobbyManager;
        private EOSFriendsManager FriendsManager;

        public void Awake()
        {
            // Hide Invite Pop-up (Default)
            UIInvitePanel.SetActive(false);

            HideMenu();
        }

        private void Start()
        {
            SearchByBucketIdBox.InputField.onEndEdit.AddListener(SearchByBucketAttributeEnterPressed);
            SearchByLevelBox.InputField.onEndEdit.AddListener(SearchByLevelAttributeEnterPressed);
            SearchByLobbyIdBox.InputField.onEndEdit.AddListener(SearchByLobbyIdEnterPressed);

            LobbyManager = EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
        }

        private void Update()
        {
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            if (productUserId == null || !productUserId.IsValid())
            {
                return;
            }

            if (!LobbyManager._Dirty)
            {
                return;
            }

            Lobby currentLobby = LobbyManager.GetCurrentLobby();

            if (currentLobby.IsValid())
            {
                bool ownerChanged = false;

                /* TODO: Cache external/non-friend accounts
                if(!currentLobby.LobbyOwnerAccountId.IsValid())
                {
                    currentLobby.LobbyOwnerAccountId = FriendsManager.GetAccountMapping(currentLobby.LobbyOwner);

                    if(!currentLobby.LobbyOwnerAccountId.IsValid())
                    {
                        Debug.LogWarning("UILobbiesMenu (Update): LobbyOwner EpicAccountId not found in cache, need to query...");
                        // If still invalid, need to query for account information
                        // TODO query non cached
                    }
                }

                if(currentLobby.LobbyOwnerAccountId.IsValid() && string.IsNullOrEmpty(currentLobby.LobbyOwnerDisplayName))
                {
                    currentLobby.LobbyOwnerDisplayName = FriendsManager.GetDisplayName(currentLobby.LobbyOwnerAccountId);

                    if(string.IsNullOrEmpty(currentLobby.LobbyOwnerDisplayName))
                    {
                        Debug.LogWarning("UILobbiesMenu (Update): LobbyOwner DisplayName not found in cache, need to query...");
                        // No cached display name found for user, need to query for account information
                        // TODO query non cached
                    }
                }
                */

                // Cache LobbyOwner
                if (currentLobbyOwnerCache != currentLobby.LobbyOwner)
                {
                    ownerChanged = true;
                    Result resultLobbyOwner = currentLobby.LobbyOwner.ToString(out string outBuffer);
                    if (resultLobbyOwner == Result.Success)
                    {
                        // Update owner
                        OwnerIdVal.text = outBuffer;
                    }
                    else
                    {
                        OwnerIdVal.text = "Error: " + resultLobbyOwner;
                    }

                    UIOnLobbyUpdated(Result.Success);
                    currentLobbyOwnerCache = currentLobby.LobbyOwner;
                }

                // Only update if MemberCount changes
                if (lastMemberCount != currentLobby.Members.Count || ownerChanged)
                {
                    lastMemberCount = currentLobby.Members.Count;

                    // Destroy current UI member list
                    foreach (Transform child in MemberContentParent.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }

                    UIMemberEntries.Clear();

                    //members
                    foreach (LobbyMember member in currentLobby.Members)
                    {
                        GameObject memberUIObj = Instantiate(UIMemberEntryPrefab, MemberContentParent.transform);
                        UIMemberEntry uiEntry = memberUIObj.GetComponent<UIMemberEntry>();
                        if (uiEntry != null)
                        {
                            Result result = member.ProductId.ToString(out string outBuff);
                            if (result == Result.Success)
                            {
                                uiEntry.MemberName = outBuff;
                            }
                            else
                            {
                                uiEntry.MemberName = "Error: " + result;
                            }

                            uiEntry.ProductUserId = member.ProductId;
                            //uiEntry.IsOwner = currentLobby.LobbyOwner == member.ProductId;

                            uiEntry.IsTalkingText.text = "---";

                            uiEntry.MuteOnClick = MuteButtonOnClick;
                            uiEntry.KickOnClick = KickButtonOnClick;
                            uiEntry.PromoteOnClick = PromoteButtonOnClick;

                            UIMemberEntries.Add(uiEntry);
                        }
                    }
                }

                foreach(UIMemberEntry uiEntry in UIMemberEntries)
                {
                    uiEntry.UpdateUI();
                }
            }

            // Invites UI Prompt
            if (LobbyManager.GetCurrentInvite() != null)
            {
                UIInvitePanel.SetActive(true);

                Result resultInviteFrom = LobbyManager.GetCurrentInvite().FriendId.ToString(out string outBuffer);
                if (resultInviteFrom == Result.Success)
                {
                    // Update invite from
                    InviteFromVal.text = outBuffer;
                }
                else
                {
                    InviteFromVal.text = "Error: " + resultInviteFrom;
                }

                InviteLevelVal.text = LobbyManager.GetCurrentInvite().Lobby.Attributes[0].AsString;
            }
            else
            {
                UIInvitePanel.SetActive(false);
            }


            //Only When Valid Lobby changes
            if (currentLobby.IsValid() == lastCurrentLobbyIsValid)
            {
                return;
            }
            lastCurrentLobbyIsValid = currentLobby.IsValid();

            if (currentLobby.IsValid())
            {
                // Show Leave button and Update LobbyId UI
                UIOnLobbyUpdated(Result.Success);
            }
            else
            {
                // Clear UI
                UIOnLeaveLobby(Result.Success);
                lastMemberCount = 0;
                currentLobbyOwnerCache = null;
            }
        }

        // UI Button Methods

        public void CreateNewLobbyButtonOnClick()
        {

            if (BucketIdVal.InputField.text.Length == 0)
            {
                Debug.LogError("Tried to create new lobby but missing BucketId!");
                return;
            }

            Lobby lobbyProperties = new Lobby();
            // Bucket Id
            lobbyProperties.BucketId = BucketIdVal.InputField.text;

            // Max Players
            lobbyProperties.MaxNumLobbyMembers = (uint)Int32.Parse(MaxPlayersVal.options[MaxPlayersVal.value].text);

            // Level Attributes
            LobbyAttribute attribute = new LobbyAttribute();
            attribute.Key = "LEVEL";
            attribute.AsString = LevelVal.options[LevelVal.value].text;
            attribute.ValueType = AttributeType.String;
            attribute.Visibility = LobbyAttributeVisibility.Public;  // Needs to be public for Search
            lobbyProperties.Attributes.Add(attribute);

            // Permission
            string permissionStr = PermissionVal.options[PermissionVal.value].text;
            lobbyProperties.LobbyPermissionLevel = LobbyPermissionLevel.Publicadvertised; // Default
            if (permissionStr.Equals("Join Via Presence", StringComparison.OrdinalIgnoreCase))
            {
                lobbyProperties.LobbyPermissionLevel = LobbyPermissionLevel.Joinviapresence;
            }
            else if (permissionStr.Equals("Invite Only", StringComparison.OrdinalIgnoreCase))
            {
                lobbyProperties.LobbyPermissionLevel = LobbyPermissionLevel.Inviteonly;
            }

            // Allow Invites
            lobbyProperties.AllowInvites = AllowInvitesVal.isOn;

            // Presence Enabled
            lobbyProperties.PresenceEnabled = PresenceEnabledVal.isOn;

            // Voice Chat
            lobbyProperties.RTCRoomEnabled = RTCVoiceRoomEnabledVal.isOn;

            LobbyManager.CreateLobby(lobbyProperties, UIOnLobbyUpdated);
        }

        public void ModifyLobbyButtonOnClick()
        {
            Lobby currentLobby = LobbyManager.GetCurrentLobby();

            if (currentLobby == null || !currentLobby.IsValid())
            {
                Debug.LogError("UILobbiesMenu (ModifyLobbyButtonClicked): Lobby is invalid!");
                return;
            }

            // Bucket Id
            currentLobby.BucketId = BucketIdVal.InputField.text;

            // Max Players
            currentLobby.MaxNumLobbyMembers = (uint)Int32.Parse(MaxPlayersVal.options[MaxPlayersVal.value].text);

            // Level Attribute
            LobbyAttribute attribute = new LobbyAttribute();
            attribute.Key = "LEVEL";
            attribute.AsString = LevelVal.options[LevelVal.value].text;
            attribute.ValueType = AttributeType.String;
            attribute.Visibility = LobbyAttributeVisibility.Public;
            currentLobby.Attributes.Add(attribute);

            // Permission Level
            string permissionStr = PermissionVal.options[PermissionVal.value].text;
            currentLobby.LobbyPermissionLevel = LobbyPermissionLevel.Publicadvertised; // Default
            if (permissionStr.Equals("Presence", StringComparison.OrdinalIgnoreCase))
            {
                currentLobby.LobbyPermissionLevel = LobbyPermissionLevel.Joinviapresence;
            }
            else if (permissionStr.Equals("Invite Only", StringComparison.OrdinalIgnoreCase))
            {
                currentLobby.LobbyPermissionLevel = LobbyPermissionLevel.Inviteonly;
            }

            // Allow Invites
            currentLobby.AllowInvites = AllowInvitesVal.isOn;

            // Presence Enabled (cannot be modified)
            //currentLobby.PresenceEnabled = PresenceEnabledVal.isOn;

            LobbyManager.ModifyLobby(currentLobby, UIOnLobbyUpdated);
        }

        public void LeaveLobbyButtonOnClick()
        {
            LobbyManager.LeaveLobby(UIOnLeaveLobby);
        }

        public void AddMemberAttributeOnClick()
        {
            LobbyAttribute memberAttribute = new LobbyAttribute()
            {
                Key = "MemberAttribute",
                ValueType = AttributeType.String,
                Visibility = LobbyAttributeVisibility.Public,
                AsString = "TestValue"
            };

            LobbyManager.SetMemberAttribute(memberAttribute);
        }

        public void JoinButtonOnClick(Lobby lobbyRef, LobbyDetails lobbyDetailsRef)
        {
            LobbyManager.JoinLobby(lobbyRef.Id, lobbyDetailsRef, true, UIOnLobbyUpdated);
        }

        public void MuteButtonOnClick(ProductUserId productUserId)
        {
            LobbyManager.ToggleMute(productUserId, null);
        }

        public void KickButtonOnClick(ProductUserId productUserId)
        {
            LobbyManager.KickMember(productUserId, null);
        }

        public void PromoteButtonOnClick(ProductUserId productUserId)
        {
            LobbyManager.PromoteMember(productUserId, null);
        }

        public void AcceptInviteButtonOnClick()
        {
            bool invitePresenceToggled = InvitePresence.isOn;

            LobbyManager.AcceptCurrentLobbyInvite(invitePresenceToggled, UIOnLobbyUpdated);
        }

        public void DeclineInviteButtonOnClick()
        {
            LobbyManager.DeclineLobbyInvite();
        }

        public void LobbyInviteButtonOnClick(EpicAccountId userId)
        {
            // Set Current chat

            FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friends);

            if (friends.TryGetValue(userId, out FriendData friend))
            {
                if (friend.UserProductUserId == null || !friend.UserProductUserId.IsValid())
                {
                    Debug.LogError("UILobbiesMenu (LobbyInviteButtonOnClick): UserProductUserId is not valid!");
                }
                else
                {
                    LobbyManager.SendInvite(friend.UserProductUserId);
                }
            }
            else
            {
                Debug.LogError("UIPeer2PeerMenu (ChatButtonOnClick): Friend not found in cached data.");
            }
        }

        public bool IsCurrentLobbyValid()
        {
            Lobby currentLobby = LobbyManager.GetCurrentLobby();

            return currentLobby.IsValid();
        }

        private void UIOnLobbyUpdated(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UILobbiesMenu (UIOnLobbyUpdated): LobbyUpdate error '{0}'", result);
                return;
            }

            Lobby currentLobby = LobbyManager.GetCurrentLobby();

            if (!currentLobby.IsValid())
            {
                Debug.LogErrorFormat("UILobbiesMenu (UIOnLobbyUpdated): OnLobbyCreated returned invalid CurrentLobby.Id: {0}", currentLobby.Id);
                return;
            }

            string lobbyId = currentLobby.Id;

            if (!string.IsNullOrEmpty(lobbyId))
            {
                LobbyIdVal.text = currentLobby.Id; // Update UI
            }

            // Update UI Buttons
            CreateLobbyButton.gameObject.SetActive(false);

            if (currentLobby.IsOwner(EOSManager.Instance.GetProductUserId()))
            {
                Debug.Log("UIOnLobbyUpdated (UIOnLobbyUpdated): Joined as Host (enable ModifyLobby button)");
                ModifyLobbyButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("UIOnLobbyUpdated (UIOnLobbyUpdated): Joined as Client (disable ModifyLobby button)");
                ModifyLobbyButton.gameObject.SetActive(false);
            }

            LeaveLobbyButton.gameObject.SetActive(true);
            AddMemberAttributeButton.gameObject.SetActive(true);
        }

        private void UIOnLeaveLobby(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UILobbiesMenu (UIOnLeaveLobby): LeaveLobby error '{0}'", result);
                return;
            }

            LobbyIdVal.text = "{null}";
            OwnerIdVal.text = "{null}";
            currentLobbyOwnerCache = null;

            // Update UI Buttons
            CreateLobbyButton.gameObject.SetActive(true);
            ModifyLobbyButton.gameObject.SetActive(false);
            LeaveLobbyButton.gameObject.SetActive(false);
            AddMemberAttributeButton.gameObject.SetActive(false);

            // Destroy current UI member list
            foreach (Transform child in MemberContentParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // Search UI
        public void SearchByLevelAttributeEnterPressed(string searchAttributeValue)
        {
            LobbyManager.SearchByAttribute("LEVEL", searchAttributeValue, UIUpateSearchResults);
        }

        public void SearchByBucketAttributeEnterPressed(string searchAttributeValue)
        {
            LobbyManager.SearchByAttribute("bucket", searchAttributeValue, UIUpateSearchResults);
        }

        public void SearchByLobbyIdEnterPressed(string searchString)
        {
            LobbyManager.SearchByLobbyId(searchString, UIUpateSearchResults);
        }

        public void UIUpateSearchResults(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UILobbiesMenu (UpdateSearchResults): result error '{0}'", result);
                return;
            }

            // Destroy current UI member list
            foreach (Transform child in SearchContentParent.transform)
            {
                if (child != null)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            bool firstResultSelected = false;

            foreach (KeyValuePair<Lobby, LobbyDetails> kvp in LobbyManager.GetSearchResults())
            {
                if (kvp.Key == null)
                {
                    Debug.LogError("Lobbies (OnSearchResultsReceived): SearchResults has null key!");
                    continue;
                }

                if (kvp.Value == null)
                {
                    Debug.LogError("Lobbies (OnSearchResultsReceived): SearchResults has null Value!");
                    continue;
                }

                if (string.IsNullOrEmpty(kvp.Key.Id))
                {
                    Debug.LogWarning("Lobbies (OnSearchResultsReceived): Found lobby with null Id: ");
                    continue;
                }

                if (kvp.Key.LobbyOwner == null)
                {
                    Debug.LogWarningFormat("Lobbies (OnSearchResultsReceived): Found lobby with null LobbyOwner id: ", kvp.Key.Id);
                    continue;
                }

                GameObject searchUIObj = Instantiate(UILobbyEntryPrefab, SearchContentParent.transform);

                UISearchEntry uiEntry = searchUIObj.GetComponent<UISearchEntry>();
                if (uiEntry != null)
                {
                    /* TODO: Cache external/non-friend accounts
                    if (!kvp.Key.LobbyOwnerAccountId.IsValid())
                    {
                        kvp.Key.LobbyOwnerAccountId = FriendsManager.GetAccountMapping(kvp.Key.LobbyOwner);

                        if (!kvp.Key.LobbyOwnerAccountId.IsValid())
                        {
                            Debug.LogWarning("UILobbiesMenu (UIUpateSearchResults): LobbyOwner EpicAccountId not found in cache, need to query...");
                            // If still invalid, need to query for account information
                            // TODO query non cached
                        }
                    }

                    if (kvp.Key.LobbyOwnerAccountId.IsValid() && string.IsNullOrEmpty(kvp.Key.LobbyOwnerDisplayName))
                    {
                        uiEntry.OwnerName = FriendsManager.GetDisplayName(kvp.Key.LobbyOwnerAccountId);

                        if (string.IsNullOrEmpty(kvp.Key.LobbyOwnerDisplayName))
                        {
                            Debug.LogWarning("UILobbiesMenu (Update): LobbyOwner DisplayName not found in cache, need to query...");
                            // No cached display name found for user, need to query for account information
                            // TODO query non cached
                        }
                    }
                    else
                    */
                    {
                        uiEntry.OwnerName = kvp.Key.LobbyOwnerDisplayName;
                    }

                    // If DisplayName not found, display ProductUserId
                    if (string.IsNullOrEmpty(uiEntry.OwnerName))
                    {
                        Result resultLobbyOwner = kvp.Key.LobbyOwner.ToString(out string outBuff);
                        if (resultLobbyOwner == Result.Success)
                        {
                            uiEntry.OwnerName = outBuff;
                        }
                        else
                        {
                            uiEntry.OwnerName = "Error: " + resultLobbyOwner;
                        }
                    }

                    uiEntry.MaxMembers = (int)kvp.Key.MaxNumLobbyMembers;
                    uiEntry.Members = (int)(uiEntry.MaxMembers - kvp.Key.AvailableSlots);
                    uiEntry.LobbyRef = kvp.Key;
                    uiEntry.LobbyDetailsRef = kvp.Value;
                    uiEntry.JoinButtonOnClick = JoinButtonOnClick;

                    if(!firstResultSelected && EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(uiEntry.JoinButton.gameObject);
                        firstResultSelected = true;
                    }                    

                    // Get Level
                    Result attrResult = kvp.Value.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions() { AttrKey = "LEVEL" }, out Epic.OnlineServices.Lobby.Attribute outAttrbite);
                    if (attrResult == Result.Success)
                    {
                        uiEntry.Level = outAttrbite.Data.Value.AsUtf8;
                    }
                    else
                    {
                        uiEntry.Level = "Error: " + attrResult;
                    }

                    uiEntry.UpdateUI();
                }
            }
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>().OnLoggedIn();

            LobbiesUIParent.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void HideMenu()
        {
            LobbyManager?.OnLoggedOut();

            LobbiesUIParent.gameObject.SetActive(false);
        }
    }
}
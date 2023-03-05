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

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.Presence;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIPeer2PeerMenu : UIFriendInteractionSource, ISampleSceneUI
    {
        [Header("Peer 2 Peer UI")]
        public GameObject Peer2PeerUIParent;
        public GameObject ChatWindow;

        // Chat Window
        public Button CloseChatButton;
        public Text NATTypeText;
        public Text CurrentChatUserText;

        public GameObject ChatEntriesContentParent;
        public GameObject ChatEntryPrefab;

        public UIConsoleInputField ChatMessageInput;

        public UIConsoleInputField ProductUserIdInput;

        public UIPeer2PeerParticleController ParticleManager;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        // Private

        private EOSPeer2PeerManager Peer2PeerManager;
        private EOSFriendsManager FriendsManager;

        private string currentChatDisplayName;
        private ProductUserId currentChatProductUserId;

        void Start()
        {
            Peer2PeerManager = EOSManager.Instance.GetOrCreateManager<EOSPeer2PeerManager>();
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            Peer2PeerManager.ParticleController = ParticleManager;
            Peer2PeerManager.parent = this.transform;
            CloseChatOnClick();
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSPeer2PeerManager>();
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
        }
        public void ChatMessageEndEdit(string arg0)
        {
            SendOnClick();
        }

        private void Update()
        {
            ProductUserId messageFromPlayer = Peer2PeerManager.HandleReceivedMessages();

            if (messageFromPlayer != null)
            {
                IncomingChat(messageFromPlayer);
            }

            if (currentChatProductUserId == null || !currentChatProductUserId.IsValid())
            {
                return;
            }

            switch (Peer2PeerManager.GetNATType())
            {
                case NATType.Moderate:
                    NATTypeText.text = "Moderate";
                    break;
                case NATType.Open:
                    NATTypeText.text = "Open";
                    break;
                case NATType.Strict:
                    NATTypeText.text = "Strict";
                    break;
                case NATType.Unknown:
                    NATTypeText.text = "Unknown";
                    break;
            }

            if (Peer2PeerManager.GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> ChatDataDictionary))
            {
                // Destroy current UI chat entry list
                foreach (Transform child in ChatEntriesContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                if (ChatDataDictionary.TryGetValue(currentChatProductUserId, out ChatWithFriendData chatData))
                {
                    foreach (ChatEntry entry in chatData.ChatLines)
                    {
                        GameObject chatEntryUIObj = Instantiate(ChatEntryPrefab, ChatEntriesContentParent.transform);
                        UIChatEntry uiEntry = chatEntryUIObj.GetComponent<UIChatEntry>();

                        if (entry.isOwnEntry)
                        {
                            uiEntry.SetLocalEntry("Me", entry.Message);
                        }
                        else
                        {
                            uiEntry.SetRemoteEntry(currentChatDisplayName, entry.Message);
                        }
                    }
                }
            }
        }

        public override FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            return friendData.IsFriend() && friendData.IsOnline() ? FriendInteractionState.Enabled : FriendInteractionState.Hidden;
        }

        public override void OnFriendInteractButtonClicked(FriendData friendData)
        {
            ChatButtonOnClick(friendData.UserId);
        }

        public override string GetFriendInteractButtonText()
        {
            return "Chat";
        }

        public void ChatButtonOnClick(EpicAccountId userId)
        {
            // Set Current chat

            FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friends);

            if (friends.TryGetValue(userId, out FriendData friend))
            {
                if (friend.UserProductUserId == null || !friend.UserProductUserId.IsValid())
                {
                    Debug.LogError("UIPeer2PeerMenu (ChatButtonOnClick): UserProductUserId is not valid!");
                    return;
                }

                currentChatDisplayName = friend.Name;
                currentChatProductUserId = friend.UserProductUserId;

                CurrentChatUserText.text = currentChatDisplayName;

                ChatWindow.SetActive(true);
            }
            else
            {
                Debug.LogError("UIPeer2PeerMenu (ChatButtonOnClick): Friend not found in cached data.");
            }
        }

        public void IncomingChat(ProductUserId productUserId)
        {
            if (!productUserId.IsValid())
            {
                Debug.LogError("UIPeer2PeerMenu (IncomingChat): productUserId is not valid!");
                return;
            }

            if (currentChatProductUserId == null)
            {
                // Open chat window if no window is open
                FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friends);

                string displayName = string.Empty;
                foreach (FriendData friend in friends.Values)
                {
                    if (productUserId == friend.UserProductUserId)
                    {
                        // Found friend
                        ChatButtonOnClick(friend.UserId);
                        return;
                    }
                }

                currentChatDisplayName = productUserId.ToString();
                currentChatProductUserId = productUserId;

                CurrentChatUserText.text = currentChatDisplayName;

                ChatWindow.SetActive(true);
            }
            else
            {
                // TODO: Show notification in friends list of new message from friend other than current chat window
            }
        }

        public void CloseChatOnClick()
        {
            currentChatDisplayName = string.Empty;
            currentChatProductUserId = null;
            CurrentChatUserText.text = string.Empty;
            NATTypeText.text = string.Empty;

            // Destroy current UI chat entry list
            foreach (Transform child in ChatEntriesContentParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            ChatWindow.SetActive(false);
        }

        public void SetIdOnClick()
        {
            var productUserIdText = ProductUserIdInput.InputField.text;
            var productUserId = ProductUserId.FromString(productUserIdText);
            if (!productUserId.IsValid())
            {
                Debug.LogError("UIPeer2PeerMenu (SetIdOnClick): Invalid ProductUserId.");
                return;
            }

            currentChatDisplayName = productUserIdText;
            currentChatProductUserId = productUserId;

            CurrentChatUserText.text = productUserIdText;

            ChatWindow.SetActive(true);
        }

        public void SendOnClick()
        {
            if (string.IsNullOrEmpty(currentChatDisplayName))
            {
                Debug.LogError("UIPeer2PeerMenu (SendOnClick): No chat window open.");
                return;
            }

            if (string.IsNullOrEmpty(ChatMessageInput.InputField.text))
            {
                Debug.LogError("UIPeer2PeerMenu (SendOnClick): Message is empty.");
                return;
            }
            messageData message;
            message.textData = ChatMessageInput.InputField.text;
            message.type = messageType.textMessage;
            message.xPos = 0;
            message.yPos = 0;



            if (currentChatProductUserId == null || !currentChatProductUserId.IsValid())
            {
                Debug.LogError("UIPeer2PeerMenu (SendOnClick): ProductUserId for '{0}' is not valid!");
                return;
            }


            Peer2PeerManager.SendMessage(currentChatProductUserId, message);
            ChatMessageInput.InputField.text = string.Empty;
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSPeer2PeerManager>().OnLoggedIn();

            Peer2PeerUIParent.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);


            var presenceInterface = EOSManager.Instance.GetEOSPresenceInterface();
            var presenceModificationOptions = new CreatePresenceModificationOptions();
            presenceModificationOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();

            Result result = presenceInterface.CreatePresenceModification(ref presenceModificationOptions, out PresenceModification presenceModificationHandle);

            if (result == Result.Success)
            {

                //mark user as online
                var presenceModificationSetStatusOptions = new PresenceModificationSetStatusOptions();
                presenceModificationSetStatusOptions.Status = Status.Online;
                presenceModificationHandle.SetStatus(ref presenceModificationSetStatusOptions);

                var presenceModificationSetJoinOptions = new PresenceModificationSetJoinInfoOptions();

                presenceModificationSetJoinOptions.JoinInfo = "Custom Invite";
                presenceModificationHandle.SetJoinInfo(ref presenceModificationSetJoinOptions);

                // actually update all the status changes
                var setPresenceOptions = new Epic.OnlineServices.Presence.SetPresenceOptions();
                setPresenceOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
                setPresenceOptions.PresenceModificationHandle = presenceModificationHandle;
                presenceInterface.SetPresence(ref setPresenceOptions, null, (ref SetPresenceCallbackInfo data) => { });
                presenceModificationHandle.Release();
            }

        }

        public void HideMenu()
        {
            Peer2PeerManager?.OnLoggedOut();

            CloseChatOnClick();

            Peer2PeerUIParent.gameObject.SetActive(false);
        }

        public void ParticlesOnClick()
        {
            Debug.Log("UIPeer2PeerMenu (OnMouseDown): Mouse click recieved");
            Vector2 mousePos = Input.mousePosition;

            messageData message;
            message.type = messageType.coordinatesMessage;
            message.xPos = mousePos.x;
            message.yPos = mousePos.y;
            message.textData = null;

            if (currentChatProductUserId == null || !currentChatProductUserId.IsValid())
            {
                Debug.LogError("UIPeer2PeerMenu (SendOnClick): ProductUserId for '{0}' is not valid!");
                return;
            }


            Peer2PeerManager.SendMessage(currentChatProductUserId, message);
        }
    }
}
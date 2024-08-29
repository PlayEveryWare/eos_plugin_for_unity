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
    using System.Collections.Generic;
    using Epic.OnlineServices;
    using UnityEngine;
    using UnityEngine.UI;
    using Epic.OnlineServices.Presence;

    public class UIHighFrequencyPeer2PeerMenu : SampleMenuWithFriends
    {
        [Header("Peer 2 Peer UI")]
        public GameObject ChatWindow;

        // UI
        public Button CloseChatButton;
        public Text NATTypeText;
        public Text CurrentChatUserText;

        public GameObject ChatEntriesContentParent;
        public GameObject ChatEntryPrefab;

        public UIConsoleInputField ChatMessageInput;
        public UIConsoleInputField ProductUserIdInput;
        public UIPeer2PeerParticleController ParticleManager;
        public Slider refreshRateSlider;
        
        private EOSHighFrequencyPeer2PeerManager Peer2PeerManager;
        private EOSFriendsManager FriendsManager;

        private string currentChatDisplayName;
        private ProductUserId currentChatProductUserId;

        void Start()
        {
            Peer2PeerManager = EOSManager.Instance.GetOrCreateManager<EOSHighFrequencyPeer2PeerManager>();
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            Peer2PeerManager.ParticleController = ParticleManager;
            Peer2PeerManager.owner = this;
            Peer2PeerManager.parent = this.transform;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EOSManager.Instance.RemoveManager<EOSHighFrequencyPeer2PeerManager>();
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
        }

        public void ChatMessageEndEdit(string arg0)
        {
            SendOnClick();
        }

        public void ToggleHighFrequencySending()
        {
            if (Peer2PeerManager != null)
            {
                Peer2PeerManager.sendActive = !Peer2PeerManager.sendActive;
            }
        }
        protected override void Update()
        {
            base.Update();
            ProductUserId messageFromPlayer = Peer2PeerManager.HandleReceivedMessages();
            if (messageFromPlayer != null)
            {
                IncomingChat(messageFromPlayer);
            }

            if (currentChatProductUserId == null || !currentChatProductUserId.IsValid())
            {
                return;
            }

            if(Peer2PeerManager.sendActive && currentChatProductUserId != null)
            {
                Peer2PeerManager.P2PUpdate();
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

        public ProductUserId GetCurrentFriendId()
        {
            return currentChatProductUserId;
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

                //ChatWindow.SetActive(true);
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

                //ChatWindow.SetActive(true);
            }
            else
            {
                // TODO: Show notification in friends list of new message from friend other than current chat window
            }
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

           // ChatWindow.SetActive(true);
        }

        public void SendOnClick()
        {
            if (string.IsNullOrEmpty(currentChatDisplayName) && currentChatProductUserId == null)
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

            Peer2PeerManager.SendMessage(currentChatProductUserId, message.ToString());
            ChatMessageInput.InputField.text = string.Empty;
        }

        public override void Show()
        {
            base.Show();
            EOSManager.Instance.GetOrCreateManager<EOSPeer2PeerManager>().OnLoggedIn();

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

        public void SetRefreshRate(string hz)
        {
            Peer2PeerManager.refreshRate = int.Parse(hz);
            Debug.Log("UIPeer2PeerMenu (SetRefresshRate):Updated refresh rate to " + Peer2PeerManager.refreshRate + " Hz.");
        }

        public void SetPacketSize(string mb)
        {
            Peer2PeerManager.packetSizeMB = float.Parse(mb);
            Peer2PeerManager.updatePacketSize();
            Debug.Log("UIPeer2PeerMenu (SetPacketSize):Updated packet size to " + Peer2PeerManager.packetSizeMB + " Mb.");
        }

        public override void Hide()
        {
            base.Hide();
            Peer2PeerManager?.OnLoggedOut();
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

            Peer2PeerManager.SendMessage(currentChatProductUserId, message.ToString()) ;
        }
    }
}
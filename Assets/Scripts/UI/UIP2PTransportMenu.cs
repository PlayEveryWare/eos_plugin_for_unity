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
using PlayEveryWare.EpicOnlineServices.Samples;
using Epic.OnlineServices;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.UserInfo;

namespace PlayEveryWare.EpicOnlineServices.Samples.Network
{
    public class P2PTransportPresenceData
    {
        public const string ValidIdenfier = "P2PTRANSPORT";

        public string SceneIdentifier;
        public string ServerUserId;

        public bool IsValid()
        {
            return SceneIdentifier == ValidIdenfier;
        }
    }

    public class UIP2PTransportMenu : UIFriendInteractionSource, ISampleSceneUI
    {
        public UIFriendsMenu FriendUI;
        public GameObject PlayerNetworkPrefab;
        public Image Background;
        public GameObject StartHostButton;
        public GameObject DisconnectButton;
        public GameObject MovementControls;
        public RectTransform DisplayNameContainer;
        public float MovementStepAmount = 0.5f;

        private EOSTransportManager transportManager = null;
        private bool isHost = false;
        private bool isClient = false;
        private bool uiDirty = false;

        private ulong joinGameAcceptedNotifyHandle = 0;

        private readonly Vector2[] movementVectors = new Vector2[] { Vector2.up, Vector2.left, Vector2.down, Vector2.right };

        private void Start()
        {
            transportManager = EOSManager.Instance.GetOrCreateManager<EOSTransportManager>();
        }

        public override FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            if (isHost || isClient || string.IsNullOrEmpty(friendData.Presence?.JoinInfo))
            {
                return FriendInteractionState.Hidden;
            }

            P2PTransportPresenceData joinInfo = null;
            try
            {
                joinInfo = JsonUtility.FromJson<P2PTransportPresenceData>(friendData.Presence.JoinInfo);
            }
            catch(ArgumentException)
            {
                return FriendInteractionState.Hidden;
            }
            return joinInfo?.IsValid() == true ? FriendInteractionState.Enabled : FriendInteractionState.Hidden;
        }

        public override string GetFriendInteractButtonText()
        {
            return "Join";
        }

        public override void OnFriendInteractButtonClicked(FriendData friendData)
        {
            var joinInfo = JsonUtility.FromJson<P2PTransportPresenceData>(friendData.Presence.JoinInfo);
            if (joinInfo.IsValid())
            {
                var hostId = ProductUserId.FromString(joinInfo.ServerUserId);
                if (hostId.IsValid())
                {
                    JoinGame(hostId);
                }
                else
                {
                    Debug.LogError("UIP2PTransportMenu (OnFriendInteractButtonClicked): invalid server user id");
                }
            }
            else
            {
                Debug.LogError("UIP2PTransportMenu (OnFriendInteractButtonClicked): invalid join info");
            }
        }

        public override bool IsDirty()
        {
            return uiDirty;
        }

        public override void ResetDirtyFlag()
        {
            uiDirty = false;
        }

        public void StartHostOnClick()
        {
            if (isHost)
            {
                Debug.LogError("UIP2PTransportMenu (StartHostOnClick): already hosting");
                return;
            }

            if (transportManager.StartHost())
            {
                isHost = true;
                SetSessionUIActive(true);
                SetJoinInfo(EOSManager.Instance.GetProductUserId());
                uiDirty = true;
            }
            else
            {
                Debug.LogError("UIP2PTransportMenu (StartHostOnClick): failed to start host");
            }
        }

        private void SetJoinInfo(ProductUserId serverUserId)
        {
            var joinData = new P2PTransportPresenceData()
            {
                SceneIdentifier = P2PTransportPresenceData.ValidIdenfier,
                ServerUserId = serverUserId.ToString()
            };

            string joinString = JsonUtility.ToJson(joinData);

            EOSSessionsManager.SetJoinInfo(joinString);
        }

        public void DisconnectOnClick()
        {
            transportManager?.Disconnect();
            isHost = false;
            isClient = false;
            SetSessionUIActive(false);
            EOSSessionsManager.SetJoinInfo(null);
            uiDirty = true;
        }

        private void SetSessionUIActive(bool active)
        {
            StartHostButton.SetActive(!active);
            DisconnectButton.SetActive(active);
            MovementControls.SetActive(active);
        }

        private void OnJoinGameAccepted(ref JoinGameAcceptedCallbackInfo data)
        {
            var joinData = JsonUtility.FromJson<P2PTransportPresenceData>(data.JoinInfo);
            if (joinData.IsValid())
            {
                ProductUserId serverUserId = ProductUserId.FromString(joinData.ServerUserId);
                JoinGame(serverUserId);
            }
            else
            {
                Debug.LogError("UIP2PTransportMenu (OnJoinGameAccepted): invalid join info");
            }
        }

        private void OnDisconnect(ulong _)
        {
            Debug.LogWarning("UIP2PTransportMenu (OnDisconnect): server disconnected");
            isClient = false;
            SetSessionUIActive(false);
            EOSSessionsManager.SetJoinInfo(null);
            uiDirty = true;
            NetworkSamplePlayer.UnregisterDisconnectCallback(OnDisconnect);
        }

        private void JoinGame(ProductUserId hostId)
        {
            if (hostId.IsValid())
            {
                NetworkSamplePlayer.SetNetworkHostId(hostId);
                if (transportManager.StartClient())
                {
                    NetworkSamplePlayer.RegisterDisconnectCallback(OnDisconnect);
                    SetSessionUIActive(true);
                    isClient = true;
                    SetJoinInfo(hostId);
                    uiDirty = true;
                }
                else
                {
                    Debug.LogError("UIP2PTransportMenu (JoinGame): failed to start client");
                }
            }
            else
            {
                Debug.LogError("UIP2PTransportMenu (JoinGame): invalid server user id");
            }
        }

        private void AddJoinListener()
        {
            var presenceInterface = EOSManager.Instance.GetEOSPresenceInterface();
            if (presenceInterface == null)
            {
                Debug.LogError("UIP2PTransportMenu (AddJoinListener): presence interface not available");
            }
            var options = new AddNotifyJoinGameAcceptedOptions();
            joinGameAcceptedNotifyHandle = presenceInterface.AddNotifyJoinGameAccepted(ref options, null, OnJoinGameAccepted);
        }

        private void RemoveJoinListener()
        {
            if (joinGameAcceptedNotifyHandle != 0)
            {
                var presenceInterface = EOSManager.Instance.GetEOSPresenceInterface();
                presenceInterface?.RemoveNotifyJoinGameAccepted(joinGameAcceptedNotifyHandle);
                joinGameAcceptedNotifyHandle = 0;
            }
        }

        public void HideMenu()
        {
            if (isClient || isHost)
            {
                DisconnectOnClick();
            }

            Background.enabled = true;

            StartHostButton.SetActive(false);
            DisconnectButton.SetActive(false);
            MovementControls.SetActive(false);

            RemoveJoinListener();
        }

        public void ShowMenu()
        {
            Background.enabled = false;

            SetSessionUIActive(false);

            AddJoinListener();

            NetworkSamplePlayer.DisplayNameContainer = DisplayNameContainer;
            NetworkSamplePlayer.DisplayNameSetter = SetDisplayNameText;
        }

        private void OnDestroy()
        {
            transportManager?.Disconnect();

            NetworkSamplePlayer.DisplayNameContainer = null;
            NetworkSamplePlayer.DisplayNameSetter = null;
            NetworkSamplePlayer.DestoryNetworkManager();
        }

        public void MovePlayer(int direction)
        {
            if (isHost || isClient)
            {
                if (direction >= 0 && direction < 4)
                {
                    NetworkSamplePlayer.MoveOwnerPlayerObject(movementVectors[direction] * MovementStepAmount);
                }
            }
        }

        private void SetDisplayNameText(Text displayNameUI, EpicAccountId userId)
        {
            var userInfoManager = EOSManager.Instance.GetOrCreateManager<EOSUserInfoManager>();
            var userInfo = userInfoManager.GetUserInfoById(userId);
            if (userInfo.UserId?.IsValid() == true)
            {
                displayNameUI.text = userInfo.DisplayName;
            }
            else
            {
                userInfoManager.QueryUserInfoById(userId, (EpicAccountId UserId, Result QueryResult) => {
                    if (QueryResult == Result.Success)
                    {
                        var queriedUserInfo = userInfoManager.GetUserInfoById(userId);
                        if (queriedUserInfo.UserId.IsValid())
                        {
                            displayNameUI.text = queriedUserInfo.DisplayName;
                        }
                    }
                });
            }
        }
    }
}
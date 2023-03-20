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
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Epic.OnlineServices;
using Epic.OnlineServices.Presence;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        public GameObject TakeControlButton;
        public GameObject ReleaseControlButton;
        public RectTransform DisplayNameContainer;
        public float MovementSpeed = 1f;
        public GameObject PlayField;

        private EOSTransportManager transportManager = null;
        private bool isHost = false;
        private bool isClient = false;
        private bool uiDirty = false;
        private bool controllingCharacter = false;

        private ulong joinGameAcceptedNotifyHandle = 0;
        private Vector2 moveInitialPos;
        private Vector2? moveTarget = null;

        private void Start()
        {
            transportManager = EOSManager.Instance.GetOrCreateManager<EOSTransportManager>();
        }


#if ENABLE_INPUT_SYSTEM
        private bool ShouldStopControl()
        {
            return Keyboard.current?.escapeKey.wasPressedThisFrame == true ||
                Gamepad.current?.aButton.wasPressedThisFrame == true ||
                Gamepad.current?.bButton.wasPressedThisFrame == true ||
                Gamepad.current?.xButton.wasPressedThisFrame == true ||
                Gamepad.current?.yButton.wasPressedThisFrame == true;
        }

        public void Update()
        {
            if (controllingCharacter)
            {
                if (ShouldStopControl())
                {
                    StopControl();
                }
                else
                {
                    var keyboard = Keyboard.current;
                    var gamepad = Gamepad.current;

                    var movementVector = Vector2.zero;

                    if (keyboard != null)
                    {
                        if (keyboard.upArrowKey.isPressed)
                        {
                            movementVector += Vector2.up;
                        }
                        if (keyboard.downArrowKey.isPressed)
                        {
                            movementVector += Vector2.down;
                        }
                        if (keyboard.leftArrowKey.isPressed)
                        {
                            movementVector += Vector2.left;
                        }
                        if (keyboard.rightArrowKey.isPressed)
                        {
                            movementVector += Vector2.right;
                        }
                    }
                    if (gamepad != null)
                    {
                        movementVector += gamepad.dpad.ReadValue();
                        movementVector += gamepad.leftStick.ReadValue();
                    }
                    movementVector = Vector2.ClampMagnitude(movementVector, 1) * Time.deltaTime * MovementSpeed;
                    if (movementVector.sqrMagnitude != 0)
                    {
                        NetworkSamplePlayer.MoveOwnerPlayerObject(movementVector);
                        moveTarget = null;
                    }
                }

                if (moveTarget != null)
                {
                    MoveTowardsTarget();
                }
            }
        }
#else
        public void Update()
        {
            if (controllingCharacter)
            {
                if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
                {
                    StopControl();
                }
                else
                {
                    var movementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Time.deltaTime * MovementSpeed;
                    if (movementVector.sqrMagnitude != 0)
                    {
                        NetworkSamplePlayer.MoveOwnerPlayerObject(movementVector);
                        moveTarget = null;
                    }
                }

                if (moveTarget != null)
                {
                    MoveTowardsTarget();
                }
            }
        }
#endif

        private void MoveTowardsTarget()
        {
            var currentPos = NetworkSamplePlayer.GetOwnerPlayerPosition();
            if (currentPos != null && moveTarget != null)
            {
                var currentPos2D = new Vector2(currentPos.Value.x, currentPos.Value.y);
                if (Vector2.Dot(moveTarget.Value-moveInitialPos,moveTarget.Value-currentPos2D) < 0)
                {
                    //overshot the target
                    moveTarget = null;
                    return;
                }
                var diff = moveTarget.Value - currentPos2D;
                var dist = diff.magnitude;
                if (dist <= 0.01f)
                {
                    //target reached
                    moveTarget = null;
                    return;
                }
                var moveOffset = Vector2.ClampMagnitude(diff.normalized * MovementSpeed * Time.deltaTime, dist);
                NetworkSamplePlayer.MoveOwnerPlayerObject(moveOffset);
            }
            else
            {
                moveTarget = null;
            }
        }

        public void OnPointerClick(BaseEventData data)
        {
            var pointerData = data as PointerEventData;
            if (controllingCharacter && pointerData != null)
            {
                var screenPoint = new Vector3(pointerData.position.x, pointerData.position.y, 0);
                var worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
                var moveBounds = NetworkSamplePlayer.GetPlayerMovementBounds();
                var currentPos = NetworkSamplePlayer.GetOwnerPlayerPosition();
                if (moveBounds != null && currentPos != null)
                {
                    moveInitialPos = new Vector2(currentPos.Value.x, currentPos.Value.y);
                    var targetPoint = moveBounds.Value.ClosestPoint(worldPoint);
                    moveTarget = new Vector2(targetPoint.x, targetPoint.y);
                }
            }
        }

        private void StopControl()
        {
            controllingCharacter = false;
            moveTarget = null;
            TakeControlButton.SetActive(true);
            ReleaseControlButton.SetActive(false);
            if (TakeControlButton?.activeSelf == true)
            {
                EventSystem.current.SetSelectedGameObject(TakeControlButton);
            }
            else if (DisconnectButton?.activeSelf == true)
            {
                EventSystem.current.SetSelectedGameObject(DisconnectButton);
            }
            else if (StartHostButton?.activeSelf == true)
            {
                EventSystem.current.SetSelectedGameObject(StartHostButton);
            }
            Debug.Log("UIP2PTransportMenu: Character control deactivated");
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

        public void TakeControlOnClick()
        {
            //delay input start to avoid reading face button input again and stopping control
            TakeControlButton.SetActive(false);
            ReleaseControlButton.SetActive(true);
            Invoke("StartControl", 0.1f);
        }

        public void ReleaseControlOnClick()
        {
            StopControl();
        }

        private void StartControl()
        {
            TakeControlButton.SetActive(false);
            ReleaseControlButton.SetActive(true);
            //disconnect input from ui navigation
            //if set to null, the log gets spammed on input
            EventSystem.current.SetSelectedGameObject(DisplayNameContainer.gameObject);
            controllingCharacter = true;
            Debug.Log("UIP2PTransportMenu: Character control activated");
        }

        private void SetSessionUIActive(bool active)
        {
            StartHostButton.SetActive(!active);
            DisconnectButton.SetActive(active);
            TakeControlButton.SetActive(active);
            ReleaseControlButton.SetActive(false);
            PlayField.SetActive(active);

            if (!active)
            {
                EventSystem.current.SetSelectedGameObject(StartHostButton);
                controllingCharacter = false;
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(TakeControlButton);
            }
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
            TakeControlButton.SetActive(false);
            ReleaseControlButton.SetActive(false);
            PlayField.SetActive(false);

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
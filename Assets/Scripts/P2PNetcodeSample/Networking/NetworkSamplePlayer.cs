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


namespace PlayEveryWare.EpicOnlineServices.Samples.Network
{
    using UnityEngine;
    using UnityEngine.UI;
    using Epic.OnlineServices;
    using EpicOnlineServices;
    
#if COM_UNITY_MODULE_NETCODE
    using Unity.Netcode;
    using Unity.Collections;
#endif
    public class NetworkSamplePlayer
#if COM_UNITY_MODULE_NETCODE
    : NetworkBehaviour
#else
    : MonoBehaviour
#endif
    {
        public Text DisplayNameUIPrefab;
        public Bounds MovementBounds;

        public static RectTransform DisplayNameContainer;
        public delegate void DisplayNameSetterDelegate(Text displayNameUI, EpicAccountId userId);
        public static DisplayNameSetterDelegate DisplayNameSetter;

        public static void MoveOwnerPlayerObject(Vector2 offset)
        {
#if COM_UNITY_MODULE_NETCODE
            NetworkManager.Singleton?.LocalClient?.PlayerObject?.GetComponent<NetworkSamplePlayer>()?.Move(offset);
#endif
        }

        public static Vector3? GetOwnerPlayerPosition()
        {
#if COM_UNITY_MODULE_NETCODE
            return NetworkManager.Singleton?.LocalClient?.PlayerObject?.transform.position;
#else
            return null;
#endif
        }

        public static Bounds? GetPlayerMovementBounds()
        {
#if COM_UNITY_MODULE_NETCODE
            return NetworkManager.Singleton?.LocalClient?.PlayerObject?.GetComponent<NetworkSamplePlayer>()?.MovementBounds;
#else
            return null;
#endif
        }

        public static void RegisterDisconnectCallback(System.Action<ulong> callback)
        {
#if COM_UNITY_MODULE_NETCODE
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= callback;
                NetworkManager.Singleton.OnClientDisconnectCallback += callback;
            }
#endif
        }

        public static void UnregisterDisconnectCallback(System.Action<ulong> callback)
        {
#if COM_UNITY_MODULE_NETCODE
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= callback;
            }
#endif
        }

        public static void DestoryNetworkManager()
        {
#if COM_UNITY_MODULE_NETCODE
            if (NetworkManager.Singleton?.gameObject != null)
            {
                Destroy(NetworkManager.Singleton.gameObject);
            }
#endif
        }

        public static void SetNetworkHostId(ProductUserId userId)
        {
#if COM_UNITY_MODULE_NETCODE
            var transportLayer = NetworkManager.Singleton?.GetComponent<EOSTransport>();
            if (transportLayer != null)
            {
                transportLayer.ServerUserIdToConnectTo = userId;
            }
#endif
        }

#if COM_UNITY_MODULE_NETCODE
        private NetworkVariable<Vector2> position = new NetworkVariable<Vector2>();
        private NetworkVariable<Color> color = new NetworkVariable<Color>();
        private NetworkVariable<FixedString64Bytes> userId = new NetworkVariable<FixedString64Bytes>();

        private Text displayNameUI;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                GenerateColorServerRpc();
                RandomPosServerRpc();
                SetUserIdServerRpc(EOSManager.Instance.GetLocalUserId().ToString());
            }

            color.OnValueChanged = OnColorChanged;
            position.OnValueChanged = OnPositionChanged;
            userId.OnValueChanged = OnUserIdChanged;

            if (DisplayNameContainer != null && DisplayNameUIPrefab != null)
            {
                displayNameUI = Instantiate(DisplayNameUIPrefab, DisplayNameContainer);
            }

            UpdateColor();
            UpdatePosition();
            UpdateUserId();
        }

        public override void OnNetworkDespawn()
        {
            if (displayNameUI != null)
            {
                Destroy(displayNameUI.gameObject);
            }
        }

        [ServerRpc]
        private void GenerateColorServerRpc()
        {
            color.Value = Color.HSVToRGB(Random.value, 1, 1);
        }

        private void OnColorChanged(Color oldColor, Color newColor)
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            GetComponent<Renderer>().material.color = color.Value;
        }

        [ServerRpc]
        private void SetUserIdServerRpc(string id)
        {
            userId.Value = id;
        }

        private void OnUserIdChanged(FixedString64Bytes oldId, FixedString64Bytes newId)
        {
            UpdateUserId();
        }

        private void UpdateUserId()
        {
            if (displayNameUI != null && DisplayNameSetter != null)
            {
                displayNameUI.text = userId.Value.ToString();
                DisplayNameSetter(displayNameUI, EpicAccountId.FromString(userId.Value.ToString()));
            }
        }

        [ServerRpc]
        private void RandomPosServerRpc()
        {
            float randomX = (((Random.value * 2) - 1) * MovementBounds.extents.x) + MovementBounds.center.x;
            float randomY = (((Random.value * 2) - 1) * MovementBounds.extents.y) + MovementBounds.center.y;
            position.Value = new Vector2(randomX, randomY);
        }

        public void Move(Vector2 offset)
        {
            MoveServerRpc(offset);
        }

        [ServerRpc]
        private void MoveServerRpc(Vector2 offset)
        {
            Vector2 currentPos = position.Value;
            Vector2 newPos = currentPos + offset;
            position.Value = MovementBounds.ClosestPoint(newPos);
        }

        private void OnPositionChanged(Vector2 oldPos, Vector2 newPos)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector2 newPos = position.Value;
            transform.position = new Vector3(newPos.x, newPos.y, 0);

            if (displayNameUI != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(DisplayNameContainer, screenPos, null, out Vector2 rectPos);
                (displayNameUI.transform as RectTransform).anchoredPosition = rectPos;
            }
        }

#endif
    }
}
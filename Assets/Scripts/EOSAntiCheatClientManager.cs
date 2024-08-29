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

    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using Epic.OnlineServices.AntiCheatCommon;
    using Epic.OnlineServices.AntiCheatClient;

    /// <summary>
    /// Class <c>EOSAntiCheatClientManager</c> is a simplified wrapper for EOS [AntiCheat Client Interface](https://dev.epicgames.com/docs/services/en-US/GameServices/AntiCheat/index.html).
    /// </summary>
    public class EOSAntiCheatClientManager : IEOSSubManager, IConnectInterfaceEventListener, IAuthInterfaceEventListener
    {
        private AntiCheatClientInterface AntiCheatHandle;
        private ConnectInterface ConnectHandle;

        //true = P2P, false = Client-Server
        public const bool PeerToPeerMode = true;

        public delegate void OnAntiCheatClientCallback(Result result);

        private IdToken? LocalUserIdToken = null;

        private bool SessionActive = false;

        //peer handles represent the index into this list
        //in a real use case handles would ideally represent a pointer to player data or object
        List<ProductUserId> RegisteredPeerList;
        Dictionary<ProductUserId, int> RegisteredPeerMapping;

        List<OnClientIntegrityViolatedCallback> ClientIntegrityViolatedCallbacks;
        List<OnMessageToPeerCallback> MessageToPeerCallbacks;
        List<OnPeerAuthStatusChangedCallback> PeerAuthStatusChangedCallbacks;
        List<OnPeerActionRequiredCallback> PeerActionRequiredCallbacks;
        

        public EOSAntiCheatClientManager()
        {
            AntiCheatHandle = EOSManager.Instance.GetEOSPlatformInterface().GetAntiCheatClientInterface();
            ConnectHandle = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();

            RegisteredPeerList = new List<ProductUserId>();
            //avoid registering peer with zero index
            RegisteredPeerList.Add(null);
            RegisteredPeerMapping = new Dictionary<ProductUserId, int>();

            ClientIntegrityViolatedCallbacks = new List<OnClientIntegrityViolatedCallback>();
            MessageToPeerCallbacks = new List<OnMessageToPeerCallback>();
            PeerAuthStatusChangedCallbacks = new List<OnPeerAuthStatusChangedCallback>();
            PeerActionRequiredCallbacks = new List<OnPeerActionRequiredCallback>();

            if (AntiCheatHandle == null)
            {
#if !UNITY_ANDROID && !UNITY_IOS
                Debug.LogError("AntiCheatClientManager (ctor): unable to get handle to AntiCheatClientInterface");
#endif
                return;
            }

            var notifyIntegrityOptions = new AddNotifyClientIntegrityViolatedOptions();
            AntiCheatHandle.AddNotifyClientIntegrityViolated(ref notifyIntegrityOptions, null, OnClientIntegrityViolated);

            if (PeerToPeerMode)
            {
                var messageOptions = new AddNotifyMessageToPeerOptions();
                AntiCheatHandle.AddNotifyMessageToPeer(ref messageOptions, null, OnMessageToPeer);

                var authStatusOptions = new AddNotifyPeerAuthStatusChangedOptions();
                AntiCheatHandle.AddNotifyPeerAuthStatusChanged(ref authStatusOptions, null, OnPeerAuthStatusChanged);

                var peerActionOptions = new AddNotifyPeerActionRequiredOptions();
                AntiCheatHandle.AddNotifyPeerActionRequired(ref peerActionOptions, null, OnPeerActionRequired);

            }
            else
            {
                //TODO: AddNotifyMessageToServer when using Client-Server mode
            }

            if (EOSManager.Instance.GetProductUserId() != null)
            {
                GetLocalIdToken();
                VerifyIdToken(LocalUserIdToken);
            }
        }

        /// <summary>
        /// Check if EAC functionality is availble
        /// </summary>
        /// <returns>False if EAC client functionality is not available i.e. game was launched without EAC bootstrapper</returns>
        public bool IsAntiCheatAvailable()
        {
            return AntiCheatHandle != null;
        }

        public void OnConnectLogin(LoginCallbackInfo loginCallbackInfo)
        {
            GetLocalIdToken();
            VerifyIdToken(LocalUserIdToken);
        }

        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
        {
            GetLocalIdToken();
            VerifyIdToken(LocalUserIdToken);
        }

        public void OnAuthLogout(Epic.OnlineServices.Auth.LogoutCallbackInfo logoutCallbackInfo)
        {
            LocalUserIdToken = null;
        }

        private void OnClientIntegrityViolated(ref OnClientIntegrityViolatedCallbackInfo data)
        {
            Debug.LogErrorFormat("AntiCheatClient (OnClientIntegrityViolated): Type:{0}, Message:\"{1}\"", data.ViolationType, data.ViolationMessage);
            foreach (var callback in ClientIntegrityViolatedCallbacks)
            {
                callback?.Invoke(ref data);
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_AntiCheatClient_AddNotifyClientIntegrityViolated](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_AddNotifyClientIntegrityViolated/index.html)
        /// </summary>
        /// <param name="Callback">Callback to receive notification of client integriy violation (modification of program memory or protected files, etc.</param>
        public void AddNotifyClientIntegrityViolated(OnClientIntegrityViolatedCallback Callback)
        {
            ClientIntegrityViolatedCallbacks.Add(Callback);
        }

        public void RemoveNotifyClientIntegrityViolated(OnClientIntegrityViolatedCallback Callback)
        {
            ClientIntegrityViolatedCallbacks.Remove(Callback);
        }

        private void OnMessageToPeer(ref OnMessageToClientCallbackInfo data)
        {
            Debug.Log("AntiCheatClient (OnMessageToPeer)");
            foreach (var callback in MessageToPeerCallbacks)
            {
                callback?.Invoke(ref data);
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_AntiCheatClient_AddNotifyMessageToPeer](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_AddNotifyMessageToPeer/index.html)
        /// </summary>
        /// <param name="Callback">Callback to receive message data to send to peer</param>
        public void AddNotifyToMessageToPeer(OnMessageToPeerCallback Callback)
        {
            MessageToPeerCallbacks.Add(Callback);
        }

        public void RemoveNotifyMessageToPeer(OnMessageToPeerCallback Callback)
        {
            MessageToPeerCallbacks.Remove(Callback);
        }

        /// <summary>
        /// Wrapper for calling [EOS_AntiCheatClient_ReceiveMessageFromPeer]https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_ReceiveMessageFromPeer/index.html)
        /// </summary>
        /// <param name="PeerHandle"><c>IntPtr</c> referencing another player</param>
        /// <param name="Data"><c>ArraySegment&lt;byte&gt;</c> previously received from <c>AddNotifyToMessageToPeer</c> callback</param>
        public void ReceiveMessageFromPeer(IntPtr PeerHandle, ArraySegment<byte> Data)
        {
            var options = new ReceiveMessageFromPeerOptions()
            {
                PeerHandle = PeerHandle,
                Data = Data
            };
            var result = AntiCheatHandle.ReceiveMessageFromPeer(ref options);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("AntiCheatClient (ReceiveMessageFromPeer): result code: {0}", result);
            }
        }

        private void OnPeerAuthStatusChanged(ref OnClientAuthStatusChangedCallbackInfo data)
        {
            Debug.LogFormat("AntiCheatClient (OnPeerAuthStatusChanged): handle: {0}, status: {1}", data.ClientHandle.ToInt32(), data.ClientAuthStatus);
            foreach (var callback in PeerAuthStatusChangedCallbacks)
            {
                callback?.Invoke(ref data);
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_AntiCheatClient_AddNotifyPeerAuthStatusChanged](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_AddNotifyPeerAuthStatusChanged/index.html)
        /// </summary>
        /// <param name="Callback">Callback to receive notification when peer auth is complete</param>
        public void AddNotifyPeerAuthStatusChanged(OnPeerAuthStatusChangedCallback Callback)
        {
            PeerAuthStatusChangedCallbacks.Add(Callback);
        }

        public void RemoveNotifyPeerAuthStatusChanged(OnPeerAuthStatusChangedCallback Callback)
        {
            PeerAuthStatusChangedCallbacks.Remove(Callback);
        }

        private void OnPeerActionRequired(ref OnClientActionRequiredCallbackInfo data)
        {
            Debug.Log("AntiCheatClient (OnPeerActionRequired)");
            foreach (var callback in PeerActionRequiredCallbacks)
            {
                callback?.Invoke(ref data);
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_AntiCheatClient_AddNotifyPeerActionRequired](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_AddNotifyPeerActionRequired/index.html)
        /// </summary>
        /// <param name="Callback">Callback to receive notification about action required for a peer (usually removal from the session)</param>
        public void AddNotifyPeerActionRequired(OnPeerActionRequiredCallback Callback)
        {
            PeerActionRequiredCallbacks.Add(Callback);
        }

        public void RemoveNotifyPeerActionRequired(OnPeerActionRequiredCallback Callback)
        {
            PeerActionRequiredCallbacks.Remove(Callback);
        }

        /// <summary>
        /// Get <c>ProductUserId</c> of registered peer by index
        /// </summary>
        /// <returns><c>ProductUserId</c> of peer, or null if not registered</returns>
        public ProductUserId GetPeerId(IntPtr PeerHandle)
        {
            int peerIndex = PeerHandle.ToInt32();
            if (peerIndex < RegisteredPeerList.Count)
            {
                return RegisteredPeerList[peerIndex];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get index of registered peer by <c>ProductUserId</c> as <c>IntPtr</c>
        /// </summary>
        /// <returns><c>IntPtr</c>representation of peer index for use with <c>GetPeerId</c> or <c>ReceiveMessageFromPeer</c></returns>
        public IntPtr GetPeerHandle(ProductUserId UserId)
        {
            RegisteredPeerMapping.TryGetValue(UserId, out int handle);
            return new IntPtr(handle);
        }

        /// <summary>
        /// Wrapper for calling [EOS_AntiCheatClient_RegisterPeer](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_RegisterPeer/index.html)
        /// TODO: Use ID tokens to verify player platform to determine whether their client must be protected or not (console players would use UnprotectedClient)
        /// </summary>
        /// <param name="UserId"><c>ProductUserId</c> of peer to register</param>
        /// <returns>True if peer was successfully registered, or was already registered</returns>
        public bool RegisterPeer(ProductUserId UserId)
        {
            if (IsRegisteredPeer(UserId))
            {
                Debug.LogWarning("AntiCheatClient (RegisterPeer): peer already registered");
                return true;
            }

            int peerIndex = RegisteredPeerList.Count;
            var options = new RegisterPeerOptions()
            {
                PeerHandle = new IntPtr(peerIndex),
                //TODO: get platform and protection status with connect interface
                ClientType = AntiCheatCommonClientType.ProtectedClient,
                ClientPlatform = AntiCheatCommonClientPlatform.Windows,
                AuthenticationTimeout = 60,
                PeerProductUserId = UserId
            };
            var result = AntiCheatHandle.RegisterPeer(ref options);
            if (result == Result.Success)
            {
                RegisteredPeerMapping[UserId] = peerIndex;

                Debug.Log("AntiCheatClient (RegisterPeer): successfully registered peer");
                return true;
            }
            else
            {
                Debug.LogFormat("AntiCheatClient (RegisterPeer): failed to register peer, result code: {0}", result);
                return false;
            }
        }

        /// <summary>
        /// Checks if a user with a given <c>ProductUserId</c> is registered as a peer
        /// </summary>
        /// <param name="UserId"><c>ProductUserId</c> of peer</param>
        /// <returns>True if peer is registered</returns>
        public bool IsRegisteredPeer(ProductUserId UserId)
        {
            return RegisteredPeerMapping.ContainsKey(UserId);
        }

        /// <summary>
        /// Wrapper for calling [EOS_AntiCheatClient_UnregisterPeer](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_UnregisterPeer/index.html)
        /// </summary>
        /// <param name="UserId"><c>ProductUserId</c> of peer to unregister</param>
        /// <returns>True if peer was successfully unregistered, or was already not registered</returns>
        public bool UnregisterPeer(ProductUserId UserId)
        {
            int peerIndex;
            if (!RegisteredPeerMapping.TryGetValue(UserId, out peerIndex))
            {
                Debug.LogWarning("AntiCheatClient (UnregisterPeer): peer not registered");
                return true;
            }

            var options = new UnregisterPeerOptions()
            {
                PeerHandle = new IntPtr(peerIndex)
            };
            var result = AntiCheatHandle.UnregisterPeer(ref options);
            if (result == Result.Success)
            {
                RegisteredPeerMapping.Remove(UserId);
                RegisteredPeerList[peerIndex] = null;

                Debug.Log("AntiCheatClient (RegisterPeer): successfully unregistered peer");
                return true;
            }
            else
            {
                Debug.LogFormat("AntiCheatClient (RegisterPeer): failed to unregister peer, result code: {0}", result);
                return false;
            }
        }

        /// <summary>
        /// Wrapper for calling [EOS_AntiCheatClient_BeginSession](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_BeginSession/index.html)
        /// TODO: Add support for client-server mode
        /// </summary>
        public void BeginSession()
        {
            if (SessionActive)
            {
                Debug.LogErrorFormat("AntiCheatClient (BeginSession): session already active");
                return;
            }

            var options = new BeginSessionOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Mode = AntiCheatClientMode.PeerToPeer
            };
            var result = AntiCheatHandle.BeginSession(ref options);
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("AntiCheatClient (BeginSession): failed to begin session, result code: {0}", result);
            }
        }

        /// <summary>
        /// Wrapper for calling [EOS_AntiCheatClient_EndSession](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/AntiCheatClient/EOS_AntiCheatClient_EndSession/index.html)
        /// </summary>
        public void EndSession()
        {
            if (!SessionActive)
            {
                Debug.LogErrorFormat("AntiCheatClient (BeginSession): session not active");
                return;
            }

            var options = new EndSessionOptions();
            var result = AntiCheatHandle.EndSession(ref options);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("AntiCheatClient (EndSession): failed to end session, result code: {0}", result);
            }
        }

        /// <summary>
        /// Checks if a protected EAC sessionis active
        /// </summary>
        /// <returns>True if session is active</returns>
        public bool IsSessionActive()
        {
            return SessionActive;
        }

        private void GetLocalIdToken()
        {
            CopyIdTokenOptions options = new CopyIdTokenOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            var result = ConnectHandle.CopyIdToken(ref options, out LocalUserIdToken);

            if (result != Result.Success)
            {
                Debug.LogError("AntiCheatClient (GetLocalIdToken): failed to copy local user id token");
            }
        }
    
        //used to verify an ID token
        //currently does not successfully get platform or device type information
        private void VerifyIdToken(IdToken? Token)
        {
            if(Token == null)
            {
                Debug.LogError("AntiCheatClient (VerifyIdToken):id token is null");
                return;
            }

            VerifyIdTokenOptions options = new VerifyIdTokenOptions()
            {
                IdToken = Token
            };
            ConnectHandle.VerifyIdToken(ref options, null, OnVerifyIdTokenComplete);
        }

        private void OnVerifyIdTokenComplete(ref VerifyIdTokenCallbackInfo data)
        {
            Debug.LogFormat("AntiCheatClient (VerifyIdToken): Result:{0}, Platform:\"{1}\" DeviceType:\"{2}\"", data.ResultCode, data.Platform, data.DeviceType);
        }
    }
}
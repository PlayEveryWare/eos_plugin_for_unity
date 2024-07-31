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

namespace PlayEveryWare.EpicOnlineServices.Samples.OpenId
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using static PlayEveryWare.EpicOnlineServices.EOSManager;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;

    //This sample uses a Google Cloud Function to implement a basic OpenID authentication server.
    //Before using the OpenIdRequestManager, a NodeJS cloud function must be created on Google Cloud Platform
    //using the index.js and package.json files found in the CloudFunction directory and a private key added by setting an environment variable named SECRET.
    //A Firestore database must me created using the structure shown in FirestoreSchema.png.
    //Then, an OpenID Identity Provider must be added through the EOS dev portal using the following config:
    //Type: UserInfo Endpoint
    //UserInfo API Endpoint: <cloud function URL>/userinfo
    //HTTP Method: GET
    //Name of the AccountId field: sub
    //Name of the DisplayName field: sub
    [DisallowMultipleComponent]
    public class OpenIdRequestManager : MonoBehaviour
    {
        //URL endpoint for the tokens path of the GCP function
        //for the supplied cloud function sample this would be <cloud function URL>/tokens
        const string tokensUrl = "";

        protected static OpenIdRequestManager s_instance;
        public static OpenIdRequestManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    return new GameObject("OpenIdRequestManager").AddComponent<OpenIdRequestManager>();
                }
                else
                {
                    return s_instance;
                }
            }
        }

        protected bool m_bInitialized = false;
        public static bool Initialized
        {
            get
            {
                return Instance.m_bInitialized;
            }
        }

#if UNITY_2019_3_OR_NEWER
        // In case of disabled Domain Reload, reset static members before entering Play Mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            s_instance = null;
        }
#endif

        protected virtual void Awake()
        {
            // Only one instance of DiscordManager at a time!
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;

            // We want our DiscordManager Instance to persist across scenes.
            DontDestroyOnLoad(gameObject);

            m_bInitialized = true;
        }

        protected virtual void OnEnable()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }

            if (!m_bInitialized)
            {
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            s_instance = null;
        }

        [Serializable]
        private struct UserResponse
        {
            public string user;
            public string id_token;
        }

        [Serializable]
        private struct TokenResponse
        {
            public UserResponse[] tokens;
        }

        [Serializable]
        private struct UserRequest
        {
            public string username;
            public string password;
        }

        [Serializable]
        private struct TokenRequest
        {
            public UserRequest[] requests;
        }

        private Dictionary<string,string> cachedTokens = new Dictionary<string, string>();

        public void StartConnectLoginWithOpenID(string username, string password, OnConnectLoginCallback onLoginCallback)
        {
            StartConnectLoginWithOpenID(username, password, onLoginCallback, retryAttemptNumber: 0);
        }

        private void StartConnectLoginWithOpenID(string username, string password, OnConnectLoginCallback onLoginCallback, int retryAttemptNumber = 0)
        {
            const int MaximumNumberOfRetries = 1;

            OpenId.OpenIdRequestManager.Instance.RequestToken(username, password, (string username, string id_token) =>
            {
                if (string.IsNullOrEmpty(id_token))
                {
                    onLoginCallback?.Invoke(new Epic.OnlineServices.Connect.LoginCallbackInfo() { ResultCode = Epic.OnlineServices.Result.InvalidAuth });
                    return;
                }

                EOSManager.Instance.StartConnectLoginWithOptions(ExternalCredentialType.OpenidAccessToken, id_token, onloginCallback: (LoginCallbackInfo info) =>
                {
                    // If the error message is exactly ConnectExternalAtokenValidationFailed,
                    // that may be a sign this is an expired token
                    // If this isn't a retry, retry and reacquire a new token
                    if (retryAttemptNumber < MaximumNumberOfRetries && info.ResultCode == Result.ConnectExternalTokenValidationFailed)
                    {
                        // Remove the cached token so that it re-acquired on retry
                        cachedTokens.Remove(username);
                        StartConnectLoginWithOpenID(username, password, onLoginCallback, retryAttemptNumber: retryAttemptNumber++);
                    }
                    else
                    {
                        onLoginCallback?.Invoke(info);
                    }
                });
            });
        }

        public void RequestToken(string username, string password, Action<string,string> callback)
        {
            if (cachedTokens.ContainsKey(username))
            {
                callback?.Invoke(username,cachedTokens[username]);
                return;
            }

            var request = new TokenRequest() { requests = new UserRequest[] { new UserRequest() { username = username, password = password } } };
            StartCoroutine(GetTokens(request, (TokenResponse? response) => {
                if (response != null)
                {
                    foreach (var user in response.Value.tokens)
                    {
                        if (user.user == username)
                        {
                            cachedTokens[username] = user.id_token;
                            callback?.Invoke(username, user.id_token);
                            return;
                        }
                    }
                }
                callback?.Invoke(username, null);
            }));
        }

        public void RequestTokens(Dictionary<string,string> credentials, Action<Dictionary<string, string>> callback)
        {
            bool containsAll = true;
            foreach (var user in credentials.Keys)
            {
                if (!cachedTokens.ContainsKey(user))
                {
                    containsAll = false;
                }
            }

            if (containsAll)
            {
                var response = new Dictionary<string, string>();
                foreach (var user in credentials.Keys)
                {
                    response[user] = cachedTokens[user];
                }
                callback?.Invoke(response);
                return;
            }

            var request = new TokenRequest() { requests = new UserRequest[credentials.Count] };
            int reqIndex = 0;
            foreach (var cred in credentials)
            {
                request.requests[reqIndex++] = new UserRequest()
                {
                    username = cred.Key,
                    password = cred.Value
                };
            }

            StartCoroutine(GetTokens(request, (TokenResponse? response) => {
                Dictionary<string, string> cbResponse = null;
                if (response != null)
                {
                    cbResponse = new Dictionary<string, string>();
                    foreach (var user in response.Value.tokens)
                    {
                        if (credentials.ContainsKey(user.user))
                        {
                            cachedTokens[user.user] = user.id_token;
                            cbResponse[user.user] = user.id_token;
                        }
                    }
                }
                callback?.Invoke(cbResponse);
            }));
        }

        private IEnumerator GetTokens(TokenRequest request, Action<TokenResponse?> callback)
        {
            string jsonData = JsonUtility.ToJson(request);
            using (UnityWebRequest webRequest = UnityWebRequest.Put(tokensUrl, jsonData))
            {
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                yield return webRequest.SendWebRequest();
                TokenResponse? response = null;
                if (webRequest.result == UnityWebRequest.Result.Success && webRequest.responseCode == 200)
                {
                    response = JsonUtility.FromJson<TokenResponse>(webRequest.downloadHandler.text);
                }
                callback?.Invoke(response);
            }
        }
    }
}
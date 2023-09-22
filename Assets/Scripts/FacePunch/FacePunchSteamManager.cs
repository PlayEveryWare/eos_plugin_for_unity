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

/*
* This script is made using Facepunch.Steamworks Version 2.3.2
*/


#if ENABLE_FACEPUNCH_STEAM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)

using UnityEngine;
using System.IO;

namespace PlayEveryWare.EpicOnlineServices.Samples.Steam
{
    public class SteamManager : MonoBehaviour
    {
        protected static SteamManager s_instance;
        public static SteamManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    return new GameObject("FacepunchSteamManager").AddComponent<SteamManager>();
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
            // Only one instance of SteamManager at a time!
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;

            // We want our SteamManager Instance to persist across scenes.
            DontDestroyOnLoad(gameObject);

            try
            {
                uint steamAppId = GetSteamAppID();
                Steamworks.SteamClient.Init(steamAppId);
                Debug.LogWarning("[Facepunch Steamworks] Initialized");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("[Facepunch Steamworks] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

                // Something went wrong - it's one of these:
                //
                //     Steam is closed?
                //     Can't find steam_api dll?
                //     Don't have permission to play app?
                //

                return;

            }
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
            if (s_instance != this)
            {
                return;
            }

            s_instance = null;

            if (!m_bInitialized)
            {
                return;
            }

            Steamworks.SteamClient.Shutdown();
        }

        public void StartLoginWithSteam(EOSManager.OnAuthLoginCallback onLoginCallback)
        {
#if DISABLESTEAMWORKS
            onLoginCallback?.Invoke(new Epic.OnlineServices.Auth.LoginCallbackInfo() { ResultCode = Epic.OnlineServices.Result.UnexpectedError });
#else

            string steamId = Steamworks.SteamClient.SteamId.ToString();

            byte[] sessionTicket = Steamworks.SteamUser.GetAuthSessionTicket().Data;
            string sessionTicketString = System.BitConverter.ToString(sessionTicket).Replace("-", "");

            if (steamId == null)
            {
                Debug.LogError("ExternalAuth failed: Steam ID not valid");
            }
            else if (sessionTicketString == null)
            {
                Debug.LogError("ExternalAuth failed: Steam session ticket not valid");
            }
            else
            {
                EOSManager.Instance.StartLoginWithLoginTypeAndToken(
                        Epic.OnlineServices.Auth.LoginCredentialType.ExternalAuth,
                        Epic.OnlineServices.ExternalCredentialType.SteamSessionTicket,
                        steamId,
                        sessionTicketString,
                        onLoginCallback);
            }
#endif
        }
        public async void StartConnectLoginWithSteamAppTicket(EOSManager.OnConnectLoginCallback onLoginCallback)
        {
            byte[] encryptedAppTicket = await Steamworks.SteamUser.RequestEncryptedAppTicketAsync();
            if (encryptedAppTicket == null)
            {
                Debug.LogError("Connect Login failed: Unable to get Steam app ticket");
                onLoginCallback?.Invoke(new Epic.OnlineServices.Connect.LoginCallbackInfo() { ResultCode = Epic.OnlineServices.Result.UnexpectedError });
            }
            else
            {
                string appTicketString = System.BitConverter.ToString(encryptedAppTicket).Replace("-", "");
                EOSManager.Instance.StartConnectLoginWithOptions(Epic.OnlineServices.ExternalCredentialType.SteamAppTicket, appTicketString, onloginCallback: onLoginCallback);
            }
        }
        public void StartConnectLoginWithSteamSessionTicket(EOSManager.OnConnectLoginCallback onLoginCallback)
        {
            byte[] sessionTicket = Steamworks.SteamUser.GetAuthSessionTicket().Data;
            if (sessionTicket == null)
            {
                Debug.LogError("Connect Login failed: Unable to get Steam session ticket");
                onLoginCallback?.Invoke(new Epic.OnlineServices.Connect.LoginCallbackInfo() { ResultCode = Epic.OnlineServices.Result.UnexpectedError });
            }
            else
            {
                string sessionTicketString = System.BitConverter.ToString(sessionTicket).Replace("-", "");
                EOSManager.Instance.StartConnectLoginWithOptions(Epic.OnlineServices.ExternalCredentialType.SteamSessionTicket, sessionTicketString, onloginCallback: onLoginCallback);
            }
        }

        uint GetSteamAppID() 
        {
            string steamAppIdPath = Path.Combine(Directory.GetCurrentDirectory(), "steam_appid.txt");

            uint steamAppID;

            //Read the value in steam_appid.txt if it exists
            if (File.Exists(steamAppIdPath))
            {
                using (StreamReader streamReader = new StreamReader(steamAppIdPath))
                {
                    string appIdAsText = streamReader.ReadToEnd();
                    if (uint.TryParse(appIdAsText, out steamAppID)) 
                    {
                        return steamAppID;
                    }

                    //If the value is invalid, return the default value.
                    Debug.LogError("steam_appid.txt contains invalid value, make sure it only contains the games app id and nothing else");
                    return 0;
                }
            }

            //Creates a default steam_appid.txt if its missing
            StreamWriter appIdFile = File.CreateText(steamAppIdPath);
            appIdFile.Write("480");
            appIdFile.Close();
            Debug.LogError("steam_appid.txt created, open the file and rewrite the app id with your games app id");

            return 480;

        }        
    }
}
#endif
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

// Don't shut down the interface if running in the editor.
// According to the Epic documentation, shutting down this will disable a given loaded
// instance of the SDK from ever initializing again. Which is bad because Unity often (always?) loads a library just once
// up front for a given DLL.
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN
#define EOS_CAN_SHUTDOWN
#endif

#if !UNITY_EDITOR
#define USE_STATIC_EOS_VARIABLE
#endif

#if UNITY_64
#define PLATFORM_64BITS
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
#define PLATFORM_32BITS
#endif

#define ENABLE_DEBUG_EOSMANAGER

// If using a 1.12 or newer, this allows the eos manager to use the new
// bindings API to hook up and load the library
#if EOS_DYNAMIC_BINDINGS
#define USE_EOS_DYNAMIC_BINDINGS
#endif

using System.Collections.Generic;

using System.Runtime.InteropServices;
using System;
using System.IO;
using AOT;

using UnityEngine;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Connect;

using UnityEngine.Assertions;
using System.Diagnostics;

namespace PlayEveryWare.EpicOnlineServices
{
    /// <summary>
    /// One of the responsibilities of this class is to manage the lifetime of
    /// the EOS SDK and to be the interface for getting all the managed EOS interfaces.
    /// It also handles loading and unloading EOS on platforms that need that.
    /// 
    /// See : https://dev.epicgames.com/docs/services/en-US/CSharp/GettingStarted/index.html
    /// </summary>
    public partial class EOSManager : MonoBehaviour
    {
        public delegate void OnAuthLoginCallback(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo);
        public delegate void OnAuthLogoutCallback(LogoutCallbackInfo data);
        public delegate void OnConnectLoginCallback(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo);
        public delegate void OnCreateConnectUserCallback(Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo);
        public delegate void OnConnectLinkExternalAccountCallback(Epic.OnlineServices.Connect.LinkAccountCallbackInfo linkAccountCallbackInfo);
        public delegate void OnAuthLinkExternalAccountCallback(Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo);

        /// <value>Hard-coded configuration file name ("EpicOnlineServicesConfig.json")</value>
        private static string ConfigFileName = "EpicOnlineServicesConfig.json";

        /// <value>List of logged in <c>EpicAccountId</c></value>
        private static List<EpicAccountId> loggedInAccountIDs = new List<EpicAccountId>();

        //private static Dictionary<EpicAccountId, ProductUserId> accountIDToProductId = new Dictionary<EpicAccountId, ProductUserId>();

        /// <value>Stores instances of feature managers</value>
        private static Dictionary<Type, IEOSSubManager> s_subManagers = new Dictionary<Type, IEOSSubManager>();

        /// <value>List of Login callbacks</value>
        private static List<OnConnectLoginCallback> s_onConnectLoginCallbacks = new List<OnConnectLoginCallback>();

        /// <value>List of Auth Login callbacks</value>
        private static List<OnAuthLoginCallback> s_onAuthLoginCallbacks = new List<OnAuthLoginCallback>();

        /// <value>List of Auth Logout callbacks</value>
        private static List<OnLogoutCallback> s_onAuthLogoutCallbacks = new List<OnLogoutCallback>();


        //private static List

        //-------------------------------------------------------------------------
        public partial class EOSSingleton
        {
            static private EpicAccountId s_localUserId;
            static private ProductUserId s_localProductUserId = null;

            static private NotifyEventHandle s_notifyLoginStatusChangedCallbackHandle;

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <param name="localUserId"></param>
            protected void SetLocalUserId(EpicAccountId localUserId)
            {
                s_localUserId = localUserId;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public EpicAccountId GetLocalUserId()
            {
                return s_localUserId;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <param name="localProductUserId"></param>
            protected void SetLocalProductUserId(ProductUserId localProductUserId)
            {
                s_localProductUserId = localProductUserId;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            // TODO: refactor to use the s_localProductUserID
            public ProductUserId GetProductUserId()
            {
                return s_localProductUserId;
            }

            //-------------------------------------------------------------------------
            private bool HasShutdown()
            {
                return null == s_eosPlatformInterface;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool HasLoggedInWithConnect()
            {
                return s_localProductUserId != null;
            }

            //-------------------------------------------------------------------------
            [Conditional("ENABLE_DEBUG_EOSMANAGER")]
            static void print(string toPrint)
            {
                UnityEngine.Debug.Log(toPrint);
            }


            //-------------------------------------------------------------------------
            public T GetOrCreateManager<T>() where T : IEOSSubManager, new()
            {
                T manager = default;
                Type type = typeof(T);
                if (!s_subManagers.ContainsKey(type))
                {
                    manager = new T();
                    s_subManagers.Add(type, manager);

                    if (manager is IEOSOnConnectLogin)
                    {
                        s_onConnectLoginCallbacks.Add((manager as IEOSOnConnectLogin).OnConnectLogin);
                    }
                    if (manager is IEOSOnAuthLogin)
                    {
                        s_onAuthLoginCallbacks.Add((manager as IEOSOnAuthLogin).OnAuthLogin);
                    }
                    if (manager is IEOSOnAuthLogout)
                    {
                        s_onAuthLogoutCallbacks.Add((manager as IEOSOnAuthLogout).OnAuthLogout);
                    }
                }
                else
                {
                    manager = (T)s_subManagers[type];
                }
                return manager;
            }

            //-------------------------------------------------------------------------
            // NOTE: on some platforms the EOS platform is init'd by a native dynamic library. In
            // those cases, this code will early out.
            public void Init()
            {
                if (GetEOSPlatformInterface() != null)
                {
                    print("Init completed with existing EOS PlatformInterface");
                    Epic.OnlineServices.Logging.LoggingInterface.SetCallback(SimplePrintCallback);
                    Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.VeryVerbose);
                    return;
                }


                LoadEOSLibraries();
                NativeCallToUnloadEOS();

                //TODO: provide different way to load the config file?
                string eosFinalConfigPath = System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", ConfigFileName);

                if (!File.Exists(eosFinalConfigPath))
                {
                    throw new Exception("Couldn't find EOS Config file: Please ensure " + eosFinalConfigPath + " exists and is a valid config");
                }

                var configDataAsString = System.IO.File.ReadAllText(eosFinalConfigPath);
                var configData = JsonUtility.FromJson<EOSConfig>(configDataAsString);
                print("Loaded config file: " + configDataAsString);

                var initOptions = CreateSystemInitOptions();

                initOptions.ProductName = configData.productName;
                initOptions.ProductVersion = configData.productVersion;
                initOptions.AllocateMemoryFunction = IntPtr.Zero;
                initOptions.ReallocateMemoryFunction = IntPtr.Zero;
                initOptions.ReleaseMemoryFunction = IntPtr.Zero;
                initOptions.OverrideThreadAffinity = new InitializeThreadAffinity();

                ConfigureSystemInitOptions(ref initOptions);

                Epic.OnlineServices.Result initResult = Epic.OnlineServices.Platform.PlatformInterface.Initialize(initOptions);
                if (initResult != Epic.OnlineServices.Result.Success)
                {
#if UNITY_EDITOR
                    UnloadAllLibraries();
                    ForceUnloadEOSLibrary();
                    LoadEOSLibraries();
                    var secondTryResult = Epic.OnlineServices.Platform.PlatformInterface.Initialize(initOptions);
                    if (secondTryResult != Result.Success)
#endif
                    {
                        throw new System.Exception("Epic Online Services didn't init correctly: " + initResult);
                    }
                }

                Epic.OnlineServices.Logging.LoggingInterface.SetCallback(SimplePrintCallback);
                Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Verbose);

                var platformOptions = CreateSystemPlatformOption();
                platformOptions.CacheDirectory = GetTempDir();
                platformOptions.IsServer = false;
                platformOptions.Flags =
#if UNITY_EDITOR
                PlatformFlags.LoadingInEditor;
#else
                PlatformFlags.None;
#endif
                if (!string.IsNullOrEmpty(configData.encryptionKey))
                {
                    platformOptions.EncryptionKey = configData.encryptionKey;
                }

                platformOptions.OverrideCountryCode = null;
                platformOptions.OverrideLocaleCode = null;
                platformOptions.ProductId = configData.productID;
                platformOptions.SandboxId = configData.sandboxID;
                platformOptions.DeploymentId = configData.deploymentID;
                platformOptions.ClientCredentials = new Epic.OnlineServices.Platform.ClientCredentials();


                ConfigureSystemPlatformCreateOptions(ref platformOptions);

                platformOptions.ClientCredentials.ClientId = configData.clientID;
                platformOptions.ClientCredentials.ClientSecret = configData.clientSecret;

                var eosPlatformInterface = Epic.OnlineServices.Platform.PlatformInterface.Create(platformOptions);

                if (eosPlatformInterface == null)
                {
                    throw new System.Exception("failed to create an Epic Online Services PlatformInterface");
                }

                SetEOSPlatformInterface(eosPlatformInterface);

                print("EOS loaded");
            }

            //-------------------------------------------------------------------------
            private static void SimplePrintCallback(LogMessage message)
            {
                print(message.Message);
            }

            //-------------------------------------------------------------------------
            static private Epic.OnlineServices.Auth.LoginOptions MakeLoginOptions(LoginCredentialType loginType, ExternalCredentialType externalCredentialType, string id, string token)
            {
                var loginCredentials = new Epic.OnlineServices.Auth.Credentials {
                    Type = loginType,
                    ExternalType = externalCredentialType,
                    Id = id,
                    Token = token
                };

                return new Epic.OnlineServices.Auth.LoginOptions {
                    Credentials = loginCredentials,
                    ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
                };
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper method for getting an auth Token from an EpicAccountId
            /// </summary>
            /// <param name="accountId"></param>
            /// <returns></returns>
            public Token GetUserAuthTokenForAccountId(EpicAccountId accountId)
            {
                var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                var userAuthToken = new Token();
                var copyUserTokenOptions = new Epic.OnlineServices.Auth.CopyUserAuthTokenOptions();

                EOSAuthInterface.CopyUserAuthToken(copyUserTokenOptions, accountId, out userAuthToken);
                return userAuthToken;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Struct that holds arguments to be used for <b>Epic Games Launcher</b>
            /// </summary>
            public struct EpicLauncherArgs
            {
                public string authLogin;
                public string authPassword;
                public string authType;
                public string epicApp;
                public string epicEnv;
                public string epicUsername;
                public string epicUserID;
                public string epicLocale;
            }

            /// <summary>
            /// Provide a way for a user of the EOSManager to get the parameters from
            /// the epic launcher, so they may be used to login.
            /// See https://dev.epicgames.com/docs/services/en-US/Interfaces/Auth/index.html#epicgameslauncher
            /// </summary>
            /// <returns><c>EpicLauncherArgs</c> struct</returns>
            public EpicLauncherArgs GetCommandLineArgsFromEpicLauncher()
            {
                var epicLauncherArgs = new EpicLauncherArgs();

                void ConfigureEpicArgument(string argument, ref string argumentString)
                {
                    int startIndex = argument.IndexOf('=') + 1;
                    if (!(startIndex < 0 || startIndex > argument.Length))
                    {
                        argumentString = argument.Substring(startIndex);
                    }
                }
                foreach (string argument in System.Environment.GetCommandLineArgs())
                {
                    if (argument.StartsWith("-AUTH_LOGIN="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.authLogin);
                    }
                    else if (argument.StartsWith("-AUTH_PASSWORD="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.authPassword);
                    }
                    else if (argument.StartsWith("-AUTH_TYPE="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.authType);
                    }
                    else if (argument.StartsWith("-epicapp="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicApp);
                    }
                    else if (argument.StartsWith("-epicenv="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicEnv);
                    }
                    else if (argument.StartsWith("-epicusername="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicUsername);
                    }
                    else if (argument.StartsWith("-epicuserid="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicUserID);
                    }
                    else if (argument.StartsWith("-epiclocale="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicLocale);
                    }
                }
                return epicLauncherArgs;
            }


            //-------------------------------------------------------------------------
            public void CreateConnectUserWithContinuanceToken(Epic.OnlineServices.ContinuanceToken token, OnCreateConnectUserCallback onCreateUserCallback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                var options = new Epic.OnlineServices.Connect.CreateUserOptions();

                options.ContinuanceToken = token;
                connectInterface.CreateUser(options, null, (Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo) =>
                {
                    if (createUserCallbackInfo.ResultCode == Result.Success)
                    {
                        SetLocalProductUserId(createUserCallbackInfo.LocalUserId);
                    }

                    if (onCreateUserCallback != null)
                    {
                        onCreateUserCallback(createUserCallbackInfo);
                    }
                });
            }

            //-------------------------------------------------------------------------
            // May only be called after auth login was called once
            public void AuthLinkExternalAccountWithContinuanceToken(Epic.OnlineServices.ContinuanceToken token, Epic.OnlineServices.Auth.LinkAccountFlags linkAccountFlags, OnAuthLinkExternalAccountCallback callback)
            {
                var authInterface = GetEOSPlatformInterface().GetAuthInterface();
                var linkOptions = new Epic.OnlineServices.Auth.LinkAccountOptions()
                {
                    ContinuanceToken = token,
                    LinkAccountFlags = linkAccountFlags,
                    LocalUserId = null
                };

                if (linkAccountFlags.HasFlag(LinkAccountFlags.NintendoNsaId))
                {
                    linkOptions.LocalUserId = Instance.GetLocalUserId();
                }

                authInterface.LinkAccount(linkOptions, null, (Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
                {
                    if (Instance.GetLocalUserId() == null)
                    {
                        Instance.SetLocalUserId(linkAccountCallbackInfo.LocalUserId);
                    }

                    if (callback != null)
                    {
                        callback(linkAccountCallbackInfo);
                    }
                });

            }

            //-------------------------------------------------------------------------
            // Can only be called if Connect.Login was called in before
            public void ConnectLinkExternalAccountWithContinuanceToken(Epic.OnlineServices.ContinuanceToken token, OnConnectLinkExternalAccountCallback callback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                var linkAccountOptions = new Epic.OnlineServices.Connect.LinkAccountOptions();
                linkAccountOptions.ContinuanceToken = token;
                linkAccountOptions.LocalUserId = Instance.GetProductUserId();

                connectInterface.LinkAccount(linkAccountOptions, null, (Epic.OnlineServices.Connect.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
                {
                    if (callback != null)
                    {
                        callback(linkAccountCallbackInfo);
                    }
                });
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="epicAccountId"></param>
            /// <param name="onConnectLoginCallback"></param>
            public void StartConnectLoginWithEpicAccount(Epic.OnlineServices.EpicAccountId epicAccountId, OnConnectLoginCallback onConnectLoginCallback)
            { 
                Token authToken = EOSManager.Instance.GetUserAuthTokenForAccountId(epicAccountId);
                var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions();
                connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = authToken.AccessToken,
                    Type = ExternalCredentialType.Epic
                };

                StartConnectLoginWithOptions(connectLoginOptions, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginData) => {
                    if (onConnectLoginCallback != null)
                    {
                        onConnectLoginCallback(connectLoginData);
                    }
                });
        }
            public void StartConnectLoginWithOptions(Epic.OnlineServices.ExternalCredentialType externalCredentialType, string displayname, string token, OnConnectLoginCallback onloginCallback)
            {
                var loginOptions = new Epic.OnlineServices.Connect.LoginOptions();
                loginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = token,
                    Type = externalCredentialType
                };

                switch(externalCredentialType)
                {
                    case ExternalCredentialType.XblXstsToken:
                        loginOptions.UserLoginInfo = null;
                        break;
                    case ExternalCredentialType.NintendoIdToken:
                    case ExternalCredentialType.NintendoNsaIdToken:
                    case ExternalCredentialType.AppleIdToken:
                        loginOptions.UserLoginInfo = new UserLoginInfo();
                        loginOptions.UserLoginInfo.DisplayName = displayname;
                        break;
                }

                StartConnectLoginWithOptions(loginOptions, onloginCallback);
            }

            //-------------------------------------------------------------------------
            // 
            public void StartConnectLoginWithOptions(Epic.OnlineServices.Connect.LoginOptions connectLoginOptions, OnConnectLoginCallback onloginCallback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                connectInterface.Login(connectLoginOptions, (object)null, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginData) =>
                {
                    if(connectLoginData.LocalUserId != null)
                    {
                        SetLocalProductUserId(connectLoginData.LocalUserId);
                        CallOnConnectLogin(connectLoginData);
                    }
                    if (onloginCallback != null)
                    {
                        onloginCallback(connectLoginData);
                    }
                });
            }

            //-------------------------------------------------------------------------
            public void StartConnectLoginWithDeviceToken(string displayName, OnConnectLoginCallback onLoginCallback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions();
                connectLoginOptions.UserLoginInfo = new Epic.OnlineServices.Connect.UserLoginInfo();
                connectLoginOptions.UserLoginInfo.DisplayName = displayName;
                connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = null,
                    Type = ExternalCredentialType.DeviceidAccessToken,
                };

                StartConnectLoginWithOptions(connectLoginOptions, onLoginCallback);
            }

            //-------------------------------------------------------------------------
            // Helper method
            public void StartPersistantLogin(OnAuthLoginCallback onLoginCallback)
            {
                StartLoginWithLoginTypeAndToken(LoginCredentialType.PersistentAuth, null, null, (callbackInfo) =>
                {
                        // Handle invalid or expired tokens for the caller
                        switch(callbackInfo.ResultCode)
                        {
                            case Result.AuthInvalidPlatformToken:
                            case Result.AuthInvalidRefreshToken:
                                var authInterface = EOSManager.Instance.GetEOSPlatformInterface().GetAuthInterface();
                                var options = new Epic.OnlineServices.Auth.DeletePersistentAuthOptions();

                                authInterface.DeletePersistentAuth(options, null, (DeletePersistentAuthCallbackInfo deletePersistentAuthCallbackInfo) =>
                                {
                                    if (onLoginCallback != null)
                                    {
                                        onLoginCallback(callbackInfo);
                                    }
                                });
                                return;
                            default:
                                break;
                        }

                        if (onLoginCallback != null)
                        {
                            onLoginCallback(callbackInfo);
                        }
                });
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Start an EOS auth login using a passed in LoginCredentialType, id, and token.
            /// </summary>
            /// <param name="loginType"></param>
            /// <param name="id"></param>
            /// <param name="token"></param>
            /// <param name="onLoginCallback"></param>
            public void StartLoginWithLoginTypeAndToken(LoginCredentialType loginType, string id, string token, OnAuthLoginCallback onLoginCallback)
            {
                StartLoginWithLoginTypeAndToken(loginType, ExternalCredentialType.Epic, id, token, onLoginCallback);
            }

            //-------------------------------------------------------------------------
            public void StartLoginWithLoginTypeAndToken(LoginCredentialType loginType, ExternalCredentialType externalCredentialType, string id, string token, OnAuthLoginCallback onLoginCallback)
            {
                var loginOptions = MakeLoginOptions(loginType, externalCredentialType, id, token);
                StartLoginWithLoginOptions(loginOptions, onLoginCallback);
            }


            // Make sure that the EOSManager knows about when someone logs in our logs out
            private void ConfigureAuthStatusCallback()
            {
                if (s_notifyLoginStatusChangedCallbackHandle == null)
                {
                    var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                    ulong callbackHandle = EOSAuthInterface.AddNotifyLoginStatusChanged(new Epic.OnlineServices.Auth.AddNotifyLoginStatusChangedOptions(), null, (Epic.OnlineServices.Auth.LoginStatusChangedCallbackInfo callbackInfo) =>
                    {
                        // if the user logged off
                        if (callbackInfo.CurrentStatus == LoginStatus.NotLoggedIn && callbackInfo.PrevStatus == LoginStatus.LoggedIn)
                        {
                            loggedInAccountIDs.Remove(callbackInfo.LocalUserId);
                        }
                    });
                    s_notifyLoginStatusChangedCallbackHandle = new NotifyEventHandle(callbackHandle, (ulong handle) =>
                    {
                        GetEOSAuthInterface().RemoveNotifyLoginStatusChanged(handle);
                    });
                }
            }

            private void CallOnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
            {
                foreach(var callback in s_onAuthLoginCallbacks)
                {
                    callback.Invoke(loginCallbackInfo);
                }
            }

            private void CallOnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginData)
            {
                foreach (var callback in s_onConnectLoginCallbacks)
                {
                    callback.Invoke(connectLoginData);
                }
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Start an EOS Auth Login with the passed in LoginOptions. Call this instead of the method on EOSAuthInterface to ensure that 
            /// the EOSManager has it's state setup correctly.
            /// </summary>
            /// <param name="loginType"></param>
            /// <param name="externalCredentialType"></param>
            /// <param name="id"></param>
            /// <param name="token"> might be a password</param>
            /// <param name="onLoginCallback"></param>
            public void StartLoginWithLoginOptions(Epic.OnlineServices.Auth.LoginOptions loginOptions, OnAuthLoginCallback onLoginCallback)
            {
                // start login things
                var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();

                Assert.IsNotNull(EOSAuthInterface, "EOSAuthInterface was null!");
                Assert.IsNotNull(loginOptions, "Error creating login options");

                // TODO: put this in a config file?
                var displayOptions = new Epic.OnlineServices.UI.SetDisplayPreferenceOptions
                {
                    NotificationLocation = Epic.OnlineServices.UI.NotificationLocation.TopRight
                };
                EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().SetDisplayPreference(displayOptions);

                print("StartLoginWithLoginTypeAndToken");

                EOSAuthInterface.Login(loginOptions, null, (Epic.OnlineServices.Auth.OnLoginCallback)((Epic.OnlineServices.Auth.LoginCallbackInfo data) => {
                    if(data.ResultCode == Result.Success)
                    {
                        loggedInAccountIDs.Add(data.LocalUserId);

                        if (GetLocalUserId() == null)
                        {
                            SetLocalUserId(data.LocalUserId);
                        }

                        ConfigureAuthStatusCallback();

                        CallOnAuthLogin(data);
                    }
                                        
                    if (onLoginCallback != null)
                    {
                        onLoginCallback(data);
                    }
                    
                }));
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Starts a logout for Auth
            /// </summary>
            /// <param name="accountId"></param>
            /// <param name="onLogoutCallback"></param>
            public void StartLogout(EpicAccountId accountId, OnLogoutCallback onLogoutCallback)
            {
                var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                LogoutOptions options = new LogoutOptions
                {
                    LocalUserId = accountId
                };

                EOSAuthInterface.Logout(options, null, (LogoutCallbackInfo data) => {
                    if (onLogoutCallback != null)
                    {
                        onLogoutCallback(data);

                        foreach(var callback in s_onAuthLogoutCallbacks)
                        {
                            callback(data);
                        }
                    }
                });
            }

            //-------------------------------------------------------------------------
            public void Tick()
            {
                if (GetEOSPlatformInterface() != null)
                {
                    GetEOSPlatformInterface().Tick();
                }
            }

            //-------------------------------------------------------------------------
            public void OnShutdown()
            {
                print("Shutting down");
                var PlatformInterface = GetEOSPlatformInterface();
                if(PlatformInterface == null)
                {
                    return;
                }

                var EOSAuthInterface = PlatformInterface.GetAuthInterface();
                // I don't need to create a new LogoutOption every time because the EOS wrapper API 
                // makes a copy each time LogOut is called.
                var logoutOptions = new LogoutOptions();

                foreach(var epicUserID in loggedInAccountIDs)
                {
                    logoutOptions.LocalUserId = epicUserID;
                    EOSAuthInterface.Logout(logoutOptions, null, (LogoutCallbackInfo data) => {
                        if (data.ResultCode != Result.Success)
                        {
                            print("failed to logout ");
                        }
                    });
                }

#if EOS_CAN_SHUTDOWN
                if (!HasShutdown())
                {
                    OnApplicationShutdown();
                }
#endif
            }

            //-------------------------------------------------------------------------
            public void OnApplicationShutdown()
            {
                if (!HasShutdown())
                {
                    print("Shutting down eos and releasing handles");
                    // Not doing this in the editor, because it doesn't seem to be an issue there
#if !UNITY_EDITOR
                    LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Off);
                    Epic.OnlineServices.Logging.LoggingInterface.SetCallback(null);
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
#endif
                    GetEOSPlatformInterface().Release();
                    Epic.OnlineServices.Platform.PlatformInterface.Shutdown();
                    SetEOSPlatformInterface(null);
#if UNITY_EDITOR
                    UnloadAllLibraries();
#endif
                }
            }
        }

        /// <value>Private static instance of <c>EOSSingleton</c></value>
        static EOSSingleton s_instance;

        /// <value>Public static instance of <c>EOSSingleton</c></value>
        //-------------------------------------------------------------------------
        static public EOSSingleton Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new EOSSingleton();
                }
                return s_instance;
            }
        }


        //-------------------------------------------------------------------------
        /// <summary>Unity [Awake](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html) is called when script instance is being loaded.
        /// <list type="bullet">
        ///     <item><description>Calls <c>Init()</c></description></item>
        /// </list>
        /// </summary>
        void Awake()
        {
            EOSManager.Instance.Init();
        }

        //-------------------------------------------------------------------------
        /// <summary>Unity [Update](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html) is called every frame if enabled.
        /// <list type="bullet">
        ///     <item><description>Calls <c>Tick()</c></description></item>
        /// </list>
        /// </summary>
        void Update()
        {
            EOSManager.Instance.Tick();
        }

        //-------------------------------------------------------------------------
        /// <summary>Unity [OnApplicationQuit](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html) is called before the application quits.
        /// <list type="bullet">
        ///     <item><description>Calls <c>OnShutdown()</c></description></item>
        /// </list>
        /// </summary>
        private void OnApplicationQuit()
        {
            EOSManager.Instance.OnShutdown();
        }

    }
}

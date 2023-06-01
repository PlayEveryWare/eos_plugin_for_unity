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


//#define ENABLE_DEBUG_EOSMANAGER

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

#if !EOS_DISABLE
using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.UI;
#endif

using UnityEngine;
using UnityEngine.Assertions;
using System.Diagnostics;
using System.Collections;

namespace PlayEveryWare.EpicOnlineServices
{
    /// <summary>
    /// One of the responsibilities of this class is to manage the lifetime of
    /// the EOS SDK and to be the interface for getting all the managed EOS interfaces.
    /// It also handles loading and unloading EOS on platforms that need that.
    /// 
    /// See : https://dev.epicgames.com/docs/services/en-US/CSharp/GettingStarted/index.html
    /// </summary>
    public partial class EOSManager : MonoBehaviour, IEOSCoroutineOwner
    {
        // <value>If true, EOSManager initialized itself at startup.</value>
        public bool InitializeOnAwake = true;

#if !EOS_DISABLE
        public delegate void OnAuthLoginCallback(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo);
        public delegate void OnAuthLogoutCallback(LogoutCallbackInfo data);
        public delegate void OnConnectLoginCallback(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo);
        public delegate void OnCreateConnectUserCallback(Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo);
        public delegate void OnConnectLinkExternalAccountCallback(Epic.OnlineServices.Connect.LinkAccountCallbackInfo linkAccountCallbackInfo);
        public delegate void OnAuthLinkExternalAccountCallback(Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo);

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
        private static List<OnAuthLogoutCallback> s_onAuthLogoutCallbacks = new List<OnAuthLogoutCallback>();

        /// <value>True if EOS Overlay is visible and has exclusive input.</value>
        private static bool s_isOverlayVisible = false;
        private static bool s_DoesOverlayHaveExcusiveInput = false;

        //cached log levels for retrieving later
        private static Dictionary<LogCategory, LogLevel> logLevels = null;

        enum EOSState
        {
            NotStarted,
            Starting,
            Running,
            ShuttingDown,
            Shutdown
        };

        static private EOSState s_state = EOSState.NotStarted;

        // Application is paused? (ie. suspended)
        static private bool s_isPaused = false;
        static public bool ApplicationIsPaused { get => s_isPaused; }

        // Application is in focus? (ie. is the foreground application)
        static private bool s_hasFocus = true;
        static public bool ApplicationHasFocus { get => s_hasFocus; }

        // When not in focus, is the application running in a constrained capacity? (ie. reduced CPU/GPU resources)
        static private bool s_isConstrained = true;
        static public bool ApplicationIsConstrained { get => s_isConstrained; }

        //private static List

        //-------------------------------------------------------------------------
        public partial class EOSSingleton
        {
            static private EpicAccountId s_localUserId;
            static private ProductUserId s_localProductUserId = null;

            static private NotifyEventHandle s_notifyLoginStatusChangedCallbackHandle;
            static private NotifyEventHandle s_notifyConnectLoginStatusChangedCallbackHandle;
            static private NotifyEventHandle s_notifyConnectAuthExpirationCallbackHandle;
            static private EOSConfig loadedEOSConfig;

            // Setting it twice will cause an exception
            static bool hasSetLoggingCallback = false;

            // Need to keep track for shutting down EOS after a successful platform initialization
            static private bool s_hasInitializedPlatform = false;

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
            // Debug method for getting a valid string to use for logging
            private string PUIDToString(ProductUserId puid)
            {
                string toReturn = null;
                if (puid != null)
                {
                    toReturn = puid.ToString();
                }

                if (toReturn == null)
                {
                    toReturn = "null";
                }
                return toReturn;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <param name="localProductUserId"></param>
            protected void SetLocalProductUserId(ProductUserId localProductUserId)
            {
                print("Changing PUID: " + PUIDToString(s_localProductUserId) + " => " + PUIDToString(localProductUserId));
                s_localProductUserId = localProductUserId;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public ProductUserId GetProductUserId()
            {
                return s_localProductUserId;
            }

            private EOSConfig GetLoadedEOSConfig()
            {
                return loadedEOSConfig;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the ProductID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetProductId()
            {
                return GetLoadedEOSConfig().productID;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the SandboxID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetSandboxId()
            {
                return GetLoadedEOSConfig().sandboxID;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the DeploymentID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetDeploymentID()
            {
                return GetLoadedEOSConfig().deploymentID;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Check if encryption key is EOS config is a valid 32-byte hex string.
            /// </summary>
            /// <returns></returns>
            public bool IsEncryptionKeyValid()
            {
                return GetLoadedEOSConfig().IsEncryptionKeyValid();
            }

            //-------------------------------------------------------------------------
            private bool HasShutdown()
            {
                return s_state == EOSState.Shutdown;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool HasLoggedInWithConnect()
            {
                return s_localProductUserId != null;
            }

            //-------------------------------------------------------------------------
            public bool ShouldOverlayReceiveInput()
            {
                return (EOSManager.s_isOverlayVisible && EOSManager.s_DoesOverlayHaveExcusiveInput)
                    || GetLoadedEOSConfig().alwaysSendInputToOverlay
                ;
                
            }

            public bool IsOverlayOpenWithExclusiveInput()
            {
                return EOSManager.s_isOverlayVisible && EOSManager.s_DoesOverlayHaveExcusiveInput;
            }

            //-------------------------------------------------------------------------
            [Conditional("ENABLE_DEBUG_EOSMANAGER")]
            static void print(string toPrint)
            {
                UnityEngine.Debug.Log(toPrint);
            }

            //-------------------------------------------------------------------------
            public void AddConnectLoginListener(IEOSOnConnectLogin connectLogin)
            {
                s_onConnectLoginCallbacks.Add(connectLogin.OnConnectLogin);
            }

            public void AddAuthLoginListener(IEOSOnAuthLogin authLogin)
            {
                s_onAuthLoginCallbacks.Add(authLogin.OnAuthLogin);
            }

            public void AddAuthLogoutListener(IEOSOnAuthLogout authLogout)
            {
                s_onAuthLogoutCallbacks.Add(authLogout.OnAuthLogout);
            }

            public void RemoveConnectLoginListener(IEOSOnConnectLogin connectLogin)
            {
                s_onConnectLoginCallbacks.Remove(connectLogin.OnConnectLogin);
            }

            public void RemoveAuthLoginListener(IEOSOnAuthLogin authLogin)
            {
                s_onAuthLoginCallbacks.Remove(authLogin.OnAuthLogin);
            }

            public void RemoveAuthLogoutListener(IEOSOnAuthLogout authLogout)
            {
                s_onAuthLogoutCallbacks.Remove(authLogout.OnAuthLogout);
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
                        AddConnectLoginListener(manager as IEOSOnConnectLogin);
                    }
                    if (manager is IEOSOnAuthLogin)
                    {
                        AddAuthLoginListener(manager as IEOSOnAuthLogin);
                    }
                    if (manager is IEOSOnAuthLogout)
                    {
                        AddAuthLogoutListener(manager as IEOSOnAuthLogout);
                    }
                }
                else
                {
                    manager = (T)s_subManagers[type];
                }
                return manager;
            }

            public void RemoveManager<T>() where T : IEOSSubManager
            {
                Type type = typeof(T);
                if (s_subManagers.ContainsKey(type))
                {
                    T manager = (T)s_subManagers[type];
                    if (manager is IEOSOnConnectLogin)
                    {
                        RemoveConnectLoginListener(manager as IEOSOnConnectLogin);
                    }
                    if (manager is IEOSOnAuthLogin)
                    {
                        RemoveAuthLoginListener(manager as IEOSOnAuthLogin);
                    }
                    if (manager is IEOSOnAuthLogout)
                    {
                        RemoveAuthLogoutListener(manager as IEOSOnAuthLogout);
                    }

                    s_subManagers.Remove(type);
                }
            }

            //-------------------------------------------------------------------------
            private Epic.OnlineServices.Result InitializePlatformInterface(EOSConfig configData)
            {
                IEOSManagerPlatformSpecifics platformSpecifics = EOSManagerPlatformSpecifics.Instance;
                print("InitializePlatformInterface: platformSpecifics.GetType() = " + platformSpecifics.GetType().ToString());

                IEOSInitializeOptions initOptions = platformSpecifics.CreateSystemInitOptions();

                print("InitializePlatformInterface: initOptions.GetType() = " + initOptions.GetType().ToString());

                initOptions.ProductName = configData.productName;
                initOptions.ProductVersion = configData.productVersion;
                initOptions.OverrideThreadAffinity = new InitializeThreadAffinity();

                initOptions.AllocateMemoryFunction = IntPtr.Zero;
                initOptions.ReallocateMemoryFunction = IntPtr.Zero;
                initOptions.ReleaseMemoryFunction = IntPtr.Zero;

                var overrideThreadAffinity = new InitializeThreadAffinity();

                overrideThreadAffinity.NetworkWork = configData.GetThreadAffinityNetworkWork(overrideThreadAffinity.NetworkWork);
                overrideThreadAffinity.StorageIo = configData.GetThreadAffinityStorageIO(overrideThreadAffinity.StorageIo);
                overrideThreadAffinity.WebSocketIo = configData.GetThreadAffinityWebSocketIO(overrideThreadAffinity.WebSocketIo);
                overrideThreadAffinity.P2PIo = configData.GetThreadAffinityP2PIO(overrideThreadAffinity.P2PIo);
                overrideThreadAffinity.HttpRequestIo = configData.GetThreadAffinityHTTPRequestIO(overrideThreadAffinity.HttpRequestIo);
                overrideThreadAffinity.RTCIo = configData.GetThreadAffinityRTCIO(overrideThreadAffinity.RTCIo);

                initOptions.OverrideThreadAffinity = overrideThreadAffinity;

                platformSpecifics.ConfigureSystemInitOptions(ref initOptions, configData);

#if UNITY_PS4 && !UNITY_EDITOR
                // On PS4, RegisterForPlatformNotifications is called at a later time by EOSPSNManager
#else
                RegisterForPlatformNotifications();
#endif

                return platformSpecifics.InitializePlatformInterface(initOptions);
            }

            //-------------------------------------------------------------------------
            private PlatformInterface CreatePlatformInterface(EOSConfig configData)
            {
                IEOSManagerPlatformSpecifics platformSpecifics = EOSManagerPlatformSpecifics.Instance;

                var platformOptions = platformSpecifics.CreateSystemPlatformOption();
                platformOptions.CacheDirectory = platformSpecifics.GetTempDir();
                platformOptions.IsServer = false;
                platformOptions.Flags =
#if UNITY_EDITOR
                PlatformFlags.LoadingInEditor;
#else
                configData.platformOptionsFlagsAsPlatformFlags();
#endif
                if (configData.IsEncryptionKeyValid())
                {
                    platformOptions.EncryptionKey = configData.encryptionKey;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("EOS config data does not contain a valid encryption key which is needed for Player Data Storage and Title Storage.");
                }

                platformOptions.OverrideCountryCode = null;
                platformOptions.OverrideLocaleCode = null;
                platformOptions.ProductId = configData.productID;
                platformOptions.SandboxId = configData.sandboxID;
                platformOptions.DeploymentId = configData.deploymentID;

                platformOptions.TickBudgetInMilliseconds = configData.tickBudgetInMilliseconds;

                var clientCredentials = new Epic.OnlineServices.Platform.ClientCredentials
                {
                    ClientId = configData.clientID,
                    ClientSecret = configData.clientSecret
                };
                platformOptions.ClientCredentials = clientCredentials;


#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || (UNITY_STANDALONE_LINUX && EOS_PREVIEW_PLATFORM) || (UNITY_EDITOR_LINUX && EOS_PREVIEW_PLATFORM))
                var createIntegratedPlatformOptionsContainerOptions = new Epic.OnlineServices.IntegratedPlatform.CreateIntegratedPlatformOptionsContainerOptions();
                //TODO: handle errors
                var integratedPlatformOptionsContainer = new Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainer();
                Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformInterface.CreateIntegratedPlatformOptionsContainer(ref createIntegratedPlatformOptionsContainerOptions, out integratedPlatformOptionsContainer);
                platformOptions.IntegratedPlatformOptionsContainerHandle = integratedPlatformOptionsContainer;
#endif
                platformSpecifics.ConfigureSystemPlatformCreateOptions(ref platformOptions);

                return platformSpecifics.CreatePlatformInterface(platformOptions);
            }

            private void InitializeOverlay(IEOSCoroutineOwner coroutineOwner)
            {
                EOSManagerPlatformSpecifics.Instance.InitializeOverlay(coroutineOwner);

                AddNotifyDisplaySettingsUpdatedOptions addNotificationData = new AddNotifyDisplaySettingsUpdatedOptions()
                {
                };

               GetEOSUIInterface().AddNotifyDisplaySettingsUpdated(ref addNotificationData, null, (ref OnDisplaySettingsUpdatedCallbackInfo data) => {
                   EOSManager.s_isOverlayVisible = data.IsVisible;
                   EOSManager.s_DoesOverlayHaveExcusiveInput = data.IsExclusiveInput;
                });
            }

            //-------------------------------------------------------------------------
            // NOTE: on some platforms the EOS platform is init'd by a native dynamic library. In
            // those cases, this code will early out.
            public void Init(IEOSCoroutineOwner coroutineOwner)
            {
#if !UNITY_EDITOR && !(UNITY_STANDALONE_WIN) && !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_WSA
#warning Platform not supported
                UnityEngine.Debug.LogError("Platform not supported");    
#endif

                Init(coroutineOwner, EOSPackageInfo.ConfigFileName);
            }

            //-------------------------------------------------------------------------
            private EOSConfig LoadEOSConfigFileFromPath(string eosFinalConfigPath)
            {
                string configDataAsString = "";
#if UNITY_ANDROID && !UNITY_EDITOR

                configDataAsString = AndroidFileIOHelper.ReadAllText(eosFinalConfigPath);
#else
                if (!File.Exists(eosFinalConfigPath))
                {
                    throw new Exception("Couldn't find EOS Config file: Please ensure " + eosFinalConfigPath + " exists and is a valid config");
                }
                configDataAsString = System.IO.File.ReadAllText(eosFinalConfigPath);
#endif
                var configData = JsonUtility.FromJson<EOSConfig>(configDataAsString);

                print("Loaded config file: " + configDataAsString);
                return configData;
            }

            public void Init(IEOSCoroutineOwner coroutineOwner, string configFileName)
            {
                string eosFinalConfigPath = System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", configFileName);
                if (loadedEOSConfig == null)
                {
                    loadedEOSConfig = LoadEOSConfigFileFromPath(eosFinalConfigPath);
                }

                if (GetEOSPlatformInterface() != null)
                {
                    print("Init completed with existing EOS PlatformInterface");

                    if (!hasSetLoggingCallback)
                    {
                        Epic.OnlineServices.Logging.LoggingInterface.SetCallback(SimplePrintCallback);
                        hasSetLoggingCallback = true;
                    }
#if UNITY_EDITOR
                    SetLogLevel(LogCategory.AllCategories, LogLevel.VeryVerbose);
#else
                    SetLogLevel(LogCategory.AllCategories, LogLevel.Warning);
#endif

                    InitializeOverlay(coroutineOwner);
                    return;
                }

                s_state = EOSState.Starting;

                LoadEOSLibraries();
                NativeCallToUnloadEOS();

                var epicArgs = GetCommandLineArgsFromEpicLauncher();

                if (!string.IsNullOrWhiteSpace(epicArgs.epicSandboxID))
                {
                    UnityEngine.Debug.Log("Sandbox ID override specified: " + epicArgs.epicSandboxID);
                    loadedEOSConfig.sandboxID = epicArgs.epicSandboxID;
                }

                if (loadedEOSConfig.sandboxDeploymentOverrides != null)
                {
                    //check if a deployment id override exists for sandbox id
                    foreach (var deploymentOverride in loadedEOSConfig.sandboxDeploymentOverrides)
                    {
                        if (loadedEOSConfig.sandboxID == deploymentOverride.sandboxID)
                        {
                            UnityEngine.Debug.Log("Sandbox Deployment ID override specified: " + deploymentOverride.deploymentID);
                            loadedEOSConfig.deploymentID = deploymentOverride.deploymentID;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(epicArgs.epicDeploymentID))
                {
                    UnityEngine.Debug.Log("Deployment ID override specified: " + epicArgs.epicDeploymentID);
                    loadedEOSConfig.deploymentID = epicArgs.epicDeploymentID;
                }

                Epic.OnlineServices.Result initResult = InitializePlatformInterface(loadedEOSConfig);
                UnityEngine.Debug.LogWarning($"EOSManager::Init: InitializePlatformInterface: initResult = {initResult}");
                
                if (initResult != Epic.OnlineServices.Result.Success)
                {
#if UNITY_EDITOR
                    ShutdownPlatformInterface();
                    UnloadAllLibraries();
                    ForceUnloadEOSLibrary();
                    LoadEOSLibraries();
                    var secondTryResult = InitializePlatformInterface(loadedEOSConfig);
                    UnityEngine.Debug.LogWarning($"EOSManager::Init: InitializePlatformInterface: initResult = {secondTryResult}");
                    if (secondTryResult != Result.Success)
#endif
#if (UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX) && EOS_PREVIEW_PLATFORM
                    if (secondTryResult != Result.AlreadyConfigured)
#endif
                    {
                        throw new System.Exception("Epic Online Services didn't init correctly: " + initResult);
                    }
                }

                s_hasInitializedPlatform = true;

                Epic.OnlineServices.Logging.LoggingInterface.SetCallback(SimplePrintCallback);
                SetLogLevel(LogCategory.AllCategories, LogLevel.Warning);


                var eosPlatformInterface = CreatePlatformInterface(loadedEOSConfig);

                if (eosPlatformInterface == null)
                {
                    throw new System.Exception("failed to create an Epic Online Services PlatformInterface");
                }

                SetEOSPlatformInterface(eosPlatformInterface);
                UpdateEOSApplicationStatus();


                InitializeOverlay(coroutineOwner);

                print("EOS loaded");
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Does what is needed to configure the the EOS SDK to register for platform notifications.
            /// Some platforms might require this to be called after a platform and title specific SDK call.
            /// </summary>
            public void RegisterForPlatformNotifications()
            {
                IEOSManagerPlatformSpecifics platformSpecifics = EOSManagerPlatformSpecifics.Instance;
                if (platformSpecifics != null)
                {
                    UnityEngine.Debug.Log("EOSManager: Registering for platform-specific notifications");
                    platformSpecifics.RegisterForPlatformNotifications();
                }
            }

            //-------------------------------------------------------------------------
            [MonoPInvokeCallback(typeof(string))]
            private static void SimplePrintStringCallback(string str)
            {
                UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", str);
            }

            //-------------------------------------------------------------------------
            [MonoPInvokeCallback(typeof(LogMessageFunc))]
            private static void SimplePrintCallback(ref LogMessage message)
            {
                var dateTime = DateTime.Now;
                var messageCategory = message.Category.Length == 0 ? new Utf8String() : message.Category;

                LogType type;
                if (message.Level < LogLevel.Warning)
                {
                    type = LogType.Error;
                }
                else if (message.Level > LogLevel.Warning)
                {
                    type = LogType.Log;
                }
                else
                {
                    type = LogType.Warning;
                }

                UnityEngine.Debug.LogFormat(type, LogOption.NoStacktrace, null, "{0:O} {1}({2}): {3}", dateTime, messageCategory, message.Level, message.Message);
            }

            /// <summary>
            /// Wrapper function for [EOS_Logging_SetLogLevel](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/NoInterface/EOS_Logging_SetLogLevel/index.html)
            /// that stores log level for later access
            /// </summary>
            /// <param name="Category">Log category to modify</param>
            /// <param name="Level">New log level to set</param>
            public void SetLogLevel(LogCategory Category, LogLevel Level)
            {
                LoggingInterface.SetLogLevel(Category, Level);
                if (logLevels == null)
                {
                    //don't construct logLevels until it's needed
                    logLevels = new Dictionary<LogCategory, LogLevel>();
                }
                if (Category == LogCategory.AllCategories)
                {
                    foreach (LogCategory cat in Enum.GetValues(typeof(LogCategory)))
                    {
                        if (cat != LogCategory.AllCategories)
                        {
                            logLevels[cat] = Level;
                        }
                    }
                }
                else
                {
                    logLevels[Category] = Level;
                }
            }

            /// <summary>
            /// Retrieves a log level previously set with <c>SetLogLevel</c>
            /// </summary>
            /// <param name="Category"><c>LogCategory</c> to retrieve <c>LogLevel</c> for</param>
            /// <returns><c>LogLevel</c> for the given <c>LogCategory</c>. Returns -1 if Category is AllCategories and not all categories are set to the same level.</returns>
            public LogLevel GetLogLevel(LogCategory Category)
            {
                if (logLevels == null)
                {
                    //logLevels will only be null if log level was never set, so it should be off
                    return LogLevel.Off;
                }
                if (Category == LogCategory.AllCategories)
                {
                    LogLevel level = GetLogLevel(LogCategory.Core);
                    foreach (LogCategory cat in Enum.GetValues(typeof(LogCategory)))
                    {
                        if (cat != LogCategory.AllCategories)
                        {
                            LogLevel catLevel = GetLogLevel(cat);
                            if (catLevel != level)
                            {
                                return (LogLevel)(-1);
                            }
                        }
                    }
                    return level;
                }
                else
                {
                    if (logLevels.ContainsKey(Category))
                    {
                        return logLevels[Category];
                    }
                    else
                    {
                        return LogLevel.Off;
                    }
                }
            }

            //-------------------------------------------------------------------------
            [MonoPInvokeCallback(typeof(LogMessageFunc))]
            private static void SimplePrintCallbackWithCallstack(LogMessage message)
            {
                var dateTime = DateTime.Now;
                var messageCategory = message.Category.Length == 0 ? new Utf8String() : message.Category;

                UnityEngine.Debug.LogFormat(null, "{0:O} {1}({2}): {3}", dateTime, messageCategory, message.Level, message.Message);
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

                var defaultScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence;

                return new Epic.OnlineServices.Auth.LoginOptions {
                    Credentials = loginCredentials,
                    ScopeFlags = loadedEOSConfig.authScopeOptionsFlags.Count > 0 ? loadedEOSConfig.authScopeOptionsFlagsAsAuthScopeFlags() : defaultScopeFlags
                };
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper method for getting an auth Token from an EpicAccountId
            /// </summary>
            /// <param name="accountId"></param>
            /// <returns></returns>
            public Token? GetUserAuthTokenForAccountId(EpicAccountId accountId)
            {
                var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                var copyUserTokenOptions = new Epic.OnlineServices.Auth.CopyUserAuthTokenOptions();

                EOSAuthInterface.CopyUserAuthToken(ref copyUserTokenOptions, accountId, out Token? userAuthToken);
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
                public string epicSandboxID;
                public string epicDeploymentID;
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
                    else if (argument.StartsWith("-epicsandboxid="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicSandboxID);
                    }
                    //support custom args for overriding sandbox or deployment
                    else if (argument.StartsWith("-eossandboxid="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicSandboxID);
                    }
                    else if (argument.StartsWith("-eosdeploymentid="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicDeploymentID);
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
                connectInterface.CreateUser(ref options, null, (ref Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo) =>
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

                authInterface.LinkAccount(ref linkOptions, null, (ref Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
                {
                    Instance.SetLocalUserId(linkAccountCallbackInfo.LocalUserId);

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

                connectInterface.LinkAccount(ref linkAccountOptions, null, (ref Epic.OnlineServices.Connect.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
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
                Token? authToken = EOSManager.Instance.GetUserAuthTokenForAccountId(epicAccountId);
                var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions();
                connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = authToken.Value.AccessToken,
                    Type = ExternalCredentialType.Epic
                };

                StartConnectLoginWithOptions(connectLoginOptions, onConnectLoginCallback);
            }

            public void StartConnectLoginWithOptions(Epic.OnlineServices.ExternalCredentialType externalCredentialType, string token, string displayname = null, OnConnectLoginCallback onloginCallback = null)
            {
                var loginOptions = new Epic.OnlineServices.Connect.LoginOptions();
                loginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = token,
                    Type = externalCredentialType
                };

                switch(externalCredentialType)
                {
                    case ExternalCredentialType.NintendoIdToken:
                    case ExternalCredentialType.NintendoNsaIdToken:
                    case ExternalCredentialType.AppleIdToken:
                    case ExternalCredentialType.OculusUseridNonce:
                    case ExternalCredentialType.GoogleIdToken:
                    case ExternalCredentialType.AmazonAccessToken:
                    case ExternalCredentialType.DeviceidAccessToken:
                        loginOptions.UserLoginInfo = new UserLoginInfo { DisplayName = displayname };
                        break;

                    default:
                        loginOptions.UserLoginInfo = null;
                        break;
                }

                StartConnectLoginWithOptions(loginOptions, onloginCallback);
            }

            //-------------------------------------------------------------------------
            // 
            public void StartConnectLoginWithOptions(Epic.OnlineServices.Connect.LoginOptions connectLoginOptions, OnConnectLoginCallback onloginCallback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                connectInterface.Login(ref connectLoginOptions, (object)null, (ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginData) =>
                {
                    if(connectLoginData.LocalUserId != null)
                    {
                        SetLocalProductUserId(connectLoginData.LocalUserId);
                        ConfigureConnectStatusCallback();
                        ConfigureConnectExpirationCallback();
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
                connectLoginOptions.UserLoginInfo = new Epic.OnlineServices.Connect.UserLoginInfo { DisplayName = displayName };

                connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = null,
                    Type = ExternalCredentialType.DeviceidAccessToken,
                };

                StartConnectLoginWithOptions(connectLoginOptions, onLoginCallback);
            }

            //-------------------------------------------------------------------------
            // Using this method is preferable as it allows the EOSManager to keep track of the product ID
            public void ConnectTransferDeviceIDAccount(TransferDeviceIdAccountOptions options, object clientData, OnTransferDeviceIdAccountCallback completionDelegate = null)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();

                connectInterface.TransferDeviceIdAccount(ref options, clientData, (ref TransferDeviceIdAccountCallbackInfo data) =>
                {
                    SetLocalProductUserId(data.LocalUserId);
                    if (completionDelegate != null)
                    {
                        completionDelegate(ref data);
                    }
                });
            }

            //-------------------------------------------------------------------------
            // Helper method
            public void StartPersistentLogin(OnAuthLoginCallback onLoginCallback)
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

                                authInterface.DeletePersistentAuth(ref options, null, (ref DeletePersistentAuthCallbackInfo deletePersistentAuthCallbackInfo) =>
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


            //-------------------------------------------------------------------------
            // Make sure that the EOSManager knows about when someone logs in our logs out
            private void ConfigureAuthStatusCallback()
            {
                if (s_notifyLoginStatusChangedCallbackHandle == null)
                {
                    var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                    var addNotifyLoginStatusChangedOptions = new Epic.OnlineServices.Auth.AddNotifyLoginStatusChangedOptions();

                    ulong callbackHandle = EOSAuthInterface.AddNotifyLoginStatusChanged(ref addNotifyLoginStatusChangedOptions, null, (ref Epic.OnlineServices.Auth.LoginStatusChangedCallbackInfo callbackInfo) =>
                    {
                        // if the user logged off
                        if (callbackInfo.CurrentStatus == LoginStatus.NotLoggedIn && callbackInfo.PrevStatus == LoginStatus.LoggedIn)
                        {
                            loggedInAccountIDs.Remove(callbackInfo.LocalUserId);
                            SetLocalUserId(null);
                        }
                    });
                    s_notifyLoginStatusChangedCallbackHandle = new NotifyEventHandle(callbackHandle, (ulong handle) =>
                    {
                        GetEOSAuthInterface()?.RemoveNotifyLoginStatusChanged(handle);
                    });
                }
            }

            //-------------------------------------------------------------------------
            private void ConfigureConnectStatusCallback()
            {
                if (s_notifyConnectLoginStatusChangedCallbackHandle == null)
                {
                    var EOSConnectInterface = GetEOSConnectInterface();
                    var addNotifyLoginStatusChangedOptions = new Epic.OnlineServices.Connect.AddNotifyLoginStatusChangedOptions();
                    ulong callbackHandle = EOSConnectInterface.AddNotifyLoginStatusChanged(ref addNotifyLoginStatusChangedOptions, null, (ref Epic.OnlineServices.Connect.LoginStatusChangedCallbackInfo callbackInfo) =>
                    {
                        if (callbackInfo.CurrentStatus == LoginStatus.NotLoggedIn && callbackInfo.PreviousStatus == LoginStatus.LoggedIn)
                        {
                            SetLocalProductUserId(null);
                        }
                        else if (callbackInfo.CurrentStatus == LoginStatus.LoggedIn && callbackInfo.PreviousStatus == LoginStatus.NotLoggedIn)
                        {
                            SetLocalProductUserId(callbackInfo.LocalUserId);
                        }
                    });

                    s_notifyConnectLoginStatusChangedCallbackHandle = new NotifyEventHandle(callbackHandle, (ulong handle) =>
                    {
                        GetEOSConnectInterface()?.RemoveNotifyLoginStatusChanged(handle);
                    });
                }
            }

            //-------------------------------------------------------------------------
            private void ConfigureConnectExpirationCallback()
            {
                if (s_notifyConnectAuthExpirationCallbackHandle == null)
                {
                    var EOSConnectInterface = GetEOSConnectInterface();
                    var addNotifyAuthExpirationOptions = new Epic.OnlineServices.Connect.AddNotifyAuthExpirationOptions();
                    ulong callbackHandle = EOSConnectInterface.AddNotifyAuthExpiration(ref addNotifyAuthExpirationOptions, null, (ref Epic.OnlineServices.Connect.AuthExpirationCallbackInfo callbackInfo) =>
                    {
                        var accountId = GetLocalUserId();
                        if (accountId != null)
                        {
                            StartConnectLoginWithEpicAccount(accountId, null);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("EOSSingleton.ConfigureConnectExpirationCallback: Cannot refresh Connect token, no valid EpicAccountId");
                        }
                    });

                    s_notifyConnectAuthExpirationCallbackHandle = new NotifyEventHandle(callbackHandle, (ulong handle) =>
                    {
                        GetEOSConnectInterface()?.RemoveNotifyAuthExpiration(handle);
                    });
                }
            }

            //-------------------------------------------------------------------------
            private void CallOnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
            {
                //create a copy of the callback list to iterate on in case the original list is modified during iteration
                var callbacks = new List<OnAuthLoginCallback>(s_onAuthLoginCallbacks);

                foreach (var callback in callbacks)
                {
                    callback?.Invoke(loginCallbackInfo);
                }
            }

            private void CallOnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginData)
            {
                var callbacks = new List<OnConnectLoginCallback>(s_onConnectLoginCallbacks);

                foreach (var callback in callbacks)
                {
                    callback?.Invoke(connectLoginData);
                }
            }

            private void CallOnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
            {
                var callbacks = new List<OnAuthLogoutCallback>(s_onAuthLogoutCallbacks);

                foreach (var callback in callbacks)
                {
                    callback?.Invoke(logoutCallbackInfo);
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

                // TODO: put this in a config file?
                var displayOptions = new Epic.OnlineServices.UI.SetDisplayPreferenceOptions
                {
                    NotificationLocation = Epic.OnlineServices.UI.NotificationLocation.TopRight
                };
                EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().SetDisplayPreference(ref displayOptions);

                print("StartLoginWithLoginTypeAndToken");

#if UNITY_IOS && !UNITY_EDITOR
                IOSLoginOptions modifiedLoginOptions = EOS_iOSLoginOptionsHelper.MakeIOSLoginOptionsFromDefualt(loginOptions);
                EOSAuthInterface.Login(ref modifiedLoginOptions, null, (Epic.OnlineServices.Auth.OnLoginCallback)((ref Epic.OnlineServices.Auth.LoginCallbackInfo data) => {
#else
                EOSAuthInterface.Login(ref loginOptions, null, (Epic.OnlineServices.Auth.OnLoginCallback)((ref Epic.OnlineServices.Auth.LoginCallbackInfo data) => {
#endif
                    print("LoginCallBackResult : " + data.ResultCode.ToString());
                    if (data.ResultCode == Result.Success)
                    {
                        loggedInAccountIDs.Add(data.LocalUserId);

                        SetLocalUserId(data.LocalUserId);

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
            /// Helper method to set presence
            /// </summary>
            /// <param name="accountId"></param>
            /// <param name="richText"></param>
            public void SetPresenceRichTextForUser(EpicAccountId accountId, string richText /*, string platformText */)
            {
                var presenceInterface = GetEOSPresenceInterface();
                var presenceHandle = new Epic.OnlineServices.Presence.PresenceModification();
                var presenceModificationOption = new Epic.OnlineServices.Presence.CreatePresenceModificationOptions();
                presenceModificationOption.LocalUserId = accountId;

                var createPresenceModificationResult = presenceInterface.CreatePresenceModification(ref presenceModificationOption, out presenceHandle);

                if (createPresenceModificationResult != Result.Success)
                {
                    UnityEngine.Debug.LogError("Unable to create presence modfication handle");
                }

                var presenceModificationSetStatUsOptions = new Epic.OnlineServices.Presence.PresenceModificationSetStatusOptions();
                presenceModificationSetStatUsOptions.Status = Epic.OnlineServices.Presence.Status.Online;
                var setStatusResult = presenceHandle.SetStatus(ref presenceModificationSetStatUsOptions);

                if (setStatusResult != Result.Success)
                {
                    UnityEngine.Debug.LogError("unable to set status");
                }

                var richTextOptions = new Epic.OnlineServices.Presence.PresenceModificationSetRawRichTextOptions();
                richTextOptions.RichText = richText;
                presenceHandle.SetRawRichText(ref richTextOptions);

                var options = new Epic.OnlineServices.Presence.SetPresenceOptions();
                options.LocalUserId = accountId;
                options.PresenceModificationHandle = presenceHandle;
                presenceInterface.SetPresence(ref options, null, (ref Epic.OnlineServices.Presence.SetPresenceCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode != Result.Success)
                    {
                        UnityEngine.Debug.LogError("Unable to set presence: " + callbackInfo.ResultCode.ToString());
                    }
                });
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

                EOSAuthInterface.Logout(ref options, null, (ref LogoutCallbackInfo data) => {
                    if (onLogoutCallback != null)
                    {
                        onLogoutCallback(ref data);

                        CallOnAuthLogout(data);
                    }
                });
            }

            //Clears a local ProductUserId since the Connect interface doesn't have a logout function
            public void ClearConnectId(ProductUserId userId)
            {
                if (GetProductUserId() == userId)
                {
                    SetLocalProductUserId(null);
                }
            }

            //-------------------------------------------------------------------------
            public void Tick()
            {
                if (GetEOSPlatformInterface() != null)
                {
                    // Poll for any application constrained state change that didn't
                    // already coincide with a prior application focus or pause event
                    UpdateApplicationConstrainedState(true);
                    UpdateNetworkStatus();

                    GetEOSPlatformInterface().Tick();
                }
            }

            //-------------------------------------------------------------------------
            public void OnShutdown()
            {
                print("Shutting down");
                var PlatformInterface = GetEOSPlatformInterface();
                if(PlatformInterface != null)
                {
                    var EOSAuthInterface = PlatformInterface.GetAuthInterface();
                    // I don't need to create a new LogoutOption every time because the EOS wrapper API 
                    // makes a copy each time LogOut is called.
                    var logoutOptions = new LogoutOptions();

                    foreach (var epicUserID in loggedInAccountIDs)
                    {
                        logoutOptions.LocalUserId = epicUserID;
                        EOSAuthInterface.Logout(ref logoutOptions, null, (ref LogoutCallbackInfo data) => {
                            if (data.ResultCode != Result.Success)
                            {
                                print("failed to logout ");
                            }
                        });
                    }
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
                    s_state = EOSState.ShuttingDown;
                    print("Shutting down eos and releasing handles");
                    // Not doing this in the editor, because it doesn't seem to be an issue there
#if !UNITY_EDITOR_OSX
#if !UNITY_EDITOR
                    //LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Off);
                    //Epic.OnlineServices.Logging.LoggingInterface.SetCallback(null);
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
#endif
                    GetEOSPlatformInterface()?.Release();
                    ShutdownPlatformInterface();
                    SetEOSPlatformInterface(null);
#endif
#if UNITY_EDITOR
                    UnloadAllLibraries();
#endif
                    s_state = EOSState.Shutdown;
                }
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Shuts down the <see cref="PlatformInterface"/> if it was initialized.
            /// </summary>
            private void ShutdownPlatformInterface()
            {
                if (s_hasInitializedPlatform)
                {
                    Epic.OnlineServices.Platform.PlatformInterface.Shutdown();
                }
                s_hasInitializedPlatform = false;
            }
            
            public ApplicationStatus GetEOSApplicationStatus()
            {
                ApplicationStatus applicationStatus = GetEOSPlatformInterface().GetApplicationStatus();
                return applicationStatus;
            }

            private void SetEOSApplicationStatus(ApplicationStatus newStatus)
            {
                ApplicationStatus currentStatus = GetEOSApplicationStatus();
                if(currentStatus != newStatus)
                {
                    print($"EOSSingleton.SetEOSApplicationStatus: {currentStatus} -> {newStatus}");

                    Result result = GetEOSPlatformInterface().SetApplicationStatus(newStatus);
                    if (result != Result.Success)
                    {
                        UnityEngine.Debug.LogError($"EOSSingleton.SetEOSApplicationStatus: Error setting EOS application status (Result = {result})");
                    }
                }
            }

            private void UpdateEOSApplicationStatus()
            {
                if (GetEOSPlatformInterface() == null)
                {
                    // EOS platform interface doesn't exist yet, nothing to update
                    return;
                }

                if (s_isPaused)
                {
                    // Application is in the background and not running (it's suspended)
                    SetEOSApplicationStatus(ApplicationStatus.BackgroundSuspended);
                }
                else // NOT Paused
                {
                    if (s_hasFocus)
                    {
                        // Application is in the foreground and running normally
                        SetEOSApplicationStatus(ApplicationStatus.Foreground);
                    }
                    else // NOT Focused
                    {
                        if (s_isConstrained)
                        {
                            // Application is in the background but running with reduced CPU/GPU resouces (it's constrained)
                            SetEOSApplicationStatus(ApplicationStatus.BackgroundConstrained);
                        }
                        else // NOT Constrained
                        {
                            // Application is in the background but running normally (should be non-interactable since it's in the background)
                            SetEOSApplicationStatus(ApplicationStatus.BackgroundUnconstrained);
                        }
                    }
                }
            }

            public void OnApplicationPause(bool isPaused)
            {
                bool wasPaused = s_isPaused;
                s_isPaused = isPaused;
                print($"EOSSingleton.OnApplicationPause: IsPaused {wasPaused} -> {s_isPaused}");

                // Poll for the latest application constrained state as we're about
                // to need it to determine the appropriate EOS application status
                UpdateApplicationConstrainedState(false);
            }

            public void OnApplicationFocus(bool hasFocus)
            {
                bool hadFocus = s_hasFocus;
                s_hasFocus = hasFocus;
                print($"EOSSingleton.OnApplicationFocus: HasFocus {hadFocus} -> {s_hasFocus}");

                // Poll for the latest application constrained state as we're about
                // to need it to determine the appropriate EOS application status
                UpdateApplicationConstrainedState(false);
            }

            public void OnApplicationConstrained(bool isConstrained, bool shouldUpdateEOSAppStatus)
            {
                bool wasConstrained = s_isConstrained;
                s_isConstrained = isConstrained;
                print($"EOSSingleton.OnApplicationConstrained: IsConstrained {wasConstrained} -> {s_isConstrained}");

                if (shouldUpdateEOSAppStatus)
                {
                    UpdateEOSApplicationStatus();
                }
            }

            // Call at least once per Update to poll whether or not the application has become constrained since
            // the last call (ie. is the application is now running in the background with reduced CPU/GPU resources?)
            // We must poll this because not all platforms generate a Unity event for constrained state changes
            // (if they even support constraining applications at all).
            private void UpdateApplicationConstrainedState(bool shouldUpdateEOSAppStatus)
            {
                if (EOSManagerPlatformSpecifics.Instance == null)
                {
                    return;
                }

                bool wasConstrained = s_isConstrained;
                bool isConstrained = EOSManagerPlatformSpecifics.Instance.IsApplicationConstrainedWhenOutOfFocus();

                // Constrained state changed?
                if (wasConstrained != isConstrained)
                {
                    OnApplicationConstrained(isConstrained, shouldUpdateEOSAppStatus);
                }
            }

            private void UpdateNetworkStatus()
            {
                var platformSpecifics = EOSManagerPlatformSpecifics.Instance;

                if (platformSpecifics != null && platformSpecifics is IEOSNetworkStatusUpdater)
                {
                    (platformSpecifics as IEOSNetworkStatusUpdater).UpdateNetworkStatus();
                }
            }
        }
#endif

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

#if !EOS_DISABLE
        //-------------------------------------------------------------------------
        /// <summary>Unity [Awake](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html) is called when script instance is being loaded.
        /// <list type="bullet">
        ///     <item><description>Calls <c>Init()</c></description></item>
        /// </list>
        /// </summary>
        void Awake()
        {
            if (InitializeOnAwake)
            {
                EOSManager.Instance.Init(this);
            }
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

        //-------------------------------------------------------------------------
        /// <summary>Unity [OnApplicationFocus](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationFocus.html) is called when the application loses or gains focus.
        /// <list type="bullet">
        ///     <item><description>Calls <c>OnApplicationFocus()</c></description></item>
        /// </list>
        /// </summary>
        void OnApplicationFocus(bool hasFocus)
        {
            EOSManager.Instance.OnApplicationFocus(hasFocus);
        }

        //-------------------------------------------------------------------------
        /// <summary>If the game is hidden (fully or partly) by another application then Unity [OnApplicationPause](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationPause.html) will return true. When the game is changed back to current it will no longer be paused and OnApplicationPause will return to false.
        /// <list type="bullet">
        ///     <item><description>Calls <c>OnApplicationPause()</c></description></item>
        /// </list>
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            EOSManager.Instance.OnApplicationPause(pauseStatus);
        }
#endif

        //-------------------------------------------------------------------------
        void IEOSCoroutineOwner.StartCoroutine(IEnumerator routine)
        {
            base.StartCoroutine(routine);
        }
    }
}

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

// This define controls if the EOS SDK should be unloaded in the editor at shutdown to work around DLL unload errors.
//#define EOS_DO_NOT_UNLOAD_SDK_ON_SHUTDOWN

// On macOS and Linux, there isn't a known reliable way to unload shared libraries, therefore this is the default behavior.
#if (UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)
#define EOS_DO_NOT_UNLOAD_SDK_ON_SHUTDOWN
#endif

#if !UNITY_EDITOR
#define USE_STATIC_EOS_VARIABLE
#endif

//#define ENABLE_DEBUG_EOSMANAGER

// If using a 1.12 or newer, this allows the eos manager to use the new
// bindings API to hook up and load the library
#if EOS_DYNAMIC_BINDINGS
#define USE_EOS_DYNAMIC_BINDINGS
#endif

namespace PlayEveryWare.EpicOnlineServices
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Collections;

#if !EOS_DISABLE
    using Epic.OnlineServices.Platform;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Logging;
    using Epic.OnlineServices.Connect;
    using Epic.OnlineServices.UI;
#endif

#if !EOS_DISABLE
    using Epic.OnlineServices.Presence;

    using System.Diagnostics;
    using System.Globalization;
    using UnityEngine.Assertions;
    using AddNotifyLoginStatusChangedOptions = Epic.OnlineServices.Auth.AddNotifyLoginStatusChangedOptions;
    using Credentials = Epic.OnlineServices.Auth.Credentials;
    using Debug = UnityEngine.Debug;
    using LinkAccountCallbackInfo = Epic.OnlineServices.Connect.LinkAccountCallbackInfo;
    using LinkAccountOptions = Epic.OnlineServices.Auth.LinkAccountOptions;
    using LoginCallbackInfo = Epic.OnlineServices.Auth.LoginCallbackInfo;
    using LoginOptions = Epic.OnlineServices.Auth.LoginOptions;
    using LoginStatusChangedCallbackInfo = Epic.OnlineServices.Auth.LoginStatusChangedCallbackInfo;

    using Utility;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;
    using LogoutCallbackInfo = Epic.OnlineServices.Auth.LogoutCallbackInfo;
    using LogoutOptions = Epic.OnlineServices.Auth.LogoutOptions;
    using OnLogoutCallback = Epic.OnlineServices.Auth.OnLogoutCallback;
#endif
    /// <summary>
    /// One of the responsibilities of this class is to manage the lifetime of
    /// the EOS SDK and to be the interface for getting all the managed EOS interfaces.
    /// It also handles loading and unloading EOS on platforms that need that.
    /// 
    /// See : https://dev.epicgames.com/docs/services/en-US/CSharp/GettingStarted/index.html
    /// </summary>
    public partial class EOSManager : MonoBehaviour, IEOSCoroutineOwner
    {
        /// <value>If true, EOSManager will shutdown the EOS SDK when Unity runs <see cref="Application.quitting"/>.</value>
        public bool ShouldShutdownOnApplicationQuit = true;

#if !EOS_DISABLE
        public delegate void OnAuthLoginCallback(LoginCallbackInfo loginCallbackInfo);

        public delegate void OnAuthLogoutCallback(LogoutCallbackInfo data);

        public delegate void OnConnectLoginCallback(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo);

        private static event OnAuthLoginCallback OnAuthLogin;
        private static event OnAuthLogoutCallback OnAuthLogout;
        private static event OnConnectLoginCallback OnConnectLogin;

        public delegate void OnCreateConnectUserCallback(CreateUserCallbackInfo createUserCallbackInfo);

        public delegate void OnConnectLinkExternalAccountCallback(LinkAccountCallbackInfo linkAccountCallbackInfo);

        public delegate void OnAuthLinkExternalAccountCallback(
            Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo);

        /// <value>List of logged in <c>EpicAccountId</c></value>
        private static List<EpicAccountId> loggedInAccountIDs = new List<EpicAccountId>();

        //private static Dictionary<EpicAccountId, ProductUserId> accountIDToProductId = new Dictionary<EpicAccountId, ProductUserId>();

        /// <value>Stores instances of feature managers</value>
        private static Dictionary<Type, IEOSSubManager> s_subManagers = new Dictionary<Type, IEOSSubManager>();

        /// <value>List of application shutdown callbacks</value>
        private static List<Action> s_onApplicationShutdownCallbacks = new List<Action>();

        /// <value>True if EOS Overlay is visible and has exclusive input.</value>
        private static bool s_isOverlayVisible;

        private static bool s_DoesOverlayHaveExcusiveInput;

        //cached log levels for retrieving later
        private static Dictionary<LogCategory, LogLevel> logLevels;

        /// <summary>
        /// A pointer to the active EOSManager instance.
        /// This is set when a EOSManager runs Awake, and this value is null.
        /// This value may be "null" if the EOSManager has its game object destroyed,
        /// for example between automated tests.
        /// </summary>
        private static EOSManager s_EOSManagerInstance = null;

        enum EOSState
        {
            NotStarted,
            Starting,
            Running,
            Suspending,
            Suspended,
            ShuttingDown,
            Shutdown
        }

        static private EOSState s_state = EOSState.NotStarted;

        // Application is paused? (ie. suspended)
        static private bool s_isPaused;
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
            static private ProductUserId s_localProductUserId;

            static private NotifyEventHandle s_notifyLoginStatusChangedCallbackHandle;
            static private NotifyEventHandle s_notifyConnectLoginStatusChangedCallbackHandle;
            static private NotifyEventHandle s_notifyConnectAuthExpirationCallbackHandle;
            
            // Setting it twice will cause an exception
            static bool hasSetLoggingCallback;

            // Need to keep track for shutting down EOS after a successful platform initialization
            static private bool s_hasInitializedPlatform;

            private static readonly bool s_eosUnloadSDKOnShutdown =
#if EOS_DO_NOT_UNLOAD_SDK_ON_SHUTDOWN
                false
#else
                true
#endif
            ;

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
                print("Changing PUID: " + PUIDToString(s_localProductUserId) + " => " +
                      PUIDToString(localProductUserId));
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

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the ProductID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetProductId()
            {
                return Config.Get<EOSConfig>().productID;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the SandboxID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetSandboxId()
            {
                return Config.Get<EOSConfig>().sandboxID;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the DeploymentID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetDeploymentID()
            {
                return Config.Get<EOSConfig>().deploymentID;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Check if encryption key is EOS config is a valid 32-byte hex string.
            /// </summary>
            /// <returns></returns>
            public bool IsEncryptionKeyValid()
            {
                return Config.Get<EOSConfig>().IsEncryptionKeyValid();
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
                return (s_isOverlayVisible && s_DoesOverlayHaveExcusiveInput)
                       || Config.Get<EOSConfig>().alwaysSendInputToOverlay
                    ;
            }

            public bool IsOverlayOpenWithExclusiveInput()
            {
                return s_isOverlayVisible && s_DoesOverlayHaveExcusiveInput;
            }

            //-------------------------------------------------------------------------
            [Conditional("ENABLE_DEBUG_EOSMANAGER")]
            internal static void print(string toPrint, LogType type = LogType.Log)
            {
                Debug.LogFormat(type, LogOption.None, null, toPrint);
            }

            //-------------------------------------------------------------------------
            public void AddConnectLoginListener(IEOSOnConnectLogin connectLogin)
            {
                OnConnectLogin += connectLogin.OnConnectLogin;
            }

            public void AddAuthLoginListener(IEOSOnAuthLogin authLogin)
            {
                OnAuthLogin += authLogin.OnAuthLogin;
            }

            public void AddAuthLogoutListener(IEOSOnAuthLogout authLogout)
            {
                OnAuthLogout += authLogout.OnAuthLogout;
            }

            public void AddApplicationCloseListener(Action listener)
            {
                s_onApplicationShutdownCallbacks.Add(listener);
            }

            public void RemoveConnectLoginListener(IEOSOnConnectLogin connectLogin)
            {
                OnConnectLogin -= connectLogin.OnConnectLogin;
            }

            public void RemoveAuthLoginListener(IEOSOnAuthLogin authLogin)
            {
                OnAuthLogin -= authLogin.OnAuthLogin;
            }

            public void RemoveAuthLogoutListener(IEOSOnAuthLogout authLogout)
            {
                OnAuthLogout -= authLogout.OnAuthLogout;
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

                    if (manager is IEOSOnConnectLogin connectLogin)
                    {
                        OnConnectLogin += connectLogin.OnConnectLogin;
                    }

                    if (manager is IEOSOnAuthLogin authLogin)
                    {
                        OnAuthLogin += authLogin.OnAuthLogin;
                    }

                    if (manager is IEOSOnAuthLogout authLogout)
                    {
                        OnAuthLogout += authLogout.OnAuthLogout;
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
            private Result InitializePlatformInterface()
            {
                EOSConfig configData = Config.Get<EOSConfig>();
                IPlatformSpecifics platformSpecifics = EOSManagerPlatformSpecificsSingleton.Instance;

                print("InitializePlatformInterface: platformSpecifics.GetType() = " + platformSpecifics.GetType());

                EOSInitializeOptions initOptions = new EOSInitializeOptions();

                print("InitializePlatformInterface: initOptions.GetType() = " + initOptions.GetType());

                initOptions.options.ProductName = configData.productName;
                initOptions.options.ProductVersion = configData.productVersion;
                initOptions.options.OverrideThreadAffinity = new InitializeThreadAffinity();

                initOptions.options.AllocateMemoryFunction = IntPtr.Zero;
                initOptions.options.ReallocateMemoryFunction = IntPtr.Zero;
                initOptions.options.ReleaseMemoryFunction = IntPtr.Zero;

                var overrideThreadAffinity = new InitializeThreadAffinity();

                configData.ConfigureOverrideThreadAffinity(ref overrideThreadAffinity);

                initOptions.options.OverrideThreadAffinity = overrideThreadAffinity;

                platformSpecifics.ConfigureSystemInitOptions(ref initOptions);

#if UNITY_PS4 && !UNITY_EDITOR
                // On PS4, RegisterForPlatformNotifications is called at a later time by EOSPSNManager
#else
                RegisterForPlatformNotifications();
#endif

                return PlatformInterface.Initialize(ref (initOptions as EOSInitializeOptions).options);
            }

            //-------------------------------------------------------------------------
            private PlatformInterface CreatePlatformInterface()
            {
                EOSConfig configData = Config.Get<EOSConfig>();
                IPlatformSpecifics platformSpecifics = EOSManagerPlatformSpecificsSingleton.Instance;

                EOSCreateOptions platformOptions = new EOSCreateOptions();

                platformOptions.options.CacheDirectory = platformSpecifics.GetTempDir();
                platformOptions.options.IsServer = configData.isServer;
                platformOptions.options.Flags =
#if UNITY_EDITOR
                    PlatformFlags.LoadingInEditor;
#else
                configData.GetPlatformFlags();
#endif
                if (configData.IsEncryptionKeyValid())
                {
                    platformOptions.options.EncryptionKey = configData.encryptionKey;
                }
                else
                {
                    print(
                        "EOS config data does not contain a valid encryption key which is needed for Player Data Storage and Title Storage.",
                        LogType.Warning);
                }

                platformOptions.options.OverrideCountryCode = null;
                platformOptions.options.OverrideLocaleCode = null;
                platformOptions.options.ProductId = configData.productID;
                platformOptions.options.SandboxId = configData.sandboxID;
                platformOptions.options.DeploymentId = configData.deploymentID;

                platformOptions.options.TickBudgetInMilliseconds = configData.tickBudgetInMilliseconds;

                // configData has to serialize to JSON, so it doesn't represent null
                // If the value is <= 0, then set it to null, which the EOS SDK will handle by using default of 30 seconds.
                platformOptions.options.TaskNetworkTimeoutSeconds = configData.taskNetworkTimeoutSeconds > 0 ? configData.taskNetworkTimeoutSeconds : null;

                var clientCredentials = new ClientCredentials
                {
                    ClientId = configData.clientID,
                    ClientSecret = configData.clientSecret
                };
                platformOptions.options.ClientCredentials = clientCredentials;


#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
                var createIntegratedPlatformOptionsContainerOptions = new Epic.OnlineServices.IntegratedPlatform.CreateIntegratedPlatformOptionsContainerOptions();
                var integratedPlatformOptionsContainer = new Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainer();
                var integratedPlatformOptionsContainerResult = Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformInterface.CreateIntegratedPlatformOptionsContainer(ref createIntegratedPlatformOptionsContainerOptions, out integratedPlatformOptionsContainer);
                
                if (integratedPlatformOptionsContainerResult != Result.Success)
                {
                    print($"Error creating integrated platform container: {integratedPlatformOptionsContainerResult}");
                }
                platformOptions.options.IntegratedPlatformOptionsContainerHandle = integratedPlatformOptionsContainer;
#endif
                platformSpecifics.ConfigureSystemPlatformCreateOptions(ref platformOptions);

                PlatformInterface platformInterface = PlatformInterface.Create(ref (platformOptions as EOSCreateOptions).options);

#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
                integratedPlatformOptionsContainer.Release();
#endif
                return platformInterface;

            }

            //-------------------------------------------------------------------------
            private void InitializeOverlay(IEOSCoroutineOwner coroutineOwner)
            {
                EOSConfig configData = Config.Get<EOSConfig>();

                // Sets the button for the bringing up the overlay
                var friendToggle = new SetToggleFriendsButtonOptions
                {
                    ButtonCombination = configData.GetToggleFriendsButtonCombinationFlags()
                };
                UIInterface uiInterface = Instance.GetEOSPlatformInterface().GetUIInterface();
                uiInterface.SetToggleFriendsButton(ref friendToggle);

                EOSManagerPlatformSpecificsSingleton.Instance.InitializeOverlay(coroutineOwner);

                AddNotifyDisplaySettingsUpdatedOptions addNotificationData =
                    new AddNotifyDisplaySettingsUpdatedOptions();

                GetEOSUIInterface().AddNotifyDisplaySettingsUpdated(ref addNotificationData, null,
                    (ref OnDisplaySettingsUpdatedCallbackInfo data) =>
                    {
                        s_isOverlayVisible = data.IsVisible;
                        s_DoesOverlayHaveExcusiveInput = data.IsExclusiveInput;
                    });
            }

            //-------------------------------------------------------------------------
            // NOTE: on some platforms the EOS platform is init'd by a native dynamic library. In
            // those cases, this code will early out.
            public void Init(IEOSCoroutineOwner coroutineOwner)
            {
                Init(coroutineOwner, EOSPackageInfo.ConfigFileName);
            }

            //-------------------------------------------------------------------------
            public void Init(IEOSCoroutineOwner coroutineOwner, string configFileName)
            {
                if (GetEOSPlatformInterface() != null)
                {
                    print("Init completed with existing EOS PlatformInterface");

                    if (!hasSetLoggingCallback)
                    {
                        LoggingInterface.SetCallback(SimplePrintCallback);
                        hasSetLoggingCallback = true;
                    }

                    // The log levels are set in the native plugin
                    // This is here to sync the settings visually in UILogWindow
                    InitializeLogLevels();

                    InitializeOverlay(coroutineOwner);
                    return;
                }

                s_state = EOSState.Starting;

                LoadEOSLibraries();

                // Set log level prior to platform interface initialization
                // VeryVerbose for dynamic linking platforms, otherwise set levels from configs 
#if UNITY_EDITOR
                SetLogLevel(LogCategory.AllCategories, LogLevel.VeryVerbose);
#else
                InitializeLogLevels();
#endif

                var epicArgs = GetCommandLineArgsFromEpicLauncher();

                if (!string.IsNullOrWhiteSpace(epicArgs.epicSandboxID))
                {
                    Config.Get<EOSConfig>().SetDeployment(epicArgs.epicSandboxID);
                }

                Result initResult = InitializePlatformInterface();


                if (initResult != Result.Success)
                {
                    if (s_eosUnloadSDKOnShutdown)
                    {
#if UNITY_EDITOR
                        ShutdownPlatformInterface();
                        UnloadAllLibraries();
                        ForceUnloadEOSLibrary();
                        LoadEOSLibraries();
#endif
                    }
                    else if (initResult == Result.AlreadyConfigured)
                    {

#if UNITY_EDITOR
                        // in the case where the error is AlreadyConfigured and EOSManager is configured to not
                        // shutdown, we can pretend the initResult was a 'real' Success so that we can continue to boot
                        initResult = Result.Success;
#endif
                    }

                    if (initResult != Result.Success)
                    {
#if UNITY_EDITOR
                        initResult = InitializePlatformInterface();
#endif

                        if (initResult != Result.Success)
                        {
                            throw new Exception("Epic Online Services didn't init correctly: " + initResult);
                        }
                    }
                }

                print($"EOSManager::Init: InitializePlatformInterface: initResult = {initResult}");


                s_hasInitializedPlatform = true;

                LoggingInterface.SetCallback(SimplePrintCallback);


                var eosPlatformInterface = CreatePlatformInterface();

                if (eosPlatformInterface == null)
                {
                    throw new Exception("failed to create an Epic Online Services PlatformInterface");
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
                IPlatformSpecifics platformSpecifics = EOSManagerPlatformSpecificsSingleton.Instance;
                if (platformSpecifics != null)
                {
                    print("EOSManager: Registering for platform-specific notifications");
                    platformSpecifics.RegisterForPlatformNotifications();
                }
            }

            //-------------------------------------------------------------------------
            [MonoPInvokeCallback(typeof(string))]
            private static void SimplePrintStringCallback(string str)
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", str);
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

                Debug.LogFormat(
                    type,
                    LogOption.NoStacktrace,
                    null, "{0:O} {1}({2}): {3}",
                    dateTime.ToString(DateTimeFormatInfo.InvariantInfo),
                    messageCategory,
                    message.Level,
                    message.Message);
            }

            //-------------------------------------------------------------------------
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

            //-------------------------------------------------------------------------
            /// <summary>
            /// Initialize log levels loaded from <see cref="LogLevelConfig" />.
            /// Should only be called after EOS library loaded, especially for dynamic linking platforms
            /// </summary>
            private void InitializeLogLevels()
            {
                var logLevelList = LogLevelUtility.LogLevelList;

                if (logLevelList == null)
                {
                    SetLogLevel(LogCategory.AllCategories, LogLevel.Info);
                    return;
                }

                for (int logCategoryIndex = 0; logCategoryIndex < logLevelList.Count; logCategoryIndex++)
                {
                    SetLogLevel((LogCategory)logCategoryIndex, logLevelList[logCategoryIndex]);
                }
            }

            //-------------------------------------------------------------------------
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

                if (logLevels.ContainsKey(Category))
                {
                    return logLevels[Category];
                }

                return LogLevel.Off;
            }

            //-------------------------------------------------------------------------
            [MonoPInvokeCallback(typeof(LogMessageFunc))]
            private static void SimplePrintCallbackWithCallstack(LogMessage message)
            {
                var dateTime = DateTime.Now;
                var messageCategory = message.Category.Length == 0 ? new Utf8String() : message.Category;

                print(string.Format("{0:O} {1}({2}): {3}", dateTime, messageCategory, message.Level, message.Message));
            }

            //-------------------------------------------------------------------------
            static private LoginOptions MakeLoginOptions(LoginCredentialType loginType,
                ExternalCredentialType externalCredentialType, string id, string token)
            {
                var loginCredentials = new Credentials
                {
                    Type = loginType,
                    ExternalType = externalCredentialType,
                    Id = id,
                    Token = token
                };

                var defaultScopeFlags =
                    AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence;

                return new LoginOptions
                {
                    Credentials = loginCredentials,
                    ScopeFlags = Config.Get<EOSConfig>().authScopeOptionsFlags.Count > 0
                        ? Config.Get<EOSConfig>().GetAuthScopeFlags()
                        : defaultScopeFlags
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
                var copyUserTokenOptions = new CopyUserAuthTokenOptions();

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

            //-------------------------------------------------------------------------
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

                foreach (string argument in Environment.GetCommandLineArgs())
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
                    else if (argument.StartsWith("-epicdeploymentid="))
                    {
                        ConfigureEpicArgument(argument, ref epicLauncherArgs.epicDeploymentID);
                    }
                }

                return epicLauncherArgs;
            }


            //-------------------------------------------------------------------------
            public void CreateConnectUserWithContinuanceToken(ContinuanceToken token,
                OnCreateConnectUserCallback onCreateUserCallback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                var options = new CreateUserOptions();

                options.ContinuanceToken = token;
                connectInterface.CreateUser(ref options, null, (ref CreateUserCallbackInfo createUserCallbackInfo) =>
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
            public void AuthLinkExternalAccountWithContinuanceToken(ContinuanceToken token,
                LinkAccountFlags linkAccountFlags, OnAuthLinkExternalAccountCallback callback)
            {
                var authInterface = GetEOSPlatformInterface().GetAuthInterface();
                var linkOptions = new LinkAccountOptions
                {
                    ContinuanceToken = token,
                    LinkAccountFlags = linkAccountFlags,
                    LocalUserId = null
                };

                if (linkAccountFlags.HasFlag(LinkAccountFlags.NintendoNsaId))
                {
                    linkOptions.LocalUserId = Instance.GetLocalUserId();
                }

                authInterface.LinkAccount(ref linkOptions, null,
                    (ref Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
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
            public void ConnectLinkExternalAccountWithContinuanceToken(ContinuanceToken token,
                OnConnectLinkExternalAccountCallback callback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                var linkAccountOptions = new Epic.OnlineServices.Connect.LinkAccountOptions();
                linkAccountOptions.ContinuanceToken = token;
                linkAccountOptions.LocalUserId = Instance.GetProductUserId();

                connectInterface.LinkAccount(ref linkAccountOptions, null,
                    (ref LinkAccountCallbackInfo linkAccountCallbackInfo) =>
                    {
                        if (callback != null)
                        {
                            callback(linkAccountCallbackInfo);
                        }
                    });
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <param name="epicAccountId"></param>
            /// <param name="onConnectLoginCallback"></param>
            public void StartConnectLoginWithEpicAccount(EpicAccountId epicAccountId,
                OnConnectLoginCallback onConnectLoginCallback)
            {
                var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                var copyUserTokenOptions = new CopyUserAuthTokenOptions();
                var result =
                    EOSAuthInterface.CopyUserAuthToken(ref copyUserTokenOptions, epicAccountId, out Token? authToken);
                var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions();

                if (result == Result.NotFound)
                {
                    print("No User Auth tokens found to login");
                    if (onConnectLoginCallback != null)
                    {
                        var dummyLoginCallbackInfo = new Epic.OnlineServices.Connect.LoginCallbackInfo();
                        dummyLoginCallbackInfo.ResultCode = Result.ConnectAuthExpired;
                        onConnectLoginCallback(dummyLoginCallbackInfo);
                    }

                    return;
                }

                print($"CopyUserAuthToken result code: {result}");

                if (!authToken.HasValue)
                {
                    print("authToken was not found, unable to login");

                    var dummyLoginCallbackInfo = new Epic.OnlineServices.Connect.LoginCallbackInfo();
                    dummyLoginCallbackInfo.ResultCode = Result.InvalidAuth;
                    onConnectLoginCallback(dummyLoginCallbackInfo);

                    return;
                }

                // If the authToken returned a value, and there is a RefreshToken, then try to login using that
                // Otherwise, try to use the AccessToken if that's available
                // One or the other should be provided, but if neither is available then fail to login
                if (authToken.Value.RefreshToken != null)
                {
                    print("Attempting to use refresh token to login with connect");

                    // need to refresh the epicaccount id
                    // LoginCredentialType.RefreshToken
                    Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.RefreshToken, null,
                        authToken.Value.RefreshToken, callbackInfo =>
                        {
                            var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
                            var copyUserTokenOptions = new CopyUserAuthTokenOptions();
                            var result = EOSAuthInterface.CopyUserAuthToken(ref copyUserTokenOptions,
                                callbackInfo.LocalUserId, out Token? userAuthToken);

                            connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                            {
                                Token = userAuthToken.Value.AccessToken,
                                Type = ExternalCredentialType.Epic
                            };

                            StartConnectLoginWithOptions(connectLoginOptions, onConnectLoginCallback);
                        });
                }
                else if (authToken.Value.AccessToken != null)
                {
                    print("Attempting to use access token to login with connect");

                    connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                    {
                        Token = authToken.Value.AccessToken,
                        Type = ExternalCredentialType.Epic
                    };

                    StartConnectLoginWithOptions(connectLoginOptions, onConnectLoginCallback);
                }
                else
                {
                    print("authToken has a value, but neither the refresh token nor the access token was provided. Cannot login.");

                    var dummyLoginCallbackInfo = new Epic.OnlineServices.Connect.LoginCallbackInfo();
                    dummyLoginCallbackInfo.ResultCode = Result.InvalidAuth;
                    onConnectLoginCallback(dummyLoginCallbackInfo);
                }
            }

            //-------------------------------------------------------------------------
            public void StartConnectLoginWithOptions(ExternalCredentialType externalCredentialType, string token,
                string displayname = null, string nsaIdToken = null, OnConnectLoginCallback onloginCallback = null)
            {
                var loginOptions = new Epic.OnlineServices.Connect.LoginOptions();
                loginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = token,
                    Type = externalCredentialType
                };

                switch (externalCredentialType)
                {
                    case ExternalCredentialType.EpicIdToken:
                        // If an NSA ID token is provided for an Epic ID token login, also added it in the login info
                        // to connect to Nintendo services along with Epic.
                        if (!string.IsNullOrEmpty(nsaIdToken))
                        {
                            loginOptions.UserLoginInfo = new UserLoginInfo
                            {
                                DisplayName = displayname,
                                NsaIdToken = nsaIdToken,
                            };
                        }

                        break;
                    case ExternalCredentialType.XblXstsToken:
                        loginOptions.UserLoginInfo = null;
                        break;
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
            public void StartConnectLoginWithOptions(ExternalCredentialType externalCredentialType, string token,
                string displayname, OnConnectLoginCallback onloginCallback)
            {
                StartConnectLoginWithOptions(externalCredentialType, token, displayname, null, onloginCallback);
            }

            //-------------------------------------------------------------------------
            // 
            public void StartConnectLoginWithOptions(Epic.OnlineServices.Connect.LoginOptions connectLoginOptions,
                OnConnectLoginCallback onloginCallback)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();
                connectInterface.Login(ref connectLoginOptions, null,
                    (ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginData) =>
                    {
                        if (connectLoginData.ResultCode != Result.Success)
                        {
                            print($"Connect login was not successful. ResultCode: {connectLoginData.ResultCode}", LogType.Error);
                        }

                        if (connectLoginData.LocalUserId != null)
                        {
                            SetLocalProductUserId(connectLoginData.LocalUserId);
                            ConfigureConnectStatusCallback();
                            ConfigureConnectExpirationCallback(connectLoginOptions);
                            OnConnectLogin?.Invoke(connectLoginData);
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
                connectLoginOptions.UserLoginInfo = new UserLoginInfo { DisplayName = displayName };

                connectLoginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = null,
                    Type = ExternalCredentialType.DeviceidAccessToken,
                };

                StartConnectLoginWithOptions(connectLoginOptions, onLoginCallback);
            }

            //-------------------------------------------------------------------------
            // Using this method is preferable as it allows the EOSManager to keep track of the product ID
            public void ConnectTransferDeviceIDAccount(TransferDeviceIdAccountOptions options, object clientData,
                OnTransferDeviceIdAccountCallback completionDelegate = null)
            {
                var connectInterface = GetEOSPlatformInterface().GetConnectInterface();

                connectInterface.TransferDeviceIdAccount(ref options, clientData,
                    (ref TransferDeviceIdAccountCallbackInfo data) =>
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
                StartLoginWithLoginTypeAndToken(LoginCredentialType.PersistentAuth, null, null, callbackInfo =>
                {
                    // Handle invalid or expired tokens for the caller
                    switch (callbackInfo.ResultCode)
                    {
                        case Result.AuthInvalidPlatformToken:
                        case Result.AuthInvalidRefreshToken:
                            var authInterface = Instance.GetEOSPlatformInterface().GetAuthInterface();
                            var options = new DeletePersistentAuthOptions();

                            authInterface.DeletePersistentAuth(ref options, null,
                                (ref DeletePersistentAuthCallbackInfo deletePersistentAuthCallbackInfo) =>
                                {
                                    if (onLoginCallback != null)
                                    {
                                        onLoginCallback(callbackInfo);
                                    }
                                });
                            return;
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
            public void StartLoginWithLoginTypeAndToken(LoginCredentialType loginType, string id, string token,
                OnAuthLoginCallback onLoginCallback)
            {
                StartLoginWithLoginTypeAndToken(loginType, ExternalCredentialType.Epic, id, token, onLoginCallback);
            }

            //-------------------------------------------------------------------------
            public void StartLoginWithLoginTypeAndToken(LoginCredentialType loginType,
                ExternalCredentialType externalCredentialType, string id, string token,
                OnAuthLoginCallback onLoginCallback)
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
                    var addNotifyLoginStatusChangedOptions = new AddNotifyLoginStatusChangedOptions();

                    ulong callbackHandle = EOSAuthInterface.AddNotifyLoginStatusChanged(
                        ref addNotifyLoginStatusChangedOptions, null,
                        (ref LoginStatusChangedCallbackInfo callbackInfo) =>
                        {
                            // if the user logged off
                            if (callbackInfo.CurrentStatus == LoginStatus.NotLoggedIn &&
                                callbackInfo.PrevStatus == LoginStatus.LoggedIn)
                            {
                                loggedInAccountIDs.Remove(callbackInfo.LocalUserId);
                            }
                        });
                    s_notifyLoginStatusChangedCallbackHandle = new NotifyEventHandle(callbackHandle, handle =>
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
                    var addNotifyLoginStatusChangedOptions =
                        new Epic.OnlineServices.Connect.AddNotifyLoginStatusChangedOptions();
                    ulong callbackHandle = EOSConnectInterface.AddNotifyLoginStatusChanged(
                        ref addNotifyLoginStatusChangedOptions, null,
                        (ref Epic.OnlineServices.Connect.LoginStatusChangedCallbackInfo callbackInfo) =>
                        {
                            if (callbackInfo.CurrentStatus == LoginStatus.NotLoggedIn &&
                                callbackInfo.PreviousStatus == LoginStatus.LoggedIn)
                            {
                                SetLocalProductUserId(null);
                            }
                            else if (callbackInfo.CurrentStatus == LoginStatus.LoggedIn &&
                                     callbackInfo.PreviousStatus == LoginStatus.NotLoggedIn)
                            {
                                SetLocalProductUserId(callbackInfo.LocalUserId);
                            }
                        });

                    s_notifyConnectLoginStatusChangedCallbackHandle = new NotifyEventHandle(callbackHandle, handle =>
                    {
                        GetEOSConnectInterface()?.RemoveNotifyLoginStatusChanged(handle);
                    });
                }
            }

            //-------------------------------------------------------------------------
            private void ConfigureConnectExpirationCallback(Epic.OnlineServices.Connect.LoginOptions connectLoginOptions)
            {
                if (s_notifyConnectAuthExpirationCallbackHandle == null)
                {
                    var EOSConnectInterface = GetEOSConnectInterface();
                    var addNotifyAuthExpirationOptions = new AddNotifyAuthExpirationOptions();
                    ulong callbackHandle = EOSConnectInterface.AddNotifyAuthExpiration(
                        ref addNotifyAuthExpirationOptions, null, (ref AuthExpirationCallbackInfo callbackInfo) =>
                        {
                            StartConnectLoginWithOptions(connectLoginOptions, null);
                        });

                    s_notifyConnectAuthExpirationCallbackHandle = new NotifyEventHandle(callbackHandle, handle =>
                    {
                        GetEOSConnectInterface()?.RemoveNotifyAuthExpiration(handle);
                    });
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
            public void StartLoginWithLoginOptions(LoginOptions loginOptions, OnAuthLoginCallback onLoginCallback)
            {
                // start login things
                var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();

                Assert.IsNotNull(EOSAuthInterface, "EOSAuthInterface was null!");

                // TODO: put this in a config file?
                var displayOptions = new SetDisplayPreferenceOptions
                {
                    NotificationLocation = NotificationLocation.TopRight
                };
                Instance.GetEOSPlatformInterface().GetUIInterface().SetDisplayPreference(ref displayOptions);

                print("StartLoginWithLoginTypeAndToken");

#if UNITY_IOS && !UNITY_EDITOR
                IOSLoginOptions modifiedLoginOptions = EOS_iOSLoginOptionsHelper.MakeIOSLoginOptionsFromDefault(loginOptions);

                EOSAuthInterface.Login(ref modifiedLoginOptions, null, (ref LoginCallbackInfo data) =>
                {
#else
                EOSAuthInterface.Login(ref loginOptions, null, (ref LoginCallbackInfo data) =>
                {
#endif
                    print("LoginCallBackResult : " + data.ResultCode);
                    if (data.ResultCode == Result.Success)
                    {
                        loggedInAccountIDs.Add(data.LocalUserId);

                        SetLocalUserId(data.LocalUserId);

                        ConfigureAuthStatusCallback();

                        OnAuthLogin?.Invoke(data);
                    }

                    if (onLoginCallback != null)
                    {
                        onLoginCallback(data);
                    }
                });
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
                var presenceHandle = new PresenceModification();
                var presenceModificationOption = new CreatePresenceModificationOptions();
                presenceModificationOption.LocalUserId = accountId;

                var createPresenceModificationResult =
                    presenceInterface.CreatePresenceModification(ref presenceModificationOption, out presenceHandle);

                if (createPresenceModificationResult != Result.Success)
                {
                    print("Unable to create presence modfication handle", LogType.Error);
                }

                var presenceModificationSetStatUsOptions = new PresenceModificationSetStatusOptions();
                presenceModificationSetStatUsOptions.Status = Status.Online;
                var setStatusResult = presenceHandle.SetStatus(ref presenceModificationSetStatUsOptions);

                if (setStatusResult != Result.Success)
                {
                    print("unable to set status", LogType.Error);
                }

                var richTextOptions = new PresenceModificationSetRawRichTextOptions();
                richTextOptions.RichText = richText;
                presenceHandle.SetRawRichText(ref richTextOptions);

                var options = new SetPresenceOptions();
                options.LocalUserId = accountId;
                options.PresenceModificationHandle = presenceHandle;
                presenceInterface.SetPresence(ref options, null, (ref SetPresenceCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode != Result.Success)
                    {
                        print("Unable to set presence: " + callbackInfo.ResultCode, LogType.Error);
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
                LogoutOptions options = new LogoutOptions { LocalUserId = accountId };

                EOSAuthInterface.Logout(ref options, null, (ref LogoutCallbackInfo data) =>
                {
                    if (onLogoutCallback == null)
                    {
                        return;
                    }

                    SetLocalUserId(null);

                    onLogoutCallback(ref data);

                    OnAuthLogout?.Invoke(data);
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
            /// <summary>
            /// Clears the stored token for persistent login
            /// </summary>
            public void RemovePersistentToken()
            {
                var authInterface = Instance.GetEOSPlatformInterface().GetAuthInterface();
                var options = new DeletePersistentAuthOptions();

                authInterface.DeletePersistentAuth(ref options, null,
                    (ref DeletePersistentAuthCallbackInfo deletePersistentAuthCallbackInfo) =>
                    {
                        if (deletePersistentAuthCallbackInfo.ResultCode != Result.Success)
                        {
                            print("Unable to delete persistent token, Result : " +
                                           deletePersistentAuthCallbackInfo.ResultCode, 
                                           LogType.Error);
                        }
                        else
                        {
                            print("Successfully deleted persistent token");
                        }
                    });
            }

            //-------------------------------------------------------------------------
            public void Tick()
            {
                if (GetEOSPlatformInterface() != null)
                {
                    // Poll for any application constrained state change that didn't
                    // already coincide with a prior application focus or pause event
                    UpdateApplicationConstrainedState();

                    UpdateNetworkStatus();

                    if (s_state != EOSState.Suspended)
                    {
                        // Only tick if awake?
                        GetEOSPlatformInterface().Tick();
                        if (s_state == EOSState.Suspending)
                        {
                            // do anything needed to inform EOS systems they need to suspend
                            s_state = EOSState.Suspended;
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------
            public void OnShutdown()
            {
                print("Shutting down");

                foreach (Action callback in s_onApplicationShutdownCallbacks)
                {
                    callback();
                }


                var PlatformInterface = GetEOSPlatformInterface();
                if (PlatformInterface != null)
                {
                    var EOSAuthInterface = PlatformInterface.GetAuthInterface();
                    // I don't need to create a new LogoutOption every time because the EOS wrapper API 
                    // makes a copy each time LogOut is called.
                    var logoutOptions = new LogoutOptions();

                    foreach (var epicUserID in loggedInAccountIDs)
                    {
                        logoutOptions.LocalUserId = epicUserID;
                        EOSAuthInterface.Logout(ref logoutOptions, null, (ref LogoutCallbackInfo data) =>
                        {
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
                    print("Running garbage collection.");
                    System.GC.Collect();

                    print("Waiting for pending finalizers.");
                    System.GC.WaitForPendingFinalizers();
#endif
                    print("Releasing the EOS Platform Interface.");
                    GetEOSPlatformInterface()?.Release();

                    if (s_eosUnloadSDKOnShutdown)
                    {
                        print("Shutting down the platform interface.");
                        ShutdownPlatformInterface();
                    }

                    SetEOSPlatformInterface(null);


#endif
#if UNITY_EDITOR
                    if (s_eosUnloadSDKOnShutdown)
                    {
                        print("Unloading all libraries.");
                        UnloadAllLibraries();
                    }
#endif
                    print("Finished shutdown.");
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
                    PlatformInterface.Shutdown();
                }

                s_hasInitializedPlatform = false;
            }

            //-------------------------------------------------------------------------
            public ApplicationStatus GetEOSApplicationStatus()
            {
                ApplicationStatus applicationStatus = GetEOSPlatformInterface().GetApplicationStatus();
                return applicationStatus;
            }

            //-------------------------------------------------------------------------
            private void SetEOSApplicationStatus(ApplicationStatus newStatus)
            {
                ApplicationStatus currentStatus = GetEOSApplicationStatus();
                if (currentStatus != newStatus)
                {
                    print($"EOSSingleton.SetEOSApplicationStatus: {currentStatus} -> {newStatus}");

                    Result result = GetEOSPlatformInterface().SetApplicationStatus(newStatus);
                    if (result != Result.Success)
                    {
                        print(
                            $"EOSSingleton.SetEOSApplicationStatus: Error setting EOS application status (Result = {result})",
                            LogType.Error);
                    }
                }
            }

            //-------------------------------------------------------------------------
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
#if UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT
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
#endif
                    }
                }
            }

            //-------------------------------------------------------------------------
            public void OnApplicationPause(bool isPaused)
            {
                bool wasPaused = s_isPaused;
                s_isPaused = isPaused;
                print($"EOSSingleton.OnApplicationPause: IsPaused {wasPaused} -> {s_isPaused}");

                //                // Poll for the latest application constrained state as we're about
                //                // to need it to determine the appropriate EOS application status
                //#if UNITY_PS4 || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT
                //                UpdateApplicationConstrainedState(false);
                //#else
                //                UpdateApplicationConstrainedState(true);
                //#endif
            }

            //-------------------------------------------------------------------------
            public void OnApplicationFocus(bool hasFocus)
            {
                bool hadFocus = s_hasFocus;
                s_hasFocus = hasFocus;
                print($"EOSSingleton.OnApplicationFocus: HasFocus {hadFocus} -> {s_hasFocus}");

                //                // Poll for the latest application constrained state as we're about
                //                // to need it to determine the appropriate EOS application status
                //#if UNITY_PS4 || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT
                //                UpdateApplicationConstrainedState(false);
                //#else
                //                UpdateApplicationConstrainedState(true);
                //#endif
            }

            //-------------------------------------------------------------------------
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

            //-------------------------------------------------------------------------
            // Call at least once per Update to poll whether or not the application has become constrained since
            // the last call (ie. is the application is now running in the background with reduced CPU/GPU resources?)
            // We must poll this because not all platforms generate a Unity event for constrained state changes
            // (if they even support constraining applications at all).
            private void UpdateApplicationConstrainedState()
            {
                if (EOSManagerPlatformSpecificsSingleton.Instance == null)
                {
                    return;
                }

                bool wasConstrained = s_isConstrained;
                bool isConstrained = EOSManagerPlatformSpecificsSingleton.Instance.IsApplicationConstrainedWhenOutOfFocus();

                // Constrained state changed?
                if (wasConstrained != isConstrained)
                {
                    s_isConstrained = isConstrained;
                    print(
                        $"EOSSingleton.OnApplicationConstrained: IsConstrained {wasConstrained} -> {s_isConstrained}");
                    UpdateEOSApplicationStatus();
                }
            }

            private static void UpdateNetworkStatus()
            {
                var platformSpecifics = EOSManagerPlatformSpecificsSingleton.Instance;

                platformSpecifics?.UpdateNetworkStatus();
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
            // If there's already been an EOSManager,
            // disable this behaviour so that it doesn't fire Unity messages
            if (s_EOSManagerInstance != null)
            {
                EOSSingleton.print($"{nameof(EOSManager)} {(nameof(Awake))}: An EOSManager instance already exists and is running, so this behaviour is marking as inactive to not perform duplicate work.");
                enabled = false;
                return;
            }

            // Indicate that a EOSManager has been created, and mark it to not be destroyed
            s_EOSManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);

#if UNITY_PS5 && !UNITY_EDITOR
            EOSPSNManagerPS5.EnsurePS5Initialized();
#endif

            Instance.Init(this);
        }

        //-------------------------------------------------------------------------
        /// <summary>Unity [Update](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html) is called every frame if enabled.
        /// <list type="bullet">
        ///     <item><description>Calls <c>Tick()</c></description></item>
        /// </list>
        /// </summary>
        void Update()
        {
            Instance.Tick();
        }

        //-------------------------------------------------------------------------
        /// <summary>Unity [OnApplicationFocus](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationFocus.html) is called when the application loses or gains focus.
        /// <list type="bullet">
        ///     <item><description>Calls <c>OnApplicationFocus()</c></description></item>
        /// </list>
        /// </summary>
        void OnApplicationFocus(bool hasFocus)
        {
            Instance.OnApplicationFocus(hasFocus);
        }

        //-------------------------------------------------------------------------
        /// <summary>If the game is hidden (fully or partly) by another application then Unity [OnApplicationPause](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationPause.html) will return true. When the game is changed back to current it will no longer be paused and OnApplicationPause will return to false.
        /// <list type="bullet">
        ///     <item><description>Calls <c>OnApplicationPause()</c></description></item>
        /// </list>
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            Instance.OnApplicationPause(pauseStatus);
        }

        /// <summary>
        /// Whenever the EOSManager becomes available and active, it subscribes
        /// to [Application.quitting](https://docs.unity3d.com/ScriptReference/Application-quitting.html).
        /// The event will only run when the application is definitely closing without the ability for it to be canceled.
        /// </summary>
        void OnEnable()
        {
            Application.quitting += OnApplicationQuitting;
        }

        /// <summary>
        /// Whenever the EOSManager becomes inactive, it unsubscribes to 
        /// to [Application.quitting](https://docs.unity3d.com/ScriptReference/Application-quitting.html).
        /// This is in case the manager is unloaded without the application ending.
        /// </summary>
        void OnDisable()
        {
            Application.quitting -= OnApplicationQuitting;
        }

        /// <summary>
        /// Event that should be subscribed to Application.quitting, with the event
        /// managed by <see cref="OnEnable"/> and <see cref="OnDisable"/>.
        /// This is intentionally named to be different than "OnApplicationQuit", which is a Unity Message
        /// that runs when Unity begins considering quitting.
        /// Instead, this should be subscribed to <see cref="Application.quitting"/>, which is an event
        /// that only fires when the Application is irreversably shutting down.
        /// </summary>
        void OnApplicationQuitting()
        {
            if (ShouldShutdownOnApplicationQuit)
            {
#if EOS_CAN_SHUTDOWN
                EOSSingleton.print($"{nameof(EOSManager)} ({nameof(OnApplicationQuitting)}): Application is quitting. {nameof(ShouldShutdownOnApplicationQuit)} is true, so the plugin is being shut down. EOS_CAN_SHUTDOWN is true, so the EOS SDK will now be shut down fully.");
#else
                EOSSingleton.print($"{nameof(EOSManager)} ({nameof(OnApplicationQuitting)}): Application is quitting. {nameof(ShouldShutdownOnApplicationQuit)} is true, so the plugin is being shut down. EOS_CAN_SHUTDOWN is false, so the EOS SDK will not be shut down.");
#endif
                Instance.OnShutdown();
            }
            else
            {
                EOSSingleton.print($"{nameof(EOSManager)} ({nameof(OnApplicationQuitting)}): Application is quitting. {nameof(ShouldShutdownOnApplicationQuit)} is false, so this manager will not shut down the EOS SDK.");
            }
        }
#endif

        //-------------------------------------------------------------------------
        void IEOSCoroutineOwner.StartCoroutine(IEnumerator routine)
        {
            base.StartCoroutine(routine);
        }
    }
}
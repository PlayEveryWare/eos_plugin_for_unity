/*
 * Copyright (c) 2026 Epic Games Inc
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

// Uncomment the following line to enable logging of application focus state 
// changes. If this is on it can clutter the log window and make debugging 
// difficult, so please enable it when you need to diagnose application focus
// state-related issues.
//#define LOG_APPLICATION_FOCUS_CHANGE

// Don't shut down the interface if running in the editor.
// According to the Epic documentation, shutting down this will disable a given loaded
// instance of the SDK from ever initializing again. Which is bad because Unity often (always?) loads a library just once
// up front for a given DLL.

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define EOS_CAN_SHUTDOWN
#endif


// As per Epic's documentation they recommend not calling PlatformInterface.Release / PlatformInterface.Shutdown in editor
// because Unity doesn't unload managed libraries until the Editor exits. Doing so will cause unintended side effects:
// https://dev.epicgames.com/docs/epic-online-services/eos-get-started/get-started-guide/next-steps#shut-down-the-eos-sdk
// This is also required on macOS/Linux always, because there isn't a known reliable way to unload shared libraries
#if UNITY_EDITOR && (!UNITY_EDITOR_WIN || !EOS_UNLOAD_SDK_ON_SHUTDOWN_IN_WIN_EDITOR)
// This define controls if the EOS SDK should be unloaded in the editor at shutdown to work around DLL unload errors.
#define EOS_DO_NOT_UNLOAD_SDK_ON_SHUTDOWN_IN_EDITOR
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
    //using Extensions;
    using Common;
    using Common.Extensions;
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

    using Epic.OnlineServices.Presence;
    using PlayEveryWare.EpicOnlineServices.Utility;

    using Extensions;
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

    using LogoutCallbackInfo = Epic.OnlineServices.Auth.LogoutCallbackInfo;
    using LogoutOptions = Epic.OnlineServices.Auth.LogoutOptions;
    using OnLogoutCallback = Epic.OnlineServices.Auth.OnLogoutCallback;
    using System.Threading.Tasks;
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
        /// <summary>
        /// Raised whenever the EOS overlay’s visibility changes, or when the title
        /// loses/regains focus and we must indicate whether the overlay was open
        /// before the interruption. Some platform-specific managers use
        /// this to synchronize their native overlay state.
        /// </summary>
        /// <param name="isOccluded">Whether the overlay was open when focus was lost</param>
        public delegate void OnOverlayOccludedChangedDelegate(bool isOccluded);

        public static event OnOverlayOccludedChangedDelegate OnOverlayOccludedChanged;
        private static event OnAuthLoginCallback OnAuthLogin;
        private static event OnAuthLogoutCallback OnAuthLogout;
        private static event OnConnectLoginCallback OnConnectLogin;

        /// <summary>
        /// Some platforms require additional user information while performing 
        /// a connect login. This delegate can be provided to saturate a
        /// UserLoginInfo during <see cref="StartConnectLoginWithEpicAccount"/>.
        /// If this is not provided, no UserLoginInfo will be set.
        /// </summary>
        public static Func<Task<UserLoginInfo>> GetUserLoginInfo = null;

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

        private static bool s_isOverlayOccluded;

        private static bool s_DoesOverlayHaveExcusiveInput;

        private static Coroutine s_restoreOverlayCoroutine;

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
            private const float NetworkStatusUpdateIntervalSecs = 0.5f;

            private static float s_nextNetworkStatusUpdateTime = 0.0f;
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
#if EOS_DO_NOT_UNLOAD_SDK_ON_SHUTDOWN_IN_EDITOR && UNITY_EDITOR
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
                Log("Changing PUID: " + LoggingUtils.Redact(PUIDToString(s_localProductUserId)) + " => " +
                      LoggingUtils.Redact(PUIDToString(localProductUserId)));
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
                return Config.Get<ProductConfig>().ProductId.ToString("N").ToLowerInvariant();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the SandboxID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetSandboxId()
            {
                return PlatformManager.GetPlatformConfig().deployment.SandboxId.ToString();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Get the DeploymentID configured from Unity Editor that was used during startup of the EOS SDK.
            /// </summary>
            /// <returns></returns>
            public string GetDeploymentID()
            {
                return PlatformManager.GetPlatformConfig().deployment.DeploymentId.ToString("N").ToLowerInvariant();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Check if encryption key is EOS config is a valid 32-byte hex string.
            /// </summary>
            /// <returns></returns>
            public bool IsEncryptionKeyValid()
            {
                return PlatformManager.GetPlatformConfig().clientCredentials.IsEncryptionKeyValid();
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
                       || PlatformManager.GetPlatformConfig().alwaysSendInputToOverlay
                    ;
            }

            public bool IsOverlayOpenWithExclusiveInput()
            {
                return s_isOverlayVisible && s_DoesOverlayHaveExcusiveInput;
            }

            //-------------------------------------------------------------------------
            [Conditional("ENABLE_DEBUG_EOSMANAGER")]
            internal static void Log(string toPrint, LogType type = LogType.Log)
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
                EOSInitializeOptions initOptions = ConfigurationUtility.GetEOSInitializeOptions();

#if UNITY_PS4 && !UNITY_EDITOR
                // On PS4, RegisterForPlatformNotifications is called at a later time by EOSPSNManager
#else
                RegisterForPlatformNotifications();
#endif

                return PlatformInterface.Initialize(ref initOptions.options);
            }

            //-------------------------------------------------------------------------
            private PlatformInterface CreatePlatformInterface()
            {
                EOSCreateOptions platformOptions = ConfigurationUtility.GetEOSCreateOptions();

                PlatformInterface platformInterface = PlatformInterface.Create(ref platformOptions.options);

#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
                platformOptions.options.IntegratedPlatformOptionsContainerHandle.Release();
#endif
                return platformInterface;

            }

            //-------------------------------------------------------------------------
            private void InitializeNetworkChecks(IEOSCoroutineOwner coroutineOwner)
            {
                EOSManagerPlatformSpecificsSingleton.Instance.InitializeNetworkChecks(coroutineOwner);
            }

            //-------------------------------------------------------------------------
            private void InitializeOverlay(IEOSCoroutineOwner coroutineOwner)
            {
                // Sets the button for the bringing up the overlay
                var friendToggle = new SetToggleFriendsButtonOptions
                {
                    ButtonCombination = PlatformManager.GetPlatformConfig().toggleFriendsButtonCombination
                };
                UIInterface uiInterface = Instance.GetEOSPlatformInterface().GetUIInterface();
                uiInterface.SetToggleFriendsButton(ref friendToggle);

                EOSManagerPlatformSpecificsSingleton.Instance.InitializeOverlay(coroutineOwner);

                AddNotifyDisplaySettingsUpdatedOptions addNotificationData = new();

                GetEOSUIInterface().AddNotifyDisplaySettingsUpdated(ref addNotificationData, null,
                    (ref OnDisplaySettingsUpdatedCallbackInfo data) =>
                    {
                        s_isOverlayVisible = data.IsVisible;
                        s_DoesOverlayHaveExcusiveInput = data.IsExclusiveInput;
                        OnOverlayOccludedChanged?.Invoke(s_isOverlayOccluded);
                    });
            }

            /// <summary>
            /// This function applies any command line arguments that may have
            /// been provided to the application from the Epic Games Launcher.
            /// </summary>
            private void ApplyCommandLineArguments()
            {
                EpicLauncherArgs epicArgs = GetCommandLineArgsFromEpicLauncher();

                // If neither the sandbox id nor the deployment id have been specified on the command line, the application of the arguments can stop here.
                if (string.IsNullOrEmpty(epicArgs.epicSandboxID) && string.IsNullOrEmpty(epicArgs.epicDeploymentID))
                {
                    return;
                }

                ProductConfig productConfig = Config.Get<ProductConfig>();

                bool sandboxIdOverrideByCommandLine = !string.IsNullOrEmpty(epicArgs.epicSandboxID);
                if (sandboxIdOverrideByCommandLine)
                {
                    Debug.Log($"Found sandbox id command line arg: {epicArgs.epicSandboxID}");

                    bool sandboxDefined = false;
                    SandboxId sandboxFromCommandLine = SandboxId.FromString(epicArgs.epicSandboxID);
                    foreach (var namedSandbox in productConfig.Environments.Sandboxes)
                    {
                        if (namedSandbox.Value.Equals(sandboxFromCommandLine))
                        {
                            Debug.Log($"{namedSandbox} selected as sandbox.");
                            sandboxDefined = true;
                            break;
                        }
                    }

                    PlatformManager.GetPlatformConfig().deployment.SandboxId = sandboxFromCommandLine;

                    if (!sandboxDefined)
                    {
                        Debug.LogWarning(
                            $"Sandbox Id \"{sandboxFromCommandLine}\" was " +
                            $"provided on the command line, but was not " +
                            $"found in the product config. Attempting to use " +
                            $"it regardless.");
                    }
                }

                if (!string.IsNullOrEmpty(epicArgs.epicDeploymentID))
                {
                    Debug.Log($"Found deployment id command line arg: {epicArgs.epicDeploymentID}");

                    bool deploymentDefined = false;

                    foreach (var namedDeployment in productConfig.Environments.Deployments)
                    {
                        // Check for equality regardless of case - and
                        // regardless of whether the dashes are included in the
                        // Guid for the purposes of comparison.
                        if (namedDeployment.Value.DeploymentId.ToString().Equals(epicArgs.epicDeploymentID,
                                StringComparison.OrdinalIgnoreCase) ||
                            namedDeployment.Value.DeploymentId.ToString("N").Equals(epicArgs.epicDeploymentID,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.Log($"{namedDeployment} selected as deployment.");
                            deploymentDefined = true;
                            break;
                        }
                    }

                    // NOTE: An empty guid is known to cause the EOS SDK to fail
                    //       to initialize - however in the native code when
                    //       this same operation is done, no check is performed
                    //       on whether the Guid is a valid Guid. This
                    //       implementation has been written to provide
                    //       verisimilitude with the native implementation on
                    //       Windows. Regardless - a warning is logged here -
                    //       despite the fact that it could be arguably be
                    //       logged as an error.
                    if (!Guid.TryParse(epicArgs.epicDeploymentID, out Guid deploymentFromCommandLine))
                    {
                        Debug.LogWarning(
                            $"ERROR: Invalid Guid " +
                            $"\"{epicArgs.epicDeploymentID}\" for Deployment " +
                            $"Id was provided on the command line. EOS SDK " +
                            $"will almost certainly fail to initialize.");

                        deploymentFromCommandLine = Guid.Empty;
                    }

                    PlatformManager.GetPlatformConfig().deployment.DeploymentId = deploymentFromCommandLine;

                    if (!deploymentFromCommandLine.Equals(Guid.Empty) && !deploymentDefined)
                    {
                        Debug.LogWarning(
                            $"Deployment \"{deploymentFromCommandLine}\" was " +
                            $"provided on the command line, but was not " +
                            $"found in the product config. Attempting to use " +
                            $"it regardless.");
                    }
                }
                else if (sandboxIdOverrideByCommandLine)
                {
                    Debug.LogWarning("[Warning] DeploymentID wasn't provided on the command line, while SandboxID was provided. If this is an EGS build this suggests that the configuration in the Epic Dev Portal is incorrect");
                }
            }

            public void Init(IEOSCoroutineOwner coroutineOwner, string configFileName = null)
            {
                // Apply overrides from command line args. They are handled by the native libs also, but we load
                // the config files from disk, so must also always apply them here
                ApplyCommandLineArguments();

                if (GetEOSPlatformInterface() != null)
                {
                    Log("Init completed with existing EOS PlatformInterface");

                    if (!hasSetLoggingCallback)
                    {
                        LoggingInterface.SetCallback(SimplePrintCallback);
                        hasSetLoggingCallback = true;
                    }

                    // The log levels are set in the native plugin
                    // This is here to sync the settings visually in UILogWindow
                    InitializeLogLevels();
                    InitializeNetworkChecks(coroutineOwner);
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

                Log($"EOSManager::Init: InitializePlatformInterface: initResult = {initResult}");


                s_hasInitializedPlatform = true;

                LoggingInterface.SetCallback(SimplePrintCallback);


                var eosPlatformInterface = CreatePlatformInterface();

                if (eosPlatformInterface == null)
                {
                    throw new Exception("failed to create an Epic Online Services PlatformInterface");
                }

                SetEOSPlatformInterface(eosPlatformInterface);
                UpdateEOSApplicationStatus();

                InitializeNetworkChecks(coroutineOwner);
                InitializeOverlay(coroutineOwner);

                Log("EOS loaded");
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
                    Log("EOSManager: Registering for platform-specific notifications");
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
                // This compile conditional is here to circumnavigate issues
                // unique to android with respect to Config class functionality.
#if UNITY_ANDROID && !UNITY_EDITOR
                SetLogLevel(LogCategory.AllCategories, LogLevel.Info);
                return;
#else
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
#endif
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

                Log(string.Format("{0:O} {1}({2}): {3}", dateTime, messageCategory, message.Level, message.Message));
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

                return new LoginOptions
                {
                    Credentials = loginCredentials,
                    ScopeFlags = PlatformManager.GetPlatformConfig().authScopeOptionsFlags,
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
            public static EpicLauncherArgs GetCommandLineArgsFromEpicLauncher()
            {
                var epicLauncherArgs = new EpicLauncherArgs();

                static void ConfigureEpicArgument(string argument, ref string argumentString)
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
            /// Starts a Connect Login using a provided EpicAccountId.
            /// If <see cref="GetUserLoginInfoDelegate"/> is set, this will
            /// use that delegate to determine the 
            /// <see cref="Epic.OnlineServices.Connect.LoginOptions.UserLoginInfo"/>.
            /// </summary>
            /// <param name="epicAccountId">
            /// The Epic Account to login as.
            /// This is provided by logging in through the Auth interface.
            /// </param>
            /// <param name="onConnectLoginCallback">
            /// Callback to run with information about the results of the login.
            /// Also contains the information needed to set ProductUserId.
            /// <see cref="s_localProductUserId"/>
            /// </param>
            public async void StartConnectLoginWithEpicAccount(EpicAccountId epicAccountId, OnConnectLoginCallback onConnectLoginCallback)
            {
                var authInterface = GetEOSPlatformInterface().GetAuthInterface();

                var idOpts = new Epic.OnlineServices.Auth.CopyIdTokenOptions
                {
                    AccountId = epicAccountId
                };

                var result = authInterface.CopyIdToken(ref idOpts, out Epic.OnlineServices.Auth.IdToken? idToken);

                if (result != Result.Success || !idToken.HasValue)
                {
                    Debug.LogError($"{nameof(EOSManager)} {nameof(StartConnectLoginWithEpicAccount)}: CopyIdToken failed with result: {result}");
                    var dummy = new Epic.OnlineServices.Connect.LoginCallbackInfo
                    {
                        ResultCode = Result.InvalidAuth
                    };
                    onConnectLoginCallback?.Invoke(dummy);
                    return;
                }

                Log($"{nameof(EOSManager)} {nameof(StartConnectLoginWithEpicAccount)}: Using ID Token for Connect Login");

                var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions
                {
                    Credentials = new Epic.OnlineServices.Connect.Credentials
                    {
                        Token = idToken.Value.JsonWebToken,
                        Type = Epic.OnlineServices.ExternalCredentialType.EpicIdToken
                    }
                };

                if (EOSManager.GetUserLoginInfo != null)
                {
                    connectLoginOptions.UserLoginInfo = await EOSManager.GetUserLoginInfo();
                }

                StartConnectLoginWithOptions(connectLoginOptions, onConnectLoginCallback);
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
                            Log($"Connect login was not successful. ResultCode: {connectLoginData.ResultCode}", LogType.Error);
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
                if (loginType == LoginCredentialType.ExchangeCode && string.IsNullOrEmpty(token))
                {
                    Debug.LogError($"{nameof(EOSManager)} {nameof(StartLoginWithLoginTypeAndToken)}: ExchangeCode login attempted with empty token. Abort login.");
                    onLoginCallback?.Invoke(new LoginCallbackInfo
                    {
                        ResultCode = Result.AuthExchangeCodeNotFound
                    });
                    return;
                }
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
                            if (connectLoginOptions.Credentials.HasValue &&
                                connectLoginOptions.Credentials.Value.Type == ExternalCredentialType.EpicIdToken)
                            {
                                var epicAccountId = GetLocalUserId();
                                if (epicAccountId != null && epicAccountId.IsValid())
                                {
                                    // connectLoginOptions captures the JWT string from initial login; by expiration
                                    // time that JWT is also stale. Fetch a fresh one from the Auth interface instead.
                                    StartConnectLoginWithEpicAccount(epicAccountId, null);
                                    return;
                                }
                            }
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

                Log("StartLoginWithLoginTypeAndToken");

#if UNITY_IOS && !UNITY_EDITOR
                IOSLoginOptions modifiedLoginOptions = EOS_iOSLoginOptionsHelper.MakeIOSLoginOptionsFromDefault(loginOptions);

                EOSAuthInterface.Login(ref modifiedLoginOptions, null, (ref LoginCallbackInfo data) =>
                {
#else
                EOSAuthInterface.Login(ref loginOptions, null, (ref LoginCallbackInfo data) =>
                {
#endif
                    Log("LoginCallBackResult : " + data.ResultCode);

                    if (data.ResultCode == Result.Success)
                    {
                        loggedInAccountIDs.Add(data.LocalUserId);

                        SetLocalUserId(data.LocalUserId);

                        ConfigureAuthStatusCallback();

                        OnAuthLogin?.Invoke(data);
                    }
                    else
                    {
                        string credentialType = loginOptions.Credentials?.Type.ToString() ?? "UNKNOWN";
                        Debug.LogWarning($"{nameof(EOSManager)} {nameof(StartLoginWithLoginOptions)}: {credentialType} login failed with ResultCode: {data.ResultCode}");
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
                    Log("Unable to create presence modfication handle", LogType.Error);
                }

                var presenceModificationSetStatUsOptions = new PresenceModificationSetStatusOptions();
                presenceModificationSetStatUsOptions.Status = Status.Online;
                var setStatusResult = presenceHandle.SetStatus(ref presenceModificationSetStatUsOptions);

                if (setStatusResult != Result.Success)
                {
                    Log("unable to set status", LogType.Error);
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
                        Log("Unable to set presence: " + callbackInfo.ResultCode, LogType.Error);
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
                            Log("Unable to delete persistent token, Result : " +
                                           deletePersistentAuthCallbackInfo.ResultCode,
                                           LogType.Error);
                        }
                        else
                        {
                            Log("Successfully deleted persistent token");
                        }
                    });
            }

            //-------------------------------------------------------------------------
            public void Tick()
            {
                ExecuteQueuedMainThreadTasks();
                if (GetEOSPlatformInterface() != null)
                {
                    // Poll for any application constrained state change that didn't
                    // already coincide with a prior application focus or pause event
                    UpdateApplicationConstrainedState();

                    if (Time.realtimeSinceStartup >= s_nextNetworkStatusUpdateTime)
                    {
                        UpdateNetworkStatus();
                        s_nextNetworkStatusUpdateTime = Time.realtimeSinceStartup + NetworkStatusUpdateIntervalSecs;
                    }

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
                Log("Shutting down");

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
                                Log("failed to logout ");
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
                    Log("Shutting down eos and releasing handles");
                    // Not doing this in the editor, because it doesn't seem to be an issue there
#if !UNITY_EDITOR_OSX
#if !UNITY_EDITOR
                    Log("Running garbage collection.");
                    System.GC.Collect();

                    Log("Waiting for pending finalizers.");
                    System.GC.WaitForPendingFinalizers();
#endif
                    Log("Disposing notification handles before platform release.");
                    s_notifyLoginStatusChangedCallbackHandle?.Dispose();
                    s_notifyLoginStatusChangedCallbackHandle = null;

                    s_notifyConnectLoginStatusChangedCallbackHandle?.Dispose();
                    s_notifyConnectLoginStatusChangedCallbackHandle = null;

                    s_notifyConnectAuthExpirationCallbackHandle?.Dispose();
                    s_notifyConnectAuthExpirationCallbackHandle = null;

                    Log("Releasing the EOS Platform Interface.");
                    GetEOSPlatformInterface()?.Release();

#if UNITY_EDITOR
                    if (s_eosUnloadSDKOnShutdown)
#endif
                    {
                        Log("Shutting down the platform interface.");
                        ShutdownPlatformInterface();
                    }
                    SetEOSPlatformInterface(null);


#endif
#if UNITY_EDITOR
                    if (s_eosUnloadSDKOnShutdown)
                    {
                        Log("Unloading all libraries.");
                        UnloadAllLibraries();
                    }
#endif
                    Log("Finished shutdown.");
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
                    Log($"EOSSingleton.SetEOSApplicationStatus: {currentStatus} -> {newStatus}");

                    Result result = GetEOSPlatformInterface().SetApplicationStatus(newStatus);
                    if (result != Result.Success)
                    {
                        Log(
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
                }
            }

            //-------------------------------------------------------------------------
            public void OnApplicationPause(bool isPaused)
            {
                bool wasPaused = s_isPaused;
                s_isPaused = isPaused;
                Log($"EOSSingleton.OnApplicationPause: IsPaused {wasPaused} -> {s_isPaused}");

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
#if LOG_APPLICATION_FOCUS_CHANGE
                bool hadFocus = s_hasFocus;
                Log($"EOSSingleton.OnApplicationFocus: HasFocus {hadFocus} -> {s_hasFocus}");
#endif
                s_hasFocus = hasFocus;

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
                Log($"EOSSingleton.OnApplicationConstrained: IsConstrained {wasConstrained} -> {s_isConstrained}");

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
                    Log(
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

        /// <summary>
        /// Actions that need to be executed on the main thread.
        /// Lazy allocated in <see cref="DispatchAsync"/>.
        /// </summary>
        private static List<Action> s_enqueuedTasks;

        /// <summary>
        /// Locak object used for <see cref="s_enqueuedTasks"/>, such that it can
        /// be executed thread-safe way.
        /// </summary>
        private static System.Object s_enqueuedTasksLock = new System.Object();

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
                EOSSingleton.Log($"{nameof(EOSManager)} {(nameof(Awake))}: An EOSManager instance already exists and is running, so this behaviour is marking as inactive to not perform duplicate work.");
                enabled = false;
                return;
            }

            // Indicate that a EOSManager has been created, and mark it to not be destroyed
            s_EOSManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);

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
        ///     <item>When the application loses focus (system UI opened), record whether the overlay
        ///     was visible at that moment and immediately notify listeners that the overlay is
        ///     now considered “occluded”. Platform modules use this to maintain correct
        ///     ApplicationStatus behavior or defer overlay restoration.</item>
        /// </list>
        /// </summary>
        void OnApplicationFocus(bool hasFocus)
        {
            Instance.OnApplicationFocus(hasFocus);

            if (!hasFocus)
            {
                if (s_isOverlayVisible)
                {
                    s_isOverlayOccluded = s_isOverlayVisible;
                    OnOverlayOccludedChanged?.Invoke(s_isOverlayOccluded);
                }
                return;
            }

            if (s_restoreOverlayCoroutine == null) 
            {
                s_restoreOverlayCoroutine = StartCoroutine(RestoreOverlayAfterDelay());
            }
        }


        /// <summary>
        /// After regaining focus, wait several frames before clearing occlusion state.
        /// This allows system UI dismissal, swapchain stabilization, and PrePresent
        /// to resume before native overlay state is re-enabled. This delay prevents
        /// race conditions where overlay state is updated too early.
        /// </summary>
        private System.Collections.IEnumerator RestoreOverlayAfterDelay()
        {
            yield return null;
            yield return null;
            yield return null;
            s_isOverlayOccluded = false;
            OnOverlayOccludedChanged?.Invoke(s_isOverlayOccluded);
            s_restoreOverlayCoroutine = null;
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
                EOSSingleton.Log($"{nameof(EOSManager)} ({nameof(OnApplicationQuitting)}): Application is quitting. {nameof(ShouldShutdownOnApplicationQuit)} is true, so the plugin is being shut down. EOS_CAN_SHUTDOWN is true, so the EOS SDK will now be shut down fully.");
#else
                EOSSingleton.Log($"{nameof(EOSManager)} ({nameof(OnApplicationQuitting)}): Application is quitting. {nameof(ShouldShutdownOnApplicationQuit)} is true, so the plugin is being shut down. EOS_CAN_SHUTDOWN is false, so the EOS SDK will not be shut down.");
#endif
                Instance.OnShutdown();
            }
            else
            {
                EOSSingleton.Log($"{nameof(EOSManager)} ({nameof(OnApplicationQuitting)}): Application is quitting. {nameof(ShouldShutdownOnApplicationQuit)} is false, so this manager will not shut down the EOS SDK.");
            }
        }
#endif

        //-------------------------------------------------------------------------
        void IEOSCoroutineOwner.StartCoroutine(IEnumerator routine)
        {
            base.StartCoroutine(routine);
        }

        /// <summary>
        /// Enqueues an Action to be executed on the main thread.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public static void DispatchAsync(Action action)
        {
            lock (s_enqueuedTasksLock)
            {
                // Lazy allocate the queue
                if (s_enqueuedTasks == null)
                {
                    s_enqueuedTasks = new List<Action>();
                }
                s_enqueuedTasks.Add(action);
            }
        }

        private static void ExecuteQueuedMainThreadTasks()
        {
            // Lock the enqued tasks list, and hold reference to the enqueued tasks.
            // This is done so that the foreach loop doesn't potentially go "forever"
            // if a given action in the queue happens to generate a list of tasks that 
            // also generate a list of task. The s_enqueuedTasks list is also nulled out
            // because we only allocate the list if we need to queue up new tasks. See DispatchSync
            List<Action> actionsToRun;
            lock (s_enqueuedTasksLock)
            {
                actionsToRun = s_enqueuedTasks;
                s_enqueuedTasks = null;
                if (actionsToRun == null)
                {
                    return;
                }
            }

            foreach (Action action in actionsToRun)
            {
                action.Invoke();
            }
        }
    }
}
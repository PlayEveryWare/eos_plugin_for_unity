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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using System;

using jint = System.Int32;
using jsize = System.Int32;
using JavaVM = System.IntPtr;
using System.Diagnostics;



#if UNITY_ANDROID && !UNITY_EDITOR
namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSAndroidOptions : IEOSCreateOptions
    {

        public Epic.OnlineServices.Platform.Options options;

        public Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainer IntegratedPlatformOptionsContainerHandle { get => options.IntegratedPlatformOptionsContainerHandle; set => options.IntegratedPlatformOptionsContainerHandle = value; }
        IntPtr IEOSCreateOptions.Reserved { get => options.Reserved; set => options.Reserved = value; }
        Utf8String IEOSCreateOptions.ProductId { get => options.ProductId; set => options.ProductId = value; }
        Utf8String IEOSCreateOptions.SandboxId { get => options.SandboxId; set => options.SandboxId = value; }
        ClientCredentials IEOSCreateOptions.ClientCredentials { get => options.ClientCredentials; set => options.ClientCredentials = value; }
        bool IEOSCreateOptions.IsServer { get => options.IsServer; set => options.IsServer = value; }
        Utf8String IEOSCreateOptions.EncryptionKey { get => options.EncryptionKey; set => options.EncryptionKey = value; }
        Utf8String IEOSCreateOptions.OverrideCountryCode { get => options.OverrideCountryCode; set => options.OverrideCountryCode = value; }
        Utf8String IEOSCreateOptions.OverrideLocaleCode { get => options.OverrideLocaleCode; set => options.OverrideLocaleCode = value; }
        Utf8String IEOSCreateOptions.DeploymentId { get => options.DeploymentId; set => options.DeploymentId = value; }
        PlatformFlags IEOSCreateOptions.Flags { get => options.Flags; set => options.Flags = value; }
        Utf8String IEOSCreateOptions.CacheDirectory { get => options.CacheDirectory; set => options.CacheDirectory = value; }
        uint IEOSCreateOptions.TickBudgetInMilliseconds { get => options.TickBudgetInMilliseconds; set => options.TickBudgetInMilliseconds = value; }
    }

    //-------------------------------------------------------------------------
    public class EOSAndroidInitializeOptions : IEOSInitializeOptions
    {
        public Epic.OnlineServices.Platform.AndroidInitializeOptions options;

        public IntPtr AllocateMemoryFunction { get => options.AllocateMemoryFunction; set => options.AllocateMemoryFunction = value; }
        public IntPtr ReallocateMemoryFunction { get => options.ReallocateMemoryFunction; set => options.ReallocateMemoryFunction = value; }
        public IntPtr ReleaseMemoryFunction { get => options.ReleaseMemoryFunction; set => options.ReleaseMemoryFunction = value; }
        public Utf8String ProductName { get => options.ProductName; set => options.ProductName = value; }
        public Utf8String ProductVersion { get => options.ProductVersion; set => options.ProductVersion = value; }
        public InitializeThreadAffinity? OverrideThreadAffinity { get => options.OverrideThreadAffinity; set => options.OverrideThreadAffinity = value; }
    }

    //-------------------------------------------------------------------------
    // Android specific Unity Parts.
    public partial class EOSPlatformSpecificsAndroid : IEOSManagerPlatformSpecifics
    {

        EOSAndroidConfig androidConfig;

        [DllImport("UnityHelpers_Android")]
        private static extern JavaVM UnityHelpers_GetJavaVM();

        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        static public void Register()
        {
            EOSManagerPlatformSpecifics.SetEOSManagerPlatformSpecificsInterface(new EOSPlatformSpecificsAndroid());
        }

        //-------------------------------------------------------------------------
        private string GetPathToEOSConfig()
        {
            return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", "eos_android_config.json");
        }

        //-------------------------------------------------------------------------
        EOSAndroidConfig GetEOSAndroidConfig()
        {
            string eosFinalConfigPath = GetPathToEOSConfig();
            var configDataAsString = AndroidFileIOHelper.ReadAllText(eosFinalConfigPath);
            androidConfig = JsonUtility.FromJson<EOSAndroidConfig>(configDataAsString);
            return androidConfig;
        }

        //-------------------------------------------------------------------------
        public string GetTempDir()
        {

            return Application.temporaryCachePath;
        }

        //-------------------------------------------------------------------------
        static private void ConfigureAndroidActivity()
        {

            UnityEngine.Debug.Log("EOSAndroid: Getting activity context...");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            if(context != null)
            {
                UnityEngine.Debug.Log("EOSAndroid: activity context found!");
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.epicgames.mobile.eossdk.EOSSDK");

                UnityEngine.Debug.Log("EOSAndroid: call EOS SDK init.");
                pluginClass.CallStatic("init", context);
            }
            else
            {
                UnityEngine.Debug.LogError("EOSAndroid: activity context is null!");
            }
        }

        //-------------------------------------------------------------------------
        // This does some work to configure the Android side of things before doing the
        // 'normal' EOS init things.
        // TODO: Configure the internal and external directory
        public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptionsRef, EOSConfig configData)
        {

            EOSAndroidInitializeOptions initializeOptions = (initializeOptionsRef as EOSAndroidInitializeOptions);

            if (initializeOptions == null)
            {
                throw new Exception("ConfigureSystemInitOptions: initializeOptions is null!");
            }
            initializeOptions.options.Reserved = IntPtr.Zero;


            AndroidInitializeOptionsSystemInitializeOptions androidInitOptionsSystemInitOptions = new AndroidInitializeOptionsSystemInitializeOptions();
            initializeOptions.options.SystemInitializeOptions = androidInitOptionsSystemInitOptions;

            ConfigureAndroidActivity();

            if (GetEOSAndroidConfig() != null)
            {
                if (initializeOptions.OverrideThreadAffinity.HasValue)
                {
                    var overrideThreadAffinity = initializeOptions.OverrideThreadAffinity.Value;
                    overrideThreadAffinity.NetworkWork = androidConfig.overrideValues.GetThreadAffinityNetworkWork(overrideThreadAffinity.NetworkWork);
                    overrideThreadAffinity.StorageIo = androidConfig.overrideValues.GetThreadAffinityStorageIO(overrideThreadAffinity.StorageIo);
                    overrideThreadAffinity.WebSocketIo = androidConfig.overrideValues.GetThreadAffinityWebSocketIO(overrideThreadAffinity.WebSocketIo);
                    overrideThreadAffinity.P2PIo = androidConfig.overrideValues.GetThreadAffinityP2PIO(overrideThreadAffinity.P2PIo);
                    overrideThreadAffinity.HttpRequestIo = androidConfig.overrideValues.GetThreadAffinityHTTPRequestIO(overrideThreadAffinity.HttpRequestIo);
                    overrideThreadAffinity.RTCIo = androidConfig.overrideValues.GetThreadAffinityRTCIO(overrideThreadAffinity.RTCIo);
                    initializeOptions.OverrideThreadAffinity = overrideThreadAffinity;
                }
            }
        }

        //-------------------------------------------------------------------------
        public IEOSCreateOptions CreateSystemPlatformOption()
        {

            return new EOSAndroidOptions();
        }

        //-------------------------------------------------------------------------
        public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
            var rtcOptions = new RTCOptions();



            // assume that RTC needs to be enabled and enable with a default option.
            (createOptions as EOSAndroidOptions).options.RTCOptions = rtcOptions;

        }

        //-------------------------------------------------------------------------
        private void InitailizeOverlaySupport()
        {

        }

        //-------------------------------------------------------------------------
        public void AddPluginSearchPaths(ref List<string> pluginPaths)
        {

        }

        //-------------------------------------------------------------------------
        public string GetDynamicLibraryExtension()
        {

            return ".so";
        }

        //-------------------------------------------------------------------------
        public void LoadDelegatesWithEOSBindingAPI()
        {

        }

        public IEOSInitializeOptions CreateSystemInitOptions()
        {

            return new EOSAndroidInitializeOptions();
        }


        public Result InitializePlatformInterface(IEOSInitializeOptions options)
        {

            return PlatformInterface.Initialize(ref (options as EOSAndroidInitializeOptions).options);
        }

        public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
        {

            return PlatformInterface.Create(ref (platformOptions as EOSAndroidOptions).options);
        }

        public void InitializeOverlay(IEOSCoroutineOwner owner)
        {

        }

        //-------------------------------------------------------------------------
        public void RegisterForPlatformNotifications()
        {

        }

        public bool IsApplicationConstrainedWhenOutOfFocus()
        {

            // TODO: Need to implement this for Android
            return false;
        }

        //-------------------------------------------------------------------------
        [Conditional("ENABLE_DEBUG_EOSMANAGERANDROID")]
        static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }
    }
}
#endif

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
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using System;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;

#if UNITY_IOS && !UNITY_EDITOR
namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSiOSOptions : IEOSCreateOptions
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
    public class EOSiOSInitializeOptions : IEOSInitializeOptions
    {
        public Epic.OnlineServices.Platform.InitializeOptions options;

        public IntPtr AllocateMemoryFunction { get => options.AllocateMemoryFunction; set => options.AllocateMemoryFunction = value; }
        public IntPtr ReallocateMemoryFunction { get => options.ReallocateMemoryFunction; set => options.ReallocateMemoryFunction = value; }
        public IntPtr ReleaseMemoryFunction { get => options.ReleaseMemoryFunction; set => options.ReleaseMemoryFunction = value; }
        public Utf8String ProductName { get => options.ProductName; set => options.ProductName = value; }
        public Utf8String ProductVersion { get => options.ProductVersion; set => options.ProductVersion = value; }
        public InitializeThreadAffinity? OverrideThreadAffinity { get => options.OverrideThreadAffinity; set => options.OverrideThreadAffinity = value; }
    }

    //-------------------------------------------------------------------------
    public class EOSPlatformSpecificsiOS : IEOSManagerPlatformSpecifics
    {
        EOS_iOSConfig iOSConfig;
        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        static public void Register()
        {
            EOSManagerPlatformSpecifics.SetEOSManagerPlatformSpecificsInterface(new EOSPlatformSpecificsiOS());
        }

        //-------------------------------------------------------------------------
        //TODO: Hook up correctly for each platform?
        public string GetTempDir()
        {
            return Application.temporaryCachePath;
        }

        //-------------------------------------------------------------------------
        public System.Int32 IsReadyForNetworkActivity()
        {
            return 1;
        }

        //-------------------------------------------------------------------------
        public void AddPluginSearchPaths(ref List<string> pluginPaths)
        {
        }

        //-------------------------------------------------------------------------
        public string GetDynamicLibraryExtension()
        {
            return ".dylib";
        }

        //-------------------------------------------------------------------------
        public void LoadDelegatesWithEOSBindingAPI()
        {

        }
        //-------------------------------------------------------------------------
        private string GetPathToEOSConfig()
        {
            return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", "eos_ios_config.json");
        }
        //-------------------------------------------------------------------------
        private EOS_iOSConfig GetEOSIOSConfig()
        {
            if (iOSConfig != null)
            {
                return iOSConfig;
            }

            string eosFinalConfigPath = GetPathToEOSConfig();
            
            if (!File.Exists(eosFinalConfigPath))
            {
                return null;
            }
            
            var configDataAsString = File.ReadAllText(eosFinalConfigPath);
            iOSConfig = JsonUtility.FromJson<EOS_iOSConfig>(configDataAsString);
            return iOSConfig;
        }
        //-------------------------------------------------------------------------
        public Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref (options as EOSiOSInitializeOptions).options);
        }
        //-------------------------------------------------------------------------
        public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
        {
            Debug.Log(platformOptions.ProductId);
            return Epic.OnlineServices.Platform.PlatformInterface.Create(ref (platformOptions as EOSiOSOptions).options);
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEOSInitializeOptions CreateSystemInitOptions()
        {
            return new EOSiOSInitializeOptions();
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializeOptions"></param>
        /// <param name="configData"></param>
        public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptionsRef, EOSConfig configData)
        {
            Debug.Log("ConfigureSystemInitOptions");

            EOSiOSInitializeOptions initializeOptions = (initializeOptionsRef as EOSiOSInitializeOptions);

            if (initializeOptions == null)
            {
                throw new Exception("ConfigureSystemInitOptions: initializeOptions is null!");
            }

            if (GetEOSIOSConfig() != null)
            {
                Debug.Log("GetEOSIOSConfig() is not null");
                if (initializeOptions.OverrideThreadAffinity.HasValue)
                {
                    var overrideThreadAffinity = initializeOptions.OverrideThreadAffinity.Value;
                    overrideThreadAffinity.NetworkWork = iOSConfig.overrideValues.GetThreadAffinityNetworkWork(overrideThreadAffinity.NetworkWork);
                    overrideThreadAffinity.StorageIo = iOSConfig.overrideValues.GetThreadAffinityStorageIO(overrideThreadAffinity.StorageIo);
                    overrideThreadAffinity.WebSocketIo = iOSConfig.overrideValues.GetThreadAffinityWebSocketIO(overrideThreadAffinity.WebSocketIo);
                    overrideThreadAffinity.P2PIo = iOSConfig.overrideValues.GetThreadAffinityP2PIO(overrideThreadAffinity.P2PIo);
                    overrideThreadAffinity.HttpRequestIo = iOSConfig.overrideValues.GetThreadAffinityHTTPRequestIO(overrideThreadAffinity.HttpRequestIo);
                    overrideThreadAffinity.RTCIo = iOSConfig.overrideValues.GetThreadAffinityRTCIO(overrideThreadAffinity.RTCIo);
                    initializeOptions.OverrideThreadAffinity = overrideThreadAffinity;
                }
            }
        }

        //-------------------------------------------------------------------------
        public IEOSCreateOptions CreateSystemPlatformOption()
        {
            var createOptions = new EOSiOSOptions();

            return createOptions;
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="createOptions"></param>
        public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
            var rtcOptions = new RTCOptions();

            (createOptions as EOSiOSOptions).options.RTCOptions = rtcOptions;
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Set Default Audio Session for iOS (Category to AVAudioSessionCategoryPlayAndRecord)
        /// </summary>
        [DllImport("__Internal")]
        static private extern void MicrophoneUtility_set_default_audio_session();

        //-------------------------------------------------------------------------
        /// <summary>
        /// Set Default Audio Session for iOS
        /// </summary>
        public void SetDefaultAudioSession()
        {
            MicrophoneUtility_set_default_audio_session();
        }
        //-------------------------------------------------------------------------

        public void InitializeOverlay(IEOSCoroutineOwner owner)
        {
        }

        //-------------------------------------------------------------------------
        public void RegisterForPlatformNotifications()
        {
        }

        public bool IsApplicationConstrainedWhenOutOfFocus()
        {
            return false;
        }
    }
}
#endif

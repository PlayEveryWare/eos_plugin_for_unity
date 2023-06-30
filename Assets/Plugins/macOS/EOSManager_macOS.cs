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

#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX) && EOS_PREVIEW_PLATFORM
namespace PlayEveryWare.EpicOnlineServices 
{
    //-------------------------------------------------------------------------
    public class EOSmacOSOptions : IEOSCreateOptions
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
    public class EOSmacOSInitializeOptions : IEOSInitializeOptions
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
    public class EOSPlatformSpecificsmacOS : IEOSManagerPlatformSpecifics
    {
        EOS_macOSConfig macOSConfig;
        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        static public void Register()
        {
            EOSManagerPlatformSpecifics.SetEOSManagerPlatformSpecificsInterface(new EOSPlatformSpecificsmacOS());
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
            return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", "eos_macos_config.json");
        }
        //-------------------------------------------------------------------------
        private EOS_macOSConfig GetEOS_macOSConfig()
        {
            if (macOSConfig != null)
            {
                return macOSConfig;
            }

            string eosFinalConfigPath = GetPathToEOSConfig();
            var configDataAsString = File.ReadAllText(eosFinalConfigPath);
            var configData = JsonUtility.FromJson<EOS_macOSConfig>(configDataAsString);
            macOSConfig = configData;
            return macOSConfig;
        }
        //-------------------------------------------------------------------------
        public Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref (options as EOSmacOSInitializeOptions).options);
        }

        //-------------------------------------------------------------------------
        public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Create(ref(platformOptions as EOSmacOSOptions).options);
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// The windows version doesn't have any specific types for init
        /// </summary>
        /// <returns></returns>
        public IEOSInitializeOptions CreateSystemInitOptions()
        {
            return new EOSmacOSInitializeOptions();
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Nothing to be done on windows for the moment
        /// </summary>
        /// <param name="initializeOptions"></param>
        /// <param name="configData"></param>
        public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptionsRef, EOSConfig configData)
        {
            Debug.Log("ConfigureSystemInitOptions");

            EOSmacOSInitializeOptions initializeOptions = (initializeOptionsRef as EOSmacOSInitializeOptions);

            if (initializeOptions == null)
            {
                throw new Exception("ConfigureSystemInitOptions: initializeOptions is null!");
            }

            if (GetEOS_macOSConfig() != null)
            {
                Debug.Log("GetEOS_macOSConfig() is not null");
                if (initializeOptions.OverrideThreadAffinity.HasValue)
                {
                    var overrideThreadAffinity = initializeOptions.OverrideThreadAffinity.Value;
                    overrideThreadAffinity.NetworkWork = macOSConfig.overrideValues.GetThreadAffinityNetworkWork(overrideThreadAffinity.NetworkWork);
                    overrideThreadAffinity.StorageIo = macOSConfig.overrideValues.GetThreadAffinityStorageIO(overrideThreadAffinity.StorageIo);
                    overrideThreadAffinity.WebSocketIo = macOSConfig.overrideValues.GetThreadAffinityWebSocketIO(overrideThreadAffinity.WebSocketIo);
                    overrideThreadAffinity.P2PIo = macOSConfig.overrideValues.GetThreadAffinityP2PIO(overrideThreadAffinity.P2PIo);
                    overrideThreadAffinity.HttpRequestIo = macOSConfig.overrideValues.GetThreadAffinityHTTPRequestIO(overrideThreadAffinity.HttpRequestIo);
                    overrideThreadAffinity.RTCIo = macOSConfig.overrideValues.GetThreadAffinityRTCIO(overrideThreadAffinity.RTCIo);
                    initializeOptions.OverrideThreadAffinity = overrideThreadAffinity;
                }
            }
        }

        //-------------------------------------------------------------------------
        public IEOSCreateOptions CreateSystemPlatformOption()
        {
            var createOptions = new  EOSmacOSOptions();

            return createOptions;
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// 
        /// <param name="createOptions"></param>
        public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
            var rtcOptions = new RTCOptions();

            (createOptions as EOSmacOSOptions).options.RTCOptions = rtcOptions;
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

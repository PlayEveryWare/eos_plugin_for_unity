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

#if UNITY_64 || UNITY_EDITOR_64
#define PLATFORM_64BITS
#elif (UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX) && EOS_PREVIEW_PLATFORM
#define PLATFORM_32BITS
#endif

#if UNITY_EDITOR
#define EOS_DYNAMIC_BINDINGS
#endif

//#define ENABLE_CONFIGURE_STEAM_FROM_MANAGED

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
using System.Text;
using Epic.OnlineServices.IntegratedPlatform;

#if !UNITY_EDITOR_WIN && (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && EOS_PREVIEW_PLATFORM
namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSLinuxOptions : IEOSCreateOptions
    {
        public Epic.OnlineServices.Platform.Options options;
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
    public class EOSLinuxInitializeOptions : IEOSInitializeOptions
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
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    struct EOSSteamInternal : IDisposable
    {
        public int ApiVersion;
        public IntPtr m_OverrideLibraryPath;

        public string OverrideLibraryPath
        {
            set
            {
                if(m_OverrideLibraryPath != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(m_OverrideLibraryPath);
                }
                if (value == null)
                {
                    m_OverrideLibraryPath = IntPtr.Zero;
                    return;
                }

                // Manually copy of the C# string to an UTF-8 C-String
                byte[] valueAsBytes = Encoding.UTF8.GetBytes(value);
                int valueSizeInBytes = valueAsBytes.Length;
                m_OverrideLibraryPath = Marshal.AllocCoTaskMem(valueSizeInBytes + 1);
                Marshal.Copy(valueAsBytes, 0, m_OverrideLibraryPath, valueSizeInBytes);
                // NULL terminate the string
                Marshal.WriteByte(m_OverrideLibraryPath, valueSizeInBytes, 0);
                
            }
        }

        public void Dispose()
        {
            if (m_OverrideLibraryPath != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_OverrideLibraryPath);
            }
        }
    }

    //-------------------------------------------------------------------------
    public class EOSPlatformSpecificsLinux : IEOSManagerPlatformSpecifics
    {
        public static string SteamConfigPath = "eos_steam_config.json";

#if ENABLE_CONFIGURE_STEAM_FROM_MANAGED
#if PLATFORM_64BITS
        static string SteamDllName = "steam_api64.dll";
#else
static string SteamDllName = "steam_api.dll";
#endif
#endif

        private static GCHandle SteamOptionsGCHandle;

        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        static public void Register()
        {
            EOSManagerPlatformSpecifics.SetEOSManagerPlatformSpecificsInterface(new EOSPlatformSpecificsLinux());
        }

        //-------------------------------------------------------------------------
        //TODO: Hook up correctly for each platform?
        public string GetTempDir()
        {
            return Application.temporaryCachePath;
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
#if EOS_DYNAMIC_BINDINGS
            const string EOSBinaryName = Epic.OnlineServices.Config.LibraryName;
            var eosLibraryHandle = EOSManager.EOSSingleton.LoadDynamicLibrary(EOSBinaryName);
#endif
        }

        //-------------------------------------------------------------------------
        private string GetPathToEOSConfig()
        {
            return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", "eos_linux_config.json");
        }

        //-------------------------------------------------------------------------
        EOSLinuxConfig GetEOSLinuxConfig()
        {
            EOSLinuxConfig linuxConfigData = null;
            string eosFinalConfigPath = GetPathToEOSConfig();

            if (File.Exists(eosFinalConfigPath))
            {
                var linuxConfigDataAsString = System.IO.File.ReadAllText(eosFinalConfigPath);
                linuxConfigData = JsonUtility.FromJson<EOSLinuxConfig>(linuxConfigDataAsString);
            }
            return linuxConfigData;
        }

        //-------------------------------------------------------------------------
        public Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref (options as EOSLinuxInitializeOptions).options);
        }

        //-------------------------------------------------------------------------
        public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Create(ref (platformOptions as EOSLinuxOptions).options);
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// The linux version doesn't have any specific types for init
        /// </summary>
        /// <returns></returns>
        public IEOSInitializeOptions CreateSystemInitOptions()
        {
            return new EOSLinuxInitializeOptions();
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Nothing to be done on linux for the moment
        /// </summary>
        /// <param name="initializeOptions"></param>
        /// <param name="configData"></param>
        public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptionsRef, EOSConfig configData)
        {
            EOSLinuxInitializeOptions initializeOptions = (initializeOptionsRef as EOSLinuxInitializeOptions);

            if (initializeOptions == null)
            {
                throw new Exception("ConfigureSystemInitOptions: initializeOptions is null!");
            }

            EOSLinuxConfig linuxConfig = GetEOSLinuxConfig();

            if (linuxConfig != null)
            {
                if (initializeOptions.OverrideThreadAffinity.HasValue)
                {
                    var overrideThreadAffinity = initializeOptions.OverrideThreadAffinity.Value;
                    overrideThreadAffinity.NetworkWork = linuxConfig.overrideValues.GetThreadAffinityNetworkWork(overrideThreadAffinity.NetworkWork);
                    overrideThreadAffinity.StorageIo = linuxConfig.overrideValues.GetThreadAffinityStorageIO(overrideThreadAffinity.StorageIo);
                    overrideThreadAffinity.WebSocketIo = linuxConfig.overrideValues.GetThreadAffinityWebSocketIO(overrideThreadAffinity.WebSocketIo);
                    overrideThreadAffinity.P2PIo = linuxConfig.overrideValues.GetThreadAffinityP2PIO(overrideThreadAffinity.P2PIo);
                    overrideThreadAffinity.HttpRequestIo = linuxConfig.overrideValues.GetThreadAffinityHTTPRequestIO(overrideThreadAffinity.HttpRequestIo);
                    overrideThreadAffinity.RTCIo = linuxConfig.overrideValues.GetThreadAffinityRTCIO(overrideThreadAffinity.RTCIo);
                    initializeOptions.OverrideThreadAffinity = overrideThreadAffinity;
                }
            }
        }

        //-------------------------------------------------------------------------
        public IEOSCreateOptions CreateSystemPlatformOption()
        {
            var createOptions = new EOSLinuxOptions();

            return createOptions;
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// On Linux, this method doesn't do anything specific at the moment
        /// </summary>
        /// <param name="createOptions"></param>
        public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
            var rtcOptions = new RTCOptions();
            (createOptions as EOSLinuxOptions).options.RTCOptions = rtcOptions;

            // This code seems to commonly cause hangs in the editor, so until those can be resolved this code is being 
            // disabled in the editor
#if !UNITY_EDITOR && ENABLE_CONFIGURE_STEAM_FROM_MANAGED
            string steamEOSFinalConfigPath = System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", SteamConfigPath);

            if (File.Exists(steamEOSFinalConfigPath))
            {
                var steamConfigDataAsString = System.IO.File.ReadAllText(steamEOSFinalConfigPath);
                var steamConfigData = JsonUtility.FromJson<EOSSteamConfig>(steamConfigDataAsString);
                var integratedPlatforms = new Epic.OnlineServices.IntegratedPlatform.Options[1];

                integratedPlatforms[0] = new Epic.OnlineServices.IntegratedPlatform.Options();

                integratedPlatforms[0].Type = Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformInterface.IptSteam;

                var steamIntegratedPlatform = new EOSSteamInternal
                {
                    ApiVersion = Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformInterface.SteamOptionsApiLatest
                };

                if (steamConfigData.overrideLibraryPath?.Length == 0)
                {
                    steamIntegratedPlatform.OverrideLibraryPath = null;
                }
                else
                {
                    steamIntegratedPlatform.OverrideLibraryPath = steamConfigData.overrideLibraryPath;
                }

                string SteamDllVersion = DLLHandle.GetVersionForLibrary(SteamDllName);

                if (steamIntegratedPlatform.m_OverrideLibraryPath != IntPtr.Zero)
                {
                    SteamOptionsGCHandle = GCHandle.Alloc(steamIntegratedPlatform, GCHandleType.Pinned);
                    integratedPlatforms[0].InitOptions = SteamOptionsGCHandle.AddrOfPinnedObject();


                    /*//TODO: Change this when the generated code updates so we can support the steam options in editor.
                    var integratedPlatformOptionsContainerAddOption = new Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainerAddOptions();
                    integratedPlatformOptionsContainerAddOption.Options = (createOptions as EOSLinuxOptions);
                    (createOptions as EOSLinuxOptions).IntegratedPlatformOptionsContainerHandle.Add(integratedPlatformOptionsContainerAddOption);*/

                }
            }
#endif
        }

        //-------------------------------------------------------------------------
        public void InitializeOverlay(IEOSCoroutineOwner owner)
        {
        }

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


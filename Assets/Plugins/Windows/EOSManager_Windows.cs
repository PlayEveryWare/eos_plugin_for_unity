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
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
#define PLATFORM_32BITS
#endif

//#define ENABLE_CONFIGURE_STEAM_FROM_MANAGED

using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;
using System.Text;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WSA_10_0)
namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSWindowsOptions : Epic.OnlineServices.Platform.WindowsOptions, IEOSCreateOptions
    {
    }

    //-------------------------------------------------------------------------
    public class EOSWindowsInitializeOptions : Epic.OnlineServices.Platform.InitializeOptions, IEOSInitializeOptions
    {

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
                if(m_OverrideLibraryPath != null)
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
            if (m_OverrideLibraryPath != null)
            {
                Marshal.FreeHGlobal(m_OverrideLibraryPath);
            }
        }
    }

    //-------------------------------------------------------------------------
    public class EOSPlatformSpecificsWindows : IEOSManagerPlatformSpecifics
    {
        static string Xaudio2DllName = "xaudio2_9redist.dll";
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
        static public void Register()
        {
            EOSManagerPlatformSpecifics.SetEOSManagerPlatformSpecificsInterface(new EOSPlatformSpecificsWindows());
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
            return ".dll";
        }

        //-------------------------------------------------------------------------
        static string GetPlatformPathComponent()
        {
#if PLATFORM_64BITS
            return "x64";
#elif PLATFORM_32BITS
            return "x86";
#else
            return "";
#endif
        }

        //-------------------------------------------------------------------------
        public Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Initialize(options as InitializeOptions);
        }

        //-------------------------------------------------------------------------
        public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Create((platformOptions as WindowsOptions));
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// The windows version doesn't have any specific types for init
        /// </summary>
        /// <returns></returns>
        public IEOSInitializeOptions CreateSystemInitOptions()
        {
            return new EOSWindowsInitializeOptions();
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Nothing to be done on windows for the moment
        /// </summary>
        /// <param name="initializeOptions"></param>
        /// <param name="configData"></param>
        public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptions, EOSConfig configData)
        {
        }

        //-------------------------------------------------------------------------
        public IEOSCreateOptions CreateSystemPlatformOption()
        {
            var createOptions = new EOSWindowsOptions();

            return createOptions;
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// On Windows, this method handles looking up where the RTC options are.
        /// This method assumes that the IEOSCreateOptions passed in is the right type.
        /// </summary>
        /// <param name="createOptions"></param>
        public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
            string pluginPlatfromPathComponent = GetPlatformPathComponent();

            if (pluginPlatfromPathComponent.Length > 0)
            {
                List<string> pluginPaths = DLLHandle.GetPathsToPlugins();
                var rtcPlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions();
                foreach (string pluginPath in pluginPaths)
                {
                    string path = Path.Combine(pluginPath, "Windows", pluginPlatfromPathComponent, Xaudio2DllName);
                    if (File.Exists(path))
                    {
                        rtcPlatformSpecificOptions.XAudio29DllPath = path;
                        break;
                    }

                    path = Path.Combine(pluginPath, pluginPlatfromPathComponent, Xaudio2DllName);
                    if (File.Exists(path))
                    {
                        rtcPlatformSpecificOptions.XAudio29DllPath = path;
                        break;
                    }

                }

                var rtcOptions = new WindowsRTCOptions();
                rtcOptions.PlatformSpecificOptions = rtcPlatformSpecificOptions;
                (createOptions as EOSWindowsOptions).RTCOptions = rtcOptions;

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

                    if (steamIntegratedPlatform.m_OverrideLibraryPath != null)
                    {
                        SteamOptionsGCHandle = GCHandle.Alloc(steamIntegratedPlatform, GCHandleType.Pinned);
                        integratedPlatforms[0].InitOptions = SteamOptionsGCHandle.AddrOfPinnedObject();
                        (createOptions as EOSWindowsOptions).IntegratedPlatforms = integratedPlatforms;
                    }
                }
#endif
            }
        }

        //-------------------------------------------------------------------------
        public void InitializeOverlay(IEOSCoroutineOwner owner)
        {
        }

        public void RegisterForPlatformNotifications()
        {
        }
    }
}
#endif


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

#if !EOS_DISABLE

//#define ENABLE_CONFIGURE_STEAM_FROM_MANAGED

// If standalone windows and not editor, or the windows editor.
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || UNITY_EDITOR_WIN

namespace PlayEveryWare.EpicOnlineServices
{
    using System.Collections.Generic;
    using UnityEngine;

    using Epic.OnlineServices.Platform;
    using System.Runtime.InteropServices;
    using Utility;

    public class EOSCreateOptions
    {
        public WindowsOptions options;
    }

    public class EOSInitializeOptions
    {
        public InitializeOptions options;
    }

    //-------------------------------------------------------------------------
    public class WindowsPlatformSpecifics : PlatformSpecifics<WindowsConfig>
    {
        static string Xaudio2DllName = "xaudio2_9redist.dll";
        public static string SteamConfigPath = "eos_steam_config.json";

#if ENABLE_CONFIGURE_STEAM_FROM_MANAGED
        private static readonly string SteamDllName = 
#if UNITY_64
        "steam_api64.dll";
#else
        "steam_api.dll";
#endif
#endif

        private static GCHandle SteamOptionsGCHandle;

        public WindowsPlatformSpecifics() : base(PlatformManager.Platform.Windows, ".dll") { }

        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static public void Register()
        {
            EOSManagerPlatformSpecificsSingleton.SetEOSManagerPlatformSpecificsInterface(new WindowsPlatformSpecifics());
        }

        //-------------------------------------------------------------------------
        public override void LoadDelegatesWithEOSBindingAPI()
        {
            // In the editor, EOS needs to be dynamically bound.
#if EOS_DYNAMIC_BINDINGS || UNITY_EDITOR 
            const string EOSBinaryName = Epic.OnlineServices.Config.LibraryName;
            var eosLibraryHandle = EOSManager.EOSSingleton.LoadDynamicLibrary(EOSBinaryName);
            Epic.OnlineServices.WindowsBindings.Hook<DLLHandle>(eosLibraryHandle, (DLLHandle handle, string functionName) => {
                return handle.LoadFunctionAsIntPtr(functionName);
            });
#endif
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Nothing to be done on windows for the moment
        /// </summary>
        /// <param name="initializeOptions"></param>
        public override void ConfigureSystemInitOptions(ref EOSInitializeOptions initializeOptions)
        {
            // Overriden with empty behavior because Windows has no concept of
            // override configuration values - the main EOSConfig values are
            // taken as they are.

            // TODO: This is a symptom of Windows historically having "default"
            //       configuration values. And needs to be fixed.
        }


        //-------------------------------------------------------------------------
        /// <summary>
        /// On Windows, this method handles looking up where the RTC options are.
        /// This method assumes that the IEOSCreateOptions passed in is the right type.
        /// </summary>
        /// <param name="createOptions"></param>
        public override void ConfigureSystemPlatformCreateOptions(ref EOSCreateOptions createOptions)
        {
            string pluginPlatformPath =
#if UNITY_64
            "x64";
#else
            "x86";
#endif

            if (pluginPlatformPath.Length > 0)
            {
                List<string> pluginPaths = DLLHandle.GetPathsToPlugins();
                var rtcPlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions();
                foreach (string pluginPath in pluginPaths)
                {
                    string path = FileSystemUtility.CombinePaths(pluginPath, "Windows", pluginPlatformPath, Xaudio2DllName);
                    if (FileSystemUtility.FileExists(path))
                    {
                        rtcPlatformSpecificOptions.XAudio29DllPath = path;
                        break;
                    }

                    path = FileSystemUtility.CombinePaths(pluginPath, pluginPlatformPath, Xaudio2DllName);
                    if (FileSystemUtility.FileExists(path))
                    {
                        rtcPlatformSpecificOptions.XAudio29DllPath = path;
                        break;
                    }

                }

                var rtcOptions = new WindowsRTCOptions();
                rtcOptions.PlatformSpecificOptions = rtcPlatformSpecificOptions;
                createOptions.options.RTCOptions = rtcOptions;

                // This code seems to commonly cause hangs in the editor, so until those can be resolved this code is being 
                // disabled in the editor
#if !UNITY_EDITOR && ENABLE_CONFIGURE_STEAM_FROM_MANAGED
                string steamEOSFinalConfigPath = System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", SteamConfigPath);

                if (File.Exists(steamEOSFinalConfigPath))
                {
                    var steamConfigDataAsString = FileUtility.ReadAllText(steamEOSFinalConfigPath);
                    var steamConfigData = JsonUtility.FromJson<SteamConfig>(steamConfigDataAsString);
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

                        /*
                         * TODO: Change this when the generated code updates so we can support the steam options in editor.
                        var integratedPlatformOptionsContainerAddOption = new Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainerAddOptions();
                        integratedPlatformOptionsContainerAddOption.Options = (createOptions as EOSCreateOptions);
                        (createOptions as EOSCreateOptions).IntegratedPlatformOptionsContainerHandle.Add(integratedPlatformOptionsContainerAddOption);
                        */
                    }
                }
#endif
            }
        }
    }
}
#endif
#endif // !EOS_DISABLE

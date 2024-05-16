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
using PlayEveryWare.EpicOnlineServices.Utility;
using UnityEngine;
using UnityEngine.Scripting;
using System.Runtime.InteropServices;

// If standalone linux and not editor, or the linux editor
#if (UNITY_STANDALONE_LINUX && !UNITY_EDITOR) || UNITY_EDITOR_LINUX

namespace PlayEveryWare.EpicOnlineServices
{
    public class EOSCreateOptions
    {
        public Epic.OnlineServices.Platform.Options options;
    }

    public class EOSInitializeOptions
    {
        public Epic.OnlineServices.Platform.InitializeOptions options;
    }

    //-------------------------------------------------------------------------
    public class LinuxPlatformSpecifics : PlatformSpecifics<LinuxConfig>
    {
        public static string SteamConfigPath = "eos_steam_config.json";

        private static GCHandle SteamOptionsGCHandle;

        public LinuxPlatformSpecifics() : base(PlatformManager.Platform.Linux, ".so") { }

        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static public void Register()
        {
            UnityEngine.Debug.LogWarning("Linux platform is currently in preview.");
            EOSManagerPlatformSpecificsSingleton.SetEOSManagerPlatformSpecificsInterface(new LinuxPlatformSpecifics());
        }

        //-------------------------------------------------------------------------
        public override void LoadDelegatesWithEOSBindingAPI()
        {
#if EOS_DYNAMIC_BINDINGS || UNITY_EDITOR
            // TODO: This code does not appear to do anything...
            const string EOSBinaryName = Epic.OnlineServices.Config.LibraryName;
            var eosLibraryHandle = EOSManager.EOSSingleton.LoadDynamicLibrary(EOSBinaryName);
#endif
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// On Linux, this method doesn't do anything specific at the moment
        /// </summary>
        /// <param name="createOptions"></param>
        public override void ConfigureSystemPlatformCreateOptions(ref EOSCreateOptions createOptions)
        {
            base.ConfigureSystemPlatformCreateOptions(ref createOptions);
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


                    /*//TODO: Change this when the generated code updates so we can support the steam options in editor.
                    var integratedPlatformOptionsContainerAddOption = new Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainerAddOptions();
                    integratedPlatformOptionsContainerAddOption.Options = (createOptions as EOSCreateOptions);
                    (createOptions as EOSCreateOptions).IntegratedPlatformOptionsContainerHandle.Add(integratedPlatformOptionsContainerAddOption);*/

                }
            }
#endif
        }
    }
}
#endif
#endif

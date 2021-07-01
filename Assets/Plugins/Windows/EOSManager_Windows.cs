﻿#if UNITY_64
#define PLATFORM_64BITS
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
#define PLATFORM_32BITS
#endif

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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

namespace PlayEveryWare.EpicOnlineServices
{
    public partial class EOSManager
    {
        static string Xaudio2DllName = "xaudio2_9redist.dll";

        //-------------------------------------------------------------------------
        //TODO: Hook up correctly for each platform?
        static string GetTempDir()
        {
            return Application.temporaryCachePath;
        }


        static private Int32 IsReadyForNetworkActivity()
        {
            return 1;
        }

        //-------------------------------------------------------------------------
        // Nothing to be done on windows for the moment
        static private void ConfigureSystemInitOptions(ref InitializeOptions initializeOptions)
        {
        }

        //-------------------------------------------------------------------------
        static string GetPlatformPathComponent()
        {
#if PLATFROM_64BITS
            return "x64";
#elif PLATFORM_32BITS
            return "x86";
#else
            return "";
#endif
        }

        //-------------------------------------------------------------------------
        // TODO merge this with the ConfigureSystemPlatformCreateOptions?
        static private WindowsOptions CreateSystemPlatformOption()
        {
            var createOptions = new WindowsOptions();

            return createOptions;
        }

        //-------------------------------------------------------------------------
        static private void ConfigureSystemPlatformCreateOptions(ref WindowsOptions createOptions)
        {
            string pluginPath = DLLHandle.GetPathToPlugins();
            string pluginPlatfromPathComponent = GetPlatformPathComponent();

            if (pluginPlatfromPathComponent.Length > 0)
            {
                var rtcPlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions();
                rtcPlatformSpecificOptions.XAudio29DllPath = Path.Combine(pluginPath, pluginPlatfromPathComponent, Xaudio2DllName);

                var rtcOptions = new WindowsRTCOptions();
                rtcOptions.PlatformSpecificOptions = rtcPlatformSpecificOptions;

                createOptions.RTCOptions = rtcOptions;
            }
        }
    }
}
#endif


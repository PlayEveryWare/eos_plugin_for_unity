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

﻿#if UNITY_64 || UNITY_EDITOR_64
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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WSA_10_0

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
#if PLATFORM_64BITS
            return "x64";
#elif PLATFORM_32BITS
            return "x86";
#else
            return "";
#endif
        }

        static private InitializeOptions CreateSystemInitOptions()
        {
            return new InitializeOptions();
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
                createOptions.RTCOptions = rtcOptions;
            }
        }
    }
}
#endif


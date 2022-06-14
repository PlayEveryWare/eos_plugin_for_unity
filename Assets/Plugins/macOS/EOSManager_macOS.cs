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
using System;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;

namespace PlayEveryWare.EpicOnlineServices 
{
    //-------------------------------------------------------------------------
    public class EOSmacOSOptions : Epic.OnlineServices.Platform.Options, IEOSCreateOptions
    {
    }

    //-------------------------------------------------------------------------
    public class EOSmacOSInitializeOptions : Epic.OnlineServices.Platform.InitializeOptions, IEOSInitializeOptions
    {

    }

    //-------------------------------------------------------------------------
    public class EOSPlatformSpecificsmacOS : IEOSManagerPlatformSpecifics
    {
        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
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
        public string GetDynamicLibraryExtension()
        {
            return ".dylib";
        }

        //-------------------------------------------------------------------------
        public Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Initialize(options as InitializeOptions);
        }

        //-------------------------------------------------------------------------
        public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
        {
            return Epic.OnlineServices.Platform.PlatformInterface.Create((platformOptions as Options));
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
        public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptions, EOSConfig configData)
        {
        }

        //-------------------------------------------------------------------------
        public IEOSCreateOptions CreateSystemPlatformOption()
        {
            var createOptions = new  EOSmacOSOptions();

            return createOptions;
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// On macOS, this method handles looking up where the RTC options are.
        /// This method assumes that the IEOSCreateOptions passed in is the right type.
        /// </summary>
        /// <param name="createOptions"></param>
        public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
        }

        //-------------------------------------------------------------------------
        public void InitializeOverlay(IEOSCoroutineOwner owner)
        {
        }

        //-------------------------------------------------------------------------
        public void RegisterForPlatformNotifications()
        {
        }

    }
}


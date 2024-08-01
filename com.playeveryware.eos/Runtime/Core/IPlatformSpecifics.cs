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

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if !EOS_DISABLE
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
#endif

namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSManagerPlatformSpecificsSingleton
    {
        static IPlatformSpecifics s_platformSpecifics;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            s_platformSpecifics = null;
        }

        //-------------------------------------------------------------------------
        // Should only be called once
        static public void SetEOSManagerPlatformSpecificsInterface(IPlatformSpecifics platformSpecifics)
        {
            if (s_platformSpecifics != null)
            {
                throw new Exception(string.Format("Trying to set the EOSManagerPlatformSpecificsSingleton twice: {0} => {1}", 
                    s_platformSpecifics.GetType().Name,
                    platformSpecifics == null ? "NULL" : platformSpecifics.GetType().Name
                ));
            }
            s_platformSpecifics = platformSpecifics;
        }

        //-------------------------------------------------------------------------
        static public IPlatformSpecifics Instance
        {
            get
            {
                return s_platformSpecifics;
            }
        }
    }

    //-------------------------------------------------------------------------
    public interface IEOSNetworkStatusUpdater
    {
        void UpdateNetworkStatus();
    }

    //-------------------------------------------------------------------------
    public interface IEOSCoroutineOwner
    {
        void StartCoroutine(IEnumerator routine);
    }

    //-------------------------------------------------------------------------
    public interface IPlatformSpecifics
    {
#if !EOS_DISABLE
        string GetTempDir();

       // Int32 IsReadyForNetworkActivity();

        void AddPluginSearchPaths(ref List<string> pluginPaths);

        string GetDynamicLibraryExtension();

//#if EOS_DYNAMIC_BINDINGS
        // Only called if EOS_DYNAMIC_BINDINGS is defined
        void LoadDelegatesWithEOSBindingAPI();
//#endif

        void ConfigureSystemInitOptions(ref EOSInitializeOptions initializeOptions);

        void ConfigureSystemPlatformCreateOptions(ref EOSCreateOptions createOptions);

        void InitializeOverlay(IEOSCoroutineOwner owner);

        void RegisterForPlatformNotifications();

        bool IsApplicationConstrainedWhenOutOfFocus();

        Int32 IsReadyForNetworkActivity();

        /// <summary>
        /// Sets the default audio session.
        /// NOTE: This is only implemented for iOS
        /// </summary>
        void SetDefaultAudioSession();

        void UpdateNetworkStatus();
#endif
    }
}

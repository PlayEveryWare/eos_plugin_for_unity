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

using UnityEngine;
using UnityEngine.Scripting;
using System.Runtime.InteropServices;
using System;

using jint = System.Int32;
using jsize = System.Int32;
using JavaVM = System.IntPtr;
using System.Diagnostics;

#if UNITY_ANDROID && !UNITY_EDITOR

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Platform;

    public class EOSCreateOptions
    {
        public Options options;
    }

    public class EOSInitializeOptions
    {
        public AndroidInitializeOptions options;
    }

    //-------------------------------------------------------------------------
    // Android specific Unity Parts.
    public class AndroidPlatformSpecifics : PlatformSpecifics<AndroidConfig>
    {

        [DllImport("UnityHelpers_Android")]
        private static extern JavaVM UnityHelpers_GetJavaVM();

        public AndroidPlatformSpecifics() : base(PlatformManager.Platform.Android, ".so") { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static public void Register()
        {
            EOSManagerPlatformSpecificsSingleton.SetEOSManagerPlatformSpecificsInterface(new AndroidPlatformSpecifics());
        }

        private static void ConfigureAndroidActivity()
        {
            UnityEngine.Debug.Log("EOSAndroid: Getting activity context...");
            using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if(activity != null)
            {
                UnityEngine.Debug.Log("EOSAndroid: activity context found!");
                using AndroidJavaClass pluginClass = new("com.epicgames.mobile.eossdk.EOSSDK");

                UnityEngine.Debug.Log("EOSAndroid: call EOS SDK init.");
                pluginClass.CallStatic("init", activity);
            }
            else
            {
                UnityEngine.Debug.LogError("EOSAndroid: activity context is null!");
            }
        }

        // This does some work to configure the Android side of things before doing the
        // 'normal' EOS init things.
        // TODO: Configure the internal and external directory
        public override void ConfigureSystemInitOptions(ref EOSInitializeOptions initializeOptionsRef)
        {
            // Do the standard overriding stuff
            base.ConfigureSystemInitOptions(ref initializeOptionsRef);

            // check again for if it's null after coming out of the base ConfigureSystemInitOptions method
            // (it shouldn't, but check anyways to make compile-time checks happy)
            if (initializeOptionsRef is not EOSInitializeOptions initializeOptions)
            {
                throw new Exception("ConfigureSystemInitOptions: initializeOptions is null!");
            }

            // Options that need to be set for Android specifically
            initializeOptions.options.Reserved = IntPtr.Zero;
            initializeOptions.options.SystemInitializeOptions = new();

            // Additional setup stuff needed for Android
            ConfigureAndroidActivity();
        }

        //-------------------------------------------------------------------------
        [Conditional("ENABLE_DEBUG_EOSMANAGERANDROID")]
        static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }
    }
}
#endif
#endif
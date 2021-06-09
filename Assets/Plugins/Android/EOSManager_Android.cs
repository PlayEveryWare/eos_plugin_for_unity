using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using System;

using jint = System.Int32;
using jsize = System.Int32;
using JavaVM = System.IntPtr;

#if UNITY_ANDROID && !UNITY_EDITOR
namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    // Android specific Unity Parts.
    public partial class EOSManager
    {
        static EOSAndroidInitializeOptions androidInitOptions;
        static IntPtr androidInitializeOptionsAllocH;

        [DllImport("UnityHelpers_Android")]
        private static extern JavaVM UnityHelpers_GetJavaVM();

        //-------------------------------------------------------------------------
        static string GetTempDir()
        {
            return Application.temporaryCachePath;
        }

        //-------------------------------------------------------------------------
        static private void ConfigureAndroidActivity()
        {
            Debug.Log("EOSAndroid: Getting activity context...");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            if(context != null)
            {
                Debug.Log("EOSAndroid: activity context found!");
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.epicgames.mobile.eossdk.EOSSDK");

                Debug.Log("EOSAndroid: call EOS SDK init.");
                pluginClass.CallStatic("init", context);
            }
            else
            {
                Debug.LogError("EOSAndroid: activity context is null!");
            }
        }

        //-------------------------------------------------------------------------
        // This does some work to configure the Android side of things before doing the
        // 'normal' EOS init things.
        // TODO: Configure the internal and external directory
        static private void ConfigureSystemInitOptions(ref InitializeOptions initializeOptions)
        {
            ConfigureAndroidActivity();

            // It should be safe to assume there is only one JVM, because
            // android assumes there is only one
            JavaVM javavm = UnityHelpers_GetJavaVM();

            Assert.IsTrue(javavm != IntPtr.Zero, "Fetched JavaVM is Null!");
            androidInitOptions = new EOSAndroidInitializeOptions
            {
                ApiVersion = 1,
                           VM = javavm,
                           OptionalInternalDirectory = IntPtr.Zero,
                           OptionalExternalDirectory = IntPtr.Zero
            };

            androidInitializeOptionsAllocH = Marshal.AllocHGlobal(Marshal.SizeOf<EOSAndroidInitializeOptions>());
            Marshal.StructureToPtr(androidInitOptions, androidInitializeOptionsAllocH, false);
            initializeOptions.SystemInitializeOptions = androidInitializeOptionsAllocH; 
        }
    }
}
#endif

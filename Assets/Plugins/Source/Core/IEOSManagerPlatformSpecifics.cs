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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !EOS_DISABLE
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
#endif

namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    /// <summary>
    /// This interface is to allow for abstracting away the platform specific 
    /// differences between the various init options.
    /// To use, define a new class that inherits from the platform specific version, then adopt
    /// this interface.
    /// </summary>
    public interface IEOSInitializeOptions
    {
#if !EOS_DISABLE
        System.IntPtr AllocateMemoryFunction { get; set; }

        System.IntPtr ReallocateMemoryFunction { get; set; }

        System.IntPtr ReleaseMemoryFunction { get; set; }

        Utf8String ProductName { get; set; }

        Utf8String ProductVersion { get; set; }

        InitializeThreadAffinity? OverrideThreadAffinity { get; set; }
#endif
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// This interface is to allow for abstracting away the platform specific 
    /// differences between the various create options.
    /// To use, define a new class that inherits from the platform specific version, then adopt
    /// this interface.
    /// </summary>
    public interface IEOSCreateOptions
    {
#if !EOS_DISABLE
        System.IntPtr Reserved { get; set; }
        Utf8String ProductId { get; set; }
        Utf8String SandboxId { get; set; }
        ClientCredentials ClientCredentials { get; set; }
        bool IsServer { get; set; }
        Utf8String EncryptionKey { get; set; }
        Utf8String OverrideCountryCode { get; set; }
        Utf8String OverrideLocaleCode { get; set; }
        Utf8String DeploymentId { get; set; }
        PlatformFlags Flags { get; set; }
        Utf8String CacheDirectory { get; set; }
        uint TickBudgetInMilliseconds { get; set; }

#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || (UNITY_STANDALONE_LINUX && EOS_PREVIEW_PLATFORM) || (UNITY_EDITOR_LINUX && EOS_PREVIEW_PLATFORM))
        Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformOptionsContainer IntegratedPlatformOptionsContainerHandle { get; set; }
#endif

#endif
    }

    //-------------------------------------------------------------------------
    public class EOSManagerPlatformSpecifics
    {
        static IEOSManagerPlatformSpecifics s_platformSpecifics;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            s_platformSpecifics = null;
        }

        //-------------------------------------------------------------------------
        // Should only be called once
        static public void SetEOSManagerPlatformSpecificsInterface(IEOSManagerPlatformSpecifics platformSpecifics)
        {
            if (s_platformSpecifics != null)
            {
                throw new Exception(string.Format("Trying to set the EOSManagerPlatformSpecifics twice: {0} => {1}", 
                    s_platformSpecifics.GetType().Name,
                    platformSpecifics == null ? "NULL" : platformSpecifics.GetType().Name
                ));
            }
            s_platformSpecifics = platformSpecifics;
        }

        //-------------------------------------------------------------------------
        static public IEOSManagerPlatformSpecifics Instance
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
    public interface IEOSManagerPlatformSpecifics
    {
#if !EOS_DISABLE
        string GetTempDir();

        void AddPluginSearchPaths(ref List<string> pluginPaths);

        string GetDynamicLibraryExtension();

//#if EOS_DYNAMIC_BINDINGS
        // Only called if EOS_DYNAMIC_BINDINGS is defined
        void LoadDelegatesWithEOSBindingAPI();
//#endif

        //-------------------------------------------------------------------------
        IEOSInitializeOptions CreateSystemInitOptions();
        void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptions, EOSConfig configData);

        IEOSCreateOptions CreateSystemPlatformOption();
        void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions);

        Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options);

        Epic.OnlineServices.Platform.PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions);

        void InitializeOverlay(IEOSCoroutineOwner owner);

        void RegisterForPlatformNotifications();

        bool IsApplicationConstrainedWhenOutOfFocus();
#endif
    }
}

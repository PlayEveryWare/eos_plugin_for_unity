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

using Epic.OnlineServices.Platform;
using System;
using System.Collections;
using System.Collections.Generic;

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
        System.IntPtr AllocateMemoryFunction { get; set; }

        System.IntPtr ReallocateMemoryFunction { get; set; }

        System.IntPtr ReleaseMemoryFunction { get; set; }

        string ProductName { get; set; }

        string ProductVersion { get; set; }

        InitializeThreadAffinity OverrideThreadAffinity { get; set; }
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
        System.IntPtr Reserved { get; set; }
        string ProductId { get; set; }
        string SandboxId { get; set; }
        ClientCredentials ClientCredentials { get; set; }
        bool IsServer { get; set; }
        string EncryptionKey { get; set; }
        string OverrideCountryCode { get; set; }
        string OverrideLocaleCode { get; set; }
        string DeploymentId { get; set; }
        PlatformFlags Flags { get; set; }
        string CacheDirectory { get; set; }
        uint TickBudgetInMilliseconds { get; set; }
    }

    //-------------------------------------------------------------------------
    public class EOSManagerPlatformSpecifics
    {
        static IEOSManagerPlatformSpecifics s_platformSpecifics;

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
    public interface IEOSCoroutineOwner
    {
        void StartCoroutine(IEnumerator routine);
    }

    //-------------------------------------------------------------------------
    public interface IEOSManagerPlatformSpecifics
    {
        string GetTempDir();

        void AddPluginSearchPaths(ref List<string> pluginPaths);

        string GetDynamicLibraryExtension();

        //-------------------------------------------------------------------------
        IEOSInitializeOptions CreateSystemInitOptions();
        void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptions, EOSConfig configData);

        IEOSCreateOptions CreateSystemPlatformOption();
        void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions);

        Epic.OnlineServices.Result InitializePlatformInterface(IEOSInitializeOptions options);

        Epic.OnlineServices.Platform.PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions);

        void InitializeOverlay(IEOSCoroutineOwner owner);

        void RegisterForPlatformNotifications();
    }
}

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
namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;

    public abstract class PlatformSpecifics<T> : IPlatformSpecifics where T : PlatformConfig, new()
    {
        protected PlatformManager.Platform Platform;
        protected T Config;

        #region Methods for which the functionality is shared (consider these "sealed")

        protected PlatformSpecifics(PlatformManager.Platform platform)
        {
            this.Platform = platform;
        }

        public string GetTempDir()
        {
            return UnityEngine.Application.temporaryCachePath;
        }

        public string GetDynamicLibraryExtension()
        {
            return PlatformManager.GetDynamicLibraryExtension(Platform);
        }

        public T GetConfig()
        {
            if (Config != null)
            {
                return Config;
            }

            return JsonUtility.FromJsonFile<T>(
                PlatformManager.GetConfigFilePath(Platform)
            );
        }

        #endregion


        #region Virtual methods that have a default behavior, but may need to be overwritten by deriving classes.

        public virtual void InitializeOverlay(IEOSCoroutineOwner owner)
        {
            // default behavior is to take no action.
        }

        public virtual void AddPluginSearchPaths(ref List<string> pluginPaths)
        {
            // default behavior is to take no action.
        }

        public virtual void LoadDelegatesWithEOSBindingAPI()
        {
            // default behavior is to take no action.
        }

        public virtual void RegisterForPlatformNotifications()
        {
            // default behavior is to take no action.
        }

        public virtual void SetDefaultAudioSession()
        {
            // default behavior is to take no action.
        }

        public virtual bool IsApplicationConstrainedWhenOutOfFocus()
        {
            // by default applications are not considered constrained when out of focus
            // this might be different on future platforms.
            return false;
        }
        public virtual void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
        {
            ((EOSCreateOptions)createOptions).options.RTCOptions = new();
        }

        public virtual void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptionsRef,
            EOSConfig configData)
        {
            Debug.Log("ConfigureSystemInitOptions");

            if (initializeOptionsRef is not EOSInitializeOptions initializeOptions)
            {
                throw new Exception("ConfigureSystemInitOptions: initializeOptions is null!");
            }

            if (GetConfig() != null)
            {
                Debug.Log("GetConfig() is not null");
                if (initializeOptions.OverrideThreadAffinity.HasValue)
                {
                    var overrideThreadAffinity = initializeOptions.OverrideThreadAffinity.Value;
                    overrideThreadAffinity.NetworkWork = Config.overrideValues.GetThreadAffinityNetworkWork(overrideThreadAffinity.NetworkWork);
                    overrideThreadAffinity.StorageIo = Config.overrideValues.GetThreadAffinityStorageIO(overrideThreadAffinity.StorageIo);
                    overrideThreadAffinity.WebSocketIo = Config.overrideValues.GetThreadAffinityWebSocketIO(overrideThreadAffinity.WebSocketIo);
                    overrideThreadAffinity.P2PIo = Config.overrideValues.GetThreadAffinityP2PIO(overrideThreadAffinity.P2PIo);
                    overrideThreadAffinity.HttpRequestIo = Config.overrideValues.GetThreadAffinityHTTPRequestIO(overrideThreadAffinity.HttpRequestIo);
                    overrideThreadAffinity.RTCIo = Config.overrideValues.GetThreadAffinityRTCIO(overrideThreadAffinity.RTCIo);
                    initializeOptions.OverrideThreadAffinity = overrideThreadAffinity;
                }
            }
        }

        public virtual Int32 IsReadyForNetworkActivity()
        {
            return 1;
        }

        #endregion
    }
}
#endif //!EOS_DISABLE
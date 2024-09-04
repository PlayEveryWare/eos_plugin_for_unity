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

namespace PlayEveryWare.EpicOnlineServices
{
    // This compile conditional is here so that when EOS_DISABLE is defined, and
    // subsequently the Epic namespace is not available, it's exclusion does not
    // cause compile errors.
#if !EOS_DISABLE
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Platform;
    using Epic.OnlineServices.IntegratedPlatform;
#endif
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Text.RegularExpressions;
    using Extensions;

    /// <summary>
    /// Represents the default deployment ID to use when a given sandbox ID is active.
    /// </summary>
    [Serializable]
    public class SandboxDeploymentOverride
    {
        public string sandboxID;
        public string deploymentID;
    }

    /// <summary>
    /// Represents the EOS Configuration used for initializing EOS SDK.
    /// </summary>
    [Serializable]
    public class EOSConfig : Config
    {
        static EOSConfig()
        {
            InvalidEncryptionKeyRegex = new Regex("[^0-9a-fA-F]");
            RegisterFactory(() => new EOSConfig());
        }

        protected EOSConfig() : base("EpicOnlineServicesConfig.json") { }

        /// <summary>
        /// Product Name defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string productName;

        /// <summary>
        /// Version of Product.
        /// </summary>
        public string productVersion;

        /// <summary>
        /// Product Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string productID;

        /// <summary>
        /// Sandbox Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string sandboxID;

        /// <summary>
        /// Deployment Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string deploymentID;

        /// <summary>
        /// SandboxDeploymentOverride pairs used to override Deployment ID when
        /// a given Sandbox ID is used.
        /// </summary>
        public List<SandboxDeploymentOverride> sandboxDeploymentOverrides;

        /// <summary>
        /// Client Secret defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string clientSecret;

        /// <summary>
        /// Client Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string clientID;

        /// <summary>
        /// Encryption Key&lt; used by default to decode files previously
        /// encoded and stored in EOS.
        /// </summary>
        public string encryptionKey;

        /// <summary>
        /// Flags; used to initialize the EOS platform.
        /// </summary>
        public List<string> platformOptionsFlags;

        /// <summary>
        /// Flags; used to set user auth when logging in.
        /// </summary>
        public List<string> authScopeOptionsFlags;

        /// <summary>
        /// Tick Budget; used to define the maximum amount of execution time the
        /// EOS SDK can use each frame.
        /// </summary>
        public uint tickBudgetInMilliseconds;

        /// <summary>
        /// TaskNetworkTimeoutSeconds; used to define the maximum number of seconds
        /// the EOS SDK will allow network calls to run before failing with EOS_TimedOut.
        /// This plugin treats any value that is less than or equal to zero as
        /// using the default value for the EOS SDK, which is 30 seconds.
        /// This value is only used when the <see cref="NetworkStatus"/> is not <see cref="NetworkStatus.Online"/>.
        /// <seealso cref="PlatformInterface.GetNetworkStatus"/>
        /// </summary>
        public double taskNetworkTimeoutSeconds;

        /// <summary>
        /// Network Work Affinity; specifies thread affinity for network
        /// management that is not IO.
        /// </summary>
        public string ThreadAffinity_networkWork;
        
        /// <summary>
        /// Storage IO Affinity; specifies affinity for threads that will
        /// interact with a storage device.
        /// </summary>
        public string ThreadAffinity_storageIO;
        
        /// <summary>
        /// Web Socket IO Affinity; specifies affinity for threads that generate
        /// web socket IO.
        /// </summary>
        public string ThreadAffinity_webSocketIO;
        
        /// <summary>
        /// P2P IO Affinity; specifies affinity for any thread that will
        /// generate IO related to P2P traffic and management.
        /// </summary>
        public string ThreadAffinity_P2PIO;
        
        /// <summary>
        /// HTTP Request IO Affinity; specifies affinity for any thread that
        /// will generate http request IO.
        /// </summary>
        public string ThreadAffinity_HTTPRequestIO;

        /// <summary>
        /// RTC IO Affinity&lt;/c&gt; specifies affinity for any thread that
        /// will generate IO related to RTC traffic and management.
        /// </summary>
        public string ThreadAffinity_RTCIO;

        /// <summary>
        /// Always Send Input to Overlay &lt;/c&gt;If true, the plugin will
        /// always send input to the overlay from the C# side to native, and
        /// handle showing the overlay. This doesn't always mean input makes
        /// it to the EOS SDK.
        /// </summary>
        public bool alwaysSendInputToOverlay;

        /// <summary>
        /// Initial Button Delay; Stored as a string so it can be 'empty'
        /// </summary>
        public string initialButtonDelayForOverlay;

        /// <summary>
        /// Repeat button delay for overlay; Stored as a string so it can be
        /// 'empty'.
        /// </summary>
        public string repeatButtonDelayForOverlay;

        /// <summary>
        /// HACK: send force send input without delay&lt;/c&gt;If true, the
        /// native plugin will always send input received directly to the SDK.
        /// If set to false, the plugin will attempt to delay the input to
        /// mitigate CPU spikes caused by spamming the SDK.
        /// </summary>
        public bool hackForceSendInputDirectlyToSDK;

        /// <summary>
        /// Set to 'true' if the application is a dedicated game server.
        /// </summary>
        public bool isServer;

        public static Regex InvalidEncryptionKeyRegex;
        
        private static bool IsEncryptionKeyValid(string key)
        {
            return
                //key not null
                key != null &&
                //key is 64 characters
                key.Length == 64 &&
                //key is all hex characters
                !InvalidEncryptionKeyRegex.Match(key).Success;
        }

        /// <summary>
        /// Override the default sandbox and deployment id. Uses the sandboxId
        /// as a key to determine the corresponding deploymentId that has been
        /// set by the user in the configuration window.
        /// </summary>
        /// <param name="sandboxId">The sandbox id to use.</param>
        public void SetDeployment(string sandboxId)
        {
            // Confirm that the sandboxId is stored in the list of overrides
            if (!TryGetDeployment(sandboxDeploymentOverrides, sandboxId,
                    out SandboxDeploymentOverride overridePair))
            {
                Debug.LogError($"The given sandboxId \"{sandboxId}\" could not be found in the configured list of deployment override values.");
                return;
            }

            Debug.Log($"Sandbox ID overridden to: \"{overridePair.sandboxID}\".");
            Debug.Log($"Deployment ID overridden to: \"{overridePair.deploymentID}\".");

            // Override the sandbox and deployment Ids
            sandboxID = overridePair.sandboxID;
            deploymentID = overridePair.deploymentID;

            // TODO: This will trigger a need to re-validate the config values
        }

        /// <summary>
        /// Given a specified SandboxId, try and retrieve the pair of values for
        /// the deployment override from the given list of deployment overrides.
        /// </summary>
        /// <param name="deploymentOverrides">
        /// The deployment overrides to search for the pair within.
        /// </param>
        /// <param name="sandboxId">
        /// The sandboxId of the pair to find.
        /// </param>
        /// <param name="deploymentOverride">
        /// The sandboxId and deploymentId override pair that matches the given
        /// sandboxId.
        /// </param>
        /// <returns>
        /// True if the pair was retrieved, false otherwise.
        /// </returns>
        private static bool TryGetDeployment(
            List<SandboxDeploymentOverride> deploymentOverrides,
            string sandboxId,
            out SandboxDeploymentOverride deploymentOverride)
        {
            deploymentOverride = null;
            foreach (var overridePair in deploymentOverrides)
            {
                if (overridePair.sandboxID != sandboxId)
                {
                    continue;
                }

                deploymentOverride = overridePair;
                return true;
            }

            return false;
        }

#if !EOS_DISABLE

        /// <summary>
        /// Returns a single PlatformFlags enum value that results from a
        /// bitwise OR operation of all the platformOptionsFlags flags on this
        /// config.
        /// </summary>
        /// <returns>A PlatformFlags enum value.</returns>
        public PlatformFlags GetPlatformFlags()
        {
            return StringsToEnum<PlatformFlags>(platformOptionsFlags, PlatformFlagsExtensions.TryParse);
        }

        /// <summary>
        /// Returns a single AuthScopeFlags enum value that results from a
        /// bitwise OR operation of all the authScopeOptionsFlags flags on this
        /// config.
        /// </summary>
        /// <returns>An AuthScopeFlags enum value.</returns>
        public AuthScopeFlags GetAuthScopeFlags()
        {
            return StringsToEnum<AuthScopeFlags>(authScopeOptionsFlags, AuthScopeFlagsExtensions.TryParse);
        }

        /// <summary>
        /// Given a reference to an InitializeThreadAffinity struct, set the
        /// member fields contained within to match the values of this config.
        /// </summary>
        /// <param name="affinity">
        /// The initialize thread affinity object to change the values of.
        /// </param>
        public void ConfigureOverrideThreadAffinity(ref InitializeThreadAffinity affinity)
        {
            affinity.NetworkWork = GetULongFromString(ThreadAffinity_networkWork);
            affinity.StorageIo = GetULongFromString(ThreadAffinity_storageIO);
            affinity.WebSocketIo = GetULongFromString(ThreadAffinity_webSocketIO);
            affinity.P2PIo = GetULongFromString(ThreadAffinity_P2PIO);
            affinity.HttpRequestIo = GetULongFromString(ThreadAffinity_HTTPRequestIO);
            affinity.RTCIo = GetULongFromString(ThreadAffinity_RTCIO);
        }
#endif

        /// <summary>
        /// Wrapper function for ulong.Parse. Returns the value from ulong.Parse
        /// if it succeeds, otherwise sets the value to the indicated default
        /// value.
        /// </summary>
        /// <param name="str">The string to parse into a ulong.</param>
        /// <param name="defaultValue">
        /// The value to return in the event parsing fails.
        /// </param>
        /// <returns>
        /// The result of parsing the string to a ulong, or defaultValue if
        /// parsing fails.
        /// </returns>
        private static ulong GetULongFromString(string str, ulong defaultValue = 0)
        {
            if (!ulong.TryParse(str, out ulong value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Determines whether the encryption key for the config is valid.
        /// </summary>
        /// <returns>
        /// True if the encryption key is valid, false otherwise.
        /// </returns>
        public bool IsEncryptionKeyValid()
        {
            return IsEncryptionKeyValid(encryptionKey);
        }
    }
}

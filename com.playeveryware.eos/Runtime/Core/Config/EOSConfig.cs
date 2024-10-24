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
    using Epic.OnlineServices.UI;
#endif
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Text.RegularExpressions;
    using Extensions;
    using Newtonsoft.Json;
    using PlayEveryWare.EpicOnlineServices.Utility;



    /// <summary>
    /// Represents the default deployment ID to use when a given sandbox ID is
    /// active.
    /// </summary>
    public class SandboxDeploymentOverride
    {
        [SandboxIDFieldValidator]
        public string sandboxID;

        [GUIDFieldValidator]
        public string deploymentID;
    }

    /// <summary>
    /// Represents the EOS Configuration used for initializing EOS SDK.
    /// </summary>
    [Serializable]
    [ConfigGroup("EOS Config", new[]
    {
        "Product Information",
        "Deployment",
        "Client Authentication",
        "Flags",
        "Thread Affinity & Tick Budgets",
        "Overlay Options"
    }, false)]
    public class EOSConfig : Config
    {
        static EOSConfig()
        {
            InvalidEncryptionKeyRegex = new Regex("[^0-9a-fA-F]");
            RegisterFactory(() => new EOSConfig());
        }

        protected EOSConfig() : base("EpicOnlineServicesConfig.json")
        { }

        #region Product Information

        /// <summary>
        /// Product Name defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        [ConfigField("Product Name", ConfigFieldType.Text,
            "Product name defined in the Development Portal.", 0)]
        [NonEmptyStringFieldValidator]
        public string productName;

        /// <summary>
        /// Version of Product.
        /// </summary>
        [ConfigField("Product Version", ConfigFieldType.Text,
            "Version of the product.", 0)]
        [NonEmptyStringFieldValidator]
        public string productVersion;

        /// <summary>
        /// Product Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        [ConfigField("Product Id", ConfigFieldType.Text,
            "Product Id defined in the Development Portal.", 0)]
        [GUIDFieldValidator]
        public string productID;

        #endregion

        #region Deployment

        /// <summary>
        /// Sandbox Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        [ConfigField("Sandbox Id", ConfigFieldType.Text,
            "Sandbox Id to use.", 1)]
        [SandboxIDFieldValidator]
        public string sandboxID;

        /// <summary>
        /// Deployment Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        [ConfigField("Deployment Id", ConfigFieldType.Text,
            "Deployment Id to use.", 1)]
        [GUIDFieldValidator]
        public string deploymentID;

        /// <summary>
        /// SandboxDeploymentOverride pairs used to override Deployment ID when
        /// a given Sandbox ID is used.
        /// </summary>
        [ConfigField("Sandbox Deployment Overrides",
            ConfigFieldType.TextList,
            "Deployment Id to use.", 1)]
        [ExpandField]
        public List<SandboxDeploymentOverride> sandboxDeploymentOverrides;

        /// <summary>
        /// Set to 'true' if the application is a dedicated game server.
        /// </summary>
        [ConfigField("Is Server",
            ConfigFieldType.Flag,
            "Indicates whether the application is a dedicated game " +
            "server.",
            1)]
        public bool isServer;

        #endregion

        #region Client Authentication

        /// <summary>
        /// Client Secret defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        [ConfigField("Client Secret", ConfigFieldType.Text,
            "Client Secret defined in the Development Portal.",
            2)]
        public string clientSecret;

        /// <summary>
        /// Client Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        [ConfigField("Client Id", ConfigFieldType.Text,
            "Client Id defined in the Development Portal.",
            2)]
        public string clientID;

        /// <summary>
        /// Encryption Key&lt; used by default to decode files previously
        /// encoded and stored in EOS.
        /// </summary>
        [ConfigField("Encryption Key", ConfigFieldType.Text,
            "Encryption key to use for client authentication.",
            2)]
        public string encryptionKey;

        /// <summary>
        /// Used to store what should be done when the Generate Key button is
        /// pressed.
        /// </summary>
        [ConfigField("Generate Key", ConfigFieldType.Button,
            "Click to generate an encryption key.",
            2)]
        private Action GenerateKeyButtonAction;

        #endregion

        #region Flags

#if !EOS_DISABLE
        /// <summary>
        /// Flags; used to initialize the EOS platform.
        /// </summary>
        [ConfigField("Platform Options",
            ConfigFieldType.TextList,
            "Platform option flags",
            3)]
        [JsonConverter(typeof(ListOfStringsToPlatformFlags))]
        public WrappedPlatformFlags platformOptionsFlags;

        /// <summary>
        /// Flags; used to set user auth when logging in.
        /// </summary>
        [ConfigField("Auth Scope Options",
            ConfigFieldType.TextList,
            "Platform option flags",
            3)]
        [JsonConverter(typeof(ListOfStringsToAuthScopeFlags))]
        public AuthScopeFlags authScopeOptionsFlags;
#endif

        #endregion

        #region Thread Affinity & Various Time Budgets

        /// <summary>
        /// Tick Budget; used to define the maximum amount of execution time the
        /// EOS SDK can use each frame.
        /// </summary>
        [ConfigField("Tick Budget (ms)",
            ConfigFieldType.Uint,
            "Used to define the maximum amount of execution time the " +
            "EOS SDK can use each frame.",
            3)]
        public uint tickBudgetInMilliseconds;

        /// <summary>
        /// TaskNetworkTimeoutSeconds; used to define the maximum number of
        /// seconds the EOS SDK will allow network calls to run before failing
        /// with EOS_TimedOut. This plugin treats any value that is less than or
        /// equal to zero as using the default value for the EOS SDK, which is
        /// 30 seconds.
        ///
        /// This value is only used when the <see cref="NetworkStatus"/> is not
        /// <see cref="NetworkStatus.Online"/>.
        /// <seealso cref="PlatformInterface.GetNetworkStatus"/>
        /// </summary>
        [ConfigField("Network Timeout Seconds",
            ConfigFieldType.Double,
            "Indicates the maximum number of seconds that EOS SDK " +
            "will allow network calls to run before failing with EOS_TimedOut.",
            3)]
        public double taskNetworkTimeoutSeconds;

        /// <summary>
        /// Network Work Affinity; specifies thread affinity for network
        /// management that is not IO.
        /// </summary>
        [ConfigField("Network Work", ConfigFieldType.Ulong,
            "Specifies affinity for threads that manage network tasks " +
            "that are not IO related.",
            3)]
        [JsonConverter(typeof(StringToTypeConverter<ulong>))]
        public ulong? ThreadAffinity_networkWork;

        /// <summary>
        /// Storage IO Affinity; specifies affinity for threads that will
        /// interact with a storage device.
        /// </summary>
        [ConfigField("Storage IO", ConfigFieldType.Ulong,
            "Specifies affinity for threads that generate storage IO.",
            3)]
        [JsonConverter(typeof(StringToTypeConverter<ulong>))]
        public ulong? ThreadAffinity_storageIO;

        /// <summary>
        /// Web Socket IO Affinity; specifies affinity for threads that generate
        /// web socket IO.
        /// </summary>
        [ConfigField("Web Socket IO", ConfigFieldType.Ulong,
            "Specifies affinity for threads that generate web socket " +
            "IO.", 3)]
        [JsonConverter(typeof(StringToTypeConverter<ulong>))]
        public ulong? ThreadAffinity_webSocketIO;

        /// <summary>
        /// P2P IO Affinity; specifies affinity for any thread that will
        /// generate IO related to P2P traffic and management.
        /// </summary>
        [ConfigField("P2P IO", ConfigFieldType.Ulong,
            "Specifies affinity for any thread that will generate IO " +
            "related to P2P traffic and management.",
            3)]
        [JsonConverter(typeof(StringToTypeConverter<ulong>))]
        public ulong? ThreadAffinity_P2PIO;

        /// <summary>
        /// HTTP Request IO Affinity; specifies affinity for any thread that
        /// will generate http request IO.
        /// </summary>
        [ConfigField("HTTP Request IO", ConfigFieldType.Ulong,
            "Specifies the affinity for any thread that will generate " +
            "HTTP request IO.",
            3)]
        [JsonConverter(typeof(StringToTypeConverter<ulong>))]
        public ulong? ThreadAffinity_HTTPRequestIO;

        /// <summary>
        /// RTC IO Affinity&lt;/c&gt; specifies affinity for any thread that
        /// will generate IO related to RTC traffic and management.
        /// </summary>
        [ConfigField("RTC IO", ConfigFieldType.Ulong,
            "Specifies the affinity for any thread that will generate " +
            "IO related to RTC traffic and management.",
            3)]
        [JsonConverter(typeof(StringToTypeConverter<ulong>))]
        public ulong? ThreadAffinity_RTCIO;

        #endregion

        #region Overlay Options

        /// <summary>
        /// Always Send Input to Overlay &lt;/c&gt;If true, the plugin will
        /// always send input to the overlay from the C# side to native, and
        /// handle showing the overlay. This doesn't always mean input makes it
        /// to the EOS SDK.
        /// </summary>
        [ConfigField("Always Send Input to Overlay",
            ConfigFieldType.Flag,
            "If true, the plugin will always send input to the " +
            "overlay from the C# side to native, and handle showing the " +
            "overlay. This doesn't always mean input makes it to the EOS SDK.",
            4)]
        public bool alwaysSendInputToOverlay;

        /// <summary>
        /// Initial Button Delay.
        /// </summary>
        [ConfigField("Initial Button Delay", ConfigFieldType.Float,
            "Initial Button Delay (if not set, whatever the default " +
            "is will be used).", 4)]
        [JsonConverter(typeof(StringToTypeConverter<float>))]
        public float? initialButtonDelayForOverlay;

        /// <summary>
        /// Repeat button delay for overlay.
        /// </summary>
        [ConfigField("Repeat Button Delay", ConfigFieldType.Text,
            "Repeat button delay for the overlay. If not set, " +
            "whatever the default is will be used.", 4)]
        [JsonConverter(typeof(StringToTypeConverter<float>))]
        public float? repeatButtonDelayForOverlay;

        // This compile conditional is here so that when EOS is disabled, in the
        // Epic namespace is referenced.
#if !EOS_DISABLE
        /// <summary>
        /// When this combination of buttons is pressed on a controller, the
        /// social overlay will toggle on.
        /// Default to <see cref="InputStateButtonFlags.SpecialLeft"/>, and will
        /// use that value if this configuration field is null, empty, or contains
        /// only <see cref="InputStateButtonFlags.None"/>.
        /// </summary>
        [JsonConverter(typeof(ListOfStringsToInputStateButtonFlags))]
        public InputStateButtonFlags toggleFriendsButtonCombination = InputStateButtonFlags.SpecialLeft;
#endif

        #endregion

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
        /// <param name="launcherSandboxId">The sandbox id to use.</param>
        public void SetDeployment(string launcherSandboxId)
        {
            // Confirm that the sandboxId is stored in the list of overrides
            if (TryGetDeployment(sandboxDeploymentOverrides, launcherSandboxId,
                    out SandboxDeploymentOverride overridePair))
            {
                Debug.Log($"Sandbox ID overridden to: \"{overridePair.sandboxID}\".");
                Debug.Log($"Deployment ID overridden to: \"{overridePair.deploymentID}\".");

                // Override the sandbox and deployment Ids
                sandboxID = overridePair.sandboxID;
                deploymentID = overridePair.deploymentID;
            }
            else 
            {
                if (sandboxID != launcherSandboxId) 
                {
                    throw new Exception($"The launcher sandboxId \"{launcherSandboxId}\" does not have a corresponding deploymentId configured.");
                }
            }
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
        /// Given a reference to an InitializeThreadAffinity struct, set the
        /// member fields contained within to match the values of this config.
        /// </summary>
        /// <param name="affinity">
        /// The initialize thread affinity object to change the values of.
        /// </param>
        public void ConfigureOverrideThreadAffinity(ref InitializeThreadAffinity affinity)
        {
            if (ThreadAffinity_HTTPRequestIO.HasValue)
            {
                affinity.HttpRequestIo = ThreadAffinity_HTTPRequestIO.Value;
            }

            if (ThreadAffinity_P2PIO.HasValue)
            {
                affinity.P2PIo = ThreadAffinity_P2PIO.Value;
            }

            if (ThreadAffinity_RTCIO.HasValue)
            {
                affinity.RTCIo = ThreadAffinity_RTCIO.Value;
            }

            if (ThreadAffinity_networkWork.HasValue)
            {
                affinity.NetworkWork = ThreadAffinity_networkWork.Value;
            }

            if (ThreadAffinity_storageIO.HasValue)
            {
                affinity.StorageIo = ThreadAffinity_storageIO.Value;
            }

            if (ThreadAffinity_webSocketIO.HasValue)
            {
                affinity.WebSocketIo = ThreadAffinity_webSocketIO.Value;
            }
        }
#endif

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

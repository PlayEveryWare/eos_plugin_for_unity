/*
 * Copyright (c) 2024 PlayEveryWare
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
    // This compile conditional is here so that when EOS is disabled, nothing is
    // referenced in the Epic namespace.
#if !EOS_DISABLE
    using Epic.OnlineServices.IntegratedPlatform;
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Platform;
    using Epic.OnlineServices.UI;
    using UnityEditor.VersionControl;
#endif
    using Common;
    using Newtonsoft.Json;
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;
    using Utility;

    /// <summary>
    /// Represents a set of configuration data for use by the EOS Plugin for
    /// Unity on a specific platform.
    /// </summary>
    [Serializable]
    [ConfigGroup("EOS Config", new[]
    {
        "Deployment",
        "Flags",
        "Thread Affinity & Tick Budgets",
        "Overlay Options"
    }, false)]
    public abstract class PlatformConfig : Config
    {
        private const PlatformManager.Platform OVERLAY_COMPATIBLE_PLATFORMS = ~(PlatformManager.Platform.Android |
                                                                   PlatformManager.Platform.iOS |
                                                                   PlatformManager.Platform.macOS |
                                                                   PlatformManager.Platform.Linux);

        /// <summary>
        /// The platform that the set of configuration data is to be applied on.
        /// </summary>
        [JsonIgnore]
        public PlatformManager.Platform Platform { get; }

        /// <summary>
        /// Any overriding values that should replace the central EOSConfig. On
        /// a platform-by-platform basis values can be overridden by setting
        /// these values. At runtime, these values will replace the ones on the
        /// central/main EOSConfig. This behavior is currently incomplete in
        /// it's implementation, but the intent is documented here for the sake
        /// of clarity and context.
        /// </summary>
        [JsonProperty] // Allow deserialization
        [JsonIgnore]   // Disallow serialization
        [Obsolete]     // Mark as obsolete so that warnings are generated
                       // whenever this is utilized (in the code that does the
                       // migration those warnings will be suppressed).
        public EOSConfig overrideValues;

        #region Deployment

        [ConfigField("Deployment", ConfigFieldType.Deployment, "Select the deployment to use.", 0)]
        public Deployment deployment;

        [ConfigField("Client Credentials", ConfigFieldType.ClientCredentials, "Select client credentials to use.", 0)]
        public EOSClientCredentials clientCredentials;

        #endregion

        #region Flags

        // This conditional is here because if EOS_DISABLE is defined, then
        // the field member types will not be available.
#if !EOS_DISABLE
        /// <summary>
        /// Flags; used to initialize the EOS platform.
        /// </summary>
        [ConfigField("Platform Options",
            ConfigFieldType.Enum,
            "Platform option flags",
            1)]
        [JsonConverter(typeof(ListOfStringsToPlatformFlags))]
        public WrappedPlatformFlags platformOptionsFlags;

        /// <summary>
        /// Flags; used to set user auth when logging in.
        /// </summary>
        [ConfigField("Auth Scope Options",
            ConfigFieldType.Enum,
            "Platform option flags",
            1)]
        [JsonConverter(typeof(ListOfStringsToAuthScopeFlags))]
        public AuthScopeFlags authScopeOptionsFlags;

        /// <summary>
        /// Used to store integrated platform management flags.
        /// </summary>
        [ConfigField("Integrated Platform Management Flags", 
            ConfigFieldType.Enum, "Integrated Platform Management " +
                                  "Flags for platform specific options.",
            1)]
        [JsonConverter(typeof(ListOfStringsToIntegratedPlatformManagementFlags))]
        [JsonProperty("flags")] // Allow deserialization from old field member.
        public IntegratedPlatformManagementFlags integratedPlatformManagementFlags;
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
            2)]
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
            2)]
        public double taskNetworkTimeoutSeconds;

        // This compile conditional is here so that when EOS is disabled, nothing is
        // referenced in the Epic namespace.
#if !EOS_DISABLE
        //[ConfigField("Thread Affinity")]
        public InitializeThreadAffinity threadAffinity;
#endif
        #endregion

        #region Overlay Options

        /// <summary>
        /// Always Send Input to Overlay &lt;/c&gt;If true, the plugin will
        /// always send input to the overlay from the C# side to native, and
        /// handle showing the overlay. This doesn't always mean input makes it
        /// to the EOS SDK.
        /// </summary>
        [ConfigField(OVERLAY_COMPATIBLE_PLATFORMS,
            "Always Send Input to Overlay",
            ConfigFieldType.Flag,
            "If true, the plugin will always send input to the " +
            "overlay from the C# side to native, and handle showing the " +
            "overlay. This doesn't always mean input makes it to the EOS SDK.",
            3)]
        public bool alwaysSendInputToOverlay;

        /// <summary>
        /// Initial Button Delay.
        /// </summary>
        [ConfigField(OVERLAY_COMPATIBLE_PLATFORMS,
            "Initial Button Delay", ConfigFieldType.Float,
            "Initial Button Delay (if not set, whatever the default " +
            "is will be used).", 3)]
        [JsonConverter(typeof(StringToTypeConverter<float>))]
        public float initialButtonDelayForOverlay;

        /// <summary>
        /// Repeat button delay for overlay.
        /// </summary>
        [ConfigField(OVERLAY_COMPATIBLE_PLATFORMS,
            "Repeat Button Delay", ConfigFieldType.Float,
            "Repeat button delay for the overlay. If not set, " +
            "whatever the default is will be used.", 3)]
        [JsonConverter(typeof(StringToTypeConverter<float>))]
        public float repeatButtonDelayForOverlay;

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
        [ConfigField(
            OVERLAY_COMPATIBLE_PLATFORMS,
            "Default Activate Overlay Button",
            ConfigFieldType.Enum,
            "Users can press the button's associated with this value " +
            "to activate the Epic Social Overlay. Not all combinations are " +
            "valid; the SDK will log an error at the start of runtime if an " +
            "invalid combination is selected.", 3)]
        public InputStateButtonFlags toggleFriendsButtonCombination = InputStateButtonFlags.SpecialLeft;
#endif

        #endregion

        /// <summary>
        /// Used to keep track of whether values have been moved from the
        /// deprecated overrideValues field member.
        /// </summary>
        [JsonProperty]
        private bool _configValuesMigrated = false;

        /// <summary>
        /// Create a PlatformConfig by defining the platform it pertains to.
        /// </summary>
        /// <param name="platform">
        /// The platform to apply the config values on.
        /// </param>
        protected PlatformConfig(PlatformManager.Platform platform) :
            base(PlatformManager.GetConfigFileName(platform))
        {
            Platform = platform;
        }

        #region Logic for Migrating Override Values from Previous Structure

        protected sealed class NonOverrideableConfigValues : Config
        {
            public uint tickBudgetInMilliseconds;
            public double taskNetworkTimeoutSeconds;
            public WrappedPlatformFlags platformOptionsFlags;
            public AuthScopeFlags authScopeOptionsFlags;
            public bool alwaysSendInputToOverlay;

            static NonOverrideableConfigValues()
            {
                RegisterFactory(() => new NonOverrideableConfigValues());
            }

            internal NonOverrideableConfigValues() : base("EpicOnlineServicesConfig.json") { }
        }

        internal sealed class OverrideableConfigValues : Config
        {
            [JsonConverter(typeof(ListOfStringsToPlatformFlags))]
            public WrappedPlatformFlags platformOptionsFlags;

            [JsonConverter(typeof(StringToTypeConverter<float>))]
            public float? initialButtonDelayForOverlay;

            [JsonConverter(typeof(StringToTypeConverter<float>))]
            public float? repeatButtonDelayForOverlay;

            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong? ThreadAffinity_networkWork;

            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong? ThreadAffinity_storageIO;

            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong? ThreadAffinity_webSocketIO;

            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong? ThreadAffinity_P2PIO;

            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong? ThreadAffinity_HTTPRequestIO;

            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong? ThreadAffinity_RTCIO;

            static OverrideableConfigValues()
            {
                RegisterFactory(() => new OverrideableConfigValues());
            }

            internal OverrideableConfigValues() : base("EpicOnlineServicesConfig.json") { }
        }

        private TK SelectValue<TK>(TK overrideValuesFromFieldMember, TK mainConfigValue)
        {
            // If the value in the overrides is not default, then it takes
            // precedent
            return !overrideValuesFromFieldMember.Equals(default) ? overrideValuesFromFieldMember : mainConfigValue;
        }

        private void MigrateButtonDelays(EOSConfig overrideValuesFromFieldMember, OverrideableConfigValues mainOverrideableConfig)
        {
            // Import the values for initial button delay and repeat button
            // delay
            initialButtonDelayForOverlay = SelectValue(
                overrideValuesFromFieldMember.initialButtonDelayForOverlay ?? 0,
                mainOverrideableConfig.initialButtonDelayForOverlay ?? 0);

            repeatButtonDelayForOverlay = SelectValue(
                overrideValuesFromFieldMember.repeatButtonDelayForOverlay ?? 0,
                mainOverrideableConfig.repeatButtonDelayForOverlay ?? 0);
        }

        private void MigrateThreadAffinity(EOSConfig overrideValuesFromFieldMember, OverrideableConfigValues mainOverrideableConfig)
        {
            // Import the values for thread initialization
            threadAffinity.NetworkWork = SelectValue(
                overrideValuesFromFieldMember.ThreadAffinity_networkWork,
                mainOverrideableConfig.ThreadAffinity_networkWork) ?? 0;

            threadAffinity.StorageIo = SelectValue(
                overrideValuesFromFieldMember.ThreadAffinity_storageIO,
                mainOverrideableConfig.ThreadAffinity_storageIO) ?? 0;

            threadAffinity.WebSocketIo = SelectValue(
                overrideValuesFromFieldMember.ThreadAffinity_webSocketIO,
                mainOverrideableConfig.ThreadAffinity_webSocketIO) ?? 0;

            threadAffinity.P2PIo = SelectValue(
                overrideValuesFromFieldMember.ThreadAffinity_P2PIO,
                mainOverrideableConfig.ThreadAffinity_P2PIO) ?? 0;

            threadAffinity.HttpRequestIo = SelectValue(
                overrideValuesFromFieldMember.ThreadAffinity_HTTPRequestIO,
                mainOverrideableConfig.ThreadAffinity_HTTPRequestIO) ?? 0;

            threadAffinity.RTCIo = SelectValue(
                overrideValuesFromFieldMember.ThreadAffinity_RTCIO,
                mainOverrideableConfig.ThreadAffinity_RTCIO) ?? 0;
        }

        private void MigrateOverrideableConfigValues(EOSConfig overrideValuesFromFieldMember,
            OverrideableConfigValues mainOverrideableConfig)
        {
            // Import the values for platform option flags.
            platformOptionsFlags |= overrideValuesFromFieldMember.platformOptionsFlags;

            MigrateButtonDelays(overrideValuesFromFieldMember, mainOverrideableConfig);
            MigrateThreadAffinity(overrideValuesFromFieldMember, mainOverrideableConfig);
        }

        protected virtual void MigrateNonOverrideableConfigValues(NonOverrideableConfigValues mainNonOverrideableConfig)
        {
            authScopeOptionsFlags = mainNonOverrideableConfig.authScopeOptionsFlags;
            tickBudgetInMilliseconds = mainNonOverrideableConfig.tickBudgetInMilliseconds;
            taskNetworkTimeoutSeconds = mainNonOverrideableConfig.taskNetworkTimeoutSeconds;
            alwaysSendInputToOverlay = mainNonOverrideableConfig.alwaysSendInputToOverlay;
        }

        protected virtual void MigratePlatformFlags(EOSConfig overrideValuesFromFieldMember,
            NonOverrideableConfigValues mainNonOverrideableConfig)
        {
            // The best (perhaps only) way to meaningfully migrate values from
            // the overrideValues field member and the main config is to combine
            // them, then filter them to exclude any platform flags that are 
            // incompatible with the platform for this Config. THIS is the 
            // primary reason it is necessary to warn the user and ask them to 
            // double check the values after migration.
            WrappedPlatformFlags combinedPlatformFlags =
                overrideValuesFromFieldMember.platformOptionsFlags | mainNonOverrideableConfig.platformOptionsFlags;

            WrappedPlatformFlags migratedPlatformFlags = WrappedPlatformFlags.None;
            foreach (WrappedPlatformFlags flag in EnumUtility<WrappedPlatformFlags>.GetEnumerator(combinedPlatformFlags))
            {
                switch (flag)
                {
                    case WrappedPlatformFlags.None:
                    case WrappedPlatformFlags.LoadingInEditor:
                    case WrappedPlatformFlags.DisableOverlay:
                    case WrappedPlatformFlags.DisableSocialOverlay:
                    case WrappedPlatformFlags.Reserved1:
                        migratedPlatformFlags |= flag;
                        break;
                    case WrappedPlatformFlags.WindowsEnableOverlayD3D9:
                    case WrappedPlatformFlags.WindowsEnableOverlayD3D10:
                    case WrappedPlatformFlags.WindowsEnableOverlayOpengl:
                        if (Platform == PlatformManager.Platform.Windows)
                        {
                            migratedPlatformFlags |= flag;
                        }
                        break;
                    case WrappedPlatformFlags.ConsoleEnableOverlayAutomaticUnloading:
                        if (Platform == PlatformManager.Platform.Console)
                        {
                            migratedPlatformFlags |= flag;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            platformOptionsFlags = migratedPlatformFlags;
        }


        protected override void MigrateConfig()
        {
            // The following code takes any values that used to exist within the
            // overrideValues field member, and places them into the
            // appropriate new field members. It also takes values from the 
            // main EOSConfig - if the values are not defined in the
            // overrideValues.

            // Do nothing if the values have already been moved, or if
            // overrideValues is null.
#pragma warning disable CS0612 // Type or member is obsolete
            if (_configValuesMigrated || null == overrideValues)
            {
                return;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            ProductConfig productConfig = Get<ProductConfig>();

            foreach (Named<Deployment> dep in productConfig.Environments.Deployments)
            {

            }

            // This config represents the set of values that previously were 
            // overrideable from the editor window. These values should take
            // priority over the main config if they are not default values.
            OverrideableConfigValues mainOverrideableConfigValues = Get<OverrideableConfigValues>();
#pragma warning disable CS0612 // Type or member is obsolete
            MigrateOverrideableConfigValues(overrideValues, mainOverrideableConfigValues);
#pragma warning restore CS0612 // Type or member is obsolete

            // This config represents the set of values that were not
            // overrideable from the editor window. The migrated values should
            // favor these set of values.
            NonOverrideableConfigValues mainNonOverrideableConfigValuesThatCouldNotBeOverridden = Get<NonOverrideableConfigValues>();
            MigrateNonOverrideableConfigValues(mainNonOverrideableConfigValuesThatCouldNotBeOverridden);

            // Notify the user of the migration, encourage them to double check
            // that migration was successful.
            Debug.LogWarning("Configuration values have been " +
                             "migrated. Please double check your " +
                             "configuration in EOS Plugin -> EOS " +
                             "Configuration to make sure that the migration " +
                             "was successful.");

            // Mark that the values have been imported
            _configValuesMigrated = true;

            // Write the imported values
            Write();
        }

        #endregion
    }
}
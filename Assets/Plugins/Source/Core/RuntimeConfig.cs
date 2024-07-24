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
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#define EOS_RUNTIME_NEW_CONFIG_SYSTEM

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.IntegratedPlatform;
    using Epic.OnlineServices.Platform;
    using System;
    using UnityEditor.PackageManager;

#if EOS_RUNTIME_NEW_CONFIG_SYSTEM
    /// <summary>
    /// Contains the information necessary to configure the EOS SDK to connect
    /// properly to Epic Online Services. Is ignorant of platform, because it
    /// _only_ represents the _current_ runtime values to use. The values of the
    /// field members defined within will need to be determined at runtime.
    ///
    /// _Project Settings_ is the area where there will be options to define
    /// configuration values on a platform by platform basis. The EOS Plugin
    /// allows users who are making their game with EOS to define different
    /// properties for each platform, and to store sandbox / deployment
    /// overrides so that it is easy to switch between deployment environments
    /// during development. Those settings are distinct from these set of
    /// values.
    ///
    /// Most of the values for these field members come from the
    /// [Development Portal](https://dev.epicgames.com/portal/)
    /// </summary>
    public struct RuntimeConfig
    {

        /*
         * The following region contains values that are required in order to
         * use the EOS SDK.
         */
        #region EOS SDK Configuration Values

        /// <summary>
        /// Product Name defined in the
        /// 
        /// </summary>
        public readonly string ProductName;

        /// <summary>
        /// Version of Product.
        /// </summary>
        public readonly Version ProductVersion;

        /// <summary>
        /// Product Id
        /// </summary>
        public readonly Guid ProductId;

        /// <summary>
        /// Sandbox Id
        /// </summary>
        public readonly Guid SandboxId;

        /// <summary>
        /// Deployment Id
        /// </summary>
        public readonly Guid DeploymentId;

        /// <summary>
        /// Stores the ClientSecret and ClientId
        /// </summary>
        public readonly ClientCredentials ClientCredentials;

        /// <summary>
        /// Encryption Key&lt; used by default to decode files previously
        /// encoded and stored in EOS.
        /// </summary>
        public readonly string EncryptionKey;

        /// <summary>
        /// Flags; used to initialize the EOS platform.
        /// </summary>
        public readonly PlatformFlags PlatformFlags;

        /// <summary>
        /// Flags; used to set user auth when logging in.
        /// </summary>
        public readonly AuthScopeFlags AuthScopeFlags;

        /// <summary>
        /// Flags for options related to integrated platform management.
        /// </summary>
        public readonly IntegratedPlatformManagementFlags IntegratedPlatformManagementFlags;

        /// <summary>
        /// Tick Budget; used to define the maximum amount of execution time the
        /// EOS SDK can use each frame.
        /// </summary>
        public readonly uint TickBudgetInMilliseconds;

        /// <summary>
        /// When the EOS SDK initializes threads for usage, the affinity for
        /// each thread is set based on the value of this struct.
        /// </summary>
        public readonly InitializeThreadAffinity ThreadAffinity;

        /// <summary>
        /// Determines whether or not the application is running as a server.
        /// </summary>
        public readonly bool IsServer;

        #endregion

        /*
         * The following region contains configuration values that pertain to
         * the EOS Plugin, distinct from configuration values that pertain more
         * specifically to the function of the EOS SDK.
         */
        #region Plugin Configuration Values

        /// <summary>
        /// Always Send Input to Overlay &lt;/c&gt;If true, the plugin will
        /// always send input to the overlay from the C# side to native, and
        /// handle showing the overlay. This doesn't always mean input makes
        /// it to the EOS SDK.
        /// </summary>
        public readonly bool AlwaysSendInputToOverlay;

        /// <summary>
        /// Initial Button Delay.
        /// </summary>
        public readonly float InitialButtonDelayForOverlay;

        /// <summary>
        /// Repeat button delay for overlay
        /// </summary>
        public readonly float RepeatButtonDelayForOverlay;

        /// <summary>
        /// Force send input without delay&lt;/c&gt;If true, the
        /// native plugin will always send input received directly to the SDK.
        /// If set to false, the plugin will attempt to delay the input to
        /// mitigate CPU spikes caused by spamming the SDK.
        /// </summary>
        public readonly bool SendInputDirectlyToSDK;

        #endregion

        public RuntimeConfig(
            Guid productId, 
            string productName, 
            Version productVersion, 
            Guid sandboxId,
            Guid deploymentId,
            ClientCredentials clientCredentials,
            string encryptionKey,
            PlatformFlags platformFlags,
            AuthScopeFlags authScopeFlags,
            IntegratedPlatformManagementFlags integratedPlatformManagementFlags,
            uint tickBudgetInMilliseconds,
            InitializeThreadAffinity threadAffinity,
            bool isServer,
            bool alwaysSendInputToOverlay,
            float initialButtonDelayForOverlay,
            float repeatButtonDelayForOverlay,
            bool sendInputDirectlyToSdk)
        {
            ProductId = productId;
            ProductName = productName;
            ProductVersion = productVersion;
            SandboxId = sandboxId;
            DeploymentId = deploymentId;
            ClientCredentials = clientCredentials;
            EncryptionKey = encryptionKey;
            PlatformFlags = platformFlags;
            AuthScopeFlags = authScopeFlags;
            IntegratedPlatformManagementFlags = integratedPlatformManagementFlags;
            TickBudgetInMilliseconds = tickBudgetInMilliseconds;
            ThreadAffinity = threadAffinity;
            IsServer = isServer;
            AlwaysSendInputToOverlay = alwaysSendInputToOverlay;
            InitialButtonDelayForOverlay = initialButtonDelayForOverlay;
            RepeatButtonDelayForOverlay = repeatButtonDelayForOverlay;
            SendInputDirectlyToSDK = sendInputDirectlyToSdk;
        }
    }
#endif
}
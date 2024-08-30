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

//#define EOS_RUNTIME_NEW_CONFIG_SYSTEM

namespace PlayEveryWare.EpicOnlineServices
{
#if !EOS_DISABLE
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Platform;
    using System;

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
    public readonly struct RuntimeConfig
    {
    #region EOS SDK Configuration Values

        /*
         * The following region contains values that are required in order to
         * use the EOS SDK.
         */

        /// <summary>
        /// Product Name defined in the dev portal.
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
        /// Sandbox Id. Non-private sandboxes are Guid. Private sandboxes
        /// have the prefix "p-", and are not otherwise Guids.
        /// </summary>
        public readonly string SandboxId;

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

        public RuntimeConfig(
            Guid productId,
            string productName,
            Version productVersion,
            string sandboxId,
            Guid deploymentId,
            ClientCredentials clientCredentials,
            string encryptionKey,
            PlatformFlags platformFlags,
            AuthScopeFlags authScopeFlags,
            uint tickBudgetInMilliseconds,
            InitializeThreadAffinity threadAffinity,
            bool isServer)
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
            TickBudgetInMilliseconds = tickBudgetInMilliseconds;
            ThreadAffinity = threadAffinity;
            IsServer = isServer;
        }

        /// <summary>
        /// Uses the values in the given EOSConfig class to create a
        /// RuntimeConfig struct that contains readonly values with the
        /// appropriate types.
        ///
        /// The result of using this for setting EOS SDK config values is a sort
        /// of data validation, in that if the corresponding string values are
        /// not parseable into the expected data type. Future work will be done
        /// to expose this validation in an appropriately user-friendly manner.
        ///
        /// The result of this conversion should be used when assigning the
        /// values to structures that aid in the creation and initialization of
        /// the EOS SDK.
        /// </summary>
        /// <param name="config">
        /// The EOSConfig to convert into RuntimeConfig.
        /// </param>
        public static implicit operator RuntimeConfig(EOSConfig config)
        {
            if (null == config)
            {
                throw new ArgumentNullException($"Cannot convert from a null \"EOSConfig\" to \"RuntimeConfig\".");
            }

            // Use the configure override thread affinity helper function to
            // set the thread affinity parameter for the RuntimeConfig object.
            InitializeThreadAffinity threadAffinity = new();
            config.ConfigureOverrideThreadAffinity(ref threadAffinity);

            return new RuntimeConfig(
                Guid.Parse(config.productID),
                config.productName,
                Version.Parse(config.productVersion),
                config.sandboxID,
                Guid.Parse(config.deploymentID),
                new ClientCredentials() { ClientId = config.clientID, ClientSecret = config.clientSecret },
                config.encryptionKey,
                config.GetPlatformFlags(),
                config.GetAuthScopeFlags(),
                config.tickBudgetInMilliseconds,
                threadAffinity,
                config.isServer);
        }
    }
#endif
#endif
}
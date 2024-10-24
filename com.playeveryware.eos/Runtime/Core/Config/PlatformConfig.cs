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
#if !EOS_DISABLE
    using Epic.OnlineServices.IntegratedPlatform;
    using Utility;
#endif
    using Extensions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a set of configuration data for use by the EOS Plugin for
    /// Unity on a specific platform.
    /// </summary>
    [Serializable]
    public abstract class PlatformConfig : Config
    {
        /// <summary>
        /// The platform that the set of configuration data is to be applied on.
        /// </summary>
        public PlatformManager.Platform Platform { get; }

        /// <summary>
        /// Any overriding values that should replace the central EOSConfig. On
        /// a platform-by-platform basis values can be overridden by setting
        /// these values. At runtime, these values will replace the ones on the
        /// central/main EOSConfig. This behavior is currently incomplete in
        /// it's implementation, but the intent is documented here for the sake
        /// of clarity and context.
        /// </summary>
        public EOSConfig overrideValues;

        /// <summary>
        /// Used to store integrated platform management flags.
        /// </summary>
        public List<string> flags;

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

        /// <summary>
        /// Returns a single IntegratedPlatformManagementFlags enum value that
        /// results from a bitwise OR operation of all the
        /// integratedPlatformManagementFlags flags on this config.
        /// </summary>
        /// <returns>An IntegratedPlatformManagementFlags enum value.</returns>
#if !EOS_DISABLE
        public IntegratedPlatformManagementFlags GetIntegratedPlatformManagementFlags()
        {
            _ = IntegratedPlatformManagementFlagsExtensions.TryParse(flags,
                out IntegratedPlatformManagementFlags flagsEnum);

            return flagsEnum;
        }
#endif
    }
}
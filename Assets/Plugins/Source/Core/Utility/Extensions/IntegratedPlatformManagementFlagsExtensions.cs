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

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices.Extensions
{
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.IntegratedPlatform;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a means of parsing string representations of enum values for
    /// the enum type AuthScopeFlags.
    /// </summary>
    public static class IntegratedPlatformManagementFlagsExtensions
    {
        /// <summary>
        /// An alternate set of string values that can be parsed into
        /// IntegratedPlatformManagementFlagsExtensions enum values.
        /// </summary>
        private static readonly Dictionary<string, IntegratedPlatformManagementFlags> s_customMappings = new()
        {
            {"EOS_IPMF_Disabled",                        IntegratedPlatformManagementFlags.Disabled },

            {"EOS_IPMF_LibraryManagedByApplication",     IntegratedPlatformManagementFlags.LibraryManagedByApplication },
            {"ManagedByApplication",                     IntegratedPlatformManagementFlags.LibraryManagedByApplication},

            {"ManagedBySDK",                             IntegratedPlatformManagementFlags.LibraryManagedBySDK },
            {"EOS_IPMF_ManagedBySDK",                    IntegratedPlatformManagementFlags.LibraryManagedBySDK },
            {"EOS_IPMF_LibraryManagedBySDK",             IntegratedPlatformManagementFlags.LibraryManagedBySDK },

            {"DisableSharedPresence",                    IntegratedPlatformManagementFlags.DisablePresenceMirroring },
            {"EOS_IPMF_DisableSharedPresence",           IntegratedPlatformManagementFlags.DisablePresenceMirroring },
            {"EOS_IPMF_DisablePresenceMirroring",        IntegratedPlatformManagementFlags.DisablePresenceMirroring },

            {"DisableSessions",                          IntegratedPlatformManagementFlags.DisableSDKManagedSessions },
            {"EOS_IPMF_DisableSessions",                 IntegratedPlatformManagementFlags.DisableSDKManagedSessions },
            {"EOS_IPMF_DisableSDKManagedSessions",       IntegratedPlatformManagementFlags.DisableSDKManagedSessions },

            {"PreferEOS",                                IntegratedPlatformManagementFlags.PreferEOSIdentity },
            {"EOS_IPMF_PreferEOS",                       IntegratedPlatformManagementFlags.PreferEOSIdentity },
            {"EOS_IPMF_PreferEOSIdentity",               IntegratedPlatformManagementFlags.PreferEOSIdentity },

            {"PreferIntegrated",                         IntegratedPlatformManagementFlags.PreferIntegratedIdentity },
            {"EOS_IPMF_PreferIntegrated",                IntegratedPlatformManagementFlags.PreferIntegratedIdentity },
            {"EOS_IPMF_PreferIntegratedIdentity",        IntegratedPlatformManagementFlags.PreferIntegratedIdentity },

            {"EOS_IPMF_ApplicationManagedIdentityLogin", IntegratedPlatformManagementFlags.ApplicationManagedIdentityLogin },
        };

        /// <summary>
        /// Tries to parse the given string into an enum value.
        /// </summary>
        /// <param name="str">The string to parse into an enum value.</param>
        /// <param name="flags">
        /// The enum value resulting from the parse operation.
        /// </param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryParse(string str, out IntegratedPlatformManagementFlags flags)
        {
            return EnumUtility<IntegratedPlatformManagementFlags>.TryParse(str, s_customMappings, out flags);
        }

        public static bool TryParse(IList<string> stringFlags, out IntegratedPlatformManagementFlags flags)
        {
            return EnumUtility<IntegratedPlatformManagementFlags>.TryParse(stringFlags, s_customMappings, out flags);
        }
    }
}

#endif
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
    using Epic.OnlineServices.IntegratedPlatform;
    using System;
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
        public static Dictionary<string, IntegratedPlatformManagementFlags> CustomMappings { get; } = new()
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

        public static string GetDescription(this IntegratedPlatformManagementFlags flags)
        {
            return flags switch
            {
                IntegratedPlatformManagementFlags.Disabled => "The integrated platform library should be disabled. This is equivalent to providing no flags.",
                IntegratedPlatformManagementFlags.LibraryManagedByApplication => "The integrated platform library is managed by the calling application. EOS SDK should only hook into an existing instance of the integrated platform library.",
                IntegratedPlatformManagementFlags.LibraryManagedBySDK => "EOS SDK should fully manage the integrated platform library. It will do this by performing the load, initialize, tick and unload operations as necessary.",
                IntegratedPlatformManagementFlags.DisablePresenceMirroring => "The EOS SDK should not mirror the EOS rich presence with the Integrated Platform.\nThe default behavior is for EOS SDK to share local presence with the Integrated Platform.",
                IntegratedPlatformManagementFlags.DisableSDKManagedSessions => "EOS SDK should not perform any sessions management through the Integrated Platform.\nThe default behavior is for EOS SDK to perform sessions management through the Integrated Platform.\nSessions management includes:\n - sharing the lobby and session presence enabled games with the Integrated Platform.\n - handling Social Overlay join button events which cannot be handled by normal processing of Epic Services.\n - handling Social Overlay invite button events which cannot be handled by normal processing of Epic Services.\n - handling startup requests from the Integrated Platform to immediately join a game due to in invite while offline.",
                IntegratedPlatformManagementFlags.PreferEOSIdentity => "Some features within the EOS SDK may wish to know a preference of Integrated Platform versus EOS.\nWhen determining an absolute platform preference those with this flag will be skipped.\nThe IntegratedPlatforms list is provided via the Platform.Options during Platform.PlatformInterface.Create.\n\nThe primary usage of the PreferEOSIdentity and PreferIntegratedIdentity flags is with game invites from the Social Overlay.\n\nFor game invites from the Social Overlay the EOS SDK will follow these rules:\n - If the only account ID we can determine for the target player is an EAS ID then the EOS system will be used.\n - If the only account ID we can determine for the target player is an integrated platform ID then the integrated platform system will be used.\n - If both are available then the EOS SDK will operate in 1 of 3 modes:\n - no preference identified: use both the EOS and integrated platform systems.\n - PreferEOS: Use EOS if the target is an EAS friend and is either online in EAS or not online for the integrated platform.\n - PreferIntegrated: Use integrated platform if the target is an integrated platform friend and is either online in the integrated platform or not online for EAS.\n - If the integrated platform fails to send then try EAS if was not already used.",
                IntegratedPlatformManagementFlags.PreferIntegratedIdentity => "Some features within the EOS SDK may wish to know a preference of Integrated Platform versus EOS.\nFor further explanation see PreferEOSIdentity.",
                IntegratedPlatformManagementFlags.ApplicationManagedIdentityLogin => "By default the EOS SDK will attempt to detect the login/logout events of local users and update local states accordingly. Setting this flag will disable this functionality, relying on the application to process login/logout events and notify EOS SDK. It is not possible for the EOS SDK to do this on all platforms, making this flag not always optional.\n\nThis flag must be set to use the manual platform user login/logout functions, even on platforms where it is not possible for the EOS SDK to detect login/logout events, making this a required flag for correct Integrated Platform behavior on those platforms.",
                _ => throw new ArgumentOutOfRangeException(nameof(flags), flags, null),
            };
        }

        /// <summary>
        /// Tries to parse the given list of strings representing individual
        /// IntegratedPlatformManagementFlags enum values, and performing a
        /// bitwise OR operation on those values.
        /// </summary>
        /// <param name="stringFlags">
        /// List of strings representing individual
        /// IntegratedPlatformManagementFlags enum values.
        /// </param>
        /// <param name="flags">
        /// The result of performing a bitwise OR operation on the values that
        /// are represented by the list of string values.
        /// </param>
        /// <returns>
        /// True if all the list of strings was successfully parsed, and the
        /// resulting list of enum values was bitwise ORed together.
        /// </returns>
        public static bool TryParse(IList<string> stringFlags, out IntegratedPlatformManagementFlags flags)
        {
            return EnumUtility<IntegratedPlatformManagementFlags>.TryParse(stringFlags, CustomMappings, out flags);
        }
    }
}

#endif
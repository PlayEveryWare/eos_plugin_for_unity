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
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Auth;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace PlayEveryWare.EpicOnlineServices
{
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
    public class EOSConfig : ICloneableGeneric<EOSConfig>, IEmpty
    {
        /// <value><c>Product Name</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string productName;

        /// <value>Version of Product</value>
        public string productVersion;

        /// <value><c>Product Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string productID;

        /// <value><c>Sandbox Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string sandboxID;

        /// <value><c>Deployment Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string deploymentID;

        /// <value><c>SandboxDeploymentOverride</c> pairs used to override Deployment ID when a given Sandbox ID is used</value>
        public List<SandboxDeploymentOverride> sandboxDeploymentOverrides;

        /// <value><c>Client Secret</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string clientSecret;

        /// <value><c>Client Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string clientID;

        /// <value><c>Encryption Key</c> used by default to decode files previously encoded and stored in EOS</value>
        public string encryptionKey;

        /// <value><c>Flags</c> used to initilize the EOS platform.</value>
        public List<string> platformOptionsFlags;

        /// <value><c>Flags</c> used to set user auth when logging in.</value>
        public List<string> authScopeOptionsFlags;

        /// <value><c>Tick Budget</c> used to define the maximum amount of execution time the EOS SDK can use each frame.</value>
        public uint tickBudgetInMilliseconds;

        /// <value><c>Network Work Affinity</c> specifies thread affinity for network management that is not IO.</value>
        public string ThreadAffinity_networkWork;
        /// <value><c>Storage IO Affinity</c> specifies affinity for threads that will interact with a storage device.</value>
        public string ThreadAffinity_storageIO;
        /// <value><c>Web Socket IO Affinity</c> specifies affinity for threads that generate web socket IO.</value>
        public string ThreadAffinity_webSocketIO;
        /// <value><c>P2P IO Affinity</c> specifies affinity for any thread that will generate IO related to P2P traffic and management.</value>
        public string ThreadAffinity_P2PIO;
        /// <value><c>HTTP Request IO Affinity</c> specifies affinity for any thread that will generate http request IO.</value>
        public string ThreadAffinity_HTTPRequestIO;
        /// <value><c>RTC IO Affinity</c> specifies affinity for any thread that will generate IO related to RTC traffic and management.</value>
        public string ThreadAffinity_RTCIO;


        /// <value><c>Always Send Input to Overlay </c>If true, the plugin will always send input to the overlay from the C# side to native, and handle showing the overlay. This doesn't always mean input makes it to the EOS SDK</value>
        public bool alwaysSendInputToOverlay;

        /// <value><c>Initial Button Delay</c> Stored as a string so it can be 'empty'</value>
        public string initialButtonDelayForOverlay;

        /// <value><c>Repeat button delay for overlay</c> Stored as a string so it can be 'empty' </value>
        public string repeatButtonDelayForOverlay;

        /// <value><c>HACK: send force send input without delay</c>If true, the native plugin will always send input received directly to the SDK. If set to false, the plugin will attempt to delay the input to mitigate CPU spikes caused by spamming the SDK </value>
        public bool hackForceSendInputDirectlyToSDK;



        public static Regex InvalidEncryptionKeyRegex;
        static EOSConfig()
        {
            InvalidEncryptionKeyRegex = new Regex("[^0-9a-fA-F]");
        }

        public static bool IsEncryptionKeyValid(string key)
        {
            return
                //key not null
                key != null &&
                //key is 64 characters
                key.Length == 64 &&
                //key is all hex characters
                !InvalidEncryptionKeyRegex.Match(key).Success;
        }

        //-------------------------------------------------------------------------
        //TODO: Move this to a shared place
        public static bool StringIsEqualToAny(string flagAsCString, params string[] parameters)
        {
            foreach(string s in parameters)
            {
                if (flagAsCString == s)
                {
                    return true;
                }
            }
            return false;
        }

        public static T EnumCast<T, V>(V value)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

#if !EOS_DISABLE
        //-------------------------------------------------------------------------
        public static Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags flagsAsIntegratedPlatformManagementFlags(List<string> flags)
        {
            int toReturn = 0;
 
            foreach (var flagAsCString in flags)
            {
                if (StringIsEqualToAny(flagAsCString, "EOS_IPMF_Disabled", "Disabled"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.Disabled;
                }
                else if (StringIsEqualToAny(flagAsCString, "EOS_IPMF_ManagedByApplication", "ManagedByApplication", "EOS_IPMF_LibraryManagedByApplication"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.LibraryManagedByApplication;
                }
                else if (StringIsEqualToAny(flagAsCString,"EOS_IPMF_ManagedBySDK", "ManagedBySDK", "EOS_IPMF_LibraryManagedBySDK"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.LibraryManagedBySDK;
                }
                else if (StringIsEqualToAny(flagAsCString, "EOS_IPMF_DisableSharedPresence", "DisableSharedPresence", "EOS_IPMF_DisablePresenceMirroring"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.DisablePresenceMirroring;
                }
                else if (StringIsEqualToAny(flagAsCString, "EOS_IPMF_DisableSessions", "DisableSessions", "EOS_IPMF_DisableSDKManagedSessions"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.DisableSDKManagedSessions;
                }
                else if (StringIsEqualToAny(flagAsCString, "EOS_IPMF_PreferEOS", "PreferEOS", "EOS_IPMF_PreferEOSIdentity"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.PreferEOSIdentity;
                }
                else if (StringIsEqualToAny(flagAsCString, "EOS_IPMF_PreferIntegrated", "PreferIntegrated", "EOS_IPMF_PreferIntegratedIdentity"))
                {
                    toReturn |= (int)Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags.PreferIntegratedIdentity;
                }
            }

            return EnumCast<Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags, int>(toReturn);
        }

        //-------------------------------------------------------------------------
        public static PlatformFlags platformOptionsFlagsAsPlatformFlags(List<string> platformOptionsFlags)
        {
            PlatformFlags toReturn = PlatformFlags.None;

            foreach(var flagAsString in platformOptionsFlags)
            {
                if(flagAsString == "LoadingInEditor" || flagAsString == "EOS_PF_LOADING_IN_EDITOR")
                {
                    toReturn |= PlatformFlags.LoadingInEditor;
                }

                else if(flagAsString == "DisableOverlay" || flagAsString == "EOS_PF_DISABLE_OVERLAY")
                {
                    toReturn |= PlatformFlags.DisableOverlay;
                }

                else if(flagAsString == "DisableSocialOverlay" || flagAsString == "EOS_PF_DISABLE_SOCIAL_OVERLAY")
                {
                    toReturn |= PlatformFlags.DisableSocialOverlay;
                }

                else if(flagAsString == "Reserved1" || flagAsString == "EOS_PF_RESERVED1")
                {
                    toReturn |= PlatformFlags.Reserved1;
                }

                else if(flagAsString == "WindowsEnabledOverlayD3D9" || flagAsString == "EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9")
                {
                    toReturn |= PlatformFlags.WindowsEnableOverlayD3D9;
                }
                else if(flagAsString == "WindowsEnabledOverlayD3D10" || flagAsString == "EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10")
                {
                    toReturn |= PlatformFlags.WindowsEnableOverlayD3D10;
                }
                else if(flagAsString == "WindowsEnabledOverlayOpengl" || flagAsString == "EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL")
                {
                    toReturn |= PlatformFlags.WindowsEnableOverlayOpengl;
                }
            }

            return toReturn;
        }

        //-------------------------------------------------------------------------
        public PlatformFlags platformOptionsFlagsAsPlatformFlags()
        {
            return EOSConfig.platformOptionsFlagsAsPlatformFlags(platformOptionsFlags);
        }

        //-------------------------------------------------------------------------
        public static AuthScopeFlags authScopeOptionsFlagsAsAuthScopeFlags(List<string> authScopeOptionsFlags)
        {
            AuthScopeFlags toReturn = AuthScopeFlags.NoFlags;

            foreach (var flagAsString in authScopeOptionsFlags)
            {
                if (flagAsString == "NoFlags" || flagAsString == "EOS_AS_NoFlags")
                {
                }
                else if (flagAsString == "BasicProfile" || flagAsString == "EOS_AS_BasicProfile")
                {
                    toReturn |= AuthScopeFlags.BasicProfile;
                }
                else if (flagAsString == "FriendsList" || flagAsString == "EOS_AS_FriendsList")
                {
                    toReturn |= AuthScopeFlags.FriendsList;
                }
                else if (flagAsString == "Presence" || flagAsString == "EOS_AS_Presence")
                {
                    toReturn |= AuthScopeFlags.Presence;
                }
                else if (flagAsString == "FriendsManagement" || flagAsString == "EOS_AS_FriendsManagement")
                {
                    toReturn |= AuthScopeFlags.FriendsManagement;
                }
                else if (flagAsString == "Email" || flagAsString == "EOS_AS_Email")
                {
                    toReturn |= AuthScopeFlags.Email;
                }
                else if (flagAsString == "Country" || flagAsString == "EOS_AS_Country")
                {
                    toReturn |= AuthScopeFlags.Country;
                }
            }

            return toReturn;
        }

        //-------------------------------------------------------------------------
        public AuthScopeFlags authScopeOptionsFlagsAsAuthScopeFlags()
        {
            return EOSConfig.authScopeOptionsFlagsAsAuthScopeFlags(authScopeOptionsFlags);
        }
#endif

        //-------------------------------------------------------------------------
        /// <summary>
        /// Creates a shallow copy of the current <c>EOSConfig</c>
        /// </summary>
        /// <returns>Shallow copy of <c>EOSConfig</c></returns>
        public EOSConfig Clone()
        {
            return (EOSConfig)this.MemberwiseClone();
        }

        //-------------------------------------------------------------------------
        public bool IsEmpty()
        {
            return EmptyPredicates.IsEmptyOrNull(productName)
                && EmptyPredicates.IsEmptyOrNull(productVersion)
                && EmptyPredicates.IsEmptyOrNull(productID)
                && EmptyPredicates.IsEmptyOrNull(sandboxID)
                && EmptyPredicates.IsEmptyOrNull(deploymentID)
                && (sandboxDeploymentOverrides == null || sandboxDeploymentOverrides.Count == 0)
                && EmptyPredicates.IsEmptyOrNull(clientSecret)
                && EmptyPredicates.IsEmptyOrNull(clientID)
                && EmptyPredicates.IsEmptyOrNull(encryptionKey)
                && EmptyPredicates.IsEmptyOrNull(platformOptionsFlags)
                && EmptyPredicates.IsEmptyOrNull(authScopeOptionsFlags)
                && EmptyPredicates.IsEmptyOrNull(repeatButtonDelayForOverlay)
                && EmptyPredicates.IsEmptyOrNull(initialButtonDelayForOverlay)
                ;
        }

        //-------------------------------------------------------------------------
        public float GetInitialButtonDelayForOverlayAsFloat()
        {
            return float.Parse(initialButtonDelayForOverlay);
        }

        //-------------------------------------------------------------------------
        public void SetInitialButtonDelayForOverlayFromFloat(float f)
        {
            initialButtonDelayForOverlay = f.ToString();
        }

        //-------------------------------------------------------------------------
        public float GetRepeatButtonDelayForOverlayAsFloat()
        {
           return float.Parse(repeatButtonDelayForOverlay);
        }

        //-------------------------------------------------------------------------
        public void SetRepeatButtonDelayForOverlayFromFloat(float f)
        {
            repeatButtonDelayForOverlay = f.ToString();
        }

        //-------------------------------------------------------------------------
        public ulong GetThreadAffinityNetworkWork(ulong defaultValue = 0)
        {
            ulong value;
            if (!string.IsNullOrEmpty(ThreadAffinity_networkWork))
            {
                value = ulong.Parse(ThreadAffinity_networkWork);
            }
            else
            {
                value = defaultValue;
            }

            return value;
        }

        //-------------------------------------------------------------------------
        public ulong GetThreadAffinityStorageIO(ulong defaultValue = 0)
        {
            ulong value;
            if (!string.IsNullOrEmpty(ThreadAffinity_storageIO))
            {
                value = ulong.Parse(ThreadAffinity_storageIO);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }
 
        //-------------------------------------------------------------------------
        public ulong GetThreadAffinityWebSocketIO(ulong defaultValue = 0)
        {
            ulong value;
            if (!string.IsNullOrEmpty(ThreadAffinity_webSocketIO))
            {
                value = ulong.Parse(ThreadAffinity_webSocketIO);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        //-------------------------------------------------------------------------
        public ulong GetThreadAffinityP2PIO(ulong defaultValue = 0)
        {
            ulong value;
            if (!string.IsNullOrEmpty(ThreadAffinity_P2PIO))
            {
                value = ulong.Parse(ThreadAffinity_P2PIO);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        //-------------------------------------------------------------------------
        public ulong GetThreadAffinityHTTPRequestIO(ulong defaultValue = 0)
        {
            ulong value;
            if (!string.IsNullOrEmpty(ThreadAffinity_HTTPRequestIO))
            {
                value = ulong.Parse(ThreadAffinity_HTTPRequestIO);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        //-------------------------------------------------------------------------
        public ulong GetThreadAffinityRTCIO(ulong defaultValue = 0)
        {
            ulong value;
            if (!string.IsNullOrEmpty(ThreadAffinity_RTCIO))
            {
                value = ulong.Parse(ThreadAffinity_RTCIO);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        //-------------------------------------------------------------------------
        public bool IsEncryptionKeyValid()
        {
            return IsEncryptionKeyValid(encryptionKey);
        }
    }
}

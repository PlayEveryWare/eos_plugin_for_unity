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

#if !STEAMWORKS_MODULE || !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using System.Collections.Generic;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    public class SteamworksUtility : MonoBehaviour
    {
        public static string GetSteamworksVersion()
        {
#if DISABLESTEAMWORKS
            return "Steamworks not imported or not supported on platform";
#else
            return Steamworks.Version.SteamworksSDKVersion;
#endif
        }

        /// <summary>
        /// This is to populate the SteamConfig's steamApiInterfaceVersionsArray. It needs to be configured exactly like this for Steamworks v1.58a onwards.
        /// This value is identical to steam_api.h's pszInternalCheckInterfaceVersions value.
        /// https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-integrated-platform-steam-options
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSteamInterfaceVersions()
        {
#if DISABLESTEAMWORKS
            return new List<string>();
#else
            return new List<string>()
            {
                 Steamworks.Constants.STEAMUTILS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMNETWORKINGUTILS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMAPPS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMFRIENDS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMGAMESEARCH_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMHTMLSURFACE_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMHTTP_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMINPUT_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMINVENTORY_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMMATCHMAKINGSERVERS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMMATCHMAKING_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMMUSICREMOTE_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMMUSIC_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMNETWORKINGMESSAGES_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMNETWORKINGSOCKETS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMNETWORKING_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMPARENTALSETTINGS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMPARTIES_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMREMOTEPLAY_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMREMOTESTORAGE_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMSCREENSHOTS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMUGC_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMUSERSTATS_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMUSER_INTERFACE_VERSION,
                 Steamworks.Constants.STEAMVIDEO_INTERFACE_VERSION
        };
#endif
        }
    }
}
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

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    public class Steamworks_Utility : MonoBehaviour
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
        /// https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-integrated-platform-steam-options
        /// </summary>
        /// <returns></returns>
        public static string GetPszInternalCheckInterfaceVersions()
        {
#if DISABLESTEAMWORKS
            return string.Empty;
#else
            return $"{Steamworks.Constants.STEAMUTILS_INTERFACE_VERSION} \0 {Steamworks.Constants.STEAMNETWORKINGUTILS_INTERFACE_VERSION} \0 {Steamworks.Constants.STEAMUSER_INTERFACE_VERSION} \0 {Steamworks.Constants.STEAMVIDEO_INTERFACE_VERSION} \0 \0";
#endif
        }
    }
}
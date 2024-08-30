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
    using System;
    using System.Collections.Generic;
    using Editor;
    using Editor.Utility;
    using Utility;

    [Serializable]
    [ConfigGroup("Steam Configuration")]
    // TODO: Make SteamConfig derive from EditorConfig, and update the native code
    //       to properly reference the correct file where appropriate.
    public class SteamConfig : EpicOnlineServices.Config
    {
        [ConfigField("Steam Flags", ConfigFieldType.TextList)]
        public List<string> flags;

        #region These fields are referenced by the native code 

        [DirectoryPathField("Override Library Path")]
        public string overrideLibraryPath;

        [ConfigField("Steamworks SDK Major Version", ConfigFieldType.Uint)]
        public uint steamSDKMajorVersion;

        [ConfigField("Steamworks SDK Minor Version", ConfigFieldType.Uint)]
        public uint steamSDKMinorVersion;

        [ConfigField("Steamworks Interface Versions", ConfigFieldType.TextList)]
        public List<string> steamApiInterfaceVersionsArray;

        #endregion

        [ButtonField("Update from Steamworks.NET")]
        public Action UpdateFromSteamworksNET;

        static SteamConfig()
        {
            RegisterFactory(() => new SteamConfig());
        }

        protected SteamConfig() : base("eos_steam_config.json")
        {
            UpdateFromSteamworksNET = () =>
            {
                string steamworksVersion = SteamworksUtility.GetSteamworksVersion();

                if (Version.TryParse(steamworksVersion, out Version version))
                {
                    _ = SafeTranslatorUtility.TryConvert(version.Major, out steamSDKMajorVersion);
                    _ = SafeTranslatorUtility.TryConvert(version.Minor, out steamSDKMinorVersion);
                }

                steamApiInterfaceVersionsArray = SteamworksUtility.GetSteamInterfaceVersions();
            };
        }


    }
}


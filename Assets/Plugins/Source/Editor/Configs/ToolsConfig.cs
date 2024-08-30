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

namespace PlayEveryWare.EpicOnlineServices.Editor.Config
{
    using System;
    
    /// <summary>
    /// Represents the EOS Plugin Configuration used for values that might 
    /// vary depending  on the environment that one is running Unity from.
    /// </summary>
    [Serializable]
    [ConfigGroup("Tools")]
    public class ToolsConfig : EditorConfig
    {
        /// <summary>
        /// The path to find the tool used for generating EAC certs.
        /// </summary>
        [FilePathField("Path to EAC Integrity Tool", "", "EOS SDK tool used to generate EAC certificate from file hashes.")]
        public string pathToEACIntegrityTool;

        [FilePathField("Path to EAC Integrity Tool Config", "cfg", "Config file used by integrity tool. Defaults to anticheat_integritytool.cfg in same directory.")]
        public string pathToEACIntegrityConfig;

        // TODO: Note that this field member is not used, it is here for backwards compatibility.
        public string pathToDefaultCertificate;

        [FilePathField("Path to EAC Private Key", "key", "EAC private key used in integrity tool cert generation. Exposing this to the public wil compromise anti-cheat functionality.")]
        public string pathToEACPrivateKey;

        [FilePathField("Path to EAC Certificate", "cer")]
        public string pathToEACCertificate;

        [FilePathField("Path to EAC Splash Image", "png")]
        public string pathToEACSplashImage;

        /// <summary>
        /// Optional override name for EOSBootstrapper.exe.
        /// </summary>
        [ConfigField("Bootstrapper Name Override", ConfigFieldType.Text, "Name to use instead of 'Bootstrapper.exe'")]
        public string bootstrapperNameOverride;

        /// <summary>
        /// If enabled, making a build will run the Easy Anti-Cheat integrity
        /// tool and copy EAC files to the build directory.
        /// </summary>
        [ConfigField("Use EAC", ConfigFieldType.Flag, "If set to true, integrates Easy AntiCheat.")]
        public bool useEAC;

        static ToolsConfig()
        {
            RegisterFactory(() => new ToolsConfig());
        }

        protected ToolsConfig() : base("eos_plugin_tools_config.json") { }
    }
}
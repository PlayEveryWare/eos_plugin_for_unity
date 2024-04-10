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
    public class ToolsConfig : EditorConfig
    {
        public ToolsConfig() : base("eos_plugin_tools_config.json") { }

        /// <value><c>Path To EAC integrity tool</c> The path to find the tool used for generating EAC certs</value>
        public string pathToEACIntegrityTool;
        public string pathToEACIntegrityConfig;
        public string pathToDefaultCertificate;
        public string pathToEACPrivateKey;
        public string pathToEACCertificate;
        public string pathToEACSplashImage;

        /// <value><c>Bootstrapper override name</c>Optional override name for EOSBootstrapper.exe</value>
        public string bootstrapperNameOverride;

        /// <value><c>Use EAC</c>If enabled, making a build will run the Easy Anti-Cheat integrity tool and copy EAC files to the build directory</value>
        public bool useEAC;
    }
}
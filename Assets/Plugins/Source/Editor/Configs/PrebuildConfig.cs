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

    [Serializable]
    public class PrebuildConfig : EditorConfig
    {
        /// <summary>
        /// Indicates that the application version should be set to the same as
        /// the value in
        /// the EOS Configuration for version.
        /// </summary>
        public bool useAppVersionAsProductVersion;

        /// <summary>
        /// If enabled, the EOSBootstrapper will be included in the build.
        /// If disabled, then the EOSBootstrapper will not be included or configured for the build.
        /// Use this if you only want to enable EAC, and not the rest of the features of the plugin.
        /// This is true by default. If this is set to false, many functions of the plugin, such as the overlay, will not be included in the build.
        /// </summary>
        public bool useEOSBootstrapper = true;

        static PrebuildConfig()
        {
            RegisterFactory(() => new PrebuildConfig());
        }

        protected PrebuildConfig() : base("eos_plugin_version_config.json") { }
    }
}
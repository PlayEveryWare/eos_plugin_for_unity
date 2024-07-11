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

using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices.Editor.Config
{
    using System;

    /// <summary>
    /// Contains configuration values pertinent to signing DLLs.
    /// </summary>
    [Serializable]
    [ConfigGroup("Code Signing")]
    public class SigningConfig : EditorConfig
    {
        /// <summary>
        /// Path to the tool used for signing the DLLs.
        /// </summary>
        [FilePathField("Path to SignTool", "exe", group:0)]
        public string pathToSignTool;

        /// <summary>
        /// Path to the PFX file used for signing.
        /// </summary>
        [FilePathField("Path to PFX Key", "pfx", group: 0)]
        public string pathToPFX;

        /// <summary>
        /// Password to the PFX file.
        /// </summary>
        [ConfigField("PFX Password", ConfigFieldType.Text, group: 1)]
        public string pfxPassword;

        /// <summary>
        /// The URL to use for getting a timestamp.
        /// </summary>
        [ConfigField("Timestamp Authority URL", ConfigFieldType.Text, group: 1)]
        public string timestampURL;

        /// <summary>
        /// List of paths to the DLLs that can be signed.
        /// </summary>
        [ConfigField("Target DLL Paths", ConfigFieldType.TextList, group: 2)]
        public List<string> dllPaths;

        static SigningConfig()
        {
            RegisterFactory(() => new SigningConfig());
        }

        protected SigningConfig() : base("eos_plugin_signing_config.json") { }
    }
}
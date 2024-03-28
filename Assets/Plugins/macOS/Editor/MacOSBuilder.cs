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

namespace PlayEveryWare.EpicOnlineServices.Build
{
    using System.IO;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    public class MacOSBuilder : PlatformSpecificBuilder
    {
        public MacOSBuilder() : base("Plugins/macOS") { }

        public override void PreBuild(BuildReport report)
        {
            string macOSPackagePluginFolder =
                Path.Combine("Packages", EOSPackageInfo.PackageName, "Runtime", "macOS");
            if (File.Exists(Path.Combine(macOSPackagePluginFolder, "libDynamicLibraryLoaderHelper.dylib")) &&
                File.Exists(Path.Combine(macOSPackagePluginFolder, "MicrophoneUtility_macos.dylib")))
            {
                return;
            }

            string macOSPluginFolder = Path.Combine(Application.dataPath, "Plugins", "macOS");
            if (!File.Exists(Path.Combine(macOSPluginFolder, "libDynamicLibraryLoaderHelper.dylib")) ||
                !File.Exists(Path.Combine(macOSPluginFolder, "MicrophoneUtility_macos.dylib")))
            {
                // TODO: Implement functionality to actually do this for the user, instead of prompting them to do it.
                Debug.LogError(
                    "Custom native libraries missing for mac build, use the makefile in lib/NativeCode/DynamicLibraryLoaderHelper_macOS to install the libraries");
            }
        }
    }
}
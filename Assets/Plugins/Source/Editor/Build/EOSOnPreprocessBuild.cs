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

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    public class EOSOnPreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            //if (report.summary.platform == BuildTarget.StandaloneWindows || report.summary.platform == BuildTarget.StandaloneWindows64)
            if (EOSPreprocessUtilities.isEOSDisableScriptingDefineEnabled(report))
            {
                return;
            }

            Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " +
                      report.summary.outputPath);

            if (report.summary.platform == BuildTarget.StandaloneOSX)
            {

                string macOSPacakgePluginFolder =
                    Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "Runtime", "macOS");
                if (!File.Exists(Path.Combine(macOSPacakgePluginFolder, "libDynamicLibraryLoaderHelper.dylib")) ||
                    !File.Exists(Path.Combine(macOSPacakgePluginFolder, "MicrophoneUtility_macos.dylib")))
                {
                    string macOSPluginFolder = Path.Combine(Application.dataPath, "Plugins", "macOS");
                    if (!File.Exists(Path.Combine(macOSPluginFolder, "libDynamicLibraryLoaderHelper.dylib")) ||
                        !File.Exists(Path.Combine(macOSPluginFolder, "MicrophoneUtility_macos.dylib")))
                    {

                        Debug.LogError(
                            "Custom native libraries missing for mac build, use the makefile in lib/NativeCode/DynamicLibraryLoaderHelper_macOS to install the libraries");
                    }
                }
            }

            AutoSetProductVersion();
        }

        public void AutoSetProductVersion()
        {
#if !EOS_DISABLE
            var eosVersionConfigSection = new PrebuildConfigEditor();

            eosVersionConfigSection.Load();

            string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSPackageInfo.ConfigFileName);
            var eosConfigFile = new ConfigHandler<EOSConfig>(configFilePath);
            eosConfigFile.Read();

            var previousProdVer = eosConfigFile.Data.productVersion;
            var currentSectionConfig = eosVersionConfigSection.GetConfig().Data;

            if (currentSectionConfig == null)
            {
                return;
            }

            if (currentSectionConfig.useAppVersionAsProductVersion)
            {
                eosConfigFile.Data.productVersion = Application.version;
            }

            if (previousProdVer != eosConfigFile.Data.productVersion)
            {
                eosConfigFile.Write(true);
            }
#endif
        }
    }
}
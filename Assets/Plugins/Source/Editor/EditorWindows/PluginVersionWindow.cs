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

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using EpicOnlineServices.Utility;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;

    /// <summary>
    /// Unity Editor tool to display plug-in version information.
    /// </summary>
    [Serializable]
    public class PluginVersionWindow : EOSEditorWindow
    {
        string eos_library_version = "Not found";
        string eos_plugin_version = "Not found";

        public PluginVersionWindow() : base("EOS Version Information")
        {
        }

        /// <summary>
        /// Unity Editor tool to display plug-in version information.
        /// </summary>

        [Serializable]
        private class UPMPackage
        {
            [SerializeField]
            public string version;
        }

        [MenuItem("Tools/EOS Plugin/Version", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<PluginVersionWindow>();
        }

        public string GetRepositoryRoot()
        {
            return Path.Combine(Application.dataPath, "..");
        }

        public string GetTemplateDirectory()
        {
            return Path.Combine(GetRepositoryRoot(), "etc/PackageTemplate");
        }

        private static string GetPackageName()
        {
            return "com.playeveryware.eos";
        }

        private void ConfigureEOSPluginVersionFieldFromPath(string pathToJSONPackage)
        {
            UPMPackage package = JsonUtility.FromJsonFile<UPMPackage>(pathToJSONPackage);
            eos_plugin_version = package.version;
        }

        protected override void Setup()
        {
#if EOS_DISABLE
            eos_library_version = "disabled";
#else
            eos_library_version = DLLHandle.GetProductVersionForLibrary("EOSSDK-Win64-Shipping");
#endif

            string templateDirectory = GetTemplateDirectory();
            string packagedPluginPath = Path.GetFullPath("Packages/" + GetPackageName());

            // read the the plugin version if the template exists
            if (Directory.Exists(templateDirectory))
            {
                var pathToJSONPackage = Path.Combine(templateDirectory, "package.json");
                ConfigureEOSPluginVersionFieldFromPath(pathToJSONPackage);
            }
            else if(Directory.Exists(packagedPluginPath))
            {
                var pathToJSONPackage = Path.Combine(packagedPluginPath, "package.json");
                ConfigureEOSPluginVersionFieldFromPath(pathToJSONPackage);
            }

        }

        protected override void RenderWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Epic Online Services Version:", EditorStyles.boldLabel);
            GUILayout.Label(eos_library_version);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Epic Online Services Plugin For Unity:", EditorStyles.boldLabel);
            GUILayout.Label(eos_plugin_version);
            GUILayout.EndHorizontal();
        }
    }
}

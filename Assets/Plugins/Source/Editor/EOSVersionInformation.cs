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
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices
{
    /// <summary>
    /// Unity Editor tool to display plug-in version information.
    /// </summary>
    public class EOSVersionInformation : EditorWindow
    {
        string eos_library_version = "Not found";
        string eos_plugin_version = "Not found";

        /// <summary>
        /// Unity Editor tool to display plug-in version information.
        /// </summary>

        [Serializable]
        private class UPMPackage
        {
            [SerializeField]
            public string version;
        }

        //-------------------------------------------------------------------------
        [MenuItem("Tools/Epic Online Services Version")]
        public static void ShowWindow()
        {
            GetWindow(typeof(EOSVersionInformation), false, "EOS Version", true);
        }

        public string GetRepositoryRoot()
        {
            return Path.Combine(Application.dataPath, "..");
        }

        public string GetTemplateDirectory()
        {
            return Path.Combine(GetRepositoryRoot(), "EOSUnityPlugin_package_template");
        }

        private static string GetPackageName()
        {
            return "com.playeveryware.eos";
        }

        private void ConfigureEOSPluginVersionFieldFromPath(string pathToJSONPackage)
        {
            var packageAsJSONString = File.ReadAllText(pathToJSONPackage);
            UPMPackage package = JsonUtility.FromJson<UPMPackage>(packageAsJSONString);
            eos_plugin_version = package.version;
        }

        //-------------------------------------------------------------------------
        public void Awake()
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

        //-------------------------------------------------------------------------
        public void OnGUI()
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

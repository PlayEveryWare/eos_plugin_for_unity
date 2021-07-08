using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices
{
    public class EOSVersionInformation : EditorWindow
    {
        string eos_library_version = "Not found";
        string eos_plugin_version = "Not found";

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
            GetWindow(typeof(EOSVersionInformation));
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
            eos_library_version = DLLHandle.GetVersionForLibrary("EOSSDK-Win64-Shipping");
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

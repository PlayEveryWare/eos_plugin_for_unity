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
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public interface IEOSPluginEditorConfigurationSection
    {
        string GetNameForMenu();

        void Awake();

        void LoadConfigFromDisk();

        void SaveToJSONConfig(bool prettyPrint);

        void OnGUI();

        bool DoesHaveUnsavedChanges();
    }
 
    //-------------------------------------------------------------------------
    public class EOSPluginEditorToolsConfigSection : IEOSPluginEditorConfigurationSection
    {
        private static string ConfigName = "eos_plugin_tools_config.json";
        private EOSConfigFile<EOSPluginEditorToolsConfig> configFile;

        [InitializeOnLoadMethod]
        static void Register()
        {
            EOSPluginEditorConfigEditor.AddConfigurationSectionEditor(new EOSPluginEditorToolsConfigSection());
        }

        //-------------------------------------------------------------------------
        public string GetNameForMenu()
        {
            return "Tools";
        }

        //-------------------------------------------------------------------------
        public void Awake()
        {
            var configFilenamePath = EOSPluginEditorConfigEditor.GetConfigPath(ConfigName);
            configFile = new EOSConfigFile<EOSPluginEditorToolsConfig>(configFilenamePath);
        }

        //-------------------------------------------------------------------------
        public bool DoesHaveUnsavedChanges()
        {
            return false;
        }

        //-------------------------------------------------------------------------
        public void LoadConfigFromDisk()
        {
            configFile.LoadConfigFromDisk();
        }

        public EOSPluginEditorToolsConfig GetCurrentConfig()
        {
            return configFile.currentEOSConfig;
        }

        //-------------------------------------------------------------------------
        void IEOSPluginEditorConfigurationSection.OnGUI()
        {

            string pathToIntegrityTool = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACIntegrityTool);
            string pathToIntegrityConfig = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACIntegrityConfig);
            string pathToEACCertificate = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACCertificate);
            string pathToEACPrivateKey = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACPrivateKey);
            string pathToEACSplashImage = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACSplashImage);
            string bootstrapOverideName = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.bootstrapperNameOverride);
            bool useEAC = configFile.currentEOSConfig.useEAC;

            EpicOnlineServicesConfigEditor.AssigningPath("Path to EAC Integrity Tool", ref pathToIntegrityTool, "Select EAC Integrity Tool", 
                tooltip: "EOS SDK tool used to generate EAC certificate from file hashes");
            EpicOnlineServicesConfigEditor.AssigningPath("Path to EAC Integrity Tool Config", ref pathToIntegrityConfig, "Select EAC Integrity Tool Config",
                tooltip: "Config file used by integry tool. Defaults to anticheat_integritytool.cfg in same directory.", extension: "cfg", labelWidth:200);
            EpicOnlineServicesConfigEditor.AssigningPath("Path to EAC private key", ref pathToEACPrivateKey, "Select EAC private key", extension: "key",
                tooltip: "EAC private key used in integrity tool cert generation. Exposing this to the public will comprimise anti-cheat functionality.");
            EpicOnlineServicesConfigEditor.AssigningPath("Path to EAC Certificate", ref pathToEACCertificate, "Select EAC public key", extension: "cer",
                tooltip: "EAC public key used in integrity tool cert generation");
            EpicOnlineServicesConfigEditor.AssigningPath("Path to EAC splash image", ref pathToEACSplashImage, "Select 800x450 EAC splash image PNG", extension: "png",
                tooltip: "EAC splash screen used by launcher. Must be a PNG of size 800x450.");

            EpicOnlineServicesConfigEditor.AssigningBoolField("Use EAC", ref useEAC, tooltip: "If set to true, uses the EAC");
            EpicOnlineServicesConfigEditor.AssigningTextField("Bootstrapper Name Override", ref bootstrapOverideName, labelWidth: 180, tooltip: "Name to use instead of 'Bootstrapper.exe'");

            configFile.currentEOSConfig.pathToEACIntegrityTool = pathToIntegrityTool;
            configFile.currentEOSConfig.pathToEACIntegrityConfig = pathToIntegrityConfig;
            configFile.currentEOSConfig.pathToEACPrivateKey = pathToEACPrivateKey;
            configFile.currentEOSConfig.pathToEACCertificate = pathToEACCertificate;
            configFile.currentEOSConfig.pathToEACSplashImage = pathToEACSplashImage;
            configFile.currentEOSConfig.useEAC = useEAC;
            configFile.currentEOSConfig.bootstrapperNameOverride = bootstrapOverideName;
        }

        //-------------------------------------------------------------------------
        public void SaveToJSONConfig(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }
    }

    //-------------------------------------------------------------------------

    public class EOSPluginEditorPrebuildConfigSection : IEOSPluginEditorConfigurationSection
    {
        private static string ConfigName = "eos_plugin_version_config.json";
        private EOSConfigFile<EOSPluginEditorPrebuildConfig> configFile;

        [InitializeOnLoadMethod]
        static void Register()
        {
            EOSPluginEditorConfigEditor.AddConfigurationSectionEditor(new EOSPluginEditorPrebuildConfigSection());
        }

        //-------------------------------------------------------------------------
        public string GetNameForMenu()
        {
            return "Prebuild Settings";
        }

        //-------------------------------------------------------------------------
        public void Awake()
        {
            var configFilenamePath = EOSPluginEditorConfigEditor.GetConfigPath(ConfigName);
            configFile = new EOSConfigFile<EOSPluginEditorPrebuildConfig>(configFilenamePath);
        }

        //-------------------------------------------------------------------------
        public bool DoesHaveUnsavedChanges()
        {
            return false;
        }

        //-------------------------------------------------------------------------
        public void LoadConfigFromDisk()
        {
            configFile.LoadConfigFromDisk();
        }

        public EOSPluginEditorPrebuildConfig GetCurrentConfig()
        {
            return configFile.currentEOSConfig;
        }

        //-------------------------------------------------------------------------
        void IEOSPluginEditorConfigurationSection.OnGUI()
        {
            EpicOnlineServicesConfigEditor.AssigningBoolField("Use Unity App Version for the EOS product version", ref configFile.currentEOSConfig.useAppVersionAsProductVersion, 300);
        }

        //-------------------------------------------------------------------------
        public void SaveToJSONConfig(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Creates the view for showing the eos plugin editor config values.
    ///
    /// </summary>
    public class EOSPluginEditorConfigEditor : EditorWindow
    {
        private static string ConfigDirectory = "EOSPluginEditorConfiguration";

        static List<IEOSPluginEditorConfigurationSection> configurationSectionEditors;

        bool prettyPrint = false;

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var eosPluginEditorConfigEditor = ScriptableObject.CreateInstance<EOSPluginEditorConfigEditor>();
            var provider = new SettingsProvider("Preferences/EOS Plugin", SettingsScope.User)
            {
                label = "EOS Plugin",
                guiHandler = (searchContext) =>
                {
                    EditorGUI.BeginChangeCheck();

                    eosPluginEditorConfigEditor.OnGUI();
                    if(EditorGUI.EndChangeCheck())
                    {
                        // save settings
                    }
                }
            };

            return provider;
        }

        //-------------------------------------------------------------------------
        [MenuItem("Edit/EOS Plugin Editor Configuration...")]
        public static void ShowWindow()
        {
            GetWindow(typeof(EOSPluginEditorConfigEditor));
        }

        //-------------------------------------------------------------------------
        public static void AddConfigurationSectionEditor(IEOSPluginEditorConfigurationSection section)
        {
            if (configurationSectionEditors == null)
            {
                configurationSectionEditors = new List<IEOSPluginEditorConfigurationSection>();
            }

            configurationSectionEditors.Add(section);
        }

        //-------------------------------------------------------------------------
        public static T GetConfigurationSectionEditor<T>() where T : IEOSPluginEditorConfigurationSection, new()
        {
            if (configurationSectionEditors != null)
            {
                foreach (var configurationSectionEditor in configurationSectionEditors)
                {
                    if (configurationSectionEditor.GetType() == typeof(T))
                    {
                        return (T)configurationSectionEditor;
                    }
                }
            }
            return default;
        }


        //-------------------------------------------------------------------------
        private static string GetConfigDirectory()
        {
            return System.IO.Path.Combine(Application.dataPath, "..", ConfigDirectory);
        }

        //-------------------------------------------------------------------------
        public static string GetConfigPath(string configFilename)
        {
            return System.IO.Path.Combine(GetConfigDirectory(), configFilename);
        }

        //-------------------------------------------------------------------------
        public static bool IsAsset(string configFilepath)
        {
            var assetDir = new DirectoryInfo(Application.dataPath);
            var fileDir = new DirectoryInfo(configFilepath);
            bool isParent = false;
            while (fileDir.Parent != null)
            {
                if (fileDir.Parent.FullName == assetDir.FullName)
                {
                    isParent = true;
                    break;
                }
                else fileDir = fileDir.Parent;
            }
            return isParent;
        }

        //-------------------------------------------------------------------------
        private void LoadConfigFromDisk()
        {

            if (!Directory.Exists(GetConfigDirectory()))
            {
                Directory.CreateDirectory(GetConfigDirectory());
            }

            foreach(var configurationSectionEditor in configurationSectionEditors)
            {
                configurationSectionEditor.LoadConfigFromDisk();
            }
        }

        //-------------------------------------------------------------------------
        private void Awake()
        {
            if (configurationSectionEditors == null)
            {
                configurationSectionEditors = new List<IEOSPluginEditorConfigurationSection>();
            }

            foreach (var configurationSectionEditor in configurationSectionEditors)
            {
                configurationSectionEditor.Awake();
            }

            LoadConfigFromDisk();
        }

        //-------------------------------------------------------------------------
        private void OnGUI()
        {

            GUILayout.BeginScrollView(new Vector2(), GUIStyle.none);
            if (configurationSectionEditors.Count > 0)
            {
                foreach (var configurationSectionEditor in configurationSectionEditors)
                {

                    GUILayout.Label(configurationSectionEditor.GetNameForMenu(), EditorStyles.boldLabel);
                    EpicOnlineServicesConfigEditor.HorizontalLine(Color.white);
                    configurationSectionEditor.OnGUI();
                    EditorGUILayout.Space();
                }
            }

            GUILayout.EndScrollView();
            EpicOnlineServicesConfigEditor.AssigningBoolField("Save JSON in 'Pretty' Format", ref prettyPrint, 190);
            GUI.SetNextControlName("Save");
            if (GUILayout.Button("Save All Changes"))
            {
                GUI.FocusControl("Save");
                SaveToJSONConfig(prettyPrint);
            }
        }

        //-------------------------------------------------------------------------
        private void SaveToJSONConfig(bool prettyPrint)
        {

            foreach (var configurationSectionEditor in configurationSectionEditors)
            {
                configurationSectionEditor.SaveToJSONConfig(prettyPrint);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        //-------------------------------------------------------------------------
        private bool DoesHaveUnsavedChanges()
        {
            foreach (var configurationSectionEditor in configurationSectionEditors)
            {
                if (configurationSectionEditor.DoesHaveUnsavedChanges())
                {
                    return true;
                }
            }

            return false;
        }

        //-------------------------------------------------------------------------
        private void OnGUIForUnsavedChanges(int idx)
        {
            if (GUI.Button(new Rect(10, 20, 100, 20), "Save"))
            {
                SaveToJSONConfig(prettyPrint);
            }

            if (GUI.Button(new Rect(10, 20, 100, 20), "Cancel"))
            {
            }
        }

        //-------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (DoesHaveUnsavedChanges())
            {
                GUI.ModalWindow(0, new Rect(20, 20, 120, 50), OnGUIForUnsavedChanges, "Unsaved Changes");
            }
        }
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Represents the EOS Plugin Configuration used for values that might 
    /// vary depending  on the environment that one is running Unity from.
    /// </summary>
    public class EOSPluginEditorToolsConfig : ICloneableGeneric<EOSPluginEditorToolsConfig>, IEmpty
    {
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

        public EOSPluginEditorToolsConfig Clone()
        {
            return (EOSPluginEditorToolsConfig)this.MemberwiseClone();
        }

        static public bool operator ==(EOSPluginEditorToolsConfig a, EOSPluginEditorToolsConfig b)
        {
            if (object.ReferenceEquals(a, null) != object.ReferenceEquals(b, null))
            {
                return false;
            }

            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            return a.pathToEACIntegrityTool == b.pathToEACIntegrityTool &&
                a.pathToDefaultCertificate == b.pathToDefaultCertificate &&
                a.pathToEACPrivateKey == b.pathToEACPrivateKey &&
                a.pathToEACCertificate == b.pathToEACCertificate &&
                a.pathToEACSplashImage == b.pathToEACSplashImage &&
                a.bootstrapperNameOverride == b.bootstrapperNameOverride &&
                a.useEAC == b.useEAC;
        }

        static public bool operator !=(EOSPluginEditorToolsConfig a, EOSPluginEditorToolsConfig b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is EOSPluginEditorToolsConfig && this == (EOSPluginEditorToolsConfig)obj; 
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(pathToEACIntegrityTool)
                && String.IsNullOrEmpty(pathToDefaultCertificate)
                && String.IsNullOrEmpty(pathToEACPrivateKey)
                && String.IsNullOrEmpty(pathToEACCertificate)
                && String.IsNullOrEmpty(pathToEACSplashImage)
                && String.IsNullOrEmpty(bootstrapperNameOverride)
                && !useEAC
            ;
        }
    }

    public class EOSPluginEditorPrebuildConfig : ICloneableGeneric<EOSPluginEditorPrebuildConfig>, IEmpty
    {

        public bool useAppVersionAsProductVersion;

        public EOSPluginEditorPrebuildConfig Clone()
        {
            return (EOSPluginEditorPrebuildConfig)this.MemberwiseClone();
        }

        public bool IsEmpty()
        {
            return false;
        }
    }
}

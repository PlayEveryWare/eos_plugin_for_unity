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
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{
    public class EOSPluginEditorToolsConfigEditor : ConfigEditor<EOSPluginEditorToolsConfig>
    {
        public EOSPluginEditorToolsConfigEditor() : base("Tools", "eos_plugin_tools_config.json") { }
        
        public override void OnGUI()
        {
            string pathToIntegrityTool = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACIntegrityTool);
            string pathToIntegrityConfig = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACIntegrityConfig);
            string pathToEACCertificate = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACCertificate);
            string pathToEACPrivateKey = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACPrivateKey);
            string pathToEACSplashImage = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToEACSplashImage);
            string bootstrapOverideName = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.bootstrapperNameOverride);
            bool useEAC = configFile.currentEOSConfig.useEAC;

            GUIEditorHelper.AssigningPath("Path to EAC Integrity Tool", ref pathToIntegrityTool, "Select EAC Integrity Tool", 
                tooltip: "EOS SDK tool used to generate EAC certificate from file hashes");
            GUIEditorHelper.AssigningPath("Path to EAC Integrity Tool Config", ref pathToIntegrityConfig, "Select EAC Integrity Tool Config",
                tooltip: "Config file used by integry tool. Defaults to anticheat_integritytool.cfg in same directory.", extension: "cfg", labelWidth:200);
            GUIEditorHelper.AssigningPath("Path to EAC private key", ref pathToEACPrivateKey, "Select EAC private key", extension: "key",
                tooltip: "EAC private key used in integrity tool cert generation. Exposing this to the public will comprimise anti-cheat functionality.");
            GUIEditorHelper.AssigningPath("Path to EAC Certificate", ref pathToEACCertificate, "Select EAC public key", extension: "cer",
                tooltip: "EAC public key used in integrity tool cert generation");
            GUIEditorHelper.AssigningPath("Path to EAC splash image", ref pathToEACSplashImage, "Select 800x450 EAC splash image PNG", extension: "png",
                tooltip: "EAC splash screen used by launcher. Must be a PNG of size 800x450.");

            GUIEditorHelper.AssigningBoolField("Use EAC", ref useEAC, tooltip: "If set to true, uses the EAC");
            GUIEditorHelper.AssigningTextField("Bootstrapper Name Override", ref bootstrapOverideName, labelWidth: 180, tooltip: "Name to use instead of 'Bootstrapper.exe'");

            configFile.currentEOSConfig.pathToEACIntegrityTool = pathToIntegrityTool;
            configFile.currentEOSConfig.pathToEACIntegrityConfig = pathToIntegrityConfig;
            configFile.currentEOSConfig.pathToEACPrivateKey = pathToEACPrivateKey;
            configFile.currentEOSConfig.pathToEACCertificate = pathToEACCertificate;
            configFile.currentEOSConfig.pathToEACSplashImage = pathToEACSplashImage;
            configFile.currentEOSConfig.useEAC = useEAC;
            configFile.currentEOSConfig.bootstrapperNameOverride = bootstrapOverideName;
        }
    }

    public class EOSPluginEditorPrebuildConfigEditor : ConfigEditor<EOSPluginEditorPrebuildConfig>
    {
        public EOSPluginEditorPrebuildConfigEditor() : base("Prebuild Settings", "eos_plugin_version_config.json") { }

        /// <summary>
        /// It's possible for the config file to not load in certain cases (like making test builds).
        /// </summary>
        public bool IsValid => configFile != null;
        
        public override void OnGUI()
        {
            GUIEditorHelper.AssigningBoolField("Use Unity App Version for the EOS product version", ref configFile.currentEOSConfig.useAppVersionAsProductVersion, 300);
        }
    }

    /// <summary>
    /// Creates the view for showing the eos plugin editor config values.
    /// </summary>
    [Serializable]
    public class EOSPluginEditorConfigEditor : EOSEditorWindow
    {
        private const string ConfigDirectory = "etc/EOSPluginEditorConfiguration";

        private List<IConfigEditor> configurationSectionEditors;

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
        
        [MenuItem("Tools/EOS Plugin/Configuration")]
        public static void ShowWindow()
        {
            GetWindow<EOSPluginEditorConfigEditor>("EOS Plugin Config");
        }
        
        private static string GetConfigDirectory()
        {
            return System.IO.Path.Combine(Application.dataPath, "..", ConfigDirectory);
        }
        
        public static string GetConfigPath(string configFilename)
        {
            return System.IO.Path.Combine(GetConfigDirectory(), configFilename);
        }
        
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
                
                fileDir = fileDir.Parent;
            }
            return isParent;
        }
        
        private void LoadConfigFromDisk()
        {
            if (!Directory.Exists(GetConfigDirectory()))
            {
                Directory.CreateDirectory(GetConfigDirectory());
            }

            foreach(var configurationSectionEditor in configurationSectionEditors)
            {
                configurationSectionEditor.Read();
            }
        }
        
        protected override void Setup()
        {
            configurationSectionEditors ??= new List<IConfigEditor>
                {
                    new EOSPluginEditorPrebuildConfigEditor(),
                    new EOSPluginEditorToolsConfigEditor(),
                    new EOSPluginEditorAndroidBuildConfigEditor(),
                    new LibraryBuildConfigEditor(),
                    new SignToolConfigEditor(),
                    new EOSPluginEditorPackagingConfigEditor()
                };

            LoadConfigFromDisk();
        }
        
        protected override void RenderWindow()
        {
            if (configurationSectionEditors.Count > 0)
            {
                foreach (var configurationSectionEditor in configurationSectionEditors)
                {
                    GUILayout.Label(configurationSectionEditor.GetLabel(), EditorStyles.boldLabel);
                    GUIEditorHelper.HorizontalLine(Color.white);
                    configurationSectionEditor.OnGUI();
                    EditorGUILayout.Space();
                }
            }

            GUIEditorHelper.AssigningBoolField("Save JSON in 'Pretty' Format", ref prettyPrint, 190);
            GUI.SetNextControlName("Save");
            if (GUILayout.Button("Save All Changes"))
            {
                GUI.FocusControl("Save");
                Save(prettyPrint);
            }
        }
        
        private void Save(bool prettyPrint)
        {
            foreach (var configurationSectionEditor in configurationSectionEditors)
            {
                configurationSectionEditor.Save(prettyPrint);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    
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

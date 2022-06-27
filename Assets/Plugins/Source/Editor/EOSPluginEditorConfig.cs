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
            string pathToSigntool = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToSignTool);
            EpicOnlineServicesConfigEditor.AssigningTextField("Path to signtool", ref pathToSigntool);

            if (pathToSigntool.Length != 0)
            {
                configFile.currentEOSConfig.pathToSignTool = pathToSigntool;
            }
        }

        //-------------------------------------------------------------------------
        public void SaveToJSONConfig(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }
    }

    //-------------------------------------------------------------------------
    public class EOSPluginEditorPackagingConfigSection : IEOSPluginEditorConfigurationSection
    {
        private static string ConfigName = "eos_plugin_packaging_config.json";
        private EOSConfigFile<EOSPluginEditorPackagingConfig> configFile;

        [InitializeOnLoadMethod]
        static void Register()
        {
            EOSPluginEditorConfigEditor.AddConfigurationSectionEditor(new EOSPluginEditorPackagingConfigSection());
        }

        //-------------------------------------------------------------------------
        public string GetNameForMenu()
        {
            return "Packaging";
        }

        //-------------------------------------------------------------------------
        public void Awake()
        {
            var configFilenamePath = EOSPluginEditorConfigEditor.GetConfigPath(ConfigName);
            configFile = new EOSConfigFile<EOSPluginEditorPackagingConfig>(configFilenamePath);
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

        public EOSPluginEditorPackagingConfig GetCurrentConfig()
        {
            return configFile.currentEOSConfig;
        }

        //-------------------------------------------------------------------------
        void IEOSPluginEditorConfigurationSection.OnGUI()
        {
            string customBuildDirectoryPath = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.customBuildDirectoryPath);
            EpicOnlineServicesConfigEditor.AssigningTextField("Custom Build Directory Path", ref customBuildDirectoryPath);

            if (customBuildDirectoryPath.Length != 0)
            {
                configFile.currentEOSConfig.customBuildDirectoryPath = customBuildDirectoryPath;
            }
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
                }
            }

            GUILayout.EndScrollView();
            EpicOnlineServicesConfigEditor.AssigningBoolField("Save JSON in 'Pretty' Format", 190, ref prettyPrint);
            if (GUILayout.Button("Save All Changes"))
            {
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
        /// <value><c>Path To signtool</c> The path to find the tool used for signing binaries</value>
        public string pathToSignTool;
        public string pathToDefaultCertificate;

        public EOSPluginEditorToolsConfig Clone()
        {
            return (EOSPluginEditorToolsConfig)this.MemberwiseClone();
        }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(pathToSignTool)
                && String.IsNullOrEmpty(pathToDefaultCertificate)
            ;
        }
    }

    public class EOSPluginEditorPackagingConfig : ICloneableGeneric<EOSPluginEditorPackagingConfig>, IEmpty
    {
        public string customBuildDirectoryPath;
        public string pathToJSONPackageDescription;
        public string pathToOutput;

        public EOSPluginEditorPackagingConfig Clone()
        {
            return (EOSPluginEditorPackagingConfig)this.MemberwiseClone();
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(customBuildDirectoryPath)
                && string.IsNullOrEmpty(pathToJSONPackageDescription)
                && string.IsNullOrEmpty(pathToOutput)
                ;
        }
    }
}

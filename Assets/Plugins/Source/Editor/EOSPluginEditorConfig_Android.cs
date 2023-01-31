using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSPluginEditorAndroidBuildConfigSection : IEOSPluginEditorConfigurationSection
    {
        private static string ConfigName = "eos_plugin_android_build_config.json";
        private EOSConfigFile<EOSPluginEditorAndroidBuildConfig> configFile;

        [InitializeOnLoadMethod]
        static void Register()
        {
            EOSPluginEditorConfigEditor.AddConfigurationSectionEditor(new EOSPluginEditorAndroidBuildConfigSection());
        }

        //-------------------------------------------------------------------------
        public string GetNameForMenu()
        {
            return "Android Build Settings";
        }

        //-------------------------------------------------------------------------
        public void Awake()
        {
            var configFilenamePath = EOSPluginEditorConfigEditor.GetConfigPath(ConfigName);
            configFile = new EOSConfigFile<EOSPluginEditorAndroidBuildConfig>(configFilenamePath);
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

        public EOSPluginEditorAndroidBuildConfig GetCurrentConfig()
        {
            return configFile.currentEOSConfig;
        }

        //-------------------------------------------------------------------------
        void IEOSPluginEditorConfigurationSection.OnGUI()
        {
            EpicOnlineServicesConfigEditor.AssigningBoolField("Link EOS Library Dynamically", ref configFile.currentEOSConfig.DynamicallyLinkEOSLibrary);
        }

        //-------------------------------------------------------------------------
        public void SaveToJSONConfig(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }
    }

    public class EOSPluginEditorAndroidBuildConfig : ICloneableGeneric<EOSPluginEditorAndroidBuildConfig>, IEmpty
    {

        public bool DynamicallyLinkEOSLibrary;

        public EOSPluginEditorAndroidBuildConfig Clone()
        {
            return (EOSPluginEditorAndroidBuildConfig)this.MemberwiseClone();
        }

        public bool IsEmpty()
        {
            return false;
        }
    }
}
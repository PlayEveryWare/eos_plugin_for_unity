using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PlayEveryWare.EpicOnlineServices
{
    
    public class EOSPluginEditorAndroidBuildConfigSection : PlatformSpecificConfigEditor<EOSPluginEditorAndroidBuildConfig>
    {
        public EOSPluginEditorAndroidBuildConfigSection() : base("Android Build Settings",
            "eos_plugin_android_build_config.json")
        {
        }

        public EOSPluginEditorAndroidBuildConfig GetCurrentConfig()
        {
            return configFile.currentEOSConfig;
        }

        public override void OnGUI()
        {
            GUIEditorHelper.AssigningBoolField("Link EOS Library Dynamically", ref configFile.currentEOSConfig.DynamicallyLinkEOSLibrary);
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
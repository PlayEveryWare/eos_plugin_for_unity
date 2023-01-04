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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices
{
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
            string pathToJSONPackageDescription = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToJSONPackageDescription);
            string customBuildDirectoryPath = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.customBuildDirectoryPath);
            string pathToOutput = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToOutput);
            EpicOnlineServicesConfigEditor.AssigningPath("JSON Description Path", ref pathToJSONPackageDescription, "Pick JSON Package Description", extension: "json", labelWidth: 170);
            EpicOnlineServicesConfigEditor.AssigningPath("Custom Build Directory Path", ref customBuildDirectoryPath, "Pick Custom Build Directory", selectFolder: true, labelWidth: 170);
            EpicOnlineServicesConfigEditor.AssigningPath("Output Path", ref pathToOutput, "Pick Output Directory", selectFolder: true, labelWidth: 170);

            configFile.currentEOSConfig.pathToJSONPackageDescription = pathToJSONPackageDescription;
            configFile.currentEOSConfig.customBuildDirectoryPath = customBuildDirectoryPath;
            configFile.currentEOSConfig.pathToOutput = pathToOutput;
        }

        //-------------------------------------------------------------------------
        public void SaveToJSONConfig(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }
    }
}

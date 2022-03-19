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

//#define ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{

    public class EpicOnlineServicesConfigEditor : EditorWindow
    {
        public static void AddPlatformSpecificConfigEditor(IPlatformSpecificConfigEditor platformSpecificConfigEditor)
        {
            if (platformSpecificConfigEditors == null)
            {
                platformSpecificConfigEditors = new List<IPlatformSpecificConfigEditor>();
            }
            platformSpecificConfigEditors.Add(platformSpecificConfigEditor);
        }

        private static string ConfigFilename = "EpicOnlineServicesConfig.json";
        private static string IntegratedPlatformConfigFilenameForSteam = "eos_steam_config.json";

        int toolbarInt = 0;
        string[] toolbarTitleStrings;
        static List<IPlatformSpecificConfigEditor> platformSpecificConfigEditors;

        public class EOSConfigFile<T> where T : ICloneableGeneric<T>, IEmpty, new()
        {
            public string configFilename;
            public T configDataOnDisk;
            public T currentEOSConfig;

            public EOSConfigFile(string aConfigFilename)
            {
                configFilename = aConfigFilename;
            }

            public void LoadConfigFromDisk()
            {
                string eosFinalConfigPath = GetConfigPath(configFilename);
                bool jsonConfigExists = File.Exists(eosFinalConfigPath);

                if (jsonConfigExists)
                {
                    var configDataAsString = System.IO.File.ReadAllText(eosFinalConfigPath);
                    configDataOnDisk = JsonUtility.FromJson<T>(configDataAsString);
                }
                else
                {
                    configDataOnDisk = new T();
                }
                currentEOSConfig = configDataOnDisk.Clone();
            }

            public void SaveToJSONConfig(bool prettyPrint)
            {
                if (currentEOSConfig.IsEmpty())
                {
                    AssetDatabase.DeleteAsset(GetConfigPath(configFilename));
                }
                else
                {
                    var configDataAsJSON = JsonUtility.ToJson(currentEOSConfig, prettyPrint);

                    File.WriteAllText(GetConfigPath(configFilename), configDataAsJSON);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        EOSConfigFile<EOSConfig> mainEOSConfigFile;

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        string eosGeneratedCFilePath = "";
#endif
        bool prettyPrint = false;

        EOSConfigFile<EOSSteamConfig> steamEOSConfigFile;

        [MenuItem("Tools/EpicOnlineServicesConfigEditor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(EpicOnlineServicesConfigEditor));
        }

        private static string GetConfigDirectory()
        {
            return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS");
        }

        private static string GetConfigPath(string configFilename)
        {
            return System.IO.Path.Combine(GetConfigDirectory(), configFilename);
        }

        private string GetWindowsPluginDirectory()
        {
            return "";
        }

        private string GenerateEOSGeneratedFile(EOSConfig aEOSConfig)
        {
            return string.Format(String.Join("\n", new string[] {
            "#define EOS_PRODUCT_NAME \"{0}\"",
            "#define EOS_PRODUCT_VERSION \"{1}\"",
            "#define EOS_SANDBOX_ID \"{2}\"",
            "#define EOS_PRODUCT_ID \"{3}\"",
            "#define EOS_DEPLOYMENT_ID \"{4}\"",
            "#define EOS_CLIENT_SECRET \"{5}\"",
            "#define EOS_CLIENT_ID \"{6}\""
        }), aEOSConfig.productName,
            aEOSConfig.productVersion,
            aEOSConfig.productID,
            aEOSConfig.sandboxID,
            aEOSConfig.deploymentID,
            aEOSConfig.clientSecret,
            aEOSConfig.clientID) +
            @"
_WIN32 || _WIN64
#define PLATFORM_WINDOWS 1
#endif

#if _WIN64
#define PLATFORM_64BITS 1
#else
#define PLATFORM_32BITS 1
#endif

        extern ""C"" __declspec(dllexport) char*  __stdcall GetConfigAsJSONString()
{
            return ""{""
              ""productName:"" EOS_PRODUCT_NAME "",""
              ""productVersion: "" EOS_PRODUCT_VERSION "",""
              ""productID: ""  EOS_PRODUCT_ID "",""
              ""sandboxID: ""  EOS_SANDBOX_ID "",""
              ""deploymentID: "" EOS_DEPLOYMENT_ID "",""
              ""clientSecret: ""  EOS_CLIENT_SECRET "",""
              ""clientID: ""  EOS_CLIENT_ID

           ""}""
        ;
        }";
        }


        // read data from json file, if it exists
        // TODO: Handle different versions of the file?
        private void LoadConfigFromDisk()
        {

            if (!Directory.Exists(GetConfigDirectory()))
            {
                Directory.CreateDirectory(GetConfigDirectory());
            }

            mainEOSConfigFile.LoadConfigFromDisk();
            steamEOSConfigFile.LoadConfigFromDisk();

            foreach(var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                platformSpecificConfigEditor.LoadConfigFromDisk();
            }
        }

        private void Awake()
        {
            mainEOSConfigFile = new EOSConfigFile<EOSConfig>(ConfigFilename);
            steamEOSConfigFile = new EOSConfigFile<EOSSteamConfig>(IntegratedPlatformConfigFilenameForSteam);

            if (platformSpecificConfigEditors == null)
            {
                platformSpecificConfigEditors = new List<IPlatformSpecificConfigEditor>();
            }

            toolbarTitleStrings = new string[2 + platformSpecificConfigEditors.Count];
            toolbarTitleStrings[0] = "Main";
            toolbarTitleStrings[1] = "Steam";

            int i = 2;
            foreach (var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                platformSpecificConfigEditor.Awake();
                toolbarTitleStrings[i] = platformSpecificConfigEditor.GetNameForMenu();
                i++;
            }

            LoadConfigFromDisk();
        }

        private bool DoesHaveUnsavedChanges()
        {
            return false;
        }

        private void SaveToJSONConfig(bool prettyPrint)
        {
            mainEOSConfigFile.SaveToJSONConfig(prettyPrint);
            steamEOSConfigFile.SaveToJSONConfig(prettyPrint);

            foreach(var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                platformSpecificConfigEditor.SaveToJSONConfig(prettyPrint);
            }

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
            string generatedCFile = GenerateEOSGeneratedFile(mainEOSConfigFile.currentEOSConfig);
            File.WriteAllText(Path.Combine(eosGeneratedCFilePath, "EOSGenerated.c"), generatedCFile);
#endif

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string CollectFlags(List<string> flags)
        {
            return String.Join("|", EmptyPredicates.NewIfNull(flags));
        }

        public static List<string> SplitFlags(string collectedFlags)
        {
            return new List<string>(collectedFlags.Split('|'));
        }

        public static void AssigningFlagTextField(string label, ref List<string> flags)
        {
            var collectedEOSplatformFlags = CollectFlags(flags);
            var platformFlags = EditorGUILayout.TextField(label, collectedEOSplatformFlags);
            flags = SplitFlags(platformFlags);
        }

        public static void AssigningFlagTextField(string label, float labelWidth, ref List<string> flags)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            AssigningFlagTextField(label, ref flags);

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningTextField(string label, ref string value)
        {
            var newValue = EditorGUILayout.TextField(label, EmptyPredicates.NewIfNull(value), GUILayout.ExpandWidth(true));
            if (!EmptyPredicates.IsEmptyOrNull(newValue))
            {
                value = newValue;
            }
        }

        public static void AssigningBoolField(string label, float labelWidth, ref bool value)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
            AssigningBoolField(label, ref value);
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
        public static void AssigningBoolField(string label, ref bool value)
        {
            var newValue = EditorGUILayout.Toggle(label, value, GUILayout.ExpandWidth(true));
            value = newValue;
        }

        public static void AssigningFloatToStringField(string label, float labelWidth, ref string value)
        {
            float valueAsFloat = EmptyPredicates.IsEmptyOrNull(value) ? 0.0f : float.Parse(value);

            EditorGUILayout.BeginHorizontal();
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
            float newValueAsFloat = EditorGUILayout.FloatField(label, valueAsFloat, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = originalLabelWidth;
            if(GUILayout.Button("Clear"))
            {
                value = null;
            }
            else
            {
                if (!(newValueAsFloat == valueAsFloat && EmptyPredicates.IsEmptyOrNull(value)))
                {
                    value = newValueAsFloat.ToString();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnDefaultGUI()
        {
            GUILayout.Label("Epic Online Services", EditorStyles.boldLabel);

            // TODO: Id the Product Name userfacing? If so, we need loc
            AssigningTextField("Product Name", ref mainEOSConfigFile.currentEOSConfig.productName);

            // TODO: bool to take product version form application version; should be automatic?
            AssigningTextField("Product Version", ref mainEOSConfigFile.currentEOSConfig.productVersion);
            AssigningTextField("Product ID", ref mainEOSConfigFile.currentEOSConfig.productID);
            AssigningTextField("Sandbox ID", ref mainEOSConfigFile.currentEOSConfig.sandboxID);
            AssigningTextField("Deployment ID", ref mainEOSConfigFile.currentEOSConfig.deploymentID);

            // This will be used on Windows via the nativeredner code, unless otherwise specified
            GUILayout.Label("Default Client Credentials");
            AssigningTextField("Client ID", ref mainEOSConfigFile.currentEOSConfig.clientID);
            AssigningTextField("Client Secret", ref mainEOSConfigFile.currentEOSConfig.clientSecret);
            AssigningTextField("Encryption Key", ref mainEOSConfigFile.currentEOSConfig.encryptionKey);

            AssigningFlagTextField("Platform Flags (Seperated by '|')", 190, ref mainEOSConfigFile.currentEOSConfig.platformOptionsFlags);

            AssigningBoolField("Always send Input to Overlay", 190, ref mainEOSConfigFile.currentEOSConfig.alwaysSendInputToOverlay);
        }

        private void OnSteamGUI()
        {
            GUILayout.Label("Steam Configuration Values");
            AssigningFlagTextField("Steam Flags (Seperated by '|')", 190, ref steamEOSConfigFile.currentEOSConfig.flags);
            AssigningTextField("Override Library path", ref steamEOSConfigFile.currentEOSConfig.overrideLibraryPath);
        }

        // TODO: create way to hook up new platforms dynamically 
        private string[] CreateToolbarTitles()
        {
            return toolbarTitleStrings;
        }

        //TODO: Add verification for data
        //TODO: Add something that warns if a feature won't work without some config
        private void OnGUI()
        {
            string[] toolbarTitlesToUse = CreateToolbarTitles();
            toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarTitleStrings);
            switch (toolbarInt)
            {
                case 0:
                    OnDefaultGUI();
                    break;
                case 1:
                    OnSteamGUI();
                    break;
                default:
                    if (platformSpecificConfigEditors.Count > toolbarInt - 2)
                    {
                        platformSpecificConfigEditors[toolbarInt - 2].OnGUI();
                    }
                    break;
            }

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        if (GUILayout.Button("Pick Path For Generated C File"))
        {
            eosGeneratedCFilePath = EditorUtility.OpenFolderPanel("Pick Path For Generated C File", "", "");
        }
#endif
            GUILayout.Label("Config Format Options", EditorStyles.boldLabel);
            AssigningBoolField("Save JSON in 'Pretty' Format", 190, ref prettyPrint);
            if (GUILayout.Button("Save All Changes"))
            {
                SaveToJSONConfig(prettyPrint);
            }
        }

        private void OnDestroy()
        {
            if (DoesHaveUnsavedChanges())
            {
                //Show Model window to confirm close on changes?
            }
        }
    }
}
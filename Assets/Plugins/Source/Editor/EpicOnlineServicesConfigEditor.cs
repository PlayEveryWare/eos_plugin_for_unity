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
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{

    public class EpicOnlineServicesConfigEditor : EditorWindow
    {
        static Regex EncryptionKeyRegex;

        static EpicOnlineServicesConfigEditor()
        {
            EncryptionKeyRegex = new Regex("[^0-9a-fA-F]");
        }

        public static void AddPlatformSpecificConfigEditor(IPlatformSpecificConfigEditor platformSpecificConfigEditor)
        {
            if (platformSpecificConfigEditors == null)
            {
                platformSpecificConfigEditors = new List<IPlatformSpecificConfigEditor>();
            }
            platformSpecificConfigEditors.Add(platformSpecificConfigEditor);
        }

        private static string IntegratedPlatformConfigFilenameForSteam = "eos_steam_config.json";

        static List<IPlatformSpecificConfigEditor> platformSpecificConfigEditors;


        int toolbarInt = 0;
        string[] toolbarTitleStrings;

        EOSConfigFile<EOSConfig> mainEOSConfigFile;

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        string eosGeneratedCFilePath = "";
#endif
        bool prettyPrint = false;

        EOSConfigFile<EOSSteamConfig> steamEOSConfigFile;

        [MenuItem("Tools/EpicOnlineServicesConfigEditor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(EpicOnlineServicesConfigEditor), false, "EOS Config Editor", true);
        }


        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            var eosPluginEditorConfigEditor = ScriptableObject.CreateInstance<EpicOnlineServicesConfigEditor>();
            var keywords = new List<string>();

            foreach(var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                keywords.Add(platformSpecificConfigEditor.GetNameForMenu());
            }

            var provider = new SettingsProvider("Project/EOS Plugin", SettingsScope.Project)
            {
                label = "EOS Plugin",
                keywords = keywords,
                guiHandler = (searchContext) =>
                {
                    eosPluginEditorConfigEditor.OnGUI();
                }
            };

            return provider;
        }

        private static string GetConfigDirectory()
        {
            return System.IO.Path.Combine("Assets", "StreamingAssets", "EOS");
        }

        public static string GetConfigPath(string configFilename)
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
            mainEOSConfigFile = new EOSConfigFile<EOSConfig>(EpicOnlineServicesConfigEditor.GetConfigPath(EOSPackageInfo.ConfigFileName));
            steamEOSConfigFile = new EOSConfigFile<EOSSteamConfig>(EpicOnlineServicesConfigEditor.GetConfigPath(IntegratedPlatformConfigFilenameForSteam));

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

        private static GUIContent CreateGUIContent(string label, string tooltip = null)
        {
            if (label == null)
            {
                label = "";
            }
            if (tooltip == null)
            {
                return new GUIContent(label);
            }
            else
            {
                return new GUIContent(label, tooltip);
            }
        }

        public static void AssigningFlagTextField(string label, ref List<string> flags, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var collectedEOSplatformFlags = CollectFlags(flags);
            var platformFlags = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), collectedEOSplatformFlags);
            flags = SplitFlags(platformFlags);

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningTextField(string label, ref string value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), EmptyPredicates.NewIfNull(value), GUILayout.ExpandWidth(true));
            if (newValue != null)
            {
                value = newValue;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningPath(string label, ref string filePath, string prompt, string directory = "", string extension = "", bool selectFolder = false, bool horizontalLayout = true, float maxButtonWidth = 100, float labelWidth = -1, string tooltip = null)
        {
            if (horizontalLayout)
            {
                EditorGUILayout.BeginHorizontal();
            }

            AssigningTextField(label, ref filePath, labelWidth, tooltip);

            bool buttonPressed = maxButtonWidth > 0 ? GUILayout.Button("Select", GUILayout.MaxWidth(maxButtonWidth)) : GUILayout.Button("Select");

            if (buttonPressed)
            {
                var newFilePath = selectFolder ? EditorUtility.OpenFolderPanel(prompt, "", "") : EditorUtility.OpenFilePanel(prompt, directory, extension);
                if (!string.IsNullOrWhiteSpace(newFilePath))
                {
                    filePath = newFilePath;
                }
            }

            if (horizontalLayout)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        public static void AssigningULongField(string label, ref ulong value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            ulong newValue = value;
            var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value.ToString(), GUILayout.ExpandWidth(true));
            if (string.IsNullOrWhiteSpace(newValueAsString))
            {
                newValueAsString = "0";
            }

            try
            {
                newValue = ulong.Parse(newValueAsString);
                value = newValue;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningUintField(string label, ref uint value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            uint newValue = value;
            var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value.ToString(), GUILayout.ExpandWidth(true));
            if (string.IsNullOrWhiteSpace(newValueAsString))
            {
                newValueAsString = "0";
            }

            try
            {
                newValue = uint.Parse(newValueAsString);
                value = newValue;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningULongToStringField(string label, ref string value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            try
            {
                EditorGUILayout.BeginHorizontal();
                var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value == null ? "" : value, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.MaxWidth(50)))
                {
                    value = null;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(newValueAsString))
                    {
                        value = null;
                        return;
                    }

                    var valueAsLong = ulong.Parse(newValueAsString);
                    value = valueAsLong.ToString();
                }
            }
            catch (FormatException)
            {
                value = null;
            }
            catch (OverflowException)
            {
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningBoolField(string label, ref bool value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue = EditorGUILayout.Toggle(CreateGUIContent(label, tooltip), value, GUILayout.ExpandWidth(true));
            value = newValue;

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningFloatToStringField(string label, ref string value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            try
            {
                EditorGUILayout.BeginHorizontal();
                var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value == null ? "" : value, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.MaxWidth(50)))
                {
                    value = null;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(newValueAsString))
                    {
                        value = null;
                        return;
                    }

                    var valueAsFloat = float.Parse(newValueAsString);
                    value = valueAsFloat.ToString();
                }
            }
            catch (FormatException)
            {
                value = null;
            }
            catch (OverflowException)
            {
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }


        public static void HorizontalLine(Color color)
        {
            var defaultHorizontalLineStyle = new GUIStyle();
            defaultHorizontalLineStyle.normal.background = EditorGUIUtility.whiteTexture;
            defaultHorizontalLineStyle.margin = new RectOffset(0, 0, 4, 4);
            defaultHorizontalLineStyle.fixedHeight = 1;
            HorizontalLine(color, defaultHorizontalLineStyle);
        }

        public static void HorizontalLine(Color color, GUIStyle guiStyle)
        {
            var currentColor = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, guiStyle);
            GUI.color = currentColor;
        }

        private void OnDefaultGUI()
        {
            GUILayout.Label("Epic Online Services", EditorStyles.boldLabel);

            float originalLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 200;

            // TODO: Id the Product Name userfacing? If so, we need loc
            AssigningTextField("Product Name", ref mainEOSConfigFile.currentEOSConfig.productName, tooltip: "Product Name defined in the EOS Development Portal");

            // TODO: bool to take product version form application version; should be automatic?
            AssigningTextField("Product Version", ref mainEOSConfigFile.currentEOSConfig.productVersion, tooltip: "Version of Product");
            AssigningTextField("Product ID", ref mainEOSConfigFile.currentEOSConfig.productID, tooltip: "Product ID defined in the EOS Development Portal");
            AssigningTextField("Sandbox ID", ref mainEOSConfigFile.currentEOSConfig.sandboxID, tooltip: "Sandbox ID defined in the EOS Development Portal");
            AssigningTextField("Deployment ID", ref mainEOSConfigFile.currentEOSConfig.deploymentID, tooltip: "Deployment ID defined in the EOS Development Portal");

            EditorGUILayout.LabelField("Sandbox Deployment Overrides");
            if(mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides == null)
            {
                mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides = new List<SandboxDeploymentOverride>();
            }
            for (int i = 0; i < mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                AssigningTextField("Sandbox ID", ref mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides[i].sandboxID, tooltip: "Deployment ID will be overridden when Sandbox ID is set to this", labelWidth:70);
                mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides[i].sandboxID = mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides[i].sandboxID.Trim();
                AssigningTextField("Deployment ID", ref mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides[i].deploymentID, tooltip: "Deployment ID to use for override", labelWidth: 90);
                mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides[i].deploymentID = mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides[i].deploymentID.Trim();
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(70)))
                {
                    mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                mainEOSConfigFile.currentEOSConfig.sandboxDeploymentOverrides.Add(new SandboxDeploymentOverride());
            }

            AssigningULongToStringField("Thread Affinity: networkWork", ref mainEOSConfigFile.currentEOSConfig.ThreadAffinity_networkWork,
                tooltip: "(Optional) Specifies thread affinity for network management that is not IO");
            AssigningULongToStringField("Thread Affinity: storageIO", ref mainEOSConfigFile.currentEOSConfig.ThreadAffinity_storageIO,
                tooltip: "(Optional) Specifies affinity for threads that will interact with a storage device");
            AssigningULongToStringField("Thread Affinity: webSocketIO", ref mainEOSConfigFile.currentEOSConfig.ThreadAffinity_webSocketIO,
                tooltip: "(Optional) Specifies affinity for threads that generate web socket IO");
            AssigningULongToStringField("Thread Affinity: P2PIO", ref mainEOSConfigFile.currentEOSConfig.ThreadAffinity_P2PIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate IO related to P2P traffic and management");
            AssigningULongToStringField("Thread Affinity: HTTPRequestIO", ref mainEOSConfigFile.currentEOSConfig.ThreadAffinity_HTTPRequestIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate http request IO");
            AssigningULongToStringField("Thread Affinity: RTCIO", ref mainEOSConfigFile.currentEOSConfig.ThreadAffinity_RTCIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate IO related to RTC traffic and management");

            string timeBudgetAsSting = "";

            if (mainEOSConfigFile.currentEOSConfig.tickBudgetInMilliseconds != 0)
            {
                timeBudgetAsSting = mainEOSConfigFile.currentEOSConfig.tickBudgetInMilliseconds.ToString();
            }
            AssigningTextField("Time Budget in milliseconds", ref timeBudgetAsSting, tooltip: "(Optional) Define the maximum amount of execution time the EOS SDK can use each frame");

            if (timeBudgetAsSting.Length != 0)
            {
                try
                {
                   mainEOSConfigFile.currentEOSConfig.tickBudgetInMilliseconds = Convert.ToUInt32(timeBudgetAsSting, 10);
                }
                catch
                {

                }
            }
            else
            {
                mainEOSConfigFile.currentEOSConfig.tickBudgetInMilliseconds = 0;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;

            // This will be used on Windows via the nativerender code, unless otherwise specified
            EditorGUILayout.Separator();
            GUILayout.Label("Default Client Credentials", EditorStyles.boldLabel);
            AssigningTextField("Client ID", ref mainEOSConfigFile.currentEOSConfig.clientID, tooltip: "Client ID defined in the EOS Development Portal");
            AssigningTextField("Client Secret", ref mainEOSConfigFile.currentEOSConfig.clientSecret, tooltip: "Client Secret defined in the EOS Development Portal");
            GUI.SetNextControlName("KeyText");
            AssigningTextField("Encryption Key", ref mainEOSConfigFile.currentEOSConfig.encryptionKey, tooltip: "Used to decode files previously encoded and stored in EOS");
            GUI.SetNextControlName("GenerateButton");
            if (GUILayout.Button("Generate"))
            {
                //generate random 32-byte hex sequence
                var rng = new System.Random(SystemInfo.deviceUniqueIdentifier.GetHashCode() * (int)(EditorApplication.timeSinceStartup * 1000));
                var keyBytes = new byte[32];
                rng.NextBytes(keyBytes);
                mainEOSConfigFile.currentEOSConfig.encryptionKey = BitConverter.ToString(keyBytes).Replace("-", "");
                //unfocus key input field so the new key is shown
                if (GUI.GetNameOfFocusedControl() == "KeyText")
                {
                    GUI.FocusControl("GenerateButton");
                }
            }

            if (!mainEOSConfigFile.currentEOSConfig.IsEncryptionKeyValid())
            {
                int keyLength = mainEOSConfigFile.currentEOSConfig.encryptionKey?.Length ?? 0;
                EditorGUILayout.HelpBox("Used for Player Data Storage and Title Storage. Must be left blank if unused. Encryption key must be 64 hex characters (0-9,A-F). Current length is " + keyLength + ".", MessageType.Warning);
            }

            AssigningFlagTextField("Platform Flags (Seperated by '|')", ref mainEOSConfigFile.currentEOSConfig.platformOptionsFlags, 190,
                "Flags used to initialize EOS Platform. Available flags are defined in PlatformFlags.cs");
            AssigningFlagTextField("Auth Scope Flags (Seperated by '|')", ref mainEOSConfigFile.currentEOSConfig.authScopeOptionsFlags, 210,
                "Flags used to specify Auth Scope during login. Available flags are defined in AuthScopeFlags.cs");

            AssigningBoolField("Always send Input to Overlay", ref mainEOSConfigFile.currentEOSConfig.alwaysSendInputToOverlay, 190,
                "If true, the plugin will always send input to the overlay from the C# side to native, and handle showing the overlay. This doesn't always mean input makes it to the EOS SDK.");
        }

        private void OnSteamGUI()
        {
            GUILayout.Label("Steam Configuration Values", EditorStyles.boldLabel);
            AssigningFlagTextField("Steam Flags (Seperated by '|')", ref steamEOSConfigFile.currentEOSConfig.flags, 190);
            AssigningTextField("Override Library path", ref steamEOSConfigFile.currentEOSConfig.overrideLibraryPath);
            AssigningUintField("Steamworks SDK major version", ref steamEOSConfigFile.currentEOSConfig.steamSDKMajorVersion, 190);
            AssigningUintField("Steamworks SDK minor version", ref steamEOSConfigFile.currentEOSConfig.steamSDKMinorVersion, 190);
#if STEAMWORKS_MODULE
            if (GUILayout.Button("Update from Steamworks.NET", GUILayout.MaxWidth(200)))
            {
                var steamworksVersion = Steamworks.Version.SteamworksSDKVersion;
                var versionParts = steamworksVersion.Split(".");
                bool success = false;
                if (versionParts.Length >= 2)
                {
                    success = uint.TryParse(versionParts[0], out uint major);
                    success &= uint.TryParse(versionParts[1], out uint minor);
                    if (success)
                    {
                        steamEOSConfigFile.currentEOSConfig.steamSDKMajorVersion = major;
                        steamEOSConfigFile.currentEOSConfig.steamSDKMinorVersion = minor;
                    }
                }
                if (!success)
                {
                    Debug.LogError("Failed to retrive Steamworks SDK version from Steamworks.NET");
                }
            }
#endif
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
            EnsureConfigLoaded();
            string[] toolbarTitlesToUse = CreateToolbarTitles();
            int xCount = (int)(EditorGUIUtility.currentViewWidth / 200);
            toolbarInt = GUILayout.SelectionGrid(toolbarInt, toolbarTitlesToUse, xCount);
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
            EditorGUILayout.Separator();
            GUILayout.Label("Config Format Options", EditorStyles.boldLabel);
            AssigningBoolField("Save JSON in 'Pretty' Format", ref prettyPrint, 190);
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

        private void EnsureConfigLoaded()
        {
            if (mainEOSConfigFile == null ||
                mainEOSConfigFile.configDataOnDisk == null ||
                mainEOSConfigFile.currentEOSConfig == null ||
                steamEOSConfigFile == null ||
                steamEOSConfigFile.configDataOnDisk == null ||
                steamEOSConfigFile.currentEOSConfig == null)
            {
                Awake();
            }
        }
    }
}
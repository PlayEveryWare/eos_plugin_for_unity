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

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using Utility;
    using Config = EpicOnlineServices.Config;
    using Random = System.Random;

    [Serializable]
    public class EOSSettingsWindow : EOSEditorWindow
    {
        private const string WindowTitle = "EOS Configuration";
        private List<IConfigEditor> platformSpecificConfigEditors;

        private static readonly string ConfigDirectory = Path.Combine("Assets", "StreamingAssets", "EOS");

        int toolbarInt;
        string[] toolbarTitleStrings;

        EOSConfig mainEOSConfigFile;

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        string eosGeneratedCFilePath = "";
#endif
        bool prettyPrint;

        EOSSteamConfig steamEOSConfigFile;

        [MenuItem("Tools/EOS Plugin/EOS Configuration")]
        public static void ShowWindow()
        {
            GetWindow<EOSSettingsWindow>(WindowTitle);
        }

        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            var eosPluginEditorConfigEditor = CreateInstance<EOSSettingsWindow>();
            string[] keywords = {"Epic", "EOS", "Online", "Services", "PlayEveryWare"};
            // mark the editor window as being embedded, so it skips auto formatting stuff.
            eosPluginEditorConfigEditor.SetIsEmbedded(true);
            var provider = new SettingsProvider($"Preferences/{WindowTitle}", SettingsScope.Project)
            {
                label = WindowTitle,
                keywords = keywords,
                guiHandler = searchContext =>
                {
                    eosPluginEditorConfigEditor.OnGUI();
                }
            };

            return provider;
        }

        private string GenerateEOSGeneratedFile(EOSConfig aEOSConfig)
        {
            return string.Format(
                       String.Join("\n", "#define EOS_PRODUCT_NAME \"{0}\"", "#define EOS_PRODUCT_VERSION \"{1}\"",
                           "#define EOS_SANDBOX_ID \"{2}\"", "#define EOS_PRODUCT_ID \"{3}\"",
                           "#define EOS_DEPLOYMENT_ID \"{4}\"", "#define EOS_CLIENT_SECRET \"{5}\"",
                           "#define EOS_CLIENT_ID \"{6}\""), aEOSConfig.productName,
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

        protected override async Task AsyncSetup()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            mainEOSConfigFile = await Config.GetAsync<EOSConfig>();
            steamEOSConfigFile = await Config.GetAsync<EOSSteamConfig>();

            platformSpecificConfigEditors ??= new List<IConfigEditor>
            {
                new LinuxConfigEditor(), new AndroidConfigEditor(), new IOSConfigEditor(), new MacOSConfigEditor()
            };

            toolbarTitleStrings = new string[2 + platformSpecificConfigEditors.Count];
            toolbarTitleStrings[0] = "Main";
            toolbarTitleStrings[1] = "Steam";

            int i = 2;
            foreach (var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                await platformSpecificConfigEditor.Load();
                toolbarTitleStrings[i] = platformSpecificConfigEditor.GetLabelText();
                i++;
            }

            await base.AsyncSetup();
        }

        private async Task Save(bool usePrettyFormat)
        {
            await mainEOSConfigFile.WriteAsync(usePrettyFormat);
            await steamEOSConfigFile.WriteAsync(usePrettyFormat);

            foreach (var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                await platformSpecificConfigEditor.Save(usePrettyFormat);
            }

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
            string generatedCFile = GenerateEOSGeneratedFile(mainEOSConfigFile.Data);
            File.WriteAllText(Path.Combine(eosGeneratedCFilePath, "EOSGenerated.c"), generatedCFile);
#endif

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnDefaultGUI()
        {
            GUILayout.Label("Epic Online Services", EditorStyles.boldLabel);

            float originalLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 200;

            // TODO: Id the Product Name userfacing? If so, we need loc
            GUIEditorUtility.AssigningTextField("Product Name", ref mainEOSConfigFile.productName,
                tooltip: "Product Name defined in the EOS Development Portal");

            // TODO: bool to take product version form application version; should be automatic?
            GUIEditorUtility.AssigningTextField("Product Version", ref mainEOSConfigFile.productVersion,
                tooltip: "Version of Product");
            GUIEditorUtility.AssigningTextField("Product ID", ref mainEOSConfigFile.productID,
                tooltip: "Product ID defined in the EOS Development Portal");
            GUIEditorUtility.AssigningTextField("Sandbox ID", ref mainEOSConfigFile.sandboxID,
                tooltip: "Sandbox ID defined in the EOS Development Portal");
            GUIEditorUtility.AssigningTextField("Deployment ID", ref mainEOSConfigFile.deploymentID,
                tooltip: "Deployment ID defined in the EOS Development Portal");

            GUIEditorUtility.AssigningBoolField("Is Server", ref mainEOSConfigFile.isServer,
                tooltip: "Set to 'true' if the application is a dedicated game serve");

            EditorGUILayout.LabelField("Sandbox Deployment Overrides");
            if (mainEOSConfigFile.sandboxDeploymentOverrides == null)
            {
                mainEOSConfigFile.sandboxDeploymentOverrides = new List<SandboxDeploymentOverride>();
            }

            for (int i = 0; i < mainEOSConfigFile.sandboxDeploymentOverrides.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                GUIEditorUtility.AssigningTextField("Sandbox ID",
                    ref mainEOSConfigFile.sandboxDeploymentOverrides[i].sandboxID,
                    tooltip: "Deployment ID will be overridden when Sandbox ID is set to this", labelWidth: 70);
                mainEOSConfigFile.sandboxDeploymentOverrides[i].sandboxID =
                    mainEOSConfigFile.sandboxDeploymentOverrides[i].sandboxID.Trim();
                GUIEditorUtility.AssigningTextField("Deployment ID",
                    ref mainEOSConfigFile.sandboxDeploymentOverrides[i].deploymentID,
                    tooltip: "Deployment ID to use for override", labelWidth: 90);
                mainEOSConfigFile.sandboxDeploymentOverrides[i].deploymentID =
                    mainEOSConfigFile.sandboxDeploymentOverrides[i].deploymentID.Trim();
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(70)))
                {
                    mainEOSConfigFile.sandboxDeploymentOverrides.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                mainEOSConfigFile.sandboxDeploymentOverrides.Add(new SandboxDeploymentOverride());
            }

            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: networkWork",
                ref mainEOSConfigFile.ThreadAffinity_networkWork,
                tooltip: "(Optional) Specifies thread affinity for network management that is not IO");
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: storageIO",
                ref mainEOSConfigFile.ThreadAffinity_storageIO,
                tooltip: "(Optional) Specifies affinity for threads that will interact with a storage device");
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: webSocketIO",
                ref mainEOSConfigFile.ThreadAffinity_webSocketIO,
                tooltip: "(Optional) Specifies affinity for threads that generate web socket IO");
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: P2PIO",
                ref mainEOSConfigFile.ThreadAffinity_P2PIO,
                tooltip:
                "(Optional) Specifies affinity for any thread that will generate IO related to P2P traffic and management");
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: HTTPRequestIO",
                ref mainEOSConfigFile.ThreadAffinity_HTTPRequestIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate http request IO");
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: RTCIO",
                ref mainEOSConfigFile.ThreadAffinity_RTCIO,
                tooltip:
                "(Optional) Specifies affinity for any thread that will generate IO related to RTC traffic and management");

            string timeBudgetAsSting = "";

            if (mainEOSConfigFile.tickBudgetInMilliseconds != 0)
            {
                timeBudgetAsSting = mainEOSConfigFile.tickBudgetInMilliseconds.ToString();
            }

            GUIEditorUtility.AssigningTextField("Time Budget in milliseconds", ref timeBudgetAsSting,
                tooltip: "(Optional) Define the maximum amount of execution time the EOS SDK can use each frame");

            if (timeBudgetAsSting.Length != 0)
            {
                try
                {
                    mainEOSConfigFile.tickBudgetInMilliseconds = Convert.ToUInt32(timeBudgetAsSting, 10);
                }
                catch
                {

                }
            }
            else
            {
                mainEOSConfigFile.tickBudgetInMilliseconds = 0;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;

            // This will be used on Windows via the nativerender code, unless otherwise specified
            EditorGUILayout.Separator();
            GUILayout.Label("Default Client Credentials", EditorStyles.boldLabel);
            GUIEditorUtility.AssigningTextField("Client ID", ref mainEOSConfigFile.clientID,
                tooltip: "Client ID defined in the EOS Development Portal");
            GUIEditorUtility.AssigningTextField("Client Secret", ref mainEOSConfigFile.clientSecret,
                tooltip: "Client Secret defined in the EOS Development Portal");
            GUI.SetNextControlName("KeyText");
            GUIEditorUtility.AssigningTextField("Encryption Key", ref mainEOSConfigFile.encryptionKey,
                tooltip: "Used to decode files previously encoded and stored in EOS");
            GUI.SetNextControlName("GenerateButton");
            if (GUILayout.Button("Generate"))
            {
                //generate random 32-byte hex sequence
                var rng = new Random(SystemInfo.deviceUniqueIdentifier.GetHashCode() *
                                     (int)(EditorApplication.timeSinceStartup * 1000));
                var keyBytes = new byte[32];
                rng.NextBytes(keyBytes);
                mainEOSConfigFile.encryptionKey = BitConverter.ToString(keyBytes).Replace("-", "");
                //unfocus key input field so the new key is shown
                if (GUI.GetNameOfFocusedControl() == "KeyText")
                {
                    GUI.FocusControl("GenerateButton");
                }
            }

            if (!mainEOSConfigFile.IsEncryptionKeyValid())
            {
                int keyLength = mainEOSConfigFile.encryptionKey?.Length ?? 0;
                EditorGUILayout.HelpBox(
                    "Used for Player Data Storage and Title Storage. Must be left blank if unused. Encryption key must be 64 hex characters (0-9,A-F). Current length is " +
                    keyLength + ".", MessageType.Warning);
            }

            GUIEditorUtility.AssigningFlagTextField("Platform Flags (Seperated by '|')",
                ref mainEOSConfigFile.platformOptionsFlags, 190,
                "Flags used to initialize EOS Platform. Available flags are defined in PlatformFlags.cs");
            GUIEditorUtility.AssigningFlagTextField("Auth Scope Flags (Seperated by '|')",
                ref mainEOSConfigFile.authScopeOptionsFlags, 210,
                "Flags used to specify Auth Scope during login. Available flags are defined in AuthScopeFlags.cs");

            GUIEditorUtility.AssigningBoolField("Always send Input to Overlay",
                ref mainEOSConfigFile.alwaysSendInputToOverlay, 190,
                "If true, the plugin will always send input to the overlay from the C# side to native, and handle showing the overlay. This doesn't always mean input makes it to the EOS SDK.");
        }

        private void OnSteamGUI()
        {
            GUILayout.Label("Steam Configuration Values", EditorStyles.boldLabel);
            GUIEditorUtility.AssigningFlagTextField("Steam Flags (Seperated by '|')", ref steamEOSConfigFile.flags,
                190);
            GUIEditorUtility.AssigningTextField("Override Library path", ref steamEOSConfigFile.overrideLibraryPath);
            GUIEditorUtility.AssigningUintField("Steamworks SDK major version",
                ref steamEOSConfigFile.steamSDKMajorVersion, 190);
            GUIEditorUtility.AssigningUintField("Steamworks SDK minor version",
                ref steamEOSConfigFile.steamSDKMinorVersion, 190);

            if (GUILayout.Button("Update from Steamworks.NET", GUILayout.MaxWidth(200)))
            {
                var steamworksVersion = Steamworks_Utility.GetSteamworksVersion();
                var versionParts = steamworksVersion.Split('.');
                bool success = false;
                if (versionParts.Length >= 2)
                {
                    success = uint.TryParse(versionParts[0], out uint major);
                    success &= uint.TryParse(versionParts[1], out uint minor);
                    if (success)
                    {
                        steamEOSConfigFile.steamSDKMajorVersion = major;
                        steamEOSConfigFile.steamSDKMinorVersion = minor;
                    }
                }

                if (!success)
                {
                    Debug.LogError("Failed to retrieve Steamworks SDK version from Steamworks.NET");
                }
            }
        }

        protected override void RenderWindow()
        {
            int xCount = (int)(EditorGUIUtility.currentViewWidth / 200);
            toolbarInt = GUILayout.SelectionGrid(toolbarInt, toolbarTitleStrings, xCount);
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
                        platformSpecificConfigEditors[toolbarInt - 2].Render();
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
            GUIEditorUtility.AssigningBoolField("Save JSON in 'Pretty' Format", ref prettyPrint, 190);
            if (GUILayout.Button("Save All Changes"))
            {
                Task.Run(() => Save(prettyPrint));
            }

            if (GUILayout.Button("Show in Explorer"))
            {
                EditorUtility.RevealInFinder(ConfigDirectory);
            }
        }
    }
}
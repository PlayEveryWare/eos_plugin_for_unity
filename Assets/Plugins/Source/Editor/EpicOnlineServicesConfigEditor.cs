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

public class EpicOnlineServicesConfigEditor : EditorWindow
{
    private static string ConfigFilename = "EpicOnlineServicesConfig.json";
    TextAsset configOnDisk;
    EOSConfig configData;
    EOSConfig currentEOSConfig;
    string eosGeneratedCFilePath = "";

    [MenuItem("Tools/EpicOnlineServicesConfigEditor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(EpicOnlineServicesConfigEditor));
    }

    private string GetConfigDirectory()
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS");
    }

    private string GetConfigPath()
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "EOS", ConfigFilename);
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
        string eosFinalConfigPath = GetConfigPath();
        if (File.Exists(eosFinalConfigPath))
        {
            var configDataAsString = System.IO.File.ReadAllText(eosFinalConfigPath);
            configOnDisk = new TextAsset(configDataAsString);
        }
        else
        {
            Directory.CreateDirectory(GetConfigDirectory());
        }
    }

    private void Awake()
    {
        LoadConfigFromDisk();
        bool jsonConfigExists = configOnDisk != null;

        if(jsonConfigExists)
        {
            string jsonString = configOnDisk.text;
            configData = JsonUtility.FromJson<EOSConfig>(jsonString);
            currentEOSConfig = configData.Clone();
        }
        else
        {
            configData = new EOSConfig();
            currentEOSConfig = configData.Clone();

            configOnDisk = new TextAsset(JsonUtility.ToJson(configData));
            System.IO.File.WriteAllText(GetConfigPath(), configOnDisk.text);

        }
    }

    private bool DoesHaveUnsavedChanges()
    {
        return false;
    }

    private void SaveToJSONConfig()
    {
        var configDataAsJSON = JsonUtility.ToJson(currentEOSConfig);
        string generatedCFile = GenerateEOSGeneratedFile(currentEOSConfig);

        File.WriteAllText(GetConfigPath(), configDataAsJSON);
        File.WriteAllText(Path.Combine(eosGeneratedCFilePath, "EOSGenerated.c"), generatedCFile);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //TODO: Add verification for data
    //TODO: Add something that warns if a feature won't work without some config
    private void OnGUI()
    {
        GUILayout.Label("Epic Online Services", EditorStyles.boldLabel);

        // require restart of editor to take effect
        // TODO: Id the Product Name userfacing? If so, we need loc
        currentEOSConfig.productName = EditorGUILayout.TextField("Product Name", currentEOSConfig.productName);

        // TODO: bool to take product version form application version; should be automatic?
        currentEOSConfig.productVersion = EditorGUILayout.TextField("Product Version", currentEOSConfig.productVersion);

        currentEOSConfig.productID = EditorGUILayout.TextField("Product ID", currentEOSConfig.productID);
        currentEOSConfig.sandboxID = EditorGUILayout.TextField("Sandbox ID", currentEOSConfig.sandboxID);
        currentEOSConfig.deploymentID = EditorGUILayout.TextField("Deployment ID", currentEOSConfig.deploymentID);


        // This will be used on Windows via the nativeredner code, unless otherwise specified
        GUILayout.Label("Default Client Credentials");
        currentEOSConfig.clientID = EditorGUILayout.TextField("Client ID", currentEOSConfig.clientID);
        currentEOSConfig.clientSecret = EditorGUILayout.TextField("Client Secret", currentEOSConfig.clientSecret);
        currentEOSConfig.encryptionKey = EditorGUILayout.TextField("Encryption Key", currentEOSConfig.encryptionKey);

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        if (GUILayout.Button("Pick Path For Generated C File"))
        {
            eosGeneratedCFilePath = EditorUtility.OpenFolderPanel("Pick Path For Generated C File", "", "");
        }
#endif

        if (GUILayout.Button("Save"))
        {
            SaveToJSONConfig();
        }
    }

    private void OnDestroy()
    {
        if(DoesHaveUnsavedChanges())
        {
            //Show Model window to confirm close on changes?
        }
    }
}

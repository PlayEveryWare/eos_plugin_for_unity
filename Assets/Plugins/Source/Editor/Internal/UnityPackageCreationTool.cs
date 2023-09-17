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

using UnityEngine;
using UnityEditor;

// make lines a little shorter
using UPCUtil = UnityPackageCreationUtility;
using ConfigEditor = PlayEveryWare.EpicOnlineServices.EpicOnlineServicesConfigEditor;

//-------------------------------------------------------------------------
public class UnityPackageCreationTool : EditorWindow
{
    //-------------------------------------------------------------------------
    [MenuItem("Tools/EOS Plugin/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool), false, "Create Package", true);
    }

    //-------------------------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Label("Unity Package Create", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        ConfigEditor.AssigningTextField("JSON Description Path", ref UPCUtil.jsonPackageFile);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var jsonFile = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
            if (!string.IsNullOrWhiteSpace(jsonFile))
            {
                UPCUtil.jsonPackageFile = jsonFile;
                UPCUtil.packageConfig.GetCurrentConfig().pathToJSONPackageDescription = UPCUtil.jsonPackageFile;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ConfigEditor.AssigningTextField("Output Path", ref UPCUtil.pathToOutput);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var outputDir = EditorUtility.OpenFolderPanel("Pick Output Directory", "", "");
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                UPCUtil.pathToOutput = outputDir;
                UPCUtil.packageConfig.GetCurrentConfig().pathToOutput = UPCUtil.pathToOutput;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ConfigEditor.AssigningTextField("Custom Build Directory", ref UPCUtil.customOutputDirectory);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var buildDir = EditorUtility.OpenFolderPanel("Pick Custom Build Directory", "", "");
            if (!string.IsNullOrWhiteSpace(buildDir))
            {
                UPCUtil.customOutputDirectory = buildDir;
                UPCUtil.packageConfig.GetCurrentConfig().customBuildDirectoryPath = buildDir;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20f);

        if (GUILayout.Button("Create UPM Package", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UPCUtil.pathToOutput))
            {
                return;
            }
            UPCUtil.packageConfig.SaveToJSONConfig(true);
            UPCUtil.CreateUPMTarball(UPCUtil.pathToOutput, UPCUtil.jsonPackageFile);
        }

        if (GUILayout.Button("Create .unitypackage", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UPCUtil.pathToOutput))
            {
                return;
            }
            UPCUtil.packageConfig.SaveToJSONConfig(true);
            UPCUtil.CreateDotUnityPackage(UPCUtil.pathToOutput, UPCUtil.jsonPackageFile);
        }

        if (GUILayout.Button("Export to Custom Build Directory", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UPCUtil.customOutputDirectory))
            {
                return;
            }
            UPCUtil.packageConfig.SaveToJSONConfig(true);
            CopyFilesInPackageDescriptionToBuildDir(UPCUtil.jsonPackageFile);
        }
    }

    //-------------------------------------------------------------------------
    private void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.5f);

        UPCUtil.CreateUPM(UPCUtil.jsonPackageFile);

        EditorUtility.ClearProgressBar();
    }
}

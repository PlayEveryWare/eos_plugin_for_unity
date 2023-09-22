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
    const string DEFAULT_OUTPUT_DIRECTORY = "Build";

    bool showJSON = false;

    //-------------------------------------------------------------------------
    [MenuItem("Tools/EOS Plugin/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool), false, "Create Package", true);
    }

    //-------------------------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        GUILayout.Space(10f);
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
        GUILayout.Space(10f);
        GUILayout.EndHorizontal();

        showJSON = EditorGUILayout.Foldout(showJSON, "Advanced");
        if (showJSON)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
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
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
        }
        
        GUILayout.Space(20f);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20f);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create UPM Package", GUILayout.MaxWidth(200)))
        {
            if (SaveConfiguration())
            {
                UPCUtil.CreateUPMTarball(UPCUtil.pathToOutput, UPCUtil.jsonPackageFile);
                OnPackageCreated(UPCUtil.pathToOutput);
            }
        }

        if (GUILayout.Button("Create .unitypackage", GUILayout.MaxWidth(200)))
        {
            if (SaveConfiguration())
            {
                // Creating the dot unity package file is asynchronous, so don't display a popup
                UPCUtil.CreateDotUnityPackage(UPCUtil.pathToOutput, UPCUtil.jsonPackageFile);

                //OnPackageCreated(UPCUtil.pathToOutput);
            }
        }

        if (GUILayout.Button("Export Directory", GUILayout.MaxWidth(200)))
        {
            if (SaveConfiguration())
            {
                UPCUtil.CreateUPM(UPCUtil.pathToOutput, UPCUtil.jsonPackageFile);
                OnPackageCreated(UPCUtil.pathToOutput);
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.Space(20f);
        GUILayout.EndHorizontal();
    }

    private void OnPackageCreated(string outputPath)
    {
        EditorUtility.DisplayDialog(
            "Package created",
            $"Package was successfully created at \"{outputPath}\"",
            "Ok");
    }

    private bool SaveConfiguration()
    {
        if (string.IsNullOrWhiteSpace(UPCUtil.pathToOutput) &&
                false == OnEmptyOutputPath(ref UPCUtil.pathToOutput))
        {
            return false;
        }
        UPCUtil.packageConfig.SaveToJSONConfig(true);
        return true;
    }

    private bool OnEmptyOutputPath(ref string output)
    {
        // Display dialog saying no output path was provided, and offering to default to the 'Build' directory.
        if (EditorUtility.DisplayDialog(
            "Empty output path",
            $"No output path was provided, do you want to use {DEFAULT_OUTPUT_DIRECTORY}?",
            "Yes", "Cancel"))
        {
            output = DEFAULT_OUTPUT_DIRECTORY;
            return true;
        }

        return false;
    }
}

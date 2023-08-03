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
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;
using PlayEveryWare.EpicOnlineServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Playeveryware.Editor;

//-------------------------------------------------------------------------
public class UnityPackageCreationTool : EditorWindow
{
    //-------------------------------------------------------------------------
    [MenuItem("Tools/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool), false, "Create Package", true);
    }

    //-------------------------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Label("Unity Package Create", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        EpicOnlineServicesConfigEditor.AssigningTextField("JSON Description Path", ref UnityPackageCreationUtility.pathToJSONPackageDescription);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var jsonFile = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
            if (!string.IsNullOrWhiteSpace(jsonFile))
            {
                UnityPackageCreationUtility.pathToJSONPackageDescription = jsonFile;
                UnityPackageCreationUtility.packagingConfigSection.GetCurrentConfig().pathToJSONPackageDescription = UnityPackageCreationUtility.pathToJSONPackageDescription;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EpicOnlineServicesConfigEditor.AssigningTextField("Output Path", ref UnityPackageCreationUtility.pathToOutput);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var outputDir = EditorUtility.OpenFolderPanel("Pick Output Directory", "", "");
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                UnityPackageCreationUtility.pathToOutput = outputDir;
                UnityPackageCreationUtility.packagingConfigSection.GetCurrentConfig().pathToOutput = UnityPackageCreationUtility.pathToOutput;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EpicOnlineServicesConfigEditor.AssigningTextField("Custom Build Directory", ref UnityPackageCreationUtility.customBuildDirectoryPath);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var buildDir = EditorUtility.OpenFolderPanel("Pick Custom Build Directory", "", "");
            if (!string.IsNullOrWhiteSpace(buildDir))
            {
                UnityPackageCreationUtility.customBuildDirectoryPath = buildDir;
                UnityPackageCreationUtility.packagingConfigSection.GetCurrentConfig().customBuildDirectoryPath = buildDir;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20f);

        if (GUILayout.Button("Create UPM Package", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UnityPackageCreationUtility.pathToOutput))
            {
                return;
            }
            UnityPackageCreationUtility.packagingConfigSection.SaveToJSONConfig(true);
            UnityPackageCreationUtility.CreateUPMPackage(UnityPackageCreationUtility.pathToOutput, UnityPackageCreationUtility.pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Create .unitypackage", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UnityPackageCreationUtility.pathToOutput))
            {
                return;
            }
            UnityPackageCreationUtility.packagingConfigSection.SaveToJSONConfig(true);
            UnityPackageCreationUtility.CreateLegacyUnityPackage(UnityPackageCreationUtility.pathToOutput, UnityPackageCreationUtility.pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Export to Custom Build Directory", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UnityPackageCreationUtility.customBuildDirectoryPath))
            {
                return;
            }
            UnityPackageCreationUtility.packagingConfigSection.SaveToJSONConfig(true);
            CopyFilesInPackageDescriptionToBuildDir(UnityPackageCreationUtility.pathToJSONPackageDescription);
        }
    }

    //-------------------------------------------------------------------------
/*     private void CreateLegacyUnityPackage(string outputPath, string pathToJSONPackageDescription, string packageName = "pew_eos_plugin.unitypackage")
    {
        EditorUtility.DisplayProgressBar("PEW Package Tool", "Packaging...", 0.5f);
        
        UnityPackageCreationUtility.CreateLegacyUnityPackage(
            outputPath,
            UnityPackageCreationUtility.pathToJSONPackageDescription,
            packageName
        );

        EditorUtility.ClearProgressBar();
    } */

    //-------------------------------------------------------------------------
    private void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.5f);
        
        UnityPackageCreationUtility.CopyFilesInPackageDescriptionToBuildDir(UnityPackageCreationUtility.pathToJSONPackageDescription);

        EditorUtility.ClearProgressBar();
    }
}

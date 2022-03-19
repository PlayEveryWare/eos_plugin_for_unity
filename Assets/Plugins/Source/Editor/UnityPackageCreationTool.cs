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
    UnityEditor.PackageManager.Requests.PackRequest packRequest;
    string pathToJSONPackageDescription = "";
    string pathToOutput = "";
    string customBuildDirectoryPath = "";

    //-------------------------------------------------------------------------
    [MenuItem("Tools/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool));
    }

    //-------------------------------------------------------------------------
    public string GetRepositoryRoot()
    {
        return Path.Combine(Application.dataPath, "..");
    }

    //-------------------------------------------------------------------------
    public string GetPackageConfigDirectory()
    {
        return Path.Combine(GetRepositoryRoot(), "PackageDescriptionConfigs");
    }

    //-------------------------------------------------------------------------
    private void Awake()
    {
        // Configure UI defaults
        pathToJSONPackageDescription = Path.Combine(GetPackageConfigDirectory(), "eos_package_description.json");
    }

    //-------------------------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Label("Unity Package Create", EditorStyles.boldLabel);

        GUILayout.Label("JSON Description Path");
        GUILayout.BeginHorizontal(GUIStyle.none);
        GUILayout.Label(pathToJSONPackageDescription);
        if (GUILayout.Button("Select"))
        {
            pathToJSONPackageDescription = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Output Path");
        GUILayout.BeginHorizontal(GUIStyle.none);
        GUILayout.Label(pathToOutput);
        if (GUILayout.Button("Select"))
        {
            pathToOutput = EditorUtility.OpenFolderPanel("Pick Output Directory", "", "");
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Custom Build Directory");
        GUILayout.BeginHorizontal(GUIStyle.none);
        GUILayout.Label(customBuildDirectoryPath);
        if (GUILayout.Button("Select"))
        {
            customBuildDirectoryPath = EditorUtility.OpenFolderPanel("Pick Custom Build Directory", "", "");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Create UPM Package"))
        {
            if (pathToOutput.Length == 0)
            {
                return;
            }

            CreateUPMPackage(pathToOutput, pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Create .unitypackage"))
        {
            if (pathToOutput.Length == 0)
            {
                return;
            }
            CreateLegacyUnityPackage(pathToOutput, pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Export to Custom Build Directory"))
        {
            if (customBuildDirectoryPath.Length == 0)
            {
                return;
            }
            CopyFilesInPackageDescriptionToBuildDir(pathToJSONPackageDescription);

        }

        if (packRequest != null && !packRequest.IsCompleted)
        {
            EditorUtility.DisplayProgressBar("Title", "Info", 0.5f);
        }
        else
        {
            EditorUtility.ClearProgressBar();
        }
    }

    //-------------------------------------------------------------------------
    private PackageDescription ReadPackageDescription(string pathToJSONPackageDescription)
    {
         var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
         var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
         return packageDescription;
    }

    //-------------------------------------------------------------------------
    private List<string> GetFilePathsMatchingPackageDescription(PackageDescription packageDescription)
    {
        string root = "./";
        return FileUtils.GetFilePathsMatchingPackageDescription(root, packageDescription);
    }

    //-------------------------------------------------------------------------
    private List<FileInfoMatchingResult> GetFileInfoMatchingPackageDescription(PackageDescription packageDescription)
    {
        return FileUtils.GetFileInfoMatchingPackageDescription("./", packageDescription);
    }

    //-------------------------------------------------------------------------
    private string GenerateTemporaryBuildPath()
    {
        return FileUtils.GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    private string GetPackageOutputFolder()
    {
        if (customBuildDirectoryPath != null && customBuildDirectoryPath.Length > 0)
        {
            return customBuildDirectoryPath;
        }
        return GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    private void CopyFilesToPackageDirectory(string packageFolder, List<FileInfoMatchingResult> fileInfoForFilesToCompress)
    {
        FileUtils.CopyFilesToDirectory(packageFolder, fileInfoForFilesToCompress);
    }

    //-------------------------------------------------------------------------
    private void CreateUPMPackage(string outputPath, string pathToJSONPackageDescription)
    {
        UnityEngine.Debug.Log("DEBUG " + pathToJSONPackageDescription);
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
        string packageFolder = GetPackageOutputFolder();
        var fileInfoForFilesToCompress = GetFileInfoMatchingPackageDescription(packageDescription);

        CopyFilesToPackageDirectory(packageFolder, fileInfoForFilesToCompress);

        packRequest = UnityEditor.PackageManager.Client.Pack(packageFolder, outputPath);

        
        while(!packRequest.IsCompleted)
        {
            EditorUtility.DisplayProgressBar("PEW Package Tool", "Packaging...", 0.5f);
        }

        if (packRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
        {
            if (packRequest.Error != null)
            {
                throw new Exception("Error making package " + packRequest.Error.message);
            }
        }
    }

    //-------------------------------------------------------------------------
    private void CreateLegacyUnityPackage(string outputPath, string pathToJSONPackageDescription, string packageName = "pew_eos_plugin.unitypackage")
    {
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

        // Transform PackageDescription into a list of actual files that can be copied to a directory that can be zipped 
        string gzipFilePathName = Path.Combine(outputPath, packageName);
        List<string> fileInfoForFilesToCompress = GetFilePathsMatchingPackageDescription(packageDescription);
        var toExport = fileInfoForFilesToCompress.Where((path) => { return !path.Contains(".meta"); }).ToArray();
        var options = ExportPackageOptions.Interactive;

        AssetDatabase.ExportPackage(toExport, gzipFilePathName, options);
    }

    //-------------------------------------------------------------------------
    private void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        var packageDescription = ReadPackageDescription(pathToJSONPackageDescription);
        var fileInfoForFilesToCopy = GetFileInfoMatchingPackageDescription(packageDescription);
        CopyFilesToPackageDirectory(customBuildDirectoryPath, fileInfoForFilesToCopy);
    }

    //-------------------------------------------------------------------------
    // This can't work without a way to write to tar files
    // Grab all the files as described in the text
    private void GZipUnityPackage(string outputPath, string pathToJSONPackageDescription)
    {
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

        // Transform PackageDescription into a list of actual files that can be copied to a directory that can be zipped 
        string gzipFilePathName = outputPath;
        var fileInfoForFilesToCompress = GetFileInfoMatchingPackageDescription(packageDescription);

        using (FileStream fileStream = File.Create(gzipFilePathName))
        {
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                foreach(var fileInfo in fileInfoForFilesToCompress)
                {
                    using (var fileStreamToCompress = fileInfo.fileInfo.OpenRead())
                    {
                        fileStreamToCompress.CopyTo(gzipStream);
                    }
                }
            }
        }
    }
}

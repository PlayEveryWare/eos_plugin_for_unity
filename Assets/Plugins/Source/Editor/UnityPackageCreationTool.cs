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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;

public class UnityPackageCreationTool : EditorWindow
{
    UnityEditor.PackageManager.Requests.PackRequest packRequest;
    string pathToJSONPackageDescription = "";
    string pathToOutput = "";
    string customBuildDirectoryPath = "";

    [Serializable]
    private class SrcDestPair
    {
        [SerializeField]
        public bool recursive;
        [SerializeField]
        public string src;
        [SerializeField]
        public string dest;
    }

    [Serializable]
    private class PackageDescription
    {
        [SerializeField]
        public List<SrcDestPair> source_to_dest;

        [SerializeField]
        public List<string> blacklist;
    }

    [MenuItem("Tools/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool));
    }

    public string GetRepositoryRoot()
    {
        return Path.Combine(Application.dataPath, "..");
    }

    public string GetPackageConfigDirectory()
    {
        return Path.Combine(GetRepositoryRoot(), "PackageDescriptionConfigs");
    }

    private void Awake()
    {
        // Configure UI defaults
        pathToJSONPackageDescription = Path.Combine(GetPackageConfigDirectory(), "eos_package_description.json");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Package Create", EditorStyles.boldLabel);

        GUILayout.Label("JSON Description Path");
        GUILayout.BeginHorizontal("");
        GUILayout.Label(pathToJSONPackageDescription);
        if (GUILayout.Button("Select"))
        {
            pathToJSONPackageDescription = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Output Path");
        GUILayout.BeginHorizontal("");
        GUILayout.Label(pathToOutput);
        if (GUILayout.Button("Select"))
        {
            pathToOutput = EditorUtility.OpenFolderPanel("Pick Output Directory", "", "");
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Custom Build Directory");
        GUILayout.BeginHorizontal("");
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

    private PackageDescription ReadPackageDescription(string pathToJSONPackageDescription)
    {
         var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
         var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
         return packageDescription;
    }

    private List<string> GetFilePathsMatchingPackageDescription(PackageDescription packageDescription)
    {
        var filepaths = new List<string>();
        foreach(var srcToDestKeyValues in packageDescription.source_to_dest)
        {
            SearchOption searchOption = srcToDestKeyValues.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var collectedFiles = Directory.EnumerateFiles("./", srcToDestKeyValues.src, searchOption);
            foreach (var entry in collectedFiles)
            {
                // Remove the "./", as it makes the AssetDatabase.ExportPackage code break
                filepaths.Add(entry.Remove(0, 2));
            }
        }

        return filepaths;
    }

    private List<Tuple<FileInfo, string>> GetFileInfoMatchingPackageDescription(PackageDescription packageDescription)
    {
        var fileInfos = new List<Tuple<FileInfo, string>>();

        foreach(var srcToDestKeyValues in packageDescription.source_to_dest)
        {
            SearchOption searchOption = srcToDestKeyValues.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var srcFileInfo = new FileInfo(srcToDestKeyValues.src);

            // Find instead the part of the path that does exist on disk
            if(!srcFileInfo.Exists)
            {
                srcFileInfo = new FileInfo(srcFileInfo.DirectoryName);
            }
            var collectedFiles = Directory.EnumerateFiles("./", srcToDestKeyValues.src, searchOption);
            foreach (var entry in collectedFiles)
            {
                FileInfo srcItem = new FileInfo(entry);
                var newItem = new Tuple<FileInfo, string>(srcItem, srcToDestKeyValues.dest);
                fileInfos.Add(newItem);
            }
        }

        return fileInfos;
    }

    private string GenerateTemporaryBuildPath()
    {
        return Application.temporaryCachePath + "/Output-" + System.Guid.NewGuid().ToString() + "/";
    }

    private string GetPackageOutputFolder()
    {
        if (customBuildDirectoryPath != null && customBuildDirectoryPath.Length > 0)
        {
            return customBuildDirectoryPath;
        }
        return GenerateTemporaryBuildPath();
    }

    private void CopyFilesToPackageDirectory(string packageFolder, List<Tuple<FileInfo, string>> fileInfoForFilesToCompress)
    {
        Directory.CreateDirectory(packageFolder);

        foreach (var fileInfo in fileInfoForFilesToCompress)
        {
            FileInfo src = fileInfo.Item1;
            string dest = fileInfo.Item2;

            string finalDestinationPath = Path.Combine(packageFolder, dest);
            string finalDestinationParent = Path.GetDirectoryName(finalDestinationPath);
            bool isDestinationADirectory = dest.EndsWith("/") || dest.Length == 0;

            if (!Directory.Exists(finalDestinationParent))
            {
                Directory.CreateDirectory(finalDestinationParent);
            }

            // If it ends in a '/', treat it as a directory to move to
            if (!Directory.Exists(finalDestinationPath))
            {
                if (isDestinationADirectory)
                {
                    Directory.CreateDirectory(finalDestinationPath);
                }
            }
            string destPath = isDestinationADirectory ? Path.Combine(finalDestinationPath, src.Name) : finalDestinationPath;

            // Ensure we can write over the dest path
            if (File.Exists(destPath))
            {
                var destPathFileInfo = new System.IO.FileInfo(destPath);
                destPathFileInfo.IsReadOnly = false;
            }

            File.Copy(src.FullName, destPath, true);
        }
    }

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

    private void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        var packageDescription = ReadPackageDescription(pathToJSONPackageDescription);
        var fileInfoForFilesToCopy = GetFileInfoMatchingPackageDescription(packageDescription);
        CopyFilesToPackageDirectory(customBuildDirectoryPath, fileInfoForFilesToCopy);
    }

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
                    using (var fileStreamToCompress = fileInfo.Item1.OpenRead())
                    {
                        fileStreamToCompress.CopyTo(gzipStream);
                    }
                }
            }
        }
    }
}

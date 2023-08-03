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

// Helper to allow for StartCoroutine to be used from a static context
public class CoroutineExecutor : MonoBehaviour { }

//-------------------------------------------------------------------------
public static class UnityPackageCreationUtility
{
    public static UnityEditor.PackageManager.Requests.PackRequest packRequest;
    public static string pathToJSONPackageDescription = "";
    public static string pathToOutput = "";
    public static string customBuildDirectoryPath = "";
    public static EOSPluginEditorPackagingConfigSection packagingConfigSection;

    static UnityPackageCreationUtility() 
    {
        packagingConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorPackagingConfigSection>();
        packagingConfigSection.Awake();
        packagingConfigSection.LoadConfigFromDisk();

        // Configure UI defaults
        pathToJSONPackageDescription = Path.Combine(
            UnityPackageCreationUtility.GetPackageConfigDirectory(), 
            "eos_package_description.json"
        );

        var currentConfig = packagingConfigSection.GetCurrentConfig();
        if (!string.IsNullOrEmpty(currentConfig.pathToJSONPackageDescription))
        {
            pathToJSONPackageDescription = currentConfig.pathToJSONPackageDescription;
        }
        if (!string.IsNullOrEmpty(currentConfig.pathToOutput))
        {
            pathToOutput = currentConfig.pathToOutput;
        }
        if(!string.IsNullOrEmpty(currentConfig.customBuildDirectoryPath))
        {
            customBuildDirectoryPath = currentConfig.customBuildDirectoryPath;
        }
    }

    //-------------------------------------------------------------------------
    public static string GetRepositoryRoot()
    {
        return Path.Combine(Application.dataPath, "..");
    }

    //-------------------------------------------------------------------------
    public static string GetPackageConfigDirectory()
    {
        return Path.Combine(GetRepositoryRoot(), "PackageDescriptionConfigs");
    }

    //-------------------------------------------------------------------------
    public static PackageDescription ReadPackageDescription(string pathToJSONPackageDescription)
    {
         var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
         var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
         return packageDescription;
    }

    //-------------------------------------------------------------------------
    public static List<string> GetFilePathsMatchingPackageDescription(PackageDescription packageDescription)
    {
        string root = "./";
        return PackageFileUtils.GetFilePathsMatchingPackageDescription(root, packageDescription);
    }

    //-------------------------------------------------------------------------
    public static List<FileInfoMatchingResult> GetFileInfoMatchingPackageDescription(PackageDescription packageDescription)
    {
        return PackageFileUtils.GetFileInfoMatchingPackageDescription("./", packageDescription);
    }

    //-------------------------------------------------------------------------
    public static string GenerateTemporaryBuildPath()
    {
        return PackageFileUtils.GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    public static string GetPackageOutputFolder()
    {
        if (customBuildDirectoryPath != null && customBuildDirectoryPath.Length > 0)
        {
            return customBuildDirectoryPath;
        }
        return GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    public static void CopyFilesToPackageDirectory(string packageFolder, List<FileInfoMatchingResult> fileInfoForFilesToCompress)
    {
        PackageFileUtils.CopyFilesToDirectory(packageFolder, fileInfoForFilesToCompress, WriteVersionInfo);
    }

    //-------------------------------------------------------------------------
    public static void WriteVersionInfo(string destPath)
    {
        if (Path.GetFileName(destPath) == "EOSPackageInfo.cs")
        {
            string version = EOSPackageInfo.GetPackageVersion();
            string contents = File.ReadAllText(destPath);
            string start = "//VERSION START";
            string end = "//VERSION END";
            var startIndex = contents.IndexOf(start) + start.Length;
            var endIndex = contents.IndexOf(end);
            var newFunction =
@"
    public static string GetPackageVersion()
    {
        return """+ version + @""";
    }
    ";
            string newContents = contents.Substring(0, startIndex) + newFunction + contents.Substring(endIndex);

            File.WriteAllText(destPath, newContents);
        }
    }

    private static CoroutineExecutor ExecutorInstance;

    //-------------------------------------------------------------------------
    public static void CreateUPMPackage(string outputPath, string pathToJSONPackageDescription)
    {
        UnityEngine.Debug.Log("DEBUG " + pathToJSONPackageDescription);
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
        string packageFolder = GetPackageOutputFolder();
        var fileInfoForFilesToCompress = GetFileInfoMatchingPackageDescription(packageDescription);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.3f);
        CopyFilesToPackageDirectory(packageFolder, fileInfoForFilesToCompress);

        if (!ExecutorInstance)
        {
            ExecutorInstance = UnityEngine.Object.FindObjectOfType<CoroutineExecutor>();

            if (!ExecutorInstance)
            {
                ExecutorInstance = new GameObject ("CoroutineExecutor").AddComponent<CoroutineExecutor>();
            }
        }

        ExecutorInstance.StartCoroutine(ClientMakePackage(packageFolder, outputPath));
    }

    //-------------------------------------------------------------------------
    public static void CreateLegacyUnityPackage(string outputPath, string pathToJSONPackageDescription, string packageName = "pew_eos_plugin.unitypackage")
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

    public static void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        var packageDescription = UnityPackageCreationUtility.ReadPackageDescription(pathToJSONPackageDescription);
        var fileInfoForFilesToCopy = UnityPackageCreationUtility.GetFileInfoMatchingPackageDescription(packageDescription);

        UnityPackageCreationUtility.CopyFilesToPackageDirectory(pathToJSONPackageDescription, fileInfoForFilesToCopy);
    }

        //-------------------------------------------------------------------------
    // Helper coroutine for making the client package.
    public static IEnumerator ClientMakePackage(string packageFolder, string outputPath)
    {   
        UnityPackageCreationUtility.packRequest = UnityEditor.PackageManager.Client.Pack(packageFolder, outputPath);
        while (!UnityPackageCreationUtility.packRequest.IsCompleted)
        {
            yield return null;
        }

        if (UnityPackageCreationUtility.packRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
        {
            if (UnityPackageCreationUtility.packRequest.Error != null)
            {
                throw new Exception("Error making package " + UnityPackageCreationUtility.packRequest.Error.message);
            }
        }
    }
}
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
    UnityEditor.PackageManager.Requests.PackRequest packRequest;
    string pathToJSONPackageDescription = "";
    string pathToOutput = "";
    string customBuildDirectoryPath = "";
    EOSPluginEditorPackagingConfigSection packagingConfigSection;

    //-------------------------------------------------------------------------
    [MenuItem("Tools/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool), false, "Create Package", true);
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
        packagingConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorPackagingConfigSection>();
        packagingConfigSection.Awake();
        packagingConfigSection.LoadConfigFromDisk();

        // Configure UI defaults
        pathToJSONPackageDescription = Path.Combine(GetPackageConfigDirectory(), "eos_package_description.json");

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
    private void OnGUI()
    {
        GUILayout.Label("Unity Package Create", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        EpicOnlineServicesConfigEditor.AssigningTextField("JSON Description Path", ref pathToJSONPackageDescription);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var jsonFile = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
            if (!string.IsNullOrWhiteSpace(jsonFile))
            {
                pathToJSONPackageDescription = jsonFile;
                packagingConfigSection.GetCurrentConfig().pathToJSONPackageDescription = pathToJSONPackageDescription;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EpicOnlineServicesConfigEditor.AssigningTextField("Output Path", ref pathToOutput);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var outputDir = EditorUtility.OpenFolderPanel("Pick Output Directory", "", "");
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                pathToOutput = outputDir;
                packagingConfigSection.GetCurrentConfig().pathToOutput = pathToOutput;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EpicOnlineServicesConfigEditor.AssigningTextField("Custom Build Directory", ref customBuildDirectoryPath);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
        {
            var buildDir = EditorUtility.OpenFolderPanel("Pick Custom Build Directory", "", "");
            if (!string.IsNullOrWhiteSpace(buildDir))
            {
                customBuildDirectoryPath = buildDir;
                packagingConfigSection.GetCurrentConfig().customBuildDirectoryPath = buildDir;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20f);

        if (GUILayout.Button("Create UPM Package", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(pathToOutput))
            {
                return;
            }
            packagingConfigSection.SaveToJSONConfig(true);
            CreateUPMPackage(pathToOutput, pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Create .unitypackage", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(pathToOutput))
            {
                return;
            }
            packagingConfigSection.SaveToJSONConfig(true);
            CreateLegacyUnityPackage(pathToOutput, pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Export to Custom Build Directory", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(customBuildDirectoryPath))
            {
                return;
            }
            packagingConfigSection.SaveToJSONConfig(true);
            CopyFilesInPackageDescriptionToBuildDir(pathToJSONPackageDescription);
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
        return PackageFileUtils.GetFilePathsMatchingPackageDescription(root, packageDescription);
    }

    //-------------------------------------------------------------------------
    private List<FileInfoMatchingResult> GetFileInfoMatchingPackageDescription(PackageDescription packageDescription)
    {
        return PackageFileUtils.GetFileInfoMatchingPackageDescription("./", packageDescription);
    }

    //-------------------------------------------------------------------------
    private string GenerateTemporaryBuildPath()
    {
        return PackageFileUtils.GenerateTemporaryBuildPath();
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
        PackageFileUtils.CopyFilesToDirectory(packageFolder, fileInfoForFilesToCompress, WriteVersionInfo);
    }

    //-------------------------------------------------------------------------
    private void WriteVersionInfo(string destPath)
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

    //-------------------------------------------------------------------------
    private void CreateUPMPackage(string outputPath, string pathToJSONPackageDescription)
    {
        UnityEngine.Debug.Log("DEBUG " + pathToJSONPackageDescription);
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
        string packageFolder = GetPackageOutputFolder();
        var fileInfoForFilesToCompress = GetFileInfoMatchingPackageDescription(packageDescription);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.3f);
        CopyFilesToPackageDirectory(packageFolder, fileInfoForFilesToCompress);

        this.StartCoroutine(ClientMakePackage(packageFolder, outputPath));
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

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Packaging...", 0.5f);
        AssetDatabase.ExportPackage(toExport, gzipFilePathName, options);
        EditorUtility.ClearProgressBar();
    }

    //-------------------------------------------------------------------------
    private void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        var packageDescription = ReadPackageDescription(pathToJSONPackageDescription);
        var fileInfoForFilesToCopy = GetFileInfoMatchingPackageDescription(packageDescription);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.5f);
        CopyFilesToPackageDirectory(customBuildDirectoryPath, fileInfoForFilesToCopy);
        EditorUtility.ClearProgressBar();
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

    //-------------------------------------------------------------------------
    // Helper coroutine for making the client package.
    private IEnumerator ClientMakePackage(string packageFolder, string outputPath)
    {
        packRequest = UnityEditor.PackageManager.Client.Pack(packageFolder, outputPath);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Packaging...", 0.5f);

        while (!packRequest.IsCompleted)
        {
            yield return null;
        }

        if (packRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
        {
            if (packRequest.Error != null)
            {
                throw new Exception("Error making package " + packRequest.Error.message);
            }
        }

        EditorUtility.ClearProgressBar();
    }
}

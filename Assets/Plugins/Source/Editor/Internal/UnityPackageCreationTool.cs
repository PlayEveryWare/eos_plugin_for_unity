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
    //UnityEditor.PackageManager.Requests.packRequest packRequest;
    string pathToJSONPackageDescription = "";
    //string pathToOutput = "";
    //string customBuildDirectoryPath = "";
    //EOSPluginEditor.packagingConfigSection .packagingConfigSection;

    //-------------------------------------------------------------------------
    [MenuItem("Tools/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityPackageCreationTool), false, "Create Package", true);
    }

/*
    //-------------------------------------------------------------------------
    public string GetRepositoryRoot()
    {
        return Path.Combine(Application.dataPath, "..");
    }

    //-------------------------------------------------------------------------
    public string GetPackageConfigDirectory()
    {
        return Path.Combine(
            UnityPackageCreationUtility.GetRepositoryRoot(), 
            "PackageDescriptionConfigs"
        );
    }


    //-------------------------------------------------------------------------
    private void Awake()
    {
        UnityPackageCreationUtility.packagingConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorUnityPackageCreationUtility.packagingConfigSection>();
        UnityPackageCreationUtility.packagingConfigSection.Awake();
        UnityPackageCreationUtility.packagingConfigSection.LoadConfigFromDisk();

        // Configure UI defaults
        pathToJSONPackageDescription = Path.Combine(
            UnityPackageCreationUtility.GetPackageConfigDirectory(), 
            "eos_package_description.json"
        );

        var currentConfig = UnityPackageCreationUtility.packagingConfigSection.GetCurrentConfig();
        if (!string.IsNullOrEmpty(currentConfig.pathToJSONPackageDescription))
        {
            pathToJSONPackageDescription = currentConfig.pathToJSONPackageDescription;
        }
        if (!string.IsNullOrEmpty(currentConfig.UnityPackageCreationUtility.pathToOutput))
        {
            UnityPackageCreationUtility.pathToOutput = currentConfig.UnityPackageCreationUtility.pathToOutput;
        }
        if(!string.IsNullOrEmpty(currentConfig.UnityPackageCreationUtility.customBuildDirectoryPath))
        {
            UnityPackageCreationUtility.customBuildDirectoryPath = currentConfig.UnityPackageCreationUtility.customBuildDirectoryPath;
        }
    }
*/
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
                UnityPackageCreationUtility.packagingConfigSection.GetCurrentConfig().pathToJSONPackageDescription = pathToJSONPackageDescription;
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
            CreateUPMPackage(UnityPackageCreationUtility.pathToOutput, pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Create .unitypackage", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UnityPackageCreationUtility.pathToOutput))
            {
                return;
            }
            UnityPackageCreationUtility.packagingConfigSection.SaveToJSONConfig(true);
            CreateLegacyUnityPackage(UnityPackageCreationUtility.pathToOutput, pathToJSONPackageDescription);
        }

        if (GUILayout.Button("Export to Custom Build Directory", GUILayout.MaxWidth(200)))
        {
            if (string.IsNullOrWhiteSpace(UnityPackageCreationUtility.customBuildDirectoryPath))
            {
                return;
            }
            UnityPackageCreationUtility.packagingConfigSection.SaveToJSONConfig(true);
            CopyFilesInPackageDescriptionToBuildDir(pathToJSONPackageDescription);
        }
    }
/*
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
    private List<FileInfoMatchingResult> UnityPackageCreationUtility.GetFileInfoMatchingPackageDescription(PackageDescription packageDescription)
    {
        return PackageFileUtils.UnityPackageCreationUtility.GetFileInfoMatchingPackageDescription("./", packageDescription);
    }

    //-------------------------------------------------------------------------
    private string GenerateTemporaryBuildPath()
    {
        return PackageFileUtils.GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    private string GetPackageOutputFolder()
    {
        if (UnityPackageCreationUtility.customBuildDirectoryPath != null && UnityPackageCreationUtility.customBuildDirectoryPath.Length > 0)
        {
            return UnityPackageCreationUtility.customBuildDirectoryPath;
        }
        return UnityPackageCreationUtility.GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    private void CopyFilesToPackageDirectory(string packageFolder, List<FileInfoMatchingResult> fileInfoForFilesToCompress)
    {
        PackageFileUtils.CopyFilesToDirectory(
            packageFolder, 
            fileInfoForFilesToCompress,
            UnityPackageCreationUtility.WriteVersionInfo
        );
    }
6a
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
*/
    //-------------------------------------------------------------------------
    private void CreateUPMPackage(string outputPath, string pathToJSONPackageDescription)
    {
        UnityEngine.Debug.Log("DEBUG " + pathToJSONPackageDescription);
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
        string packageFolder = UnityPackageCreationUtility.GetPackageOutputFolder();
        var fileInfoForFilesToCompress = UnityPackageCreationUtility.GetFileInfoMatchingPackageDescription(packageDescription);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.3f);
        UnityPackageCreationUtility.CopyFilesToPackageDirectory(
            packageFolder, 
            fileInfoForFilesToCompress
        );

        this.StartCoroutine(ClientMakePackage(packageFolder, outputPath));
    }

    //-------------------------------------------------------------------------
    private void CreateLegacyUnityPackage(string outputPath, string pathToJSONPackageDescription, string packageName = "pew_eos_plugin.unitypackage")
    {
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

        // Transform PackageDescription into a list of actual files that can be copied to a directory that can be zipped 
        string gzipFilePathName = Path.Combine(outputPath, packageName);
        List<string> fileInfoForFilesToCompress = UnityPackageCreationUtility.GetFilePathsMatchingPackageDescription(packageDescription);
        var toExport = fileInfoForFilesToCompress.Where((path) => { return !path.Contains(".meta"); }).ToArray();
        var options = ExportPackageOptions.Interactive;

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Packaging...", 0.5f);
        AssetDatabase.ExportPackage(toExport, gzipFilePathName, options);
        EditorUtility.ClearProgressBar();
    }

    //-------------------------------------------------------------------------
    private void CopyFilesInPackageDescriptionToBuildDir(string pathToJSONPackageDescription)
    {
        var packageDescription = UnityPackageCreationUtility.ReadPackageDescription(
            pathToJSONPackageDescription
        );
        var fileInfoForFilesToCopy = UnityPackageCreationUtility.GetFileInfoMatchingPackageDescription(packageDescription);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Copying files...", 0.5f);
        
        UnityPackageCreationUtility.CopyFilesToPackageDirectory(
            UnityPackageCreationUtility.customBuildDirectoryPath, 
            fileInfoForFilesToCopy
        );

        EditorUtility.ClearProgressBar();
    }
/*
    //-------------------------------------------------------------------------
    // This can't work without a way to write to tar files
    // Grab all the files as described in the text
    private void GZipUnityPackage(string outputPath, string pathToJSONPackageDescription)
    {
        var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
        var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

        // Transform PackageDescription into a list of actual files that can be copied to a directory that can be zipped 
        string gzipFilePathName = outputPath;
        var fileInfoForFilesToCompress = UnityPackageCreationUtility.GetFileInfoMatchingPackageDescription(packageDescription);

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
*/
    //-------------------------------------------------------------------------
    // Helper coroutine for making the client package.
    private IEnumerator ClientMakePackage(string packageFolder, string outputPath)
    {
        UnityPackageCreationUtility.packRequest = UnityEditor.PackageManager.Client.Pack(packageFolder, outputPath);

        EditorUtility.DisplayProgressBar("PEW Package Tool", "Packaging...", 0.5f);

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

        EditorUtility.ClearProgressBar();
    }
}

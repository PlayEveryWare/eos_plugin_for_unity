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
using System.Linq;
using Playeveryware.Editor;
using UnityEditor.Build;

// help make lines shorter
using PackagingConfigSection = PlayEveryWare.EpicOnlineServices.EOSPluginEditorPackagingConfigSection;
using ConfigEditor = PlayEveryWare.EpicOnlineServices.EOSPluginEditorConfigEditor;


// Helper to allow for StartCoroutine to be used from a static context
public class CoroutineExecutor : MonoBehaviour { }

//-------------------------------------------------------------------------
public static class UnityPackageCreationUtility
{
    /// <summary>
    /// This is where we will store the request we sent to Unity to make the
    /// package.
    /// </summary>
    public static UnityEditor.PackageManager.Requests.PackRequest packRequest;

    /// <summary>
    /// This is the path to the package.json file.
    /// </summary>
    public static string jsonPackageFile = "";

    /// <summary>
    /// The path to output to
    /// </summary>
    public static string pathToOutput = "";

    /// <summary>
    /// If there is a specific directory to build to, it's stored here
    /// </summary>
    public static string customOutputDirectory = "";

    /// <summary>
    /// Contains section of package.json file pertaining to configuration
    /// </summary>
    public static PackagingConfigSection packageConfig;

    /// <summary>
    /// This is used in order to use StartCoroutine from a static context.
    /// </summary>
    public static CoroutineExecutor ExecutorInstance;

    /// <summary>
    /// Static constructor
    /// </summary>
    static UnityPackageCreationUtility() 
    {
        packageConfig = ConfigEditor.GetConfigurationSectionEditor<PackagingConfigSection>();
        packageConfig.Awake();
        packageConfig.LoadConfigFromDisk();

        // Configure UI defaults
        jsonPackageFile = Path.Combine(
            Application.dataPath, 
            "..", 
            "PackageDescriptionConfigs",
            "eos_package_description.json");

        var currentConfig = packageConfig.GetCurrentConfig();
        if (!string.IsNullOrEmpty(currentConfig.pathToJSONPackageDescription))
        {
            jsonPackageFile = currentConfig.pathToJSONPackageDescription;
        }
        if (!string.IsNullOrEmpty(currentConfig.pathToOutput))
        {
            pathToOutput = currentConfig.pathToOutput;
        }
        if(!string.IsNullOrEmpty(currentConfig.customBuildDirectoryPath))
        {
            customOutputDirectory = currentConfig.customBuildDirectoryPath;
        }
    }

    //-------------------------------------------------------------------------
    private static PackageDescription ReadPackageDescription(
        string pathToJSONPackageDescription)
    {
         var JSONPackageDescription = File.ReadAllText(
             pathToJSONPackageDescription
         );

         var packageDescription = JsonUtility.FromJson<PackageDescription>(
             JSONPackageDescription
         );

         return packageDescription;
    }

    //-------------------------------------------------------------------------
    private static string GetPackageOutputFolder()
    {
        if (customOutputDirectory != null && 
            customOutputDirectory.Length > 0)
        {
            return customOutputDirectory;
        }
        return PackageFileUtils.GenerateTemporaryBuildPath();
    }

    //-------------------------------------------------------------------------
    private static void CopyFilesToPackageDirectory(
        string packageFolder, 
        List<FileInfoMatchingResult> fileInfoForFilesToCompress)
    {
        PackageFileUtils.CopyFilesToDirectory(
            packageFolder, 
            fileInfoForFilesToCompress, 
            WriteVersionInfo);
    }

    //-------------------------------------------------------------------------
    private static void WriteVersionInfo(string destPath)
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
            string newContents = contents.Substring(0, startIndex) 
                + newFunction 
                + contents.Substring(endIndex);

            File.WriteAllText(destPath, newContents);
        }
    }

    

    //-------------------------------------------------------------------------
    public static void CreateUPMPackage(
        string outputPath, 
        string pathToJSONPackageDescription)
    {
        UnityEngine.Debug.Log("DEBUG " + pathToJSONPackageDescription);
        var JSONPackageDescription = File.ReadAllText(
            pathToJSONPackageDescription
            );
        var packageDescription = JsonUtility.FromJson<PackageDescription>(
            JSONPackageDescription);
        string packageFolder = GetPackageOutputFolder();
        var filesToCompress = PackageFileUtils.GetFileInfoMatchingPackageDescription(
            "./",
            packageDescription);

        EditorUtility.DisplayProgressBar(
            "PEW Package Tool", 
            "Copying files...",
            0.3f
            );

        CopyFilesToPackageDirectory(
            packageFolder, 
            filesToCompress
            );

        if (!ExecutorInstance)
        {
            ExecutorInstance = UnityEngine.Object.FindObjectOfType<
                CoroutineExecutor>();

            if (!ExecutorInstance)
            {
                ExecutorInstance = new GameObject(
                    "CoroutineExecutor").AddComponent<CoroutineExecutor>();
            }
        }

        ExecutorInstance.StartCoroutine(
            ClientMakePackage(
                packageFolder, 
                outputPath
                )
            );
    }

    //-------------------------------------------------------------------------
    public static void CreateLegacyUnityPackage(
        string outputPath, 
        string pathToJSONPackageDescription, 
        string packageName = "pew_eos_plugin.unitypackage")
    {
        var JSONPackageDescription = File.ReadAllText(
            pathToJSONPackageDescription);

        var packageDescription = JsonUtility.FromJson<PackageDescription>(
            JSONPackageDescription);

        // Transform PackageDescription into a list of actual files that can be
        // copied to a directory that can be zipped 
        string gzipFilePathName = Path.Combine(outputPath, packageName);

        List<string> filesToCompress = PackageFileUtils.GetFilePathsMatchingPackageDescription(
            "./",
            packageDescription
        );

        var toExport = filesToCompress.Where(
            (path) => { return !path.Contains(".meta"); }
            ).ToArray();

        var options = ExportPackageOptions.Interactive;

        AssetDatabase.ExportPackage(toExport, gzipFilePathName, options);        
    }

    public static void CopyFilesInPackageDescriptionToBuildDir(
        string pathToJSONPackageDescription)
    {
        var packageDescription = ReadPackageDescription(
            pathToJSONPackageDescription);

        var filesToCopy = PackageFileUtils.GetFileInfoMatchingPackageDescription(
            "./",
            packageDescription);

        CopyFilesToPackageDirectory(
            pathToJSONPackageDescription, filesToCopy);
    }

    //-------------------------------------------------------------------------
    // Helper coroutine for making the client package.
    private static IEnumerator ClientMakePackage(
        string packageFolder, 
        string outputPath)
    {   
        packRequest = UnityEditor.PackageManager.Client.Pack(
            packageFolder, outputPath);

        while (!packRequest.IsCompleted)
        {
            yield return null;
        }

        if (packRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
        {
            if (packRequest.Error != null)
            {
                throw new BuildFailedException(
                    "Error making package " + packRequest.Error.message);
            }
        } 
    }
}
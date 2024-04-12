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
using UPMUtility = PlayEveryWare.EpicOnlineServices.Editor.Utility.UnityPackageCreationUtility;

using System;

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using Build;
    using Config;
    using EpicOnlineServices.Utility;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Utility;
    using Config = EpicOnlineServices.Config;

    [Serializable]
    public class ModularPackageWindow : EOSEditorWindow
    {
        private const string DefaultPackageDescription = "etc/PackageConfigurations/eos_package_description.json";

        // TODO: Re-enable the following fields once their values are actually utilized
        //       in the package creation process.
        //[RetainPreference("CleanBeforeCreate")]
        //private bool _cleanBeforeCreate = true;

        //[RetainPreference("IgnoreGitWhenCleaning")]
        //private bool _ignoreGitWhenCleaning = true;

        private PackagingConfig _packagingConfig;

        private CancellationTokenSource _createPackageCancellationTokenSource;

        private bool _operationInProgress;
        private float _progress;
        private string _progressText;

        public static Dictionary<string, string> platformDescDict = new Dictionary<string, string>();
        public static Dictionary<string, bool> isPackageExported = new Dictionary<string, bool>();
        public static Dictionary<string, string> identifierPath = new Dictionary<string, string>();

        public static Dictionary<string, List<string>> packagePlatformsDict = new Dictionary<string, List<string>>();

        [MenuItem("Tools/EOS Plugin/Create Package Modularly")]
        public static void ShowWindow()
        {
            GetWindow<ModularPackageWindow>("Create Package Modularly");
        }

        protected override async Task AsyncSetup()
        {
            _packagingConfig = await Config.GetAsync<PackagingConfig>();

            if (string.IsNullOrEmpty(_packagingConfig.pathToJSONPackageDescription))
            {
                _packagingConfig.pathToJSONPackageDescription =
                    Path.Combine(FileUtility.GetProjectPath(), DefaultPackageDescription);
                await _packagingConfig.WriteAsync();
            }
            SetAutoResize(false);
            await base.AsyncSetup();

            ReloadPackageDescriptions();
        }

        protected static bool SelectOutputDirectory(ref string path)
        {
            string selectedPath = EditorUtility.OpenFolderPanel(
                "Pick output directory",
                Path.GetDirectoryName(FileUtility.GetProjectPath()),
                "");

            if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                return false;
            }

            path = selectedPath;
            return true;
        }

        protected override void Teardown()
        {
            base.Teardown();

            if (_createPackageCancellationTokenSource != null)
            {
                _createPackageCancellationTokenSource.Cancel();
                _createPackageCancellationTokenSource.Dispose();
                _createPackageCancellationTokenSource = null;
            }
        }

        protected override void RenderWindow()
        {
            if (_operationInProgress)
            {
                GUI.enabled = false;
            }

            foreach (var package in packagePlatformsDict)
            {
                GUILayout.BeginHorizontal();

                isPackageExported[package.Key] = GUILayout.Toggle(isPackageExported[package.Key], package.Key, "button", GUILayout.MaxWidth(100));

                foreach (var platform in package.Value)
                {
                    GUILayout.Label(platform, GUILayout.MaxWidth(80));
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            var outputPath = _packagingConfig.pathToOutput;
            GUIEditorUtility.AssigningTextField("Output Path", ref outputPath, 100f);
            if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
            {
                if (SelectOutputDirectory(ref outputPath))
                {
                    _packagingConfig.pathToOutput = outputPath;
                    _packagingConfig.Write();
                }
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Build All Selected"))
            {
                foreach (var package in packagePlatformsDict)
                {
                    if (isPackageExported[package.Key])
                    {
                        string tempPath = FileUtility.GenerateTempDirectory();
                        CreateUPMTarballModularly(_packagingConfig.pathToOutput, tempPath, package.Key);
                    }
                }
            }
        }

        public static void ReloadPackageDescriptions()
        {
            string pathToExportDescDirectory = Application.dataPath + "/../etc/PackageConfigurations/";

            platformDescDict.Clear();
            packagePlatformsDict.Clear();
            isPackageExported.Clear();
            identifierPath.Clear();

            var JSONPackageDescription = File.ReadAllText(pathToExportDescDirectory + "eos_platform_export_info_list.json");
            var exportInfoList = JsonUtility.FromJson<PlatformExportInfoList>(JSONPackageDescription);

            foreach (var entry in exportInfoList.platformExportInfoList)
            {
                platformDescDict[entry.platform] = pathToExportDescDirectory + entry.descPath;
            }

            var presetDescription = File.ReadAllText(pathToExportDescDirectory + "eos_package_export_preset_list.json");
            var presetList = JsonUtility.FromJson<PackagePresetList>(presetDescription);

            foreach (var entry in presetList.packagePresetList)
            {
                packagePlatformsDict[entry.name] = entry.platformList;
                isPackageExported[entry.name] = false;
                identifierPath[entry.name] = entry.packageIdentifierPath;
            }
        }

        public void CreateUPMTarballModularly(string outputPath, string tempPath, string preset)
        {
            foreach (var subset in packagePlatformsDict[preset])
            {
                string json_file = platformDescDict[subset];
                if (!File.Exists(json_file))
                {
                    Debug.LogError($"Could not read package description file \"{json_file}\", it does not exist.");
                    return;
                }

                PackageDescription packageDescription;
                try
                {
                    packageDescription = UPMUtility.ReadPackageDescription(json_file);
                }
                catch
                {
                    Debug.LogError($"JSON syntax error in file: \"{json_file}\". See log for details.");
                    return;
                }

                var filesToCopy = PackageFileUtility.FindPackageFiles(
                    FileUtility.GetProjectPath(),
                    packageDescription
                    );

                CopyFilesToDirectorySequential(tempPath, filesToCopy, null);
                
            }

            File.Copy(identifierPath[preset], Path.Combine(tempPath, "package.json"));
            File.Copy(identifierPath[preset] + ".meta", Path.Combine(tempPath, "package.json.meta"));

            if (!UPMUtility.executorInstance)
            {
                UPMUtility.executorInstance = UnityEngine.Object.FindObjectOfType<
                    CoroutineExecutor>();

                if (!UPMUtility.executorInstance)
                {
                    UPMUtility.executorInstance = new GameObject(
                        "CoroutineExecutor").AddComponent<CoroutineExecutor>();
                }
            }

            UPMUtility.executorInstance.StartCoroutine(
                UPMUtility.StartMakingTarball(
                    tempPath,
                    outputPath
                    )
                );
        }

        private void CopyFilesToDirectorySequential(string tempPath, List<FileInfoMatchingResult> filesToCopy, Action<string> p)
        {
            CopyFilesToDirectory(
                tempPath,
                filesToCopy,
                p);
        }
        public static void CopyFilesToDirectory(string packageFolder, List<FileInfoMatchingResult> fileInfoForFilesToCompress, Action<string> postProcessCallback = null)
        {
            Directory.CreateDirectory(packageFolder);


            foreach (var fileInfo in fileInfoForFilesToCompress)
            {
                FileInfo src = fileInfo.fileInfo;
                string dest = fileInfo.GetDestination();

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

                if (fileInfo.originalSrcDestPair.copy_identical || !FilesAreEqual(new FileInfo(src.FullName), new FileInfo(destPath)))
                {
                    File.Copy(src.FullName, destPath, true);
                }
                postProcessCallback?.Invoke(destPath);
            }
        }

        const int BYTES_TO_READ = sizeof(Int64); //check 4 bytes at a time
        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Exists != second.Exists)
                return false;

            if (!first.Exists && !second.Exists)
                return true;

            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }
    }
}

/*
* Copyright (c) 2024 PlayEveryWare
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
using UnityEditor.Build;

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    using Build;
    using Config;
    using EpicOnlineServices.Utility;
    using System;
    using System.Threading.Tasks;
    using Config = EpicOnlineServices.Config;

    // Helper to allow for StartCoroutine to be used from a static context
    public class CoroutineExecutor : MonoBehaviour { }

    public static class UnityPackageCreationUtility
    {
        /// <summary>
        /// Defines the different kinds of packages that can be created.
        /// </summary>
        public enum PackageType
        {
            /// <summary>
            /// UPM is just a directory with the proper structure and a package.json file.
            /// </summary>
            UPM,
            
            /// <summary>
            /// UPMTarball is the same as UPM, except it is compressed into a tarball.
            /// </summary>
            UPMTarball,

            /// <summary>
            /// DotUnity creates a .unitypackage formatted package.
            /// </summary>
            DotUnity
        };

        /// <summary>
        /// This is where we will store the request we sent to Unity to make the
        /// package.
        /// </summary>
        public static UnityEditor.PackageManager.Requests.PackRequest packRequest;

        /// <summary>
        /// This is used in order to use StartCoroutine from a static context.
        /// </summary>
        public static CoroutineExecutor executorInstance;
        
        private static PackageDescription ReadPackageDescription(string pathToJSONPackageDescription)
        {
            var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);

            var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

            return packageDescription;
        }

        private static void CopyFilesToPackageDirectory(string packageFolder,
            List<FileInfoMatchingResult> fileInfoForFilesToCompress)
        {
            PackageFileUtility.CopyFilesToDirectory(
                packageFolder,
                fileInfoForFilesToCompress,
                WriteVersionInfo);
        }

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
        return """ + version + @""";
    }
    ";
                string newContents = contents.Substring(0, startIndex)
                                     + newFunction
                                     + contents.Substring(endIndex);

                File.WriteAllText(destPath, newContents);
            }
        }

        private static void CreateUPMTarball(string outputPath, string json_file)
        {
            string tempOutput = PackageFileUtility.GenerateTemporaryBuildPath();
            
            CreateUPM(tempOutput, json_file);
            
            if (!executorInstance)
            {
                executorInstance = UnityEngine.Object.FindObjectOfType<
                    CoroutineExecutor>();

                if (!executorInstance)
                {
                    executorInstance = new GameObject(
                        "CoroutineExecutor").AddComponent<CoroutineExecutor>();
                }
            }

            executorInstance.StartCoroutine(
                StartMakingTarball(
                    tempOutput,
                    outputPath
                )
            );
        }
        
        private static void CreateDotUnityPackage(string outputPath, string json_file,
            string packageName = "pew_eos_plugin.unitypackage")
        {
            var JSONPackageDescription = File.ReadAllText(json_file);

            var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

            // Transform PackageDescription into a list of actual files that can be
            // copied to a directory that can be zipped 
            string gzipFilePathName = Path.Combine(outputPath, packageName);

            List<string> filesToCompress =
                PackageFileUtility.GetFilePathsMatchingPackageDescription("./", packageDescription);

            var toExport = filesToCompress.Where(
                (path) => { return !path.Contains(".meta"); }
            ).ToArray();

            var options = ExportPackageOptions.Interactive;

            AssetDatabase.ExportPackage(toExport, gzipFilePathName, options);
        }

        private static void CreateUPM(string outputPath, string json_file)
        {
            if (!File.Exists(json_file))
            {
                Debug.LogError($"Could not read package description file \"{json_file}\", it does not exist.");
                return;
            }
            var packageDescription = ReadPackageDescription(json_file);

            var filesToCopy = PackageFileUtility.GetFileInfoMatchingPackageDescription("./", packageDescription);

            CopyFilesToPackageDirectory(outputPath, filesToCopy);
        }

        public static async Task CreatePackage(PackageType packageType, bool clean = true, bool ignoreGit = true)
        {
            var packagingConfig = await Config.Get<PackagingConfig>();

            if (clean)
            {
                FileUtility.CleanDirectory(packagingConfig.pathToOutput, ignoreGit);
            }

            switch (packageType)
            {
                case PackageType.UPM:
                    CreateUPM(packagingConfig.pathToOutput, packagingConfig.pathToJSONPackageDescription);
                    break;
                case PackageType.UPMTarball:
                    CreateUPMTarball(packagingConfig.pathToOutput, packagingConfig.pathToJSONPackageDescription);
                    break;
                case PackageType.DotUnity: // Deprecated
                default:
                    throw new ArgumentOutOfRangeException(nameof(packageType), packageType, null);
            }
        }

        // Helper coroutine for making the client package.
        private static IEnumerator StartMakingTarball(string packageFolder, string outputPath)
        {
            packRequest = UnityEditor.PackageManager.Client.Pack(packageFolder, outputPath);

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

            // Delete the packageFolder - as when making a tarball it is only a temporary directory
            Directory.Delete(packageFolder, true);
        }
    }
}
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
    using System.Threading;
    using System.Threading.Tasks;
    using Config = EpicOnlineServices.Config;

    // Helper to allow for StartCoroutine to be used from a static context
    public class CoroutineExecutor : MonoBehaviour { }

    internal static class UnityPackageCreationUtility
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
        
        public static async Task CreatePackage(PackageType packageType, bool clean = false, IProgress<FileSystemUtility.CopyFileProgressInfo> progress = null, CancellationToken cancellationToken = default)
        {
            var packagingConfig = await Config.GetAsync<PackagingConfig>();

            if (clean)
            {
	            FileSystemUtility.CleanDirectory(packagingConfig.pathToOutput, true);
            }

            switch (packageType)
            {
                case PackageType.UPM:
                    await CreateUPM(packagingConfig.pathToOutput, packagingConfig.pathToJSONPackageDescription, progress, cancellationToken);
                    break;
                case PackageType.UPMTarball:
                    await CreateUPMTarball(packagingConfig.pathToOutput, packagingConfig.pathToJSONPackageDescription, progress, cancellationToken);
                    break;
                case PackageType.DotUnity: // Deprecated
                default:
                    throw new ArgumentOutOfRangeException(nameof(packageType), packageType, null);
            }

            // Validate the package inasmuch as possible.
            ValidatePackage(packagingConfig.pathToOutput);
        }

        private static async Task CreateUPM(string outputPath, string json_file, IProgress<FileSystemUtility.CopyFileProgressInfo> progress, CancellationToken cancellationToken)
        {
            /*
             * NOTES:
             *
             * A preliminary step that can be automated is that the README.md, CHANGELOG.md, and LICENSE.md files
             * are supposed to be exported to the root directory of the UPM, and they need to have .meta files,
             * but these files are not stored within the Assets directory, so Unity doesn't generate them for us.
             *
             * Options to resolve this are myriad, but currently the process is manual.
             *
             */
            if (!File.Exists(json_file))
            {
                Debug.LogError($"Could not read package description file \"{json_file}\", it does not exist.");
                return;
            }

            PackageDescription packageDescription = JsonUtility.FromJsonFile<PackageDescription>(json_file);

            var filesToCopy = PackageFileUtility.FindPackageFiles(
                FileSystemUtility.GetProjectPath(),
                packageDescription
            );

            await PackageFileUtility.CopyFilesToDirectory(outputPath, filesToCopy, progress, cancellationToken);
        }

        private static async Task CreateUPMTarball(string outputPath, string json_file,
            IProgress<FileSystemUtility.CopyFileProgressInfo> progress, CancellationToken cancellationToken)
        {
            if (!FileSystemUtility.TryGetTempDirectory(out string tempOutput))
            {
                throw new BuildFailedException(
                    "Could not create temporary directory into which to place files for compression.");
            }

            await CreateUPM(tempOutput, json_file, progress, cancellationToken);

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

        /// <summary>
        /// Given the path to an exported package, run some basic checks and log warnings around any potential issues that can be determined.
        /// </summary>
        /// <param name="packagePath">Path to exported package.</param>
        private static void ValidatePackage(string packagePath)
        {
            FileSystemUtility.NormalizePath(ref packagePath);

            // Get all entries.
            var allEntries = Directory.GetFileSystemEntries(packagePath);

            foreach (var entry in allEntries)
            {
                // Skip if the entry is a meta file.
                if (File.Exists(entry) && Path.GetExtension(entry) == ".meta") { continue; }

                // If the entry contains a "~", then it's special and doesn't need a meta file.
                if (entry.Contains('~')) { continue; }

                // Otherwise - check to make sure that there is a meta file that corresponds to the file system entry.
                if (!allEntries.Contains($"{entry}.meta"))
                {
                    Debug.LogWarning($"Item \"{entry}\" in output package directory \"{packagePath}\" does not have a corresponding .meta file. This will likely cause errors when the package is subsequently imported.");
                }
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
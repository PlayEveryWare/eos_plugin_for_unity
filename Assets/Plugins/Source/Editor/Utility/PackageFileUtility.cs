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

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using Editor.Build;
    using Extensions;
    using System.Linq;
    using Editor.Utility;
    using System.Threading;
    using System.Threading.Tasks;

    public class PackageFileUtility
    {
        /// <summary>
        /// Interval with which to update progress UI when there are progresses to report.
        /// </summary>
        private const double UpdateProgressIntervalInSeconds = 1.5;

        /// <summary>
        /// Generates a unique and new temporary directory inside the Temporary Cache Path as determined by Unity,
        /// and returns the fully-qualified path to the newly created directory.
        /// </summary>
        /// <returns>Fully-qualified file path to the newly generated directory.</returns>
        public static bool TryGetTempDirectory(out string path)
        {
            // Generate a temporary directory path.
            string tempPath = Path.Combine(Application.temporaryCachePath, $"Output-{Guid.NewGuid()}/");

            // If (by some crazy miracle) the directory path already exists, keep generating until there is a new one.
            if (Directory.Exists(tempPath))
            {
                Debug.LogWarning(
                    $"The temporary directory created collided with an existing temporary directory of the same name. This is very unlikely.");
                tempPath = Path.Combine(Application.temporaryCachePath, $"Output-{Guid.NewGuid()}/");

                if (Directory.Exists(tempPath))
                {
                    Debug.LogError(
                        $"When generating a temporary directory, the temporary directory generated collided twice with already existing directories of the same name. This is very unlikely.");
                    path = null;
                    return false;
                }
            }

            try
            {
                // Create the directory.
                var dInfo = Directory.CreateDirectory(tempPath);

                // Make sure the directory exists.
                if (!dInfo.Exists)
                {
                    Debug.LogError($"Could not generate temporary directory.");
                    path = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not generate temporary directory: {e.Message}");
                path = null;
                return false;
            }

            // return the fully-qualified path to the newly created directory.
            path = Path.GetFullPath(tempPath);
            return true;
        }


        /// <summary>
        /// Generates a list of FileInfoMatchingResults that represent the contents of the package to create.
        /// </summary>
        /// <param name="root">Where to search for the files to create the package out of.</param>
        /// <param name="packageDescription">The package description.</param>
        /// <returns>List of FileInfoMatchingResults that represent the contents of the package to create.</returns>
        public static List<FileInfoMatchingResult> FindPackageFiles(string root, PackageDescription packageDescription)
        {
            List<FileInfoMatchingResult> fileInfos = new();
            List<SrcDestPair> ignoreList = new();

            // TODO: Replace the path separator logic with system-specific things / methods provided by Mono.
            string currentWorkingDir = Path.GetFullPath(Directory.GetCurrentDirectory()).Replace('\\', '/') + "/";

            // Iterate through the SrcDestPair entries that are not merely comments.
            foreach (var srcDestPair in packageDescription.source_to_dest.Where(p => !p.IsCommentOnly()))
            {
                // If the SrcDestPair has an ignore_regex, then add it to the list of pairs that are to be interpreted as ignore patterns,
                // and move on to the next SrcDestPair.
                if (!string.IsNullOrEmpty(srcDestPair.ignore_regex))
                {
                    ignoreList.Add(srcDestPair);
                    continue;
                }

                // If the source file exists, and if the SrcDestPair has a sha1 value, AND if that sha1 value no longer matches, then
                // log a warning / error indicating that there is a SHA mismatch. (This is typically to help when binaries need updating).
                var srcFileInfo = new FileInfo(srcDestPair.src);
                if (srcFileInfo.Exists && !string.IsNullOrEmpty(srcDestPair.sha1) && srcFileInfo.ComputeSHA() != srcDestPair.sha1)
                {
                    // If there is a SHA mismatch, and if there is a unique error message added to the SrcDestPair, then make certain
                    // to utilize it when logging the warning, as it likely has information pertinent to the developer.
                    string errorMessageToUse = string.IsNullOrEmpty(srcDestPair.sha1_mismatch_error)
                        ? "SHA1 mismatch"                  // Use a standard error message.
                        : srcDestPair.sha1_mismatch_error; // Use the error message in the Json.

                    Debug.LogWarning(
                        $"Copy error for file \"{srcDestPair.src}\": {errorMessageToUse}");
                }

                var matchingFiles = FindMatchingFiles(root, currentWorkingDir, srcDestPair);

                if (null == matchingFiles)
                {
                    // Log a warning indicating that no matching files were found for the srcDestPair
                    Debug.LogWarning($"Source \"{srcDestPair.src}\" did not match any files.");
                }
                else
                {
                    fileInfos.AddRange(matchingFiles);
                }
            }

            // Prune the list of fileInfos to remove any entries that should be ignored according to the ignore list.
            fileInfos = fileInfos.FindAll((e) => {
                foreach (var ignorePattern in ignoreList)
                {
                    // TODO: Replace the path separator logic with system-specific things / methods provided by Mono.
                    var regex = new Regex(ignorePattern.ignore_regex.Replace(@"\\", @"\"));
                    var normalizedPath = e.fileInfo.FullName.Replace('\\', '/').Replace(currentWorkingDir, "");
                    if (regex.IsMatch(normalizedPath))
                    {
                        return false;
                    }
                }
                return true;
            });

            return fileInfos;
        }

        /// <summary>
        /// Given a root directory, the current working directory, and a given SrcDestPair, find all the matching files.
        /// </summary>
        /// <param name="root">The root path within which to find matching files.</param>
        /// <param name="currentWorkingDir">
        /// The current working directory
        /// TODO: Document why this is a needed parameter, needing to keep track of current working directory seems like
        ///       a strange state to keep track of while accomplishing this task.
        /// </param>
        /// <param name="pair">The SrcDestPair to find the matching files for.</param>
        /// <returns>An IEnumerable of all the FileInfoMatchingResult structs that represent the files that match the SrcDestPair given the root directory and the current working directory.</returns>
        private static IEnumerable<FileInfoMatchingResult> FindMatchingFiles(string root, string currentWorkingDir, SrcDestPair pair)
        {
            IEnumerable<string> collectedFiles;
            SearchOption searchOption = pair.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            string searchPattern = pair.src;
            string path = root;

            if (!string.IsNullOrEmpty(pair.pattern))
            {
                searchPattern = pair.pattern;
                path = Path.Combine(root, pair.src);
            }

            try
            {
                collectedFiles = Directory.EnumerateFiles(path, searchPattern, searchOption);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error enumerating files at \"{root}\": \"{e.Message}\".");
                throw;
            }

            foreach (var entry in collectedFiles)
            {
                FileInfo srcItem = new(Path.GetFullPath(entry).Replace('\\', '/').Replace(currentWorkingDir, ""));
                var newItem = new FileInfoMatchingResult();
                if (pair.recursive && Directory.Exists(Path.Combine(root, pair.src)))
                {
                    newItem.relativePath = Path.GetRelativePath(Path.Combine(root, pair.src), entry);
                }
                newItem.fileInfo = srcItem;
                newItem.originalSrcDestPair = pair;

                yield return newItem;
            }
        }

        /// <summary>
        /// Determine the file operations to perform in order to create the package.
        /// </summary>
        /// <param name="destination">The destination at which to create the package.</param>
        /// <param name="matchingResults">The matching results, determined by evaluating the package description json file.</param>
        /// <param name="directoriesToCreate">The directories to create before copy files.</param>
        /// <param name="filesToCopy">The file copy operations that need to take place to create the package.</param>
        private static void GetFileSystemOperations(
            string destination,
            List<FileInfoMatchingResult> matchingResults,
            out List<string> directoriesToCreate,
            out List<(string from, string to, long size)> filesToCopy)
        {
            filesToCopy = new();
            directoriesToCreate = new();

            foreach (var file in matchingResults)
            {
                FileInfo src = file.fileInfo;
                string dest = file.GetDestination();

                string finalDestinationPath = Path.Combine(destination, dest);
                string finalDestinationParent = Path.GetDirectoryName(finalDestinationPath);
                bool isDestinationADirectory = dest.EndsWith("/") || dest.Length == 0;

                if (!string.IsNullOrEmpty(finalDestinationParent) && !Directory.Exists(finalDestinationParent))
                {
                    directoriesToCreate.Add(finalDestinationParent);
                }

                if (!Directory.Exists(finalDestinationPath) && isDestinationADirectory)
                {
                    directoriesToCreate.Add(finalDestinationPath);
                }

                string destPath = isDestinationADirectory ? Path.Combine(finalDestinationPath, src.Name) : finalDestinationPath;

                if (file.originalSrcDestPair.copy_identical || !src.AreContentsSemanticallyEqual(new FileInfo(destPath)))
                {
                    filesToCopy.Add((src.FullName, destPath, src.Length));
                }
            }

            // Order the directories by length of path, and make the list unique.
            directoriesToCreate = directoriesToCreate.Distinct().OrderBy(d => d.Length).ToList();
        }

        /// <summary>
        /// Run the copy operations, reporting the progress.
        /// </summary>
        /// <param name="operations">File copy operations to perform.</param>
        /// <param name="progress">Progress reporter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task</returns>
        private static async Task RunCopyOperations(
            List<(string from, string to, long size)> operations,
            IProgress<UnityPackageCreationUtility.CreatePackageProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            // Determine the total size of files that need to be copied.
            long sizeOfFilesToCopy = operations.Sum(op => op.size);

            // Used to measure how long it's been since the progress has been reported.
            DateTime progressLastUpdated = DateTime.Now;

            // Used to keep track of how many files have been copied.
            int filesCopied = 0;

            // Keeps track of how many bytes have been copied.
            long sizeOfCopiedFiles = 0L;

            // Execute each file copy operation.
            foreach ((string from, string to, long size) in operations)
            {
                // Make sure to throw an exception if a cancellation was requested of the token.
                cancellationToken.ThrowIfCancellationRequested();

                // Run the file copy asynchronously, passing on the cancellation token.
                await Task.Run(() => File.Copy(from, to, true), cancellationToken);

                // Increment the number of files copied.
                filesCopied++;

                // Update the number of bytes that have been copied.
                sizeOfCopiedFiles += size;

                if (null == progress ||
                    (DateTime.Now - progressLastUpdated).TotalSeconds < UpdateProgressIntervalInSeconds)
                {
                    continue;
                }

                progressLastUpdated = DateTime.Now;
                progress.Report(new UnityPackageCreationUtility.CreatePackageProgressInfo()
                {
                    FilesCopied = filesCopied,
                    TotalFilesToCopy = operations.Count,
                    SizeOfFilesCopied = sizeOfCopiedFiles,
                    TotalSizeOfFilesToCopy = sizeOfFilesToCopy
                });
            }
        }

        /// <summary>
        /// Copies files to a directory to create a UPM package. Directory structure to create is inferred.
        /// </summary>
        /// <param name="destination">The destination into which to copy the files.</param>
        /// <param name="matchingResults">The list of files that represent the contents of the package to create.</param>
        /// <param name="progress">Used to report progress to UI.</param>
        /// <param name="cancellationToken">Used to cancel the operation if requested.</param>
        /// <param name="postProcessCallback">Method to call when files have been copied.</param>
        /// <returns>Task</returns>
        public static async Task CopyFilesToDirectory(
            string destination,
            List<FileInfoMatchingResult> matchingResults,
            IProgress<UnityPackageCreationUtility.CreatePackageProgressInfo> progress = null,
            CancellationToken cancellationToken = default,
            Action<string> postProcessCallback = null)
        {
            // TODO: A major improvement to this system would be to automatically include .meta files, and gracefully
            //       handle instances where the meta file *should* exist, but does not - gracefully in this context
            //       would mean that if the .meta file exists, it should just be copied over; whereas if there is a
            //       file entry copied over that does not have a corresponding meta file in the source, it indicates
            //       that a meta file entry is MISSING from the source, and that should stop the creation of a package.


            Directory.CreateDirectory(destination);

            GetFileSystemOperations(destination, matchingResults, out List<string> directoriesToCreate, out List<(string from, string to, long size)> copyOperations);

            // Create the directory structure
            foreach (string directory in directoriesToCreate)
            {
                var dInfo = Directory.CreateDirectory(directory);
                if (!dInfo.Exists)
                {
                    Debug.LogWarning($"Could not create directory \"{directory}\".");
                }
            }

            // Copy the files
            await RunCopyOperations(copyOperations, progress, cancellationToken);

            // Execute callback
            postProcessCallback?.Invoke(destination);
        }
    }
}
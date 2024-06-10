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
    using System.Threading;
    using System.Threading.Tasks;

    public class PackageFileUtility
    {
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
        /// <param name="filesToCopy">The file copy operations that need to take place to create the package.</param>
        private static void GetFileSystemOperations(
            string destination,
            List<FileInfoMatchingResult> matchingResults,
            out List<FileUtility.CopyFileOperation> filesToCopy)
        {
            filesToCopy = new();

            foreach (var file in matchingResults)
            {
                FileInfo src = file.fileInfo;
                string dest = file.GetDestination();

                string finalDestinationPath = Path.Combine(destination, dest);
                bool isDestinationADirectory = dest.EndsWith("/") || dest.Length == 0;

                string destPath = isDestinationADirectory ? Path.Combine(finalDestinationPath, src.Name) : finalDestinationPath;

                if (file.originalSrcDestPair.copy_identical || !src.AreContentsSemanticallyEqual(new FileInfo(destPath)))
                {
                    filesToCopy.Add(new()
                    {
                        SourcePath = src.FullName,
                        DestinationPath = destPath,
                        Bytes = src.Length
                    });
                }
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
            IProgress<FileUtility.CopyFileProgressInfo> progress = null,
            CancellationToken cancellationToken = default,
            Action<string> postProcessCallback = null)
        {
            // TODO: A major improvement to this system would be to automatically include .meta files, and gracefully
            //       handle instances where the meta file *should* exist, but does not - gracefully in this context
            //       would mean that if the .meta file exists, it should just be copied over; whereas if there is a
            //       file entry copied over that does not have a corresponding meta file in the source, it indicates
            //       that a meta file entry is MISSING from the source, and that should stop the creation of a package.


            Directory.CreateDirectory(destination);

            GetFileSystemOperations(
                destination, 
                matchingResults,
                out List<FileUtility.CopyFileOperation> copyOperations);

            // Copy the files
            await FileUtility.CopyFilesAsync(copyOperations, cancellationToken, progress);
            
            // Execute callback
            postProcessCallback?.Invoke(destination);
        }
    }
}
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

    internal class PackageFileUtility
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

            Stack<SrcDestPair> srcDestPairs = new(packageDescription.source_to_dest.Where(p => !p.IsCommentOnly()));

            // Iterate through the SrcDestPair entries that are not merely comments.
            while (srcDestPairs.Count != 0)
            {
                SrcDestPair srcDestPair = srcDestPairs.Pop();

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

                if (srcDestPair.recursive)
                {
                    string baseSource = srcDestPair.src[..^1];
                    string baseDest = srcDestPair.dest;

                    var subdirectories = Directory.EnumerateDirectories(baseSource, "*", SearchOption.AllDirectories);

                    foreach (var subdir in subdirectories)
                    {
                        SrcDestPair subDirectoryEntry = new()
                        {
                            src = Path.Join(subdir, "*"),
                            dest = Path.Join(baseDest, subdir[(baseSource.Length)..]) + "/"
                        };
                        srcDestPairs.Push(subDirectoryEntry);
                    }
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
            
            string searchPattern = pair.src;
            string path = root;

            if (!string.IsNullOrEmpty(pair.pattern))
            {
                searchPattern = pair.pattern;
                path = Path.Combine(root, pair.src);
            }

            try
            {
                collectedFiles = Directory.EnumerateFiles(path, searchPattern);
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
            IEnumerable<FileInfoMatchingResult> matchingResults,
            out List<FileSystemUtility.CopyFileOperation> filesToCopy)
        {
            filesToCopy = new();

            // Use a stack so that we can add more items to the collection as
            // we go along.
            Stack<FileInfoMatchingResult> matchingResultsStack = new(matchingResults);

            while (matchingResultsStack.Count > 0)
            {
                var file = matchingResultsStack.Pop();

                FileInfo src = file.fileInfo;
                string dest = file.GetDestination();

                string finalDestinationPath = Path.Combine(destination, dest);
                bool isDestinationADirectory = dest.EndsWith("/") || dest.Length == 0;

                string destPath = isDestinationADirectory ? Path.Combine(finalDestinationPath, src.Name) : finalDestinationPath;

                FileInfo destInfo = new(destPath);

                if (file.originalSrcDestPair.dest.Contains('~') && !file.originalSrcDestPair.dest.Contains("Samples~"))
                {
                    // When generating a upm package, all directories that contain a 
                    // tilde character at the end of the name are ignored by Unity's
                    // Asset pipeline, so we skip copying meta files for those.
                    // However, an exception to this rule is the Samples directory,
                    // because an imported project allows the importing of the contents
                    // of the samples directory - at which point the meta files become
                    // necessary.
                    if (".meta" == src.Extension)
                        continue;
                }
                else
                {
                    // If the destination path does not contain a tilde, and
                    // the source file does _not_ have a .meta extension, AND if
                    // the current file has a sibling that _does_ have a .meta
                    // extension, then we should copy that file additionally.
                    if (".meta" != src.Extension && File.Exists($"{src.FullName}.meta"))
                    {
                        FileInfoMatchingResult newResult = new()
                        {
                            fileInfo = new FileInfo($"{src.FullName}.meta"),
                            originalSrcDestPair = file.originalSrcDestPair
                        };
                        matchingResultsStack.Push(newResult);
                    }
                }

                // The file needs to be copied in the following circumstances:
                // 1. The file doesn't exist at the destination
                // 2. The field member copy_identical is true
                // 3. If the file contents are not semantically equal
                if (!destInfo.Exists || file.originalSrcDestPair.copy_identical || !src.AreContentsSemanticallyEqual(destInfo))
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
            IProgress<FileSystemUtility.CopyFileProgressInfo> progress = null,
            CancellationToken cancellationToken = default,
            Action<string> postProcessCallback = null)
        {
            Directory.CreateDirectory(destination);

            GetFileSystemOperations(
                destination, 
                matchingResults,
                out List<FileSystemUtility.CopyFileOperation> copyOperations);

            if (0 == copyOperations.Count)
            {
                Debug.LogWarning("There were no files that need to be moved to create the package (nothing seems to have changed).");
            }
            else
            {
                Debug.Log($"There are a total of {copyOperations.Count} files that need to be copied to update the package.");
            }

            // Copy the files
            await FileSystemUtility.CopyFilesAsync(copyOperations, cancellationToken, progress);
            
            // Execute callback
            postProcessCallback?.Invoke(destination);
        }
    }
}
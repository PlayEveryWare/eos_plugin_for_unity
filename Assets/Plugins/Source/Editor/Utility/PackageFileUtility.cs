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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using Editor;
    using Editor.Build;
    using Extensions;
    using System.Linq;
    using Editor.Utility;
    using System.Threading;
    using System.Threading.Tasks;

    public class PackageFileUtility
    {
        public static void Dos2UnixLineEndings(string srcFilename, string destFilename)
        {
            const byte CR = 0x0d;

            var fileAsBytes = File.ReadAllBytes(srcFilename);

            using (var filestream = File.OpenWrite(destFilename))
            {
                var writer = new BinaryWriter(filestream);
                int filePosition = 0;
                int indexOfDOSNewline = 0;

                do
                {
                    indexOfDOSNewline = Array.IndexOf<byte>(fileAsBytes, CR, filePosition);

                    if (indexOfDOSNewline >= 0)
                    {
                        writer.Write(fileAsBytes, filePosition, indexOfDOSNewline - filePosition);
                        filePosition = indexOfDOSNewline + 1;
                    }
                    else if (filePosition < fileAsBytes.Length)
                    {
                        writer.Write(fileAsBytes, filePosition, fileAsBytes.Length - filePosition);
                    }

                } while (indexOfDOSNewline > 0);

                // truncate trailing garbage.
                filestream.SetLength(filestream.Position);
            }
        }
        
        public static void Dos2UnixLineEndings(string filename)
        {
            Dos2UnixLineEndings(filename, filename);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root">Where the files start from</param>
        /// <param name="packageDescription"></param>
        /// <returns></returns>
        public static List<string> GetFilePathsMatchingPackageDescription(string root, PackageDescription packageDescription)
        {
            var filepaths = new List<string>();
            foreach(var srcToDestKeyValues in packageDescription.source_to_dest)
            {
                if (srcToDestKeyValues.IsCommentOnly() || (srcToDestKeyValues.comment != null && srcToDestKeyValues.comment.StartsWith("//")))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(srcToDestKeyValues.ignore_regex))
                {
                    continue;
                }

                SearchOption searchOption = srcToDestKeyValues.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var collectedFiles = Directory.EnumerateFiles(root, srcToDestKeyValues.src, searchOption);
                foreach (var entry in collectedFiles)
                {
                    if (root.StartsWith("./"))
                    {
                        // Remove the "./", as it makes the AssetDatabase.ExportPackage code break
                        filepaths.Add(entry.Remove(0, 2));
                    }
                    else
                    {
                        filepaths.Add(entry);
                    }
                }
            }

            return filepaths;
        }
        
        // Root is often "./"
        public static List<FileInfoMatchingResult> GetFileInfoMatchingPackageDescription(string root, PackageDescription packageDescription)
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
                
                fileInfos.AddRange(FindMatchingFiles(root, currentWorkingDir, srcDestPair));
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
            if (string.IsNullOrEmpty(pair.pattern))
            {
                collectedFiles = Directory.EnumerateFiles(root, pair.src, searchOption);
            }
            else
            {
                collectedFiles = Directory.EnumerateFiles(Path.Combine(root, pair.src), pair.pattern, searchOption);
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
        
        private static void Shuffle<T>(IList<T> list)
        {
            System.Random rng = new();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static async Task CopyFilesToDirectory(
            string packageFolder, 
            List<FileInfoMatchingResult> fileInfoForFilesToCompress, 
            IProgress<UnityPackageCreationUtility.CreatePackageProgressInfo> progress = null,
            CancellationToken cancellationToken = default,
            Action<string> postProcessCallback = null)
        {
            // TODO: A major improvement to this system would be to automatically include .meta files, and gracefully
            //       handle instances where the meta file *should* exist, but does not - gracefully in this context
            //       would mean that if the .meta file exists, it should just be copied over; whereas if there is a
            //       file entry copied over that does not have a corresponding meta file in the source, it indicates
            //       that a meta file entry is MISSING from the source, and that should stop the creation of a package.


            Directory.CreateDirectory(packageFolder);

            long sizeOfFilesToCopy = 0L;
            List<(string from, string to, long size)> fileCopyOperations = new();

            // First create the directory structure
            foreach (var fileInfo in fileInfoForFilesToCompress)
            {
                FileInfo src = fileInfo.fileInfo;
                string dest = fileInfo.GetDestination();

                string finalDestinationPath = Path.Combine(packageFolder, dest);
                string finalDestinationParent = Path.GetDirectoryName(finalDestinationPath);
                bool isDestinationADirectory = dest.EndsWith("/") || dest.Length == 0;

                if (!string.IsNullOrEmpty(finalDestinationParent) && !Directory.Exists(finalDestinationParent))
                {
                    Directory.CreateDirectory(finalDestinationParent);
                }

                // If it ends in a '/', treat it as a directory to move to
                if (!Directory.Exists(finalDestinationPath) && isDestinationADirectory)
                {
                    Directory.CreateDirectory(finalDestinationPath);
                }

                string destPath = isDestinationADirectory ? Path.Combine(finalDestinationPath, src.Name) : finalDestinationPath;

                // Ensure we can write over the dest path
                if (File.Exists(destPath))
                {
                    var destPathFileInfo = new System.IO.FileInfo(destPath);
                    destPathFileInfo.IsReadOnly = false;
                }

                if (fileInfo.originalSrcDestPair.copy_identical || !src.AreContentsSemanticallyEqual(new FileInfo(destPath)))
                {
                    fileCopyOperations.Add((src.FullName, destPath, src.Length));
                    sizeOfFilesToCopy += src.Length;
                }   
                
                postProcessCallback?.Invoke(destPath);
            }

            const float ProgressUpdateIntervalInSeconds = 1.5f;
            DateTime progressLastUpdated = DateTime.Now;

            int filesCopied = 0;

            // Shuffling the file copy operations makes the file copy task have a more even rate of progress
            // when the task is measured by number of bytes moved vs number of bytes that need to move.
            Shuffle(fileCopyOperations);

            long sizeOfCopiedFiles = 0L;
            foreach ((string from, string to, long size) in fileCopyOperations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                await Task.Run(() => File.Copy(from, to, true), cancellationToken);

                filesCopied++;
                sizeOfCopiedFiles += size;

                if (null != progress && (DateTime.Now - progressLastUpdated).TotalSeconds >= ProgressUpdateIntervalInSeconds)
                {
                    progress.Report(new UnityPackageCreationUtility.CreatePackageProgressInfo()
                    {
                        FilesCopied = filesCopied,
                        TotalFilesToCopy = fileCopyOperations.Count,
                        SizeOfFilesCopied = sizeOfCopiedFiles,
                        TotalSizeOfFilesToCopy = sizeOfFilesToCopy
                    });
                }
            }
        }
    }
}

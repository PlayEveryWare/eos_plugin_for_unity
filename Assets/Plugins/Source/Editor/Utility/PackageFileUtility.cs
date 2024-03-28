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
            var fileInfos = new List<FileInfoMatchingResult>();
            List<SrcDestPair> ignoreList = new List<SrcDestPair>();
            string currentWorkingDir = Path.GetFullPath(Directory.GetCurrentDirectory()).Replace('\\', '/') + "/";

            var toolsSection = new ToolsConfigEditor();
            toolsSection?.Load();

            foreach (var srcToDestKeyValues in packageDescription.source_to_dest)
            {
                if (srcToDestKeyValues.IsCommentOnly() || (srcToDestKeyValues.comment != null && srcToDestKeyValues.comment.StartsWith("//")))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(srcToDestKeyValues.ignore_regex))
                {
                    ignoreList.Add(srcToDestKeyValues);
                    continue;
                }

                SearchOption searchOption = srcToDestKeyValues.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var srcFileInfo = new FileInfo(srcToDestKeyValues.src);

                // Find instead the part of the path that does exist on disk
                if (!srcFileInfo.Exists)
                {
                    srcFileInfo = new FileInfo(srcFileInfo.DirectoryName);
                }
                else
                {
                    if (!string.IsNullOrEmpty(srcToDestKeyValues.sha1))
                    {
                        string computedSHA = "";

                        using (SHA1 fileSHA = SHA1.Create())
                        {
                            byte[] computedHash = null;

                            using (var srcFileStream = srcFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                srcFileStream.Position = 0;
                                computedHash = fileSHA.ComputeHash(srcFileStream);
                            }
                            StringBuilder formatedStr = new StringBuilder(computedHash.Length * 2);
                            foreach (byte b in computedHash)
                            {
                                formatedStr.AppendFormat("{0:x2}", b);
                            }
                            computedSHA = formatedStr.ToString();
                        }
                        if (computedSHA != srcToDestKeyValues.sha1)
                        {
                            string errorMessageToUse = string.IsNullOrEmpty(srcToDestKeyValues.sha1_mismatch_error) ? "SHA1 mismatch" : srcToDestKeyValues.sha1_mismatch_error;
                            Debug.LogWarning("Copy error for file (" + srcToDestKeyValues.src + ") :" + srcToDestKeyValues.sha1_mismatch_error);
                        }
                    }
                }

                IEnumerable<string> collectedFiles;

                if (string.IsNullOrEmpty(srcToDestKeyValues.pattern))
                {
                    collectedFiles = Directory.EnumerateFiles(root, srcToDestKeyValues.src, searchOption);
                }
                else
                {
                    collectedFiles = Directory.EnumerateFiles(Path.Combine(root, srcToDestKeyValues.src), srcToDestKeyValues.pattern, searchOption);
                }

                foreach (var entry in collectedFiles)
                {
                    FileInfo srcItem = new FileInfo(Path.GetFullPath(entry).Replace('\\', '/').Replace(currentWorkingDir,""));
                    var newItem = new FileInfoMatchingResult();
                    if (srcToDestKeyValues.recursive && Directory.Exists(Path.Combine(root, srcToDestKeyValues.src)))
                    {
                        newItem.relativePath = Path.GetRelativePath(Path.Combine(root, srcToDestKeyValues.src), entry);
                    }
                    newItem.fileInfo = srcItem;
                    newItem.originalSrcDestPair = srcToDestKeyValues;

                    fileInfos.Add(newItem);
                }

            }

            fileInfos = fileInfos.FindAll((e) => {
                foreach (var ignorePattern in ignoreList)
                {
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

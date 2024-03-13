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
    using Editor;
    using Editor.Build;
    using Extensions;

    public class PackageUtility
    {
        /// <summary>
        /// Gets all the filepaths that match the given package description.
        /// </summary>
        /// <param name="root">Where the files start from.</param>
        /// <param name="packageDescription">The description for the package.</param>
        /// <returns>A list of all the file paths in the root that match the given package description.</returns>
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

                if (srcFileInfo.Exists &&
                    !string.IsNullOrEmpty(srcToDestKeyValues.sha1) && 
                    srcFileInfo.CalculateSHA1() != srcToDestKeyValues.sha1)
                {
                    string errorMessageToUse = string.IsNullOrEmpty(srcToDestKeyValues.sha1_mismatch_error) ? "SHA1 mismatch" : srcToDestKeyValues.sha1_mismatch_error;
                    Debug.LogWarning("Copy error for file (" + srcToDestKeyValues.src + ") :" + errorMessageToUse);
                }
            
                IEnumerable<string> collectedFiles = Directory.EnumerateFiles(root, srcToDestKeyValues.src, searchOption);
                
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

                // Create the directory if it does not exist (does nothing if the file already exists).
                Directory.CreateDirectory(finalDestinationParent);

                if (isDestinationADirectory)
                {
                    Directory.CreateDirectory(finalDestinationPath);
                }

                string destPath = isDestinationADirectory ? Path.Combine(finalDestinationPath, src.Name) : finalDestinationPath;

                // Create the fileInfo for the destination.
                FileInfo destinationPath = new (destPath);

                // If it exists, make sure we can write over it.
                if (destinationPath.Exists)
                {
                    destinationPath.IsReadOnly = false;
                }
                
                // If the pair is either supposed to be copied identically, or if the files are not equal.
                if (fileInfo.originalSrcDestPair.copy_identical || 
                    !destinationPath.AreSemanticallyEqual(src))
                {
                    // Copy the file, overwriting the destination.
                    File.Copy(src.FullName, destPath, true);
                }

                // Invoke the callback indicating that the current file has been properly copied.
                postProcessCallback?.Invoke(destPath);
            }
        }

    }
}

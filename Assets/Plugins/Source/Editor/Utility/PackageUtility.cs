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
        /// Stores the package description - which is more appropriately a list of which files to copy where in order to create a package.
        /// </summary>
        [Serializable]
        public class PackageDescription
        {
            [SerializeField]
            public List<SrcDestPair> source_to_dest;
        }

        /// <summary>
        /// Reads the package description from JSON.
        /// </summary>
        /// <param name="path">Fully-qualified path to the JSON file that describes the contents of the package to create.</param>
        /// <returns>PackageDescription stored in the indicated path.</returns>
        public static PackageDescription ReadPackageDescription(string path)
        {
            string jsonText = File.ReadAllText(path);

            return JsonUtility.FromJson<PackageDescription>(jsonText);
        }

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
                // Skip if it's just a comment
                if (srcToDestKeyValues.IsCommentOnly()) { continue; }

                var collectedFiles = Directory.EnumerateFiles(root, srcToDestKeyValues.src);
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
            
            string currentWorkingDir = Path.GetFullPath(Directory.GetCurrentDirectory()).Replace('\\', '/') + "/";

            var toolsSection = new ToolsConfigEditor();
            toolsSection?.Load();

            foreach (var srcToDestKeyValues in packageDescription.source_to_dest)
            {
                // Skip if it's just a comment.
                if (srcToDestKeyValues.IsCommentOnly()) { continue; }

                var srcFileInfo = new FileInfo(srcToDestKeyValues.src);

                if (srcFileInfo.Exists &&
                    !string.IsNullOrEmpty(srcToDestKeyValues.sha1) &&
                    srcFileInfo.CalculateSHA1() != srcToDestKeyValues.sha1)
                {
                    Debug.LogWarning($"Copy error for file (\"{srcToDestKeyValues.src}\") : SHA1 mismatch.");
                }
            
                IEnumerable<string> collectedFiles = Directory.EnumerateFiles(root, srcToDestKeyValues.src);
                
                foreach (var entry in collectedFiles)
                {
                    FileInfo srcItem = new FileInfo(Path.GetFullPath(entry).Replace('\\', '/').Replace(currentWorkingDir,""));
                    var newItem = new FileInfoMatchingResult();
                    newItem.fileInfo = srcItem;
                    newItem.originalSrcDestPair = srcToDestKeyValues;

                    fileInfos.Add(newItem);
                }

            }

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
                if (!destinationPath.AreSemanticallyEqual(src))
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

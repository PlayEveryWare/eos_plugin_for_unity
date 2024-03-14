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
    using System.Linq;
    using UnityEditor;
    using Directory = System.IO.Directory;
    using File = System.IO.File;

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

        public class CopyFileTask
        {
            public string From;
            public string To;
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
        /// Parses the SrcDestPair objects listed within the given package description. Combined with
        /// the path to a source directory, this function determines each of the individual file copy
        /// operations that need to be carried out to create a package.
        /// </summary>
        /// <param name="source">The directory in which to scan for files that need copying.</param>
        /// <param name="packageDescription">The package description.</param>
        /// <returns></returns>
        public static IList<CopyFileTask> DetermineFileCopyTasks(string source, PackageDescription packageDescription)
        {
            IList<CopyFileTask> copyTasks = new List<CopyFileTask>();
            IList<string> filesToIgnore = new List<string>();

            // Process all the entries in the source_to_dest list.
            foreach (var srcDestPair in packageDescription.source_to_dest)
            {
                // Skip if it's just a comment.
                if (srcDestPair.IsCommentOnly()) { continue; }

                // Determine whether we're supposed to include the matching files, or exclude them.
                bool isExclusionary = srcDestPair.src.StartsWith("!");

                // Otherwise, create a FileInfo object 
                var srcFileInfo = new FileInfo(srcDestPair.src);

                // If the sha1 value is set for the SrcDestPair entry
                if (!string.IsNullOrEmpty(srcDestPair.sha1))
                {
                    // Then (assuming the file exists) calculate it's SHA1, and determine whether it matches
                    if (srcFileInfo.Exists && srcFileInfo.CalculateSHA1() != srcDestPair.sha1)
                    {
                        Debug.LogWarning($"Copy error for file (\"{srcDestPair.src}\") : SHA1 mismatch - {srcDestPair.sha1_mismatch_error}.");
                    }
                }
                
                // Process each file that matches the pattern in the SrcDestPair
                foreach (var matchingFile in GetMatchingFiles(source, srcDestPair))
                {
                    // If we are meant to exclude all matches, add the matchingFile path to the ignore list.
                    if (isExclusionary)
                    {
                        filesToIgnore.Add(matchingFile);
                    }
                    else
                    {
                        // Otherwise, add the match to the list of files that need to be copied.
                        copyTasks.Add(new CopyFileTask()
                        {
                            From = matchingFile,
                            To = Path.Combine(srcDestPair.dest, Path.GetFileName(matchingFile))
                        });
                    }
                }
            }

            // Now that the list of files to ignore is complete, we can trim the list of files to copy
            copyTasks = copyTasks.Where(
                copyTask => !filesToIgnore.Contains(copyTask.From)
                ).ToList();
            
            // Add to the list of files to copy any meta files that exist and match files already marked for copy
            IList<CopyFileTask> completeCopyTasks = new List<CopyFileTask>();
            foreach (var copyTask in copyTasks)
            {
                // add the copy task for the file
                completeCopyTasks.Add(copyTask);

                // if the copy task is a .meta file, we don't need to look for a meta-meta file
                if (copyTask.From.EndsWith(".meta")) { continue; }

                // determine if there is a meta file
                var metaFilePath = Path.Combine(copyTask.From, ".meta");
                if (!File.Exists(metaFilePath)) { continue; }
            
                // if there is a meta file, then add it to the list of copy tasks as well
                completeCopyTasks.Add(new CopyFileTask()
                {
                    From = Path.Combine(copyTask.From, ".meta"),
                    To = Path.Combine(copyTask.To, ".meta")
                });
            }

            return completeCopyTasks;
        }

        [MenuItem("ZAP/ZAP!")]
        public static void DoTheZap()
        {
            string current_directory = Directory.GetCurrentDirectory();
            string dot_slash_fqp = Path.GetFullPath("./");

            Debug.Log($"Current directory: {current_directory}");
            Debug.Log($"dot_slash: {dot_slash_fqp}");
        }

        /// <summary>
        /// Executes the given list of copy file tasks, copying them into the given output directory, and invoking the task complete callback for
        /// each copy task that completes.
        /// </summary>
        /// <param name="outputDirectory">The directory in which to output the files.</param>
        /// <param name="tasks">The copy tasks to execute.</param>
        /// <param name="taskCompleteCallback">Callback function that is invoked when a copy file task is completed.</param>
        public static void ExecuteCopyFileTasks(string outputDirectory, IList<CopyFileTask> tasks, Action<string> taskCompleteCallback = null)
        {
            // TODO: Disable this before ship - we should not actually ddelete all the contents of the output directory.
            Directory.Delete(outputDirectory, true);

            // Create the output directory if it does not already exist
            Directory.CreateDirectory(outputDirectory);

            // This helps keep track of whether a file has already been copied
            HashSet<string> copiedFiles = new HashSet<string>();

            // process each copy task
            foreach (var task in tasks)
            {
                // Create the directory if it does not exist (does nothing if the directory already exists).
                string destinationParent = Path.GetDirectoryName(task.To);
                if (destinationParent != null)
                {
                    // TODO: Make sure that the proper *.meta files are copied over where and when appropriate for the new Directories.
                    string directoryToCreate = Path.Combine(outputDirectory, destinationParent);
                    Directory.CreateDirectory(directoryToCreate);
                    //ListDirectoriesToCreateInOrder(directoryToCreate);
                    //Debug.Log($"New directory \"{directoryToCreate}\".");
                }

                // Create the fileInfo for the destination.
                FileInfo destinationPath = new (task.To);

                // If it exists, make sure we can write over it.
                if (destinationPath.Exists)
                {
                    destinationPath.IsReadOnly = false;
                }

                try
                {
                    // Copy the file, overwriting the destination.
                    //Debug.Log($"CopyTaskDebug: \"{task.From}\" => \"{Path.Combine(outputDirectory, task.To)}\"");
                    // TODO: Re-enable (not performing IO until we know it is working properly.
                    File.Copy(task.From, Path.Combine(outputDirectory, task.To), true);
                }
                catch(Exception ex) 
                {
                    Debug.LogWarning($"Error copying file \"{task.From}\": {ex.Message}.");
                }
                finally
                {
                    // Invoke the callback indicating that the current file has been properly copied.
                    // TODO: Alter the callback to indicate success or failure
                    taskCompleteCallback?.Invoke(task.To);
                }
            }

            Debug.Log($"{tasks.Count} copy tasks have completed successfully.");
        }


        private static void ListDirectoriesToCreateInOrder(string directory)
        {
            string current = Path.GetDirectoryName(directory);
            var createOrder = new Queue<string>();
            while (!string.IsNullOrEmpty(current))
            {
                // We stop going up the directory structure once we have found a directory that exists.
                if (Directory.Exists(current)) { break; }
                
                // Otherwise we add it to the queue of directories to create
                createOrder.Enqueue(current);

                // And move up one level
                current = Path.GetDirectoryName(current);
            }

            while (createOrder.Count > 0)
            {
                var directoryToCreate = createOrder.Dequeue();
                Debug.Log($"Creating directory \"{directoryToCreate}\".");
            }
        }

        /// <summary>
        /// Helper function that accepts a SrcDestPair object, and a root directory, and finds all files that match
        /// the pattern in the "src" field of the SrcDestPair object.
        /// </summary>
        /// <param name="root">Source directory to scan for files in.</param>
        /// <param name="pair">The SrcDestPair object that contains the information about the pattern to match and the destination to copy files to.</param>
        /// <returns>An enumerable of fully-qualified file paths to all files that match the pattern in the "src" field of the given SrcDestPair object.</returns>
        private static IEnumerable<string> GetMatchingFiles(string root, SrcDestPair pair)
        {
            // Start constructing the path that we will call EnumerateFiles with by removing 
            // any preceding exclamation marks.
            string pathPattern = pair.src.TrimStart('!');

            // If the filepath represents a file that exists (instead of being a pattern)
            // then just return that.
            if (File.Exists(Path.Combine(root, pathPattern)))
            {
                return new[] { Path.Combine(root, pathPattern) };
            }

            // The convention we use for our package description json files is that a double
            // asterisk at the end of a pattern indicates that the pattern should be matched 
            // recursively.
            bool shouldRecurse = pathPattern.EndsWith("**");

            // Default behavior is to not recursively match the pattern.
            SearchOption searchOption = SearchOption.TopDirectoryOnly;

            // If we are supposed to recurse.
            if (shouldRecurse)
            {
                // If we are going to recurse, we need to modify the string we will pass to 
                // Directory.EnumerateFiles, because it does not support the double asterisk
                // convention.
                pathPattern = pathPattern[..^1];

                // Set the search option accordingly
                searchOption = SearchOption.AllDirectories;
            }

            // Split the pattern into respective directory and file patterns, in order to 
            // call Directory.EnumerateFiles.
            SplitPathAndFilePatterns(pathPattern, out string directoryPattern, out string filePattern);

            // If the directory does not exist, then stop
            if (!Directory.Exists(directoryPattern))
            {
                return new List<string>();
            }

            return Directory.EnumerateFiles(Path.Combine(root, directoryPattern), filePattern, searchOption);
        }

        /// <summary>
        /// Takes a string pattern, and splits it into a directory path pattern and a file pattern. If no file
        /// pattern is found, a default one matching all files ("*") will be set.
        /// </summary>
        /// <param name="pattern">The pattern to split into components.</param>
        /// <param name="directoryPattern">The component of the pattern representing the directory.</param>
        /// <param name="filePattern">The component of the pattern representing the file.</param>
        private static void SplitPathAndFilePatterns(string pattern, out string directoryPattern, out string filePattern)
        {
            // Find the last directory separator
            // TODO: Deal properly with platform differences of forward / backward slashes.
            int lastSeparatorIndex = pattern.LastIndexOf('/');

            // Separate the path and pattern
            directoryPattern = pattern[..lastSeparatorIndex];
            filePattern = pattern[(lastSeparatorIndex + 1)..];

            // Check if the pattern is actually a directory (no wildcards present)
            if (!filePattern.Contains("*") && !filePattern.Contains("?"))
            {
                // If so, treat the entire input as a directory path and use a default pattern
                directoryPattern = pattern;
                filePattern = "*"; // Default pattern to match all files
            }
        }

    }
}

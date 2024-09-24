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
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("com.playeveryware.eos-Editor")]
namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    // This compile conditional exists to ensure that the UnityEngine.Networking
    // namespace is included when not in editor and when the platform is 
    // Android - because IO operations on Android require use of it.
#if UNITY_ANDROID && !UNITY_EDITOR
    using UnityEngine.Networking;
#endif

    // This compile conditional exists to ensure that the Linq namespace is
    // not utilized during runtime operations.
#if UNITY_EDITOR
    using System.Linq;
#endif

    /// <summary>
    /// Utility class used for a variety of File tasks.
    /// </summary>
    internal static class FileSystemUtility
    {
        /// <summary>
        /// Interval with which to update progress, in milliseconds
        /// </summary>
        private const int DefaultUpdateIntervalMS = 1500;

        #region Data Structures 

        /// <summary>
        /// Stores information regarding a file copy operation.
        /// </summary>
        public struct CopyFileOperation
        {
            /// <summary>
            /// The fully-qualified path of the source file.
            /// </summary>
            public string SourcePath;

            /// <summary>
            /// The fully-qualified path to copy the file to. The path does not
            /// have to exist, and in fact might not.
            /// </summary>
            public string DestinationPath;

            /// <summary>
            /// The number of bytes in the file to copy.
            /// </summary>
            public long Bytes;
        }

        /// <summary>
        /// Contains information about the progress of a copy file operation.
        /// </summary>
        public class CopyFileProgressInfo
        {
            /// <summary>
            /// The number of files that have been copied.
            /// </summary>
            public int FilesCopied;

            /// <summary>
            /// The total number of files being copied.
            /// </summary>
            public int TotalFilesToCopy;

            /// <summary>
            /// The size in bytes of the files that have been copied.
            /// </summary>
            public long BytesCopied;

            /// <summary>
            /// The total size in bytes of all the files being copied.
            /// </summary>
            public long TotalBytesToCopy;
        }

        #endregion

        /// <summary>
        /// Generates a unique and new temporary directory inside the Temporary
        /// Cache Path as determined by Unity, and returns the fully-qualified
        /// path to the newly created directory. The directory name will be
        /// "Temp-" appended with a GUID.
        /// </summary>
        /// <param name="path">
        /// Fully-qualified path to the newly generated directory if one was
        /// created, otherwise path is set to default System.String value.
        /// </param>
        /// <returns>
        /// True if the temporary directory was created, false otherwise.
        /// </returns>
        public static bool TryGetTempDirectory(out string path)
        {
            path = default;
            
            // Nested local function to reduce repetitive code.
            string GenerateTempPath()
            {
                return Path.Combine(
                    Application.temporaryCachePath,
                    $"Temp-{Guid.NewGuid()}");
            }

            // Generate a temporary directory path.
            string tempPath = GenerateTempPath();

            // If (by some crazy miracle) the directory path already exists,
            // try once more.
            if (Directory.Exists(tempPath))
            {
                Debug.LogWarning(
                    $"The temporary directory created collided with " +
                    $"an existing temporary directory of the same name. This " +
                    $"is very unlikely.");

                tempPath = GenerateTempPath();

                if (Directory.Exists(tempPath))
                {
                    Debug.LogError(
                        $"When generating a temporary directory, the " +
                        $"temporary directory generated collided twice with " +
                        $"already existing directories of the same name. " +
                        $"This is very unlikely.");

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
                    Debug.LogError(
                        $"Could not generate temporary directory.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Could not generate temporary directory: " +
                    $"{e.Message}");
                return false;
            }

            // return the fully-qualified path to the newly created directory.
            path = Path.GetFullPath(tempPath);
            return true;
        }

        // This compile conditional exists because the following functions 
        // make use of the System.Linq namespace which is undesirable to use
        // during runtime. Since these functions are currently only ever 
        // utilized in areas of the code that run in the editor, it is
        // appropriate to use compile conditionals to include / exclude them.
#if UNITY_EDITOR
        /// <summary>
        /// Get a list of the directories that are represented by the filepaths
        /// provided. The list is unique, and is ordered by smallest path first,
        /// so that the list can be utilized to create each directory in-order
        /// if that is useful
        /// </summary>
        /// <param name="filepaths">
        /// The filepaths to get the directories for.
        /// </param>
        /// <param name="creationOrder">
        /// If true, return the list of directories in such an order that they
        /// do not rely on the existence of directories further down the list.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<string> GetDirectories(
            IEnumerable<string> filepaths, 
            bool creationOrder = true)
        {
            // For each filepath, determine the immediate parent directory of
            // the file. Make a unique set of these by utilizing a HashSet.
            ISet<string> directoriesToCreate = new HashSet<string>();
            foreach (var path in filepaths)
            {
                string parent = Path.GetDirectoryName(path);

                // skip if no parent
                if (null == parent) continue;
                
                directoriesToCreate.Add(parent);
                
            }

            // Return the list of directories to create in ascending order of
            // string length.
            if (creationOrder)
            {
                return directoriesToCreate.OrderBy(s => s.Length);
            }

            return directoriesToCreate;
        }

        /// <summary>
        /// Run a list of file copy operations. If the destination directory
        /// indicated does not exist, it will be created. It is expected that
        /// the source file indicated exists. Default behavior is to overwrite
        /// destination files.
        ///
        /// Before the file copy operations begin, the copy file operations are
        /// inspected for missing destination directories - and the directory
        /// structure required is created in its entirety before file copy
        /// operations commence.
        /// 
        /// If a progress interface is provided, the file copy operations will
        /// be randomized so as to average out the number of bytes copied at
        /// each interval. Otherwise the files will be copied in the order they
        /// are in the provided operations parameter.
        /// </summary>
        /// <param name="operations">File copy operations to perform.</param>
        /// <param name="updateIntervalMS">
        /// The interval in milliseconds with which to report progress.
        /// </param>
        /// <param name="progress">Progress reporter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task</returns>
        public static async Task CopyFilesAsync(
            IList<CopyFileOperation> operations, 
            CancellationToken cancellationToken = default, 
            IProgress<CopyFileProgressInfo> progress = null, 
            int updateIntervalMS = DefaultUpdateIntervalMS)
        {
            IEnumerable<string> directoriesToCreate = GetDirectories(
                operations.Select(o => o.DestinationPath));

            // Create each directory
            foreach (var directory in directoriesToCreate)
            {
                Directory.CreateDirectory(directory);
            }

            // If the progress is defined, then do extra work to track the
            // progress.
            if (null != progress)
            {
                // Create struct to track and report on progress
                CopyFileProgressInfo progressInfo = new()
                {
                    TotalBytesToCopy = operations.Sum(o => o.Bytes),
                    TotalFilesToCopy = operations.Count()
                };

                // Create timer to periodically report on the progress based on
                // the interval (at some future point it may be appropriate to
                // make that interval a parameter)
                await using Timer progressTimer = new(state =>
                {
                    progress?.Report(progressInfo);
                }, null, 0, updateIntervalMS);

                // Get a list out of the operations, and shuffle it
                IList<CopyFileOperation> operationsList = operations.ToList();
                operationsList.Shuffle();

                // Copy the files asynchronously with the provided
                // cancellation token, and progress stuff.
                await CopyFilesAsyncInternal(
                    operationsList, 
                    cancellationToken, 
                    progress, 
                    progressInfo);
            }
            else
            {
                await CopyFilesAsyncInternal(operations, cancellationToken);
            }
        }

        /// <summary>
        /// Performs the core file copy operations.
        /// </summary>
        /// <param name="operations">File copy operations to perform.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter.</param>
        /// <param name="progressInfo">Progress information.</param>
        /// <returns>Task</returns>
        private static async Task CopyFilesAsyncInternal(
            IEnumerable<CopyFileOperation> operations, 
            CancellationToken cancellationToken = default, 
            IProgress<CopyFileProgressInfo> progress = null, 
            CopyFileProgressInfo progressInfo = default)
        {
            var tasks = operations.Select(async copyOperation =>
            {
                await CopyFileAsync(copyOperation, cancellationToken);

                // If there is no progress reporter, then skip updating the
                // update info.
                if (null != progress)
                {
                    // lock the progressInfo, since multiple threads will be 
                    // updating it at once
                    lock (progressInfo)
                    {
                        // Increment the number of files copied.
                        progressInfo.FilesCopied++;

                        // Update the number of bytes that have been copied.
                        progressInfo.BytesCopied += copyOperation.Bytes;

                        // Report progress if a progress reporter is provided.
                        progress?.Report(progressInfo);
                    }
                }
            }).ToList();

            await Task.WhenAll(tasks);
        }
#endif
        /// <summary>
        /// Copies a single file asynchronously.
        /// </summary>
        /// <param name="op">The file copy operation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task</returns>
        private static async Task CopyFileAsync(
            CopyFileOperation op, 
            CancellationToken cancellationToken)
        {
            // Maximum number of times the operation is retried if it fails.
            const int maxRetries = 3;

            // This is the initial delay before the operation is retried.
            const int delayMilliseconds = 200; 

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Make sure to throw an exception if a cancellation was
                    // requested of the token.
                    cancellationToken.ThrowIfCancellationRequested();

                    // Run the file copy asynchronously, passing on the
                    // cancellation token.
                    await Task.Run(() =>
                    {
                        File.Copy(op.SourcePath, op.DestinationPath, true);
                    }, cancellationToken);

                    // if the file was not copied on the first attempt, then
                    // be sure to log the fact that it eventually *was*
                    // copied successfully
                    if (attempt > 0)
                    {
                        Debug.Log($"File \"{op.SourcePath}\" was successfully copied after {attempt + 1} retries.");
                    }

                    // if the task completes, then break out of the retry loop
                    break;
                }
                catch (IOException ex) when (attempt < maxRetries - 1)
                {
                    // exponentially increase the delay to maximize the chance
                    // it will succeed without waiting too long.
                    var delay = delayMilliseconds * (int)Math.Pow(2, attempt);
                    
                    // Construct detailed message regarding the nature of the problem.
                    StringBuilder sb = new();
                    sb.AppendLine($"Exception occurred during the following copy operation:");
                    sb.AppendLine($"From: \"{op.SourcePath}\"");
                    sb.AppendLine($"  To: \"{op.DestinationPath}\"");
                    sb.AppendLine($"Exception message: \"{ex.Message}\"");
                    sb.AppendLine($"Retrying in {delay}ms.");
                    Debug.LogWarning(sb.ToString());

                    // Only retry if there are remaining attempts.
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns the root of the Unity project.
        /// </summary>
        /// <returns>Fully-qualified file path to the root of the Unity project.</returns>
        public static string GetProjectPath()
        {
            // Assuming the current directory is within the project (e.g., in the Editor or during Play mode)
            string assetsPath = CombinePaths(Directory.GetCurrentDirectory(), "Assets");

            // Ensure the Assets folder exists at the expected location
            if (DirectoryExists(assetsPath))
            {
                // Move up one directory from Assets to get the root directory of the project
                return Path.GetFullPath(CombinePaths(assetsPath, ".."));
            }

            // If running in a different context or the assumption is wrong, handle accordingly
            throw new DirectoryNotFoundException("Unable to locate the Assets folder from the current directory.");
        }


        #region Line Ending Manipulations

        public static void ConvertDosToUnixLineEndings(string filename)
        {
            ConvertDosToUnixLineEndings(filename, filename);
        }

        public static void ConvertDosToUnixLineEndings(string srcFilename, string destFilename)
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

        #endregion
#endif

        #region File Read Functionality

        public static async Task<(bool Success, string Result)> TryReadAllTextAsync(string filePath)
        {
            bool fileExists = await ExistsInternal(filePath, false);

            if (!fileExists)
            {
                return (false, null);
            }

            string contents = await ReadAllTextAsync(filePath);

            return null == contents ? (false, null) : (true, contents);
        }

        /// <summary>
        /// Reads all text from the indicated file.
        /// </summary>
        /// <param name="path">Filepath to the file to read from.</param>
        /// <returns>The contents of the file at the indicated path as a string.</returns>
        public static string ReadAllText(string path)
        {
            return Task.Run(() => ReadAllTextAsync(path)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously reads all text from the indicated file.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <returns>Task</returns>
        public static async Task<string> ReadAllTextAsync(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, use a custom helper to read the file synchronously
            return await AndroidFileIOHelper.ReadAllText(path);
#else
            // On other platforms, read asynchronously or synchronously as
            // appropriate.
            try
            {
                return await File.ReadAllTextAsync(path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
#endif
        }

        #endregion

        #region Get File System Entries Functionality

        /// <summary>
        /// Wrapper function for calls to Directory.EnumerateFileSystemEntries.
        /// The reason this is implemented here is to restrict usage of the
        /// System.IO namespace to be within this file.
        /// </summary>
        /// <param name="path">
        /// The path to enumerate file system entries from.
        /// </param>
        /// <param name="pattern">The pattern to match entries to.</param>
        /// <param name="recursive">
        /// Whether to search recursively, or in the top directory only.
        /// </param>
        /// <returns>
        /// An enumerable collection of strings representing the file system
        /// entries being retrieved.
        /// </returns>
        public static IEnumerable<string> GetFileSystemEntries(string path, string pattern, bool recursive = true)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory.EnumerateFileSystemEntries(path, pattern, option);
        }

        #endregion


        #region Path Functionality

        /// <summary>
        /// Wrapper function for calls to Path.Combine. The reason this is
        /// implemented here is to restrict usage of the System.IO namespace to
        /// be within this file.
        /// </summary>
        /// <param name="paths">A variable number of path elements.</param>
        /// <returns>
        /// The result of calling System.IO.Path.Combine with the provided path
        /// components.
        /// </returns>
        public static string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Wrapper function for calls to Path.GetFullPath. The reason this is
        /// implemented here is to restrict usage of the System.IO namespace to
        /// be within this file.
        /// </summary>
        /// <param name="path">The path (relative or full).</param>
        /// <returns>The fully-qualified path.</returns>
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Wrapper function for calls to Path.GetFileName. The reason this is
        /// implemented here is to restrict usage of the System.IO namespace to
        /// be within this file.
        /// </summary>
        /// <param name="path">The path of the file to get the name of.</param>
        /// <returns>The filename component of the given path.</returns>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        #endregion

        #region Write Functionality

        /// <summary>
        /// Synchronously writes the given file contents to the given filepath,
        /// optionally creating the directory structure if it does not already
        /// exist.
        /// </summary>
        /// <param name="filePath">The path to write to.</param>
        /// <param name="content">The contents to write to the file.</param>
        /// <param name="createDirectory">
        /// Whether or not to create the directory structure represented by the
        /// filepath.
        /// </param>
        public static void WriteFile(string filePath, string content, bool createDirectory = true)
        {
            // Appropriately call the async function synchronously.
            Task.Run(() => WriteFileAsync(filePath, content, createDirectory)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously writes the given file contents to the given filepath,
        /// optionally creating the directory structure if it does not already
        /// exist.
        /// </summary>
        /// <param name="filePath">The path to write to.</param>
        /// <param name="content">The contents to write to the file.</param>
        /// <param name="createDirectory">
        /// Whether or not to create the directory structure represented by the
        /// filepath.
        /// </param>
        /// <returns>Task</returns>
        public static async Task WriteFileAsync(string filePath, string content, bool createDirectory = true)
        {
            FileInfo file = new(filePath);

            // If the directory should be created, and the DirectoryInfo is not
            // null, then create the directory.
            if (createDirectory && null != file.Directory)
            {
                CreateDirectory(file.Directory);
            }

            await using StreamWriter writer = new(filePath);
            await writer.WriteAsync(content);
        }

        /// <summary>
        /// Helper function to create a directory. If the directory already
        /// exists, then nothing will happen.
        /// </summary>
        /// <param name="dInfo">
        /// The DirectoryInfo object that represents the directory to be
        /// created.
        /// </param>
        private static void CreateDirectory(DirectoryInfo dInfo)
        {
            try
            {
                dInfo.Create();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Directory and File Exists functionality

        public static bool DirectoryExists(string path)
        {
            return DirectoryExistsAsync(path).GetAwaiter().GetResult();
        }

        public static async Task<bool> DirectoryExistsAsync(string path)
        {
            return await ExistsInternal(path, isDirectory: true);
        }

        public static bool FileExists(string path)
        {
            return FileExistsAsync(path).GetAwaiter().GetResult();
        }

        public static async Task<bool> FileExistsAsync(string path)
        {
            return await ExistsInternal(path, isDirectory: false);
        }

        private static async Task<bool> ExistsInternal(string path, bool isDirectory)
        {
            bool exists = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            using UnityWebRequest request = UnityWebRequest.Get(path);
            request.SendWebRequest();
            while (!request.isDone)
            {
                await Task.Yield();
            }

            exists = (UnityWebRequest.Result.Success == request.result);
#else
            if (isDirectory)
            {
                exists = Directory.Exists(path);
            }
            else
            {
                exists = File.Exists(path);
            }
#endif
            return await Task.FromResult(exists);
        }

        #endregion

        public static void NormalizePath(ref string path)
        {
            char toReplace = Path.DirectorySeparatorChar == '\\' ? '/' : '\\';
            path = path.Replace(toReplace, Path.DirectorySeparatorChar);
        }

#if UNITY_EDITOR

        public static void CleanDirectory(string directoryPath, bool ignoreGit = true)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"Cannot clean directory \"{directoryPath}\", because it does not exist.");
                return;
            }

            try
            {
                foreach (string subDir in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories))
                {
                    // Skip .git directories 
                    if (ignoreGit && subDir.EndsWith(".git")) { continue; }
                    
                    // TODO: This is a little bit dangerous as one developer has found out. If the output directory is not
                    //       empty, and contains directories and files unrelated to output, this will (without prompting)
                    //       delete them. So, if you're outputting to, say the "Desktop" directory, it will delete everything
                    //       on your desktop (zoinks!)
                    if (Directory.Exists(subDir))
                        Directory.Delete(subDir, true);
                }

                foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName is ".gitignore" or ".gitattributes" && Path.GetDirectoryName(file) == directoryPath)
                    {
                        if (Path.GetDirectoryName(file) == directoryPath)
                        {
                            continue; // Skip these files if they are in the root directory
                        }
                    }

                    if (File.Exists(file))
                        File.Delete(file);
                }

                Debug.Log($"Finished cleaning directory \"{directoryPath}\".");
            }
            catch (Exception ex)
            {
                Debug.Log($"An error (which was ignored) occurred while cleaning \"{directoryPath}\": {ex.Message}");
            }
        }


        public static void OpenDirectory(string path)
        {
            // Correctly format the path based on the operating system.
            // For Windows, the path format is fine as is.
            // For macOS, use the "open" command.
            // For Linux, use the "xdg-open" command.
            path = path.Replace("/", "\\"); // Replace slashes for Windows compatibility

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                System.Diagnostics.Process.Start("open", path);
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                System.Diagnostics.Process.Start("xdg-open", path);
            }
        }
#endif
    }
}

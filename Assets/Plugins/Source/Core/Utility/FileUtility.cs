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

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Utility class used for a variety of File tasks.
    /// </summary>
    public static class FileUtility
    {
        // Note: "\r\n" (Windows), "\n" (Unix/Linux), "\r" (older Macs)
        private static readonly string[] LineEndings = new[] { "\r\n", "\n", "\r" };

        /// <summary>
        /// Generates a unique and new temporary directory inside the Temporary Cache Path as determined by Unity,
        /// and returns the fully-qualified path to the newly created directory.
        /// </summary>
        /// <returns>Fully-qualified file path to the newly generated directory.</returns>
        public static bool TryGetTempDirectory(out string path)
        {
            // Generate a temporary directory path.
            string tempPath = Path.Combine(Application.temporaryCachePath, $"/Output-{Guid.NewGuid()}/");

            // If (by some crazy miracle) the directory path already exists, keep generating until there is a new one.
            if (Directory.Exists(tempPath))
            {
                Debug.LogWarning(
                    $"The temporary directory created collided with an existing temporary directory of the same name. This is very unlikely.");
                tempPath = Path.Combine(Application.temporaryCachePath, $"/Output-{Guid.NewGuid()}/");

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
        /// Returns the root of the Unity project.
        /// </summary>
        /// <returns>Fully-qualified file path to the root of the Unity project.</returns>
        public static string GetProjectPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        }

        /// <summary>
        /// Reads all text from the indicated file.
        /// </summary>
        /// <param name="path">Filepath to the file to read from.</param>
        /// <returns>The contents of the file at the indicated path as a string.</returns>
        public static string ReadAllText(string path)
        {
            string text;
#if UNITY_ANDROID
            using var request = UnityEngine.Networking.UnityWebRequest.Get(path);
            request.timeout = 2; //seconds till timeout
            request.SendWebRequest();

            // Wait until webRequest completed
            while (!request.isDone) { }

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"Requesting \"{path}\", please make sure it exists.");
                throw new Exception("UnityWebRequest didn't succeed, Result : " + request.result);
            }
#else
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log($"Requesting \"{path}\", please make sure it exists and is a valid config");
                throw new Exception("UnityWebRequest didn't succeed : Network or HTTP Error");
            }
#endif
            text = request.downloadHandler.text;
#else
            text = File.ReadAllText(path);
#endif
            return text;
        }

        public static async Task<string[]> ReadAllLinesAsync(string path)
        {
            return await File.ReadAllLinesAsync(path);
        }

        public static string[] ReadAllLines(string path, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            // Use ReadAllText so platform-specific implementation of Android will function properly
            string fileContents = ReadAllText(path);

            return fileContents.Split(LineEndings, splitOptions);
        }

        /// <summary>
        /// Asynchronously reads all text from the indicated file.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <returns>Task</returns>
        public static async Task<string> ReadAllTextAsync(string path)
        {
            return await File.ReadAllTextAsync(path);
        }

        /// <summary>
        /// Writes all text to the indicated file.
        /// </summary>
        /// <param name="path">Filepath to the file to write to.</param>
        /// <param name="content">The content to write to the file.</param>
        public static void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        #region Line Ending Manipulations

        public static void ConvertDosToUnixLineEndings(string filename)
        {
            ConvertDosToUnixLineEndings(filename, filename);
        }

        public static void ConvertDosToUnixLineEndings(string srcFilename, string destFilename)
        {
            string fileContents = ReadAllText(srcFilename);
            fileContents = fileContents.Replace("\r\n", "\n");
            WriteAllText(destFilename, fileContents);
        }

        #endregion

        /// <summary>
        /// Normalizes a string path by replacing all directory separator characters with the
        /// directory separator character that is used on the system.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        public static void NormalizePath(ref string path)
        {
            char toReplace = Path.DirectorySeparatorChar == '\\' ? '/' : '\\';
            path = path.Replace(toReplace, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Cleans the given directory by removing all contents from it. 
        /// </summary>
        /// <param name="directoryPath">Path to clean.</param>
        /// <param name="ignoreGit">Whether to ignore ".git" directory and any files that start with ".git" in the root of the directory being cleaned.</param>
        public static void CleanDirectory(string directoryPath, bool ignoreGit = true)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Could not find directory \"{directoryPath}\" to clean.");
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
                        continue; // Skip these files if they are in the root directory
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

#if UNITY_EDITOR
        public static void OpenFolder(string path)
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

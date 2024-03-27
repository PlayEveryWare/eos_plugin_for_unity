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
        /// <summary>
        /// Generates a unique and new temporary directory inside the Temporary Cache Path as determined by Unity,
        /// and returns the fully-qualified path to the newly created directory.
        /// </summary>
        /// <returns>Fully-qualified file path to the newly generated directory.</returns>
        public static string GenerateTempDirectory()
        {
            // Generate a temporary directory path.
            string tempDirectory = Path.Combine(Application.temporaryCachePath, $"/Output-{Guid.NewGuid()}/");

            // If (by some crazy miracle) the directory path already exists, keep generating until there is a new one.
            while (Directory.Exists(tempDirectory))
            {
                tempDirectory = Path.Combine(Application.temporaryCachePath, $"/Output-{Guid.NewGuid()}/");
            }

            // Create the directory.
            Directory.CreateDirectory(tempDirectory);

            // return the fully-qualified path to the newly created directory.
            return Path.GetFullPath(tempDirectory);
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
            return File.ReadAllText(path);
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

        /// <summary>
        /// Asynchronously reads all text from the indicated file.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <returns>Task</returns>
        public static async Task<string> ReadAllTextAsync(string path)
        {
            return await File.ReadAllTextAsync(path);
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

                    Directory.Delete(subDir, true);
                }

                foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName is ".gitignore" or ".gitattributes")
                    {
                        if (Path.GetDirectoryName(file) == directoryPath)
                        {
                            continue; // Skip these files if they are in the root directory
                        }
                    }

                    File.Delete(file);
                }

                Debug.Log($"Finished cleaning directory \"{directoryPath}\".");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"An error occurred while cleaning \"{directoryPath}\": {ex.Message}");
            }
        }
    }
}

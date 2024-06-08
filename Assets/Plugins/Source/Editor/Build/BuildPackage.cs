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

using System.IO;
using UnityEngine;
using UnityEditor.Build;

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using Utility;

    public static class BuildPackage
    {
        public enum PackageType
        {
            /// <summary>
            /// Un-compressed directory of UPM contents.
            /// </summary>
            UPMDirectory,

            /// <summary>
            /// Legacy package type.
            /// </summary>
            DotUnity,

            /// <summary>
            /// Compressed directory of UPM contents
            /// </summary>
            UPMTarBall,
        }

        /// <summary>
        /// Unless another path is explicitly provided, default to writing the output to a
        /// directory in the root of the project folder named "Build"
        /// </summary>
        private const string DEFAULT_OUTPUT_DIRECTORY = "Build";

        /// <summary>
        /// Note that this is a Non-Unity defined command line argument.
        /// </summary>
        private const string ARG_FLAG_OUTPUT = "-EOSPluginOutput";

        private const string ARG_FLAG_PACKAGE_TYPE = "-PackageType";

        /// <summary>
        /// Returns a path to output the plugin to.
        /// </summary>
        /// <returns>Path to output the plugin to</returns>
        private static string GetOutputPath()
        {
            string output_path = GetCLIArgument(ARG_FLAG_OUTPUT);

            if (string.Empty == output_path)
            {
                // In this case we want the absolute path, because we don't
                // want any code down-stream to try and use it as a relative path.
                // Note that it's resolved using the project's root directory.
                output_path = Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        "..",
                        DEFAULT_OUTPUT_DIRECTORY));
            }

            return output_path;
        }

        private static string GetCLIArgument(string flag)
        {
            string value = string.Empty;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (ARG_FLAG_OUTPUT == args[i] && i + 1 < args.Length)
                {
                    value = args[i + 1];
                }
            }

            return value;
        }

        /// <summary>
        /// This is the JSON file that defines which files to put into the UPM directory.
        /// @TODO: Handle the scenario where whomever is developing the UPM wants to 
        /// introduce a new file. Is there a way that we could intelligently include that
        /// and add it to this JSON without things getting really messy?
        /// </summary>
        /// <param name="type">The type of the package</param>
        /// <returns>Filepath to the appropriate package.json file to use.</returns>
        private static string GetDefaultJSONPackageFile(PackageType type)
        {
            string path = string.Empty;

            switch (type)
            {
                case PackageType.DotUnity:
                    path = "etc/PackageConfigurations/eos_dotunitypackage_package_desc.json";
                    break;
                case PackageType.UPMTarBall:
                    path = "etc/PackageConfigurations/eos_package_description.json";
                    break;
                case PackageType.UPMDirectory:
                default:
                    path = "etc/PackageConfigurations/eos_export_assets_package_desc.json";
                    break;
            }

            return path;
        }

        private static PackageType GetPackageType()
        {
            return PackageType.UPMDirectory;
        }

        private static void Clean()
        {
            string output_path = GetOutputPath();
            if (Directory.Exists(output_path))
            {
                foreach (string filepath in Directory.EnumerateFiles(output_path))
                {
                    File.Delete(filepath);
                }

                foreach (string dirpath in Directory.EnumerateDirectories(output_path))
                {
                    Directory.Delete(dirpath, true);
                }
            }
        }

        /// <summary>
        ///Does the exporting of the plugin to a UPM directory 
        /// </summary>
        public static void ExportPlugin()
        {
            // Get output directory
            string OutputDirectory = GetOutputPath();

            PackageType packageType = GetPackageType();

            // if the output directory is not already fully qualified,
            // then we will assume it's relative to the place the "Unity.exe" 
            // command was executed.
            if (!Path.IsPathFullyQualified(OutputDirectory))
            {
                OutputDirectory = Path.GetFullPath(
                    Path.Join(
                        Directory.GetCurrentDirectory(),
                        OutputDirectory));
            }

            string jsonFile = Path.Combine(
                Application.dataPath,
                "..",
                GetDefaultJSONPackageFile(PackageType.UPMTarBall));

            // Validate file paths
            if (!File.Exists(jsonFile))
            {
                ExportError("JSON file \"" + jsonFile + "\" does not appear to exist.");
            }

            // If the output directory does not exist, try and create it.
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
                Debug.LogWarning("Output Directory: \"" + OutputDirectory + "\" created.");
            }
            else
            {
                Clean();
            }

            // Create package
            // TODO: Join build systems - note the duplicate existence of the PackageType enum.
            // TODO: Make clean a parameter
            bool shouldClean = false;

            switch (packageType)
            {
                case PackageType.UPMTarBall:
                    UnityPackageCreationUtility
                        .CreatePackage(UnityPackageCreationUtility.PackageType.UPMTarball, shouldClean, null).Wait();
                    break;
                case PackageType.DotUnity:
                    UnityPackageCreationUtility.CreatePackage(UnityPackageCreationUtility.PackageType.DotUnity, shouldClean, null).Wait();
                    break;
                case PackageType.UPMDirectory:
                default:
                    UnityPackageCreationUtility.CreatePackage(UnityPackageCreationUtility.PackageType.UPM, shouldClean, null).Wait();
                    break;

            }
        }

        /// <summary>
        /// Helper method to log and throw fatal errors that may occur during plugin export.
        /// </summary>
        /// <param name="message">The message to log / throw</param>
        private static void ExportError(string message)
        {
            Debug.LogError(message);
            throw new BuildFailedException(message);
        }
    }
}
/*
 * Copyright (c) 2023 PlayEveryWare
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

// TODO: Make sure to enclose this properly in scripting defines so that it is only enabled for Windows platforms.
//       Alternatively, implement it so that it behaves as expected on a Linux machine (ie running the Makefile
//       natively.

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using PlayEveryWare.EpicOnlineServices.Utility;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;
    /// <summary>
    /// Contains functions to carry out a variety of tasks related to building.
    /// </summary>
    public static class BuildUtility
    {
        private static Nullable<bool> s_isDeployedAsUPM;

        private class Manifest
        {
            public Dictionary<string, string> dependencies;
        }

        /// <summary>
        /// Whether the plugin is deployed as a UPM or not.
        /// </summary>
        public static bool DeployedAsUPM
        {
            get
            {
                s_isDeployedAsUPM ??= IsDeployedAsUPM();

                return s_isDeployedAsUPM.Value;
            }
        }

        /// <summary>
        /// Determine if deployed as UPM by checking to see if the manifest has an entry for the package.
        /// </summary>
        /// <returns>True if the plugin is deployed as UPM, false otherwise.</returns>
        private static bool IsDeployedAsUPM()
        {
            string packagePathname = Path.GetFullPath(Path.Combine("Packages", EOSPackageInfo.PackageName));

            if (Directory.Exists(packagePathname))
            {
                Debug.Log("Deployed via UPM");
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Contains information regarding a specific installation of Visual Studio.
        /// </summary>
        private class VSInstallation
        {
            /// <summary>
            /// Path to the Visual Studio Developer Command Prompt
            /// </summary>
            public string VSDevCmdPath;

            /// <summary>
            /// The version of Visual Studio that is installed.
            /// </summary>
            public Version Version;

            /// <summary>
            /// The toolsets that are installed with the installation of Visual Studio.
            /// </summary>
            public string[] Toolsets;
        }

        [Serializable]
        public class Installation
        {
            public string installationPath;
            public string installationVersion;
        }

        [Serializable]
        private class VSWhereOutput
        {
            public Installation[] installations;
        }

        /// <summary>
        /// Delegate defining the function signature of a function capable of building the native code at the indicated project file path.
        /// </summary>
        /// <param name="projectFilePath">Path to the project file.</param>
        /// <param name="binaryOutput">Location to output the results to.</param>
        /// <returns>True if the build was successful, false otherwise.</returns>
        private delegate bool BuildNativeLibraryDelegate(string projectFilePath, string binaryOutput);

        static BuildUtility()
        {
            // Note: This conditional is here because it only makes sense to 
            // look for visual studio installations if running on Windows.
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            FindVSInstallations();
#endif
        }

        /// <summary>
        /// Used to store information about all the installations of Visual Studio on the current system.
        /// </summary>
        private static IList<VSInstallation> VisualStudioInstallations;

        /// <summary>
        /// Checks for the existence of the appropriate platform configuration JSON file.
        /// </summary>
        /// <exception cref="BuildFailedException">Throws BuildFailedException if the required config file cannot be found.</exception>
        public static void ValidatePlatformConfiguration()
        {
            // TODO: Modify this method so that if the config file is not found, one is automatically created.
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            if (PlatformManager.TryGetConfigFilePath(target, out string configFilePath))
            {
                if (File.Exists(configFilePath))
                {
                    Debug.Log($"Confirmed configuration exists for target \"{target}\" at \"{configFilePath}\"");
                }
                else
                {
                    throw new BuildFailedException($"Config file for target \"{target}\" (\"{configFilePath}\") was not found.");
                }
            }
            else
            {
                throw new BuildFailedException($"Target \"{target}\" is not supported.");
            }
        }

        #region Methods for determining toolsets installed on build system

        /// <summary>
        /// Uses the vswhere.exe utility to determine the path of all installed versions of visual studio,
        /// then inspects each installation for which versions of the PlatformToolset are installed. The
        /// result is a collection of all the versions of the PlatformToolset that are installed on the
        /// system.
        /// </summary>
        /// <exception cref="FileNotFoundException">Gets thrown if the vswhere.exe utility cannot be found.</exception>
        /// <exception cref="BuildFailedException">Gets called if the vswhere.exe utility returned an exit code indicating failure.</exception>
        public static void FindVSInstallations()
        {
            // Stop if we have already got the visual studio installations.
            if (VisualStudioInstallations != null && VisualStudioInstallations.Count > 0)
            {
                return;
            }

            const string vsWhereFilePath = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";

            if (false == File.Exists(vsWhereFilePath))
            {
                throw new FileNotFoundException(
                    $"While determining the PlatformToolsets that are installed, the vswhere.exe utility could not be found.");
            }

            Process p = new();
            p.StartInfo.FileName = $"\"{vsWhereFilePath}\"";
            p.StartInfo.Arguments = " -all -format json";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;

            p.Start();

            // Read the output asynchronously and store it in a variable.
            var outputBuilder = new StringBuilder();
            p.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
            p.BeginOutputReadLine();

            // Wait for the process to exit
            p.WaitForExit();

            // Alter the json output from vswhere so that it plays nice with JsonUtility.
            string vsWhereOutputString = @"{""installations"":" + outputBuilder.ToString() + "}";

            // Continue with processing vsWhereOutput...
            VSWhereOutput vsWhereOutput = JsonUtility.FromJson<VSWhereOutput>(vsWhereOutputString);

            // Used to store the different installations of VS on the system.
            VisualStudioInstallations = new List<VSInstallation>();
            foreach (var installation in vsWhereOutput.installations)
            {
                try
                {
                    Version version = new(installation.installationVersion ?? string.Empty);
                    string installPath = installation.installationPath;

                    // stop processing if the install path is empty for some reason.
                    if (string.IsNullOrEmpty(installPath))
                        continue;

                    string[] toolsetsInstalled = GetToolsetsInstalled(installPath)?.ToArray();
                    string msBuildPath = Path.Combine(installPath + @"MSBuild\Current\Bin\amd64\MSBuild.exe");

                    string vsDevCmd = Path.Combine(installPath, @"Common7\Tools\VsDevCmd.bat");

                    var install = new VSInstallation()
                    {
                        Version = version,
                        Toolsets = toolsetsInstalled,
                        VSDevCmdPath = vsDevCmd
                    };

                    VisualStudioInstallations.Add(install);
                }
                catch (Exception e)
                {
                    // this is mostly for handling parsing issues
                    Debug.LogWarning(
                        $"There was a problem reading the output from \"{vsWhereFilePath}\" utility. {e.Message}");
                }
            }

            p.Close();
        }

        /// <summary>
        /// For every Visual Studio installation, there are a set of PlatformToolsets installed.
        /// This function will take the path of a visual studio installation, and return the PlatformToolset versions
        /// that are included with the installation indicated.
        /// </summary>
        /// <param name="vsInstallationPath">The path to the installation of Visual Studio, generated by the vswhere.exe utility.</param>
        /// <returns>A collection of PlatformToolset version strings (eg "v140," "v141", etc).</returns>
        private static IEnumerable<string> GetToolsetsInstalled(string vsInstallationPath)
        {
            // Used to store the unique versions of PlatformToolset that are included with the indicated VS installation.
            var installedPlatformToolsets = new HashSet<string>();

            // Check if the directory exists
            if (!Directory.Exists(vsInstallationPath))
            {
                Debug.LogWarning("Directory does not exist: " + vsInstallationPath);
            }

            // Regex pattern for matching the directory name (case insensitive)
            Regex regex = new(@"[vV]\d{3}", RegexOptions.IgnoreCase);

            // Get all subdirectories
            string[] subdirectories = Directory.GetDirectories(vsInstallationPath, "*", SearchOption.AllDirectories);

            foreach (string dir in subdirectories)
            {
                string name = new DirectoryInfo(dir).Name;
                if (regex.IsMatch(name))
                {
                    installedPlatformToolsets.Add(name.ToLower());
                }
            }

            return installedPlatformToolsets;
        }

        #endregion

        /// <summary>
        /// Attempts to retrieve an installation of Visual Studio on the system that can satisfy the requirements of the indicated solution file.
        /// </summary>
        /// <param name="solutionFilepath">The fully-qualified path to the solution file.</param>
        /// <param name="installation">The details of the installation of Visual Studio needed to compile the solution file (if one exists).</param>
        /// <returns>True if an appropriate installation of Visual Studio has been found that can compile the indicated solution file, false otherwise.</returns>
        private static bool TryGetCompatibleTools(string solutionFilepath, out VSInstallation installation)
        {
            // TODO: A good improvement to this method would be to limit the search of required PlatformToolset to the PropertyGroup that matches the configuration
            // Get all the platform toolsets required by the solution file.
            DetermineVSPrerequisites(
                solutionFilepath, out Version vsVersionRequired, out IList<string> toolsetsRequired
            );

            foreach (var vs in VisualStudioInstallations)
            {
                // only skip if the major versions don't match
                if (vsVersionRequired.Major != vs.Version.Major)
                {
                    continue;
                }

                // now check to see that the required platform toolsets are installed
                IList<string> missingToolsets = toolsetsRequired
                    .Where(t => (false == vs.Toolsets.Contains(t.ToLower())))
                    .ToList();

                foreach (var toolset in missingToolsets)
                {
                    Debug.LogWarning($"PlatformToolset \"{toolset}\" is missing for VS {vs.Version.Major}.");
                }

                // If there are no missing toolsets, then return this installation as the installation to use.
                if (0 == missingToolsets.Count)
                {
                    installation = vs;
                    return true;
                }
            }

            Debug.LogWarning($"Cannot find installation of Visual Studio of major version {vsVersionRequired.Major} containing the requisite platform toolsets.");
            installation = null;
            return false;
        }

        /// <summary>
        /// Given a project filepath, return a function capable of building the project at the indicated path.
        /// </summary>
        /// <param name="projectFilePath">The path to the project to be built.</param>
        /// <returns>A delegate function to use to build the project at the given filepath.</returns>
        /// <exception cref="NotImplementedException">If the project path contains a project type that is not supported, this will be thrown.</exception>
        private static BuildNativeLibraryDelegate FindNativeLibraryBuildFunction(string projectFilePath)
        {
            if (Path.GetExtension(projectFilePath) == ".sln")
            {
                return BuildFromSolutionFile;
            }
            else if (Path.GetFileName(projectFilePath) == "Makefile")
            {
                return BuildFromMakefile;
            }
            else
            {
                throw new NotImplementedException(
                    $"Unfamiliar with type of project file at \"{projectFilePath}\". Current supported project files are solution (.sln) files, or makefiles (Makefile).");
            }
        }

        /// <summary>
        /// Given a fully-qualified filepath to a project file, build the project and put the resulting binary
        /// files in the directory indicated.
        /// </summary>
        /// <param name="projectFilePath">Fully-qualified path to the project file to build.</param>
        /// <param name="binaryOutput">Fully-qualified path to output the results to.</param>
        /// <exception cref="BuildFailedException">If building fails, a BuildFailedException is thrown.</exception>
        public static bool BuildNativeLibrary(string projectFilePath, string binaryOutput)
        {
            Debug.Log($"Building native libraries from project file {projectFilePath}");

            var buildDelegate = FindNativeLibraryBuildFunction(projectFilePath);

            return buildDelegate(projectFilePath, binaryOutput);
        }

        /// <summary>
        /// Given a fully qualified path to a solution file, build the project using the "Release" configuration,
        /// placing the resulting binary files in the directory indicated.
        /// </summary>
        /// <param name="solutionFilePath">Fully-qualified path to the solution file to build.</param>
        /// <param name="binaryOutput">Fully-qualified path to output the results to.</param>
        /// <exception cref="BuildFailedException">Thrown if building fails.</exception>
        /// <returns>True if the build was successful, false otherwise.</returns>
        private static bool BuildFromSolutionFile(string solutionFilePath, string binaryOutput)
        {
            if (!TryGetCompatibleTools(solutionFilePath, out VSInstallation tools))
            {
                //Debug.LogError($"Cannot build native code libraries.");
                //throw new BuildFailedException("Build failed. View log for details.");
                return false;
            }

            string configuration = "Release";
            if (EditorUserBuildSettings.development)
            {
                configuration = "Debug";
            }

            // construct the msbuild command
            string msBuildCommand = $"msbuild \"{solutionFilePath}\"" +
                                    $" /t:Clean;Rebuild" +
                                    $" /p:Configuration={configuration}" +
                                    // TODO: Re-implement GetPlatformString to re-enable this component?
                                    // NOTE: This may not be necessary, because typically the platform to build against
                                    // is defined within the project and/or solution file.
                                    //$" /p:Platform={PlatformManager.GetPlatformString()}" +
                                    $" /p:OutDir={binaryOutput}";

            // TODO: Consider running this asynchronously? If only for better user feedback during build.
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{tools.VSDevCmdPath}\" &&  {msBuildCommand}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            string output = process?.StandardOutput.ReadToEnd();
            string errors = process?.StandardError.ReadToEnd();

            process?.WaitForExit();

            if (0 == process?.ExitCode)
            {
                Debug.Log(output);
                Debug.Log($"Succeeded in building native code library \"{solutionFilePath}\"");
                return true;
            }
            else
            {
                // msbuild might succeed to not build - it did its job if it determines that it cannot build
                errors = "" == errors ? output : errors;
                Debug.LogError(errors);
                Debug.LogError($"Failed to build solution \"{solutionFilePath}\"");
                //throw new BuildFailedException($"Failed to build solution \"{solutionFilePath}\"");
                return false;
            }
        }

        /// <summary>
        /// Given a fully qualified path to a Makefile, build the project, placing the resulting
        /// binary files in the directory indicated.
        /// </summary>
        /// <param name="makefileFilePath">Fully-qualified path to the Makefile to build.</param>
        /// <param name="binaryOutput">Fully-qualified path to output the results to.</param>
        /// <exception cref="BuildFailedException">Thrown if building fails.</exception>
        private static bool BuildFromMakefile(string makefileFilePath, string binaryOutput)
        {
            // Check for required packages and install if missing
            const string checkAndInstallPackagesCmd = "bash -c \"which clang || sudo apt-get update && sudo apt-get install -y clang; " +
                                                      "which make || sudo apt-get install -y make;\"";

            string makeFilePath = Path.GetDirectoryName(makefileFilePath);

            // Command to run the makefile
            string makeCmd = $"bash -c \"cd \"{makeFilePath}\" && make\"";

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (!CheckWSLInstalled())
            {
                Debug.LogError("Windows Subsystem for Linux is not installed. On Windows, WSL is required in order to properly compile native libraries that are required for running the EOS Plugin on the Linux platform. Please install WSL and rebuild.");
                throw new BuildFailedException("Failed to run Makefile. Please see log for further details.");
            }
#endif

            // Execute commands
            RunBashCommand(checkAndInstallPackagesCmd);
            RunBashCommand(makeCmd);

            // TODO: Properly implement check here. False is returned to bring attention to this needing to be implemented.
            return false;
        }

        /// <summary>
        /// Determines whether Windows Subsystem for Linux is installed or not on this machine.
        /// </summary>
        /// <returns>True if WSL is installed, False otherwise.</returns>
        private static bool CheckWSLInstalled()
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "cmd.exe",
                Arguments = "/c wsl -l",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // If the command executes successfully, WSL is installed
            return process.ExitCode == 0 && string.IsNullOrEmpty(err);
        }

        /// <summary>
        /// Runs a command in Windows Subsystem for Linux.
        /// </summary>
        /// <param name="command">The command to run in WSL</param>
        private static void RunBashCommand(string command)
        {
            ProcessStartInfo startInfo = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo.FileName = "wsl.exe";
                startInfo.Arguments = command;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo.FileName = "/bin/bash";
                startInfo.Arguments = $"-c \"{command}\"";
            }
            else
            {
                Debug.LogError("Unsupported OS");
                return;
            }

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            using Process process = Process.Start(startInfo);
            using StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log(result);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }

        /// <summary>
        /// Gets a list of strings representing each toolset platform required as indicated by the project files included in the solution file indicated.
        /// </summary>
        /// <param name="solutionFilePath">Fully-qualified path to a Visual Studio solution file.</param>
        /// <param name="vsVersion">The version of VS required by the solution file.</param>
        /// <param name="toolsets">The toolsets required by the solution file.</param>
        /// <exception cref="FileNotFoundException">Thrown if either the solution file does not exist, or if any of it's project files do not exist.</exception>
        private static void DetermineVSPrerequisites(string solutionFilePath, out Version vsVersion,
            out IList<string> toolsets)
        {
            // if there is no solution file
            if (string.IsNullOrEmpty(solutionFilePath) || false == File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException($"Could not find solution file \"{solutionFilePath}\"");
            }

            // Get the directory for the solution, because all project files will be relative to it
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath);

            string[] solutionLines = File.ReadAllLines(solutionFilePath);

            // first find the version of visual studio required
            vsVersion = ParseSolutionFileForVSVersion(solutionLines);
            var toolsetHashSet = new HashSet<string>();

            // NOTE: This will fail if the native binary contains project files other than C++ Visual Studio projects
            Regex projRegex = new("\"[^\"]+\\.vcxproj\"", RegexOptions.Compiled);

            // used to keep track of error messages in the event that the solution references project files that do not exist.
            // this is because we want to report all missing toolsets, not just the first one
            IList<string> missingProjectFiles = new List<string>();

            foreach (string solutionLine in solutionLines)
            {
                Match solutionMatch = projRegex.Match(solutionLine);

                // skip the line if it's not a match for a filepath to a .csproj file.
                if (!solutionMatch.Success)
                    continue;

                // construct the path to the project file.
                string projectPath = Path.Combine(solutionDirectory, solutionMatch.Groups[0].Value.Replace("\"", ""));

                // if the project file does not exist
                if (string.IsNullOrEmpty(projectPath) || false == File.Exists(projectPath))
                {
                    // store an exception to throw later - we will collect them to report all that are missing instead of just
                    // failing on the first one.
                    missingProjectFiles.Add(
                        $"Could not find project file \"{projectPath}\" referenced in solution file \"{solutionFilePath}\"");
                    continue;
                }

                var toolsetsRequiredByProject = ParseProjectFileForToolsets(projectPath);
                foreach (var toolset in toolsetsRequiredByProject)
                {
                    toolsetHashSet.Add(toolset);
                }
            }

            // if there were missing project files
            if (0 != missingProjectFiles.Count)
            {
                throw new FileNotFoundException(string.Join(Environment.NewLine, missingProjectFiles));
            }

            toolsets = toolsetHashSet.ToList();
        }

        /// <summary>
        /// Scans the given lines of a Visual Studio Solution file to determine which version of Visual Studio is required.
        /// </summary>
        /// <param name="contents">Line by line contents of a solution file.</param>
        /// <returns>The version of Visual Studio required by the solution file, as indicated by it's contents.</returns>
        private static Version ParseSolutionFileForVSVersion(string[] contents)
        {
            Regex vsVersionRegex = new(@"^VisualStudioVersion\s*=\s*\b\d+(\.\d+)*\b");
            Version version = null;
            foreach (string line in contents)
            {
                Match versionMatch = vsVersionRegex.Match(line);
                if (!versionMatch.Success)
                    continue;

                // once we find the VS version we can continue
                version = new Version(versionMatch.Groups[0].Value.Split("=")[1].Trim());
                break;
            }

            return version;
        }

        /// <summary>
        /// Given a fully-qualified path to a project file, parses the project file to determine which toolsets
        /// it requires in order to be built.
        /// </summary>
        /// <param name="projectFilepath">Fully-qualified path to a project file.</param>
        /// <returns>Collection of string representations of the toolsets that the indicated project file requires in order to compile properly.</returns>
        private static IEnumerable<string> ParseProjectFileForToolsets(string projectFilepath)
        {
            var toolsets = new HashSet<string>();
            // scan the project file for platform toolsets.
            string[] projectLines = File.ReadAllLines(projectFilepath);
            Regex toolsetRegex = new("<PlatformToolset>(.*)</PlatformToolset>", RegexOptions.Compiled);
            foreach (string projectLine in projectLines)
            {
                Match projectMatch = toolsetRegex.Match(projectLine);

                // skip if it's not a match
                if (!projectMatch.Success)
                    continue;

                // add the toolset to the list of toolsets.
                toolsets.Add(projectMatch.Groups[1].Value.ToLower());
            }

            return toolsets.ToList();
        }


        /// <summary>
        /// Builds native binary files given a mapping of project file to binary file, and an output directory.
        /// </summary>
        /// <param name="projectToBinaryMap">Dictionary that maps project file (sln or Makefile) with binary output files.</param>
        /// <param name="outputDirectory">The directory to output the native binaries to.</param>
        /// <param name="rebuild">Whether to rebuild the libraries each time.</param>
        public static void BuildNativeBinaries(IDictionary<string, string[]> projectToBinaryMap, string outputDirectory, bool rebuild = false)
        {
            var projectsToBuild = new HashSet<string>();

            IDictionary<string, string> cachedProjectOutput = new Dictionary<string, string>();

            foreach (string projectFile in projectToBinaryMap.Keys)
            {
                string[] binaryFiles = projectToBinaryMap[projectFile];

                if (binaryFiles.All(File.Exists))
                {
                    Debug.Log($"Caching the existing binaries for project \"{projectFile}\".");
                    var cachedDirectory = CacheExistingBinaries(binaryFiles);
                    cachedProjectOutput.Add(projectFile, cachedDirectory);
                }

                if (rebuild)
                {
                    Debug.LogWarning($"Because rebuild = {rebuild}, project file \"{projectFile}\" has been marked for rebuilding.");
                    projectsToBuild.Add(projectFile);
                    continue;
                }

                var missingBinaryFiles = binaryFiles.Where(outputFile => !File.Exists(outputFile)).ToList();
                if (missingBinaryFiles.Count > 0)
                {
                    StringBuilder missingBinaryFilesMessage = new($"Project file \"{projectFile}\" has been marked for rebuilding, because the following binary files are missing: \n");
                    foreach (var missingBinaryFile in missingBinaryFiles)
                    {
                        missingBinaryFilesMessage.AppendLine($"\"{missingBinaryFile}\"");
                    }

                    Debug.LogWarning(missingBinaryFilesMessage.ToString());

                    projectsToBuild.Add(projectFile);
                    continue;
                }
            }

            // Build any project that needs to be built.
            foreach (string project in projectsToBuild)
            {
                bool projectBuildSuccessfully = BuildUtility.BuildNativeLibrary(project, outputDirectory);

                // if the build was successful, skip processing
                if (projectBuildSuccessfully) { continue; }

                // If a cache exists containing previous binaries, restore the cache.
                if (cachedProjectOutput.TryGetValue(project, out string cacheDirectory))
                {
                    Debug.Log($"Restoring binaries that were cached for project \"{project}\".");
                    RestoreCachedBinaries(projectToBinaryMap, project, cacheDirectory);
                }
            }
        }

        /// <summary>
        /// Takes an enumerable of filepaths and moves them to a temporary directory, returning the path to that directory.
        /// </summary>
        /// <param name="files">The filepaths to move.</param>
        /// <returns>The path to the temporary directory in which to store the files temporarily.</returns>
        private static string CacheExistingBinaries(IEnumerable<string> files)
        {
            if (!FileSystemUtility.TryGetTempDirectory(out string temporaryDirectory))
            {
                Debug.LogWarning("Could not create temporary directory to cache existing binaries.");
                return string.Empty;
            }

            foreach (string filepath in files)
            {
                string filename = Path.GetFileName(filepath);
                File.Move(filepath, Path.Combine(temporaryDirectory, filepath));
            }

            return temporaryDirectory;
        }

        /// <summary>
        /// Restores cached binaries to their original location.
        /// </summary>
        /// <param name="projectFileToBinaryMap">Dictionary that maps project file to binary output files.</param>
        /// <param name="project">The project whose binary files should be restored.</param>
        /// <param name="cacheDirectory">The directory in which the binary files were cached.</param>
        private static void RestoreCachedBinaries(IDictionary<string, string[]> projectFileToBinaryMap, string project, string cacheDirectory)
        {
            // To move cached binaries back to original locations, first a filename-to-directory lookup map must be made
            Dictionary<string, string> fileToDestination = new();
            foreach (var outputPath in projectFileToBinaryMap[project])
            {
                string filename = Path.GetFileName(outputPath);
                string destination = Path.GetDirectoryName(outputPath);
                fileToDestination.Add(filename, destination);
            }

            foreach (var cachedFile in Directory.GetFiles(cacheDirectory))
            {
                string cachedFileName = Path.GetFileName(cachedFile);
                File.Move(cachedFile, Path.Combine(fileToDestination[cachedFileName], cachedFileName));
            }
        }

    }
}
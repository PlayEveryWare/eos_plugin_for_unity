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

namespace PlayEveryWare.EpicOnlineServices.Build
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build;
    using Debug = UnityEngine.Debug;

    public static class BuildUtility
    {
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

            string vsWhereOutput = outputBuilder.ToString();

            // Continue with processing vsWhereOutput...
            JArray installations = JArray.Parse(vsWhereOutput);

            // Used to store the different installations of VS on the system.
            VisualStudioInstallations = new List<VSInstallation>();

            foreach (var installation in installations)
            {
                try
                {
                    Version version = new(installation["installationVersion"]?.ToString() ?? string.Empty);
                    string installPath = installation["installationPath"]?.ToString();

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

                installation = vs;
                return true;
            }

            Debug.LogWarning($"Cannot find installation of Visual Studio of major version {vsVersionRequired.Major}");
            installation = null;
            return false;
        }

        /// <summary>
        /// Given a fully-qualified filepath to a solution file, build the solution with the 'Release' configuration.
        /// This only works on Windows, if the editor is running on a different platform, and this function is called, an
        /// exception of "NotImplementedException" will be thrown.
        /// </summary>
        /// <param name="solutionFilePath">Fully-qualified path to the solution to build.</param>
        /// <param name="binaryOutput">Fully-qualified path to output the results to.</param>
        /// <exception cref="BuildFailedException">If building the solution fails, a BuildFailedException is thrown.</exception>
        public static void BuildNativeLibrary(string solutionFilePath, string binaryOutput)
        {
            Debug.Log($"Building native libraries from solution {solutionFilePath}");

            if (!TryGetCompatibleTools(solutionFilePath, out VSInstallation tools))
            {
                Debug.LogError($"Cannot build native library.");
                throw new BuildFailedException("Build failed. View log for details.");
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
                                    // TODO: Re-implement GetMSPlatformString to re-enable this component.
                                    //$" /p:Platform={PlatformManager.GetMSPlatformString()}" +
                                    $" /p:OutDir={binaryOutput}";

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{tools.VSDevCmdPath}\" &&  {msBuildCommand}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                string output = process?.StandardOutput.ReadToEnd();
                string errors = process?.StandardError.ReadToEnd();

                process?.WaitForExit();

                if (0 == process?.ExitCode)
                {
                    Debug.Log(output);
                    Debug.Log($"Succeeded in building native code library \"{solutionFilePath}\"");
                }
                else
                {
                    // msbuild might succeed to not build - it did it's job if it determines that it cannot build
                    errors = "" == errors ? output : errors;
                    Debug.LogError(errors);
                    Debug.LogError($"Failed to build solution \"{solutionFilePath}\"");
                    throw new BuildFailedException($"Failed to build solution \"{solutionFilePath}\"");
                }
            }
        }

        /// <summary>
        /// Gets a list of strings representing each toolset platform required as indicated by the project files included in the solution file indicated.
        /// </summary>
        /// <param name="solutionFilePath">Fully-qualified path to a Visual Studio solution file.</param>
        /// <param name="vsVersion">The version of VS required by the solution file.</param>
        /// <param name="toolsets">The toolsets required by the solution file.</param>
        /// <exception cref="FileNotFoundException">Thrown if either the solution file does not exist, or if any of it's project files do not exist.</exception>
        private static void DetermineVSPrerequisites(string solutionFilePath, out Version vsVersion, out IList<string> toolsets)
        {
            // if there is no solution file
            if (string.IsNullOrEmpty(solutionFilePath) || false == File.Exists(solutionFilePath))
                throw new FileNotFoundException($"Could not find solution file \"{solutionFilePath}\"");

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
        /// it requires in order to be build.
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
    }
}
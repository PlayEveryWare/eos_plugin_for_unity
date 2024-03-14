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

namespace PlayEveryWare.EpicOnlineServices.Build
{
    using PlayEveryWare.EpicOnlineServices.Editor.Config;
    using PlayEveryWare.EpicOnlineServices.Editor;
    using UnityEditor.Build.Reporting;
    using System.IO;
    using UnityEditor.Build;
    using UnityEngine;
    using Utility;

    public class WindowsBuilder : PlatformSpecificBuilder
    {
        private const string ProjectPathToEOSBootstrapperTool = "tools/bin/EOSBootstrapperTool.exe";

        public WindowsBuilder() : base("Plugins/Windows")
        {
            // TODO/NOTE: Add support for 32-bit project to binary file mapping?
            AddProjectFileToBinaryMapping(
                "DynamicLibraryLoaderHelper/DynamicLibraryLoaderHelper.sln",
                "x64/DynamicLibraryLoaderHelper-x64.dll",
                "x64/GfxPluginNativeRender-x64.dll");
        }

        public override void PostBuild(BuildReport report)
        {
            base.PostBuild(report);

            ConfigureAndInstallBootstrapper(report);
        }

        private static void ConfigureAndInstallBootstrapper(BuildReport report)
        {
            /*
             * NOTE:
             *
             * The following code functions properly, but exposes some poor design with
             * respect to the build process. For starters, in order to determine whether
             * EAC is installed, this function must instantiate a config editor. It would
             * be nice if there was a way to query the config values via a static property
             * like this:
             *
             * if (ToolsConfig.UseEAC) { ... }
             *
             * However, that does not actually answer the question that needs answering
             * in the context of installing the bootstrapper. This answers the question
             * "Is EAC supposed to be configured?" Because if the answer is yes, then
             * the bootstrapper tool needs to use EACLauncher.exe as the target.
             *
             * The reason it is insufficient to answer the question "Is EAC supposed to be
             * configured?" for this purpose is that it doesn't determine if EAC *IS*
             * configured. This current solution relies on the fact that the steps happen
             * to be in-order.
             *
             * Rectifying these design flaws is beyond the scope of what needs to be done
             * right now, but this note remains for the sake of future Build engineers
             * wishing to improve the system, and future developers who may encounter
             * build issues surrounding the Bootstrapper and/or the Easy Anti-Cheat system
             * that are difficult to diagnose.
             */

            // Determine whether or not to install EAC
            var editorToolsConfigSection = new ToolsConfigEditor();
            bool useEAC = false;

            editorToolsConfigSection.Load();

            ToolsConfig editorToolConfig = editorToolsConfigSection.GetConfig().Data;
            if (editorToolConfig != null)
            {
                useEAC = editorToolConfig.useEAC;
            }

            string bootstrapperName = null;
            if (editorToolConfig != null)
            {
                bootstrapperName = editorToolConfig.bootstrapperNameOverride;
            }

            if (string.IsNullOrWhiteSpace(bootstrapperName))
            {
                bootstrapperName = "EOSBootstrapper.exe";
            }

            if (!bootstrapperName.EndsWith(".exe"))
            {
                bootstrapperName += ".exe";
            }

            string pathToEOSBootStrapperTool = Path.Combine(PackageFileUtility.GetProjectPath(), ProjectPathToEOSBootstrapperTool);

            string installDirectory = Path.GetDirectoryName(report.summary.outputPath);

            string bootstrapperTarget =
                useEAC ? "EACLauncher.exe" : Path.GetFileName(report.summary.outputPath);

            InstallBootStrapper(bootstrapperTarget, installDirectory, pathToEOSBootStrapperTool,
                bootstrapperName);
        }

        private static void InstallBootStrapper(string appFilenameExe, string installDirectory,
            string pathToEOSBootStrapperTool, string bootstrapperFileName)
        {
            string installPathForEOSBootStrapper = Path.Combine(installDirectory, bootstrapperFileName);
            string workingDirectory = EACPostBuild.GetPathToEOSBin();
            string bootStrapperArgs = ""
                                      + $" --output-path \"{installPathForEOSBootStrapper}\""
                                      + $" --app-path \"{appFilenameExe}\"";

            var procInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pathToEOSBootStrapperTool, Arguments = bootStrapperArgs,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new System.Diagnostics.Process { StartInfo = procInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.Log(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.LogError(e.Data);
                }
            };

            if (false == process.Start())
            {
                throw new BuildFailedException($"Failed to run the BootstrapperTool \"{pathToEOSBootStrapperTool}\".");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        }
    }
}
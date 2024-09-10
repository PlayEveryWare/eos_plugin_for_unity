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

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
#if !EOS_DISABLE
    using Epic.OnlineServices.Platform;
#endif
    using Config;
    using Config = EpicOnlineServices.Config;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// WindowsBuilder for 64-bit deployment.
    /// </summary>
    public class WindowsBuilder64 : WindowsBuilder
    {
        public WindowsBuilder64() : base("Plugins/Windows/x64", BuildTarget.StandaloneWindows64)
        {
            AddProjectFileToBinaryMapping(
                "DynamicLibraryLoaderHelper/DynamicLibraryLoaderHelper.sln",
                "DynamicLibraryLoaderHelper-x64.dll",
                "GfxPluginNativeRender-x64.dll");
        }
    }

    /// <summary>
    /// WindowsBuilder for 32-bit deployment.
    /// </summary>
    public class WindowsBuilder32 : WindowsBuilder
    {
        public WindowsBuilder32() : base("Plugins/Windows/x86", BuildTarget.StandaloneWindows)
        {
            // TODO: These libraries do not appear to be building properly - and the process
            //       also appears to delete the x64 libraries. It's possible that both things
            //       are caused by some other process.
            AddProjectFileToBinaryMapping(
                "DynamicLibraryLoaderHelper/DynamicLibraryLoaderHelper.sln",
                "DynamicLibraryLoaderHelper-x86.dll",
                "GfxPluginNativeRender-x86.dll");
        }
    }

    /// <summary>
    /// Base implementation for a WindowsBuilder. Cannot be instantiated, but is used
    /// as base implementation for both 64 and 32 bit flavors of Windows.
    /// </summary>
    public abstract class WindowsBuilder : PlatformSpecificBuilder
    {
        private const string ProjectPathToEOSBootstrapperTool = "tools/bin/EOSBootstrapperTool.exe";

        protected WindowsBuilder(string nativeBinaryDirectory, params BuildTarget[] buildTargets) : base(nativeBinaryDirectory, buildTargets) {   }

        public override void PostBuild(BuildReport report)
        {
            base.PostBuild(report);

            ConfigureAndInstallBootstrapper(report);
        }

        private static async void ConfigureAndInstallBootstrapper(BuildReport report)
        {
#if !EOS_DISABLE
            // Determine if 'DisableOverlay' is set in Platform Flags. If it is, then the EOSBootstrapper.exe is not included in the build,
            // because without needing the overlay, the EOSBootstrapper.exe is not useful to users of the plugin
            EOSConfig configuration = await Config.GetAsync<EOSConfig>();
            PlatformFlags configuredFlags = configuration.GetPlatformFlags();
            if (configuredFlags.HasFlag(PlatformFlags.DisableOverlay))
            {
                Debug.Log($"The '{nameof(PlatformFlags.DisableOverlay)}' flag has been configured, EOSBootstrapper.exe will not be included in this build.");
                return;
            }
#endif
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

            // Determine whether to install EAC
            
            ToolsConfig toolsConfig = await Config.GetAsync<ToolsConfig>();

            string bootstrapperName = null;
            if (toolsConfig != null)
            {
                bootstrapperName = toolsConfig.bootstrapperNameOverride;
            }

            if (string.IsNullOrWhiteSpace(bootstrapperName))
            {
                bootstrapperName = "EOSBootstrapper.exe";
            }

            if (!bootstrapperName.EndsWith(".exe"))
            {
                bootstrapperName += ".exe";
            }

            string pathToEOSBootStrapperTool = Path.Combine(EACUtility.GetPathToEOSBin(), "EOSBootstrapperTool.exe");

            string installDirectory = Path.GetDirectoryName(report.summary.outputPath);

            string bootstrapperTarget = toolsConfig.useEAC ? "EACLauncher.exe" : Path.GetFileName(report.summary.outputPath);

            InstallBootStrapper(bootstrapperTarget, installDirectory, pathToEOSBootStrapperTool,
                bootstrapperName);
        }

        private static void InstallBootStrapper(string appFilenameExe, string installDirectory,
            string pathToEOSBootStrapperTool, string bootstrapperFileName)
        {
            string installPathForEOSBootStrapper = Path.Combine(installDirectory, bootstrapperFileName);
            string workingDirectory = EACUtility.GetPathToEOSBin();
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
                    Debug.Log($"BootstrapperTool stdout: \"{e.Data}\"");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.LogError($"BootstrapperTool stderr: \"{e.Data}\"");
                }
            };

            if (false == process.Start())
            {
                throw new BuildFailedException(
                    $"Failed to run the BootstrapperTool \"{pathToEOSBootStrapperTool}\". Please see log for more details."
                    );
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        }
    }
}
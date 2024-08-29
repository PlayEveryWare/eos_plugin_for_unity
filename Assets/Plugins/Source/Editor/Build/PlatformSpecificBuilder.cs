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
    using Config;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using Debug = UnityEngine.Debug;
    using UnityEditor;
    using System;
    using Config = EpicOnlineServices.Config;

    public abstract class PlatformSpecificBuilder : IPlatformSpecificBuilder,
        IPreprocessBuildWithReport
    {
        /// <summary>
        /// For every platform, there are certain binary files that represent native console-specific implementations of the EOS plugin.
        /// Each of these binaries maps to a solution file that must be compiled in order for the project to function properly on the
        /// platform. This dictionary stores the relationship between the fully-qualified path to the project file (.sln or Makefile
        /// depending on the platform) and the fully-qualified path to the binary that is expected.
        /// </summary>
        private IDictionary<string, string[]> _projectFileToBinaryFilesMap;

        /// <summary>
        /// Fully-qualified path to the directory that should contain the output from the native code.
        /// </summary>
        private readonly string _nativeCodeOutputDirectory;

        /// <summary>
        /// Fully-qualified path to the directory containing all the native code solution directories.
        /// </summary>
        private readonly static string NativeCodeDirectory = Path.Combine(Application.dataPath, "../lib/NativeCode");

        /// <summary>
        /// Contains a mapping from BuildTarget to a specific platform builder.
        /// </summary>
        protected static IDictionary<BuildTarget, Type> s_builders =
            new Dictionary<BuildTarget, Type>();

        /// <summary>
        /// Callback Order is set to 0 because the platform specific builders
        /// need to execute first in-order to register themselves as builders
        /// for their respective platforms.
        /// </summary>
        public int callbackOrder => 1;

        /// <summary>
        /// Stores the targets for which the builder can be used.
        /// </summary>
        private BuildTarget[] _buildTargets;

        /// <summary>
        /// Constructs a new PlatformSpecificBuilder script.
        /// </summary>
        /// <param name="nativeCodeOutputDirectory">The filepath to the location of the binary files, relative to the Assets directory.</param>
        /// <param name="buildTargets">The BuildTargets that this builder runs for.</param>
        protected PlatformSpecificBuilder(string nativeCodeOutputDirectory, params BuildTarget[] buildTargets)
        {
            _nativeCodeOutputDirectory = Path.Combine(
                Application.dataPath,
                nativeCodeOutputDirectory);

            _projectFileToBinaryFilesMap = new Dictionary<string, string[]>();
            _buildTargets = buildTargets;
        }

        /// <summary>
        /// This pre-process build step is designed to take place before the
        /// BuildRunner executes because it is here that which platform builder
        /// to use is determined.
        /// </summary>
        /// <param name="report">The prebuild report.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            // If the platform being built is one of the platforms that this
            // builder builds to, then set this as the builder with the
            // BuildRunner.
            if (_buildTargets.Contains(report.summary.platform))
            {
                // Note that in this context, despite being within an abstract
                // class, the most derived instance will be returned when
                // "this" is accessed.
                BuildRunner.Builder = this;
            }
        }

        /// <summary>
        /// Adds a mapping of solution file to expected binary file output.
        /// </summary>
        /// <param name="projectFile">Path of the project file relative to the NativeCode directory (lib/NativeCode).</param>
        /// <param name="binaryFiles">Paths of any expected binary files, relative to the native code output directory defined for the builder.</param>
        protected void AddProjectFileToBinaryMapping(string projectFile, params string[] binaryFiles)
        {
            string fullyQualifiedOutputPath = Path.Combine(
                Application.dataPath, 
                _nativeCodeOutputDirectory);

            string[] fullyQualifiedBinaryPaths = new string[binaryFiles.Length];
            for (int i = 0; i < binaryFiles.Length; i++)
            {
                fullyQualifiedBinaryPaths[i] = Path.Combine(fullyQualifiedOutputPath, binaryFiles[i]);
            }

            _projectFileToBinaryFilesMap.Add(Path.Combine(NativeCodeDirectory, projectFile), fullyQualifiedBinaryPaths);
        }

        /// <summary>
        /// Implement this function on a per-platform basis to provide custom logic for the platform being compiled.
        /// Any overriding implementations should first call the base implementation.
        /// </summary>
        /// <param name="report"></param>
        public virtual void PreBuild(BuildReport report)
        {
            // Check to make sure that the platform configuration exists
            CheckPlatformConfiguration();

            // Configure the version numbers per user defined preferences
            ConfigureVersion();

            // Check to make sure that the binaries for the platform exist, and build them if necessary.
            CheckPlatformBinaries();
        }

        /// <summary>
        /// Implement this function on a per-platform basis to provide custom logic for the platform being compiled.
        /// Any overriding implementations should first call the base implementation.
        /// </summary>
        /// <param name="report"></param>
        public virtual void PostBuild(BuildReport report)
        {
            // The only standalone platforms that are supported are WIN/OSX/Linux
            if (IsStandalone())
            {
                // Configure easy-anti-cheat.
                EACUtility.ConfigureEAC(report);
            }
        }

        public virtual void BuildNativeCode()
        {
            // Only try to build native code if all the project files exist.
            // This is mostly to prevent an attempt to build native binaries when the plugin is deployed
            // via UPM.
            if (_projectFileToBinaryFilesMap.Keys.All(File.Exists))
            {
                BuildUtility.BuildNativeBinaries(_projectFileToBinaryFilesMap, _nativeCodeOutputDirectory, true);
            }
            else
            {
                Debug.Log("Project files for native code compilation not found, skipping.");
            }
        }

        /// <summary>
        /// Check for platform specific binaries. If this method is overridden, be sure to start by calling the
        /// base implementation, because it will check for the presence of config files, and handle checking for
        /// native code and compiling it for you, and you can then add additional checks in the overriden implementation.
        /// </summary>
        protected virtual void CheckPlatformBinaries()
        {
            // Note: This compile conditional exists because it only makes sense
            // to look for visual studio installations if running on windows.
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            BuildUtility.FindVSInstallations();
#endif

            Debug.Log("Checking for platform-specific prerequisites.");

            // Validate the configuration for the platform
            BuildUtility.ValidatePlatformConfiguration();

            // Only try and build the native libraries when not deployed as a UPM.
            if (false == BuildUtility.DeployedAsUPM)
            {
                // Build any native libraries that need to be built for the platform
                // TODO: Consider having the "rebuild" be a setting users can determine.
                BuildNativeCode();

                // Validate that the binaries built are now in the correct location
                ValidateNativeBinaries();
            }
        }

        /// <summary>
        /// Checks to make sure that the platform configuration file exists where it is expected to be
        /// TODO: Add configuration validation.
        /// </summary>
        private static void CheckPlatformConfiguration()
        {
            string configFilePath = PlatformManager.GetConfigFilePath();
            if (!File.Exists(configFilePath))
            {
                throw new BuildFailedException($"Expected config file \"{configFilePath}\" for platform {PlatformManager.GetFullName(PlatformManager.CurrentPlatform)} does not exist.");
            }
        }

        /// <summary>
        /// Completes all configuration tasks.
        /// </summary>
        private static void ConfigureVersion()
        {
            AutoSetProductVersion();

            const string packageVersionPath = "Assets/Resources/eosPluginVersion.asset";
            string packageVersion = EOSPackageInfo.Version;
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            TextAsset versionAsset = new(packageVersion);
            AssetDatabase.CreateAsset(versionAsset, packageVersionPath);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Determines whether the Application Version is supposed to be used as the product version, and (if so) sets it accordingly.
        /// </summary>
        private static async void AutoSetProductVersion()
        {
            var eosConfig = await Config.GetAsync<EOSConfig>();
            var prebuildConfig = await Config.GetAsync<PrebuildConfig>();
            var previousProdVer = eosConfig.productVersion;

            if (prebuildConfig.useAppVersionAsProductVersion)
            {
                eosConfig.productVersion = Application.version;
            }

            if (previousProdVer != eosConfig.productVersion)
            {
                await eosConfig.WriteAsync(true);
            }
        }

        /// <summary>
        /// Checks to see that native code for the platform has been compiled.
        /// Will list all missing files in the error log before throwing an exception.
        /// </summary>
        /// <exception cref="BuildFailedException">Will be thrown if any expected output binary file is missing.</exception>
        private void ValidateNativeBinaries()
        {
            bool prerequisitesSatisfied = true;

            foreach (string projectFile in _projectFileToBinaryFilesMap.Keys)
            {
                foreach (string outputFile in _projectFileToBinaryFilesMap[projectFile])
                {
                    // skip if the output file exists
                    if (File.Exists(outputFile)) continue;

                    // make sure to log all the missing files / project file pairs before throwing an exception
                    Debug.LogError($"Required file \"{outputFile}\" which is output from project file \"{projectFile}\" is missing.");
                    prerequisitesSatisfied = false;
                }
            }

            if (!prerequisitesSatisfied)
            {
                throw new BuildFailedException($"Prerequisites for platform were not met. View logs for details.");
            }
        }

        /// <summary>
        /// When building on Windows, msbuild has a flag specifying the platform to build towards. Each
        /// class that derives from PlatformSpecificBuilder must define the value to pass msbuild for its
        /// respective platform. These strings can be confidential on unreleased or code-named platforms,
        /// so it is important for security reasons that only implementing classes contain the value.
        /// </summary>
        /// <returns>The appropriate string to pass to msbuild.</returns>
        public virtual string GetPlatformString()
        {
            return string.Empty;
        }

        /// <summary>
        /// Determines if the build is a standalone build.
        /// </summary>
        /// <returns>True if the build is standalone, false otherwise.</returns>
        protected bool IsStandalone()
        {
            // It is unclear from the Unity documentation what the meaning of "UNITY_STANDALONE" is,
            // although it can be reasonably inferred from context that it will be defined if any
            // of the following specific standalone scripting defines exist, for the sake of future-
            // proofing the scenario where a new standalone platform is introduced, each of the three
            // standalone platforms that the EOS Plugin current supports are explicitly checked here.
#if UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
            return true;
#else
            return false;
#endif
        }
    }
}
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
    using Editor;
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    
    public class BuildRunner : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public static IPlatformSpecificBuilder GetBuilder()
        {
            IPlatformSpecificBuilder builder = null;
            // NOTE: Try to make it so that (regarding build tasks) this is the ONLY place where compiler defines are
            //       utilized. Reducing the number of places in the code where this happens will limit the number of
            //       potential failure points, and keep the build process as linear as possible.

            try
            {
#if UNITY_IOS
                builder = new IOSBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.iOS;
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                builder = new LinuxBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.Linux;
#elif UNITY_ANDROID
                builder = new AndroidBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.Android;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                builder = new MacOSBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.macOS;
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception while creating builder: {e.Message}");
            }

            return builder;
        }

        /// <summary>
        /// For EOS Plugin, this is the only place where anything can happen before build.
        /// </summary>
        /// <param name="report">The pre-process build report.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            // Check to make sure that the configuration file for the platform being 
            // built against exists.
            CheckPlatformConfiguration();

            // Do the various version-related tasks that need to be configured.
            ConfigureVersions();

            // perform the pre-build task for the platform using it's builder.
            GetBuilder()?.PreBuild(report);
        }

        /// <summary>
        /// For EOS Plugin, this is the only place where anything can happen as a post build task
        /// </summary>
        /// <param name="report">The report from the post-process build.</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            // perform the post-build task for the platform using it's builder.
            GetBuilder()?.PostBuild(report);

            // Configure EAC
            EACPostBuild.ConfigureEAC(report);
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
        private static void ConfigureVersions()
        {
            AutoSetProductVersion();

            const string packageVersionPath = "Assets/Resources/eosPluginVersion.asset";
            string packageVersion = EOSPackageInfo.GetPackageVersion();
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
        private static void AutoSetProductVersion()
        {
#if !EOS_DISABLE
            var eosVersionConfigSection = new PrebuildConfigEditor();

            eosVersionConfigSection.Load();

            string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSPackageInfo.ConfigFileName);
            var eosConfigFile = new ConfigHandler<EOSConfig>(configFilePath);
            eosConfigFile.Read();

            var previousProdVer = eosConfigFile.Data.productVersion;
            var currentSectionConfig = eosVersionConfigSection.GetConfig().Data;

            if (currentSectionConfig == null)
            {
                return;
            }

            if (currentSectionConfig.useAppVersionAsProductVersion)
            {
                eosConfigFile.Data.productVersion = Application.version;
            }

            if (previousProdVer != eosConfigFile.Data.productVersion)
            {
                eosConfigFile.Write(true);
            }
#endif
        }
    }
}
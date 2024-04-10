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
    using Editor.Config;
    using Editor.Utility;
    using System;
    using System.IO;
    using System.Threading.Tasks;
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
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#if UNITY_64
                builder = new WindowsBuilder64();
#else
                builder = new WindowsBuilder32();
#endif
                PlatformManager.CurrentPlatform = PlatformManager.Platform.Windows;
#else
                /*
                 * The following conditionals provide functionality for building on platforms that are restricted
                 * access. For information on how to implement the EOS Plugin for Unity for the following platforms,
                 * please reach out to PlayEveryWare at eos-support@playeveryware.com.
                 */
#if UNITY_PS4
                builder = new PS4Builder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.PS4;
#elif UNITY_PS5
                builder = new PS5Builder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.PS5;
#elif UNITY_SWITCH
                builder = new SwitchBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.Switch;
#elif UNITY_GAMECORE_XBOXONE
                builder = new XboxOneBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.XboxOne;
#elif UNITY_GAMECORE_SCARLETT
                builder = new XboxSeriesXBuilder();
                PlatformManager.CurrentPlatform = PlatformManager.Platform.XboxSeriesX;
#endif
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
            bool eosDisabled = ScriptingDefineUtility.IsEOSDisabled(report);

            // Don't do any preprocessing if EOS has been disabled.
            if (eosDisabled)
            {
                return;
            }

            // Perform the pre-build task for the platform using its builder.
            GetBuilder()?.PreBuild(report);
        }

        /// <summary>
        /// For EOS Plugin, this is the only place where anything can happen as a post build task
        /// </summary>
        /// <param name="report">The report from the post-process build.</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            // Don't do any postprocessing if EOS has been disabled.
            if (ScriptingDefineUtility.IsEOSDisabled(report))
            {
                return;
            }

            // Perform the post-build task for the platform using its builder.
            GetBuilder()?.PostBuild(report);
        }
    }
}
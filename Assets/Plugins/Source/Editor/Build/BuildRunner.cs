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
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
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
    using Editor.Utility;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    public class BuildRunner : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// Callback Order for BuildRunner is 1 because all
        /// PlatformSpecificBuilder implementations should have their callback
        /// set to 0 so that they can register themselves where appropriate.
        /// </summary>
        public int callbackOrder => 1;

        /// <summary>
        /// Private value for public property (separated for easier debugging)
        /// </summary>
        private static PlatformSpecificBuilder s_builder;

        /// <summary>
        /// Stores an instance of the builder that is to be used by the
        /// BuildRunner.
        /// </summary>
        public static PlatformSpecificBuilder Builder
        {
            get
            {
                return s_builder;
            }
            set
            {
                s_builder = value;
            }
        }

        /// <summary>
        /// For EOS Plugin, this is the only place where anything can happen
        /// before build.
        /// </summary>
        /// <param name="report">The pre-process build report.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            // Set the current platform that is being built against
            if (PlatformManager.TryGetPlatform(report.summary.platform, out PlatformManager.Platform platform))
            {
                PlatformManager.CurrentPlatform = platform;
            }

            // Run the static builder's prebuild.
            s_builder?.PreBuild(report);
        }

        /// <summary>
        /// For EOS Plugin, this is the only place where anything can happen as
        /// a post build task
        /// </summary>
        /// <param name="report">The report from the post-process build.</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            // Run the static builder's postbuild
            s_builder?.PostBuild(report);
        }
    }
}
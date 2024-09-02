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
#if !EOS_DISABLE
namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using Utility;

    // This conditional is here so that no compiler errors will happen if the
    // Unity Editor is not configured to build for iOS.
#if UNITY_IOS 
    using UnityEditor.iOS.Xcode;
#endif

    /// <summary>
    /// Contains implementation of assorted build tasks that are unique to
    /// building the plugin for an iOS project.
    /// </summary>
    public class IOSBuilder : PlatformSpecificBuilder
    {
        public IOSBuilder() : base("Plugins/iOS", BuildTarget.iOS) { }

        /// <summary>
        /// Perform post build tasks that are unique to the iOS platform.
        /// </summary>
        /// <param name="report">The build report.</param>
        public override void PostBuild(BuildReport report)
        {
            // Per documentation on base implementation, begin the overriden
            // implementation of the PostBuild function by calling first the
            // base implementation.
            base.PostBuild(report);

            // This conditional is here so that no compiler errors will happen
            // if the Unity Editor is not configured to build for iOS.
#if UNITY_IOS 
            string projPath = report.summary.outputPath + 
                              "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new();

            proj.ReadFromString(FileUtility.ReadAllText(projPath));

            string targetGUID = proj.GetUnityMainTargetGuid();
            string unityTargetGUID = proj.GetUnityFrameworkTargetGuid();

            proj.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "false");
            proj.SetBuildProperty(unityTargetGUID,"ENABLE_BITCODE", "false");

            proj.AddFrameworkToProject(targetGUID,"SafariServices.framework", true);
            proj.AddFrameworkToProject(targetGUID, "AuthenticationServices.framework", true);

            File.WriteAllText(projPath, proj.WriteToString());
#endif
        }
    }
}
#endif // !EOS_DISABLE
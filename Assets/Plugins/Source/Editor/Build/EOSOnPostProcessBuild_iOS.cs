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

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;

#if UNITY_IOS

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using UnityEditor.iOS.Xcode;

    public class iOS_BuildPostProcess
    {
        //-----------------------------------------------------------------------------
        public static void ConfigureFrameworksForiOS(PBXProject proj, string targetGUID)
        {
            bool isWeakLinked = true;
            proj.AddFrameworkToProject(targetGUID, "SafariServices.framework", isWeakLinked);
            proj.AddFrameworkToProject(targetGUID, "AuthenticationServices.framework", isWeakLinked);
        }

        //-----------------------------------------------------------------------------
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {

            if (EOSPreprocessUtilities.isEOSDisableScriptingDefineEnabled(buildTarget))
            {
                return;
            }

            if (buildTarget == BuildTarget.iOS)
            {
                string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject proj = new PBXProject();

                proj.ReadFromString(File.ReadAllText(projPath));

                string targetGUID = proj.GetUnityMainTargetGuid();
                string unityTargetGUID = proj.GetUnityFrameworkTargetGuid();

                proj.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "false");
                proj.SetBuildProperty(unityTargetGUID, "ENABLE_BITCODE", "false");

                ConfigureFrameworksForiOS(proj, unityTargetGUID);

                File.WriteAllText(projPath, proj.WriteToString());
            }
        }
    }
}

#endif

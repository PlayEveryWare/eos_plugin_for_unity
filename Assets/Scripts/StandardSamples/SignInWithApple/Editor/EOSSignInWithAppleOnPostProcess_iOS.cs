using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS && APPLEAUTH_MODULE_EDITOR
using UnityEditor.iOS.Xcode;

using AppleAuth.Editor;
using PlayEveryWare.EpicOnlineServices.Editor.Utility;

public class EOSSignInWithAppleOnPostProcess_iOS
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (ScriptingDefineUtility.IsEOSDisabled(buildTarget))
        {
            return;
        }
        
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            
            PBXProject proj = new PBXProject();
        
            proj.ReadFromString(System.IO.File.ReadAllText(projPath));
            var manager = new ProjectCapabilityManager(projPath, "Entitlements.entitlements", null, proj.GetUnityMainTargetGuid());
            manager.AddSignInWithAppleWithCompatibility(proj.GetUnityFrameworkTargetGuid());
            manager.WriteToFile();
        }
        
    }
}
#endif

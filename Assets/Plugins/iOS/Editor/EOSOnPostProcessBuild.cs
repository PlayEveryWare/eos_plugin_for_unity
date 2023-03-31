using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;

#if UNITY_IOS
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

        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new PBXProject();


            AppleAuthEditorHelper appleAuthEditorHelper = new AppleAuthEditorHelper();
#if UNITY_2019_3_OR_NEWER
            proj.ReadFromString(System.IO.File.ReadAllText(projPath));
            var manager = new ProjectCapabilityManager(projPath, "Entitlements.entitlements", null, proj.GetUnityMainTargetGuid());
            appleAuthEditorHelper.AddSignInWithAppleWithCompatibilityHelper(manager, proj.GetUnityFrameworkTargetGuid());
            manager.WriteToFile();
#else
            var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", PBXProject.GetUnityTargetName());
            appleAuthEditorHelper.AddSignInWithAppleWithCompatibilityHelper(manager);
            manager.WriteToFile();
#endif

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
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AppleAuth.Editor;

public class AppleAuthEditorHelper : MonoBehaviour
{
    public bool IsAppleAuthModuleInstalled()
    {
#if APPLEAUTH_MODULE_EDITOR
        return true;
#endif
        Debug.Log("AppleAuthEditorHelper : [Sign In With Apple] package not installed.");
        return false;
    }

    public void AddSignInWithAppleWithCompatibilityHelper(UnityEditor.iOS.Xcode.ProjectCapabilityManager manager, string unityFrameworkTargetGuid = null)
    {
        manager.AddSignInWithAppleWithCompatibility(unityFrameworkTargetGuid);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AppleAuth.Editor;

public class AppleAuthEditorHelper : MonoBehaviour
{
    public void AddSignInWithAppleWithCompatibilityHelper(UnityEditor.iOS.Xcode.ProjectCapabilityManager manager, string unityFrameworkTargetGuid = null)
    {
        manager.AddSignInWithAppleWithCompatibility(unityFrameworkTargetGuid);
    }
}

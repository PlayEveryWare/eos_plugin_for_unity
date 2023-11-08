<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Connect Login with AppleID using Sign In With Apple Plugin</div>
---

## Overview

This document describes how to enable Apple ID authentication in conjunction with Epic Online Services Plugin for Unity. The suggested approach is to inject the [Sign In With Apple](https://github.com/lupidan/apple-signin-unity) unity plugin into our EOS Unity Plugin project.

## Setup

First, install the Sign In With Apple unity plugin, which could be found at [Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-unity-plugin-152088) or [Github](https://github.com/lupidan/apple-signin-unity).  

Next, imported it into the target unity project, either as a unity package or through the package manager.

## Scripts

### PostProcess Build Settings for Xcode project (iOS)

One way to add the 'Sign in with Apple' capability is to mark the needed entitlements after the Xcode project is created, inside a function using the `[PostProcessBuild]` attribute.  

Take EOS-Unity-Plugin as example, add this code block in `EOSOnPostProcessBuild.cs -> OnPostprocessBuild()`, and the `using AppleAuth.Editor` directive.

> [!IMPORTANT]
> In order to have the `using AppleAuth.Editor` directive compile properly, you will need to add `AppleAuth.asmdef`, and `AppleAuth.Editor.asmdef` (included with the plugin) to the Assembly Definitions Reference list:
> ![](https://user-images.githubusercontent.com/36757173/219812051-f4482a35-7cac-4a18-bc22-29660fc8d32b.png)

```cs
#if UNITY_2019_3_OR_NEWER
    proj.ReadFromString(System.IO.File.ReadAllText(projPath));
    var manager = new ProjectCapabilityManager(projPath, "Entitlements.entitlements", null, proj.GetUnityMainTargetGuid());
    manager.AddSignInWithAppleWithCompatibility(proj.GetUnityFrameworkTargetGuid());
    manager.WriteToFile();
#else
    var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", PBXProject.GetUnityTargetName());
    manager.AddSignInWithAppleWithCompatibility();
    manager.WriteToFile();
#endif
```

### PostProcess Build Settings for Xcode project (macOS)

The automated method could be found [here](https://github.com/lupidan/apple-signin-unity/blob/master/docs/macOS_NOTES.md). 

Here is an alternative that relies on creating an XCode project for the game, make a couple of configurations, then build from the xproject.

1. In Unity Build Settings, toggle on create xproject, then build.

2. In Xcode, navigate to `Your Project -> Signing & Capabilities -> Add capability (The + Button)`, the add the capability `Sign In With Apple`. 

3. Determine the `Team` and `Bundle Identifier`. 

4. Build and Run from XCode.  

![image](https://user-images.githubusercontent.com/36757173/221330764-1df29598-dc3b-4a27-9fba-3a8b27f0f500.png)

### Sample Apple Sign In Scripts

Here is a sample script tested in EOS-Unity-Plugin, which is modified from the example Unity's Documents [here](https://docs.unity.com/authentication/en/manual/SettingupAppleSignin)

```cs
using UnityEngine;
using System.Text;

using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;

public class AppleExampleScript : MonoBehaviour
{
    IAppleAuthManager m_AppleAuthManager;
    public string Token { get; private set; }
    public string User { get; private set; }
    public string Error { get; private set; }

    public void Initialize()
    {
        var deserializer = new PayloadDeserializer();
        m_AppleAuthManager = new AppleAuthManager(deserializer);
    }

    public void Update()
    {
        if (m_AppleAuthManager != null)
        {
            m_AppleAuthManager.Update();
        }
    }

    public void LoginToApple(System.Action callback)
    {
        // Initialize the Apple Auth Manager
        if (m_AppleAuthManager == null)
        {
            Initialize();
        }

        // Set the login arguments
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        // Perform the login
        m_AppleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIDCredential = credential as IAppleIDCredential;
                if (appleIDCredential != null)
                {
                    var idToken = Encoding.UTF8.GetString(
                        appleIDCredential.IdentityToken,
                        0,
                        appleIDCredential.IdentityToken.Length);
                    Debug.Log("Sign-in with Apple successfully done. IDToken: " + idToken);
                    Token = idToken;
                    User = appleIDCredential.User;

                    callback?.Invoke();
                }
                else
                {
                    Debug.Log("Sign-in with Apple error. Message: appleIDCredential is null");
                    Error = "Retrieving Apple Id Token failed.";
                }
            },
            error =>
            {
                Debug.Log("Sign-in with Apple error. Message: " + error);
                Error = "Retrieving Apple Id Token failed.";
            }
        );
    }
}
```

Users can modify `LoginToApple()` and the parameter passed into it according to individual project needs, such as what data gets passed along during callback.  

Then simply call `LoginToApple()` to get the Apple ID login prompt showing.  

Remember to put `m_AppleAuthManager.Update()` in the game code's `Update()` for `m_AppleAuthManager.LoginWithAppleId()` to register its callback.  
 
Here is an example test code in EOS-Unity-Plugin's `UILoginMenu.cs`, `ConnectAppleId()` gets called when pressing the login button with `Connect`, `AppleIDToken`.   

```cs
private void ConnectAppleId()
{
    appleLoginHelper = new AppleExampleScript();
    appleLoginHelper.LoginToApple(()=>
    {
        Debug.Log("Login Success AppleID");
        StartConnectLoginWithToken(ExternalCredentialType.AppleIdToken, appleLoginHelper.Token, appleLoginHelper.User.Remove(31));
    });
}
```
# Connect Login with AppleID using Sign In With Apple Plugin

There are plenty of ways to implement Apple ID connect feature.  

For testing purposes we used the Sign In With Apple unity plugin.  

This document will indicate how we injected the plugin into our EOS-Unity-Plugin Project.

## Project Setup

First, install the Sign In With Apple unity plugin, which could be found at 

[Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-unity-plugin-152088) or [Github](https://github.com/lupidan/apple-signin-unity).  

Next, imported it into the target unity project, either as a unity package or through the package manager.

## Scripts

* PostProcess Build Settings for Xcode project (iOS)

Adding the 'Sign in with Apple' capability into the Xcode project is needed.  

One way to do so is to mark the needed entitlements after the Xcode project is created, inside a function using the `[PostProcessBuild]` attribute.  

Take EOS-Unity-Plugin as example, add this code block in `EOSOnPostProcessBuild.cs -> OnPostprocessBuild()`, and the `using AppleAuth.Editor`[^1] up front.

```
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


[^1]: The plugin includes assembly definition files AppleAuth.asmdef and AppleAuth.Editor.asmdef.  
Remember to add them to Assembly Definitions Reference list when they're needed for the corresponding namespace to be recognized by C#. 
(EOS-Unity-Plugin project needs these set for example).  
![Screen Shot 2023-02-17 at 2 46 47 PM](https://user-images.githubusercontent.com/36757173/219812051-f4482a35-7cac-4a18-bc22-29660fc8d32b.png)

---------------------------------------------------------------------


* PostProcess Build Settings for Xcode project (macOS)

The automated method could be found [here](https://github.com/lupidan/apple-signin-unity/blob/master/docs/macOS_NOTES.md). 

Here is an alternative that relys on creating an XCode project for the game, make a couple of configurations, then build from the xproject.

1. In Unity Build Settings, toggle on create xproject, then build.

2. In xcdode, navigate to `Your Project -> Signing & Capabilities -> Add capability (The + Button)`, the add the capability `Sign In With Apple`. 

3. Determine the `Team` and `Bundle Identifier`. 

4. Build and Run from XCode.  

![image](https://user-images.githubusercontent.com/36757173/221330764-1df29598-dc3b-4a27-9fba-3a8b27f0f500.png)
   

---------------------------------------------------------------------


* Sample Apple Sign In Scripts

Here is a sample script tested in EOS-Unity-Plugin, which is modified from the example Unity's Documents [here](https://docs.unity.com/authentication/en/manual/SettingupAppleSignin)


```
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

Users can modify `LoginToApple()` and the parameter passed into it according individual project needs, such as what data gets passed along during callback.  

Then simply call `LoginToApple()` to get the Apple ID login promt showing.  

Remember to put `m_AppleAuthManager.Update()` in the game code's `Update()` for `m_AppleAuthManager.LoginWithAppleId()` to register its callback.  

-------------------------------------------------------------------

* Use it in your game!
 
Here is an example test code in EOS-Unity-Plugin's `UILoginMenu.cs`, `ConnectAppleId()` gets called when pressing the login button with `Connect`, `AppleIDToken`.   

```
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
Visit the plugin [website](https://github.com/lupidan/apple-signin-unity) for more information 

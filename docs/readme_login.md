# Logging in with PEW EOS Plugin

----------------------------------------------------------------------------------------

## Overview

This file documents how Login works in the PEW EOS Plugin for the following platforms
* Windows
* Mac 
* Linux
* iOS
* Android

----------------------------------------------------------------------------------------

## Integration Notes

### Prerequisites

* EOS Platform Interface must be initilaized before performing a login.
  *  Could be done by attaching `EOSManager.cs` to an object in the scene.

----------------------------------------------------------------------------------------
## Login Interfaces

There are two login interfaces.
* **Auth Login**       for authorizing an **Epic Account** and related services 
* **Connect Login**    for connecting the **User** for one **ProductID (Game)**

More information about the distinction could be found [here](https://dev.epicgames.com/en-US/news/accessing-eos-game-services-with-the-connect-interface#a-brief-summary-of-auth-vs-connect-interfaces)

### Auth Login

* Auth Login functions are declared in EOS Auth Interface
* **Account Portal** (`EOS_LCT_AccountPortal`) and **Persistent Auth** (`EOS_LCT_PersistentAuth`) are the primary Auth Types (`LoginCredentialType`) to login  
  * **Persistent Auth** will login with credentials of the previous successful **Account Portal** login
* **Dev Auth**(`EOS_LCT_Developer`) is for quick iteration for developers, which could be done by using the Dev-Auth tool provided with the EOS SDK. Read [this](https://github.com/PlayEveryWare/eos_plugin_for_unity/tree/development/docs/Walkthrough.md) for details
* **External Auth** is currently only for Steam session ticket login on these platforms
* **Exchange Code** is for logging in on Epic Game Store deployments

A list of which `LoginCredentialType` to use on which platform could be found [here](https://github.com/PlayEveryWare/eos_plugin_for_unity/tree/development/docs/login_type_by_platform.md)
![](images/login_type_by_platform.png)

### Connect Login

* Connect Login functions are declared in EOS Connect Interface

----------------------------------------------------------------------------------------
## Login Flow

```mermaid
flowchart
  direction LR

    subgraph CL[Connect Login]
      direction TB
  
      I(Attempt Connect Login);
      J{Did Connect Login Succeed};
      K(Create a new user)
      L(Attempt Connect Login Again);
      M{Did Connect Login Succeed};
      N(Login Complete, Game Ready);
      O(Connect Login Failed);
    
      I-->J-->|Yes|N;
          J-->| No|K-->L-->M-->|Yes|N;
                           M-->| No|O;

    end;
    
    subgraph AL[Auth Login]
      direction TB
  
      A(Click Login Button);
      B{Attempt External Auth Login};
      C{Attempt Persistent Login};
      D{Attempt Exchange Code Login};
      E{Attempt Account Portal/Dev Auth Login};
      F(Delete Local Refresh Token);
      G{Steam token & session ticket};
      H{Epic Launcher Args : Exchange Code};
      P{Link Epic Account to External Account};
      
      S(Auth Login Succeeded);
      T(Auth Login Succeeded); 
      U(Auth Login Succeeded);
      V(Auth Login Succeeded);
      W(Auth Login Failed);
      X(Auth Login Failed);
      Y(Auth Login Failed);
      Z(Auth Login Failed);
      

      A-->G-->|Retrieved|B-->|Success|S;
                         B-->|InvalidUser|P-->|Success|S;
                                          P-->|  Fail |W;
                         B-->|  Fail |W;
          G-->|  Null |W;
      A-->C-->|Success|T;
          C-->|  Fail |F-->X;
      A-->D-->H-->|Retrieved|U;
              H-->|  Null |Y;
      A-->E-->|Success|V;
          E-->|  Fail |Z;
    end;
  

    
```

----------------------------------------------------------------------------------------

## Code Example

Here are examples for each of the `LoginCredentialTypes`

### Persistent Auth

`StartPersistentLogin` attempts a login with `PersistentAuth`, and determine what happens according to the callback result
```cs
    public void StartPersistentLogin(OnAuthLoginCallback onLoginCallback)
    {
        StartLoginWithLoginTypeAndToken(LoginCredentialType.PersistentAuth, null, null, (callbackInfo) =>
        {
                // 🟡 Handle invalid or expired tokens for the caller
                switch(callbackInfo.ResultCode)
                {
                    // 🟡 If the login attempt results in an invalid token, delete the local refresh token
                    case Result.AuthInvalidPlatformToken:
                    case Result.AuthInvalidRefreshToken:
                    
                        var authInterface = EOSManager.Instance.GetEOSPlatformInterface().GetAuthInterface();
                        var options = new Epic.OnlineServices.Auth.DeletePersistentAuthOptions();
                        authInterface.DeletePersistentAuth(ref options, null, (ref DeletePersistentAuthCallbackInfo deletePersistentAuthCallbackInfo) =>
                        {
                               onLoginCallback?.Invoke(callbackInfo);
                        });
                        return;
                        
                    default:
                        break;
                }
                
                onLoginCallback?.Invoke(callbackInfo);
        });
    }
```
The `onLoginCallback` in the function above is defined in `OnLoginButtonClick` function, at the `LoginCredentialType.PersistentAuth` case
```cs
    EOSManager.Instance.StartPersistentLogin((Epic.OnlineServices.Auth.LoginCallbackInfo callbackInfo) =>
    {
        if (callbackInfo.ResultCode != Epic.OnlineServices.Result.Success) // 🔴 Pesistent Login Failed, handle the fail case here
        {
            return;
        }
        else // 🟢 Pesistent Login Succeeded
        {
            StartLoginWithLoginTypeAndTokenCallback(callbackInfo);
        }
    });
```

### Account Portal & Dev Auth

Unlike `Persistent Auth`, these two `LoginCredentialTypes` do not need to worry about stored token, thus they could attempt login directly 

```cs
    EOSManager.Instance.StartLoginWithLoginTypeAndToken(loginType,
                                                        usernameAsString, // 🔵 could be null for Account Portal
                                                        passwordAsString, // 🔵 could be null for Account Portal
                                                        StartLoginWithLoginTypeAndTokenCallback);
``` 

### Exchange Code

`Exchange Code` login could used when launching the game through Epic Games Launcher on desktop platforms (Windows, Mac, Linux)  
The required exchange code could be retrieved with `GetCommandLineArgsFromEpicLauncher()`

```cs
    EOSManager.Instance.StartLoginWithLoginTypeAndToken(loginType,
                                                        null, // 🔵 Intended for UserID, but is unused for Exchange Code login
                                                        EOSManager.Instance.GetCommandLineArgsFromEpicLauncher().authPassword, // 🔵 The exchange code itself, passed as login token
                                                        StartLoginWithLoginTypeAndTokenCallback);
``` 

### External Auth

Currently `External Auth` is for Steam session ticket login

```cs
    public void StartLoginWithSteam(EOSManager.OnAuthLoginCallback onLoginCallback)
    {
        string steamId = GetSteamID();
        string steamToken = GetSessionTicket();
        if (steamId == null || steamToken == null)  // 🔴 Pesistent Login Failed, handle the fail case here
        {
            return;
        }
        else
        {
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(
                    Epic.OnlineServices.Auth.LoginCredentialType.ExternalAuth,
                    Epic.OnlineServices.ExternalCredentialType.SteamSessionTicket,
                    steamId,
                    steamToken,
                    onLoginCallback);
        }
```           
### Handling Auth Login Results

When an Auth Login attempt finishes, `StartLoginWithLoginTypeAndTokenCallback` gets called and handles the login flow according to the result from Auth Login.  
* Auth Login returned `Success`: Login flow proceeds to perform a Connect Login attempt
* Auth Login returned `InvalidUser`: Implies that Steam(`External Auth`) logged in successfully but Epic Account wasn't linked yet, login flow prompts the user to link one.

```cs
    public void StartLoginWithLoginTypeAndTokenCallback(LoginCallbackInfo loginCallbackInfo)
    {
        // 🟢 Epic Account logged in successfully
        if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
        {
            // 🔵 Proceed to Connect Login
            StartConnectLoginWithLoginCallbackInfo(loginCallbackInfo);
        }
        
        // 🟡 If no Epic Account is linked to Steam account yet
        else if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.InvalidUser)
        {
            // 🟡 Prompts the user to link an Epic account to the Steam account, whether by creating a new one or linking an existing one
            EOSManager.Instance.AuthLinkExternalAccountWithContinuanceToken(loginCallbackInfo.ContinuanceToken, 
                                                                            LinkAccountFlags.NoFlags,
                                                                            (Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
            {
                
                if (linkAccountCallbackInfo.ResultCode == Result.Success)
                {
                    // 🔵 Proceed to Connect Login
                    StartConnectLoginWithLoginCallbackInfo(loginCallbackInfo);
                }
                else return; // 🔴 Epic Account wasn't linked
            });
        }

        else return; 🔴 Other fail cases of the external login attempt
    }
```

### Connect Login

When Auth Login Succeeds, Connect login gets called next 

```cs    
    private void StartConnectLoginWithLoginCallbackInfo(LoginCallbackInfo loginCallbackInfo)
    {
        // 🔵 Attempts Connect Login
        EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
        {
            if (connectLoginCallbackInfo.ResultCode == Result.Success)
            {
                // 🟢 Login flow completed and succeeded
            }
            
            // 🟡 There is no user created for this game on this Epic Account yet
            else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser)
            {
                // 🟡 Ask user if they want to connect; sample assumes they do and creates an user
                EOSManager.Instance.CreateConnectUserWithContinuanceToken(connectLoginCallbackInfo.ContinuanceToken, (Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo) =>
                {
                    // 🔵 Attempts Connect Login again after user created for the account
                    EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo retryConnectLoginCallbackInfo) =>
                    {
                        if (retryConnectLoginCallbackInfo.ResultCode == Result.Success)
                        {
                            // 🟢 Login flow completed and succeeded
                        }
                        
                        else return; // 🔴 Connect login remain failing
                    
                    });
                });
            }
        });
    }
```      
> [!WARNING]
> What happens after login should be tailored to fit the game's flow, such as configure UI, open main scene, log information, etc.
> They are omitted to keep this file concise. The full version of the functions could be found in plugin scripts `EOSManager.cs` and `UILoginMenu.cs`.
            
### Intermediate functions that `StartLoginWithLoginTypeAndToken` contains

  * Sets `ExternalCredentialType.Epic` as the ExternalCredentialType
```cs       
        public void StartLoginWithLoginTypeAndToken(LoginCredentialType loginType, string id, string token, OnAuthLoginCallback onLoginCallback)
        {
            StartLoginWithLoginTypeAndToken(loginType, ExternalCredentialType.Epic, id, token, onLoginCallback);
        }
```        
  * Packs parameters into `LoginOptions` as an input for `StartLoginWithLoginOptions` (and eventually `EOSAuthInterface.Login`)
```cs  
        public void StartLoginWithLoginTypeAndToken(LoginCredentialType loginType, ExternalCredentialType externalCredentialType, string id, string token, OnAuthLoginCallback onLoginCallback)
        {
            var loginOptions = MakeLoginOptions(loginType, externalCredentialType, id, token);
            StartLoginWithLoginOptions(loginOptions, onLoginCallback);
        }
```       
  * With parameters all set we can finally call `StartLoginWithLoginOptions` function used for all kinds of login
```cs  
        public void StartLoginWithLoginOptions(Epic.OnlineServices.Auth.LoginOptions loginOptions, OnAuthLoginCallback onLoginCallback)
        {
            var EOSAuthInterface = GetEOSPlatformInterface().GetAuthInterface();
            Assert.IsNotNull(EOSAuthInterface, "EOSAuthInterface was null!"); // 🔴 EOSAuthInterface was null

#if UNITY_IOS && !UNITY_EDITOR
            // 🟡 App Controller needs to be set in Login Options for iOS
            IOSLoginOptions modifiedLoginOptions = (EOSManagerPlatformSpecifics.Instance as EOSPlatformSpecificsiOS).MakeIOSLoginOptionsFromDefualt(loginOptions);
            EOSAuthInterface.Login(ref modifiedLoginOptions, null, (Epic.OnlineServices.Auth.OnLoginCallback)((ref Epic.OnlineServices.Auth.LoginCallbackInfo data) => {
#else
            EOSAuthInterface.Login(ref loginOptions, null, (Epic.OnlineServices.Auth.OnLoginCallback)((ref Epic.OnlineServices.Auth.LoginCallbackInfo data) => {
#endif
            // 🔵 This is the EOS login part
            EOSAuthInterface.Login(ref loginOptions, null, (Epic.OnlineServices.Auth.OnLoginCallback)((ref Epic.OnlineServices.Auth.LoginCallbackInfo data) => {

                if (data.ResultCode == Result.Success) // 🟢 Epic Account logged in successfully
                {
                    loggedInAccountIDs.Add(data.LocalUserId);

                    SetLocalUserId(data.LocalUserId);

                    ConfigureAuthStatusCallback();

                    CallOnAuthLogin(data);
                }

                if (onLoginCallback != null) // 🟡 Next step will be handled by the callback function, whether login succeeds or not
                {
                    onLoginCallback(data);
                }

            }));
        }
```

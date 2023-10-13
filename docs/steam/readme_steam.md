<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>
<br /><br /><br />

---


# <div align="center">Setting up for Steam</div> <a name="setting-up-for-steam" />
---
The EOS(Epic Online Services) SDK supports logging in with a Steam Account and several other Steam functionalities.  
This document is a guide to connect the dots between EOS SDK and Steam SDK.  

<br /> 

## Prerequisites

- Install Steam SDK version `1.57` minimum.  
- Have Steam added as an `Indentity Provider` for the game in the [Developer Portal](https://dev.epicgames.com/en-US/home) at `Product Settings -> Identity Providers`.
  
_For more information : [Identity Provider Management](https://dev.epicgames.com/docs/dev-portal/identity-provider-management) :: [Steam](https://dev.epicgames.com/docs/dev-portal/identity-provider-management#steam)_

<br /> 

## External C# Wrappers for the Steam SDK

The Steam SDK includes C++ libraries which needs C# wrappers to work in Unity.  
There are several open source C# wrappers available.  
Make sure that the minimum Steam SDK version `1.57` is supported by the wrappers if you are using one.

<br /> 

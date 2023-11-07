<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

# Steam Integration

The EOS(Epic Online Services) SDK supports logging in with a Steam Account and several other Steam functionalities.  

This document is a guide to connect the dots between EOS SDK and Steam SDK.  

## Prerequisites

- Install Steam SDK version `1.57` minimum.  
- Have Steam added as an `Indentity Provider` for your game (see [here](https://dev.epicgames.com/docs/dev-portal/identity-provider-management#steam) for how to do that).

## External C# Wrappers for the Steam SDK

The Steam SDK includes C++ libraries which needs C# wrappers to work in Unity.  
There are several open source C# wrappers available.  

> [!WARNING]
> Make sure that the minimum Steam SDK version `1.57` is supported by the wrappers if you are using one.

## Samples

The plugin includes samples for the following Steam SDK wrappers.
- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET)
- [Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks)

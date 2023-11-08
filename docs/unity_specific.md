<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Unity-specific plugin functionality</div>
---

## Overview

This document lists some Unity related plugin behavior and how we support the EOS SDK in EOS plugin for Unity.

### Why we support the versions we support.

The EOS Unity Plugin is a collective effort between multiple sources, each source also has to be compatible with each other.  

* Every version of Unity has its targeted build tools for Platform APIs.  
* Every EOS SDK version is built against specific platform SDKs.
* Every EOS SDK version has updated C# scripts for Unity to call.  

We take multiple factors into account, and decide on the versions that are most stable and expandable.  

The current release is using :  
* Unity 2021.3.8f1.  
* EOS SDK 1.15.4.   
* SDK versions of the target platform that supports the given Unity and EOS SDK versions above could be found in their platform forums.

### Why do we reload the EOS SDK DLL in the editor?

EOS SDK requires an initialization at the start of every run. 

While the DLL is loaded, Unity does not allow changes to the configuration.  

Therefore, we unload and reload the DLL for our users to rerun the plugin without needing to reboot the Unity Editor. 

### Why do we need a GFX plugin on Windows

GFX plugin is required so that the EOS Overlay can render properly on Windows.   
Due to how the EOS SDK implements the graphics system for the overlay feature, initialization of the SDK needs to happen _before_ the Unity Editor finishes _it's_ graphics system. The solution is in a GFX plugin because accomplishing this is done in native code.



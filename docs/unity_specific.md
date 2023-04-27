# Unity things for EOS SDK

----------------------------------------------------------------------------------------

## Overview

This document lists some Unity related oddities and how we support EOS SDK in our plugin
* Why we support the versions we support.
* Why some things work oddly in Unity.

----------------------------------------------------------------------------------------
### Why we support the versions we support.

The EOS Unity Plugin is a collective effort between multiple sources, each source also has to be compatible with each other.  

* Every version of Unity has its targeted build tools for Platform APIs.  
* Every Platform has their minimum supporting API version for EOS SDK.  
* Every EOS SDK has updated C# scripts for Unity to call.  

We take multiple factors into account, and decide on the versions that are most stable and expandable. 

### Why do we reload the EOS SDK dll in the editor?

EOS SDK requires an initialization at the start on every run.  
It does not allow changes to its configuration while the dll is loaded, however.  
Therefore we unload and reload the dll for our users to rerun the plugin without needing to reboot Unity Editor. 

### Why do we need a GFX plugin on Windows

GFX plugin is required so that the EOS Overlay can render properly on Windows.   
Due to how the EOS SDK does it's graphics initialization for the overlay,  
the code that call's into the EOS SDK for init needs to happen _before_
Unity finishes doing it's graphics init code. The only place to do this, is in native
code, in a specially named DLL that gets called before the engine finishes running. 



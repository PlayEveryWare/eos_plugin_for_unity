# Unity things for EOS SDK

----------------------------------------------------------------------------------------

## Overview

This document lists some Unity related oddities and how we support EOS SDK in our plugin
* Why we support the versions we support.
* Why some things work oddly in Unity.

----------------------------------------------------------------------------------------
### Why we support the versions we support.

The EOS Unity Plugin is a collective effort between multiple sources, each source also has to compat with each other.  

* Every version of Unity has its targeted build tools for Platform APIs.  
* Every Platform has their minimum supporting API version for EOS SDK.  
* Every EOS SDK has updated C# scripts for Unity to call.  

We take multiple factors into account, and decide on the versions that are most stable and expandable. 

### Why do we reload the EOS SDK dll in the editor?

EOS SDK requires an initialization at the start on every run.  
It does not allow changes to its configuration while the dll is loaded, however.  
Therefore we unload and reload the dll for our users to rerun the plugin in without needing to reboot Unity Editor.  

Here is how we load/unload a dll using a native binary, (Windows as example)

    [DllImport("kernal32", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr LoadLibrary(string lpFileName);
    
    [DllImport("kernal32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

Feed the target library name into `LoadLibrary(string)`, if it succeeds we will get a valid `IntPtr`, otherwise an `IntPtr.Zero` will be returned.  
Similarly we can feed the `IntPtr` received from `LoadLibrary(string)` into `FreeLibrary(IntPtr)`, which should unload the library.  
Hence allowing the sdk to modify its configurations.


### Why do we need a GFX plugin on Windows

GFX plugin is required so that the EOS Overlay can render properly on Windows




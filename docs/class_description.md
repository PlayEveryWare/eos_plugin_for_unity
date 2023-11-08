<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Class descriptions</div>

# EOSManager
Class that acts as the interface to EOS.
Exposes a friendly interface for Unity.
An instance of this must be added to all scenes that want to handle EOS, currently.
The settings it has are read in from a JSON file that's created by the EpicOnlineServicesConfigEditor, or 
modified by hand. The config file is currently fixed at `Assets/EOS/Resources/EpicOnlineServicesConfig.json`.

# EOSConfig
Contains the key required to startup EOS. This class is used both by the editor code and the EOS plugin at
runtime.

# EpicOnlineServicesConfigEditor
This is the object that handles the Unity Editor plugin's configuration. At the moment it only
is used for making changes to the EOS JSON config file.

# UnityEditorSharedDictionary (Soft Deprecated)
This is a Visual studio solution that contains two projects, only one of which is used. The NativeSharedDictionary library was created to work around a curious behavior and design choice in Unity.

When a DLL is loaded in the Unity editor, it is never unloaded. This is problematic for the EOS SDK because it assumes
that SDK will only be initialized once, and will in-fact return an error if one tries to do so.

To work around this behavior, I (Andrew Hirata) messed around with a few ways of holding on to an EOS handle that could 
survive multiple play in editor, till I realized I could abuse the same system that was causing this issue. Namely, I could
create another native DLL, unwrap the handle from a given EOS C# object, and store it in the aforementioned DLL, fetch it on
next 'play', and recreate the C# Object. It's not the best solution, but it allows for mitigating the issue.

A more ideal solution, but one that would require a decent amount of work on the EOS SDK auto-generated C# files, would be to
dynamically load the DLL by hand and load the symbols out by hand when running in the editor.

# Visual Studio Projects

## DynamicLibraryLoaderHelper
This project contains all the native code that is needed to make the plugins work on various platforms.
For the moment that means all the memory management calls are implemented in this Project.

Currently, this project includes source code for platforms that need to be isolated due to potential issues with NDAs.

## NativeRender (Project under the DynamicLibraryLoaderHelper Visual Studio SLN)
At the moment, all it does is search though the Windows registry
to find the EOS Overlay DLL, and then load it when Unity is loading it's
DLLs. It does this so that the Overlay DLL can do it's function
interposing (function hooking) magic. It has to happen early in the
process lifetime, which is why it's implemented in native code.

In the future, this Visual Studio project might contain rendering code or other
hooks as needed to integrate the rendering needs of the EOS SDK.
<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Advantages of using this Plugin</div>
---

## Handles Windows Init
One of the core features of the Unity Plugin for EOS, is that it handles doing the init code in such a way that the Overlay will work correctly in standalone builds on Windows. 
Due to how the EOS SDK does its graphics initialization for the overlay, the code that calls into the EOS SDK for init needs to happen _before_ Unity finishes doing its graphics init code. The only place to do this is in native code, in a specially named DLL that gets called before the engine finishes running.

If you're using this Plugin, you don't have to worry about those details, because it's already done for you! 

## Handles native code
Besides the Windows platform, the plugin also handles other requirements of the Epic Online Services SDK, such as memory management, application life-cycle events, and input. 

These features are particularly beneficial for games that target console platforms, and frees users of the plugin to focus on their title instead of spending time handling native code implementations.

## Handles the Unity Editor
The Unity Editor has unique behavior that makes integrating the EOS SDK a challenge.
One such behavior is that (on Windows) the Editor never unloads any DLLs. Without additional work, this would mean that any changes that need to be done to the EOS configuration would require a reboot of the editor.

To resolve that, we have written additional code to handle loading and unloading the EOS SDK manually.

This ensures that new configuration changes can be made without restarting the editor, makes sure that any callbacks from previous play sessions aren't called, and that all EOS objects are destroyed between runs in the Editor.

## Provides Samples and Manager Scripts
The Plugin provides added value with its Unity-specific samples that showcase both how to use the EOS SDK, and provide copyable code that can be used in projects to give you a head-start on integrating the EOS SDK. 
Because they're provided as UPM samples, they can be imported, deleted, and modified as needed.

## Works with Unity
In order to facilitate configuration of the EOS SDK, the Plugin comes with editor scripts that ensure proper inclusion of EOS SDK binaries and configuration files into the final standalone build. 
Also included in the Plugin are tools to store and load the configuration for the EOS SDK so that it can initialize correctly, and properly setup EAC (Easy Anti-cheat).

## Provides integration with NetCode for GameObjects
Provided as a part of the samples, the Plugin implements a sample Transport layer that lets you use EOS with Netcode for GameObjects.
By using the provided sample, and by editing as needed for a specific title, you can more easily switch from a different 
backend to EOS when using Netcode for GameObjects.

## Comes with documentation
The UPM and the repo come with additional documentation that can supplement the first-party EOS documentation. In particular,
it includes resources that are relevant to running the samples, such as in sample readme files, tooltips, and documents that
answer common questions, and explain the _why_ behind some of the choices made in the Plugin.

## Comes with the EOS SDK
Each release of the Plugin comes with the correct EOS SDK, ensuring that Plugin works with the EOS SDK.
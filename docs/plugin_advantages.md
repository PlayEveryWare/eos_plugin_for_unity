# Advantages of using this Plugin

## Handles Windows Init
One of the core features of the Unity Plugin for EOS, is that it handles
doing the init code in such a way that the Overlay will work correctly in 
standalone builds on windows. Due to how the EOS SDK does it's graphics initialization 
for the overlay, the code that call's into the EOS SDK for init needs to happen _before_
the Unity finishes doing it's graphics init code. The only place to do this, is in native
code, in a specially named DLL that gets called before the engine finishes running. 
If one is using this Plugin, one doesn't have to worry about any of that, as it's handled 
automatically.


## Handles the Unity Editor
Need to unload and reload the EOS SDK dynamic libraries to ensure new config works
ensures that any old callbacks can't be called

The Unity Editor has unique behavior that makes integrating the EOS SDK a challenge.
One such behavior, is that on windows, the Editor never unloads any DLLs. Without additional work,
this would mean that any changes that need to be done to the EOS configuration would require an
editor reboot.

To resolve that, we have written additional code to handle loading, and unloading, the EOS SDK manually.
This both ensures that new configuration changes can be made without restarting the editor, and makes sure
that any callbacks from previous play sessions aren't called, and that all EOS objects are destroyed between 
runs in the Editor.

## Provides Samples
One feature of the Plugin that particularly makes it standout from using just the C# SDK that comes from Epic, 
are the Unity specific samples that showcase both how to use the EOS SDK, and provide copy-past-able code
that can be used in projects to give one a head-start on integrating the EOS SDK. Because they're provided as UPM
samples, they can be imported, deleted, and modified as needed to allow users of the Plugin maximum flexibility.

## Works with Unity
In order to facilitate configuration of the EOS SDK, the Plugin comes with editor scripts that ensure 
proper inclusion of EOS SDK binaries and configuration files into the final standalone build. 
Also included in the Plugin, are tools to store and load the configuration for the EOS SDK so that it can
initialize correctly, and for setting up EAC (Epic Anti-cheat).

## Provides integration with NetCode for game objects
Provided as a part of the samples, the Plugin provides Transport layer that lets one use EOS with Netcode for game objects.
By using the provided sample, and by editing as needed for a specific title, one can more easily switch from a different 
backend to EOS when using Netcode for game objects.

## Comes with documentation
The UPM and the repo come with additional documentation that can supplement the first-party EOS documentation. In particular,
it includes resources that are relevant to running the samples, such as in sample readme files, tooltips, and documents that
answer common questions, and explain the _why_ behind some of the choices made in the Plugin.

## comes with the EOS SDK
Each release of the Plugin comes with the correct EOS SDK, ensuring that Plugin works with the EOS SDK


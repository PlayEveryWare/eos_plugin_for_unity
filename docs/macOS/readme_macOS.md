# macOS 

## Preview Platform
To access Preview platforms, enable `EOS_PREVIEW_PLATFORM`

## Building Standalone with Unity.

Prerequisites:

* macOS Device
* Unity macOS build module
* XCode

### Additional Steps

* Use the makefile in NativeCode\DynamicLibraryLoaderHelper_macOS\ to build the dylib needed for the mac build

##Known Issues.
If the user is running the plugin in UnityEditor, after modifying the configuration settings,
a UnityEditor reboot is needed for the changes to take place.


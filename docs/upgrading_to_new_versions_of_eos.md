# Installing New versions of EOS

This document covers two different scenarios where one might need to upgrade the EOS SDK:
 * When the plugin is installed into a Unity Project
 * When one has a clone of this repo

Upgrading the SDK is generally an easy process, mostly involving copying around files.
However, due to internal changes to how a new version of the SDK will act, it's generally 
not recommended to do so, and it is _not supported_.

## Upgrading the EOS SDK in an installed plugin

### Prerequisites
* Have access to the EOS Developer Portal
* Downloads of the EOS SDK one wishes to upgrade to

If one has the plugin installed via 'From disk', one may proceed to the general
instructions in [Upgrading the EOS SDK in a clone of this repository](#upgrading-the-eos-sdk-in-a-clone-of-this-repository)


If one has the plugin installed on disk as a UPM package: 
1. Uninstall the plugin
    * Optional move the package to somewhere else on disk.
2. Decompress the package.
3. Add it back in from the package manager  using 'Add Package From disk...'
4. Continue with the steps at [Upgrading the EOS SDK in a clone of this repository](#upgrading-the-eos-sdk-in-a-clone-of-this-repository)



## Upgrading the EOS SDK in a clone of this repository
These steps are for users that are planning on creating a new version of the plugin
from a clone of the repository.

* Download EOS Dlls and install them in the proper location:
```
${PROJECT_ROOT}/Assets/Plugins/${PLATFORM}/${ARCH}/ 
# Where:
#    PROJECT_ROOT is the location of the cloned project on Disk
#    PLATFORM is the Unity Platform (Windows, Linux, macOS, Consoles)
#    ARCH is the architechture (x64, x86, ETC.)
```
Additionally, the C# will have to be changed. Currently they are modified
to support dynamic loading of the DLLs in the Editor to ensure seamless 
usage of the EOS SDK in the Unity editor. Sometimes, due to a change in how
the EOS SDK initializes, native code will need to be updated and recompiled before a
new plugin can be generated.

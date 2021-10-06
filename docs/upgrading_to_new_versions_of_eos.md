# Installing New versions of EOS

## NOTE
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

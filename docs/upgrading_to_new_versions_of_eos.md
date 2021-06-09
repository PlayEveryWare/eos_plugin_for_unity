# Installing New versions of EOS
* Download EOS Dlls and install them in the proper location:
```
${PROJECT_ROOT}/Assets/Plugins/${PLATFORM}/${ARCH}/ 
# Where:
#    PROJECT_ROOT is the location of the project on Disk
#    PLATFORM is the Unity Platform (Windows, Linux, macOS, Consoles)
#    ARCH is the architechture (x64, x86, ETC.)
```
Additionally, the C# will have to be changed. Currently they are modified
to support dynamic loading of the DLLs in the Editor to ensure seamless 
usage of the EOS SDK in the Unity editor.

# macOS 

## Preview Platform

To access Preview platforms, apply `EOS_PREVIEW_PLATFORM` in   
`Player Settings > Other Settings > Scripting Define Symbols`

---------------------------------------
## Building Standalone with Unity.

**Prerequisites:**

* The standard <a href="/readme.md#prerequisites">Prerequisites</a> for all platforms.
* macOS Device
* Unity macOS build module
* XCode
* Native Libraries  
  * Required for running Unity Editor
  * Required if using RTC (Microphone) functionality

### Additional Steps

**Native libraries**

* Run `Tools > Bulid Libraries > Mac` to build the dylibs needed for the mac build.
    * Set the path of `make` at `Preferences > EOS Plugin > Platform Library Build Settings > Make path`  
    * By default the path is `/usr/bin/make` or `usr/local/bin/make`
* Alternatively, manually use the makefile in `NativeCode/DynamicLibraryLoaderHelper_macOS/` by opening the terminal at the folder and run the command `make install`


## Mac Specific Caveats

* If running the plugin in UnityEditor, after modifying the configuration settings,
a UnityEditor reboot is needed for the changes to take place.  

* With Unity `2021.3.8f1` on Mac, building while overwriting the old build causes some weird behavior. Delete the old build before compiling a new build to avoid this problem

<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>

# <div align="center">macOS</div>
---

## Prerequisites:

> [!NOTE]
> Most environment setup tasks can be accomplished by executing the setup script located [here](/tools/scripts/setup-macos.sh) by running the following command from within a terminal window at the root of the repository:
> ```
> ./tools/scripts/setup-macos.sh
> ```

If you want to do that work manually, please refer to the following:

* The standard <a href="/README.md#prerequisites">Prerequisites</a> for all platforms.
* macOS Device
* Unity macOS build module
* XCode
* Native Libraries  
  * Required for running Unity Editor
  * Required if using RTC (Microphone) functionality

### Building Native Libraries

* Run `Tools -> EOS Plugin -> Build Libraries -> Mac` to build the dylibs needed for the mac build.
    * Set the path of `make` at `Edit -> Preferences -> EOS Plugin -> Platform Library Build Settings -> Make path`  
    * By default the path is `/usr/bin/make` or `usr/local/bin/make`
* Alternatively, manually use the makefile in `lib/NativeCode/DynamicLibraryLoaderHelper_macOS/` by opening the terminal at the folder and running the command: 

    ```bash
    make install
    ```

## Mac-Specific Caveats

* If running the plugin in UnityEditor, after modifying the configuration settings, a UnityEditor reboot is needed for the changes to take place.  

* With Unity `2021.3.8f1` on Mac, building while overwriting the old build can cause some weird behavior. Delete the old build or perform a "clean" before compiling in order to avoid this problem.

<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

# <div align="center">Environment Setup for Android</div>
---

## Unity and Modules

1. Install Unity (minimum version 2021.3.16f), preferably on Unity Hub

2. Open the "Add modules" window.

    <img src="/docs/images/unity_add_modules.png" width="500" />

2. Select and install the following modules:

    * Windows Build Support (Il2CPP)
    * Android SDK & NDK Tools 
    * OpenJDK

## Manage Android SDK

It is possible that the SDK modules installed through Unity Hub would be missing components to build your game.  

For example, Unity 2020.1.11 requires build-tool 30.0.3 to build the game but the tool is not included in the Unity Hub module installation.  

The following methods will show how to install the missing build-tool 30.0.3, and give an idea how to manage SDKs.

### Manage SDK with Android Studio (Recommended for better emulator support).
 
1. [Install Android Studio](https://developer.android.com/studio)
2. Run the SDK Manager

    <img src="/docs/images/android_studio_sdk_manager.png" width="500" />

### Manage SDK through command prompt

1. (On Windows) If you don't already have the environment variable `JAVA_HOME` as a System Variable, add a new one with the value set to `C:\Program Files\Unity\Hub\Editor\2021.3.16f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK`.

2. Run the command prompt and navigate to the `SDK\tools\bin` folder.

    On Windows:

    ```bash
    cd 'C:\Program Files\Unity\Hub\Editor\2021.3.16f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\tools\bin'
    ```

    For most *nix:

    ```bash
    ../Unity/Hub/Editor/2020.1.11f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/tools/bin
    ```

3. Run

    On Windows:

    ```bash
    .\sdkmanager.bat  "platform-tools" "platforms;android-30" "build-tools;30.0.3"
    ```

    For most *nix:

    ```bash
    sdkmanager  "platform-tools" "platforms;android-30" "build-tools;30.0.3"
    ```

## External Module Setup

Modules can be shared across multiple versions of Unity on the same device by specifying the shared install path in `Edit -> Preference -> External Tools`
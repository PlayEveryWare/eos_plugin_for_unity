<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

# <div align="center">Environment Setup for Android</div>
---

## Unity/SDK&NDK/JDK Setup

1. Install Unity (minimum version 2020.1.11), preferably on Unity Hub

2. Press "Add Modules", select and install the following modules.
    * Windows Build Support (Il2CPP)
    * Android SDK & NDK Tools 
    * OpenJDK

## Manage Android SDK

It is possible that the SDK modules installed through Unity Hub would be missing components to build your game.  
For example, Unity 2020.1.11 requires build-tool 30.0.3 to build the game but the tool is not included in the Unity Hub module installation.  
The following methods will show how to install the missing build-tool 30.0.3, and give an idea how to manage SDKs.

### Manage SDK with Android Studio (Recommended for better emulator support).
 
1. Install Android Studio
2. Run the SDK Manager on the top right corner.

> [!IMPORTANT]
> Remember to set the Android SDK Location (Administrator privileges may be required to install SDK).
  
### Manage SDK through command prompt

1. Run the command prompt and navigate to the `SDK\tools\bin` folder.
```
..\Unity\Hub\Editor\2020.1.11f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\tools\bin
```
2. Run
```
sdkmanager  "platform-tools" "platforms;android-30" "build-tools;30.0.3"
```

## External Module Setup

Modules can be shared across multiple versions of Unity on the same device  
by specifying the shared install path in `Edit -> Preference -> External Tools`

## External Module Install Links   
* <a href="https://www.openlogic.com/openjdk-downloads">OpenJDK Downloads | Download Java JDK 8 & 11 | OpenLogic</a>.


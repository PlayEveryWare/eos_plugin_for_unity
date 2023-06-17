# Unity - Android Build Support version table

-------------------------------------

These are some tested versions compatible for android build support in Unity

|  Unity  |Android SDK|Android build-tool| JDK |
|---------|-----------|------------------|-----|
| 2020.1.11f1 | Target API Level in build settings | v30.0.3 | 8 |
| 2021.3.8f1  | Target API Level in build settings | v30.0.3 | 8 |

*Target API must be larger than API Level 23 as defined in eos-sdk*

-------------------------------------

## Setting up Android Build Support


1. Start with Unity Hub, Go to Install and find the target Unity version.  

2. Select `Add Module > Android Build Support`

3. Install `SDK & NDK tools` and `OpenJDK`
   *  Install `OpenJDK` manually from [this page](https://www.openlogic.com/openjdk-downloads) is recommended as it is easier to set environment variables.  
   Remember to to locate your JDK installation folder at `Edit > Preferences > External Tools` 

*  At this point, with the `SDK & NDK tools` installed from Unity Hub, it might still fail to build an apk.  
   Try building an Android build should give us error logs that show what build tools are missing.

4. Install the SDK or build tools needed.  
   *  Using the `SDKManger` in `Android Studio` is a lot easier
   *  To make changes to installed Android SDKs in `Android Studio`, remember to run with admin privileges
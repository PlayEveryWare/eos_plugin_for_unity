## What is the Android Helper Library?
It's a dynamically linked library and AAR that brings in the C++ dependencies that the
EOS SO needs on Android, and provides a place for any native Android code to live.

## Building the Android Helper library.
Prerequisites:
* Android NDK installed
* Android SDK installed
* Cmake installed (Installed with NDK)
* Java
* Android Studio or Gradle configured to work with the Android SDK

To get it to compile, you'll need to have the android NDK, and android SDK setup.
The process to setup the SDK and NDK is mostly automated by either Android Studio or Unity.

Otherwise, you can just use gradle, and build it by modifying where it searches for 
the NDK and the SDK. While it should be possible to use the version installed for Unity to
compile the library, it isn't required.

Building should just be as simple as running ```gradle build``` if one's environment is setup correctly for 
gradle, or as easy as hitting 'build' in Android Studio.

After it's built, you'll need to copy the aar from the build directory into the Assets/Plugins/Android/ directory.
One can find the aar in UnityHelpers_Android/build/outputs/aar/ .

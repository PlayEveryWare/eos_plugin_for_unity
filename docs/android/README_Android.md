<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>

# <div align="center">Android</div>
---

## Prerequisites

* The standard <a href="/README.md#prerequisites">Prerequisites</a> for all platforms.
*  Check out this <a href="environment_setup_android.md#environment-setup-for-android">Environment Setup</a> doc for a quick setup guide.
*  More detailed instructions can be found in the links below.
    * <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/android-sdksetup.html">Android environment setup</a> for Unity.
    * The Android Build Support <a href="https://docs.unity3d.com/hub/manual/AddModules.html">module</a>.

## Importing the Plugin


You can follow the standard <a href="/README.md#importing-the-plugin">Importing the Plugin</a> process. With a few changes here when <a href="#running-the-samples">running the samples</a> and <a href="#configuring-the-plugin">configuring the plugin</a>.
> [!WARNING]
> If you choose the tarball method, when downloading the release on mac it may convert the `.tgz` into a `.tar` which is not compatible with unity. Changing the file extension back to a `.tgz` should fix this.

## Samples

You can follow the standard <a href="/README.md#samples">Samples</a> process.   
Please note the details in the <a href="#running-the-samples">Running the samples</a> section when running the samples from a build for Android.  

> [!WARNING] 
> The EOS Overlay is not implemented yet. When it is (due to the limitations of phones) the EOS Overlay will not be set to be openable by a physical button.

## Running the samples

When following the steps to <a href="/README.md#running-the-samples">run a sample</a> from a build for Android, follow the Unity doc for <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/android-sdksetup.html">Debugging on an Android device</a>, to connect your device to the engine.  
This will allow the smoother ```Build And Run``` option to work instead of just using the ```Build``` button.  

When running on a device you may need to <a href="https://developer.android.com/studio/debug/dev-options#enable">enable developer mode</a> on the device, then <a href="https://developer.android.com/studio/debug/dev-options#Enable-debugging">Enable USB debugging on your device</a>, as well as accepting any popups that appear on the phone during the process.

## Configuring the Plugin

You can follow the standard <a href="/README.md#configuring-the-plugin">Configuring the Plugin</a> process.  With the additional steps after saving the Main EOS Config.

## Additional Configuration Steps <a name="configuration-steps" />

1. Select the ```Android``` button.

    ![EOS Config UI](/docs/images/eosconfig_ui_android.gif)

2. Press ```Save All Changes```.

      > [!WARNING] 
      > This is required, even if you leave every field blank.

3. Update the <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/class-PlayerSettingsAndroid.html">Minimum API Level</a> to be at least ```Android 6.0 'Marshmallow' (API Level 23)```.

4. Optionally, set the plugin to <a href="/docs/android/link_eos_library_settings.md">link the EOS Library dynamically</a>.


# FAQ

See [frequently_asked_questions.md](/docs/frequently_asked_questions.md).

<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>


# <div align="center">iOS Getting Started</div>
---

## Prerequisites

* The standard <a href="/README.md#prerequisites">Prerequisites</a> for all platforms.
* <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/ios-environment-setup.html">iOS environment setup</a> for Unity.
* The iOS Unity <a href="https://docs.unity3d.com/hub/manual/AddModules.html">module</a>.
* Xcode 10.2.x.

## Importing the Plugin

You can follow the standard <a href="/README.md#importing-the-plugin">Importing the Plugin</a> process. With a few changes here when <a href="#running-the-samples">running the samples</a> and <a href="#configuring-the-plugin">configuring the plugin</a>.

> [!WARNING]
> If you choose the tarball method, when downloading the release on mac it may convert the `.tgz` into a `.tar` which is not compatible with unity. Changing the file extension back to a `.tgz` should fix this.

## Samples

You can follow the standard <a href="/README.md#samples">Samples</a> process. Please note the details in the <a href="#running-the-samples">Running the samples</a> section when running the samples from a build for iOS.

> [!WARNING]
> The EOS Overlay is not yet implemented for iOS.

## Running the samples

When following the steps to <a href="/README.md#running-the-samples">run a sample</a> from a build for iOS, in Xcode, open the `.xcodeproj` from the resulting build folder. Follow the Apple Developer instructions to build and run the app [here](https://developer.apple.com/documentation/xcode/running-your-app-in-simulator-or-on-a-device). If running on a device you may need to <a href="https://developer.apple.com/documentation/xcode/enabling-developer-mode-on-a-device">enable developer mode</a> on the device. This may require you to set up <a href="https://help.apple.com/xcode/mac/current/#/dev80cc24546">automatic signing</a> as well.

> [!NOTE]
> Find the build steps in the Unity docs <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/iphone-BuildProcess.html">here</a>.

## Configuring the Plugin

You can follow the standard <a href="/README.md#configuring-the-plugin">Configuring the Plugin</a> process. With the additional steps after saving the Main EOS Config.

## Additional Configuration Steps

1. Select the `iOS` button.

    ![EOS Config UI](/docs/images/eosconfig_ui_ios.gif)

2. Press `Save All Changes`.

# FAQ

See [frequently_asked_questions.md](/docs/frequently_asked_questions.md).

<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginImage.gif" alt="Epic Online Services Plugin for Unity" /> </div>
<br /><br /><br />

---



# <div align="center">$\textcolor{deeppink}{\textsf{iOS Getting Started}}$</div> <a name="getting-started" />
---

## Prerequisites


* The standard <a href="/readme.md#prerequisites">Prerequisites</a> for all platforms.
* The iOS Unity <a href="https://docs.unity3d.com/hub/manual/AddModules.html">module</a>.
* Xcode 10.2.x.
* <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/ios-environment-setup.html">iOS environment setup</a> for Unity

<br />

## Importing the Plugin


You can follow the standard <a href="/readme.md#importing-the-plugin">Importing the Plugin</a> process. With a few changes here when <a href="/readme.md#running-the-samples">running the samples</a> and <a href="/readme.md#configuring-the-plugin">configuring the plugin</a>.
> :heavy_exclamation_mark: If you choose the tarball method, when downloading the release it may convert the ```.tgz``` into a ```.tar``` which is not compatible with unity. Changing the file extension back to a ```.tgz``` should fix this.

<br />

## Samples

You can follow the standard <a href="/readme.md#samples">Samples</a> process.  With the additional clarification when running from a build.
> :heavy_exclamation_mark: The EOS Overlay is not implemented yet. But when it is due to the limitations of phones, the EOS Overlay is not set to be openable by a physical button.

<br />

## Running the samples

When following the steps to <a href="/readme.md#running-the-samples">run a sample</a> from a build for ios, in Xcode, open the ```.xcodeproj``` from the resulting build folder. Follow the Apple Developer instructions to build and run the app <a href="https://developer.apple.com/documentation/xcode/running-your-app-in-simulator-or-on-a-device">here</a>. If running on a device you may need to <a href="https://developer.apple.com/documentation/xcode/enabling-developer-mode-on-a-device">enable developer mode</a> on the device. This may require you to set up <a href="https://help.apple.com/xcode/mac/current/#/dev80cc24546">automatic signing</a> as well.
> :heavy_exclamation_mark: Find the build steps in the Unity docs <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/iphone-BuildProcess.html">here</a>.

<br />

<br />

## Configuring the Plugin

You can follow the standard <a href="/readme.md#configuring-the-plugin">Configuring the Plugin</a> process. With the additional steps between ```3.``` and ```4.```.


## Additional Configuration Steps <a name="configuration-steps" />

1. Select the ```iOS``` button.

    ![EOS Config UI](/docs/images/eosconfig_ui_ios.gif)

2. Press ```Save All Changes```.

      > :heavy_exclamation_mark: This is required, even if you leave every field blank.

<br />



# <div align="center">$\textcolor{deeppink}{\textsf{FAQ}}$</div> <a name="faq" />
---

See [docs/frequently_asked_questions.md](/docs/frequently_asked_questions.md).

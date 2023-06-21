<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginImage.gif" alt="Epic Online Services Plugin for Unity" /> </div>
<br /><br /><br />

---



# <div align="center">$\textcolor{deeppink}{\textsf{Android Getting Started}}$</div> <a name="getting-started" />
---

## Prerequisites


* The standard <a href="/readme.md#prerequisites">Prerequisites</a> for all platforms.
* Windows Build Support (IL2CPP).
* <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/android-sdksetup.html">Android environment setup</a> for Unity.
* The Android Build Support <a href="https://docs.unity3d.com/hub/manual/AddModules.html">module</a>.

    > :heavy_exclamation_mark: You can <a href="https://www.openlogic.com/openjdk-downloads">install OpenJDK</a> externally so it is easier to set environment variables.

    > :heavy_exclamation_mark: ```Android Studio``` or other official sites can be used to get the ```SDK```,```NDK```, or ```Gradle``` as needed.

  > :heavy_exclamation_mark: Remember have unity point to these changes ```Edit -> Preferences -> External Tools```.


<br />

## Importing the Plugin


You can follow the standard <a href="/readme.md#importing-the-plugin">Importing the Plugin</a> process. With a few changes here when <a href="#configuring-the-plugin">configuring the plugin</a>.
> :heavy_exclamation_mark: If you choose the tarball method, when downloading the release on mac it may convert the ```.tgz``` into a ```.tar``` which is not compatible with unity. Changing the file extension back to a ```.tgz``` should fix this.

<br />

## Samples

> :heavy_exclamation_mark: The EOS Overlay is not implemented yet. But when it is due to the limitations of phones, the EOS Overlay is not set to be openable by a physical button.

<br />

## Configuring the Plugin

You can follow the standard <a href="/readme.md#configuring-the-plugin">Configuring the Plugin</a> process.  With the additional steps after saving the Main EOS Config.


## Additional Configuration Steps <a name="configuration-steps" />

1. Select the ```Android``` button.

    ![EOS Config UI](/docs/images/eosconfig_ui_android.gif)

2. Press ```Save All Changes```.

      > :heavy_exclamation_mark: This is required, even if you leave every field blank.

3. Update the <a href="https://docs.unity3d.com/2021.3/Documentation/Manual/class-PlayerSettingsAndroid.html">Minimum API Level</a> to be atleast ```Android 6.0 'Marshmallow' (API Level 23)```.

4. Optionally, set the plugin to <a href="/docs/android/link_eos_library_settings.md">link the EOS Library dynamically</a>.


<br />



# <div align="center">$\textcolor{deeppink}{\textsf{FAQ}}$</div> <a name="faq" />
---

See [docs/frequently_asked_questions.md](/docs/frequently_asked_questions.md).

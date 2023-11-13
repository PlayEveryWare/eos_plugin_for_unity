<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Readme" width="5%"/></a>

# Samples

<img src="docs/images/sample_screen_lobby.gif" alt="Lobby Screenshot" width="48%"/> <img src="docs/images/sample_screen_achievements.gif" alt="Achievements Storage Screenshot" width="48%"/>

The included samples show examples of fully functional [feature implementations](/docs/eos_features.md) to validate client and dev portal configuration as well as help with EOS integration into your own project. The samples are a collection of scenes that are imported from the UPM package, and include a series of scripts that function as generalized managers for each supported EOS SDK feature and platform.

> [!NOTE]
> The generalized managers are a great starting point for feature integration into your own project. They are named as `EOS[Feature/Platform name]Manager.cs`.

## Importing Samples

1. Select the `PlayEveryWare EOS Plugin for Unity` in the Package Manager window.

    ![Unity Install Samples](/docs/images/unity_install_samples.gif)

2. Open the `Samples` dropdown.

3. Select `Import` for each of the sample packs, to bring in the Sample scenes.

    > [!NOTE]
    > The samples are placed in `Assets/Samples` for personal modification.

4. In the Unity editor menu bar, open `File -> Build Settings`.

5. In the `Project` window, navigate to the scenes folders containing their respective sample scenes. `\Assets\Samples\PlayEveryWare EOS Plugin for Unity\[Version #]\[Pack Name]\Scenes`.

6. Add the scenes to the `Scenes In Build` section of the `Build Settings` window. This can be done quickly by using the `Shift` key to select each scene at the same time, then dragging them into the proper area. Repeating for each sample pack folder.

  > [!WARNING]
  > If you have other scenes already, and plan to look at the samples in a build, drag a sample scene to be the 0th scene in Build Settings before you build.

## Running the samples

> [!IMPORTANT]
> The plugin must be <a href="/docs/configure_plugin.md">configured</a> for samples to be functional. Some Samples may not be accessible if the extra packs were not <a href="http://github.com/PlayEveryWare/eos_plugin_for_unity/blob/development/README.md#importing-samples">imported</a>.

Sample walkthroughs for each scene can be found [here](/docs/Walkthrough.md).

<details>
  <summary><b>Steps to run a sample in editor</b></summary>

  > [!NOTE]
  > The Social Overlay Feature is not supported in editor.

  1. In the Unity editor, open the desired sample scene from the imported Scenes folder.

  2. Press the play button at the top of the editor.

  3. Login with a selected authentication type. 
    - `Account Portal` and `PersistentAuth` is easiest for the first time. 
    - `Dev Auth` can be used for faster iteration
    - To explore features that don't require an Epic Games Account, see the table in the [Supported EOS SDK Features](#supported-eos-sdk-features) section of this document.

    ![Auth and Friends Screenshot](/docs/images/sample_screen_account_login.gif)

    > [!NOTE]
    > Additional info on login type options, implementation, and use cases can be found [here](/docs/player_authentication.md).

</details>

<details>
  <summary><b>Steps to run a sample from a build</b></summary>
<br />

  > [!NOTE] 
  > Check the [Prerequisites](http://github.com/PlayEveryWare/eos_plugin_for_unity/blob/development/README.md#prerequisites) as there may be specific requirements for a player's computer.
  > For instance, Windows requires the players to have `The latest Microsoft Visual C++ Redistributable` installed on their computer in order to play any distributed builds.

  1. In the Unity editor menu bar, open `File -> Build Settings`.
    
      > [!NOTE]
      > If you have non-sample scenes, drag a sample scene to be the 0th scene in Build Settings before you build.

  2. Choose your desired platform, and settings, hitting `Build` as you normally would.

  3. Run your build.

      > [!WARNING] 
      > A Windows build, is started by running the `EOSBootstrapper` application in the resulting build, and **not** the game application itself. It is for this (and similar) reasons that the `Build And Run` button may not always function as expected.

  4.  Login with a selected authentication type. 
    - `Account Portal` and `PersistentAuth` is easiest for the first time. 
    - `Dev Auth` can be used for faster iteration
    - To explore features that don't require an Epic Games Account, see the table in the [Supported EOS SDK Features](#supported-eos-sdk-features) section of this document.

    ![Auth and Friends Screenshot](/docs/images/sample_screen_account_login.gif)

    > [!NOTE]
    > Additional info on login type options, implementation, and use cases can be found [here](/docs/player_authentication.md).

</details>
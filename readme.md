<br /><br /><br />

# Epic Online Services<br /> Plugin for Unity

  Table of Contents
  <ol>
    <li>
      <a href="#overview">Overview</a>
      <ul>
        <li><a href="#supported-platforms">Supported Platforms</a></li>
        <li><a href="#supported-eos-sdk-features">Supported EOS SDK Features</a></li>
      </ul>
    </li>
    <li>
      <a href="#centergetting-startedcenter">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#importing-the-plugin">Importing the Plugin</a></li>
        <li><a href="#samples">Samples</a></li>
        <li><a href="#configuring-the-plugin">Configuring the Plugin</a></li>
      </ul>
    </li>
    <li><a href="#centerplugin-supportcenter">Plugin Support</a></li>
    <li><a href="#centersource-code-contributor-notescenter">Source Code Contributor Notes</a></li>
    <li><a href="#centerfaqcenter">FAQ</a></li>
  </ol>



# <p style="text-align:center;color:DeepPink">Overview</p> <a name="overview" />
---

The [eos_plugin_for_unity repository](https://github.com/PlayEveryWare/eos_plugin_for_unity) contains the source code for development, samples and support for the [Epic Online Services Plugin for Unity (UPM Package)](https://github.com/PlayEveryWare/eos_plugin_for_unity_upm).

#### Repo Contents:
* Additional plugin Documentation can be found in the [docs/ directory](docs/)
* A Unity Project for development of feature managers and samples
* Native Source for ```DynamicLibraryLoaderHelper``` and ```GfxPluginNativeRender```
* A Tool to build a Unity Package Manager compatible UPM


#### Plugin Features: [(Indepth Details)](docs/plugin_advantages.md)

* Social Overlay support across platforms
* Feature specific sample scenes, that include manager classes for common uses of EOS SDK API
* Custom Unity Tool for configuring EOS settings and saving to a JSON file
* Unity editor playback support, handled by reloading EOS SDK

#### Plugin Details:

* Targets [Unity 2021.3.8f1](https://unity.com/releases/editor/whats-new/2021.3.8), for best results a version of Unity 2021 is prefered
* Targets [EOS SDK 1.15.4](https://dev.epicgames.com/docs/epic-online-services/release-notes#1154---2022-nov-16) *(bundled with plugin)*



<br />

### Supported Platforms <a name="centergetting-startedcenter" />

The support level of each target platform in Unity as of the current release of the plugin.

| Supported | Preview | Unsupported at Present |
| - | - | - |
| Unity Editor (No Social Overlay) | [Linux](docs/linux/linux_supported_versions.md) | WebGL |
| Windows Standalone x64 | [MacOS](docs/macOS/macOS_supported_versions.md) | Universal Windows Platform x86 |
| Windows Standalone x86 | Console Platforms | Unity Web Player |
| Universal Windows Platform x64 | | |
| Android | | |
| iOS | | |
> ❗ Enable `EOS_PREVIEW_PLATFORM` to access Preview platforms

<br />

## Supported EOS SDK Features

  The EOS Plugin for Unity will be updated over time to support the new content as the EOS SDK continues to release new features and functionality. 

The support level of each EOS SDK features as of the current release of the plugin.

| Supported | | | |
| - | - | - | - |
| [Achievements](https://dev.epicgames.com/docs/game-services/achievements) | [Authentication](https://dev.epicgames.com/docs/epic-account-services/auth-interface) | [Ecommerce](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/ecom) | [Friends](https://dev.epicgames.com/docs/epic-account-services/eos-friends-interface) |
| [Leaderboards](https://dev.epicgames.com/docs/game-services/leaderboards) | [Lobby](https://dev.epicgames.com/docs/game-services/lobbies) | [Lobby with Voice](https://dev.epicgames.com/docs/game-services/voice#voicewithlobbies) | [NAT P2P](https://dev.epicgames.com/docs/game-services/p-2-p) | |
| [Player Data Storage](https://dev.epicgames.com/docs/game-services/player-data-storage) | [Presence](https://dev.epicgames.com/docs/epic-account-services/eos-presence-interface) | [Sessions](https://dev.epicgames.com/docs/game-services/sessions) | [Social Overlay](https://dev.epicgames.com/docs/epic-account-services/social-overlay-overview) (Not Supported in Editor) | |
| [Stats](https://dev.epicgames.com/docs/game-services/eos-stats-interface) | [Title Storage](https://dev.epicgames.com/docs/game-services/title-storage) | [Reports](https://dev.epicgames.com/docs/game-services/reports-interface) | [Sanctions](https://dev.epicgames.com/docs/game-services/sanctions-interface) |
| [Anti-Cheat](https://dev.epicgames.com/docs/game-services/anti-cheat) | [Custom Invites](https://dev.epicgames.com/docs/game-services/custom-invites-interface) | [Metrics](https://dev.epicgames.com/docs/game-services/eos-metrics-interface) | |


| Unsupported at Present | |
| - | - |
|  [EOS Mod SDK](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/mods) |  [Voice Trusted Server](https://dev.epicgames.com/docs/game-services/voice#voicewithatrustedserverapplication) |

<br />

<details>
  <summary><b>The List EOS Features Shown by Sample</b></summary>

| Sample Name | EOS Features |
| - | - |
| - All Samples - | [Authentication](https://dev.epicgames.com/docs/epic-account-services/auth-interface), [Social Overlay](https://dev.epicgames.com/docs/epic-account-services/social-overlay-overview) |
| Achievements | [Achievements](https://dev.epicgames.com/docs/game-services/achievements) |
| Auth & Friends | [Friends](https://dev.epicgames.com/docs/epic-account-services/eos-friends-interface), [Presence](https://dev.epicgames.com/docs/epic-account-services/eos-presence-interface) |
| Custom Invites | [Custom Invites](https://dev.epicgames.com/docs/game-services/custom-invites-interface) |
| Leaderboards | [Leaderboards](https://dev.epicgames.com/docs/game-services/leaderboards), [Stats](https://dev.epicgames.com/docs/game-services/eos-stats-interface) |
| Lobbies | [Anti-Cheat](https://dev.epicgames.com/docs/game-services/anti-cheat), [Lobby](https://dev.epicgames.com/docs/game-services/lobbies), [Lobby with Voice](https://dev.epicgames.com/docs/game-services/voice#voicewithlobbies) |
| Metrics | [Metrics](https://dev.epicgames.com/docs/game-services/eos-metrics-interface) |
| Peer 2 Peer | [NAT P2P](https://dev.epicgames.com/docs/game-services/p-2-p) |
| Performance Stress Test |  |
| Player Data Storage | [Player Data Storage](https://dev.epicgames.com/docs/game-services/player-data-storage)  |
| Player Reports & Sanctions | [Reports](https://dev.epicgames.com/docs/game-services/reports-interface), [Sanctions](https://dev.epicgames.com/docs/game-services/sanctions-interface) |
| Sessions & Matchmaking | [Sessions](https://dev.epicgames.com/docs/game-services/sessions) |
| Store | [Ecommerce](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/ecom) |
| Title Storage | [Title Storage](https://dev.epicgames.com/docs/game-services/title-storage) |
| P2P Netcode | [NAT P2P](https://dev.epicgames.com/docs/game-services/p-2-p) |
| - | - |
| None (Not Supported) | [EOS Mod SDK](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/mods), [Voice Trusted Server](https://dev.epicgames.com/docs/game-services/voice#voicewithatrustedserverapplication) |
</details>

<br />

# Getting Started
---

## Prerequisites


* An Epic Games Account, you can sign up for [here](https://dev.epicgames.com/portal/)
* A product configured on the [Epic Games Developer Portal](https://dev.epicgames.com/portal/)
* A Unity project to integrate the plugin into, it can be a blank project
> ❗ Find the plugin's targeted Unity version <a href="#plugin-details">here</a>

<br />

## Importing the Plugin


There are two options to install the package, from a <a href="#adding-the-package-from-a-tarball">tarball</a> [Quickest to start], or from a <a href="#adding-the-package-from-a-git-url">GIT URL</a> [Quickest for updates] 

## Adding the package from a tarball
<br />

1. Download the latest release tarball, ```"com.playeveryware.eos-[version].tgz"``` [here](https://github.com/PlayEveryWare/eos_plugin_for_unity/releases).
    > ❗ If one uses the source download it will be missing all the git-lfs files (i.e. binaries, dynamic libraries). 

2. Move the downloaded tarball into your project folder, but outside of the ```Assets``` folder.

3. From the Unity Editor, open the Package Manager
    * ```Window -> Package Manager```

      ![unity tools package manager](docs/images/unity_tools_package_manager.gif)

4. Click the ```+``` button in the top left of the window

    ![Unity Add Tarball Package](docs/images/unity_package_tarball.gif)

5. Select '```Add package from tarball```'
6. Navigate to the directory containing the tarball, select and ```Open``` the tarball
7. After the package has finished installing, <a href="#samples">import the samples</a>.

8. Finally, <a href="#configuring-the-plugin">configure the plugin</a>.

> ❗ The Unity doc for adding a tarball can be found [here](https://docs.unity3d.com/Manual/upm-ui-tarball.html)

<br />

## Adding the package from a git URL
<br />

1. Setup Unity for [Git Dependency](https://docs.unity3d.com/Manual/upm-git.html).
2. Install [git and git-lfs](https://docs.unity3d.com/Manual/upm-git.html#req)
3.  From the Unity Editor, open the Package Manager
    * ```Window -> Package Manager```

      ![unity tools package manager](docs/images/unity_tools_package_manager.gif)

3. Click the ```+``` button in the top left of the window

    ![Unity Add Git Package](docs/images/unity_package_git.gif)

4. Select '```Add Package from Git URL```'  

6. Paste in ```git@github.com:PlayEveryWare/eos_plugin_for_unity_upm.git```
   or ```https://github.com/PlayEveryWare/eos_plugin_for_unity_upm.git```

7. After the package has finished installing, <a href="#samples">import the samples</a>.

8. Finally, <a href="#configuring-the-plugin">configure the plugin</a>.

> ❗ The Unity doc for adding a git url can be found [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

<br />

## Samples


The included samples show examples of fully functional <a href="#supported-eos-sdk-features">feature implemenation</a> to validate client and dev portal configuration aswell as help with EOS integration into your own project. The samples are a collection of scenes that are imported from the UPM package, and include a series of scripts that function as generalized managers for each supported EOS SDK feature and platform.


  > ❗ The generalized managers are a great starting point for feture integration into your own project. They are named as ```EOS[Feature/Platform name]Manager.cs```

<br />

## Importing the samples
<br />

1. Select the ```Epic Online Services Plugin for Unity``` in the Package Manager window.

    ![Unity Install Samples](docs/images/unity_install_samples.gif)

2. Open the ```Samples``` dropdown

3. Select ```Import``` to bring in the Sample scenes

  > ❗ The samples are placed in ```Assets/Samples``` for personal modification

4. In the Unity editor menu bar, open ```File->Build Settings```

5. In the ```Project``` window, navigate to the scenes folder containing all the sample scenes.

6. Add the scenes to the ```Scenes In Build``` section of the ```Build Settings``` window. This can be done quickly by using the ```Shift``` key to select each scene at the same time, then dragging them into the proper area.

7. Drag around each scene in the ```Scenes In Build``` section of the ```Build Settings``` window, to rearange them so that they are indexed in the folowing order.

| Scene Name | Index |
| - | - |
| Achievements | 0 |
| AuthAndFriends | 1 |
| CustomInvites | 2 |
| Leaderboards | 3 |
| Lobbies | 4 |
| Metrics | 5 |
| Peer2Peer | 6 |
| PerformanceStressTest | 7 |
| PlayerDataStorage | 8 |
| PlayerReportsAndSanctions | 9 |
| SessionsMatchmaking | 10 |
| Store | 11 |
| TitleStorage | 12 |
| TransportLayer | 13 |

<br />

## Running the samples

> ❗ The plugin must be configured for samples to be functional

Sample walkthroughs can be found [here](docs/Walkthrough.md).

<details>
  <summary><b>Steps to run a sample in editor</b></summary>

> ❗ The Social Overlay Feature is not supported in editor

1. In the Unity editor, open the desired sample scene from the imported Scenes folder

2. Press the play button at the top of the editor

3. Login with a selected authentication type, ```Account Portal``` is recommended for the first time.

    ![Auth and Friends Screenshot](docs/images/sample_screen_account_login.gif)

</details>

<details>
  <summary><b>Steps to run a sample from a build</b></summary>

1. In the Unity editor menu bar, open ```File->Build Settings```

2. Choose your desired platform, and settings, hitting ```Build``` as you normally would.

3. Run your build.

> ❗ A Windows build, is started by running the ```EOSBootstrapper``` application in the resulting build, and not the game application itself. it is for this and similar reasons that the ```Build And Run``` button may not always function as it usually would.

4.  Login with a selected authentication type, ```Account Portal``` is recommended for the first time.


</details>

<br />

<br />

## Configuring the Plugin


To function, the plugin needs some information from your EOS project.

## Configuration Steps

1. In the Unity editor menu bar, open ```Tools -> EpicOnlineServicesConfigEditor```

    ![EOS Config Menu](docs/images/unity_tools_eosconfig.gif)

2. From the [developer portal](https://dev.epicgames.com/portal/), copy the configuration values listed below, and paste them into the similarly named fields in the ```EOS Config Editor``` window, under the ```Main``` portion of the config:
    * ProductName
    * ProductVersion
    * [ProductID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#P?term=ProductId)
    * [SandboxID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#S?term=SandboxId)
    * [DeploymentID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=DeploymentId)
    * [ClientSecret](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#C?term=OAuth%20ClientSecret)
    * [ClientID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#C?term=OAuth%20ClientId)
    * Encryption Key
      > ❗ Click the Generate button to create a random key, if you haven't already configured an encryption key in the EOS portal

    ![EOS Config UI](docs/images/eosconfig_ui.gif)

3. Press ```Save All Changes```

4. Navigate to ```Packages/Epic Online Services for Unity/Runtime``` via the ```Project``` window.

5. Add the ```EOSManager.prefab```, to each of youre game's scenes.

6. Simply attach ```EOSManager.cs (Script)``` to a Unity object and it will intialize the plugin with the specified configuration in ```OnAwake()```.
      > ❗ The <a href="#samples">samples</a> did this last step for you!

<br />

## Disable on selected platforms

See [docs/frequently_asked_questions.md](docs/frequently_asked_questions.md)


<br />

# Plugin Support
---

Epic Online Services Plugin for Unity API Documentation can be found at https://eospluginforunity.playeveryware.com

For issues related to integration or usage of the Unity plugin, please create a ```New Issue``` under the [Issues](https://github.com/PlayEveryWare/eos_plugin_for_unity/issues) tab in the github repo.

For issues related to Epic Online Services SDK, Epic Dev Portal or general EOS SDK information, please go to [Epic Online Services Community Support](https://eoshelp.epicgames.com/)

Detailed descriptions and usage for EOS SDK Interfaces, can be found at [EOS Developer Documentation: Game Services](https://dev.epicgames.com/docs/services/en-US/GameServices/index.html).

<br />

# Source Code Contributor Notes
---

The following are guidlines for helping contribute to this open source project.

## Contributor Prerequisites

* Ensure At least Visual Studio 2017 is installed.
* Ensure At least Unity 2020.1.11f1 is installed
* Ensure required Platform SDKs are installed (Windows, Linux, macOS, Android, iOS, Consoles)

## Build steps For Native Libraries

 Build the Visual Studio solutions for the native DLLs, extra platform specifc instructions may be located in the docs for that platform.

  1. In your local repository, navigate to the ```DynamicLibraryLoaderHelper``` folder of your platform choice in [NativeCode](NativeCode)

  > ❗ These files are not included with the package imported via tarball or git url.

  2. Open and build the ```DynamicLibraryLoaderHelper``` sln in Visual Studio.

  > ❗ A successful build will place the correct binaries in the proper locations for Unity to initialize EOS SDK.

## Coding Standards

See [standards.md](docs/standards.md)

## Class description

See [docs/class_description.md](docs/class_description.md)

<br />

# FAQ
---

See [docs/frequently_asked_questions.md](docs/frequently_asked_questions.md)
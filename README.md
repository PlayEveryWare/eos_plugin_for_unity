<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>

**Table of Contents:**
1. [Overview](#overview)
    - [Plugin Features](#plugin-features)
    - [Repo Contents](#repo-contents)
    - [Targetted Versions](#targetted-versions)
2. [Platform Support](#platform-support)
    - [Supported Platforms](#supported)
    - [Unsupported Platforms](#unsupported)
3. [Supported EOS SDK Features](#supported-eos-sdk-features)
4. [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Importing the Plugin](#importing-the-plugin)
        - [Adding the package from tarball](#adding-the-package-from-a-tarball)
        - [Adding the package from git url](#adding-the-package-from-a-git-url)
    - [Configuring the Plugin](#configuring-the-plugin)
5. [Samples](#samples)
    - [Importing Samples](#importing-samples)
    - [Running the Samples](#running-the-samples)
6. [Support / Contact](#support--contact)
7. [Contributor Notes](#contributor-notes)
    - [Environment Setup](#environment-setup)
        - [Linux](#linux)
        - [Windows](#windows)
        - [macOS](#macos)
    - [Building Native Libraries](#building-native-libraries)
    - [Coding Standards](#coding-standards)
    - [Core Classes](#core-classes)
8. [FAQ](#faq)

# Overview

The PlayEveryWare EOS Plugin for Unity brings the free services from Epic that connect players across all platforms and all stores to Unity in an easy-to-use package. Find more information on EOS [here](https://dev.epicgames.com/en-US/services) and read the Epic docs on the services [here](https://dev.epicgames.com/docs/epic-online-services).

This repository contains the source code for development and serves as a destination for support for the [PlayEveryWare EOS Plugin for Unity (UPM Package)](https://github.com/PlayEveryWare/eos_plugin_for_unity_upm).

Out of the box, this project demonstrates (through a collection of sample scenes) each feature of the Epic Online Services SDK[^1]. The sample scenes (coupled with accompanying documentation) can be used to get an idea of how you can implement all the online features you want in your game!

See [this](/docs/plugin_advantages.md) for a more complete overview of the advantages of using EOS with Unity.

[^1]: See [here](#supported-eos-sdk-features) for which SDK features specifically are demonstrated.

> [!NOTE]
> If you are **not** interested in the _development_ of the EOS Plugin project (and instead just want to get to using it) you can follow the instructions [here](#importing-the-plugin) on how to start using the most recently released version of the EOS Plugin.

## Repo Contents:

* Additional plugin Documentation can be found in the [docs](/docs/) directory.
* A Unity Project for development of feature managers and samples.
* Native Source for `DynamicLibraryLoaderHelper` and `GfxPluginNativeRender`.
* A Tool to build a Unity Package Manager compatible UPM (again, if you would like to just start making use of the plugin, you can use the already created UPM [here](https://github.com/PlayEveryWare/eos_plugin_for_unity_upm)).

## Targetted Versions:

* Targets [Unity 2021.3.8f1](https://unity.com/releases/editor/whats-new/2021.3.8), for best results a version of Unity 2021 is preferred.
* Targets [EOS SDK 1.16.0](https://dev.epicgames.com/docs/epic-online-services/release-notes#116---2023-aug-16) *(bundled with plugin)*.

# Platform Support

## Supported:

- Windows (Standalone x86 & x64)
- Universal Windows Platform (x64)
- [Android](/docs/android/README_Android.md) [^1]
- [iOS](/docs/iOS/README_iOS.md) [^1]
- [Linux](/docs/linux/linux_supported_versions.md) [^1]
- [MacOS](/docs/macOS/README_macOS.md)
- Nintendo Switch (for details, please see the Nintendo Developer Portal)

We are always looking to add the functionality of the plugin to more platforms, and have functionality for some in private development

[^1]: Social overlay feature is currently not supported on this platform.

## Unsupported:

- WebGL
- Universal Windows Platform (x86)
- Unity Web Player

# Supported EOS SDK Features

Below is a table summarizing the level of support the EOS Plugin for Unity provides. Most features are demonstrated via sample scenes provided in the project, and links to the guide for each corresponding sample scene are listed below. In some cases (such as Anti-Cheat) the feature is not very well demonstrated with a scene. In those cases, a link to information about how the plugin utilizes the feature. In some cases (such as logging and overlay) the features are not implemented in any one scene specifically, but in all of them.

Use the "Select Demo Scene" dropdown in the application to select the sample scene that corresponds with the walkthrough. 

There are many EOS features that do not require your player to have an Epic Games Account (EGA) - such features are also marked accordingly in the following table.

| Feature | Status | Sample Scene Walkthrough | Requires EGA |
| :-- | :-: | :-- | :-: |
|[Achievements](https://dev.epicgames.com/docs/game-services/achievements)                 | ✅ | ["Achievements"](/docs/scene_walkthrough/achievements_walkthrough.md)                                                  | No  |
|[Anti-Cheat](https://dev.epicgames.com/docs/game-services/anti-cheat)                     | ✅ | ["Information"](/docs/easy_anticheat_configuration.md)                                                                             | No  |
|[Authentication](https://dev.epicgames.com/docs/epic-account-services/auth-interface)     | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md), [Information](/docs/player_authentication.md) | Yes |
|[Custom Invites](https://dev.epicgames.com/docs/game-services/custom-invites-interface)   | ✅ | ["Custom Invites"](/docs/scene_walkthrough/customInvites_walkthrough.md)                                               | No  |
|[Connect Interface](https://dev.epicgames.com/docs/game-services/eos-connect-interface)   | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md)                                                               | No  |
|[Ecommerce](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/ecom)    | ✅ | ["Store"](/docs/scene_walkthrough/store_walkthrough.md), [Information](/docs/ecom.md)                                        | Yes |
|[Friends](https://dev.epicgames.com/docs/epic-account-services/eos-friends-interface)     | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md)                                                               | Yes |
|[Leaderboards](https://dev.epicgames.com/docs/game-services/leaderboards)                 | ✅ | ["Leaderboards"](/docs/scene_walkthrough/leaderboards_walkthrough.md)                                                               | No  |
|[Lobby](https://dev.epicgames.com/docs/game-services/lobbies)                             | ✅ | ["Lobbies"](/docs/scene_walkthrough/lobbies_walkthrough.md)                                                                    | No  |
|[Lobby with Voice](https://dev.epicgames.com/docs/game-services/voice#voicewithlobbies)   | ✅ | ["Lobbies"](/docs/scene_walkthrough/lobbies_walkthrough.md), [Information](/docs/enabling_voice.md)                            | No  |
|[Logging Interface](https://dev.epicgames.com/docs/game-services/eos-logging-interface)   | ✅ | NA                                                                                                                               | No  |
|[Metrics](https://dev.epicgames.com/docs/game-services/eos-metrics-interface)             | ✅ | ["Metrics"](/docs/scene_walkthrough/metrics_walkthrough.md)                                                                    | No  |
|[Mod SDK](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/mods)      | ❌ | NA                                                                                                                               | Yes |
|[NAT P2P](https://dev.epicgames.com/docs/game-services/p-2-p)                                               | ✅ | ["Peer 2 Peer"](/docs/scene_walkthrough/P2P_walkthrough.md), ["P2P Netcode"](/docs/scene_walkthrough/P2P_netcode_walkthrough.md) | No  |
|[Platform Interface](https://dev.epicgames.com/docs/game-services/eos-platform-interface)                   | ✅ | NA | No  |
|[Player Data Storage](https://dev.epicgames.com/docs/game-services/player-data-storage)                     | ✅ | ["Player Data Storage"](/docs/scene_walkthrough/player_data_storage_walkthrough.md)                                                        | No  |
|[Presence](https://dev.epicgames.com/docs/epic-account-services/eos-presence-interface)                     | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md)                                                               | Yes |
|[Progression Snapshot Interface](https://dev.epicgames.com/docs/epic-account-services/progression-snapshot) | ❌ | NA                                                                                                             | Yes |
|[Reports](https://dev.epicgames.com/docs/game-services/reports-interface)                 | ✅ | ["Player Reports & Sanctions"](/docs/scene_walkthrough/player_reports_and_sanctions_walkthrough.md)                                               | No  |
|[Sanctions](https://dev.epicgames.com/docs/game-services/sanctions-interface)             | ✅ | ["Player Reports & Sanctions"](/docs/scene_walkthrough/player_reports_and_sanctions_walkthrough.md)                                               | No  |
|[Sessions](https://dev.epicgames.com/docs/game-services/sessions)                         | ✅ | ["Sessions & Matchmaking"](/docs/scene_walkthrough/sessions_and_matchmaking_walkthrough.md)                                                   | No  |
|[Social Overlay](https://dev.epicgames.com/docs/epic-account-services/social-overlay-overview) / [UI Interface](https://dev.epicgames.com/docs/epic-account-services/eosui-interface) | ✅ | [Information](/docs/overlay.md)        | Yes |
|[Stats](https://dev.epicgames.com/docs/game-services/eos-stats-interface)                 | ✅ | ["Leaderboards"](/docs/scene_walkthrough/leaderboards_walkthrough.md)                                                               | No  |
|[Title Storage](https://dev.epicgames.com/docs/game-services/title-storage)               | ✅ | ["Title Storage"](/docs/scene_walkthrough/title_storage_walkthrough.md)                                                              | No  |
|[User Info Interface](https://dev.epicgames.com/docs/epic-account-services/eos-user-info-interface) | ✅ | NA                                                                                                                     | Yes |
|[Voice Trusted Server](https://dev.epicgames.com/docs/game-services/voice#voicewithatrustedserverapplication) | ❌ | NA                                                                                                           | No  |

Efforts will be made to add corresponding support to features as they are added to the Epic Online Services SDK. The table above reflects the features as of November 2023.

# Getting Started

## Prerequisites

* An Epic Games Account, you can sign up for [here](https://www.epicgames.com/id/register) (_although, most [features](#supported-eos-sdk-features) do not require an Epic Games Account_).
* A product configured on the [Epic Games Developer Portal](https://dev.epicgames.com/portal/).
* A Unity project to integrate the plugin into.

> [!NOTE]
> Your system should also satisfy [Unity's system requirements](https://docs.unity3d.com/2021.3/Documentation/Manual/system-requirements.html) as well as the [EOS system requirements](https://dev.epicgames.com/docs/epic-online-services/eos-get-started/system-requirements)

## Importing the Plugin

There are two options to install the package:
* Via a [tarball](#adding-the-package-from-a-tarball) _(easiest to get started quickly)_.
* From a [git url](#adding-the-package-from-a-git-url) _(this method has the possible advantage of keeping the plugin up-to-date, if that's something that you would prefer)_.

### Adding the package from a tarball

1. Download the latest release tarball, `"com.playeveryware.eos-[version].tgz"` [here](https://github.com/PlayEveryWare/eos_plugin_for_unity/releases).

    > [!WARNING]
    > Do *not* attempt to create a tarball yourself from the source, unless you know what you are doing with respect to [Git LFS](https://docs.github.com/en/repositories/working-with-files/managing-large-files/configuring-git-large-file-storage).

2. Move the downloaded tarball into your project folder, but outside of the `Assets` folder.

3. From the Unity Editor, open the Package Manager via `Window -> Package Manager`.

      ![unity tools package manager](/docs/images/unity_tools_package_manager.gif)

4. Click the `+` button in the top left of the window.

    ![Unity Add Tarball Package](/docs/images/unity_package_tarball.gif)

5. Select `Add package from tarball`.
6. Navigate to the directory containing the tarball, select and `Open` the tarball.
7. After the package has finished installing, [import the samples](#samples).
8. Finally, <a href="#configuring-the-plugin">configure the plugin</a>.

> [!NOTE]
> The Unity doc for adding a tarball can be found [here](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-tarball.html).

### Adding the package from a git URL

1. Install [git](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-git.html#req) and [git-lfs](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-git.html#req).
2.  From the Unity Editor, open the Package Manager. `Window -> Package Manager`.

    ![unity tools package manager](/docs/images/unity_tools_package_manager.gif)

3. Click the `+` button in the top left of the window.

    ![Unity Add Git Package](/docs/images/unity_package_git.gif)

4. Select `Add Package from Git URL`.
6. Paste in `git@github.com:PlayEveryWare/eos_plugin_for_unity_upm.git`.
7. After the package has finished installing, <a href="#samples">import the samples</a>.
8. Finally, <a href="#configuring-the-plugin">configure the plugin</a>.

> [!NOTE]
> The Unity doc for adding a git url can be found [here](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-giturl.html).

## Configuring the Plugin

To function, the plugin needs some information from your EOS project. Epic Docs on how to set up your project can be found [here](https://dev.epicgames.com/docs/epic-account-services/getting-started?sessionInvalidated=true).

1) Open your Unity project with the integrated EOS Unity Plugin. 
2) In the Unity editor, Open ```Tools -> EOS Plugin -> Dev Portal Configuration```.

    ![EOS Config Menu](/docs/images/dev-portal-configuration-editor-menu.png)

    ![EOS Config UI](/docs/images/eosconfig_ui.gif)

3) From the [Developer Portal](https://dev.epicgames.com/portal/), copy the configuration values listed below, and paste them into the similarly named fields in the editor tool window pictured above:

     > [!NOTE]
     > Addtional information about configuration settings can be found [here](https://dev.epicgames.com/docs/game-services/eos-platform-interface#creating-the-platform-interface).

    * ProductName
    * ProductVersion
    * [ProductID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ProductId)
    * [SandboxID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=SandboxId)
    * [DeploymentID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=DeploymentId)
    * [ClientSecret](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ClientSecret)
    * [ClientID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ClientId)
    * EncryptionKey

    <br />

    > [!NOTE]
    > Click the "Generate" button to create a random key, if you haven't already configured an encryption key in the EOS portal. You can then add the generated key to the [Developer Portal](https://dev.epicgames.com/portal/).
    > The Encryption Key is Used for Player Data Storage and Title Storage, if you do not plan to use these features in your project or the samples (and don't want to create an Encryption Key) then the field can be left blank.

4) Click `Save All Changes`.

5) Navigate to `Packages/Epic Online Services for Unity/Runtime` via the `Project` window.

6) Add the `EOSManager.prefab`, to each of your game's scenes.

7) Attach `EOSManager.cs (Script)` to a Unity object, and it will initialize the plugin with the specified configuration in `OnAwake()`.

> [!NOTE]
> The included <a href="#samples">samples</a> already have configuration values set for you to experiment with!

If you would like to see specific examples of various EOS features in action, import the sample Unity scenes that are described below.

# Samples

<img src="docs/images/sample_screen_lobby.gif" alt="Lobby Screenshot" width="48%"/> <img src="docs/images/sample_screen_achievements.gif" alt="Achievements Storage Screenshot" width="48%"/>

The included samples show examples of fully functional <a href="#supported-eos-sdk-features">feature implementation</a> to validate client and dev portal configuration as well as help with EOS integration into your own project. The samples are a collection of scenes that are imported from the UPM package, and include a series of scripts that function as generalized managers for each supported EOS SDK feature and platform.

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
> The plugin must be <a href="#configuring-the-plugin">configured</a> for samples to be functional. Some Samples may not be accessible if the extra packs were not <a href="#importing-samples">imported</a>.

Sample walkthroughs can be found [here](/docs/Walkthrough.md).

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
  > Check the [Prerequisites](#prerequisites) as there may be specific requirements for a player's computer.
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

# Support / Contact

PlayEveryWare EOS Plugin for Unity API Documentation can be found at [here](https://eospluginforunity.playeveryware.com).

For issues related to integration or usage of the EOS Unity plugin, please create a `New Issue` under the [Issues](https://github.com/PlayEveryWare/eos_plugin_for_unity/issues) tab.

For issues related to Epic Online Services SDK, Epic Dev Portal or for general EOS SDK information, please go to [Epic Online Services Community Support](https://eoshelp.epicgames.com/).

Detailed descriptions and usage for EOS SDK Interfaces can be found at [here](https://dev.epicgames.com/docs/services/en-US/GameServices/index.html).

# Contributor Notes

The following are guidelines for making contributions to this open-source project, as well as guidance on setting up your development environment to do so.

## Environment Setup

### Linux

The following two guides can help you set up your development environment on Windows using Hyper-V. If you are not using Hyper-V, the second guide can still be used to configure your environment.

  #### [Hyper-V Linux Guest VM](/docs/dev_env/HyperV_Linux_Guest_VM.md)
  #### [Configuring Ubuntu 18.04](/docs/dev_env/Ubuntu_Development_Environment.md)

### Windows

To setup your environment on windows, follow these steps (or you can run the script indicated at the end of this section):

1. Install the following:
    - [git](https://git-scm.com/downloads)
    - [Unity Hub](https://unity.com/download)
    - [Visual Studio 2019 Community Edition](https://visualstudio.microsoft.com/vs/older-downloads/)

2. Clone this repository and be sure to also run `git lfs pull` from the root of the repository.

3. Sign in to Unity Hub, and locate a project on disk by navigating to your local copy of the repository.

4. After adding the plugin project to Unity Hub, you will see a little caution sign next to the project if you do not currently have the proper version of the Unity Editor installed. This is expected. Click on the caution symbol and follow the prompts to install the appropriate version of the Unity Editor.

> [!NOTE]
> You can execute the following PowerShell command in an elevated window to run the setup script which should do everything for you:
> ```powershell
> cd [root of repository]
> Set-ExecutionPolicy RemoteSigned -Force
> .\tools\scripts\setup-windows.ps1
> ```

### macOS

See [here](/docs/macOS/README_macOS.md) to read our guide on setting up your environment on macOS.

You can run [this](/tools/scripts/setup-macos.sh) script `tools/scripts/setup-macos.sh` from a terminal to accomplish most of the setup steps, or read the aforementioned guide for details.

## Building Native Libraries

 Build the Visual Studio solutions for the native DLLs (extra platform specific instructions may be located in the docs for that platform).

1. In your local repository, navigate to the `lib/NativeCode/DynamicLibraryLoaderHelper_[PLATFORM]` folder of your platform choice in [NativeCode](/lib/NativeCode).

   > [!WARNING]
   > These files are not included with the package imported via tarball or git url.

2. Open and build the `DynamicLibraryLoaderHelper.sln` in Visual Studio.

A successful build will place the correct binaries in the proper locations for Unity to initialize the EOS SDK.

## Coding Standards

See [standards.md](/docs/standards.md).

## Core Classes

Descriptions for some of the core classes can be found [here](/docs/class_description.md).

# FAQ

To disable the plugin for specific platforms, see [this](/docs/disable_plugin_per_platform.md) (which also explains why you might want to do this).

See [here](/docs/command_line_export.md) for a guide on how to export the plugin from the command line. 

For more FAQs see [here](/docs/frequently_asked_questions.md).

If you have any outstanding questions, please bring them up in the [Discussions](https://github.com/PlayEveryWare/eos_plugin_for_unity/discussions) tab.
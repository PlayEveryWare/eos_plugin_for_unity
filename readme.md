# Epic Online Services Plugin for Unity

## Overview
The [eos_plugin_for_unity repository](https://github.com/PlayEveryWare/eos_plugin_for_unity) contains the source code for development and support of the [Epic Online Services Plugin for Unity (UPM Package)](https://github.com/PlayEveryWare/eos_plugin_for_unity_upm) package.

Things this plugin provides:

* Unity GUI for configuring EOS settings and saving to a JSON file
* Unity editor playback support, handled by reloading EOS SDK
* Feature specific manager classes for most common use-cases of EOS SDK API
* Feature specific samples as Unity scenes
* Social Overlay support
* Targets [EOS SDK 1.15] (https://dev.epicgames.com/docs/services/en-US/WhatsNew/index.html#1.15-16june,2022) *(bundled with plugin)*
* Targets [Unity 2020.1](https://unity.com/releases/2020-1)

This repo contains:
* A Unity Project for development of feature managers and samples
* Native Source for <code>DynamicLibraryLoaderHelper</code>
* Native Source for <code>GfxPluginNativeRender</code>
* Tool to build a Unity Package Manager compatible UPM

## Supported Platforms

The follow target platforms are supported in Unity for the current release of the plugin.

| Unity Target Platform | Current Plugin Release |
| - | - |
| Unity Editor | Supported (No Social Overlay) |
| Windows Standalone x64 | Supported |
| Windows Standalone x86 | Supported |
| Universal Windows Platform x64 | Supported |
| Android | Future |
| iOS | Future |
| Linux | [Supported](docs/linux/linux_supported_versions.md) |
| MacOS | Future |
| Console Platforms | Future |
| WebGL | Not Supported |
| Universal Windows Platform x86 | Not Supported |
| Unity Web Player | Not Supported |

## Supported EOS SDK Features

The EOS SDK is continually releasing new features and functionality.  The following is a list of EOS features and their support level for the Unity plugin.

| EOS Feature | Included in Sample |
| - | - |
| [Achievements](https://dev.epicgames.com/docs/services/en-US/GameServices/Achievements/index.html) | Achievements Sample |
| [Authentication](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/AuthInterface/index.html) | - All Samples - |
| [Ecommerce](https://dev.epicgames.com/docs/services/en-US/EpicGamesStore/TechFeaturesConfig/Ecom/index.html) | Store Sample |
| [Friends](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/Friends/index.html) | Auth & Friends Sample |
| [Leaderboards](https://dev.epicgames.com/docs/services/en-US/GameServices/Leaderboards/index.html) | Leaderboards Sample |
| [Lobby](https://dev.epicgames.com/docs/services/en-US/GameServices/Lobbies/index.html) | Lobbies Sample |
| [Lobby with Voice](https://dev.epicgames.com/docs/services/en-US/GameServices/Voice/index.html#voicewithlobbies) | Lobbies Sample |
| [NAT P2P](https://dev.epicgames.com/docs/services/en-US/GameServices/P2P/index.html) | P2P Sample |
| [Player Data Storage](https://dev.epicgames.com/docs/services/en-US/GameServices/PlayerDataStorage/index.html) | Player Data Storage Sample |
| [Presence](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/Presence/index.html) | Auth & Friends Sample |
| [Sessions](https://dev.epicgames.com/docs/services/en-US/GameServices/Sessions/index.html) | Sessions Sample |
| [Social Overlay](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/SocialOverlayOverview/index.html) | - All Samples - |
| [Stats](https://dev.epicgames.com/docs/services/en-US/GameServices/Stats/index.html) | Leaderboards Sample |
| [Title Storage](https://dev.epicgames.com/docs/services/en-US/GameServices/TitleStorage/index.html) | Title Storage Sample |
| [Reports](https://dev.epicgames.com/docs/services/en-US/GameServices/ReportsInterface/index.html) | Player Reports & Sanctions Sample |
| [Sanctions](https://dev.epicgames.com/docs/services/en-US/GameServices/SanctionsInterface/index.html) | Player Reports & Sanctions Sample |
| [Anti-Cheat](https://dev.epicgames.com/docs/services/en-US/GameServices/AntiCheat/index.html) | Not Supported |
| [EOS Mod SDK](https://dev.epicgames.com/docs/services/en-US/EpicGamesStore/TechFeaturesConfig/Mods/index.html) | Not Supported |
| [Voice Trusted Server](https://dev.epicgames.com/docs/services/en-US/GameServices/Voice/index.html#voicewithatrustedserverapplication) | Not Supported |


---
# Integration Notes
For best results, Unity 2020 is preferred. 

## Installing from a git URL
Ensure you have property setup Unity for [Git Dependency](https://docs.unity3d.com/Manual/upm-git.html).
1. Make sure you have git and git-lfs installed
2. Open the Unity Editor
3. Open the Package Manager.
    * It's listed under ```Window -> Package Manager```
4. Click the ```+``` button
5. Select '```Add Package from Git URL```'

    ![Unity Add Git Package](docs/images/unity_package_git.gif)

6. Paste in ```git@github.com:PlayEveryWare/eos_plugin_for_unity_upm.git```
   or ```https://github.com/PlayEveryWare/eos_plugin_for_unity_upm.git```


## Installing from a tarball
Download the latest release tarball from https://github.com/PlayEveryWare/eos_plugin_for_unity/releases
1. From the Unity Editor, open the Package Manager
    * It's listed under ```Window -> Package Manager```
2. Click the ```+``` button
3. Select '```Add package from tarball```'

    ![Unity Add Tarball Package](docs/images/unity_package_tarball.gif)

4. Go to directory containing the PEW Unity plugin tarball, and select it
5. Click ```Open```

---
# Configuring the Plugin

To get the EOS working, the plugin needs to know some specific things about your EOS project.

## Prerequisites
* One has a Unity project one would like to have the plugin integrated into
* One has a Epic Games Account, which you may sign up for [here](https://dev.epicgames.com/portal/)
* One has accepted the Terms for the [Epic Online Services](https://www.epicgames.com/site/en-US/tos?lang=en-US)
* One has configured a product on the [Epic Games Developer Portal](https://dev.epicgames.com/portal/)

## Steps
1) Open your Unity project with integrated plugin 
2) In the Unity editor, Open ```Tools -> EpicOnlineServicesConfigEditor```

    ![EOS Config Menu](docs/images/unity_tools_eosconfig.gif)

3) Configure the EOS plugin

    ![EOS Config UI](docs/images/eosconfig_ui.gif)

4) From the [developer portal](https://dev.epicgames.com/portal/), copy the configuration values listed below, and paste them into the similarly named fields in the editor tool:
    * ProductName
    * ProductVersion
    * [ProductID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ProductId)
    * [SandboxID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=SandboxId)
    * [DeploymentID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=DeploymentId)
    * [ClientSecret](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ClientSecret)
    * [ClientID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ClientId)
    * EncryptionKey
   
At this point, you are ready to start developing using the Epic Online Services Plugin for Unity!  Simply attach <code>EOSManager.cs (Script)</code> to a Unity object and it will intialize the plugin with the specified configuration in <code>OnAwake()</code>.

If you would like to see examples of each feature, continue installing samples below.

---
# Samples

The included samples show fully functional feature implemenation that will both help with EOS integration as well as validate client to dev portal configuration.  After installing the samples from the UPM package, you will find scenes for each major feature.

<img src="docs/images/sample_screen_lobby.gif" alt="Lobby Screenshot" width="48%"/>
<img src="docs/images/sample_screen_titlestorage.gif" alt="Title Storage Screenshot" width="48%"/>

In addition, the samples include Unity friendly *feature* Managers that can help to quickly integrate new EOS features into your title.  They provide functional usage of the main feature functionality and can be a good base template.

## Installing the samples
To use the samples, install them from the UPM manager.

![Unity Install Samples](docs/images/unity_install_samples.gif)

The samples include both usage of the EOS SDK, and convience wrappers to make using the SDK more ergonomic in the Unity Game Engine. By being samples in the UPM sense, they are placed under Assets which allows modification.

## Running the samples
* Launch Unity project with the samples installed

* In the Unity editor, hit ```Play```

* Login with a selected authentication type

    ![Auth and Friends Screenshot](docs/images/sample_screen_auth_friends.gif)

---
# Authentication


## Running and Configuring the EOS SDK Dev Auth Tool
* Launch the [Developer Authentication Tool](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/DeveloperAuthenticationTool/index.html)

    ![Dev Auth Tool](docs/images/dev_auth_tool.gif)

* Pick a port to use on the computer. 8888 is a good quick to type number that isn't usually used by a process
* Log in with one's user credentials that are registered with Epic
* Pick a username. This username will be used in the sample to log in

More specific and up-to-date instructions can also be found on Epic's [website](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/DeveloperAuthenticationTool/index.html)


---
# Plugin Support

EOS Plugin for Unity API Documentation can be found at https://eospluginforunity.playeveryware.com

For issues related to integration or usage of the Unity plugin, please create a ```New Issue``` under the [Issues](https://github.com/PlayEveryWare/eos_plugin_for_unity/issues) tab.

For issues related to Epic Online Services SDK, Epic Dev Portal or general EOS SDK information, please go to [Epic Online Services Community Support](https://eoshelp.epicgames.com/)

Detailed descriptions and usage for EOS SDK Interfaces, can be found at [EOS Developer Documentation: Game Services](https://dev.epicgames.com/docs/services/en-US/GameServices/index.html).


---
# Source Code Contributor Notes

The following are guidlines for helping contribute to this open source project.

## Prerequisites
* Ensure At least Visual Studio 2017 is installed.
* Ensure At least Unity 2020.1.11f1 is installed
* Ensure required Platform SDKs are installed (Windows, Linux, macOS, Android, iOS, Consoles)

## Build steps For Native Libraries
* Build the Visual Studio solutions for the native DLLs
    * Build the <code>DynamicLibraryLoaderHelper</code> sln in DynamicLibraryLoaderHelper/ for all platforms
    * (Currently needed) Build the UnityEditorSharedDictionary sln in UnityEditorSharedDictionary/ for all platforms
        * Only the NativeSharedDictionary project is needed.  

A successful build will mean the correct binaries have been placed in the proper locations for Unity to successfully initialize EOS SDK.

## Standards
See [standards.md](docs/standards.md)

## Class description
See [docs/class_description.md](docs/class_description.md)

## Additional Documentation
Additional documentation can be found in the [docs/ directory](docs/).

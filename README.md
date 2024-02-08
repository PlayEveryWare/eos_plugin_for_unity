<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>

# Overview

The PlayEveryWare EOS Plugin for Unity brings the free services from Epic that connect players across all platforms and all stores to Unity in an easy-to-use package. Find more information on EOS [here](https://dev.epicgames.com/en-US/services) and read the Epic docs on the services [here](https://dev.epicgames.com/docs/epic-online-services).

This repository contains the source code for development and serves as a destination for support for the [PlayEveryWare EOS Plugin for Unity (UPM Package)](https://github.com/PlayEveryWare/eos_plugin_for_unity_upm).

Out of the box, this project demonstrates (through a collection of sample scenes) each feature of the Epic Online Services SDK[^1]. The sample scenes (coupled with accompanying documentation) can be used to get an idea of how you can implement all the online features you want in your game!

See [this](/docs/plugin_advantages.md) for a more complete overview of the advantages of using EOS with Unity.

[^1]: See [here](#supported-eos-sdk-features) for which SDK features specifically are demonstrated.

> [!NOTE]
> If you are **not** interested in the _development_ of the EOS Plugin project (and instead just want to get to using it) you can follow our guide on [Importing the Plugin Package](#importing-the-plugin-package) to start using the most recently released version of the EOS Plugin.

# Getting Started

## Prerequisites

* [Create an Epic Games Account](https://www.epicgames.com/id/register) (_although, most [features](#exploring-supported-eos-features) do not require your users to have an Epic Games Account, you must have one to configure your game with Epic Games Developer Portal_).
* A product configured on the [Epic Games Developer Portal](https://dev.epicgames.com/portal/).
* A Unity project to integrate the plugin into.

> [!NOTE]
> Your system should also satisfy [Unity's system requirements](https://docs.unity3d.com/2021.3/Documentation/Manual/system-requirements.html) as well as the [EOS system requirements](https://dev.epicgames.com/docs/epic-online-services/eos-get-started/system-requirements)

# Supported EOS SDK Features

Below is a table summarizing the level of support the EOS Plugin for Unity provides. Most features are demonstrated via sample scenes provided in the project, and links to the guide for each corresponding sample scene are listed below. In some cases (such as Anti-Cheat) the feature is not very well demonstrated with a scene. In those cases, a link to information about how the plugin utilizes the feature is provided. In some cases (such as logging and overlay) the features are not implemented in any one scene specifically, but in all of them.

Use the "Select Demo Scene" dropdown in the application to select the sample scene that corresponds with the walkthrough. 

There are many EOS features that do not require your player to have an Epic Games Account (EGA) - such features are also marked accordingly in the following table.

| Feature | Status | Sample Scene Walkthrough | Requires EGA |
| :-- | :-: | :-- | :-: |
|[Achievements](https://dev.epicgames.com/docs/game-services/achievements)                 | ✅ | ["Achievements"](/docs/scene_walkthrough/achievements_walkthrough.md)                                                  |   |
|[Anti-Cheat](https://dev.epicgames.com/docs/game-services/anti-cheat)                     | ✅ | ["Information"](/docs/easy_anticheat_configuration.md)                                                                             |  |
|[Authentication](https://dev.epicgames.com/docs/epic-account-services/auth-interface)     | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md), [Information](/docs/player_authentication.md) | ✔️ |
|[Custom Invites](https://dev.epicgames.com/docs/game-services/custom-invites-interface)   | ✅ | ["Custom Invites"](/docs/scene_walkthrough/customInvites_walkthrough.md)                                               |  |
|[Connect Interface](https://dev.epicgames.com/docs/game-services/eos-connect-interface)   | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md)                                                               |   |
|[Ecommerce](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/ecom)    | ✅ | ["Store"](/docs/scene_walkthrough/store_walkthrough.md), [Information](/docs/ecom.md)                                        | ✔️ |
|[Friends](https://dev.epicgames.com/docs/epic-account-services/eos-friends-interface)     | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md)                                                               | ✔️ |
|[Leaderboards](https://dev.epicgames.com/docs/game-services/leaderboards)                 | ✅ | ["Leaderboards"](/docs/scene_walkthrough/leaderboards_walkthrough.md)                                                               |   |
|[Lobby](https://dev.epicgames.com/docs/game-services/lobbies)                             | ✅ | ["Lobbies"](/docs/scene_walkthrough/lobbies_walkthrough.md)                                                                    |  |
|[Lobby with Voice](https://dev.epicgames.com/docs/game-services/voice#voicewithlobbies)   | ✅ | ["Lobbies"](/docs/scene_walkthrough/lobbies_walkthrough.md), [Information](/docs/enabling_voice.md)                            |  |
|[Logging Interface](https://dev.epicgames.com/docs/game-services/eos-logging-interface)   | ✅ | NA                                                                                                                               |  |
|[Metrics](https://dev.epicgames.com/docs/game-services/eos-metrics-interface)             | ✅ | ["Metrics"](/docs/scene_walkthrough/metrics_walkthrough.md)                                                                    |  |
|[Mod SDK](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/mods)      | ❌ | NA                                                                                                                               | ✔️ |
|[NAT P2P](https://dev.epicgames.com/docs/game-services/p-2-p)                                               | ✅ | ["Peer 2 Peer"](/docs/scene_walkthrough/P2P_walkthrough.md), ["P2P Netcode"](/docs/scene_walkthrough/P2P_netcode_walkthrough.md) |  |
|[Platform Interface](https://dev.epicgames.com/docs/game-services/eos-platform-interface)                   | ✅ | NA |   |
|[Player Data Storage](https://dev.epicgames.com/docs/game-services/player-data-storage)                     | ✅ | ["Player Data Storage"](/docs/scene_walkthrough/player_data_storage_walkthrough.md)                                                        |   |
|[Presence](https://dev.epicgames.com/docs/epic-account-services/eos-presence-interface)                     | ✅ | ["Auth & Friends"](/docs/scene_walkthrough/auth&friends_walkthrough.md)                                                               | ✔️ |
|[Progression Snapshot Interface](https://dev.epicgames.com/docs/epic-account-services/progression-snapshot) | ❌ | NA                                                                                                             | ✔️ |
|[Reports](https://dev.epicgames.com/docs/game-services/reports-interface)                 | ✅ | ["Player Reports & Sanctions"](/docs/scene_walkthrough/player_reports_and_sanctions_walkthrough.md)                                               |  |
|[Sanctions](https://dev.epicgames.com/docs/game-services/sanctions-interface)             | ✅ | ["Player Reports & Sanctions"](/docs/scene_walkthrough/player_reports_and_sanctions_walkthrough.md)                                               |  |
|[Sessions](https://dev.epicgames.com/docs/game-services/sessions)                         | ✅ | ["Sessions & Matchmaking"](/docs/scene_walkthrough/sessions_and_matchmaking_walkthrough.md)                                                   |  |
|[Social Overlay](https://dev.epicgames.com/docs/epic-account-services/social-overlay-overview) / [UI Interface](https://dev.epicgames.com/docs/epic-account-services/eosui-interface) | ✅ | [Information](/docs/overlay.md)        | ✔️ |
|[Stats](https://dev.epicgames.com/docs/game-services/eos-stats-interface)                 | ✅ | ["Leaderboards"](/docs/scene_walkthrough/leaderboards_walkthrough.md)                                                               |  |
|[Title Storage](https://dev.epicgames.com/docs/game-services/title-storage)               | ✅ | ["Title Storage"](/docs/scene_walkthrough/title_storage_walkthrough.md)                                                              |  |
|[User Info Interface](https://dev.epicgames.com/docs/epic-account-services/eos-user-info-interface) | ✅ | NA                                                                                                                     | ✔️ |
|[Voice Trusted Server](https://dev.epicgames.com/docs/game-services/voice#voicewithatrustedserverapplication) | ❌ | NA                                                                                                           |  |

Efforts will be made to add corresponding support to features as they are added to the Epic Online Services SDK. The table above reflects the features as of November 2023.

## Supported Platforms

We currently support the following platforms, details of each can be found on our [Supported Platforms](/docs/supported_platforms.md) document:

* Windows
* Linux
* macOS
* Android
* iOS
* Nintendo Switch
* Xbox One
* Xbox Series X
* PlayStation 4
* PlayStation 5

## Importing the Plugin Package

There are two options to install the package:
* Via a [UPM tarball](/docs/add_plugin.md#adding-the-package-from-a-tarball) _(easiest to get started quickly)_.
* From a [git url](/docs/add_plugin.md#adding-the-package-from-a-git-url) _(this method has the possible advantage of keeping the plugin up-to-date, if that's something that you would prefer)_.

Once imported into your project, be sure to [Configure the Plugin](/docs/configure_plugin.md) to work with your game.

## Exploring Supported EOS Features

### [Supported Epic Online Services Features](/docs/eos_features.md)
### [How to import sample scenes into your project](/docs/samples.md)

# Support / Contact

PlayEveryWare EOS Plugin for Unity API Documentation can be found at [here](https://eospluginforunity.playeveryware.com).

For issues related to integration or usage of the EOS Unity plugin, please create a `New Issue` under the [Issues](https://github.com/PlayEveryWare/eos_plugin_for_unity/issues) tab.

For issues related to Epic Online Services SDK, Epic Dev Portal or for general EOS SDK information, please go to [Epic Online Services Community Support](https://eoshelp.epicgames.com/).

Detailed descriptions and usage for EOS SDK Interfaces can be found at [here](https://dev.epicgames.com/docs/services/en-US/GameServices/index.html).

# Contributor Notes

This is an open source project! We welcome you to make contributions. See our [Contributions](/docs/contributions.md) document for more information.

# FAQ

To disable the plugin for specific platforms, see [this](/docs/disable_plugin_per_platform.md) (which also explains why you might want to do this).

See [here](/docs/command_line_export.md) for a guide on how to export the plugin from the command line. 

For issues of API Level compatibility, please read our [document](/docs/dotnet_quirks.md) on .NET Quirks and Unity compatibility.

For more FAQs see [here](/docs/frequently_asked_questions.md).

If you have any outstanding questions, please bring them up in the [Discussions](https://github.com/PlayEveryWare/eos_plugin_for_unity/discussions) tab.

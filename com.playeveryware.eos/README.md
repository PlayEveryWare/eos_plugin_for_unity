<div align="left">
  
<a href="">[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)</a>
<a href="">![Unity](https://img.shields.io/badge/Unity-2021.3.16f1-blue)</a>
<img src="/tools/coverage/results/Report/badge_linecoverage.svg" />

</div>

# Overview

The EOS Plugin for Unity brings the free services from Epic that connect players across all platforms and all stores to Unity in an easy-to-use package. Find more information on what services Epic Online Services encompasses at [https://dev.epicgames.com/en-US/services](https://dev.epicgames.com/en-US/services) and read the developer documentation at [https://dev.epicgames.com/docs/epic-online-services](https://dev.epicgames.com/docs/epic-online-services).

This repository contains the source code for development of the [EOS Plugin for Unity (UPM Package)](https://github.com/EOS-Contrib/eos_plugin_for_unity_upm).

Out of the box, this project demonstrates (through a collection of sample scenes) each feature of the Epic Online Services SDK[^1]. The sample scenes (coupled with accompanying documentation) can be used to get an idea of how you can implement the online features you want in your game.

See [this](/com.playeveryware.eos/Documentation~/plugin_advantages.md) for a more complete overview of the advantages of using EOS with Unity.

[^1]: See the [supported-eos-sdk-features](#supported-eos-sdk-features) section for which SDK features specifically are demonstrated.

> [!NOTE]
> If you are **not** interested in the _development_ of the EOS Plugin for Unity project (and instead just want to get to using it) you can follow our guide on [Importing the Plugin Package](#importing-the-plugin-package) to start using the most recently released version of the EOS Plugin for Unity.

# Getting Started

## Prerequisites

* [An Epic Games account](https://www.epicgames.com/id/register) (_although, most [features](#exploring-supported-eos-features) do not require your users to have an Epic Games account, you must have one to configure your game in the Epic Games Developer Portal_).
* A product configured on the [Epic Games Developer Portal](https://dev.epicgames.com/portal/) (see [Full Guide to Implementing the EOS Plugin for Unity](/com.playeveryware.eos/Documentation~/getting_started_guide/README.md) for steps on configuring a product). 
* A Unity project to integrate the plugin into.

> [!NOTE]
> Your system should also satisfy [Unity's system requirements](https://docs.unity3d.com/Manual/system-requirements.html) as well as the [EOS system requirements](https://dev.epicgames.com/docs/epic-online-services/eos-get-started/system-requirements)

> [!IMPORTANT]
> **Windows:** the EOS SDK requires the Microsoft Visual C++ runtime (`MSVCP140.dll`, `VCRUNTIME140.dll` and `VCRUNTIME140_1.dll`) on your players' machines, and the plugin does not include it. A plugin update that brings in a newer EOS SDK can require a newer runtime than your players have installed, and the game will fail to launch for them. Ship the runtime with your game as described in the [Epic Games Store prerequisites guide](https://dev.epicgames.com/docs/epic-games-store/services/game-prereqs-installer), and refresh it from the [latest Visual C++ Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) whenever you update the plugin.

# Supported EOS SDK Features

Below is a table summarizing the features the EOS Plugin for Unity supports. Most features are demonstrated via sample scenes provided in the project, and links to the guide for each corresponding sample scene are listed below. In some cases (such as Anti-Cheat) the feature is not able to be demonstrated well with a scene. In those cases, a link to information about how the plugin utilizes the feature is provided. In some cases (such as logging and overlay) the features are not implemented in any one scene specifically, but in all of them.

Use the "Select Demo Scene" dropdown in the application to select the sample scene that corresponds with the walkthrough. 

There are many EOS features that do not require your player to have an Epic Games account - such features are also marked accordingly in the following table.

| Feature | Status | Sample Scene Walkthrough | Requires Epic Games account |
| :-- | :-: | :-- | :-: |
|[Achievements](https://dev.epicgames.com/docs/game-services/achievements)                 | ✅ | ["Achievements"](/com.playeveryware.eos/Documentation~/scene_walkthrough/achievements_walkthrough.md)                                                  |   |
|[Anti-Cheat](https://dev.epicgames.com/docs/game-services/anti-cheat)                     | ✅ | ["Information"](/com.playeveryware.eos/Documentation~/easy_anticheat_configuration.md)                                                                             |  |
|[Authentication](https://dev.epicgames.com/docs/epic-account-services/auth-interface)     | ✅ | ["Auth & Friends"](/com.playeveryware.eos/Documentation~/scene_walkthrough/auth&friends_walkthrough.md), [Information](/com.playeveryware.eos/Documentation~/player_authentication.md) | ✔️ |
|[Custom Invites](https://dev.epicgames.com/docs/game-services/custom-invites-interface)   | ✅ | ["Custom Invites"](/com.playeveryware.eos/Documentation~/scene_walkthrough/customInvites_walkthrough.md)                                               |  |
|[Connect Interface](https://dev.epicgames.com/docs/game-services/eos-connect-interface)   | ✅ | ["Auth & Friends"](/com.playeveryware.eos/Documentation~/scene_walkthrough/auth&friends_walkthrough.md)                                                               |   |
|[Ecommerce](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/ecom)[^2]    | ✅ | ["Store"](/com.playeveryware.eos/Documentation~/scene_walkthrough/store_walkthrough.md), [Information](/com.playeveryware.eos/Documentation~/ecom.md)                                        | ✔️ |
|[Friends](https://dev.epicgames.com/docs/epic-account-services/eos-friends-interface)     | ✅ | ["Auth & Friends"](/com.playeveryware.eos/Documentation~/scene_walkthrough/auth&friends_walkthrough.md)                                                               | ✔️ |
|[Leaderboards](https://dev.epicgames.com/docs/game-services/leaderboards)                 | ✅ | ["Leaderboards"](/com.playeveryware.eos/Documentation~/scene_walkthrough/leaderboards_walkthrough.md)                                                               |   |
|[Lobbies](https://dev.epicgames.com/docs/game-services/lobbies)                             | ✅ | ["Lobbies"](/com.playeveryware.eos/Documentation~/scene_walkthrough/lobbies_walkthrough.md)                                                                    |  |
|[Logging Interface](https://dev.epicgames.com/docs/game-services/eos-logging-interface)   | ✅ | NA                                                                                                                               |  |
|[Metrics](https://dev.epicgames.com/docs/game-services/eos-metrics-interface)             | ✅ | ["Metrics"](/com.playeveryware.eos/Documentation~/scene_walkthrough/metrics_walkthrough.md)                                                                    |  |
|[Epic Games Store Mods](https://dev.epicgames.com/docs/epic-games-store/tech-features-config/mods)      | ❌ | NA                                                                                                                               | ✔️ |
|[NAT P2P](https://dev.epicgames.com/docs/game-services/p-2-p)                                               | ✅ | ["Peer 2 Peer"](/com.playeveryware.eos/Documentation~/scene_walkthrough/P2P_walkthrough.md), ["P2P Netcode"](/com.playeveryware.eos/Documentation~/scene_walkthrough/P2P_netcode_walkthrough.md) |  |
|[Platform Interface](https://dev.epicgames.com/docs/game-services/eos-platform-interface)                   | ✅ | NA |   |
|[Player Data Storage](https://dev.epicgames.com/docs/game-services/player-data-storage)                     | ✅ | ["Player Data Storage"](/com.playeveryware.eos/Documentation~/scene_walkthrough/player_data_storage_walkthrough.md)                                                        |   |
|[Presence](https://dev.epicgames.com/docs/epic-account-services/eos-presence-interface)                     | ✅ | ["Auth & Friends"](/com.playeveryware.eos/Documentation~/scene_walkthrough/auth&friends_walkthrough.md)                                                               | ✔️ |
|[Progression Snapshot Interface](https://dev.epicgames.com/docs/epic-account-services/progression-snapshot) | ❌ | NA                                                                                                             | ✔️ |
|[Reports](https://dev.epicgames.com/docs/game-services/reports-interface)                 | ✅ | ["Player Reports & Sanctions"](/com.playeveryware.eos/Documentation~/scene_walkthrough/player_reports_and_sanctions_walkthrough.md)                                               |  |
|[Sanctions](https://dev.epicgames.com/docs/game-services/sanctions-interface)             | ✅ | ["Player Reports & Sanctions"](/com.playeveryware.eos/Documentation~/scene_walkthrough/player_reports_and_sanctions_walkthrough.md)                                               |  |
|[Sessions](https://dev.epicgames.com/docs/game-services/sessions)                         | ✅ | ["Sessions & Matchmaking"](/com.playeveryware.eos/Documentation~/scene_walkthrough/sessions_and_matchmaking_walkthrough.md)                                                   |  |
|[Social Overlay](https://dev.epicgames.com/docs/epic-account-services/social-overlay-overview)[^2] / [UI Interface](https://dev.epicgames.com/docs/epic-account-services/eosui-interface) | ✅ | [Information](/com.playeveryware.eos/Documentation~/overlay.md)        | ✔️ |
|[Stats](https://dev.epicgames.com/docs/game-services/eos-stats-interface)                 | ✅ | ["Leaderboards"](/com.playeveryware.eos/Documentation~/scene_walkthrough/leaderboards_walkthrough.md)                                                               |  |
|[Title Storage](https://dev.epicgames.com/docs/game-services/title-storage)               | ✅ | ["Title Storage"](/com.playeveryware.eos/Documentation~/scene_walkthrough/title_storage_walkthrough.md)                                                              |  |
|[User Info Interface](https://dev.epicgames.com/docs/epic-account-services/eos-user-info-interface) | ✅ | NA                                                                                                                     | ✔️ |
|[Voice with Lobbies](https://dev.epicgames.com/docs/game-services/voice#voicewithlobbies)   | ✅ | ["Lobbies"](/com.playeveryware.eos/Documentation~/scene_walkthrough/lobbies_walkthrough.md), [Information](/com.playeveryware.eos/Documentation~/enabling_voice.md)                            |  |
|[Voice Trusted Server](https://dev.epicgames.com/docs/game-services/voice#voicewithatrustedserverapplication) | ❌ | NA                                                                                                           |  |

Efforts will be made to add corresponding support to features as they are added to the Epic Online Services SDK. The table above reflects the features as of December 2025.

[^2]: As of EOS Unity Plugin 4.1.0, only the Ecom Overlay is supported on mobile. The Social Overlay isn't supported on mobile.

The mobile Epic Games Store is currently in closed beta. We’re excited to launch our self-publishing tools in the near future.
Interested in joining our mobile store? Submit your game through our [Leads Intake form](https://e.acct.epicgames.com/click?EZWdzdG9yZS10ZWFtQGVwaWNnYW1lcy5jb20/CeyJtaWQiOiIxNzYzNTcxOTU3NDUzMTk2NTk2OGRjNzFiIiwiY3QiOiJlcGljLXR4LXByb2QtODI0ODA2ZmU0NDUwYjQ4ZTQ3MDVhODM3Y2Q4MzAwNGItMSIsInJkIjoiZXBpY2dhbWVzLmNvbSJ9/VaHR0cHM6Ly9mb3Jtcy51bnJlYWxlbmdpbmUuY29tL3MvZWdzLWxlYWQtaW50YWtlLWZvcm0/SWkhfYWNjdF9OTlRBTjExMTkyMDI1YzE4NjM3NjJiMQ/LYWUx/qP2xhbmd1YWdlPWVuX1VT/gaR35mw/JMTExOTIwMjVDMTg2Mzc2MkIx/s1v93de8f7c)

## Supported Platforms

We currently support the following platforms, details of each can be found on our [Supported Platforms](/com.playeveryware.eos/Documentation~/supported_platforms.md) document:

* Windows
* Linux
* macOS
* Android
* iOS
* Nintendo Switch™
* Nintendo Switch 2™
* Xbox One
* Xbox Series X|S
* PlayStation®4
* PlayStation®5

## Importing the Plugin Package

There are two options to install the package:
* Via a [UPM tarball](/com.playeveryware.eos/Documentation~/add_plugin.md#adding-the-package-from-a-tarball) _(easiest to get started quickly)_.
* From a [git url](/com.playeveryware.eos/Documentation~/add_plugin.md#adding-the-package-from-a-git-url) _(this method has the possible advantage of keeping the plugin up-to-date, if that's something that you would prefer)_.

Once imported into your project, be sure to [Configure the Plugin](/com.playeveryware.eos/Documentation~/configure_plugin.md) to work with your game.

## Exploring Supported EOS Features

### [Supported Epic Online Services Features](/com.playeveryware.eos/Documentation~/eos_features.md)
### [How to import sample scenes into your project](/com.playeveryware.eos/Documentation~/samples.md)

# Support / Contact

EOS Plugin for Unity documentation can be found here on GitHub.

For questions or issues related to integration or usage of Epic Online Services, Epic Games Developer Portal, or the EOS Plugin for Unity, please create a discussion on the [EOS Developer Community](https://eoshelp.epicgames.com).

Detailed descriptions and usage for EOS SDK Interfaces can be found on [Epic Developer Resources Documentation](https://dev.epicgames.com/docs/).

For issues of a confidential nature (including support using this plugin on console platforms), please create a private discussion on the [EOS Developer Community](https://eoshelp.epicgames.com).

# Contributor Notes

This is an open source project! We welcome you to make contributions. See our [Contributions](/com.playeveryware.eos/Documentation~/contributions.md) document for more information.

# FAQ

To disable the plugin for specific platforms, see [this](/com.playeveryware.eos/Documentation~/disable_plugin_per_platform.md) (which also explains why you might want to do this).

See [our guide](/com.playeveryware.eos/Documentation~/command_line_export.md) on how to export the plugin from the command line. 

For issues of API Level compatibility, please read our [document](/com.playeveryware.eos/Documentation~/dotnet_quirks.md) on .NET Quirks and Unity compatibility.

For more FAQs see [Frequently Asked Questions](/com.playeveryware.eos/Documentation~/frequently_asked_questions.md).
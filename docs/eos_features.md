<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Readme" width="5%"/></a>

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

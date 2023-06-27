## **EOS Plugin for Unity Walkthrough**
 
This document is meant to serve as a guide to our EOS Plugin for Unity demo scenes. Each scene is meant to showcase the implementation of a specific feature such as peer 2 peer communication, lobbies, or custom invites.
Each scene has a few things in common: the login page, side bar, scene selection, and debug log.

## Sidebar
The sidebar shows the current version, logged in account, framerate, readme, log copy button and exit button.
 - The ``README`` button will display the readme for the current scene
 - The ``Copy Log`` button copies the log from the log window
 - The ``Exit``    button closes the application


## Debug Log
The Debug log provides a filterable on screen debug output
- The gear icon hides/shows the debug log buttons
    - The ``'Enter filter text...'`` field allows for text filters
    - The filter icon allows for filtering by specific levels for each type channel
    - The two arrows icon expands the log to fill the window
    - The eye icon hides/shows the log


## Scene selection
The yellow dropdown in the top of the window allows the user to select which demo scene they would like to log into

❗ There is a standard sample pack, and several extra packs in the EOS Unity Plugin. If a scene doesn't load, remember to import the wanted extra pack.


## Login Page
The login menu allows the user to select which login method they would like to use through the dropdown in the center of the window. After logging in the user will be put into the selected demo scene.
The login options are as follows:
- ``Dev Auth``: Uses Epic Games’ Developer Authentication tool, documentation can be found [here](https://dev.epicgames.com/docs/epic-account-services/developer-authentication-tool).
    1. Launch the [Developer Authentication Tool](https://dev.epicgames.com/docs/services/en-US/EpicAccountServices/DeveloperAuthenticationTool/index.html).

    ![Dev Auth Tool](images/dev_auth_tool.gif)

    2. Pick a port to use on the computer. 8888 is a good quick to type number that isn't usually used by a process.
    3. Log in with one's user credentials that are registered with Epic.
    4. Pick a username. This username will be used in the sample to log in.

- ``Account Portal``: Uses the Epic Games overlay and any of the connected account types.
    - An Epic Games Account
    - A Facebook account
    - A Xbox Live account
    - A PlayStation Network Account
    - A Nintendo Account
    - A Steam Account
    - An Apple ID
- ``Persistent Auth``: Uses Epic Game's Persisting User Login. More documentation on this can be found [here](https://dev.epicgames.com/docs/epic-account-services/auth/auth-interface#persisting-user-login-to-epic-account-outside-epic-games-launcher).
- ``External Auth``: Uses a web browser to login through an online account portal similar to the Account Portal option.
- ``Connect``: Uses the connect interface to login through one of the supported options. More documentation on this can be found [here](https://dev.epicgames.com/docs/game-services/eos-connect-interface?sessionInvalidated=true).
    - Device Access Token: Uses a persistent access token based on the device, unattached to an account.
    - Steam Session Ticket
    - Steam App Ticket
    - Discord Access Token  
    - Openid Access Token



![Login Type by Platform](images/login_type_by_platform.png)


## **Individual Scene Walkthroughs**
- [Achievements](SceneWalkthrough\AchievementsWalkthrough.md)
- [Auth & Friends](SceneWalkthrough\Auth&FriendsWalkthrough.md)
- [Custom Invites](SceneWalkthrough\CustomInvitesWalkthrough.md)
- [Leaderboards](SceneWalkthrough\LeaderboardsWalkthrough.md)
- [Lobbies](SceneWalkthrough\LobbiesWalkthrough.md)
- [Metrics](SceneWalkthrough\MetricsWalkthrough.md)
- [Peer 2 Peer](SceneWalkthrough\Peer2PeerWalkthrough.md)
- [Performance Stress Test](SceneWalkthrough\PerformanceStressTestWalkthrough.md)
- [Player Data Storage](SceneWalkthrough\PlayerDataStorageWalkthrough.md)
- [Player Reports and Sanctions](SceneWalkthrough\PlayerReportsAndSanctionsWalkthrough.md)
- [Sessions and Matchmaking](SceneWalkthrough\SessionsAndMatchmakingWaklthrough.md)
- [Store](SceneWalkthrough\StoreWalkthrough.md)
- [P2PNetcode](SceneWalkthrough\P2PNetcodeWalkthrough.md)

❗ More information on the EOS interfaces can be found [here](https://dev.epicgames.com/docs)
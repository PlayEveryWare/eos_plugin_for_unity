<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

## **Lobbies Demo**
This demo showcases the lobby and includes a voice chat interface in a combined demo. The demo functions as a waiting room style lobby where users can create a new lobby or join existing ones, once in a lobby users can communicate through the lobby voice chat.
- The topmost window is where users can create and manage their own lobby.
    - The ``Bucket ID`` Is the name or lobby ID used to find the lobby through search functions.
    - The ``Max Players`` Drowpdown selects the maximum number of user the lobby will be able to hold.
    - The ``Level`` dropdown selects the level the lobby will work on, this is also used for searching for lobbies through search functions.
    - The ``Permission`` dropdown selects how user are able to join the lobby.
        - ``Public`` Allows anyone to join.
        - ``Join via Presence`` Allows only friends to join (Presence is the term used for the entire friends/status interface).
        - ``Invite Only`` Allows only users who have been sent an invite from a player in the lobby to join.
    - ``Allow Invites`` determines whether or not players in the lobby can invite other players.
    - ``Presence Enabled`` determines whether or not to associate the lobby with the [presence](https://dev.epicgames.com/docs/epic-account-services/eos-presence-interface) system.
    - ``RTC Voice Room`` determines whether or not the lobby will have voice chat.
    - ``Anti-Cheat Enabled`` toggles Easy-Anti Cheat on and off. This will kick cheating players or leave the lobby if the owner is cheating.
    - ``Create New Lobby`` Creates and joins a new lobby with the selected settings (Note: With default settings, only a bucket id is needed to create a new lobby).
![SettingsWindow](../images/eos_sdk_lobbies_settings.png)

- Once a lobby is created it will display the ``Lobby ID`` and the count id of the owner. The lobby also lists the current user and information.
    - ``Member Name`` Displays each user's account name.
    - ``Is Talking`` Displays each user's  voice activity.
    - ``Mute`` Allows players to mute user in the lobby on a client side basis.
    - ``Kick`` Allows the owner to kick user from the lobby.
    - ``Promote`` Allows the owner to transfer ownership of the lobby to another user.
    - ``Press to Talk`` toggles between having push to talk on, and what key to use as the push to talk trigger.

    ![Roster](../images/eos_sdk_lobbies_roster.png)


- The right window is where users can search for lobbies through an id, level or bucket id.
    - ``LobbyId`` Searches by the lobby id as found at the top of the lobby.
    - ``Level`` Searches by level, as chosen by the host when creating the lobby.
    - ``BucketId`` Searches by Bucket ID, as chosen by the host when creating the lobby.
    - Invites sent from other players will also appear here, with the user who sent the invite's ID and ``Accept`` and ``Decline`` option

![Search](../images/eos_sdk_lobbies_search.png)

-The ``Friends`` tab on the upper right corner opens the friends list.
    - The ``Invite`` button next to each entry sends an invite to that player.


> [!NOTE]
> More documentation on the lobby interface can be found [here](https://dev.epicgames.com/docs/game-services/lobbies).
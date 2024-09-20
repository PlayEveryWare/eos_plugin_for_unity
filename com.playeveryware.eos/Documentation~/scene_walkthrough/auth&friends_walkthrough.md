<a href="/com.playeveryware.eos/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

## **Auth & Friends Demo**
This demo showcases an implementation of the friends list into a game UI. The panel on the right shows the user's friend list and status of each friend. This panel can be accessed in other scenes that require access to the friends list.
- The ``Refresh`` button refreshes the list, repolling for information.
- The ``Friends Overlay`` button opens the EOS overlay.
- The Friends tab, on the top left of the friends list, opens and closes the friends list. **In other samples it may start closed or may not be present where appropriate**

![Friends Tab](../images/eos_sdk_friends_panel.png)

> [!NOTE]
> See [Epic's documentation on the Auth interface](https://dev.epicgames.com/docs/epic-account-services/auth) as well as [Epic's documentation on the Friends interface](https://dev.epicgames.com/docs/epic-account-services/eos-friends-interface) for more information on each respective topic.

> [!NOTE]
> This sample includes the UIFriendsMenu. Please see [the plugin's documentation on UIFriendsMenu](../uifriendsmenu.md) for more information.
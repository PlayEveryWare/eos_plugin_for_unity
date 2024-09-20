<a href="/com.playeveryware.eos/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

## **Player Reports and Sanctions Demo**
This demo showcases the reports interface and sanctions interface. This is done through a report menu, and a display of any current sanctions on the user's account.
- The friends tab on the right of the screen allows the user to open a report window with the ``Report`` button.
    - The report window has a dropdown to select the offense.
    - ``Optional Message`` allow the user to include a message with their report.
    - The ``Submit Report`` button sends the report.
- The ``Current Sanctions`` section shows any sanctions on the users account.
    - The ``Refresh`` button refreshes the display.
    - Sanctions can be added through the developer portal for the product from the ``Game Services -> Sanctions`` menu, where sanctions can be added, removed, updated and pending sanctions can be activated.

![Reports](../images/eos_sdk_player_reports_and_sanctions.png)

> [!NOTE]
> See [Epic's documentation on the reports interface](https://dev.epicgames.com/docs/game-services/reports-interface), and [Epic's documentation on the sanctions interface](https://dev.epicgames.com/docs/game-services/sanctions-interface) respectively for more information.

> [!NOTE]
> This sample includes the UIFriendsMenu. Please see [the plugin's documentation on UIFriendsMenu](../uifriendsmenu.md) for more information.
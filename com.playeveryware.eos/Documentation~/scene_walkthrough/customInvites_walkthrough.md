<a href="/com.playeveryware.eos/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

## **Custom Invites Demo**
This demo showcases the custom invite functionality allowing for messaging with arbitrary payloads. This is meant for cases where an already existing invitation system exists. This demo allows users to send invites with a text payload to anyone on their friends list.
- The ``Payload`` field is where the text payload should be input.
- The ``Set`` button finalizes the invitation creation allowing for it to be sent.
- The ``Invite`` button next to any name on the friends list sends an invitation to that user **only if an invitation has been set**.
- The ``Pending Invite`` window will display any incoming invitations, with an ``Accept`` or ``Reject`` option.
- The ``Processed Invites`` window shows all processed invites for the current login period, and if they were accepted or rejected.

![Custom Invite](../images/eos_sdk_custom_invites.png)


> [!NOTE] 
> See [Epic's documentation on the Custom Invites interface](https://dev.epicgames.com/docs/game-services/custom-invites-interface) for more information.

> [!NOTE]
> This sample includes the UIFriendsMenu. Please see [the plugin's documentation on UIFriendsMenu](../uifriendsmenu.md) for more information.
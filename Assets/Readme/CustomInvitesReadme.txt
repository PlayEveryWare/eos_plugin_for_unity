The Custom Invites Services provides a method of integrating existing invite systems with the invite functionality of the EOS Social Overlay. Calling the SetCustomInvite method of the Custom Invites Interface with a custom payload will enable the invite functionality in the friends list, at which point invites can be sent through the overlay or with the SendCustomInvite method of the Custom Invites Interface. Clients can use the Custom Invites Interface to receive and respond to custom invites, which are accompanied by the custom payload set by the sender. Those clients can then use custom behavior to handle the received payload and accept or reject the invite.

Steps to use this sample:
1. Open the friends tab on the right.
2. Confirm that your friend is displayed
(If it is not, first make sure you are friends on Epic, then make sure the friend has logged into this program and accepted the terms.)
3. Confirm your friend is online.
(This may not be an instantaneous update)
4. Set a payload.
(When a payload is set a confirmation log is made)
5. Press the refresh button, if the invite button is not active.
(If it is still not active, confirm all steps above were taken, otherwise any issues may be solved by waiting and/or pressing the refresh button)
6. Press the invite button next to their handle
7. Your friend should now see a notification that they can either accept or decline, which will leave a note in their Processed Invites.
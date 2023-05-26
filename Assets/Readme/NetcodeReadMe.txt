PlayEveryWare P2P Netcode Epic Online Services Demo

This demo is intended to show the use of the P2P functionality of the EOS Plugin for Unity as network transport layer for the Netcode for GameObjects plugin, through a simple networked application.
The TransportLayer sample can be found in the EOSTransport script and uses the P2P functionality supplied in the EOSTransportManager script.

How to use:
Select a login type
    Dev Auth uses the Dev Auth tool that comes with the EOS SDK
    Account Portal uses the popup Epic window
    Persistent uses the most recent logged in information

You may receive an error on launch if the Netcode for GameObjects plugin is not installed.

Once logged in you can start using the demo, and the Start Hosting button can be used to begin hosting a game session. Testing this will require at least 2 clients running, with at least two accounts that have friended each other.
On the second client, join the first client's session using the Join button either in the friends side bar or through the EOS overlay.
Once multiple users are connected, take control of your player object (which should be labeled with your display name) with the Take Control button and move it around with the gamepad d-pad, left stick, or keyboard arrow keys, and observe those changes being reflected on other connected clients.
If using a controller, character control can be disabled by pressing any face button or the escape keyboard key.
If using a mouse or mobile device, taking control will enable moving the character by clicking/tapping on the play field. Control can be disabled with the Release Control button.
The Disconnect button can be used by clients to disconnect from the host, or by the host to end the session.
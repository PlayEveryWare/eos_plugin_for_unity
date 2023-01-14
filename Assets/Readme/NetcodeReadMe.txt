PlayEveryWare P2P Netcode Epic Online Services Demo

This demo is intended to show the use of the P2P functionality of the EOS Plugin for Unity as network transport layer for the Netcode for GameObjects plugin, through a simple networked application.
The TransportLayer sample can be found in the EOSTransport script and uses the P2P functionality supplied in the EOSTransportManager script.

How to use:
Select a login type
    Dev Auth uses the Dev Auth tool that comes with the EOS SDK
    Account Portal uses the popup Epic window
    Persistent uses the most recent logged in information

If you are getting a error logging in pertaining to the EOS Overlay, this is caused by the overlay not being installed yet. 
To install it create a build of the demo and run the bootstrapper included in the build folder, this will install the overlay.
You may receive an additional error on launch if the Netcode for GameObjects plugin is not installed.

Once logged in you can start using the demo, and the Start Hosting button can be used to begin hosting a game session. Testing this will require at least 2 clients running, with at least two accounts that have friended each other.
On the second client, join the first client's session using the Join button either in the friends side bar or through the EOS overlay.
Once multiple users are connected, use the arrow buttons to move your player's object around, which should be labeled with your display name, and observe those changes being reflected on other connected clients.
The Disconnect button can be used by clients to disconnect from the host, or by the host to end the session.
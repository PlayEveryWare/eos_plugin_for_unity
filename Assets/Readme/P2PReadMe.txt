PlayEveryWare Peer 2 Peer Epic Online Services Demo

This demo is intended to show the Peer-to-Peer functions of the EOS Plugin for unity through a peer-to-peer chat application.
A sample of the P2P interface can be found in the EOS_P2PManager script.

How to use:
Select a login type
    Dev Auth uses the Dev Auth tool that comes with the EOS SDK
    Account Portal uses the popup Epic window
    Persistent uses the most recent logged in information
    
Once logged in you can start using the demo, the friends list on the right should contain your Epic friends list, it is recommended
that you have 2 accounts set up to run the app in parallel, as only one instance of the demo cannot do much on its own. 

Once you have 2 instances running you should be able to click the chat button on either one to open up the chat window and send a message.
Sending a message automatically opens the window on the second instance.

A ProductUserId can be set through the sample UI to test sending messages to users who are not friends or who don't have an EpicAccountId,
such as those logged in with the Connect interface
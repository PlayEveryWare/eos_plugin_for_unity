PlayEveryWare Store Epic Online Services Demo

This demo is intended to show the Store function of the EOS Plugin for unity through a Store application,
a sample of the Store interface can be found in the EOS_StoreManager script.

How to use:
Select a login type
    Dev Auth uses the Dev Auth tool that comes with the EOS SDK
    Account Portal uses the popup Epic window
    Persistent uses the most recent logged in information

If you are getting a error logging in pertaining to the EOS Overlay, this is caused by the overlay not being installed yet. 
To install it create a build of the demo and run the bootstrapper included in the build folder, this will install the overlay.

Once logged in you can start using the demo. There are two buttons which should open up an overlay for an order of the respective item, and a refresh button at the bottom. 
PlayEveryWare Achievements Epic Online Services Demo

This demo is intended to show the Achievment function of the EOS Plugin for unity through a achievement display application,
a sample of the P2P interface can be found in the EOS_AchievementManager script.

How to use:
Select a login type
    Dev Auth uses the Dev Auth tool that comes with the EOS SDK
    Account Portal uses the popup Epic window
    Persistent uses the most recent logged in information

If you are getting a error logging in pertaining to the EOS Overlay, this is caused by the overlay not being installed yet. 
To install it create a build of the demo and run the bootstrapper included in the build folder, this will install the overlay.

Once logged in you can start using the demo, The achievements are displayed in the upper left corner, clicking on an icon brings up information about that specific achievement.
Info includes the Id, display name, description, any data used to track progress and if unlocked the time and date that it was
The demo uses number of logins as the tracked achievement, you can click the increase login count button to increment the counter, alternativley you can click
the unlock button in the top right corner to unlock an achievement
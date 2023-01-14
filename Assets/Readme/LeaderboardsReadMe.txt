PlayEveryWare Leaderboard Epic Online Services Demo

This demo is intended to show the Leaderboard function of the EOS Plugin for Unity through a leaderboard display application.
A sample of the Leaderboard interface can be found in the EOSLeaderboardManager script.

How to use:
Select a login type
    Dev Auth uses the Dev Auth tool that comes with the EOS SDK
    Account Portal uses the popup Epic window
    Persistent uses the most recent logged in information

If you are getting a error logging in pertaining to the EOS Overlay, this is caused by the overlay not being installed yet. 
To install it create a build of the demo and run the bootstrapper included in the build folder, this will install the overlay.

Once logged in you can start using the demo, on the left are different sets of leaderboards, and on the right the display for the selected leaderboard.
You can input an integer value in the "Enter stat value" field and click "Ingest Stat" to add that stat to the current leaderboard.
The top right corner allows you to filter globally or against just your friends list.
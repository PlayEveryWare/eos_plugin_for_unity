# Epic Online Services Plugin for Unity

----------------------------------------------------------------------------------------
## Overview
This plugin contains a Unity Package Manager (UPM) plugin for wrapping up the Epic Online Services (EOS) C# SDK in Unity. 

Things this plugin provides:

* A GUI for configuring EOS settings and saving them to a JSON file
* Allows for loading EOS multiple times in the Unity editor.
* Provides manager classes that cover the "average" case of using EOS
* Samples as Unity scenes for showing how to use the EOS SDK and the Plugin

----------------------------------------------------------------------------------------
## Integration Notes
For best results, Unity 2020 is preferred. 
Installation of the Epic Game store is required.

### Installing it from a tarball
1. From the Unity Editor, open up the Package Manager.
    * It's listed under Window -> Package Manager
2. Then click the '+' button in the left-ish corner, then 'add package from tarball'.
3. Go to the directory containing the PEW Unity plugin tarball, and select it.
4. Click open.

### Installing it from a git
1. Make sure you have git and git-lfs installed
2. Open up the Unity Editor
3. open up the Package Manager.
    * It's listed under Window -> Package Manager
4. click the ```+``` button
5. Select 'Add Package from Git URL'
6. Paste in ```https://github.com/PlayEveryWare/eos_testbed_release.git```

## Installing the samples
To use the samples, install them from the UPM manager. The samples include both usage of the EOS SDK, and convience wrappers to make using the 
SDK more ergonomic in the Unity Game Engine. By being samples in the UPM sense, it's also easier to directly modify them.

-------------------------------------------------------------------------
### Running the samples
* Launch the Unity project with the samples installed

* Configure the EOS plugin
    * See [the docs](Documentation~/configuring_the_eos_plugin.md)

* You should be good to hit "Play"! 

* Login with your Epic Account

----------------------------------------------------------------------------------------
## Running and Configuring the EOS SDK Dev Auth Tool
* Launch the [dev auth tool](https://dev.epicgames.com/docs/services/en-US/DeveloperAuthenticationTool/index.html)
* Log in with one's user credentials that are registered with Epic
* Pick a port to use on the computer. 8888 is a good quick to type number that isn't usually used by a process
* Pick a username. This username will be used in the sample to log in.
More specific and up-to-date instructions can also be found on Epic's [website](https://dev.epicgames.com/docs/services/en-US/DeveloperAuthenticationTool/index.html)

----------------------------------------------------------------------------------------
## Additional Documentation
Additional documentation can be found in the Documentation~/ directory.

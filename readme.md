# Epic Online Services Plugin for Unity

----------------------------------------------------------------------------------------
## Overview
This repo contains a Unity Project and plugin for using the EOS SDK in Unity.

Things this project currently does:

* Has a GUI for configuring EOS settings and saving them to a JSON file
* Allows for loading EOS multiple times in the editor.
* Provides a project for making a Native Plugin for use in Unity
* Provides a simple Plugin for EOS usage in Unity
* Provides classes that cover the "average" case of using EOS
* Samples for using the EOS SDK in Unity

Things this project does not *currently* do:

* Follow a consistent coding style
* Guarantee strict correctness of EOS usage
* Have a strict over-arching design

It didn't used to have this, but now does!  
* ~~Correctly reload the DLL between launches in editor.~~

## Supported Platforms

The follow target platforms are supported in Unity for the current release of the plugin.

| Unity Target Platform | Current Plugin Release |
| - | - |
| Windows Standalone x64 | Supported |
| Windows Standalone x86 | Supported |
| Universal Windows Platform x64 | Supported |
| Android | Future |
| iOS | Future |
| Linux | Future |
| MacOS | Future |
| Console Platforms | Future |
| WebGL | Not Supported |
| Universal Windows Platform x86 | Not Supported |
| Unity Web Player | Not Supported |

----------------------------------------------------------------------------------------
## Integration Notes

### Prerequisites
* Ensure At least Visual Studio 2017 is installed.
* Ensure At least Unity 2020.1.11f1 is installed
* Ensure required Platform SDKs are installed (Windows, Linux, macOS)

### Build steps For Native Libraries
* Build the visual studio solutions for the native DLLs
    * Build the DynamicLibraryLoaderHelper sln in DynamicLibraryLoaderHelper/ for all platforms

The result from building those projects will be a bunch of libraries that should be in the correct locations
for Unity to run.  


### Installing it via copy and paste
It's possible to manually copy all the files under Assets/Plugins/ into a project to "install it"

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
6. Paste in ```https://github.com/PlayEveryWare/eos_plugin_for_unity_upm.git```

----------------------------------------------------------------------------------------
## Running and Configuring the EOS SDK Dev Auth Tool
* Launch the dev auth tool
* Log in with one's user credentials that are registered with Epic
* Pick a port to use on the computer. 8888 is a good quick to type number that isn't usually used by a process
* Pick a username. This username will be used in the sample to log in.
More specific and up-to-date instructions can also be found on Epic's website for EOS

----------------------------------------------------------------------------------------
### Running the samples
* Launch the Unity project

* Ensure that all the Dlls you added are marked correctly to be loaded in the inspector in Unity
    * This step is only needed if you have manually updated the EOS SDK.

* Configure the EOS plugin
    * See [the docs](docs/configuring_the_eos_plugin.md)

* You should be good to hit "Play"! However, it's necessary to run and configure the EOS SDK Dev Auth Tool first.

## Standards
See [standards.md](docs/standards.md)

----------------------------------------------------------------------------------------
## Class description
See [docs/class_description.md](docs/class_description.md)

----------------------------------------------------------------------------------------
## Additional Documentation
Additional documentation can be found in the [docs/ directory](docs/).


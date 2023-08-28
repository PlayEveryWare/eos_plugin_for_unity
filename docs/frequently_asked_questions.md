# Frequently Asked Questions (FAQ)

--------------------------------------------------

## Why does the plugin fail to work after changing configuration?

To rerun in UnityEditor without rebooting, we must reload the EOS SDK dll between runs.  
To find out why and how to do so look [here](https://github.com/PlayEveryWare/eos_plugin_for_unity/blob/development/docs/unity_specific.md)

## How do I override sandbox or deployment IDs when publishing on the Epic Games Store?

This functionality is outlined in the [EGS readme document](egs/egs_readme.md#overriding-sandbox-andor-deployment-id).

## How do I get the Epic Username ?
It depends on what one means by "Username".

If one means a name that's displayable to the user, a.k.a. a display name, then the following 
code should suffice, assuming one has already logged in:

```
    var userInfoManager = EOSManager.Instance.GetOrCreateManager<EOSUserInfoManager>();
    var userInfo = userInfoManager.GetLocalUserInfo();

    if (UserInfo.UserId?.IsValid() == true)
    {
        DisplayNameText.text = UserInfo.DisplayName;
    }
```

However, if one means "Epic username that's passed to the app via the Epic Launcher", there is a method in
`EOSManager` that one may use.

```
    var epicLauncherArgs = EOSManager.instance.GetCommandLineArgsFromEpicLauncher()
    string epicUsername = epicLauncherArgs.epicUsername;
```
This will get whatever username is passed on the command line from Epic.


## Can a title pass a custom device ID? How does one do that?
A title can pass a custom device ID, but must be sure that the ID is unique to the device.
For example, assuming `CoolMethodThatCreatesAUniqueDeviceID` is method that the title has written to generate
a unique string that can identify the device:

```cs
    private void CreateCustomDeviceID()
    {
        var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
        var options = new Epic.OnlineServices.Connect.CreateDeviceIdOptions()
        {
            DeviceModel = CoolMethodThatCreatesAUniqueDeviceID()
        };

        connectInterface.CreateDeviceId(ref options, null, CreateDeviceCallback);
    }``
```

More specific information can be found in [Epic's documentation](https://dev.epicgames.com/docs/api-ref/functions/eos-connect-create-device-id).

## Are their Alternatives to storing the config files in Streaming Assets? Why is the file there?
Quick summary: Those values are not as 'secret' as one might assume, and it's somewhat safe to have them in the open. 

The config file has to be in StreamingAssets so that the GfxPluginNativeRender can access the values in it before 
all of Unity has been bootstrapped so that the Plugin can hook all the appropriate things before the first graphics call by the Unity engine.
See [eos_config_security.md](eos_config_security.md) for more information. 

## Why does the Demo Scene fail to load?

There is a standard sample pack, and several extra packs in the EOS Unity Plugin. If a scene doesn't load, remember to import the wanted extra pack.
Additionally make sure all wanted sample scenes are included in the build settings as shown in steps 4.-6. of <a href="/readme.md#importing-the-samples">Importing the samples</a>.

## What is the correct way to log into the Epic Games Store?
The correct way to connect to the Epic Games Store through your application would be to use the exchange code login method:

### Exchange Code

`Exchange Code` login could be used when launching the game through Epic Games Launcher on desktop platforms (Windows, Mac, Linux)  
The required exchange code could be retrieved with `GetCommandLineArgsFromEpicLauncher()`

```cs
    EOSManager.Instance.StartLoginWithLoginTypeAndToken(
        loginType,
        null, // Intended for UserID, but is unused for Exchange Code login
        EOSManager.Instance.GetCommandLineArgsFromEpicLauncher().authPassword, // The exchange code itself, passed as login token
        StartLoginWithLoginTypeAndTokenCallback);
``` 

## Do I or my players need an Epic Games Account?

### As a developer
As a developer you will need to make/have an Epic Games account in order to interact with the [EOS Developer Portal](dev.epicgames/portal) and manage your product.

### As a player
Players are given multiple login options, this changes from platform to platform, and different login methods provide different levels of functionality. Details of which login methods are supported by each platform are listed here:
![LoginByType](images/login_type_by_platform.png)

## What does the "DllNotFoundException" error mean? 

This might be caused by libraries/binaries not being fetched from git lfs.  
Which mainly happens when adding the UPM `via git url`   

To fix this you may do one of the following:

- Initialize git lfs on the package folder (from a command window `git lfs install`).
- Add the UPM `via tarball` downloaded [here](https://github.com/PlayEveryWare/eos_plugin_for_unity/releases) instead.

### Why am I getting Overlay Errors?
Overlay errors are most likely due to not having the overlay installed, this is done in two steps:
 1. Install the [Epic Games Store](https://store.epicgames.com/) application.
 2. Run the `EOSBootstrapper.exe` that is generated with a build before running the application.

### Missing Native Libraries

Some native functionality are required for platform specific needs.  

To get these dependent libraries, use the platform library build tool in the plugin at `Tools > Build Library > [Target Platform]`

Or to install the libraries manually,  
go to the `NativeCode` folder, find the target platform, and *build the `.sln`* or *`run the makefile`* in the folder.


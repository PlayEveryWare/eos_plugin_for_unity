# Frequently Asked Questions (FAQ)

--------------------------------------------------

## Why does the plugin fail to work after changing configuration

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

```
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
The config file has to be in StreamingAssets so that the GfxPluginNativeRender can access to the values in it before 
all of Unity has been bootstrapped so that the Plugin can hook all the appropriate things before the first graphics call by the Unity engine.
See [eos_config_security.md](eos_config_security.md) for more information. 

## Why does the Demo Scene fail to load

There is a standard sample pack, and several extra packs in the EOS Unity Plugin. If a scene doesn't load, remember to import the wanted extra pack.
Additionally make sure all wanted sample scenes are included in the build settings as shown in steps 4.-6. of <a href="/readme.md#importing-the-samples">Importing the samples</a>.

## What is this error 

### DllNotFoundException

This might be caused by libraries/binaries not being fetched from git lfs.  
Which mainly happens when adding the UPM `via git url`   

This could be fix by one of the following:   

A. Initialize git lfs on the package folder  
B. Add the UPM `via tarball` downloaded [here](https://github.com/PlayEveryWare/eos_plugin_for_unity/releases) instead

### Missing Native Libraries

Some native functionality are required for platform specific needs.  

To get these dependent libraries, use the platform library build tool in the plugin at `Tools > Build Library > [Target Platform]`

Or to install the libraries manually,  
go to the `NativeCode` folder, find the target platform, and *build the `.sln`* or *`run the makefile`* in the folder.


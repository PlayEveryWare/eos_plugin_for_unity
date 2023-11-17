<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">How to configure Easy Anti-Cheat</div>
---

The EOS plugin provides a method of automating EAC installation as part of the build process for Windows, Mac and Linux platforms. First, some additional configuration is required.

## Prerequisites
* [Enable EAC modules](https://dev.epicgames.com/docs/game-services/anti-cheat/using-anti-cheat#set-up-update-and-revert-your-client-module) for your desired platforms and [activate client protection](https://dev.epicgames.com/docs/game-services/anti-cheat/using-anti-cheat#anti-cheat-service-configuration-in-developer-portal).
* Make sure the Anti-Cheat interface is enabled in your [client policy](https://dev.epicgames.com/docs/dev-portal/client-credentials#policies).
* [Download your EAC integrity keys.](https://dev.epicgames.com/docs/game-services/anti-cheat/using-anti-cheat#configure-your-integrity-tool) Make sure to store your private key in a secure manner, as making it public will allow players to bypass client protection.

## Steps
1) Open your Unity project with the plugin installed.
2) Open the editor preferences settings for the plugin, which is avaiable in `Tools -> EOS Plugin -> Configuration`. All EAC options are located under the Tools subheader.
3) Enable EAC build steps by checking the `Use EAC` toggle.
4) Set `Path to EAC private key` and `Path to EAC Certificate` to the private and public keys you previously downloaded, respectively.
5) (Recommended) Set `Path to EAC splash image` to a 800x450 PNG that will display in the EAC bootstrapper.
6) (Optional)
    1) If you want to customize the config for the EAC integrity tool, you can copy the default config from 
    `Packages/Epic Online Services Plugin for Unity/Editor/Standalone/anticheat_integritytool.cfg` to your desired location, customize it, and set `Path to EAC Integrity Tool Config` to the new file. See Epic's [documentation](https://dev.epicgames.com/docs/game-services/anti-cheat/using-anti-cheat#configure-your-integrity-tool) for more information.
    2) If you've downloaded the integrity tool yourself and want to use that, the path can be set via `Path to EAC Integrity Tool`.

If the integrity tool was configured incorrectly, the Anti-Cheat interface should print a warning message after EOS initialization.

# Testing EAC (Windows)

If you would like to test the functionality of EAC as it pertains to file integrity, then open the plugin project and follow the steps above. Once you have built the game with Easy Anti Cheat (EAS), build your game and launch the `EACBootstrapper.exe` file generated at the root of your build directory.

1. Launch the `EACBootstrapper.exe` file again, and the project should launch.

    If, when launching with `EACBootstrapper.exe` you get a message akin to the following:

    <img src="/docs/images/EAC_not_installed.png" width="450" />

    Then open an elevated (Administrator) terminal window within the `EasyAntiCheat` directory at the root of your build directory, and type the following command:

    ```bash
    .\EasyAntiCheat_EOS_Setup.exe install prod-fn
    ```

2. Once the game (or our sample project) has launched successfully and fully, **close the application**.

3. Navigate to a file that will have been hashed and covered by EAC. If using the included project replete with Sample Scenes, you could modify any of the files within the Demo_Data directory (for example you could add a property to the JSON file `EOS Unity Plugin - Demo_Data\StreamingAssets\EOS\EpicOnlineServicesConfig.json`) like so:

    <img src="/docs/images/eac_added_property.png" width="400" />

4. After saving changes made to that file, launch the sample scene "Lobbies," and look at the log. You should see an error logged there indicating that the file integrity of that modified file has been violated.

    <img src="/docs/images/file_integrity_violation.png" />

> [!NOTE]
> The "Lobbies" sample scene was selected to implement an example of EAC file integrity testing because lobbies themselves have some additional EAC features connected, so it seemed like a natural place to demonstrate that functionality. The relevant lines of code that trigger the notification of file integrity violation can be found within `Assets/Scripts/EOSEACLobbyManager.cs`:
> 
> ```cs
> public EOSEACLobbyManager()
> {
>     EOSManager.Instance.SetLogLevel(LogCategory.AntiCheat, LogLevel.Verbose);
>
>     LobbyManager = EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();
>     AntiCheatManager = EOSManager.Instance.GetOrCreateManager<EOSAntiCheatClientManager>();
>
>     CurrentLobbyPeers = new HashSet<ProductUserId>(); 
>
>     OutgoingMessageCounters = new Dictionary<ProductUserId, int>();
>     IncomingMessageCounters = new Dictionary<ProductUserId, int>(); 
>
>     if (AntiCheatManager.IsAntiCheatAvailable())
>     {
>         LobbyManager.AddNotifyLobbyChange(OnLobbyChanged);
>         LobbyManager.AddNotifyLobbyUpdate(OnLobbyUpdated);
>         LobbyManager.AddNotifyMemberUpdateReceived(OnMemberUpdated);
>
>         AntiCheatManager.AddNotifyToMessageToPeer(OnMessageToPeer);
>         AntiCheatManager.AddNotifyPeerActionRequired(OnPeerActionRequired);
>         AntiCheatManager.AddNotifyClientIntegrityViolated(OnClientIntegrityViolated);
>     }
> }
>
> /// <summary>
> /// Called when the integrity of the client has been violated according to EAC
> /// </summary>
> /// <param name="data"></param>
> private void OnClientIntegrityViolated(ref OnClientIntegrityViolatedCallbackInfo data)
> {
>     Debug.LogError("EAC Client Integrity Violeted!");
> }
> ```






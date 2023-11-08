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
4) Set `Path to EAC private key` and `Path to EAC Certificate` to the private and public keys you previosuly downloaded, respectively.
5) (Recommended) Set `Path to EAC splash image` to a 800x450 PNG that will display in the EAC bootstrapper.
6) (Optional)
    1) If you want to customize the config for the EAC integrity tool, you can copy the default config from 
    `Packages/Epic Online Services Plugin for Unity/Editor/Standalone/anticheat_integritytool.cfg` to your desired location, customize it, and set `Path to EAC Integrity Tool Config` to the new file. See Epic's [documentation](https://dev.epicgames.com/docs/game-services/anti-cheat/using-anti-cheat#configure-your-integrity-tool) for more information.
    2) If you've downloaded the integrity tool yourself and want to use that, the path can be set via `Path to EAC Integrity Tool`.

If the integrity tool was configured incorrectly, the Anti-Cheat interface should print a warning message after EOS initialization.
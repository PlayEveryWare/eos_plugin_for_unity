<a href="http://playeveryware.com"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="10%"/></a>

# <div align="center">Epic Games Store</div> <a name="epic-games-store">

## <div align="center">Disabling Steam integration</div> <a name="disabling-steam-integration">
To disable Steam integration functionality, delete the configuration `Assets/StreamingAssets/EOS/eos_steam_config.json`` before building.

## <div align="center">Overriding Sandbox and Deployment IDs</div><a name="overriding-sandbox-and-deployment-ids">
At runtime, the sandbox ID can be overridden with the launch argument `-eossandboxid=<id>` and the deployment ID with `-eosdeploymentid=<id>`. 

To specify deployment IDs in this case, Sandbox Deployment Overrides can be added in the plugin config editor to define sandbox-deployment pairs, which will override the deployment ID when a given sandbox ID is used.
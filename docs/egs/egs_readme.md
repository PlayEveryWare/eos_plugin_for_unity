# Epic Games Store

## Disabling Steam integration
If one is making a EGS build without wanting to integrate Steam functionality, the config file Assets/StreamingAssets/EOS/eos_steam_config.json should be deleted before making a build.

## Overriding Sandbox and/or Deployment ID
At runtime, the sandbox ID can be overidden with the launch argument `-eossandboxid=<id>` and the deployment ID with `-eosdeploymentid=<id>`. When launching with EGS, the argument `-epicsandboxid=<id>` will be used to specifiy the sandbox for the given launch configuration. To specify deployment IDs in this case, Sandbox Deployment Overrides can be added in the plugin config editor to define sandbox-deployment pairs, which will override the deployment ID when a given sandbox ID is used.

<a href="http://playeveryware.com"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="10%"/></a>

# <div align="center">Command-line Export of Plugin</div>
---

This document outlines how to export the plugin from source using a command line interface. This may be helpful if you have a CI/CD pipeline for your game, and want it to always incorporate whatever version of the plugin you have locally.

## BuildPackage

The following command generates a new `com.playeveryware.eos-[VERSION].tgz` file at the indicated output directory, the same exact way it would if you created a package via Unity Editor `Tools -> EOS Plugin -> Create Package` (and subsequently pressed "Create UPM Package"):

```
Unity.exe -batchMode \
    -nographics \
    -quit \
    -projectPath [Path to eos_plugin_for_unity root directory] \
    -executeMethod BuildPackage.ExportPlugin \
    -EOSPluginOutput [Absolute Path to Output]
```

> [!IMPORTANT]
> In order for this to work correctly, it is imperitive that your repository be properly checked out, this includes making sure that the lfs files are properly downloaded via `git lfs install` / `git lfs pull`.

The following command-line argument is the only one introduced by this project:

`-EOSPluginOutput` 
The directory in which the newly created tarball should be placed.

The following command-line arguments are used and defined _by Unity_, and you can read the docs on them in fuller detail [here](https://docs.unity.com/ugs/en-us/manual/ccd/manual/UnityCCDCLI):

`-batchMode`
Indicates whether Unity should launch a window or run 'headless'.

`-nographics`
Allows the build to be done on hardware that does not have a GPU. 

`-quit` 
On success or failure, quit. 

`-projectPath` 
The path to the root of this repository.

`-executeMethod` 
Indicates which static class and which static function on that class should be executed. (In this case, the value passed to this argument should be `BuildPackaghe.ExportPlugin`).
<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Security of Config File</div>
---

# Why is the config file in plain text in the Streaming assets folder?
The configuration file needs to be read before the Unity runtime environment is fully setup and running from a native DLL, which means that none of the usual Unity niceties for obscuring config files work.

# Is there anyway to make the config file more secure?
There are a few options one could take to make it at least more obfuscated.

There is a partially implemented feature that lets the native code load another DLL, and call functions from it to load values.
While this does offer more obfuscation, it is still insecure. A user could try to change the DLL, or replace it with a new one. 
Because the plugin is open source, a user could see how load the config file, and use that to grab the keys from the helper DLL by
loading the DLL from a simple program or script. Potentially easier, the user could even scrape the values via a program like `strings`
and then edit them in-place in the binary.

Finally, the config values aren't really secret anyways: the client id and secret show up in web requests, and the 
encryption key really just stops Epic from potentially reading your data stored via the PlayerDataStorage APIs.

With that all being said, there are a few options one may pursue if one still wishes to do obfuscate this.

# General instructions on how to obscure the config file

## Option One: build a custom DLL
This involves pulling the repo, then going into the `lib/NativeCode/` directory, and building the code for `GfxPluginNativeRender`.
After being sure one is able to do that, one needs to go into the `dllmain.cpp` file, and modify the code so that the config values 
are _in the code_ instead of being read from the config file. This can be done by modifying the function `UnityPluginLoad`, near where it calls 
`eos_config_from_json_value`.

## Option Two: build a custom side-loaded DLL
If one doesn't want to modify the `dllmain.cpp` of the `GfxPluginNativeRender` code, one can add a DLL
called `EOSGenerated.dll`, and export a function called `GetConfigAsJSONString()` to allow the `GfxPluginNativeRender`
to configure the EOS platform.

A disadvantage to this method is that depending on one's choice for deployment, anyone might be able to modify the 
DLL and specify different configuration values for your title.

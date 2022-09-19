# Custom EOSConfig reading on Windows

It might be preferable to obfuscate or hide the configuration used for the project.
If one doesn't want to modify the dllmanin. of the GfxPluginNativeRender, one can add a DLL
called EOSGenerated.dll, and export a function called GetConfigAsJSONString() to allow the GfxPluginNativeRender
to configure the EOS platform.

The main reason one might want to do this, is if one wants to have custom logic for which config values would be
specified at launch.

A disadvantage to this method, is that depending on one's choice for deployment, anyone might be able to modify the 
DLL and specify different configuration values for your title.

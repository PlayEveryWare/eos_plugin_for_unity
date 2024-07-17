## Overview
This document lists the "public" conditional compilation directive defines ("Preprocessor" defines) that control different behavior of the Epic Online Services for Unity Plugin.

## Defines
### `EOS_DO_NOT_UNLOAD_SDK_ON_SHUTDOWN`
If this is defined, the EOSManager will not unload on shutdown. This can alleviate issues where the Unity Editor hangs on second play.
It is not the default because if it were, then any edits to the EOSConfig file would require a reboot of the Unity Editor. In the past, not unloading 
the EOS SDK could also cause issues with state from the previous Unity run not being properly cleaned up.

On macOS and Linux, this define is automatically enabled as on those platforms there is no reliable way to actually unload the dynamic / shared libraries.

Unlike other defines, after defining this, one _must_ reboot the Unity editor.

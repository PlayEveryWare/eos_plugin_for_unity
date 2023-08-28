# .NET quirks

## How do different versions of the API Compatibility Level interact with the plugin?
The general goal of the plugin is to support as many API Levels as Unity does.
However, due to how different features of the C# language are implemented or supported by the runtime,
it can sometimes be the case that some code can be broken by changing
the "API Compatibility Level" in the Player Settings in Unity. Usually this can be considered a defect in the plugin and should be reported as such. 

That being said, it can sometimes be more prudent to change the API compatibility level than to create a fix. 
In those cases, it might be reasonable for users of the plugin to switch to a different API compatibility temporarily until the issue is resolved.
Be warned: sometimes _other_ plugins and code that is *not apart of the plugin* might break when changing the API Compatibility Level.

## Issues with namespaces
Different versions of Unity and different versions of .NET sometimes include different assemblies by default. When adding a feature to the SDK, or when
switching between versions of Unity or between versions of API compatibility, this could cause different assemblies and the namespaces within them to 'disappear'.
The general solution is to switch to a different version of Unity, or rewrite the code such that only particular common assemblies are used. In the rare case
where that is not feasible, some assemblies can be added via an addition to a `csc.rsp` file. 


# Disabling the EOS plugin on specific platforms

## Why disable the plugin?

  The plugin provides a certain amount of automatic functionality to support the operation of the EOS SDK, such as DLL loading and post-processing of builds to include the requisite files. Since some of this functionality it not under the user's direct control, the plugin supports a per-platform method of disabling all significant EOS runtime functionality.
  
## How to disable

  1) Open the Player menu in Project Settings:<br><img width="117" alt="player_settings" src="https://user-images.githubusercontent.com/106182927/234998641-b10ea417-aa27-4c64-943f-884d308c53d9.png">
  2) Select the desired platform and expand the Other Settings submenu:<br><img width="361" alt="platform_settings" src="https://user-images.githubusercontent.com/106182927/234995437-59cf73e6-ae20-4d54-9c06-f9db2e6db9c2.png">
  3) Find the Scripting Define Symbols configuration:<br><img width="341" alt="scripting_defines" src="https://user-images.githubusercontent.com/106182927/234995813-89ecdef6-a7e5-49d6-962c-796d7d196ba7.png">
  4) Use the + button to add the `EOS_DISABLE` define to the list and Apply:<br><img width="342" alt="eos_disable" src="https://user-images.githubusercontent.com/106182927/234995924-489636df-c118-4a28-81f4-5417b45c1fa0.png">

## Effects of EOS_DISABLE

  - All scripts from the C# EOS SDK included in the EOS_SDK subdirectory of the plugin are disabled
  - All scripts included in the EOS plugin samples are disabled except for `EOSHostManager`
  - Build script functionality (any classes implementing `IPreprocessBuildWithReport` or the like) are disabled
  - `EOSManager` remains a valid component but all of its functionality is disabled
  - DLL loading still occurs for the time being

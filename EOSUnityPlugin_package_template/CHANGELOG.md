# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### Unreleased
### Added
### Changed
### Removed
### Fixed

## [1.0.4] - 2022-03-15

### Added
- More debug logging around when the PUID changes.
- Wrapper method for TransferDeviceIdAccount.
- Platform specific binary files.
- Exe for eosboostrapper command.
- More annotations to logging methods in EOSManager.
- Windows post build step for bundling EOSBootstrapper.
- Method to get pointers to the allocators for configuring EOS.
- Feature to invoke the EOSBootstrapper on windows post-build.
- Better exception messaging when handling duplicate message registration.
- Code for adding files to Unity windows build dir post build.
- Config value for controlling delay of overlay input.
- Method for showing a string as a float, so one may have an 'empty' float value.
- Version of AssigningBoolField which has label width.
- Method for easy display and config of bool value in EOS config editor.
- Method for checking nullable bool.
- Ability to ignore files when building a package.
- Better logging support before platform create. Correct issue with finding SteamSDK DLL.
- Warnings in GUI Package Creation Tool.
- Editor tool to install new eos sdk from zip into project.
- Added json dependency.
- Added PlayerReportsAndSanctions to scenes.
- Support for debug builds of DynamicLibraryLoaderHelper.

### Changed
- Update to EOS SDK 1.14.2-hf-1
- Replaced HttpClient with UnityWebRequest.
- Changed DynamicLibraryLoaderHelper to a static library and add EOSOverlayRenderSupport project as a dynamic library. Moved graphics code to EOSOverlayRenderSupport.
- Changed to a format string in SimplePrintCallback, add null check for category.
- Changed SHA1 to open file with read only permissions.
- Now always sending input to EOS.
- Modified files to reference pch file correctly.
- Disabled navigation on social overlay.
- Changes to limit compilation errors when compiling some unsupported platforms.
- Disabled using steam from c#.
- Use the release configurations of native libraries.
- Improved pre-existing logging and error messages.
- Enabled a method that configures the memory allocation functions by grabbing native function ptrs instead of using C# methods.
- Change overlay rendering back to the default submission method.
- Using pause/fillAndResume until flickering can be sorted out.
- Using correct dwords->bytes conversion.
- Select default button on controller refocus.

### Removed
- Certain unused variables.
- An error that shows on some platforms about the str being a bad format.
- Removed code to turn off logging to mitigate reported potential hangs on EOS platform shutdown.
- Removed passing of values that are no longer needed for contruction of AddNotifyRTCRoomConnectionChangedOptions.
- Changed EOSManager so it can use flags from JSON file.
- Removed support for old EOS_UI_ReportKeyEvent.
- Removed input system dependency.

### Fixed
- Exclude gfx plugin in editor.
- Proper support for dos 2 unix line ending code.
- Disable using the fallback software keyboard.
- Disable input when the overlay is showing.
- Enable compiling and using the code in EOSManager_Windows when running on windows in editor.
- Fixed SHA calculation issue caused by files not being shared.
- Fixed redundant add of achievement def objects.
- Linking against static runtime libraries.
- Cleaned up FileUtils, fix bug in Dos2Unix method (incorrect def for ln).
- Correctly register for overlay callback.
- Fixed UI in EpicOnlineServicesConfigEditor so that layout isn't cut-off for "Always send Input to Overlay".
- Fixed UILoginMenu to use method for checking if EOS Overlay is open with exclusive input.
- Handled loading and calling 'init' when the steam dll has already been loaded.
- Changed how some strings are converted from UTF-16 to UTF-8, and how std::filesystem::path is converted.
- Handled case where the steam dll name isn't overriden but the found path doesn't have the steam DLL by assuming the steam dll is either steam_api.dll or steam_api64.dll.
- Moved overlay initialization into EOSManager.Init.
- Allowed starting up of Steam SDK from the Native code so that EOS can work with Steam when configured to be managed by the application. This allows for steam friends to show in the EOS friend's list.
- Fixed controller navigation on login menu when changing login types.
- Correctly increment index when adding new platforms to EOS config editor.
- Cleaned up DLLHandle and if/def away UWP plugin search path.
- Mitigated overlay ReportInputState performance issues.
- Fixed client data pinning.
- Fixed nullrefs in sessions manager.
- Fixed complication errors due to renames of steam structs in eos sdk.
- Fixed compiler errors after updating generated code.
- Fixed pause/resume submission.
- Fixed issue on launch in editor caused by outdated EOSSDK windows dlls being loaded.
- Properly adjusting the CCB buffer instead of the DCB buffer.

## [1.0.3] - 2022-02-09

### Changed
- update to EOS SDK 1.14.2

## [1.0.2] - 2021-11-12

### Changed
- update to EOS SDK 1.14.1
### Fixed
- add com.unity.modules.jsonserialize to package dependencies
- generate C File only when ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE defined
- (samples) display leaderboard entries in UI
- (samples) mark achievement description text as visible
- (samples) fixed EOSFriendsManager.GetDisplayName cached friends
- (samples) include DisableHostMigration in create lobby options.
- (samples) add UserId usage to UnlockAchievementsOptions in UnlockAchievementManually
- (samples) enable correct tabbing and controller/keypad UI navigation
### Added
- add short document with tips on how to debug the native dll
- (samples)add sample for player reporting & sanctions

## [1.0.1] - 2021-08-18

### Fixed
- make Input Manager (Old) default
### Changed
- Updated documentation

## [1.0.0] - 2021-08-16
### Added
- add controller support and virtual keyboard (#4)
- controller support (part 1)
- add controller supported onscreen keyboard
- controller keyboard: 'B' triggers backspace
- controller: add ConsoleInputField, keyboardUI and Input System
- initial support for x64 WSA applications

### Changed
- update to EOS SDK 1.13.1
- change code to make it easier to have platform specific platform init options

### Fixed
- Change DLLHandle to return the correct path when packaged
- add ConsoleInputField focus support for keyboard tab
- update to EOS SDK 1.13.1 fixing in-editor Lobby with voice crash
- change typo to so that correct EOSManager xaudio dll is picked in editor
- enable mouse on click for friends tab collapse/expand
- call InputField.onEditEnd after virtual keyboard closed

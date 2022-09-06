# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### Unreleased
### Added
### Changed
### Removed
### Fixed

## [2.1.1] - 2022-09-06

### Fixed
- Updated GfxPluginNativeRender dll with latest changes

## [2.1.0] - 2022-08-30

### Added
- Preliminary Linux platform support
- Settings UI for sample debug log to change log size, search for strings, and change EOS logging levels
- Sample for Custom Invites Interface
- Warning in EOS config editor when encryption key is invalid
- Button in EOS config editor to generate valid encryption key
- Compiler and runtime warning when running on unsupported platform
- JoinViaPresence option in Sessions sample

### Changed
- Rebuilt sample UI to scale with aspect ratio
- Changed event binding method for some sample UI to make it easier to follow code
- Reduced height and font size of sample debug log and increased scroll wheel sensitivity

### Removed
- Plugin packaging tool from packaged plugin

### Fixed
- Broken UI when resizing sample window
- Scrolling behavior of sample debug log
- UI behavior around mobile screen cutouts
- Issue with parsing uint64_t from empty string in Windows native config code
- Windows x86 logging crash
- Manual achievement unlock
- EOS config save path issue that occurred in newer versions of Unity
- Error in Sessions sample when joining session with presence enabled
- Shutting down EOS when play ends in editor
- Login button enables itself correctly after failed login
- Event listener removal when logging out of Achievements sample
- Title storage sample file query
- Updated obsolete UnityWebRequest use in Achievements sample

## [2.0.2] - 2022-08-15

### Fixed
- Fixed function pointer size causing x86 crash when logging from native DLL.

## [2.0.1] - 2022-07-29

### Added
- (sample) peer-to-peer sample with eac support

### Changed
- renamed EOSEACLobbyTestManager to EOSEACLobbyManager

### Fixed
- create config directory for eos plugin if the the dir isn't there.
- (sample) lobby sample display names 
- (sample): add local user as parameter for achievement interface calls
- (sample): Set default value for storage data
- disable debug logging in EOSManager; one may enable it in the ProjectSettings for PC.
- (sample): Disable EAC functionality if AntiCheatClientInterface is unavailable e.g. when the EAC bootstrapper is not used
    

## [2.0.0] - 2022-07-14

### Added
- Ability for platform specific implementations to update network status.
- Helper method for invoking setting User Presence with an Epic.
- EOS Plugin tools to Player Settings and Preferences.
- Support for grabbing memory counter stats from native code. Disabled by default.
- Handle application status changes.
- New feature for storing settings for the editor tools that don't need to be committed.
- Allow for package descriptions to 'comment out' a line.
- Config value for giving the plugin a time budget.
- Allow EOSManager to keep track of new login and logout changes for connected accounts.
- EAC tools and config files from EOS SDK, and updated Windows post build step to copy them and apply values from EOS config.
- EOSUserInfoManager as a general access point for user info including the local user.
- Standard set of member attributes when connecting to the lobby in EOSLobbyManager, which is currently only the display name.
- Menu item to copy link.xml from UPM package to Assets/EOS.
- Achievements sample scene has a button to manually unlock the achievement.
- When logging into the Achievements scene, the login_count stat is incremented.
- Added toggle to Achievements scene menu to change between viewing the user-specific data for the achievement and the global definition.

### Changed
- Updated to EOS SDK 1.15.1.
- ApplicationStatus no longer updates on every application focus/pause change.
- Improved keyboard navigation for login in samples.
- Improved usability of creating packages with Editor coroutines.
- Default log verbosity on non-editor platforms to Warning.
- DLLHandle.GetPackageName() to public to provide a single access point to package name.

### Removed
- HelperExtensions removed as it was renamed to Extensions.
- Reference to deleted Android docs directory from eos_package_description.json.

### Fixed
- Added workaround code to ensure voice input/output devices work correctly for RTC.
- Achievement scene shows achievements properly.
- Changed strtoull to strtoul to match int type.
- DLL binding change in Windows.
- Checks to ensure the EOS shutdown properly to prevent freezing in the editor when unloading the DLL.
- Switched if check that was commented out incorrectly in EOSLobbyManager.
- Windows workaround for loading EOS bindings so the playing in editor compiles.
- Return correct auth token from GetUserAuthTokenForAccountId.
- Disable the current event system input module when the overlay is up to prevent touch input from going through the overlay.
- Forcing window ratio to 16:9 for samples.
- Limited data transfer size to EOS max file size
- Implemented file transfer of files that don't fit within one chunk.
- Text box for file storage demo is now multiline.
- Set LibraryName when using EOS_DISABLE directive.
- Changed sample scenes to use old input system.

## [1.0.5] - 2002-07-11

### Changed
- Update to EOS SDK with Steam hotfix

## [1.0.4] - 2022-03-18

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
- Update to EOS SDK 1.14.2-hf-1.
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
- Removed passing of values that are no longer needed for construction of AddNotifyRTCRoomConnectionChangedOptions.
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
- Handled case where the steam dll name isn't overridden but the found path doesn't have the steam DLL by assuming the steam dll is either steam_api.dll or steam_api64.dll.
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

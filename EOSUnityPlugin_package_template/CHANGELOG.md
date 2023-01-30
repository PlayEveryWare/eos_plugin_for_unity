# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### Unreleased
### Added
### Changed
### Removed
### Fixed

##[2.1.9] - 2023-01-30
### Added
- feat: steam login sample
- feat(Android, lib): Library static/dynamic linking options
- feat(rtc): Press to talk functionality. (#199)
- chore: add android specific assets to eos_package_description

### Changed
- refactor: moved singletons to prefab
- docs: add upgrade steps for using the upm tool
- docs: update upm readme
- docs: update readme for repo
- docs: updated standards docs
- refactor: removed stray arrows

###Fixed
- fix(android, lib): Check Android toggle on lib metafiles
- fix(android, lib): Copy metafiles during preprocess
- fix(pkg_dscrpt, android): Removed leftover meta file from heleting the extra aar file (#212)
- chore: fixed networking sample asmdef name
- fix platform specifics domain reload error
- fix(p2pnetcodeSample) Fixed errors when hosting
- fix(pkg_descrpt, android): Remove Duplicate aar
- fix: transport sample timeout
- fix: eos transport logging
- fix: transport sample friends ui
- fix: UpdateApplicationConstrainedState null check
- fix: add `!EOS_DISABLE_FULL` constraint to Editor asmdef

## [2.1.8] - 2023-01-05
### Added
- docs(mac) : Add steps for building on mac
- docs: egs steam disable
- (oculus): adding a case for oculus auth to be considered
- feat: added packaging options to eos preferences editor
- feat: auth scope flags config
- feat: eac toggle and bootstrapper name build config
- feat: automated dll signing
- feat: editor support with EOS_DISABLE
- feat: deployment env command line override

### Changed
- docs: readme typo
- Update index.md
- refactor(editor): editor pref path selection
- chore : Update EOSPlugin Website
- chore(mac) : Shows an error message if mac build missing libraries
- (Android) : Logs error when user doesn't have the upm installed while building
- updated EOS SDK to 1.15.4
- moved android editor scripts
- style(editor): config tooltips and platform config grid
- feat: automatic packaged version string update

### Fixed
- (log,mac) : logError instead of log when missing libraries
- (android) : Disable OnPreprocessBuild_Android on other platforms
- (mac,mic) : Ask for permission only when second user joins lobby
- (android) : Disable MINIFY_WITH_R_EIGHT for Unity 2022.2 above
- (android) : Ensure gradleTemplate.properties exists for android building
- (build) : Copy gradle.properties from the package with the correct source
- (android): dynamic gradle template
- updating macOS DLL to function on all mac processors
- leaderboard menu init
- (editor): package file utils recursive copy
- (package) : Supports building with Unity 2019
- sample editor script assembly
- unitywebrequest 2019 compatibility
- EOS_DISABLE fix
- Add missing networking files to package
- editor ui cleanup and polish
- fixes for iphone build errors

## [2.1.7] - 2022-11-22
### Added
- docs(android): update readme for android to included needed settings
- feat(android): Auto config gradleTemplate file so that aar files won't cause gradle build errors
- feat(config): Config Verification Prebuild Step (#158)
- feat(config) : Save Application version as product version at build time (#151)
- feat(android): remove dependency on including gradle files in the published plugin && add support for auto configure eos_dependencies strings.
### Fixed
- (windows): add eac tool to package description.
- ensure config file is loaded and not null before using in preprocess script
- (android): resolve gradle error that occur due to unity editor version update
- (android,ios): Properly set presence in UICustomMenu and UIPeer2PeerMenu
- EOSbootstrapper and EAC binary install when building from plugin (#161)
- invalid IntPtr Null check (#160)
- resolve null pointer exception that can occur when config section hasn't been setup yet.

## [2.1.6] - 2022-10-31
### Added
- (samples) : plugin version ui
- (readme) Added Missing ReadMes
-  generalized callback functionality for eos auth

### Changed
- (macos): move more native files over to the custom makefile
### Removed
### Fixed
- (windows): re-add DynamicLibraryLoaderHelper.dll.meta
- (samples): ignore received duplicate custom invites
- (mic,iOS) : Link function from the correct dylib for iOS
- fix directory not found when using a fresh copy of the project and attempting to build the upm package
- (macos,mic): add source file for microphone utility
- (ios): Add cpp, mm, framework files and associated meta files to eos_package_description to be included with upm package builds to fix ios build errors
- (samples): display name ui error

## [2.1.5] - 2022-10-14
### Added
- doc(mac) : supported version doc
- RTC support on iOS
- feat(sessions sample): sanctions flag and presence changes
- feat: local username ui
### Changed
- EOS SDK to 1.15.3
- Move iOS specific login code to EOS iOS Specific class
### Fixed
- (Binding) : Correct binding function names for OSX Editor
- (iOS,RTC) : support RTC when hosting a lobby and someone joins
- (UI) : iOS mic permission status fix
- Moving friend query to prevent errors
- (Lobby,UI) : Make mute button not interactable if mic not permitted
- Leaderboard friends list now works

## [2.1.4] - 2022-10-03

### Fixed
- Updated DLLs and fixed merge error

## [2.1.3] - 2022-09-30

### Added
- Adding instructions to Custom Invites readme
- Custom invites readme
- Updated EOS docs urls and added EAC and custom invites sample locations
- Unlock Achievement Callback

### Changed
- Making the invite button in the appropriate scenes now visible but inactive when not useable
- Modified document to clarify options for obfuscating the EOSConfigs
- Renamed document that describes the EOSConfig loading
- Changed some storage sample text based on UX feedback
- Added spacing to login ui in to better center login button
- Updated to EOS SDK to 1.15.2.1

### Fixed
- Update achievements for users with empty stats
- Deactivate unlock button when achievement unlocked
- Session sample NotFound error
- Friends search does a proper search on cache
- Made session level search case insensitive
- Disabled start and end session buttons based on session state
- Fixed handling of session attributes in session modification
- Fixed compiler error in UIMemberEntry caused by unhandled platforms.
- Talking status for mic permission
- Added readme dir to package description
- Adjusting error logging for searches and clearning search results when scene loads
- Fixed UIMemberEntry prefab RectTransforms
- Fixed debug log layout so log options UI doesn't overlap demo scene
- Some clean up for callbacks
- Fixed scaling issues of Lobby sample UI that were interfering with the create lobby button
- EOS Config editor window on play mode fix to keep window data valid
- Fixed player data storage sample behavior when switching accounts and copying files
- Fixed expand and hide behavior of debug log ui
- EOS config editor encryption key null reference

## [2.1.2] - 2022-09-06

### Fixed
- Updated DLLs with proper build

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

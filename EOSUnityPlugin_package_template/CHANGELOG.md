# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### Inprogress
### Unreleased
### Added
### Changed
### Removed
### Fixed

##[2.3.3] - 2023-06-30

### Inprogress
Android login scheme protocal fix (pending on eossdk-ver1.16)

### Unreleased
Android getting started documentation updates
Virtual cursor

### Added
Feat(Transport Layer) Added functionality to Start Client 
image for ios getting started docs
doc(login) : add supported login type picture into login docs
doc(samples) : adds information about mutli sample packs
doc(samples) : adding additional troubleshooting line

### Changed
Updated readme.md (Getting started)
Update readme_iOS.md (Getting started on iOS)
doc(login) : Feedback ingestion updates
updated doc image for sample packs

### Removed
chore : Removing unused code warning in PackageInstaller
fix : Removed Shadergraph dependency 

### Fixed
Fix(transport) : Moved new code to conditional 
Fix(transport) : added variables to #if 
Fix Added Preserve tags to platform specific code 
Fix Added Linux 
fix Added correct header 
fix(transport) : add scope for editor specific code
fix(steam) : set steamworks.net version to work with steamworks SDK 1.57
fix : remove unknown namespace errors when EOS_DISABLE
fix(iOS) : Move EOS_DISABLE scope to make more sense

##[2.3.2] - 2023-06-15
### Fixed
Added back the missing assemblies for MacOS build in UPM repo

##[2.3.1] - 2023-06-09
### Inprogress
Android login scheme protocal fix (pending on eossdk-ver1.16)

### Added
feat(package) : package auto installer - Netcode
feat(package) : package auto installer - PostProcessing
doc(mac) : second build failing on mac information
doc(mac) : mac getting started
doc(faq): Add sections about Getting Username, Setting Custom Device ID, and Summary about the EOS Config security.
feat : exchange code login functionality (#287) 
chore(log) : log if mac dylibs aren't in repo plugin folder 
Docs: Additional images 
Docs: Updated overlay info
Docs: Updated file names and added images 

### Changed
chore : adjust package description for moved assets
chore : categorize materials by which sample pack they belong
chore : move post process profile to a better folder
feat(package) : adjust package json to have multi samples
feat(tarball) : Update package description for multi sample 
chore : move netcode specific scripts ready for packaging
chore : rearrange scenes for package sample split 
making the error a bit more clear
Update frequently_asked_questions.md (#295)

### Removed
remove platfom unsupported logs from supported platforms
removing a log and some warnings called when hidemenu is called before the user logs in

### Fixed
fix : upm repo lfs support
fix : avoid duplicate declaration in auto package loader
fix : auto shadergraph install for stress test scene
fix(build) : build fix for package installer script 
fix: index out of range error in p2pnetcode
fix(Title Storage) : handle if target filename doesn't exist in storage locally
fix(dataStorage) : clear error from refresh button
fix(tooltip) : quick tooltip disable fix
fixing sessions error  
fix(config) : config empty check 
fix(storage) : Refresh reacts to changes uploaded from other devices 
fix(transoprt) : correct fragmented packet size
fix(regression): Add back code to show how to disable host migration
fix(iOS) : IOS Login Options Refactor (#289) 

 
##[2.3.0] - 2023-05-18
### Inprogress
Android login issues
missing script references in the performance test and p2p netcode scene

### Added
doc: many new docs added and updated for multiple new areas
feat: debug log tooltips
feat: login tooltip examples

### Changed
chore: Files moved into core
refactor: moved apple signin sample editor script to samples subdirectory
refactor: use Path.Combine instead of Path.Join to make it easier for
refactor: renamed essential assembly to core
feat: load sample scenes by name
refactor: change EOSStoreManager to use EOSManager method to get ecom
feat: moved eac integrity tool config to editor directory for easier access
fix: use unique name and do cleanup for eac temp build files
fix: consolidated eac integrity config
chore: moved linux config editor script
refactor: changed version string retrieval method to avoid recompile

### Removed
chore: unneeded SystemMemory code
chore: unneeded .aar

### Fixed
fix:layout fix for mobile and mac
fix: log level menu selection behavior
fix: removed EOS_DISABLE constraint from apple signin editor assembly
feat: add error log when trying to grab ecom interface in editor.
feat: eac splash image selection
feat: eac integrity tool path default
fix: hardcoded tooltip button
fix(package) : Add EOSHostManager (samples essential) into UPM
fix: config editor deployment overrides init
chore: updated package description for essential asm changes and apple signin
feat: sample ui tooltips
fix: ui navigation fallback for input package
fix: network sample asmdef references
fix: EOS_DISABLE no longer breaks builds
feat: update steamworks api from plugin
feat: Add config value for specifying Steam API version.
fix: egs sandboxid handling
fix: case sensitive lobby search
feat: Apple ID connect for iOS
fix: login error when no internet
fix: enable correct ui when changing connect login type
fix(connect,iOS) : add ifdef for when SignInWithApple not installed
feat(AppleID,iOS) : Added Check Define for whether AppleAuth is installed
feat(connect) : AppleID login option for iOS
fix(macOS) : build with both mono and il2cpp


##[2.2.1] - 2023-02-28
### Added
feat: openid connect login sample
feat: linux makefile automation
doc: Add quick doc about how to enable voice chat
feat: windows library build automation
feat: eostransport test functionality
feat: mac makefile automation
docs: p2p sample readme
style: changed placeholder text in p2p sample productuserid ui for clarity
chore: Add description json for installing the EOS SDK from a zip
fix: android text input disappearing
fix: android text input keyboard null check
fix: android text input handling
fix: check against 0 instead of 1 to ensure games with only 1 achievement show correctly
feat: debug toggle for msbuild library automation
feat: msbuild log levels
feat: sandbox deployment id overrides
feat: platform library build config

### Changed
refactor: made MakefileUtil class partial for extensibility
fix: windows exe launch order change to fix EAC 
refactor: isolated steam functionality

### Removed
refactor(stress test) Removed TMP dependency

### Fixed
fix: overrode EOSPluginEditorToolsConfig.Equals and GetHashCode to suppress warnings
fix: openid sample token acquision
fix: linux sdk library name
fix: debug log mesh error
fix: unknown version value shown to users
fix: add missing ref to text mesh pro
fix: Use MacOS code paths when running in editor on mac, even when Unity platform is set to Android
fix: custom invites payload init and clear
fix: player report ui persists after logout
fix: plugin build version
fix(UI,Lobby) : UI Navigation and Layout change
fix(UI) : Member Entry highlight
fix(UI,friends) : UI Layout and Navigation fix
fix: build version string
fix(UI) : Hide lobby search when FriendTab on to allow invite navigation
fix: extended section tabs in eos config editor
fix: netcode sample object spawning
fix: removed missing script from stress test scene
fix(UI, leaderboard): Removed yellow highlight and other adjustments
fix: old input system dpad support
fix:(UI, lobby): Updating invite checking/popup
fix: input system asmdef ref
feat: netcode sample controller and touch input
fix: added remaining platforms to title storage platform tag list
fix: EOSPluginEditorToolsConfig comparison functions
fix(lobby): Logging in/out correctly enables callbacks
fix: custom invite entry interaction
fix: player data dropdown selection highlight
fix: log menu ui highlighting
fix(UI, controller: sanction sample ui navigation
fix(UI, controller): ui selection loss
fix: device display name .net compatibility
fix: ui selection fallback
fix: stress test ui navigation
feat: mac eac support/standalone build consolidation
fix: added missing mac eac files
fix: mac eac support
fix: linux eac support
fix: Don't check for valid EOS config files when the build target has EOS disabled.
fix controller scroll navigation
fix: login ui connect dropdown
fix: use newer method for checking platform defines on Unity versions that have it.
fix: ensure the eos config file is loaded so product id can always be accessed    
    

##[2.2.0] - 2023-02-28
### Added
- Discord connect sample
- Added UI to P2P chat sample to send messages to an arbitrary ProductUserId for testing messaging with non-friends
- Customize Press to talk button in Lobby sample

### Changed
- Updated to EOS 1.15.5
- Moved code for Steam external auth login into its own method
- Moved EAC config values out of the initialize config file and into editor config

### Fixed
- (mac): Build with both mono and il2cpp
- Added horizontal layout to P2P sample text entry UI
- Changed version UI string format to v-<version>
- GetPackageVersion() now returns ?.?.? as the unknown version string
- Added null check to remoteUserId in EOSTransportManager.CloseConnection in response to git issue #213
- Fixed lobby creation double callback invoke
- Automatically refresh Connect token if logged in with Auth interface
- Added error checking for EOSTransport packets that are below minimum expected size
- (android): Add the missing meta files for eos_sdk.aar
- Press to talk UI setup in Lobby

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

### Fixed
- fix(android, lib): Check Android toggle on lib metafiles
- fix(android, lib): Copy metafiles during preprocess
- fix(pkg_dscrpt, android): Removed leftover meta file from deleting the extra aar file (#212)
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

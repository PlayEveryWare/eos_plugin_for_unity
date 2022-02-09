# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### Unreleased
### Added
### Changed
### Removed
### Fixed

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

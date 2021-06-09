# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

## [0.0.2-preview+3ef9b760830941306c28c6a6bcfcf0f53e745be2] - 2021-06-08
### Added
- New platforms 
- update to EOS SDK 1.12
- P2P Samples
- Add methods to EOSManager to make it easier to fetch EOS interfaces
- Wrapper class for generating C# friendly Enumerators
- Ability to load EOS Config from a custom DLL on windows

### Changed
- changed namespace callbacks are in for the various login things in the EOSManager
- The various plugin managers have been moved to the Samples directory to make them easier to edit.
- Changed PlayerManager => FriendManager
- UILoginMenu sample now logs in with an explicit call to a Connect Login type
- UIAchievementMenu now dynamically creates all UI (i.e. supports 'unlimited' Achievements to be displayed)
- Update Create Lobbies for 1.12
- LeaderboardManager: Call to get Ranks/UserScores

### Removed
- EOSManager.GetProductIdFromAccountId

### Fixed
- Mitigate chance of hang when calling FreeLibrary in Unity Editor

## [0.0.1-preview+9ac8e3f56aae3afd6cbd36d4b1aa711c6971646b] - 2021-05-14

### Changed
- Fix UI prefabs and apply changes from scene
- Rename EOSPlayerManager -> EOSFriendsManager

- chore: move EOS SDK to another directory to make it easier to move it to it's own assembly.
- chore: rename the asmdef files to match the new branding of PlayEveryWare


## [0.0.1-preview+32890128fb8ae8f36e83ea59e417fcbd036ce37f] - 2021-05-12

### Added
- methods for enumerating various achievement related data. 
- method for fetching image as Texture2D for locked and unlocked achievements.

### Removed
- SimpleCameraController.cs

### Changed
- General Code cleanup; some things might become private.
- Move EOSManager, EOSConfig, into PlayEveryWare.EpicOnlineServices namespace.
- add support for installable samples from the UPM Package Manager.


## [0.0.1-preview+1334ed1a5ea6c30f915bca0290c12abc94e179d0] - 2021-05-10

### Added
- Auth: Add support for logging in with persistent tokens.
- Auth: Add Support for logging in with Connect API Only.
- Leaderboards
- EOSManager Utility: EpicLauncher helper method added for getting the options passed to the client. May be used for various Connect Login things.
- DLLHandle can now load single functions from the DLL it's managing.
- Player Data Storage 
- Sessions
- Samples: Leaderboard sample code
- Samples: Add Scene Selector
- Samples: Session UI
- Samples: Player Data Storage UI

### Changed
- Chore: Code Cleanup
- Chore: Documentation
- Chore: Spell check
- C# Code now also reads the encryption key from the JSON config file
- modify some existing code to work for future 1.12 code drop (add 1.12 and define EOS_VERSION_1_12 to use)

### Removed

### Fixed
- Replaced debug GfxPluginNativeRender DLLs with release DLLs

### Security

## [0.0.1-preview+481942a5073bacc6d6f1e4c6c31d11f399ef2c2f] - 2021-04-29

### Added
- Added basic android documentation
- Added a better EOS UPM plugin readme
- Added missing art assets for samples

### Changed
- Moved some documentation to other locations.
- Plugin now creates the Platform Interface in the NativeRender and passed it to Unity in Windows Standalone builds

### Removed
- NONE

### Fixed
- Marked GfxPluginNativeRender-x86 to default load
- Marked GfxPluginNativeRender-x64 to default load

### Security
- NONE

## [0.0.1] - 2021-04-22
### Added
First code drop of all the EOS things

# vim: set spell:

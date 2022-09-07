// Copyright Epic Games, Inc. All Rights Reserved.
#pragma once

#include "eos_init.h"
#include "eos_metrics_types.h"
#include "eos_auth_types.h"
#include "eos_ecom_types.h"
#include "eos_ui_types.h"
#include "eos_friends_types.h"
#include "eos_presence_types.h"
#include "eos_p2p_types.h"
#include "eos_sessions_types.h"
#include "eos_lobby_types.h"
#include "eos_userinfo_types.h"
#include "eos_playerdatastorage_types.h"
#include "eos_titlestorage_types.h"
#include "eos_connect.h"
#include "eos_achievements_types.h"
#include "eos_stats_types.h"
#include "eos_leaderboards_types.h"
#include "eos_mods_types.h"
#include "eos_anticheatclient_types.h"
#include "eos_anticheatserver_types.h"
#include "eos_reports_types.h"
#include "eos_sanctions_types.h"
#include "eos_kws_types.h"
#include "eos_rtc_types.h"
#include "eos_rtc_admin_types.h"
#include "eos_progressionsnapshot_types.h"
#include "eos_custominvites_types.h"

/**
 * The Platform Instance is used to gain access to all other Epic Online Service interfaces and to drive internal operations through the Tick.
 * All Platform Instance calls take a handle of type EOS_HPlatform as the first parameter.
 * EOS_HPlatform handles are created by calling EOS_Platform_Create and subsequently released by calling EOS_Platform_Release.
 *
 * @see eos_init.h
 * @see EOS_Initialize
 * @see EOS_Platform_Create
 * @see EOS_Platform_Release
 * @see EOS_Shutdown
 */

/**
 * Notify the platform instance to do work. This function must be called frequently in order for the services provided by the SDK to properly
 * function. For tick-based applications, it is usually desirable to call this once per-tick.
 */
EOS_DECLARE_FUNC(void) EOS_Platform_Tick(EOS_HPlatform Handle);

/**
 * Get a handle to the Metrics Interface.
 * @return EOS_HMetrics handle
 *
 * @see eos_metrics.h
 * @see eos_metrics_types.h
 */
EOS_DECLARE_FUNC(EOS_HMetrics) EOS_Platform_GetMetricsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Auth Interface.
 * @return EOS_HAuth handle
 *
 * @see eos_auth.h
 * @see eos_auth_types.h
 */
EOS_DECLARE_FUNC(EOS_HAuth) EOS_Platform_GetAuthInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Connect Interface.
 * @return EOS_HConnect handle
 *
 * @see eos_connect.h
 * @see eos_connect_types.h
 */
EOS_DECLARE_FUNC(EOS_HConnect) EOS_Platform_GetConnectInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Ecom Interface.
 * @return EOS_HEcom handle
 *
 * @see eos_ecom.h
 * @see eos_ecom_types.h
 */
EOS_DECLARE_FUNC(EOS_HEcom) EOS_Platform_GetEcomInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the UI Interface.
 * @return EOS_HUI handle
 *
 * @see eos_ui.h
 * @see eos_ui_types.h
 */
EOS_DECLARE_FUNC(EOS_HUI) EOS_Platform_GetUIInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Friends Interface.
 * @return EOS_HFriends handle
 *
 * @see eos_friends.h
 * @see eos_friends_types.h
 */
EOS_DECLARE_FUNC(EOS_HFriends) EOS_Platform_GetFriendsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Presence Interface.
 * @return EOS_HPresence handle
 *
 * @see eos_presence.h
 * @see eos_presence_types.h
 */
EOS_DECLARE_FUNC(EOS_HPresence) EOS_Platform_GetPresenceInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Sessions Interface.
 * @return EOS_HSessions handle
 *
 * @see eos_sessions.h
 * @see eos_sessions_types.h
 */
EOS_DECLARE_FUNC(EOS_HSessions) EOS_Platform_GetSessionsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Lobby Interface.
 * @return EOS_HLobby handle
 *
 * @see eos_lobby.h
 * @see eos_lobby_types.h
 */
EOS_DECLARE_FUNC(EOS_HLobby) EOS_Platform_GetLobbyInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the UserInfo Interface.
 * @return EOS_HUserInfo handle
 *
 * @see eos_userinfo.h
 * @see eos_userinfo_types.h
 */
EOS_DECLARE_FUNC(EOS_HUserInfo) EOS_Platform_GetUserInfoInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Peer-to-Peer Networking Interface.
 * @return EOS_HP2P handle
 *
 * @see eos_p2p.h
 * @see eos_p2p_types.h
 */
EOS_DECLARE_FUNC(EOS_HP2P) EOS_Platform_GetP2PInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Real Time Communications Interface (RTC).
 * From the RTC interface you can retrieve the handle to the audio interface (RTCAudio), which is a component of RTC.
 * @return EOS_HRTC handle
 *
 * @see EOS_RTC_GetAudioInterface
 * @see eos_rtc.h
 * @see eos_rtc_types.h
 */
EOS_DECLARE_FUNC(EOS_HRTC) EOS_Platform_GetRTCInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the RTC Admin interface
 * @return EOS_HRTCAdmin handle
 *
 * @see eos_rtc_admin.h
 * @see eos_admin_types.h
 */
EOS_DECLARE_FUNC(EOS_HRTCAdmin) EOS_Platform_GetRTCAdminInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the PlayerDataStorage Interface.
 * @return EOS_HPlayerDataStorage handle
 *
 * @see eos_playerdatastorage.h
 * @see eos_playerdatastorage_types.h
 */
EOS_DECLARE_FUNC(EOS_HPlayerDataStorage) EOS_Platform_GetPlayerDataStorageInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the TitleStorage Interface.
 * @return EOS_HTitleStorage handle
 *
 * @see eos_titlestorage.h
 * @see eos_titlestorage_types.h
 */
EOS_DECLARE_FUNC(EOS_HTitleStorage) EOS_Platform_GetTitleStorageInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Achievements Interface.
 * @return EOS_HAchievements handle
 *
 * @see eos_achievements.h
 * @see eos_achievements_types.h
 */
EOS_DECLARE_FUNC(EOS_HAchievements) EOS_Platform_GetAchievementsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Stats Interface.
 * @return EOS_HStats handle
 *
 * @see eos_stats.h
 * @see eos_stats_types.h
 */
EOS_DECLARE_FUNC(EOS_HStats) EOS_Platform_GetStatsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Leaderboards Interface.
 * @return EOS_HLeaderboards handle
 *
 * @see eos_leaderboards.h
 * @see eos_leaderboards_types.h
 */
EOS_DECLARE_FUNC(EOS_HLeaderboards) EOS_Platform_GetLeaderboardsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Mods Interface.
 * @return EOS_HMods handle
 *
 * @see eos_mods.h
 * @see eos_mods_types.h
 */
EOS_DECLARE_FUNC(EOS_HMods) EOS_Platform_GetModsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Anti-Cheat Client Interface.
 * @return EOS_HAntiCheatClient handle
 *
 * @see eos_anticheatclient.h
 * @see eos_anticheatclient_types.h
 */
EOS_DECLARE_FUNC(EOS_HAntiCheatClient) EOS_Platform_GetAntiCheatClientInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Anti-Cheat Server Interface.
 * @return EOS_HAntiCheatServer handle
 *
 * @see eos_anticheatserver.h
 * @see eos_anticheatserver_types.h
 */
EOS_DECLARE_FUNC(EOS_HAntiCheatServer) EOS_Platform_GetAntiCheatServerInterface(EOS_HPlatform Handle);

/**
 * Get the active country code that the SDK will send to services which require it.
 * This returns the override value otherwise it will use the country code of the given user.
 * This is currently used for determining pricing.
 * Get a handle to the ProgressionSnapshot Interface.
 * @return EOS_HProgressionSnapshot handle
 *
 * @see eos_progressionsnapshot.h
 * @see eos_progressionsnapshot_types.h
 */
EOS_DECLARE_FUNC(EOS_HProgressionSnapshot) EOS_Platform_GetProgressionSnapshotInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Reports Interface.
 * @return EOS_HReports handle
 *
 * @see eos_reports.h
 * @see eos_reports_types.h
 */
EOS_DECLARE_FUNC(EOS_HReports) EOS_Platform_GetReportsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Sanctions Interface.
 * @return EOS_HSanctions handle
 *
 * @see eos_sanctions.h
 * @see eos_sanctions_types.h
 */
EOS_DECLARE_FUNC(EOS_HSanctions) EOS_Platform_GetSanctionsInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Kids Web Service Interface.
 * @return EOS_HKWS handle
 *
 * @see eos_kws.h
 * @see eos_kws_types.h
 */
EOS_DECLARE_FUNC(EOS_HKWS) EOS_Platform_GetKWSInterface(EOS_HPlatform Handle);

/**
 * Get a handle to the Custom Invites Interface.
 * @return EOS_HCustomInvites handle
 *
 * @see eos_custominvites.h
 * @see eos_custominvites_types.h
 */
EOS_DECLARE_FUNC(EOS_HCustomInvites) EOS_Platform_GetCustomInvitesInterface(EOS_HPlatform Handle);

/**
 * This only will return the value set as the override otherwise EOS_NotFound is returned.
 * This is not currently used for anything internally.
 *
 * @param LocalUserId The account to use for lookup if no override exists.
 * @param OutBuffer The buffer into which the character data should be written.  The buffer must be long enough to hold a string of EOS_COUNTRYCODE_MAX_LENGTH.
 * @param InOutBufferLength The size of the OutBuffer in characters.
 *                          The input buffer should include enough space to be null-terminated.
 *                          When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
 *
 * @return An EOS_EResult that indicates whether the active country code string was copied into the OutBuffer.
 *         EOS_Success if the information is available and passed out in OutBuffer
 *         EOS_InvalidParameters if you pass a null pointer for the out parameter
 *         EOS_NotFound if there is not an override country code for the user.
 *         EOS_LimitExceeded - The OutBuffer is not large enough to receive the country code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
 *
 * @see eos_ecom.h
 * @see EOS_COUNTRYCODE_MAX_LENGTH
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_GetActiveCountryCode(EOS_HPlatform Handle, EOS_EpicAccountId LocalUserId, char* OutBuffer, int32_t* InOutBufferLength);

/**
 * Get the active locale code that the SDK will send to services which require it.
 * This returns the override value otherwise it will use the locale code of the given user.
 * This is used for localization. This follows ISO 639.
 *
 * @param LocalUserId The account to use for lookup if no override exists.
 * @param OutBuffer The buffer into which the character data should be written.  The buffer must be long enough to hold a string of EOS_LOCALECODE_MAX_LENGTH.
 * @param InOutBufferLength The size of the OutBuffer in characters.
 *                          The input buffer should include enough space to be null-terminated.
 *                          When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
 *
 * @return An EOS_EResult that indicates whether the active locale code string was copied into the OutBuffer.
 *         EOS_Success if the information is available and passed out in OutBuffer
 *         EOS_InvalidParameters if you pass a null pointer for the out parameter
 *         EOS_NotFound if there is neither an override nor an available locale code for the user.
 *         EOS_LimitExceeded - The OutBuffer is not large enough to receive the locale code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
 *
 * @see eos_ecom.h
 * @see EOS_LOCALECODE_MAX_LENGTH
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_GetActiveLocaleCode(EOS_HPlatform Handle, EOS_EpicAccountId LocalUserId, char* OutBuffer, int32_t* InOutBufferLength);

/**
 * Get the override country code that the SDK will send to services which require it.
 * This is not currently used for anything internally.
 *
 * @param OutBuffer The buffer into which the character data should be written.  The buffer must be long enough to hold a string of EOS_COUNTRYCODE_MAX_LENGTH.
 * @param InOutBufferLength The size of the OutBuffer in characters.
 *                          The input buffer should include enough space to be null-terminated.
 *                          When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
 *
 * @return An EOS_EResult that indicates whether the override country code string was copied into the OutBuffer.
 *         EOS_Success if the information is available and passed out in OutBuffer
 *         EOS_InvalidParameters if you pass a null pointer for the out parameter
 *         EOS_LimitExceeded - The OutBuffer is not large enough to receive the country code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
 *
 * @see eos_ecom.h
 * @see EOS_COUNTRYCODE_MAX_LENGTH
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_GetOverrideCountryCode(EOS_HPlatform Handle, char* OutBuffer, int32_t* InOutBufferLength);

/**
 * Get the override locale code that the SDK will send to services which require it.
 * This is used for localization. This follows ISO 639.
 *
 * @param OutBuffer The buffer into which the character data should be written.  The buffer must be long enough to hold a string of EOS_LOCALECODE_MAX_LENGTH.
 * @param InOutBufferLength The size of the OutBuffer in characters.
 *                          The input buffer should include enough space to be null-terminated.
 *                          When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
 *
 * @return An EOS_EResult that indicates whether the override locale code string was copied into the OutBuffer.
 *         EOS_Success if the information is available and passed out in OutBuffer
 *         EOS_InvalidParameters if you pass a null pointer for the out parameter
 *         EOS_LimitExceeded - The OutBuffer is not large enough to receive the locale code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
 *
 * @see eos_ecom.h
 * @see EOS_LOCALECODE_MAX_LENGTH
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_GetOverrideLocaleCode(EOS_HPlatform Handle, char* OutBuffer, int32_t* InOutBufferLength);

/**
 * Set the override country code that the SDK will send to services which require it.
 * This is not currently used for anything internally.
 *
 * @return An EOS_EResult that indicates whether the override country code string was saved.
 *         EOS_Success if the country code was overridden
 *         EOS_InvalidParameters if you pass an invalid country code
 *
 * @see eos_ecom.h
 * @see EOS_COUNTRYCODE_MAX_LENGTH
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_SetOverrideCountryCode(EOS_HPlatform Handle, const char* NewCountryCode);

/**
 * Set the override locale code that the SDK will send to services which require it.
 * This is used for localization. This follows ISO 639.
 *
 * @return An EOS_EResult that indicates whether the override locale code string was saved.
 *         EOS_Success if the locale code was overridden
 *         EOS_InvalidParameters if you pass an invalid locale code
 *
 * @see eos_ecom.h
 * @see EOS_LOCALECODE_MAX_LENGTH
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_SetOverrideLocaleCode(EOS_HPlatform Handle, const char* NewLocaleCode);

/**
 * Checks if the app was launched through the Epic Launcher, and relaunches it through the Epic Launcher if it wasn't.
 *
 * @return An EOS_EResult is returned to indicate success or an error.
 *
 * EOS_Success is returned if the app is being restarted. You should quit your process as soon as possible.
 * EOS_NoChange is returned if the app was already launched through the Epic Launcher, and no action needs to be taken.
 * EOS_UnexpectedError is returned if the LauncherCheck module failed to initialize, or the module tried and failed to restart the app.
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_CheckForLauncherAndRestart(EOS_HPlatform Handle);

/**
 * Windows only.
 * Checks that the application is ready to use desktop crossplay functionality, with the necessary prerequisites having been met.
 *
 * This function verifies that the application was launched through the Bootstrapper application,
 * the redistributable service has been installed and is running in the background,
 * and that the overlay has been loaded successfully.
 *
 * On Windows, the desktop crossplay functionality is required to use Epic accounts login
 * with applications that are distributed outside the Epic Games Store.
 *
 * @param Options input structure that specifies the API version.
 * @param OutDesktopCrossplayStatusInfo output structure to receive the desktop crossplay status information.
 *
 * @return An EOS_EResult is returned to indicate success or an error.
 *		   EOS_NotImplemented is returned on non-Windows platforms.
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_GetDesktopCrossplayStatus(EOS_HPlatform Handle, const EOS_Platform_GetDesktopCrossplayStatusOptions* Options, EOS_Platform_GetDesktopCrossplayStatusInfo* OutDesktopCrossplayStatusInfo);

/**
 * Notify a change in application state.
 *
 * @note Calling SetApplicationStatus must happen before Tick when foregrounding for the cases where we won't get the background notification.
 *
 * @param NewStatus The new status for the application.
 *
 * @return An EOS_EResult that indicates whether we changed the application status successfully.
 *         EOS_Success if the application was changed successfully.
 *         EOS_InvalidParameters if the value of NewStatus is invalid.
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_SetApplicationStatus(EOS_HPlatform Handle, const EOS_EApplicationStatus NewStatus);

/**
 * Retrieves the current application state as told to the SDK by the application.
 *
 * @return The current application status.
 */
EOS_DECLARE_FUNC(EOS_EApplicationStatus) EOS_Platform_GetApplicationStatus(EOS_HPlatform Handle);

/**
 * Notify a change in network state.
 *
 * @param NewStatus The new network status.
 *
 * @return An EOS_EResult that indicates whether we changed the network status successfully.
 *         EOS_Success if the network was changed successfully.
 *         EOS_InvalidParameters if the value of NewStatus is invalid.
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_Platform_SetNetworkStatus(EOS_HPlatform Handle, const EOS_ENetworkStatus NewStatus);

/**
 * Retrieves the current network state as told to the SDK by the application.
 *
 * @return The current network status.
 */
EOS_DECLARE_FUNC(EOS_ENetworkStatus) EOS_Platform_GetNetworkStatus(EOS_HPlatform Handle);

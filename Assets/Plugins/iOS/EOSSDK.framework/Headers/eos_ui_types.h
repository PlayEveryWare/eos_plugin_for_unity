// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "eos_common.h"

#pragma pack(push, 8)

/** Handle to the UI interface */
EXTERN_C typedef struct EOS_UIHandle* EOS_HUI;

/** ID representing a specific UI event. */
EXTERN_C typedef uint64_t EOS_UI_EventId;
#define EOS_UI_EVENTID_INVALID 0

/** The most recent version of the EOS_UI_ShowFriends API. */
#define EOS_UI_SHOWFRIENDS_API_LATEST 1

/**
 * Input parameters for the EOS_UI_ShowFriends function.
 */
EOS_STRUCT(EOS_UI_ShowFriendsOptions, (
	/** API Version: Set this to EOS_UI_SHOWFRIENDS_API_LATEST. */
	int32_t ApiVersion;
	/** The Epic Account ID of the user whose friend list is being shown. */
	EOS_EpicAccountId LocalUserId;
));

/**
 * Output parameters for the EOS_UI_ShowFriends function.
 */
EOS_STRUCT(EOS_UI_ShowFriendsCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_UI_ShowFriends */
	void* ClientData;
	/** The Epic Account ID of the user whose friend list is being shown. */
	EOS_EpicAccountId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_UI_ShowFriends
 * @param Data A EOS_UI_ShowFriendsCallbackInfo containing the output information and result
 */
EOS_DECLARE_CALLBACK(EOS_UI_OnShowFriendsCallback, const EOS_UI_ShowFriendsCallbackInfo* Data);

/** The most recent version of the EOS_UI_HideFriends API. */
#define EOS_UI_HIDEFRIENDS_API_LATEST 1

/**
 * Input parameters for the EOS_UI_HideFriends function.
 */
EOS_STRUCT(EOS_UI_HideFriendsOptions, (
	/** API Version: Set this to EOS_UI_HIDEFRIENDS_API_LATEST. */
	int32_t ApiVersion;
	/** The Epic Account ID of the user whose friend list is being shown. */
	EOS_EpicAccountId LocalUserId;
));

/**
 * Output parameters for the EOS_UI_HideFriends function.
 */
EOS_STRUCT(EOS_UI_HideFriendsCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_UI_HideFriends */
	void* ClientData;
	/** The Epic Account ID of the user whose friend list is being shown. */
	EOS_EpicAccountId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_UI_HideFriends
 * @param Data A EOS_UI_HideFriendsCallbackInfo containing the output information and result
 */
EOS_DECLARE_CALLBACK(EOS_UI_OnHideFriendsCallback, const EOS_UI_HideFriendsCallbackInfo* Data);

/** The most recent version of the EOS_UI_GetFriendsVisible API. */
#define EOS_UI_GETFRIENDSVISIBLE_API_LATEST 1

/**
 * Input parameters for the EOS_UI_GetFriendsVisible function.
 */
EOS_STRUCT(EOS_UI_GetFriendsVisibleOptions, (
	/** API Version: Set this to EOS_UI_GETFRIENDSVISIBLE_API_LATEST. */
	int32_t ApiVersion;
	/** The Epic Account ID of the user whose overlay is being checked. */
	EOS_EpicAccountId LocalUserId;
));

/** The most recent version of the EOS_UI_GetFriendsExclusiveInput API. */
#define EOS_UI_GETFRIENDSEXCLUSIVEINPUT_API_LATEST 1

/**
 * Input parameters for the EOS_UI_GetFriendsExclusiveInput function.
 */
EOS_STRUCT(EOS_UI_GetFriendsExclusiveInputOptions, (
	/** API Version: Set this to EOS_UI_GETFRIENDSEXCLUSIVEINPUT_API_LATEST. */
	int32_t ApiVersion;
	/** The Epic Account ID of the user whose overlay is being checked. */
	EOS_EpicAccountId LocalUserId;
));

/** The most recent version of the EOS_UI_AddNotifyDisplaySettingsUpdated API. */
#define EOS_UI_ADDNOTIFYDISPLAYSETTINGSUPDATED_API_LATEST 1

/**
 * Input parameters for the EOS_UI_AddNotifyDisplaySettingsUpdated function.
 */
EOS_STRUCT(EOS_UI_AddNotifyDisplaySettingsUpdatedOptions, (
	/** API Version: Set this to EOS_UI_ADDNOTIFYDISPLAYSETTINGSUPDATED_API_LATEST. */
	int32_t ApiVersion;
));

EOS_STRUCT(EOS_UI_OnDisplaySettingsUpdatedCallbackInfo, (
	/** Context that was passed into EOS_UI_AddNotifyDisplaySettingsUpdated */
	void* ClientData;
	/** True when any portion of the overlay is visible. */
	EOS_Bool bIsVisible;
	/**
	 * True when the overlay has switched to exclusive input mode. 
	 * While in exclusive input mode, no keyboard or mouse input will be sent to the game.
	 */
	EOS_Bool bIsExclusiveInput;
));

/**
 * Function prototype definition for callbacks passed to EOS_UI_AddNotifyDisplaySettingsUpdated 
 * @param Data A EOS_UI_OnDisplaySettingsUpdatedCallbackInfo containing the current display state.
 */
EOS_DECLARE_CALLBACK(EOS_UI_OnDisplaySettingsUpdatedCallback, const EOS_UI_OnDisplaySettingsUpdatedCallbackInfo* Data);

/**
 * Enum flags for storing a key combination. The low 16 bits are the key type, and modifiers are 
 * stored in the next significant bits 
 */
#define EOS_UI_KEY_CONSTANT(Prefix, Name, Value) Prefix ## Name = Value,
#define EOS_UI_KEY_MODIFIER(Prefix, Name, Value) Prefix ## Name = Value,
#define EOS_UI_KEY_MODIFIER_LAST(Prefix, Name, Value) Prefix ## Name = Value
#define EOS_UI_KEY_ENTRY_FIRST(Prefix, Name, Value) Prefix ## Name = Value,
#define EOS_UI_KEY_ENTRY(Prefix, Name) Prefix ## Name,
#define EOS_UI_KEY_CONSTANT_LAST(Prefix, Name) Prefix ## Name 
EOS_ENUM_START(EOS_UI_EKeyCombination)
#include "eos_ui_keys.h"
EOS_ENUM_END(EOS_UI_EKeyCombination);
EOS_ENUM_BOOLEAN_OPERATORS(EOS_UI_EKeyCombination)
#undef EOS_UI_KEY_CONSTANT
#undef EOS_UI_KEY_MODIFIER
#undef EOS_UI_KEY_MODIFIER_LAST
#undef EOS_UI_KEY_ENTRY_FIRST
#undef EOS_UI_KEY_ENTRY
#undef EOS_UI_KEY_CONSTANT_LAST

/** The most recent version of the EOS_UI_SetToggleFriendsKey API. */
#define EOS_UI_SETTOGGLEFRIENDSKEY_API_LATEST 1

/**
 * Input parameters for the EOS_UI_SetToggleFriendsKey function.
 */
EOS_STRUCT(EOS_UI_SetToggleFriendsKeyOptions, (
	/** API Version: Set this to EOS_UI_SETTOGGLEFRIENDSKEY_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * The new key combination which will be used to toggle the friends overlay.
	 * The combination can be any set of modifiers and one key.
	 * A value of EOS_UIK_None will cause the key to revert to the default.
	 */
	EOS_UI_EKeyCombination KeyCombination;
));

/** The most recent version of the EOS_UI_GetToggleFriendsKey API. */
#define EOS_UI_GETTOGGLEFRIENDSKEY_API_LATEST 1

/**
 * Input parameters for the EOS_UI_GetToggleFriendsKey function.
 */
EOS_STRUCT(EOS_UI_GetToggleFriendsKeyOptions, (
	/** API Version: Set this to EOS_UI_GETTOGGLEFRIENDSKEY_API_LATEST. */
	int32_t ApiVersion;
));

/**
 * Notification locations to be used to set the preference
 * for pop-up.
 *
 * @see EOS_UI_SetDisplayPreference
 */
EOS_ENUM(EOS_UI_ENotificationLocation,
	EOS_UNL_TopLeft,
	EOS_UNL_TopRight,
	EOS_UNL_BottomLeft,
	EOS_UNL_BottomRight
);

/** The most recent version of the EOS_UI_SetDisplayPreference API. */
#define EOS_UI_SETDISPLAYPREFERENCE_API_LATEST 1

/**
 * Input parameters for the EOS_UI_SetDisplayPreference function.
 */
EOS_STRUCT(EOS_UI_SetDisplayPreferenceOptions, (
	/** API Version: Set this to EOS_UI_SETDISPLAYPREFERENCE_API_LATEST. */
	int32_t ApiVersion;
	/** Preference for notification pop-up locations. */
	EOS_UI_ENotificationLocation NotificationLocation;
));


/** The most recent version of the EOS_UI_AcknowledgeEventId API. */
#define EOS_UI_ACKNOWLEDGEEVENTID_API_LATEST 1

/** DEPRECATED! Use EOS_UI_ACKNOWLEDGEEVENTID_API_LATEST instead. */
#define EOS_UI_ACKNOWLEDGECORRELATIONID_API_LATEST EOS_UI_ACKNOWLEDGEEVENTID_API_LATEST

/**
 * Input parameters for the EOS_UI_AcknowledgeEventId.
 */
EOS_STRUCT(EOS_UI_AcknowledgeEventIdOptions, (
	/** API Version: Set this to EOS_UI_ACKNOWLEDGEEVENTID_API_LATEST. */
	int32_t ApiVersion;
	/** The ID being acknowledged. */
	EOS_UI_EventId UiEventId;
	/**
	 * The result to use for the acknowledgment. 
	 * When acknowledging EOS_Presence_JoinGameAcceptedCallbackInfo this should be the 
	 * result code from the JoinSession call.
	 */
	EOS_EResult Result;
));

/** The most recent version of the EOS_UI_ShowBlockPlayer API. */
#define EOS_UI_SHOWBLOCKPLAYER_API_LATEST 1

/**
 * Parameters for the EOS_UI_ShowBlockPlayer function.
 */
EOS_STRUCT(EOS_UI_ShowBlockPlayerOptions, (
	/** API Version: Set this to EOS_UI_SHOWBLOCKPLAYER_API_LATEST. */
	int32_t ApiVersion;	
	/** The Epic Online Services Account ID of the user who is requesting the Block. */
	EOS_EpicAccountId LocalUserId;
	/** The Epic Online Services Account ID of the user whose is being Blocked. */
	EOS_EpicAccountId TargetUserId;
));

/**
 * Output parameters for the EOS_UI_ShowBlockPlayer function.
 */
EOS_STRUCT(EOS_UI_OnShowBlockPlayerCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_UI_ShowBlockPlayer */
	void* ClientData;
	/** The Epic Online Services Account ID of the user who requested the block. */
	EOS_EpicAccountId LocalUserId;
	/** The Epic Online Services Account ID of the user who was to be blocked. */
	EOS_EpicAccountId TargetUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_UI_ShowBlockPlayer
 * @param Data A EOS_UI_OnShowBlockPlayerCallbackInfo containing the output information and result
 */
EOS_DECLARE_CALLBACK(EOS_UI_OnShowBlockPlayerCallback, const EOS_UI_OnShowBlockPlayerCallbackInfo* Data);

/** The most recent version of the EOS_UI_ShowReportPlayer API. */
#define EOS_UI_SHOWREPORTPLAYER_API_LATEST 1

/**
 * Parameters for the EOS_UI_ShowReportPlayer function.
 */
EOS_STRUCT(EOS_UI_ShowReportPlayerOptions, (
	/** API Version: Set this to EOS_UI_SHOWREPORTPLAYER_API_LATEST. */
	int32_t ApiVersion;
	/** The Epic Online Services Account ID of the user who is requesting the Report. */
	EOS_EpicAccountId LocalUserId;
	/** The Epic Online Services Account ID of the user whose is being Reported. */
	EOS_EpicAccountId TargetUserId;
));

/**
 * Output parameters for the EOS_UI_ShowReportPlayer function.
 */
EOS_STRUCT(EOS_UI_OnShowReportPlayerCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_UI_ShowReportPlayer */
	void* ClientData;
	/** The Epic Online Services Account ID of the user who requested the Report. */
	EOS_EpicAccountId LocalUserId;
	/** The Epic Online Services Account ID of the user who was to be Reported. */
	EOS_EpicAccountId TargetUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_UI_ShowReportPlayer
 * @param Data A EOS_UI_OnShowReportPlayerCallbackInfo containing the output information and result
 */
EOS_DECLARE_CALLBACK(EOS_UI_OnShowReportPlayerCallback, const EOS_UI_OnShowReportPlayerCallbackInfo* Data);

/** The most recent version of the EOS_UI_PauseSocialOverlay API. */
#define EOS_UI_PAUSESOCIALOVERLAY_API_LATEST 1

/**
 * Input parameters for the EOS_UI_PauseSocialOverlay function.
 */
EOS_STRUCT(EOS_UI_PauseSocialOverlayOptions, (
	/** API Version: Set this to EOS_UI_PAUSESOCIALOVERLAY_API_LATEST. */
	int32_t ApiVersion;
	/** The desired bIsPaused state of the overlay.  */
	EOS_Bool bIsPaused;
));

/** The most recent version of the EOS_UI_IsSocialOverlayPaused API. */
#define EOS_UI_ISSOCIALOVERLAYPAUSED_API_LATEST 1

/**
 * Input parameters for the EOS_UI_IsSocialOverlayPaused function.
 */
EOS_STRUCT(EOS_UI_IsSocialOverlayPausedOptions, (
	/** API Version: Set this to EOS_UI_ISSOCIALOVERLAYPAUSED_API_LATEST. */
	int32_t ApiVersion;
));

/** The most recent version of the EOS_UI_ReportKeyEvent API. */
#define EOS_UI_REPORTKEYEVENT_API_LATEST 1

/**
 * Input parameters for the EOS_UI_ReportKeyEvent function.
 */
EOS_STRUCT(EOS_UI_ReportKeyEventOptions, (
	/** API Version: Set this to EOS_UI_REPORTKEYEVENT_API_LATEST. */
	int32_t ApiVersion;
	/** The input data pushed to the SDK. */
	const void* PlatformSpecificInputData;
));

/** The most recent version of the EOS_UI_PrePresent API. */
#define EOS_UI_PREPRESENT_API_LATEST 1

/**
 * Parameters for the EOS_UI_PrePresent function.
 */
EOS_STRUCT(EOS_UI_PrePresentOptions, (
	/** API Version: Set this to EOS_UI_PREPRESENT_API_LATEST. */
	int32_t ApiVersion;
	/** Platform specific data. */
	const void* PlatformSpecificData;
));

#pragma pack(pop)

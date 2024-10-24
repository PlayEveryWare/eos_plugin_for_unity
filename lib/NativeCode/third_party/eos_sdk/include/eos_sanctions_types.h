// Copyright Epic Games, Inc. All Rights Reserved.
#pragma once

#include "eos_common.h"

#pragma pack(push, 8)

EOS_EXTERN_C typedef struct EOS_SanctionsHandle* EOS_HSanctions;

/** The most recent version of the EOS_Sanctions_PlayerSanction struct. */
#define EOS_SANCTIONS_PLAYERSANCTION_API_LATEST 2

/**
 * Contains information about a single player sanction.
 */
EOS_STRUCT(EOS_Sanctions_PlayerSanction, (
	/** API Version: Set this to EOS_SANCTIONS_PLAYERSANCTION_API_LATEST. */
	int32_t ApiVersion;
	/** The POSIX timestamp when the sanction was placed */
	int64_t TimePlaced;
	/** The action associated with this sanction */
	const char* Action;
	/** The POSIX timestamp when the sanction will expire. If the sanction is permanent, this will be 0. */
	int64_t TimeExpires;
	/** A unique identifier for this specific sanction */
	const char* ReferenceId;
));

/** The most recent version of the EOS_Sanctions_QueryActivePlayerSanctions API. */
#define EOS_SANCTIONS_QUERYACTIVEPLAYERSANCTIONS_API_LATEST 2

/**
 * Input parameters for the EOS_Sanctions_QueryActivePlayerSanctions API.
 */
EOS_STRUCT(EOS_Sanctions_QueryActivePlayerSanctionsOptions, (
	/** API Version: Set this to EOS_SANCTIONS_QUERYACTIVEPLAYERSANCTIONS_API_LATEST. */
	int32_t ApiVersion;
	/** Product User ID of the user whose active sanctions are to be retrieved. */
	EOS_ProductUserId TargetUserId;
	/** The Product User ID of the local user who initiated this request. Dedicated servers should set this to null. */
	EOS_ProductUserId LocalUserId;
));

/**
 * Output parameters for the EOS_Sanctions_QueryActivePlayerSanctions function.
 */
EOS_STRUCT(EOS_Sanctions_QueryActivePlayerSanctionsCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Sanctions_QueryActivePlayerSanctions. */
	void* ClientData;
	/** Target Product User ID that was passed to EOS_Sanctions_QueryActivePlayerSanctions. */
	EOS_ProductUserId TargetUserId;
	/** The Product User ID of the local user who initiated this request, if applicable. */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_Sanctions_QueryActivePlayerSanctions
 * @param Data A EOS_Sanctions_QueryActivePlayerSanctionsCallbackInfo containing the output information and result
 */
EOS_DECLARE_CALLBACK(EOS_Sanctions_OnQueryActivePlayerSanctionsCallback, const EOS_Sanctions_QueryActivePlayerSanctionsCallbackInfo* Data);

/** The most recent version of the EOS_Sanctions_GetPlayerSanctionCount API. */
#define EOS_SANCTIONS_GETPLAYERSANCTIONCOUNT_API_LATEST 1

/**
 * Input parameters for the EOS_Sanctions_GetPlayerSanctionCount function.
 */
EOS_STRUCT(EOS_Sanctions_GetPlayerSanctionCountOptions, (
	/** API Version: Set this to EOS_SANCTIONS_GETPLAYERSANCTIONCOUNT_API_LATEST. */
	int32_t ApiVersion;
	/** Product User ID of the user whose sanction count should be returned */
	EOS_ProductUserId TargetUserId;
));

/** The most recent version of the EOS_Sanctions_CopyPlayerSanctionByIndex API. */
#define EOS_SANCTIONS_COPYPLAYERSANCTIONBYINDEX_API_LATEST 1

/**
 * Input parameters for the EOS_Sanctions_CopyPlayerSanctionByIndex function
 */
EOS_STRUCT(EOS_Sanctions_CopyPlayerSanctionByIndexOptions, (
	/** API Version: Set this to EOS_SANCTIONS_COPYPLAYERSANCTIONBYINDEX_API_LATEST. */
	int32_t ApiVersion;
	/** Product User ID of the user whose active sanctions are to be copied */
	EOS_ProductUserId TargetUserId;
	/** Index of the sanction to retrieve from the cache */
	uint32_t SanctionIndex;
));

/**
 * Release the memory associated with a player sanction.
 * This must be called on data retrieved from EOS_Sanctions_CopyPlayerSanctionByIndex.
 *
 * @param Sanction - The sanction data to release.
 *
 * @see EOS_Sanctions_PlayerSanction
 * @see EOS_Sanctions_CopyPlayerSanctionByIndex
 */
EOS_DECLARE_FUNC(void) EOS_Sanctions_PlayerSanction_Release(EOS_Sanctions_PlayerSanction* Sanction);

/** Sanction appeal reason codes */
EOS_ENUM(EOS_ESanctionAppealReason,
	/** Not used */
	EOS_SAR_Invalid = 0,
	/** Incorrectly placed sanction */
	EOS_SAR_IncorrectSanction = 1,
	/** The account was compromised, typically this means stolen */
	EOS_SAR_CompromisedAccount = 2,
	/** The punishment is considered too severe by the user */
	EOS_SAR_UnfairPunishment = 3,
	/** The user admits to rulebreaking, but still appeals for forgiveness */
	EOS_SAR_AppealForForgiveness = 4
);

/** The most recent version of the EOS_Sanctions_CreatePlayerSanctionAppeal struct. */
#define EOS_SANCTIONS_CREATEPLAYERSANCTIONAPPEAL_API_LATEST 1

/**
 * Input parameters for the EOS_Sanctions_CreatePlayerSanctionAppeal function.
 */
EOS_STRUCT(EOS_Sanctions_CreatePlayerSanctionAppealOptions, (
	/** API Version: Set this to EOS_SANCTIONS_CREATEPLAYERSANCTIONAPPEAL_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the local user sending their own sanction appeal. */
	EOS_ProductUserId LocalUserId;
	/** Reason code for the appeal. */
	EOS_ESanctionAppealReason Reason;
	/** A unique identifier for the specific sanction */
	const char* ReferenceId;
));

/**
 * Output parameters for the EOS_Sanctions_CreatePlayerSanctionAppealCallbackInfo function.
 */
EOS_STRUCT(EOS_Sanctions_CreatePlayerSanctionAppealCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Sanctions_CreatePlayerSanctionAppeal. */
	void* ClientData;
	/** A unique identifier for the specific sanction that was appealed */
	const char* ReferenceId;
));

/**
 * Function definition for callbacks passed to EOS_Sanctions_CreatePlayerSanctionAppeal.
 * @param Data - EOS_Sanctions_CreatePlayerSanctionAppealCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Sanctions_CreatePlayerSanctionAppealCallback, const EOS_Sanctions_CreatePlayerSanctionAppealCallbackInfo* Data);

#pragma pack(pop)

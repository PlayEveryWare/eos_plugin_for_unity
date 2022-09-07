// Copyright Epic Games, Inc. All Rights Reserved.
#pragma once

#include "eos_common.h"

#pragma pack(push, 8)

EXTERN_C typedef struct EOS_ConnectHandle* EOS_HConnect;

/** Max length of an external account ID in string form */
#define EOS_CONNECT_EXTERNAL_ACCOUNT_ID_MAX_LENGTH 256

/** The most recent version of the EOS_Connect_Credentials struct. */
#define EOS_CONNECT_CREDENTIALS_API_LATEST 1

/**
 * A structure that contains external login credentials.
 * 
 * This is part of the input structure EOS_Connect_LoginOptions.
 *
 * @see EOS_EExternalCredentialType
 * @see EOS_Connect_Login
 */
EOS_STRUCT(EOS_Connect_Credentials, (
	/** API Version: Set this to EOS_CONNECT_CREDENTIALS_API_LATEST. */
	int32_t ApiVersion;
	/** External token associated with the user logging in. */
	const char* Token;
	/** Type of external login; identifies the auth method to use. */
	EOS_EExternalCredentialType Type;
));

/** Max length of a display name, not including the terminating null. */
#define EOS_CONNECT_USERLOGININFO_DISPLAYNAME_MAX_LENGTH 32

/** The most recent version of the EOS_Connect_UserLoginInfo struct. */
#define EOS_CONNECT_USERLOGININFO_API_LATEST 1

/**
 * Additional information about the local user.
 *
 * As the information passed here is client-controlled and not part of the user authentication tokens,
 * it is only treated as non-authoritative informational data to be used by some of the feature services.
 * For example displaying player names in Leaderboards rankings.
 */
EOS_STRUCT(EOS_Connect_UserLoginInfo, (
	/** API Version: Set this to EOS_CONNECT_USERLOGININFO_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * The user's display name on the identity provider systems as UTF-8 encoded null-terminated string.
	 * The length of the name can be at maximum up to EOS_CONNECT_USERLOGININFO_DISPLAYNAME_MAX_LENGTH bytes.
	 */
	const char* DisplayName;
));

/** The most recent version of the EOS_Connect_Login API. */
#define EOS_CONNECT_LOGIN_API_LATEST 2

/**
 * Input parameters for the EOS_Connect_Login function.
 */
EOS_STRUCT(EOS_Connect_LoginOptions, (
	/** API Version: Set this to EOS_CONNECT_LOGIN_API_LATEST. */
	int32_t ApiVersion;
	/** Credentials specified for a given login method */
	const EOS_Connect_Credentials* Credentials;
	/**
	 * Additional non-authoritative information about the local user.
	 *
	 * This field is required to be set and only used when authenticating the user using Amazon, Apple, Google, Nintendo Account, Nintendo Service Account, Oculus or the Device ID feature login.
	 * When using other identity providers, set to NULL.
	 */
	const EOS_Connect_UserLoginInfo* UserLoginInfo;
));

/**
 * Output parameters for the EOS_Connect_Login function.
 */
EOS_STRUCT(EOS_Connect_LoginCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_Login. */
	void* ClientData;
	/** If login was successful, this is the Product User ID of the local player that logged in. */
	EOS_ProductUserId LocalUserId;
	/**
	 * If the user was not found with credentials passed into EOS_Connect_Login, 
	 * this continuance token can be passed to either EOS_Connect_CreateUser 
	 * or EOS_Connect_LinkAccount to continue the flow.
	 */
	EOS_ContinuanceToken ContinuanceToken;
));

/**
 * Function prototype definition for callbacks passed to EOS_Connect_Login.
 *
 * @param Data A EOS_Connect_LoginCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnLoginCallback, const EOS_Connect_LoginCallbackInfo* Data);

/** The most recent version of the EOS_Connect_CreateUser API. */
#define EOS_CONNECT_CREATEUSER_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_CreateUser function.
 */
EOS_STRUCT(EOS_Connect_CreateUserOptions, (
	/** API Version: Set this to EOS_CONNECT_CREATEUSER_API_LATEST. */
	int32_t ApiVersion;
	/** Continuance token from previous call to EOS_Connect_Login */
	EOS_ContinuanceToken ContinuanceToken;
));

/**
 * Output parameters for the EOS_Connect_CreateUser function.
 */
EOS_STRUCT(EOS_Connect_CreateUserCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_CreateUser. */
	void* ClientData;
	/** If the operation succeeded, this is the Product User ID of the local user who was created. */
	EOS_ProductUserId LocalUserId;
));

EOS_DECLARE_CALLBACK(EOS_Connect_OnCreateUserCallback, const EOS_Connect_CreateUserCallbackInfo* Data);

/** The most recent version of the EOS_Connect_LinkAccount API. */
#define EOS_CONNECT_LINKACCOUNT_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_LinkAccount function.
 */
EOS_STRUCT(EOS_Connect_LinkAccountOptions, (
	/** API Version: Set this to EOS_CONNECT_LINKACCOUNT_API_LATEST. */
	int32_t ApiVersion;
	/** The existing logged in product user for which to link the external account described by the continuance token. */
	EOS_ProductUserId LocalUserId;
	/** Continuance token from previous call to EOS_Connect_Login. */
	EOS_ContinuanceToken ContinuanceToken;
));

/**
 * Output parameters for the EOS_Connect_LinkAccount function.
 */
EOS_STRUCT(EOS_Connect_LinkAccountCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_LinkAccount. */
	void* ClientData;
	/** The Product User ID of the existing, logged-in user whose account was linked (on success). */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_Connect_LinkAccount.
 *
 * @param Data A EOS_Connect_LinkAccountCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnLinkAccountCallback, const EOS_Connect_LinkAccountCallbackInfo* Data);

/** The most recent version of the EOS_Connect_UnlinkAccount API. */
#define EOS_CONNECT_UNLINKACCOUNT_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_UnlinkAccount Function.
 */
EOS_STRUCT(EOS_Connect_UnlinkAccountOptions, (
	/** API Version: Set this to EOS_CONNECT_UNLINKACCOUNT_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * Existing logged in product user that is subject for the unlinking operation.
	 * The external account that was used to login to the product user will be unlinked from the owning keychain.
	 *
	 * On a successful operation, the product user will be logged out as the external account used to authenticate the user was unlinked from the owning keychain.
	 */
	EOS_ProductUserId LocalUserId;
));

/**
 * Output parameters for the EOS_Connect_UnlinkAccount Function.
 */
EOS_STRUCT(EOS_Connect_UnlinkAccountCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_UnlinkAccount. */
	void* ClientData;
	/**
	 * The product user that was subject for the unlinking operation.
	 *
	 * On a successful operation, the local authentication session for the product user will have been invalidated.
	 * As such, the LocalUserId value will no longer be valid in any context unless the user is logged into it again.
	 */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_Connect_UnlinkAccount.
 *
 * @param Data A EOS_Connect_UnlinkAccountCallbackInfo containing the output information and result
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnUnlinkAccountCallback, const EOS_Connect_UnlinkAccountCallbackInfo* Data);

/** The most recent version of the EOS_Connect_CreateDeviceId API. */
#define EOS_CONNECT_CREATEDEVICEID_API_LATEST 1

/** Max length of a device model name, not including the terminating null */
#define EOS_CONNECT_CREATEDEVICEID_DEVICEMODEL_MAX_LENGTH 64

/**
 * Input parameters for the EOS_Connect_CreateDeviceId function.
 */
EOS_STRUCT(EOS_Connect_CreateDeviceIdOptions, (
	/** API Version: Set this to EOS_CONNECT_CREATEDEVICEID_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * A freeform text description identifying the device type and model,
	 * which can be used in account linking management to allow the player
	 * and customer support to identify different devices linked to an EOS
	 * user keychain. For example 'iPhone 6S' or 'PC Windows'.
	 *
	 * The input string must be in UTF-8 character format, with a maximum
	 * length of 64 characters. Longer string will be silently truncated.
	 *
	 * This field is required to be present.
	 */
	const char* DeviceModel;
));

/**
 * Output parameters for the EOS_Connect_CreateDeviceId function.
 */
EOS_STRUCT(EOS_Connect_CreateDeviceIdCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_CreateDeviceId. */
	void* ClientData;
));

EOS_DECLARE_CALLBACK(EOS_Connect_OnCreateDeviceIdCallback, const EOS_Connect_CreateDeviceIdCallbackInfo* Data);

/** The most recent version of the EOS_Connect_DeleteDeviceId API. */
#define EOS_CONNECT_DELETEDEVICEID_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_DeleteDeviceId function.
 */
EOS_STRUCT(EOS_Connect_DeleteDeviceIdOptions, (
	/** API Version: Set this to EOS_CONNECT_DELETEDEVICEID_API_LATEST. */
	int32_t ApiVersion;
));

/**
 * Output parameters for the EOS_Connect_DeleteDeviceId function.
 */
EOS_STRUCT(EOS_Connect_DeleteDeviceIdCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_DeleteDeviceId */
	void* ClientData;
));

EOS_DECLARE_CALLBACK(EOS_Connect_OnDeleteDeviceIdCallback, const EOS_Connect_DeleteDeviceIdCallbackInfo* Data);

/** The most recent version of the EOS_Connect_TransferDeviceIdAccount API. */
#define EOS_CONNECT_TRANSFERDEVICEIDACCOUNT_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_TransferDeviceIdAccount Function.
 */
EOS_STRUCT(EOS_Connect_TransferDeviceIdAccountOptions, (
	/** API Version: Set this to EOS_CONNECT_TRANSFERDEVICEIDACCOUNT_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * The primary product user id, currently logged in, that is already associated with a real external user account (such as Epic Games, PlayStation(TM)Network, Xbox Live and other).
	 *
	 * The account linking keychain that owns this product user will be preserved and receive
	 * the Device ID login credentials under it.
	 */
	EOS_ProductUserId PrimaryLocalUserId;
	/**
	 * The product user id, currently logged in, that has been originally created using the anonymous local Device ID login type,
	 * and whose Device ID login will be transferred to the keychain of the PrimaryLocalUserId.
	 */
	EOS_ProductUserId LocalDeviceUserId;
	/**
	 * Specifies which EOS_ProductUserId (i.e. game progression) will be preserved in the operation.
	 *
	 * After a successful transfer operation, subsequent logins using the same external account or
	 * the same local Device ID login will return user session for the ProductUserIdToPreserve.
	 *
	 * Set to either PrimaryLocalUserId or LocalDeviceUserId.
	 */
	EOS_ProductUserId ProductUserIdToPreserve;
));

/**
 * Output parameters for the EOS_Connect_TransferDeviceIdAccount Function.
 */
EOS_STRUCT(EOS_Connect_TransferDeviceIdAccountCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_TransferDeviceIdAccount. */
	void* ClientData;
	/**
	 * The ProductUserIdToPreserve that was passed to the original EOS_Connect_TransferDeviceIdAccount call.
	 *
	 * On successful operation, this EOS_ProductUserId will have a valid authentication session
	 * and the other EOS_ProductUserId value has been discarded and lost forever.
	 *
	 * The application should remove any registered notification callbacks for the discarded EOS_ProductUserId as obsolete.
	 */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_Connect_TransferDeviceIdAccount.
 *
 * @param Data A EOS_Connect_TransferDeviceIdAccountCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnTransferDeviceIdAccountCallback, const EOS_Connect_TransferDeviceIdAccountCallbackInfo* Data);

/** The most recent version of the EOS_Connect_QueryExternalAccountMappings API. */
#define EOS_CONNECT_QUERYEXTERNALACCOUNTMAPPINGS_API_LATEST 1

/** Maximum number of account IDs that can be queried at once */
#define EOS_CONNECT_QUERYEXTERNALACCOUNTMAPPINGS_MAX_ACCOUNT_IDS 128

/**
 * Input parameters for the EOS_Connect_QueryExternalAccountMappings function.
 */
EOS_STRUCT(EOS_Connect_QueryExternalAccountMappingsOptions, (
	/** API Version: Set this to EOS_CONNECT_QUERYEXTERNALACCOUNTMAPPINGS_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the existing, logged-in user who is querying account mappings. */
	EOS_ProductUserId LocalUserId;
	/** External auth service supplying the account IDs in string form. */
	EOS_EExternalAccountType AccountIdType;
	/** An array of external account IDs to map to the product user ID representation. */
	const char** ExternalAccountIds;
	/** Number of account IDs to query. */
	uint32_t ExternalAccountIdCount;
));

/**
 * Output parameters for the EOS_Connect_QueryExternalAccountMappings function.
 */
EOS_STRUCT(EOS_Connect_QueryExternalAccountMappingsCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_QueryExternalAccountMappings. */
	void* ClientData;
	/** The Product User ID of the existing, logged-in user who made the request. */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_Connect_QueryExternalAccountMappings.
 *
 * @param Data A EOS_Connect_QueryExternalAccountMappingsCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnQueryExternalAccountMappingsCallback, const EOS_Connect_QueryExternalAccountMappingsCallbackInfo* Data);

/** The most recent version of the EOS_Connect_GetExternalAccountMapping API. */
#define EOS_CONNECT_GETEXTERNALACCOUNTMAPPING_API_LATEST 1

/** DEPRECATED! Use EOS_CONNECT_GETEXTERNALACCOUNTMAPPING_API_LATEST instead. */
#define EOS_CONNECT_GETEXTERNALACCOUNTMAPPINGS_API_LATEST EOS_CONNECT_GETEXTERNALACCOUNTMAPPING_API_LATEST

/**
 * Input parameters for the EOS_Connect_GetExternalAccountMapping function.
 */
EOS_STRUCT(EOS_Connect_GetExternalAccountMappingsOptions, (
	/** API Version: Set this to EOS_CONNECT_GETEXTERNALACCOUNTMAPPING_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the existing, logged-in user who is querying account mappings. */
	EOS_ProductUserId LocalUserId;
	/** External auth service supplying the account IDs in string form. */
	EOS_EExternalAccountType AccountIdType;
	/** Target user to retrieve the mapping for, as an external account ID. */
	const char* TargetExternalUserId;
));

/** The most recent version of the EOS_Connect_QueryProductUserIdMappings API. */
#define EOS_CONNECT_QUERYPRODUCTUSERIDMAPPINGS_API_LATEST 2

/**
 * Input parameters for the EOS_Connect_QueryProductUserIdMappings function.
 */
EOS_STRUCT(EOS_Connect_QueryProductUserIdMappingsOptions, (
	/** API Version: Set this to EOS_CONNECT_QUERYPRODUCTUSERIDMAPPINGS_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * Game Clients set this field to the Product User ID of the local authenticated user querying account mappings.
	 * Game Servers set this field to NULL. Usage is allowed given that the configured client policy for server credentials permit it.
	 */
	EOS_ProductUserId LocalUserId;
	/** Deprecated - all external mappings are included in this call, it is no longer necessary to specify this value. */
	EOS_EExternalAccountType AccountIdType_DEPRECATED;
	/** An array of Product User IDs to query for the given external account representation. */
	EOS_ProductUserId* ProductUserIds;
	/** Number of Product User IDs to query. */
	uint32_t ProductUserIdCount;
));

/**
 * Output parameters for the EOS_Connect_QueryProductUserIdMappings function.
 */
EOS_STRUCT(EOS_Connect_QueryProductUserIdMappingsCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_QueryProductUserIdMappings. */
	void* ClientData;
	/** The local Product User ID that was passed with the input options. */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for callbacks passed to EOS_Connect_QueryProductUserIdMappings.
 *
 * @param Data A EOS_Connect_QueryProductUserIdMappingsCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnQueryProductUserIdMappingsCallback, const EOS_Connect_QueryProductUserIdMappingsCallbackInfo* Data);

/** The most recent version of the EOS_Connect_GetProductUserIdMapping API. */
#define EOS_CONNECT_GETPRODUCTUSERIDMAPPING_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_GetProductUserIdMapping function.
 */
EOS_STRUCT(EOS_Connect_GetProductUserIdMappingOptions, (
	/** API Version: Set this to EOS_CONNECT_GETPRODUCTUSERIDMAPPING_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the existing, logged-in user that is querying account mappings. */
	EOS_ProductUserId LocalUserId;
	/** External auth service mapping to retrieve. */
	EOS_EExternalAccountType AccountIdType;
	/** The Product User ID of the user whose information is being requested. */
	EOS_ProductUserId TargetProductUserId;
));

/** The most recent version of the EOS_Connect_GetProductUserExternalAccountCount API. */
#define EOS_CONNECT_GETPRODUCTUSEREXTERNALACCOUNTCOUNT_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_GetProductUserExternalAccountCount function.
 */
EOS_STRUCT(EOS_Connect_GetProductUserExternalAccountCountOptions, (
	/** API Version: Set this to EOS_CONNECT_GETPRODUCTUSEREXTERNALACCOUNTCOUNT_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID to look for when getting external account info count from the cache. */
	EOS_ProductUserId TargetUserId;
));

/** The most recent version of the EOS_Connect_CopyProductUserExternalAccountByIndex API. */
#define EOS_CONNECT_COPYPRODUCTUSEREXTERNALACCOUNTBYINDEX_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_CopyProductUserExternalAccountByIndex function.
 */
EOS_STRUCT(EOS_Connect_CopyProductUserExternalAccountByIndexOptions, (
	/** API Version: Set this to EOS_CONNECT_COPYPRODUCTUSEREXTERNALACCOUNTBYINDEX_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID to look for when copying external account info from the cache. */
	EOS_ProductUserId TargetUserId;
	/** Index of the external account info to retrieve from the cache. */
	uint32_t ExternalAccountInfoIndex;
));

/** The most recent version of the EOS_Connect_CopyProductUserExternalAccountByAccountType API. */
#define EOS_CONNECT_COPYPRODUCTUSEREXTERNALACCOUNTBYACCOUNTTYPE_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_CopyProductUserExternalAccountByAccountType function.
 */
EOS_STRUCT(EOS_Connect_CopyProductUserExternalAccountByAccountTypeOptions, (
	/** API Version: Set this to EOS_CONNECT_COPYPRODUCTUSEREXTERNALACCOUNTBYACCOUNTTYPE_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID to look for when copying external account info from the cache. */
	EOS_ProductUserId TargetUserId;
	/** External auth service account type to look for when copying external account info from the cache. */
	EOS_EExternalAccountType AccountIdType;
));

/** The most recent version of the EOS_Connect_CopyProductUserExternalAccountByAccountId API. */
#define EOS_CONNECT_COPYPRODUCTUSEREXTERNALACCOUNTBYACCOUNTID_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_CopyProductUserExternalAccountByAccountId function.
 */
EOS_STRUCT(EOS_Connect_CopyProductUserExternalAccountByAccountIdOptions, (
	/** API Version: Set this to EOS_CONNECT_COPYPRODUCTUSEREXTERNALACCOUNTBYACCOUNTID_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID to look for when copying external account info from the cache. */
	EOS_ProductUserId TargetUserId;
	/** External auth service account ID to look for when copying external account info from the cache. */
	const char* AccountId;
));

/** The most recent version of the EOS_Connect_CopyProductUserInfo API. */
#define EOS_CONNECT_COPYPRODUCTUSERINFO_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_CopyProductUserInfo function.
 */
EOS_STRUCT(EOS_Connect_CopyProductUserInfoOptions, (
	/** API Version: Set this to EOS_CONNECT_COPYPRODUCTUSERINFO_API_LATEST. */
	int32_t ApiVersion;
	/** Product user ID to look for when copying external account info from the cache. */
	EOS_ProductUserId TargetUserId;
));

/** Timestamp value representing an undefined time for last login time. */
#define EOS_CONNECT_TIME_UNDEFINED -1

/** The most recent version of the EOS_Connect_ExternalAccountInfo struct. */
#define EOS_CONNECT_EXTERNALACCOUNTINFO_API_LATEST 1

/**
 * Contains information about an external account linked with a Product User ID.
 */
EOS_STRUCT(EOS_Connect_ExternalAccountInfo, (
	/** API Version: Set this to EOS_CONNECT_EXTERNALACCOUNTINFO_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the target user. */
	EOS_ProductUserId ProductUserId;
	/** Display name, can be null if not set. */
	const char* DisplayName;
	/**
	 * External account ID.
	 *
	 * May be set to an empty string if the AccountIdType of another user belongs
	 * to different account system than the local user's authenticated account.
	 * The availability of this field is dependent on account system specifics.
	 */
	const char* AccountId;
	/** The identity provider that owns the external account. */
	EOS_EExternalAccountType AccountIdType;
	/** The POSIX timestamp for the time the user last logged in, or EOS_CONNECT_TIME_UNDEFINED. */
	int64_t LastLoginTime;
));

/**
 * Release the memory associated with an external account info. This must be called on data retrieved from
 * EOS_Connect_CopyProductUserExternalAccountByIndex, EOS_Connect_CopyProductUserExternalAccountByAccountType,
 * EOS_Connect_CopyProductUserExternalAccountByAccountId or EOS_Connect_CopyProductUserInfo.
 *
 * @param ExternalAccountInfo The external account info data to release.
 *
 * @see EOS_Connect_CopyProductUserExternalAccountByIndex
 * @see EOS_Connect_CopyProductUserExternalAccountByAccountType
 * @see EOS_Connect_CopyProductUserExternalAccountByAccountId
 * @see EOS_Connect_CopyProductUserInfo
 */
EOS_DECLARE_FUNC(void) EOS_Connect_ExternalAccountInfo_Release(EOS_Connect_ExternalAccountInfo* ExternalAccountInfo);

/** The most recent version of the EOS_Connect_AddNotifyAuthExpiration API. */
#define EOS_CONNECT_ADDNOTIFYAUTHEXPIRATION_API_LATEST 1
/**
 * Structure containing information for the auth expiration notification callback.
 */
EOS_STRUCT(EOS_Connect_AddNotifyAuthExpirationOptions, (
	/** API Version: Set this to EOS_CONNECT_ADDNOTIFYAUTHEXPIRATION_API_LATEST. */
	int32_t ApiVersion;
));

/** The most recent version of the EOS_Connect_OnAuthExpirationCallback API. */
#define EOS_CONNECT_ONAUTHEXPIRATIONCALLBACK_API_LATEST 1

/**
 * Output parameters for the EOS_Connect_OnAuthExpirationCallback function.
 */
EOS_STRUCT(EOS_Connect_AuthExpirationCallbackInfo, (
	/** Context that was passed into EOS_Connect_AddNotifyAuthExpiration. */
	void* ClientData;
	/** The Product User ID of the local player whose status has changed. */
	EOS_ProductUserId LocalUserId;
));

/**
 * Function prototype definition for notifications that come from EOS_Connect_AddNotifyAuthExpiration.
 *
 * @param Data A EOS_Connect_AuthExpirationCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnAuthExpirationCallback, const EOS_Connect_AuthExpirationCallbackInfo* Data);


/** The most recent version of the EOS_Connect_AddNotifyLoginStatusChanged API. */
#define EOS_CONNECT_ADDNOTIFYLOGINSTATUSCHANGED_API_LATEST 1
/**
 * Structure containing information or the connect user login status change callback.
 */
EOS_STRUCT(EOS_Connect_AddNotifyLoginStatusChangedOptions, (
	/** API Version: Set this to EOS_CONNECT_ADDNOTIFYLOGINSTATUSCHANGED_API_LATEST. */
	int32_t ApiVersion;
));

/**
 * Output parameters for the EOS_Connect_OnLoginStatusChangedCallback function.
 */
EOS_STRUCT(EOS_Connect_LoginStatusChangedCallbackInfo, (
	/** Context that was passed into EOS_Connect_AddNotifyLoginStatusChanged. */
	void* ClientData;
	/** The Product User ID of the local player whose status has changed. */
	EOS_ProductUserId LocalUserId;
	/** The status prior to the change. */
	EOS_ELoginStatus PreviousStatus;
	/** The status at the time of the notification. */
	EOS_ELoginStatus CurrentStatus;
));

/**
 * Function prototype definition for notifications that come from EOS_Connect_AddNotifyLoginStatusChanged.
 *
 * @param Data A EOS_Connect_LoginStatusChangedCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnLoginStatusChangedCallback, const EOS_Connect_LoginStatusChangedCallbackInfo* Data);

/** The most recent version of the EOS_Connect_IdToken struct. */
#define EOS_CONNECT_IDTOKEN_API_LATEST 1

/**
 * A structure that contains an ID token.
 * These structures are created by EOS_Connect_CopyIdToken and must be passed to EOS_Connect_IdToken_Release.
 */
EOS_STRUCT(EOS_Connect_IdToken, (
	/** API Version: Set this to EOS_CONNECT_IDTOKEN_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * The Product User ID described by the ID token.
	 * Use EOS_ProductUserId_FromString to populate this field when validating a received ID token.
	 */
	EOS_ProductUserId ProductUserId;
	/** The ID token as a Json Web Token (JWT) string. */
	const char* JsonWebToken;
));

/**
 * Release the memory associated with an EOS_Connect_IdToken structure. This must be called on data retrieved from EOS_Connect_CopyIdToken.
 *
 * @param IdToken The ID token structure to be released.
 *
 * @see EOS_Connect_IdToken
 * @see EOS_Connect_CopyIdToken
 */
EOS_DECLARE_FUNC(void) EOS_Connect_IdToken_Release(EOS_Connect_IdToken* IdToken);

/** The most recent version of the EOS_Connect_CopyIdToken API. */
#define EOS_CONNECT_COPYIDTOKEN_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_CopyIdToken function.
 */
EOS_STRUCT(EOS_Connect_CopyIdTokenOptions, (
	/** API Version: Set this to EOS_CONNECT_COPYIDTOKEN_API_LATEST. */
	int32_t ApiVersion;
	/** The local Product User ID whose ID token should be copied. */
	EOS_ProductUserId LocalUserId;
));

/** The most recent version of the EOS_Connect_VerifyIdToken API. */
#define EOS_CONNECT_VERIFYIDTOKEN_API_LATEST 1

/**
 * Input parameters for the EOS_Connect_VerifyIdToken function.
 */
EOS_STRUCT(EOS_Connect_VerifyIdTokenOptions, (
	/** API Version: Set this to EOS_CONNECT_VERIFYIDTOKEN_API_LATEST. */
	int32_t ApiVersion;
	/**
	 * The ID token to verify.
	 * Use EOS_ProductUserId_FromString to populate the ProductUserId field of this struct.
	 */
	const EOS_Connect_IdToken* IdToken;
));

/**
 * Output parameters for the EOS_Connect_VerifyIdToken Function.
 */
EOS_STRUCT(EOS_Connect_VerifyIdTokenCallbackInfo, (
	/** The EOS_EResult code for the operation. EOS_Success indicates that the operation succeeded; other codes indicate errors. */
	EOS_EResult ResultCode;
	/** Context that was passed into EOS_Connect_VerifyIdToken */
	void* ClientData;
	/** The Product User ID associated with the ID token. */
	EOS_ProductUserId ProductUserId;
	/**
	 * Flag set to indicate whether account information is available.
	 * Applications must always first check this value to be set before attempting
	 * to read the AccountType, AccountId, Platform and DeviceType fields.
	 *
	 * This flag is always false for users that authenticated using EOS Connect Device ID.
	 */
	EOS_Bool bIsAccountInfoPresent;
	/**
	 * The identity provider that the user authenticated with to EOS Connect.
	 *
	 * If bIsAccountInfoPresent is set, this field describes the external account type.
	 */
	EOS_EExternalAccountType AccountIdType;
	/**
	 * The external account ID of the authenticated user.
	 *
	 * This value may be set to an empty string.
	 */
	const char* AccountId;
	/**
	 * Platform that the user is connected from.
	 *
	 * This value may be set to an empty string.
	 */
	const char* Platform;
	/**
	 * Identifies the device type that the user is connected from.
	 * Can be used to securely verify that the user is connected through a real Console device.
	 *
	 * This value may be set to an empty string.
	 */
	const char* DeviceType;
	/**
	 * Client ID of the authorized client.
	 */
	const char* ClientId;
	/**
	 * Product ID.
	 */
	const char* ProductId;
	/**
	 * Sandbox ID.
	 */
	const char* SandboxId;
	/**
	 * Deployment ID.
	 */
	const char* DeploymentId;
));

/**
 * Function prototype definition for callbacks passed into EOS_Connect_VerifyIdToken.
 *
 * @param Data A EOS_Connect_VerifyIdTokenCallbackInfo containing the output information and result.
 */
EOS_DECLARE_CALLBACK(EOS_Connect_OnVerifyIdTokenCallback, const EOS_Connect_VerifyIdTokenCallbackInfo* Data);

#pragma pack(pop)

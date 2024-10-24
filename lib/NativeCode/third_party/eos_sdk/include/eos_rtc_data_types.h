// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "eos_common.h"

#pragma pack(push, 8)

EOS_EXTERN_C typedef struct EOS_RTCDataHandle* EOS_HRTCData;

/** The maximum length of data chunk in bytes that can be sent and received */
#define EOS_RTCDATA_MAX_PACKET_SIZE 1170

/** The most recent version of the EOS_RTCData_SendData API. */
#define EOS_RTCDATA_SENDDATA_API_LATEST 1

/**
 * An enumeration of the different data channel statuses.
 */
EOS_ENUM(EOS_ERTCDataStatus,
	/** Data unsupported */
	EOS_RTCDS_Unsupported = 0,
	/** Data enabled */
	EOS_RTCDS_Enabled = 1,
	/** Data disabled */
	EOS_RTCDS_Disabled = 2
);

/** The most recent version of the EOS_RTCData_AddNotifyParticipantUpdated API. */
#define EOS_RTCDATA_ADDNOTIFYPARTICIPANTUPDATED_API_LATEST 1

/**
 * This struct is used to call EOS_RTCData_AddNotifyParticipantUpdated.
 */
EOS_STRUCT(EOS_RTCData_AddNotifyParticipantUpdatedOptions, (
	/** API Version: Set this to EOS_RTCDATA_ADDNOTIFYPARTICIPANTUPDATED_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the user trying to request this operation. */
	EOS_ProductUserId LocalUserId;
	/** The room this event is registered on. */
	const char* RoomName;
));

/**
 * This struct is passed in with a call to EOS_RTCData_OnParticipantUpdatedCallback registered event.
 */
EOS_STRUCT(EOS_RTCData_ParticipantUpdatedCallbackInfo, (
	/** Client-specified data passed into EOS_RTCData_AddNotifyParticipantUpdated. */
	void* ClientData;
	/** The Product User ID of the user who initiated this request. */
	EOS_ProductUserId LocalUserId;
	/** The room associated with this event. */
	const char* RoomName;
	/** The participant updated. */
	EOS_ProductUserId ParticipantId;
	/** The data channel status. */
	EOS_ERTCDataStatus DataStatus;
));

EOS_DECLARE_CALLBACK(EOS_RTCData_OnParticipantUpdatedCallback, const EOS_RTCData_ParticipantUpdatedCallbackInfo* Data);

/**
 * This struct is used to call EOS_RTCData_SendData.
 */
EOS_STRUCT(EOS_RTCData_SendDataOptions, (
	/** API Version: Set this to EOS_RTCDATA_SENDDATA_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the user trying to request this operation. */
	EOS_ProductUserId LocalUserId;
	/** The  room this event is registered on. */
	const char* RoomName;
	/** The size of the data to be sent to the other participants. Max value is EOS_RTCDATA_MAX_PACKET_SIZE. */
	uint32_t DataLengthBytes;
	/** The data to be sent to the other participants */
	const void* Data;
));

/** The most recent version of the EOS_RTCData_AddNotifyDataReceived API. */
#define EOS_RTCDATA_ADDNOTIFYDATARECEIVED_API_LATEST 1

/**
 * This struct is used to call EOS_RTCData_AddNotifyDataReceived.
 */
EOS_STRUCT(EOS_RTCData_AddNotifyDataReceivedOptions, (
	/** API Version: Set this to EOS_RTCDATA_ADDNOTIFYDATARECEIVED_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the user trying to request this operation. */
	EOS_ProductUserId LocalUserId;
	/** The  room this event is registered on. */
	const char* RoomName;
));

/**
 * This struct is passed in with a call to EOS_RTCData_AddNotifyDataReceived registered event.
 */
EOS_STRUCT(EOS_RTCData_DataReceivedCallbackInfo, (
	/** Client-specified data passed into EOS_RTCData_AddNotifyDataReceived. */
	void* ClientData;
	/** The Product User ID of the user who initiated this request. */
	EOS_ProductUserId LocalUserId;
	/** The room associated with this event. */
	const char* RoomName;
	/** The size of the data received. Max value is EOS_RTCDATA_MAX_PACKET_SIZE. */
	uint32_t DataLengthBytes;
	/** The data received. */
	const void* Data;
	/** The Product User ID of the participant which sent the data. */
	EOS_ProductUserId ParticipantId;
));

EOS_DECLARE_CALLBACK(EOS_RTCData_OnDataReceivedCallback, const EOS_RTCData_DataReceivedCallbackInfo* Data);

/** The most recent version of the EOS_RTCData_UpdateSending API. */
#define EOS_RTCDATA_UPDATESENDING_API_LATEST 1

/**
 * This struct is passed in with a call to EOS_RTCData_UpdateSending
 */
EOS_STRUCT(EOS_RTCData_UpdateSendingOptions, (
	/** API Version: Set this to EOS_RTCDATA_UPDATESENDING_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the user trying to request this operation. */
	EOS_ProductUserId LocalUserId;
	/** The room this settings should be applied on. */
	const char* RoomName;
	/** Creates or destroys data channel */
	EOS_Bool bDataEnabled;
));

/**
 * This struct is passed in with a call to EOS_RTCData_OnUpdateSendingCallback.
 */
EOS_STRUCT(EOS_RTCData_UpdateSendingCallbackInfo, (
	/** This returns:
	 * EOS_Success if sending of channels of the local user was successfully enabled/disabled.
	 * EOS_UnexpectedError otherwise.
	 */
	EOS_EResult ResultCode;
	/** Client-specified data passed into EOS_RTCData_UpdateSending. */
	void* ClientData;
	/** The Product User ID of the user who initiated this request. */
	EOS_ProductUserId LocalUserId;
	/** The room this settings should be applied on. */
	const char* RoomName;
	/** Created or destroyed data channel */
	EOS_Bool bDataEnabled;
));

/**
 * Callback for completion of update sending request.
 */
EOS_DECLARE_CALLBACK(EOS_RTCData_OnUpdateSendingCallback, const EOS_RTCData_UpdateSendingCallbackInfo* Data);

/** The most recent version of the EOS_RTCData_UpdateReceiving API. */
#define EOS_RTCDATA_UPDATERECEIVING_API_LATEST 1

/**
 * This struct is passed in with a call to EOS_RTCData_UpdateReceiving.
 */
EOS_STRUCT(EOS_RTCData_UpdateReceivingOptions, (
	/** API Version: Set this to EOS_RTCDATA_UPDATERECEIVING_API_LATEST. */
	int32_t ApiVersion;
	/** The Product User ID of the user trying to request this operation. */
	EOS_ProductUserId LocalUserId;
	/** The room this settings should be applied on. */
	const char* RoomName;
	/** The participant to modify or null to update the global configuration */
	EOS_ProductUserId ParticipantId;
	/** Creates or destroys data channel subscription */
	EOS_Bool bDataEnabled;
));

/**
 * This struct is passed in with a call to EOS_RTCData_OnUpdateReceivingCallback.
 */
EOS_STRUCT(EOS_RTCData_UpdateReceivingCallbackInfo, (
	/** This returns:
	 * EOS_Success if receiving of channels of remote users was successfully enabled/disabled.
	 * EOS_NotFound if the participant isn't found by ParticipantId.
	 * EOS_UnexpectedError otherwise.
	 */
	EOS_EResult ResultCode;
	/** Client-specified data passed into EOS_RTCData_UpdateReceiving. */
	void* ClientData;
	/** The Product User ID of the user who initiated this request. */
	EOS_ProductUserId LocalUserId;
	/** The room this settings should be applied on. */
	const char* RoomName;
	/** The participant to modify or null to update the global configuration */
	EOS_ProductUserId ParticipantId;
	/** Created or destroyed data channel */
	EOS_Bool bDataEnabled;
));

/**
 * Callback for completion of update receiving request
 */
EOS_DECLARE_CALLBACK(EOS_RTCData_OnUpdateReceivingCallback, const EOS_RTCData_UpdateReceivingCallbackInfo* Data);


#pragma pack(pop)


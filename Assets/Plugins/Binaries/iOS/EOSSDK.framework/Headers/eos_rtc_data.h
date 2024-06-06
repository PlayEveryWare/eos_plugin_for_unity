// Copyright Epic Games, Inc. All Rights Reserved.
#pragma once

#include "eos_rtc_data_types.h"

/**
 * The RTC Data Interface. This is used to manage Data specific RTC features
 *
 * @see EOS_RTC_GetDataInterface
 */

/**
 * Register to receive notifications with remote data packet received.
 * If the returned NotificationId is valid, you must call EOS_RTCData_RemoveNotifyDataReceived when you no longer wish to
 * have your CompletionDelegate called.
 *
 * @note The CompletionDelegate may be called from a thread other than the one from which the SDK is ticking.
 *
 * @param ClientData Arbitrary data that is passed back in the CompletionDelegate
 * @param CompletionDelegate The callback to be fired when a data packet is received
 * @return Notification ID representing the registered callback if successful, an invalid NotificationId if not
 *
 * @see EOS_INVALID_NOTIFICATIONID
 * @see EOS_RTCData_RemoveNotifyDataReceived
 */
EOS_DECLARE_FUNC(EOS_NotificationId) EOS_RTCData_AddNotifyDataReceived(EOS_HRTCData Handle, const EOS_RTCData_AddNotifyDataReceivedOptions* Options, void* ClientData, const EOS_RTCData_OnDataReceivedCallback CompletionDelegate);

/**
 * Unregister a previously bound notification handler from receiving remote data packets.
 *
 * @param NotificationId The Notification ID representing the registered callback
 */
EOS_DECLARE_FUNC(void) EOS_RTCData_RemoveNotifyDataReceived(EOS_HRTCData Handle, EOS_NotificationId NotificationId);

/**
 * Use this function to send a data packet to the rest of participants.
 *
 * @param Options structure containing the parameters for the operation.
 * @return EOS_Success the data packet was queued for sending
 *         EOS_InvalidParameters if any of the options are invalid
 *         EOS_NotFound if the specified room was not found
 */
EOS_DECLARE_FUNC(EOS_EResult) EOS_RTCData_SendData(EOS_HRTCData Handle, const EOS_RTCData_SendDataOptions* Options);

/**
 * Use this function to tweak outgoing data options for a room.
 *
 * @param Options structure containing the parameters for the operation.
 * @param ClientData Arbitrary data that is passed back in the CompletionDelegate
 * @param CompletionDelegate The callback to be fired when the operation completes, either successfully or in error
 * @return EOS_Success if the operation succeeded
 *         EOS_InvalidParameters if any of the parameters are incorrect
 *         EOS_NotFound if the local user is not in the room
 */
EOS_DECLARE_FUNC(void) EOS_RTCData_UpdateSending(EOS_HRTCData Handle, const EOS_RTCData_UpdateSendingOptions* Options, void* ClientData, const EOS_RTCData_OnUpdateSendingCallback CompletionDelegate);

/**
 * Use this function to tweak incoming data options for a room.
 *
 * @param Options structure containing the parameters for the operation.
 * @param ClientData Arbitrary data that is passed back in the CompletionDelegate
 * @param CompletionDelegate The callback to be fired when the operation completes, either successfully or in error
 * @return EOS_Success if the operation succeeded
 *         EOS_InvalidParameters if any of the parameters are incorrect
 *         EOS_NotFound if either the local user or specified participant are not in the room
 */
EOS_DECLARE_FUNC(void) EOS_RTCData_UpdateReceiving(EOS_HRTCData Handle, const EOS_RTCData_UpdateReceivingOptions* Options, void* ClientData, const EOS_RTCData_OnUpdateReceivingCallback CompletionDelegate);

/**
 * Register to receive notifications when a room participant data status is updated (f.e when connection state changes).
 *
 * The notification is raised when the participant's data status is updated. In order not to miss any participant status changes, applications need to add the notification before joining a room.
 *
 * If the returned NotificationId is valid, you must call EOS_RTCData_RemoveNotifyParticipantUpdated when you no longer wish
 * to have your CompletionDelegate called.
 *
 * @param Options structure containing the parameters for the operation.
 * @param ClientData Arbitrary data that is passed back in the CompletionDelegate
 * @param CompletionDelegate The callback to be fired when a participant changes data status
 * @return Notification ID representing the registered callback if successful, an invalid NotificationId if not
 *
 * @see EOS_INVALID_NOTIFICATIONID
 * @see EOS_RTCData_RemoveNotifyParticipantUpdated
 * @see EOS_RTCData_ParticipantUpdatedCallbackInfo
 * @see EOS_ERTCDataStatus
 */
EOS_DECLARE_FUNC(EOS_NotificationId) EOS_RTCData_AddNotifyParticipantUpdated(EOS_HRTCData Handle, const EOS_RTCData_AddNotifyParticipantUpdatedOptions* Options, void* ClientData, const EOS_RTCData_OnParticipantUpdatedCallback CompletionDelegate);

/**
 * Unregister a previously bound notification handler from receiving participant updated notifications
 *
 * @param NotificationId The Notification ID representing the registered callback
 */
EOS_DECLARE_FUNC(void) EOS_RTCData_RemoveNotifyParticipantUpdated(EOS_HRTCData Handle, EOS_NotificationId NotificationId);

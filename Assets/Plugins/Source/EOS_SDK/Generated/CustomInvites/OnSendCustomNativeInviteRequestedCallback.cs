// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.CustomInvites
{
	/// <summary>
	/// Function prototype definition for notifications that comes from <see cref="CustomInvitesInterface.AddNotifySendCustomNativeInviteRequested" />
	/// After processing the callback EOS_UI_AcknowledgeEventId must be called.
	/// <seealso cref="UI.UIInterface.AcknowledgeEventId" />
	/// </summary>
	/// <param name="data">A <see cref="SendCustomNativeInviteRequestedCallbackInfo" /> containing the output information and result</param>
	public delegate void OnSendCustomNativeInviteRequestedCallback(ref SendCustomNativeInviteRequestedCallbackInfo data);

	[System.Runtime.InteropServices.UnmanagedFunctionPointer(Config.LibraryCallingConvention)]
	internal delegate void OnSendCustomNativeInviteRequestedCallbackInternal(ref SendCustomNativeInviteRequestedCallbackInfoInternal data);
}
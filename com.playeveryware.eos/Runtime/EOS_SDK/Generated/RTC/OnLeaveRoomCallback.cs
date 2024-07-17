// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.RTC
{
	/// <summary>
	/// Callback for completion of room leave request.
	/// </summary>
	public delegate void OnLeaveRoomCallback(ref LeaveRoomCallbackInfo data);

	[System.Runtime.InteropServices.UnmanagedFunctionPointer(Config.LibraryCallingConvention)]
	internal delegate void OnLeaveRoomCallbackInternal(ref LeaveRoomCallbackInfoInternal data);
}
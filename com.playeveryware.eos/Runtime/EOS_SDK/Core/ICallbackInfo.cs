// Copyright Epic Games, Inc. All Rights Reserved.

using System;

namespace Epic.OnlineServices
{
	internal interface ICallbackInfo
	{
		object ClientData { get; }

		Result? GetResultCode();
	}

	internal interface ICallbackInfoInternal
	{
		IntPtr ClientDataAddress { get; }
	}
}

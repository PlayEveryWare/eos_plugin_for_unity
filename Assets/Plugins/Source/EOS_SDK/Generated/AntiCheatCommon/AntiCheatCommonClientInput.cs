// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.AntiCheatCommon
{
	/// <summary>
	/// Flags describing the input device used by a remote client, if known. These can be updated during a play session.
	/// </summary>
	public enum AntiCheatCommonClientInput : int
	{
		/// <summary>
		/// Unknown input device
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The client is using mouse and keyboard
		/// </summary>
		MouseKeyboard = 1,
		/// <summary>
		/// The client is using a gamepad or game controller
		/// </summary>
		Gamepad = 2,
		/// <summary>
		/// The client is using a touch input device (e.g. phone/tablet screen)
		/// </summary>
		TouchInput = 3
	}
}
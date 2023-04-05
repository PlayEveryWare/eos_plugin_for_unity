// Copyright Epic Games, Inc. All Rights Reserved.

#if DEBUG
	#define EOS_DEBUG
#endif

#if UNITY_EDITOR
	#define EOS_EDITOR
#endif

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_PS4 || UNITY_XBOXONE || UNITY_SWITCH || UNITY_IOS || UNITY_ANDROID
	#define EOS_UNITY
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || PLATFORM_64BITS || PLATFORM_32BITS
	#if UNITY_EDITOR_WIN || UNITY_64 || PLATFORM_64BITS
		#define EOS_PLATFORM_WINDOWS_64
	#else
		#define EOS_PLATFORM_WINDOWS_32
	#endif

#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	#define EOS_PLATFORM_OSX

#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
	#define EOS_PLATFORM_LINUX

#elif UNITY_PS4
	#define EOS_PLATFORM_PS4

#elif UNITY_XBOXONE
	#define EOS_PLATFORM_XBOXONE

#elif UNITY_SWITCH
	#define EOS_PLATFORM_SWITCH

#elif UNITY_IOS || __IOS__
	#define EOS_PLATFORM_IOS

#elif UNITY_ANDROID || __ANDROID__
	#define EOS_PLATFORM_ANDROID

#endif

using System.Runtime.InteropServices;

namespace Epic.OnlineServices
{
	public static class Config
	{
		public const string LibraryName =
		#if EOS_PLATFORM_WINDOWS_32 && EOS_UNITY
			"EOSSDK-Win32-Shipping"
		#elif EOS_PLATFORM_WINDOWS_32
			"EOSSDK-Win32-Shipping.dll"

		#elif EOS_PLATFORM_WINDOWS_64 && EOS_UNITY
			"EOSSDK-Win64-Shipping"
		#elif EOS_PLATFORM_WINDOWS_64
			"EOSSDK-Win64-Shipping.dll"

		#elif EOS_PLATFORM_OSX && EOS_UNITY
			"libEOSSDK-Mac-Shipping"
		#elif EOS_PLATFORM_OSX
			"libEOSSDK-Mac-Shipping.dylib"

		#elif EOS_PLATFORM_LINUX && EOS_UNITY
			"libEOSSDK-Linux-Shipping"
		#elif EOS_PLATFORM_LINUX
			"libEOSSDK-Linux-Shipping.so"

		#elif EOS_PLATFORM_IOS && EOS_UNITY && EOS_EDITOR
			"EOSSDK"
		#elif EOS_PLATFORM_IOS
			"EOSSDK.framework/EOSSDK"

		#elif EOS_PLATFORM_ANDROID
			"EOSSDK"

		#else
			#error Unable to determine the name of the EOSSDK library. Ensure you have set the correct EOS compilation symbol for the current platform, such as EOS_PLATFORM_WINDOWS_32 or EOS_PLATFORM_WINDOWS_64, so that the correct EOSSDK library can be targeted.
			"EOSSDK-UnknownPlatform-Shipping"

		#endif
		;
		
		public const CallingConvention LibraryCallingConvention =
		#if EOS_PLATFORM_WINDOWS_32
			CallingConvention.StdCall
		#else
			CallingConvention.Cdecl
		#endif
		;
	}
}
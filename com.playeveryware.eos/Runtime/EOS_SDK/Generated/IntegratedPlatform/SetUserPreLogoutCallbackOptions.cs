// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.IntegratedPlatform
{
	/// <summary>
	/// Input parameters for the <see cref="IntegratedPlatformInterface.SetUserPreLogoutCallback" /> function.
	/// </summary>
	public struct SetUserPreLogoutCallbackOptions
	{
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct SetUserPreLogoutCallbackOptionsInternal : ISettable<SetUserPreLogoutCallbackOptions>, System.IDisposable
	{
		private int m_ApiVersion;

		public void Set(ref SetUserPreLogoutCallbackOptions other)
		{
			m_ApiVersion = IntegratedPlatformInterface.SetuserprelogoutcallbackApiLatest;
		}

		public void Set(ref SetUserPreLogoutCallbackOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = IntegratedPlatformInterface.SetuserprelogoutcallbackApiLatest;
			}
		}

		public void Dispose()
		{
		}
	}
}
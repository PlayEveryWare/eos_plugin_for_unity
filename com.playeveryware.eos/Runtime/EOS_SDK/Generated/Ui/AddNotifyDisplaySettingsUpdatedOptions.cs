// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.UI
{
	/// <summary>
	/// Input parameters for the <see cref="UIInterface.AddNotifyDisplaySettingsUpdated" /> function.
	/// </summary>
	public struct AddNotifyDisplaySettingsUpdatedOptions
	{
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyDisplaySettingsUpdatedOptionsInternal : ISettable<AddNotifyDisplaySettingsUpdatedOptions>, System.IDisposable
	{
		private int m_ApiVersion;

		public void Set(ref AddNotifyDisplaySettingsUpdatedOptions other)
		{
			m_ApiVersion = UIInterface.AddnotifydisplaysettingsupdatedApiLatest;
		}

		public void Set(ref AddNotifyDisplaySettingsUpdatedOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = UIInterface.AddnotifydisplaysettingsupdatedApiLatest;
			}
		}

		public void Dispose()
		{
		}
	}
}
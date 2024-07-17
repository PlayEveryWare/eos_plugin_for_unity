// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.UI
{
	/// <summary>
	/// Parameters for the <see cref="UIInterface.ShowNativeProfile" /> function.
	/// </summary>
	public struct ShowNativeProfileOptions
	{
		/// <summary>
		/// The Epic Online Services Account ID of the user who is requesting the profile.
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// The Epic Online Services Account ID of the user whose profile is being requested.
		/// </summary>
		public EpicAccountId TargetUserId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct ShowNativeProfileOptionsInternal : ISettable<ShowNativeProfileOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_TargetUserId;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.Set(value, ref m_LocalUserId);
			}
		}

		public EpicAccountId TargetUserId
		{
			set
			{
				Helper.Set(value, ref m_TargetUserId);
			}
		}

		public void Set(ref ShowNativeProfileOptions other)
		{
			m_ApiVersion = UIInterface.ShownativeprofileApiLatest;
			LocalUserId = other.LocalUserId;
			TargetUserId = other.TargetUserId;
		}

		public void Set(ref ShowNativeProfileOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = UIInterface.ShownativeprofileApiLatest;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_LocalUserId);
			Helper.Dispose(ref m_TargetUserId);
		}
	}
}
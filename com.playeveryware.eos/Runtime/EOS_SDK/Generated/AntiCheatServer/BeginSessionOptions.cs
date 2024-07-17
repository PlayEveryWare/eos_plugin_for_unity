// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.AntiCheatServer
{
	public struct BeginSessionOptions
	{
		/// <summary>
		/// Time in seconds to allow newly registered clients to complete anti-cheat authentication.
		/// Recommended value: 60
		/// </summary>
		public uint RegisterTimeoutSeconds { get; set; }

		/// <summary>
		/// Optional name of this game server
		/// </summary>
		public Utf8String ServerName { get; set; }

		/// <summary>
		/// Gameplay data collection APIs such as LogPlayerTick will be enabled if set to true.
		/// If you do not use these APIs you should set this value to false to reduce memory use.
		/// </summary>
		public bool EnableGameplayData { get; set; }

		/// <summary>
		/// The Product User ID of the local user who is associated with this session. Dedicated servers should set this to null.
		/// </summary>
		public ProductUserId LocalUserId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct BeginSessionOptionsInternal : ISettable<BeginSessionOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private uint m_RegisterTimeoutSeconds;
		private System.IntPtr m_ServerName;
		private int m_EnableGameplayData;
		private System.IntPtr m_LocalUserId;

		public uint RegisterTimeoutSeconds
		{
			set
			{
				m_RegisterTimeoutSeconds = value;
			}
		}

		public Utf8String ServerName
		{
			set
			{
				Helper.Set(value, ref m_ServerName);
			}
		}

		public bool EnableGameplayData
		{
			set
			{
				Helper.Set(value, ref m_EnableGameplayData);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.Set(value, ref m_LocalUserId);
			}
		}

		public void Set(ref BeginSessionOptions other)
		{
			m_ApiVersion = AntiCheatServerInterface.BeginsessionApiLatest;
			RegisterTimeoutSeconds = other.RegisterTimeoutSeconds;
			ServerName = other.ServerName;
			EnableGameplayData = other.EnableGameplayData;
			LocalUserId = other.LocalUserId;
		}

		public void Set(ref BeginSessionOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = AntiCheatServerInterface.BeginsessionApiLatest;
				RegisterTimeoutSeconds = other.Value.RegisterTimeoutSeconds;
				ServerName = other.Value.ServerName;
				EnableGameplayData = other.Value.EnableGameplayData;
				LocalUserId = other.Value.LocalUserId;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_ServerName);
			Helper.Dispose(ref m_LocalUserId);
		}
	}
}
// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.AntiCheatServer
{
	public class RegisterClientOptions
	{
		/// <summary>
		/// Locally unique value describing the remote user (e.g. a player object pointer)
		/// </summary>
		public System.IntPtr ClientHandle { get; set; }

		/// <summary>
		/// Type of remote user being registered
		/// </summary>
		public AntiCheatCommon.AntiCheatCommonClientType ClientType { get; set; }

		/// <summary>
		/// Remote user's platform, if known
		/// </summary>
		public AntiCheatCommon.AntiCheatCommonClientPlatform ClientPlatform { get; set; }

		/// <summary>
		/// Account identifier for the remote user
		/// </summary>
		public string AccountId { get; set; }

		/// <summary>
		/// Optional IP address for the remote user. May be null if not available.
		/// IPv4 format: "0.0.0.0"
		/// IPv6 format: "0:0:0:0:0:0:0:0"
		/// </summary>
		public string IpAddress { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct RegisterClientOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_ClientHandle;
		private AntiCheatCommon.AntiCheatCommonClientType m_ClientType;
		private AntiCheatCommon.AntiCheatCommonClientPlatform m_ClientPlatform;
		private System.IntPtr m_AccountId;
		private System.IntPtr m_IpAddress;

		public System.IntPtr ClientHandle
		{
			set
			{
				m_ClientHandle = value;
			}
		}

		public AntiCheatCommon.AntiCheatCommonClientType ClientType
		{
			set
			{
				m_ClientType = value;
			}
		}

		public AntiCheatCommon.AntiCheatCommonClientPlatform ClientPlatform
		{
			set
			{
				m_ClientPlatform = value;
			}
		}

		public string AccountId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public string IpAddress
		{
			set
			{
				Helper.TryMarshalSet(ref m_IpAddress, value);
			}
		}

		public void Set(RegisterClientOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = AntiCheatServerInterface.RegisterclientApiLatest;
				ClientHandle = other.ClientHandle;
				ClientType = other.ClientType;
				ClientPlatform = other.ClientPlatform;
				AccountId = other.AccountId;
				IpAddress = other.IpAddress;
			}
		}

		public void Set(object other)
		{
			Set(other as RegisterClientOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle);
			Helper.TryMarshalDispose(ref m_AccountId);
			Helper.TryMarshalDispose(ref m_IpAddress);
		}
	}
}
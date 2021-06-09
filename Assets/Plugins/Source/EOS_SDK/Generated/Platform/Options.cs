// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Platform
{
	/// <summary>
	/// Platform options for <see cref="PlatformInterface.Create" />.
	/// </summary>
	public class Options
	{
		/// <summary>
		/// A reserved field that should always be nulled.
		/// </summary>
		public System.IntPtr Reserved { get; set; }

		/// <summary>
		/// The product ID for the running application, found on the dev portal
		/// </summary>
		public string ProductId { get; set; }

		/// <summary>
		/// The sandbox ID for the running application, found on the dev portal
		/// </summary>
		public string SandboxId { get; set; }

		/// <summary>
		/// Set of service permissions associated with the running application
		/// </summary>
		public ClientCredentials ClientCredentials { get; set; }

		/// <summary>
		/// Is this running as a server
		/// </summary>
		public bool IsServer { get; set; }

		/// <summary>
		/// Used by Player Data Storage and Title Storage. Must be null initialized if unused. 256-bit Encryption Key for file encryption in hexadecimal format (64 hex chars)
		/// </summary>
		public string EncryptionKey { get; set; }

		/// <summary>
		/// The override country code to use for the logged in user. (<see cref="PlatformInterface.CountrycodeMaxLength" />)
		/// </summary>
		public string OverrideCountryCode { get; set; }

		/// <summary>
		/// The override locale code to use for the logged in user. This follows ISO 639. (<see cref="PlatformInterface.LocalecodeMaxLength" />)
		/// </summary>
		public string OverrideLocaleCode { get; set; }

		/// <summary>
		/// The deployment ID for the running application, found on the dev portal
		/// </summary>
		public string DeploymentId { get; set; }

		/// <summary>
		/// Platform creation flags, e.g. <see cref="PlatformFlags.LoadingInEditor" />. This is a bitwise-or union of the defined flags.
		/// </summary>
		public PlatformFlags Flags { get; set; }

		/// <summary>
		/// Used by Player Data Storage and Title Storage. Must be null initialized if unused. Cache directory path. Absolute path to the folder that is going to be used for caching temporary data. The path is created if it's missing.
		/// </summary>
		public string CacheDirectory { get; set; }

		/// <summary>
		/// A budget, measured in milliseconds, for <see cref="PlatformInterface.Tick" /> to do its work. When the budget is met or exceeded (or if no work is available), <see cref="PlatformInterface.Tick" /> will return.
		/// This allows your game to amortize the cost of SDK work across multiple frames in the event that a lot of work is queued for processing.
		/// Zero is interpreted as "perform all available work".
		/// </summary>
		public uint TickBudgetInMilliseconds { get; set; }

		/// <summary>
		/// A reserved field that should always be nulled.
		/// </summary>
		public System.IntPtr Reserved2 { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct OptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_Reserved;
		private System.IntPtr m_ProductId;
		private System.IntPtr m_SandboxId;
		private ClientCredentialsInternal m_ClientCredentials;
		private int m_IsServer;
		private System.IntPtr m_EncryptionKey;
		private System.IntPtr m_OverrideCountryCode;
		private System.IntPtr m_OverrideLocaleCode;
		private System.IntPtr m_DeploymentId;
		private PlatformFlags m_Flags;
		private System.IntPtr m_CacheDirectory;
		private uint m_TickBudgetInMilliseconds;
		private System.IntPtr m_Reserved2;

		public System.IntPtr Reserved
		{
			set
			{
				m_Reserved = value;
			}
		}

		public string ProductId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductId, value);
			}
		}

		public string SandboxId
		{
			set
			{
				Helper.TryMarshalSet(ref m_SandboxId, value);
			}
		}

		public ClientCredentials ClientCredentials
		{
			set
			{
				Helper.TryMarshalSet(ref m_ClientCredentials, value);
			}
		}

		public bool IsServer
		{
			set
			{
				Helper.TryMarshalSet(ref m_IsServer, value);
			}
		}

		public string EncryptionKey
		{
			set
			{
				Helper.TryMarshalSet(ref m_EncryptionKey, value);
			}
		}

		public string OverrideCountryCode
		{
			set
			{
				Helper.TryMarshalSet(ref m_OverrideCountryCode, value);
			}
		}

		public string OverrideLocaleCode
		{
			set
			{
				Helper.TryMarshalSet(ref m_OverrideLocaleCode, value);
			}
		}

		public string DeploymentId
		{
			set
			{
				Helper.TryMarshalSet(ref m_DeploymentId, value);
			}
		}

		public PlatformFlags Flags
		{
			set
			{
				m_Flags = value;
			}
		}

		public string CacheDirectory
		{
			set
			{
				Helper.TryMarshalSet(ref m_CacheDirectory, value);
			}
		}

		public uint TickBudgetInMilliseconds
		{
			set
			{
				m_TickBudgetInMilliseconds = value;
			}
		}

		public System.IntPtr Reserved2
		{
			set
			{
				m_Reserved2 = value;
			}
		}

		public void Set(Options other)
		{
			if (other != null)
			{
				m_ApiVersion = PlatformInterface.OptionsApiLatest;
				Reserved = other.Reserved;
				ProductId = other.ProductId;
				SandboxId = other.SandboxId;
				ClientCredentials = other.ClientCredentials;
				IsServer = other.IsServer;
				EncryptionKey = other.EncryptionKey;
				OverrideCountryCode = other.OverrideCountryCode;
				OverrideLocaleCode = other.OverrideLocaleCode;
				DeploymentId = other.DeploymentId;
				Flags = other.Flags;
				CacheDirectory = other.CacheDirectory;
				TickBudgetInMilliseconds = other.TickBudgetInMilliseconds;
				Reserved2 = other.Reserved2;
			}
		}

		public void Set(object other)
		{
			Set(other as Options);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Reserved);
			Helper.TryMarshalDispose(ref m_ProductId);
			Helper.TryMarshalDispose(ref m_SandboxId);
			Helper.TryMarshalDispose(ref m_ClientCredentials);
			Helper.TryMarshalDispose(ref m_EncryptionKey);
			Helper.TryMarshalDispose(ref m_OverrideCountryCode);
			Helper.TryMarshalDispose(ref m_OverrideLocaleCode);
			Helper.TryMarshalDispose(ref m_DeploymentId);
			Helper.TryMarshalDispose(ref m_CacheDirectory);
			Helper.TryMarshalDispose(ref m_Reserved2);
		}
	}
}
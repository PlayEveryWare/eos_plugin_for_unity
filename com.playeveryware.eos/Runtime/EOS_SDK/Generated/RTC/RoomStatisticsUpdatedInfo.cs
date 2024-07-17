// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.RTC
{
	/// <summary>
	/// This struct is passed in with a call to <see cref="RTCInterface.AddNotifyRoomStatisticsUpdated" /> registered event.
	/// </summary>
	public struct RoomStatisticsUpdatedInfo : ICallbackInfo
	{
		/// <summary>
		/// Client-specified data passed into <see cref="RTCInterface.AddNotifyRoomStatisticsUpdated" />.
		/// </summary>
		public object ClientData { get; set; }

		/// <summary>
		/// The Product User ID of the user who initiated this request.
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The room associated with this event.
		/// </summary>
		public Utf8String RoomName { get; set; }

		/// <summary>
		/// Statistics in JSON format
		/// </summary>
		public Utf8String Statistic { get; set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(ref RoomStatisticsUpdatedInfoInternal other)
		{
			ClientData = other.ClientData;
			LocalUserId = other.LocalUserId;
			RoomName = other.RoomName;
			Statistic = other.Statistic;
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct RoomStatisticsUpdatedInfoInternal : ICallbackInfoInternal, IGettable<RoomStatisticsUpdatedInfo>, ISettable<RoomStatisticsUpdatedInfo>, System.IDisposable
	{
		private System.IntPtr m_ClientData;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_RoomName;
		private System.IntPtr m_Statistic;

		public object ClientData
		{
			get
			{
				object value;
				Helper.Get(m_ClientData, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_ClientData);
			}
		}

		public System.IntPtr ClientDataAddress
		{
			get
			{
				return m_ClientData;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				ProductUserId value;
				Helper.Get(m_LocalUserId, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_LocalUserId);
			}
		}

		public Utf8String RoomName
		{
			get
			{
				Utf8String value;
				Helper.Get(m_RoomName, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_RoomName);
			}
		}

		public Utf8String Statistic
		{
			get
			{
				Utf8String value;
				Helper.Get(m_Statistic, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_Statistic);
			}
		}

		public void Set(ref RoomStatisticsUpdatedInfo other)
		{
			ClientData = other.ClientData;
			LocalUserId = other.LocalUserId;
			RoomName = other.RoomName;
			Statistic = other.Statistic;
		}

		public void Set(ref RoomStatisticsUpdatedInfo? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				RoomName = other.Value.RoomName;
				Statistic = other.Value.Statistic;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_ClientData);
			Helper.Dispose(ref m_LocalUserId);
			Helper.Dispose(ref m_RoomName);
			Helper.Dispose(ref m_Statistic);
		}

		public void Get(out RoomStatisticsUpdatedInfo output)
		{
			output = new RoomStatisticsUpdatedInfo();
			output.Set(ref this);
		}
	}
}
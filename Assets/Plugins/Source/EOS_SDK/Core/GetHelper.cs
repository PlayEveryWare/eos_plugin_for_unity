// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices
{
	public sealed partial class Helper
	{
		internal static void Get<TArray>(TArray[] from, out int to)
		{
			Convert(from, out to);
		}

		internal static void Get<TArray>(TArray[] from, out uint to)
		{
			Convert(from, out to);
		}

		internal static void Get<TArray>(ArraySegment<TArray> from, out uint to)
		{
			Convert(from, out to);
		}

		internal static void Get<TTo>(IntPtr from, out TTo to)
			where TTo : Handle, new()
		{
			Convert(from, out to);
		}

		internal static void Get<TFrom, TTo>(ref TFrom from, out TTo to)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			from.Get(out to);
		}

		internal static void Get(int from, out bool to)
		{
			Convert(from, out to);
		}

		internal static void Get(bool from, out int to)
		{
			Convert(from, out to);
		}

		internal static void Get(long from, out DateTimeOffset? to)
		{
			Convert(from, out to);
		}

		internal static void Get<TTo>(IntPtr from, out TTo[] to, int arrayLength, bool isArrayItemAllocated)
		{
			GetAllocation(from, out to, arrayLength, isArrayItemAllocated);
		}

		internal static void Get<TTo>(IntPtr from, out TTo[] to, uint arrayLength, bool isArrayItemAllocated)
		{
			GetAllocation(from, out to, (int)arrayLength, isArrayItemAllocated);
		}

		internal static void Get<TTo>(IntPtr from, out TTo[] to, int arrayLength)
		{
			GetAllocation(from, out to, arrayLength, !typeof(TTo).IsValueType);
		}

		internal static void Get<TTo>(IntPtr from, out TTo[] to, uint arrayLength)
		{
			GetAllocation(from, out to, (int)arrayLength, !typeof(TTo).IsValueType);
		}

		internal static void Get(IntPtr from, out ArraySegment<byte> to, uint arrayLength)
		{
			to = new ArraySegment<byte>();
			if (arrayLength != 0)
			{
				byte[] bytes = new byte[arrayLength];
				Marshal.Copy(from, bytes, 0, (int)arrayLength);
				to = new ArraySegment<byte>(bytes);
			}
		}

		internal static void GetHandle<THandle>(IntPtr from, out THandle[] to, uint arrayLength)
			where THandle : Handle, new()
		{
			GetAllocation(from, out to, (int)arrayLength);
		}

		internal static void Get<TFrom, TTo>(TFrom[] from, out TTo[] to)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			to = GetDefault<TTo[]>();

			if (from != null)
			{
				to = new TTo[from.Length];

				for (int index = 0; index < from.Length; ++index)
				{
					from[index].Get(out to[index]);
				}
			}
		}

		internal static void Get<TFrom, TTo>(IntPtr from, out TTo[] to, int arrayLength)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			TFrom[] fromIntermediate;
			Get(from, out fromIntermediate, arrayLength);
			Get(fromIntermediate, out to);
		}

		internal static void Get<TFrom, TTo>(IntPtr from, out TTo[] to, uint arrayLength)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			Get<TFrom, TTo>(from, out to, (int)arrayLength);
		}

		internal static void Get<TTo>(IntPtr from, out TTo? to)
			where TTo : struct
		{
			GetAllocation(from, out to);
		}

		internal static void Get(byte[] from, out string to)
		{
			Convert(from, out to);
		}

		internal static void Get(IntPtr from, out object to)
		{
			to = GetClientData(from);
		}

		internal static void Get(IntPtr from, out Utf8String to)
		{
			GetAllocation(from, out to);
		}

		internal static void Get<T, TEnum>(T from, out T to, TEnum currentEnum, TEnum expectedEnum)
		{
			to = GetDefault<T>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				to = from;
			}
		}

		internal static void Get<TFrom, TTo, TEnum>(ref TFrom from, out TTo to, TEnum currentEnum, TEnum expectedEnum)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			to = GetDefault<TTo>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				Get(ref from, out to);
			}
		}

		internal static void Get<TEnum>(int from, out bool? to, TEnum currentEnum, TEnum expectedEnum)
		{
			to = GetDefault<bool?>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				bool fromIntermediate;
				Convert(from, out fromIntermediate);
				to = fromIntermediate;
			}
		}

		internal static void Get<TFrom, TEnum>(TFrom from, out TFrom? to, TEnum currentEnum, TEnum expectedEnum)
			where TFrom : struct
		{
			to = GetDefault<TFrom?>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				to = from;
			}
		}

		internal static void Get<TFrom, TEnum>(IntPtr from, out TFrom to, TEnum currentEnum, TEnum expectedEnum)
			where TFrom : Handle, new()
		{
			to = GetDefault<TFrom>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				Get(from, out to);
			}
		}

		internal static void Get<TEnum>(IntPtr from, out IntPtr? to, TEnum currentEnum, TEnum expectedEnum)
		{
			to = GetDefault<IntPtr?>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				Get(from, out to);
			}
		}

		internal static void Get<TEnum>(IntPtr from, out Utf8String to, TEnum currentEnum, TEnum expectedEnum)
		{
			to = GetDefault<Utf8String>();

			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				Get(from, out to);
			}
		}

		internal static void Get<TFrom, TTo>(IntPtr from, out TTo to)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			to = GetDefault<TTo>();

			TFrom? fromIntermediate;
			Get(from, out fromIntermediate);

			if (fromIntermediate.HasValue)
			{
				fromIntermediate.Value.Get(out to);
			}
		}

		internal static void Get<TFrom, TTo>(IntPtr from, out TTo? to)
			where TFrom : struct, IGettable<TTo>
			where TTo : struct
		{
			to = GetDefault<TTo?>();

			TFrom? fromIntermediate;
			Get(from, out fromIntermediate);

			if (fromIntermediate.HasValue)
			{
				TTo toIntermediate;
				fromIntermediate.Value.Get(out toIntermediate);

				to = toIntermediate;
			}
		}

		internal static void Get<TFrom, TTo>(ref TFrom from, out TTo to, out IntPtr clientDataAddress)
			where TFrom : struct, ICallbackInfoInternal, IGettable<TTo>
			where TTo : struct
		{
			from.Get(out to);
			clientDataAddress = from.ClientDataAddress;
		}
	}
}
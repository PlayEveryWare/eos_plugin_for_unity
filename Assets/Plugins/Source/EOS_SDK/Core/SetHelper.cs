// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices
{
	public sealed partial class Helper
	{
		internal static void Set<T>(ref T from, ref T to)
			where T : struct
		{
			to = from;
		}

		internal static void Set(object from, ref IntPtr to)
		{
			RemoveClientData(to);
			to = AddClientData(from);
		}

		internal static void Set(Utf8String from, ref IntPtr to)
		{
			Dispose(ref to);
			to = AddPinnedBuffer(from);
		}

		internal static void Set(Handle from, ref IntPtr to)
		{
			Convert(from, out to);
		}

		internal static void Set<T>(T? from, ref IntPtr to)
			where T : struct
		{
			Dispose(ref to);
			to = AddAllocation(Marshal.SizeOf(typeof(T)), from);
		}

		internal static void Set<T>(T[] from, ref IntPtr to, bool isArrayItemAllocated)
		{
			Dispose(ref to);
			to = AddAllocation(from, isArrayItemAllocated);
		}

		internal static void Set(ArraySegment<byte> from, ref IntPtr to, out uint arrayLength)
		{
			to = AddPinnedBuffer(from);
			Get(from, out arrayLength);
		}

		internal static void Set<T>(T[] from, ref IntPtr to)
		{
			Set(from, ref to, !typeof(T).IsValueType);
		}

		internal static void Set<T>(T[] from, ref IntPtr to, bool isArrayItemAllocated, out int arrayLength)
		{
			Set(from, ref to, isArrayItemAllocated);
			Get(from, out arrayLength);
		}

		internal static void Set<T>(T[] from, ref IntPtr to, bool isArrayItemAllocated, out uint arrayLength)
		{
			Set(from, ref to, isArrayItemAllocated);
			Get(from, out arrayLength);
		}

		internal static void Set<T>(T[] from, ref IntPtr to, out int arrayLength)
		{
			Set(from, ref to, !typeof(T).IsValueType, out arrayLength);
		}

		internal static void Set<T>(T[] from, ref IntPtr to, out uint arrayLength)
		{
			Set(from, ref to, !typeof(T).IsValueType, out arrayLength);
		}

		internal static void Set(DateTimeOffset? from, ref long to)
		{
			Convert(from, out to);
		}

		internal static void Set(bool from, ref int to)
		{
			Convert(from, out to);
		}

		internal static void Set(string from, ref byte[] to, int stringLength)
		{
			Convert(from, out to, stringLength);
		}

		internal static void Set<T, TEnum>(T from, ref T to, TEnum fromEnum, ref TEnum toEnum, IDisposable disposable = null)
		{
			if (from != null)
			{
				Dispose(ref disposable);

				to = from;
				toEnum = fromEnum;
			}
		}

		internal static void Set<TFrom, TEnum, TTo>(ref TFrom from, ref TTo to, TEnum fromEnum, ref TEnum toEnum, IDisposable disposable = null)
			where TFrom : struct
			where TTo : struct, ISettable<TFrom>
		{
			Dispose(ref disposable);

			Set(ref from, ref to);
			toEnum = fromEnum;
		}

		internal static void Set<T, TEnum>(T? from, ref T to, TEnum fromEnum, ref TEnum toEnum, IDisposable disposable = null)
			where T : struct
		{
			if (from != null)
			{
				Dispose(ref disposable);

				T value = from.Value;
				Set<T>(ref value, ref to);
				toEnum = fromEnum;
			}
		}

		internal static void Set<TEnum>(Handle from, ref IntPtr to, TEnum fromEnum, ref TEnum toEnum, IDisposable disposable = null)
		{
			if (from != null)
			{
				Dispose(ref to);
				Dispose(ref disposable);

				Set(from, ref to);
				toEnum = fromEnum;
			}
		}

		internal static void Set<TEnum>(Utf8String from, ref IntPtr to, TEnum fromEnum, ref TEnum toEnum, IDisposable disposable = null)
		{
			if (from != null)
			{
				Dispose(ref to);
				Dispose(ref disposable);

				Set(from, ref to);
				toEnum = fromEnum;
			}
		}

		internal static void Set<TEnum>(bool? from, ref int to, TEnum fromEnum, ref TEnum toEnum, IDisposable disposable = null)
		{
			if (from != null)
			{
				Dispose(ref disposable);

				Set(from.Value, ref to);
				toEnum = fromEnum;
			}
		}

		internal static void Set<TFrom, TIntermediate>(ref TFrom from, ref IntPtr to)
			where TFrom : struct
			where TIntermediate : struct, ISettable<TFrom>
		{
			TIntermediate intermediate = new TIntermediate();
			intermediate.Set(ref from);

			Dispose(ref to);
			to = AddAllocation(Marshal.SizeOf(typeof(TIntermediate)), intermediate);
		}

		internal static void Set<TFrom, TIntermediate>(ref TFrom? from, ref IntPtr to)
			where TIntermediate : struct, ISettable<TFrom>
			where TFrom : struct
		{
			Dispose(ref to);

			if (!from.HasValue)
			{
				return;
			}

			TIntermediate intermediate = new TIntermediate();
			var sourceValue = from.Value;
			intermediate.Set(ref sourceValue);

			to = AddAllocation(Marshal.SizeOf(typeof(TIntermediate)), intermediate);
		}

		internal static void Set<TFrom, TTo>(ref TFrom from, ref TTo to)
			where TFrom : struct
			where TTo : struct, ISettable<TFrom>
		{
			to.Set(ref from);
		}

		internal static void Set<TFrom, TIntermediate>(ref TFrom[] from, ref IntPtr to, out int arrayLength)
			where TFrom : struct
			where TIntermediate : struct, ISettable<TFrom>
		{
			arrayLength = 0;

			if (from != null)
			{
				TIntermediate[] intermediate = new TIntermediate[from.Length];
				for (int index = 0; index < from.Length; ++index)
				{
					intermediate[index].Set(ref from[index]);
				}

				Set(intermediate, ref to);
				Get(from, out arrayLength);
			}
		}

		internal static void Set<TFrom, TIntermediate>(ref TFrom[] from, ref IntPtr to, out uint arrayLength)
			where TFrom : struct
			where TIntermediate : struct, ISettable<TFrom>
		{
			int arrayLengthIntermediate;
			Set<TFrom, TIntermediate>(ref from, ref to, out arrayLengthIntermediate);
			arrayLength = (uint)arrayLengthIntermediate;
		}
	}
}
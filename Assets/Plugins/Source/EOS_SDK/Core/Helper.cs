// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices
{
	internal class AllocationException : Exception
	{
		public AllocationException(string message)
			: base(message)
		{
		}
	}

	internal class ExternalAllocationException : AllocationException
	{
		public ExternalAllocationException(IntPtr address, Type type)
			: base(string.Format("Attempting to allocate '{0}' over externally allocated memory at {1}", type, address.ToString("X")))
		{
		}
	}

	internal class CachedTypeAllocationException : AllocationException
	{
		public CachedTypeAllocationException(IntPtr address, Type foundType, Type expectedType)
			: base(string.Format("Cached allocation is '{0}' but expected '{1}' at {2}", foundType, expectedType, address.ToString("X")))
		{
		}
	}

	internal class CachedArrayAllocationException : AllocationException
	{
		public CachedArrayAllocationException(IntPtr address, int foundLength, int expectedLength)
			: base(string.Format("Cached array allocation has length {0} but expected {1} at {2}", foundLength, expectedLength, address.ToString("X")))
		{
		}
	}

	internal class DynamicBindingException : Exception
	{
		public DynamicBindingException(string bindingName)
			: base(string.Format("Failed to hook dynamic binding for '{0}'", bindingName))
		{
		}
	}

	public static class Helper
	{
		internal class Allocation
		{
			public int Size { get; private set; }

			public object CachedData { get; private set; }

			public bool? IsCachedArrayElementAllocated { get; private set; }

			public Allocation(int size)
			{
				Size = size;
			}

			public void SetCachedData(object data, bool? isCachedArrayElementAllocated = null)
			{
				CachedData = data;
				IsCachedArrayElementAllocated = isCachedArrayElementAllocated;
			}
		}

		private class DelegateHolder
		{
			public Delegate Public { get; private set; }
			public Delegate Private { get; private set; }
			public Delegate[] StructDelegates { get; private set; }
			public ulong? NotificationId { get; set; }

			public DelegateHolder(Delegate publicDelegate, Delegate privateDelegate, params Delegate[] structDelegates)
			{
				Public = publicDelegate;
				Private = privateDelegate;
				StructDelegates = structDelegates;
			}
		}

		private static Dictionary<IntPtr, Allocation> s_Allocations = new Dictionary<IntPtr, Allocation>();
		private static Dictionary<IntPtr, DelegateHolder> s_Callbacks = new Dictionary<IntPtr, DelegateHolder>();
		private static Dictionary<string, DelegateHolder> s_StaticCallbacks = new Dictionary<string, DelegateHolder>();

		/// <summary>
		/// Gets the number of unmanaged allocations currently active within the wrapper. Use this to find leaks related to the usage of wrapper code.
		/// </summary>
		/// <returns>The number of unmanaged allocations currently active within the wrapper.</returns>
		public static int GetAllocationCount()
		{
			return s_Allocations.Count;
		}

		// These functions are the front end when changing SDK values into wrapper values.
		// They will either fetch or convert; whichever is most appropriate for the source and target types.
#region Marshal Getters
		internal static bool TryMarshalGet<T>(T[] source, out uint target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T target)
			where T : Handle, new()
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<TSource, TTarget>(TSource source, out TTarget target)
			where TTarget : ISettable, new()
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(int source, out bool target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(bool source, out int target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(long source, out DateTimeOffset? target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, int arrayLength, bool isElementAllocated)
		{
			return TryFetch(source, out target, arrayLength, isElementAllocated);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, uint arrayLength, bool isElementAllocated)
		{
			return TryFetch(source, out target, (int)arrayLength, isElementAllocated);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, int arrayLength)
		{
			return TryMarshalGet(source, out target, arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, uint arrayLength)
		{
			return TryMarshalGet(source, out target, arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalGet<TSource, TTarget>(TSource[] source, out TTarget[] target)
			where TSource : struct
			where TTarget : class, ISettable, new()
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<TSource, TTarget>(IntPtr source, out TTarget[] target, int arrayLength)
			where TSource : struct
			where TTarget : class, ISettable, new()
		{
			target = GetDefault<TTarget[]>();

			TSource[] intermediateSource;
			if (TryMarshalGet(source, out intermediateSource, arrayLength))
			{
				return TryMarshalGet(intermediateSource, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TSource, TTarget>(IntPtr source, out TTarget[] target, uint arrayLength)
			where TSource : struct
			where TTarget : class, ISettable, new()
		{
			int arrayLengthInt = (int)arrayLength;
			return TryMarshalGet<TSource, TTarget>(source, out target, arrayLengthInt);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T? target)
			where T : struct
		{
			return TryFetch(source, out target);
		}

		internal static bool TryMarshalGet(byte[] source, out string target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(IntPtr source, out object target)
		{
			target = null;

			BoxedData boxedData;
			if (TryFetch(source, out boxedData))
			{
				target = boxedData.Data;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet(IntPtr source, out string target)
		{
			return TryFetch(source, out target);
		}

		internal static bool TryMarshalGet<T, TEnum>(T source, out T target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<T>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				target = source;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet<TTarget, TEnum>(ISettable source, out TTarget target, TEnum currentEnum, TEnum comparisonEnum)
			where TTarget : ISettable, new()
		{
			target = GetDefault<TTarget>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryConvert(source, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TEnum>(int source, out bool? target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<bool?>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				bool targetConvert;
				if (TryConvert(source, out targetConvert))
				{
					target = targetConvert;
					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalGet<T, TEnum>(T source, out T? target, TEnum currentEnum, TEnum comparisonEnum)
			where T : struct
		{
			target = GetDefault<T?>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				target = source;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet<T, TEnum>(IntPtr source, out T target, TEnum currentEnum, TEnum comparisonEnum)
			where T : Handle, new()
		{
			target = GetDefault<T>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TEnum>(IntPtr source, out IntPtr? target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<IntPtr?>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TEnum>(IntPtr source, out string target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<string>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TInternal, TPublic>(IntPtr source, out TPublic target)
			where TInternal : struct
			where TPublic : class, ISettable, new()
		{
			target = GetDefault<TPublic>();

			TInternal? targetInternal;
			if (TryMarshalGet(source, out targetInternal))
			{
				if (targetInternal.HasValue)
				{
					target = new TPublic();
					target.Set(targetInternal);

					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TCallbackInfo callbackInfo, out IntPtr clientDataAddress)
			where TCallbackInfoInternal : struct, ICallbackInfoInternal
			where TCallbackInfo : class, ISettable, new()
		{
			callbackInfo = null;
			clientDataAddress = IntPtr.Zero;

			TCallbackInfoInternal callbackInfoInternal;
			if (TryFetch(callbackInfoAddress, out callbackInfoInternal))
			{
				callbackInfo = new TCallbackInfo();
				callbackInfo.Set(callbackInfoInternal);
				clientDataAddress = callbackInfoInternal.ClientDataAddress;

				return true;
			}

			return false;
		}
#endregion

		// These functions are the front end for changing wrapper values into SDK values.
		// They will either allocate or convert; whichever is most appropriate for the source and target types.
#region Marshal Setters
		internal static bool TryMarshalSet<T>(ref T target, T source)
		{
			target = source;

			return true;
		}

		internal static bool TryMarshalSet<TTarget>(ref TTarget target, object source)
			where TTarget : ISettable, new()
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref IntPtr target, Handle source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T? source)
			where T : struct
		{
			return TryAllocate(ref target, source);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, bool isElementAllocated)
		{
			return TryAllocate(ref target, source, isElementAllocated);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source)
		{
			return TryMarshalSet(ref target, source, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out int arrayLength, bool isElementAllocated)
		{
			arrayLength = 0;

			if (TryMarshalSet(ref target, source, isElementAllocated))
			{
				arrayLength = source.Length;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out uint arrayLength, bool isElementAllocated)
		{
			arrayLength = 0;

			int arrayLengthInternal = 0;
			if (TryMarshalSet(ref target, source, out arrayLengthInternal, isElementAllocated))
			{
				arrayLength = (uint)arrayLengthInternal;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out uint arrayLength)
		{
			return TryMarshalSet(ref target, source, out arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet(ref long target, DateTimeOffset? source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref int target, bool source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref byte[] target, string source, int length)
		{
			return TryConvert(source, out target, length);
		}

		internal static bool TryMarshalSet(ref IntPtr target, string source)
		{
			return TryAllocate(ref target, source);
		}

		internal static bool TryMarshalSet<T, TEnum>(ref T target, T source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalSet<TTarget, TEnum>(ref TTarget target, ISettable source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
			where TTarget : ISettable, new()
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryConvert(source, out target))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalSet<T, TEnum>(ref T target, T? source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
			where T : struct
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source.Value))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref IntPtr target, Handle source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref IntPtr target, string source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref target);
				target = IntPtr.Zero;

				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref int target, bool? source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source.Value))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic source)
			where TInternal : struct, ISettable
			where TPublic : class
		{
			if (source != null)
			{
				TInternal targetInternal = new TInternal();
				targetInternal.Set(source);

				if (TryAllocate(ref target, targetInternal))
				{
					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out int arrayLength)
			where TInternal : struct, ISettable
			where TPublic : class
		{
			arrayLength = 0;

			if (source != null)
			{
				TInternal[] targetInternal = new TInternal[source.Length];
				for (int index = 0; index < source.Length; ++index)
				{
					targetInternal[index].Set(source[index]);
				}

				if (TryMarshalSet(ref target, targetInternal))
				{
					arrayLength = source.Length;
					return true;
				}

			}

			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out uint arrayLength)
			where TInternal : struct, ISettable
			where TPublic : class
		{
			arrayLength = 0;

			int arrayLengthInt;
			if (TryMarshalSet<TInternal, TPublic>(ref target, source, out arrayLengthInt))
			{
				arrayLength = (uint)arrayLengthInt;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out int arrayLength, bool isElementAllocated)
			where TInternal : struct, ISettable
			where TPublic : class
		{
			arrayLength = 0;

			if (source != null)
			{
				TInternal[] targetInternal = new TInternal[source.Length];
				for (int index = 0; index < source.Length; ++index)
				{
					targetInternal[index].Set(source[index]);
				}

				if (TryMarshalSet(ref target, targetInternal, isElementAllocated))
				{
					arrayLength = source.Length;
					return true;
				}

			}

			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out uint arrayLength, bool isElementAllocated)
			where TInternal : struct, ISettable
			where TPublic : class
		{
			arrayLength = 0;

			int arrayLengthInt;
			if (TryMarshalSet<TInternal, TPublic>(ref target, source, out arrayLengthInt, isElementAllocated))
			{
				arrayLength = (uint)arrayLengthInt;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalCopy(IntPtr target, byte[] source)
		{
			if (target != IntPtr.Zero && source != null)
			{
				Marshal.Copy(source, 0, target, source.Length);
				return true;
			}

			return false;
		}

		internal static bool TryMarshalAllocate(ref IntPtr target, int size, out Allocation allocation)
		{
			TryMarshalDispose(ref target);

			target = Marshal.AllocHGlobal(size);
			Marshal.WriteByte(target, 0, 0);

			allocation = new Allocation(size);
			s_Allocations.Add(target, allocation);

			return true;
		}

		internal static bool TryMarshalAllocate(ref IntPtr target, uint size, out Allocation allocation)
		{
			return TryMarshalAllocate(ref target, (int)size, out allocation);
		}
#endregion

		// These functions are the front end for disposing of unmanaged memory that this wrapper has allocated.
#region Marshal Disposers
		internal static bool TryMarshalDispose<TDisposable>(ref TDisposable disposable)
			where TDisposable : IDisposable
		{
			if (disposable != null)
			{
				disposable.Dispose();
				return true;
			}

			return false;
		}

		internal static bool TryMarshalDispose(ref IntPtr value)
		{
			return TryRelease(ref value);
		}

		internal static bool TryMarshalDispose<TEnum>(ref IntPtr member, TEnum currentEnum, TEnum comparisonEnum)
		{
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryRelease(ref member);
			}

			return false;
		}
#endregion

		// These functions are exposed to the wrapper to generally streamline blocks of generated code.
#region Helpers
		internal static T GetDefault<T>()
		{
			return default(T);
		}

		internal static void AddCallback(ref IntPtr clientDataAddress, object clientData, Delegate publicDelegate, Delegate privateDelegate, params Delegate[] structDelegates)
		{
			TryAllocateCacheOnly(ref clientDataAddress, new BoxedData(clientData));
			s_Callbacks.Add(clientDataAddress, new DelegateHolder(publicDelegate, privateDelegate, structDelegates));
		}

		internal static void AddStaticCallback(string key, Delegate publicDelegate, Delegate privateDelegate)
		{
			s_StaticCallbacks[key] = new DelegateHolder(publicDelegate, privateDelegate);
		}

		internal static bool TryAssignNotificationIdToCallback(IntPtr clientDataAddress, ulong notificationId)
		{
			if (notificationId != 0)
			{
				DelegateHolder delegateHolder = null;
				if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
				{
					delegateHolder.NotificationId = notificationId;
					return true;
				}
			}
			// We can safely release if the notification id came back invalid
			else
			{
				s_Callbacks.Remove(clientDataAddress);
				TryRelease(ref clientDataAddress);
			}

			return false;
		}

		internal static bool TryRemoveCallbackByNotificationId(ulong notificationId)
		{
			var delegateHolderPairs = s_Callbacks.Where(pair => pair.Value.NotificationId.HasValue && pair.Value.NotificationId == notificationId);
			if (delegateHolderPairs.Any())
			{
				IntPtr clientDataAddress = delegateHolderPairs.First().Key;

				s_Callbacks.Remove(clientDataAddress);
				TryRelease(ref clientDataAddress);

				return true;
			}

			return false;
		}

		internal static bool TryGetAndRemoveCallback<TCallback, TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TCallback callback, out TCallbackInfo callbackInfo)
			where TCallback : class
			where TCallbackInfoInternal : struct, ICallbackInfoInternal
			where TCallbackInfo : class, ICallbackInfo, ISettable, new()
		{
			callback = null;
			callbackInfo = null;

			IntPtr clientDataAddress = IntPtr.Zero;
			if (TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(callbackInfoAddress, out callbackInfo, out clientDataAddress)
				&& TryGetAndRemoveCallback(clientDataAddress, callbackInfo, out callback))
			{
				return true;
			}

			return false;
		}

		internal static bool TryGetStructCallback<TDelegate, TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TDelegate callback, out TCallbackInfo callbackInfo)
			where TDelegate : class
			where TCallbackInfoInternal : struct, ICallbackInfoInternal
			where TCallbackInfo : class, ISettable, new()
		{
			callback = null;
			callbackInfo = null;

			IntPtr clientDataAddress = IntPtr.Zero;
			if (TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(callbackInfoAddress, out callbackInfo, out clientDataAddress)
				&& TryGetStructCallback(clientDataAddress, out callback))
			{
				return true;
			}

			return false;
		}
#endregion

		// These functions are used for allocating unmanaged memory.
		// They should not be exposed outside of this helper.
#region Private Allocators
		private static bool TryAllocate<T>(ref IntPtr target, T source)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			Allocation allocation;
			if (!TryMarshalAllocate(ref target, Marshal.SizeOf(typeof(T)), out allocation))
			{
				return false;
			}

			allocation.SetCachedData(source);
			Marshal.StructureToPtr(source, target, false);

			return true;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T? source)
			where T : struct
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			return TryAllocate(ref target, source.Value);
		}

		private static bool TryAllocate(ref IntPtr target, string source)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			byte[] bytes;
			if (TryConvert(source, out bytes))
			{
				return TryAllocate(ref target, bytes, false);
			}

			return false;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T[] source, bool isElementAllocated)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			var itemSize = 0;
			if (isElementAllocated)
			{
				itemSize = Marshal.SizeOf(typeof(IntPtr));
			}
			else
			{
				itemSize = Marshal.SizeOf(typeof(T));
			}

			// Allocate the array
			Allocation allocation;
			if (!TryMarshalAllocate(ref target, source.Length * itemSize, out allocation))
			{
				return false;
			}

			allocation.SetCachedData(source, isElementAllocated);

			for (int itemIndex = 0; itemIndex < source.Length; ++itemIndex)
			{
				var item = (T)source.GetValue(itemIndex);

				if (isElementAllocated)
				{
					// Allocate the item
					IntPtr newItemAddress = IntPtr.Zero;

					if (typeof(T) == typeof(string))
					{
						TryAllocate(ref newItemAddress, (string)(object)item);
					}
					else if (typeof(T).BaseType == typeof(Handle))
					{
						TryConvert((Handle)(object)item, out newItemAddress);
					}
					else
					{
						TryAllocate(ref newItemAddress, item);
					}

					// Copy the item's address into the array
					IntPtr itemAddress = new IntPtr(target.ToInt64() + itemIndex * itemSize);
					Marshal.StructureToPtr(newItemAddress, itemAddress, false);
				}
				else
				{
					// Copy the data straight into memory
					IntPtr itemAddress = new IntPtr(target.ToInt64() + itemIndex * itemSize);
					Marshal.StructureToPtr(item, itemAddress, false);
				}
			}

			return true;
		}

		private static bool TryAllocateCacheOnly<T>(ref IntPtr target, T source)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			// The source should always be fetched directly from our cache, so the allocation is arbitrary.
			Allocation allocation;
			if (!TryMarshalAllocate(ref target, 1, out allocation))
			{
				return false;
			}

			allocation.SetCachedData(source);

			return true;
		}
#endregion

		// These functions are used for releasing unmanaged memory.
		// They should not be exposed outside of this helper.
#region Private Releasers
		private static bool TryRelease(ref IntPtr target)
		{
			if (target == IntPtr.Zero)
			{
				return false;
			}

			Allocation allocation = null;
			if (!s_Allocations.TryGetValue(target, out allocation))
			{
				return false;
			}

			if (allocation.IsCachedArrayElementAllocated.HasValue)
			{
				var itemSize = 0;
				if (allocation.IsCachedArrayElementAllocated.Value)
				{
					itemSize = Marshal.SizeOf(typeof(IntPtr));
				}
				else
				{
					itemSize = Marshal.SizeOf(allocation.CachedData.GetType().GetElementType());
				}

				var array = allocation.CachedData as Array;

				for (int itemIndex = 0; itemIndex < array.Length; ++itemIndex)
				{
					if (allocation.IsCachedArrayElementAllocated.Value)
					{
						var itemAddress = new IntPtr(target.ToInt64() + itemIndex * itemSize);
						itemAddress = Marshal.ReadIntPtr(itemAddress);
						TryRelease(ref itemAddress);
					}
					else
					{
						var item = array.GetValue(itemIndex);
						if (item is IDisposable)
						{
							var disposable = item as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
					}
				}
			}

			if (allocation.CachedData is IDisposable)
			{
				var disposable = allocation.CachedData as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}

			Marshal.FreeHGlobal(target);
			s_Allocations.Remove(target);
			target = IntPtr.Zero;

			return true;
		}
#endregion

		// These functions are used for fetching unmanaged memory.
		// They should not be exposed outside of this helper.
#region Private Fetchers
		private static bool TryFetch<T>(IntPtr source, out T target)
		{
			target = GetDefault<T>();

			if (source == IntPtr.Zero)
			{
				return false;
			}

			// If this is an allocation containing cached data, we should be able to fetch it from the cache
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.CachedData != null)
				{
					if (allocation.CachedData.GetType() == typeof(T))
					{
						target = (T)allocation.CachedData;
						return true;
					}
					else
					{
						throw new CachedTypeAllocationException(source, allocation.CachedData.GetType(), typeof(T));
					}
				}
			}

			target = (T)Marshal.PtrToStructure(source, typeof(T));
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T? target)
			where T : struct
		{
			target = GetDefault<T?>();

			if (source == IntPtr.Zero)
			{
				return false;
			}

			// If this is an allocation containing cached data, we should be able to fetch it from the cache
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.CachedData != null)
				{
					if (allocation.CachedData.GetType() == typeof(T))
					{
						target = (T?)allocation.CachedData;
						return true;
					}
					else
					{
						throw new CachedTypeAllocationException(source, allocation.CachedData.GetType(), typeof(T));
					}
				}
			}

			target = (T?)Marshal.PtrToStructure(source, typeof(T));
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T[] target, int arrayLength, bool isElementAllocated)
		{
			target = null;

			if (source == IntPtr.Zero)
			{
				return false;
			}

			// If this is an allocation containing cached data, we should be able to fetch it from the cache
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.CachedData != null)
				{
					if (allocation.CachedData.GetType() == typeof(T[]))
					{
						var cachedArray = (Array)allocation.CachedData;
						if (cachedArray.Length == arrayLength)
						{
							target = cachedArray as T[];
							return true;
						}
						else
						{
							throw new CachedArrayAllocationException(source, cachedArray.Length, arrayLength);
						}
					}
					else
					{
						throw new CachedTypeAllocationException(source, allocation.CachedData.GetType(), typeof(T[]));
					}
				}
			}

			var itemSize = 0;
			if (isElementAllocated)
			{
				itemSize = Marshal.SizeOf(typeof(IntPtr));
			}
			else
			{
				itemSize = Marshal.SizeOf(typeof(T));
			}

			List<T> items = new List<T>();
			for (int itemIndex = 0; itemIndex < arrayLength; ++itemIndex)
			{
				IntPtr itemAddress = new IntPtr(source.ToInt64() + itemIndex * itemSize);

				if (isElementAllocated)
				{
					itemAddress = Marshal.ReadIntPtr(itemAddress);
				}

				T item;
				TryFetch(itemAddress, out item);
				items.Add(item);
			}

			target = items.ToArray();
			return true;
		}

		private static bool TryFetch(IntPtr source, out string target)
		{
			target = null;

			if (source == IntPtr.Zero)
			{
				return false;
			}

			// Find the null terminator
			int length = 0;
			while (Marshal.ReadByte(source, length) != 0)
			{
				++length;
			}

			byte[] bytes = new byte[length];
			Marshal.Copy(source, bytes, 0, length);

			target = Encoding.UTF8.GetString(bytes);

			return true;
		}
#endregion

		// These functions are used for converting managed memory.
		// They should not be exposed outside of this helper.
#region Private Converters
		private static bool TryConvert<THandle>(IntPtr source, out THandle target)
			where THandle : Handle, new()
		{
			target = null;

			if (source != IntPtr.Zero)
			{
				target = new THandle();
				target.InnerHandle = source;
			}

			return true;
		}

		internal static bool TryConvert<TSource, TTarget>(TSource source, out TTarget target)
			where TTarget : ISettable, new()
		{
			target = GetDefault<TTarget>();

			if (source != null)
			{
				target = new TTarget();
				target.Set(source);
			}

			return true;
		}

		private static bool TryConvert(Handle source, out IntPtr target)
		{
			target = IntPtr.Zero;

			if (source != null)
			{
				target = source.InnerHandle;
			}

			return true;
		}

		private static bool TryConvert(byte[] source, out string target)
		{
			target = null;

			if (source == null)
			{
				return false;
			}

			int length = 0;
			foreach (byte currentByte in source)
			{
				if (currentByte == 0)
				{
					break;
				}

				++length;
			}

			target = Encoding.UTF8.GetString(source.Take(length).ToArray());

			return true;
		}

		private static bool TryConvert(string source, out byte[] target, int length)
		{
			if (source == null)
			{
				source = "";
			}

			target = Encoding.UTF8.GetBytes(new string(source.Take(length).ToArray()).PadRight(length, '\0'));

			return true;
		}

		private static bool TryConvert(string source, out byte[] target)
		{
			return TryConvert(source, out target, source.Length + 1);
		}

		private static bool TryConvert<T>(T[] source, out int target)
		{
			target = 0;

			if (source != null)
			{
				target = source.Length;
			}

			return true;
		}

		private static bool TryConvert<T>(T[] source, out uint target)
		{
			target = 0;

			int targetInt;
			if (TryConvert(source, out targetInt))
			{
				target = (uint)targetInt;
				return true;
			}

			return false;
		}

		internal static bool TryConvert<TSource, TTarget>(TSource[] source, out TTarget[] target)
			where TTarget : ISettable, new()
		{
			target = GetDefault<TTarget[]>();

			if (source != null)
			{
				target = new TTarget[source.Length];

				for (int index = 0; index < source.Length; ++index)
				{
					target[index] = new TTarget();
					target[index].Set(source[index]);
				}
			}

			return true;
		}
		
		private static bool TryConvert(int source, out bool target)
		{
			target = source != 0;

			return true;
		}

		private static bool TryConvert(bool source, out int target)
		{
			target = source ? 1 : 0;

			return true;
		}

		private static bool TryConvert(DateTimeOffset? source, out long target)
		{
			target = -1;

			if (source.HasValue)
			{
				DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				long unixTimestampTicks = (source.Value.UtcDateTime - unixStart).Ticks;
				long unixTimestampSeconds = unixTimestampTicks / TimeSpan.TicksPerSecond;
				target = unixTimestampSeconds;
			}

			return true;
		}

		private static bool TryConvert(long source, out DateTimeOffset? target)
		{
			target = null;

			if (source >= 0)
			{
				DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				long unixTimeStampTicks = source * TimeSpan.TicksPerSecond;
				target = new DateTimeOffset(unixStart.Ticks + unixTimeStampTicks, TimeSpan.Zero);
			}

			return true;
		}
#endregion

		// These functions exist to further streamline blocks of generated code.
#region Private Helpers
		private static bool CanRemoveCallback<TCallbackInfo>(IntPtr clientDataAddress, TCallbackInfo callbackInfo)
			where TCallbackInfo : ICallbackInfo
		{
			DelegateHolder delegateHolder = null;
			if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
			{
				if (delegateHolder.NotificationId.HasValue)
				{
					return false;
				}
			}

			if (callbackInfo.GetResultCode().HasValue)
			{
				return Common.IsOperationComplete(callbackInfo.GetResultCode().Value);
			}

			return true;
		}

		private static bool TryGetAndRemoveCallback<TCallback, TCallbackInfo>(IntPtr clientDataAddress, TCallbackInfo callbackInfo, out TCallback callback)
			where TCallback : class
			where TCallbackInfo : ICallbackInfo
		{
			callback = null;

			if (clientDataAddress != IntPtr.Zero && s_Callbacks.ContainsKey(clientDataAddress))
			{
				callback = s_Callbacks[clientDataAddress].Public as TCallback;
				if (callback != null)
				{
					if (CanRemoveCallback(clientDataAddress, callbackInfo))
					{
						s_Callbacks.Remove(clientDataAddress);
						TryRelease(ref clientDataAddress);
					}

					return true;
				}
			}

			return false;
		}

		internal static bool TryGetStaticCallback<TCallback>(string key, out TCallback callback)
			where TCallback : class
		{
			callback = null;

			if (s_StaticCallbacks.ContainsKey(key))
			{
				callback = s_StaticCallbacks[key].Public as TCallback;
				if (callback != null)
				{
					return true;
				}
			}

			return false;
		}

		private static bool TryGetStructCallback<TCallback>(IntPtr clientDataAddress, out TCallback structCallback)
			where TCallback : class
		{
			structCallback = null;

			if (clientDataAddress != IntPtr.Zero && s_Callbacks.ContainsKey(clientDataAddress))
			{
				structCallback = s_Callbacks[clientDataAddress].StructDelegates.FirstOrDefault(delegat => delegat.GetType() == typeof(TCallback)) as TCallback;
				if (structCallback != null)
				{
					return true;
				}
			}

			return false;
		}
#endregion
	}
}
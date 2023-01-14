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

	/// <summary>
	/// A helper class that manages memory in the wrapper.
	/// </summary>
	public sealed partial class Helper
	{
		private struct Allocation
		{
			public int Size { get; private set; }

			public object Cache { get; private set; }

			public bool? IsArrayItemAllocated { get; private set; }

			public Allocation(int size, object cache, bool? isArrayItemAllocated = null)
			{
				Size = size;
				Cache = cache;
				IsArrayItemAllocated = isArrayItemAllocated;
			}
		}
		private struct PinnedBuffer
		{
			public GCHandle Handle { get; private set; }

			public int RefCount { get; set; }

			public PinnedBuffer(GCHandle handle)
			{
				Handle = handle;
				RefCount = 1;
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
		private static Dictionary<IntPtr, PinnedBuffer> s_PinnedBuffers = new Dictionary<IntPtr, PinnedBuffer>();
		private static Dictionary<IntPtr, DelegateHolder> s_Callbacks = new Dictionary<IntPtr, DelegateHolder>();
		private static Dictionary<string, DelegateHolder> s_StaticCallbacks = new Dictionary<string, DelegateHolder>();
		private static long s_LastClientDataId = 0;
		private static Dictionary<IntPtr, object> s_ClientDatas = new Dictionary<IntPtr, object>();

		/// <summary>
		/// Gets the number of unmanaged allocations and other stored values in the wrapper. Use this to find leaks related to the usage of wrapper code.
		/// </summary>
		/// <returns>The number of unmanaged allocations currently active within the wrapper.</returns>
		public static int GetAllocationCount()
		{
			return s_Allocations.Count + s_PinnedBuffers.Aggregate(0, (acc, x) => acc + x.Value.RefCount) + s_Callbacks.Count + s_ClientDatas.Count;
		}

		internal static void Copy(byte[] from, IntPtr to)
		{
			if (from != null && to != IntPtr.Zero)
			{
				Marshal.Copy(from, 0, to, from.Length);
			}
		}

		internal static void Copy(ArraySegment<byte> from, IntPtr to)
		{
			if (from.Count != 0 && to != IntPtr.Zero)
			{
				Marshal.Copy(from.Array, from.Offset, to, from.Count);
			}
		}

		internal static void Dispose(ref IntPtr value)
		{
			RemoveAllocation(ref value);
			RemovePinnedBuffer(ref value);
		}

		internal static void Dispose<TDisposable>(ref TDisposable disposable)
			where TDisposable : IDisposable
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		internal static void Dispose<TEnum>(ref IntPtr value, TEnum currentEnum, TEnum expectedEnum)
		{
			if ((int)(object)currentEnum == (int)(object)expectedEnum)
			{
				Dispose(ref value);
			}
		}

		private static int GetAnsiStringLength(byte[] bytes)
		{
			int length = 0;
			foreach (byte currentByte in bytes)
			{
				if (currentByte == 0)
				{
					break;
				}

				++length;
			}

			return length;
		}

		private static int GetAnsiStringLength(IntPtr address)
		{
			int length = 0;
			while (Marshal.ReadByte(address, length) != 0)
			{
				++length;
			}

			return length;
		}

		internal static T GetDefault<T>()
		{
			return default(T);
		}

		private static void GetAllocation<T>(IntPtr source, out T target)
		{
			target = GetDefault<T>();

			if (source == IntPtr.Zero)
			{
				return;
			}

			object allocationCache;
			if (TryGetAllocationCache(source, out allocationCache))
			{
				if (allocationCache != null)
				{
					if (allocationCache.GetType() == typeof(T))
					{
						target = (T)allocationCache;
						return;
					}
					else
					{
						throw new CachedTypeAllocationException(source, allocationCache.GetType(), typeof(T));
					}
				}
			}

			target = (T)Marshal.PtrToStructure(source, typeof(T));
		}

		private static void GetAllocation<T>(IntPtr source, out T? target)
			where T : struct
		{
			target = GetDefault<T?>();

			if (source == IntPtr.Zero)
			{
				return;
			}

			// If this is an allocation containing cached data, we should be able to fetch it from the cache
			object allocationCache;
			if (TryGetAllocationCache(source, out allocationCache))
			{
				if (allocationCache != null)
				{
					if (allocationCache.GetType() == typeof(T))
					{
						target = (T?)allocationCache;
						return;
					}
					else
					{
						throw new CachedTypeAllocationException(source, allocationCache.GetType(), typeof(T));
					}
				}
			}

			target = (T?)Marshal.PtrToStructure(source, typeof(T));
		}

		private static void GetAllocation<THandle>(IntPtr source, out THandle[] target, int arrayLength)
			where THandle : Handle, new()
		{
			target = null;

			if (source == IntPtr.Zero)
			{
				return;
			}

			// If this is an allocation containing cached data, we should be able to fetch it from the cache
			object allocationCache;

			if (TryGetAllocationCache(source, out allocationCache))
			{
				if (allocationCache != null)
				{
					if (allocationCache.GetType() == typeof(THandle[]))
					{
						var cachedArray = (Array)allocationCache;
						if (cachedArray.Length == arrayLength)
						{
							target = cachedArray as THandle[];
							return;
						}
						else
						{
							throw new CachedArrayAllocationException(source, cachedArray.Length, arrayLength);
						}
					}
					else
					{
						throw new CachedTypeAllocationException(source, allocationCache.GetType(), typeof(THandle[]));
					}
				}
			}

			var itemSize = Marshal.SizeOf(typeof(IntPtr));

			List<THandle> items = new List<THandle>();
			for (int itemIndex = 0; itemIndex < arrayLength; ++itemIndex)
			{
				IntPtr itemAddress = new IntPtr(source.ToInt64() + itemIndex * itemSize);
				itemAddress = Marshal.ReadIntPtr(itemAddress);
				THandle item;
				Convert(itemAddress, out item);
				items.Add(item);
			}

			target = items.ToArray();
		}

		private static void GetAllocation<T>(IntPtr from, out T[] to, int arrayLength, bool isArrayItemAllocated)
		{
			to = null;

			if (from == IntPtr.Zero)
			{
				return;
			}

			// If this is an allocation containing cached data, we should be able to fetch it from the cache
			object allocationCache;
			if (TryGetAllocationCache(from, out allocationCache))
			{
				if (allocationCache != null)
				{
					if (allocationCache.GetType() == typeof(T[]))
					{
						var cachedArray = (Array)allocationCache;
						if (cachedArray.Length == arrayLength)
						{
							to = cachedArray as T[];
							return;
						}
						else
						{
							throw new CachedArrayAllocationException(from, cachedArray.Length, arrayLength);
						}
					}
					else
					{
						throw new CachedTypeAllocationException(from, allocationCache.GetType(), typeof(T[]));
					}
				}
			}

			int itemSize;
			if (isArrayItemAllocated)
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
				IntPtr itemAddress = new IntPtr(from.ToInt64() + itemIndex * itemSize);

				if (isArrayItemAllocated)
				{
					itemAddress = Marshal.ReadIntPtr(itemAddress);
				}

				T item;
				GetAllocation(itemAddress, out item);
				items.Add(item);
			}

			to = items.ToArray();
		}

		private static void GetAllocation(IntPtr source, out Utf8String target)
		{
			target = null;

			if (source == IntPtr.Zero)
			{
				return;
			}

			// C style strlen
			int length = GetAnsiStringLength(source);

			// +1 byte for the null terminator.
			byte[] bytes = new byte[length + 1];
			Marshal.Copy(source, bytes, 0, length + 1);

			target = new Utf8String(bytes);
		}

		internal static IntPtr AddAllocation(int size)
		{
			if (size == 0)
			{
				return IntPtr.Zero;
			}

			IntPtr address = Marshal.AllocHGlobal(size);
			Marshal.WriteByte(address, 0, 0);

			lock (s_Allocations)
			{
				s_Allocations.Add(address, new Allocation(size, null));
			}

			return address;
		}

		internal static IntPtr AddAllocation(uint size)
		{
			return AddAllocation((int)size);
		}

		private static IntPtr AddAllocation<T>(int size, T cache)
		{
			if (size == 0 || cache == null)
			{
				return IntPtr.Zero;
			}

			IntPtr address = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(cache, address, false);

			lock (s_Allocations)
			{
				s_Allocations.Add(address, new Allocation(size, cache));
			}

			return address;
		}

		private static IntPtr AddAllocation<T>(int size, T[] cache, bool? isArrayItemAllocated)
		{
			if (size == 0 || cache == null)
			{
				return IntPtr.Zero;
			}

			IntPtr address = Marshal.AllocHGlobal(size);
			Marshal.WriteByte(address, 0, 0);

			lock (s_Allocations)
			{
				s_Allocations.Add(address, new Allocation(size, cache, isArrayItemAllocated));
			}

			return address;
		}

		private static IntPtr AddAllocation<T>(T[] array, bool isArrayItemAllocated)
		{
			if (array == null)
			{
				return IntPtr.Zero;
			}

			int itemSize;
			if (isArrayItemAllocated)
			{
				itemSize = Marshal.SizeOf(typeof(IntPtr));
			}
			else
			{
				itemSize = Marshal.SizeOf(typeof(T));
			}

			IntPtr newArrayAddress = AddAllocation(array.Length * itemSize, array, isArrayItemAllocated);

			for (int itemIndex = 0; itemIndex < array.Length; ++itemIndex)
			{
				var item = (T)array.GetValue(itemIndex);

				if (isArrayItemAllocated)
				{
					IntPtr newItemAddress;
					if (typeof(T) == typeof(Utf8String))
					{
						newItemAddress = AddPinnedBuffer((Utf8String)(object)item);
					}
					else if (typeof(T).BaseType == typeof(Handle))
					{
						Convert((Handle)(object)item, out newItemAddress);
					}
					else
					{
						newItemAddress = AddAllocation(Marshal.SizeOf(typeof(T)), item);
					}

					// Copy the item's address into the array
					IntPtr itemAddress = new IntPtr(newArrayAddress.ToInt64() + itemIndex * itemSize);
					Marshal.StructureToPtr(newItemAddress, itemAddress, false);
				}
				else
				{
					// Copy the data straight into memory
					IntPtr itemAddress = new IntPtr(newArrayAddress.ToInt64() + itemIndex * itemSize);
					Marshal.StructureToPtr(item, itemAddress, false);
				}
			}

			return newArrayAddress;
		}

		private static void RemoveAllocation(ref IntPtr address)
		{
			if (address == IntPtr.Zero)
			{
				return;
			}

			Allocation allocation;
			lock (s_Allocations)
			{
				if (!s_Allocations.TryGetValue(address, out allocation))
				{
					return;
				}

				s_Allocations.Remove(address);
			}

			// If the allocation is an array, dispose and release its items as needbe.
			if (allocation.IsArrayItemAllocated.HasValue)
			{
				int itemSize;
				if (allocation.IsArrayItemAllocated.Value)
				{
					itemSize = Marshal.SizeOf(typeof(IntPtr));
				}
				else
				{
					itemSize = Marshal.SizeOf(allocation.Cache.GetType().GetElementType());
				}

				var array = allocation.Cache as Array;
				for (int itemIndex = 0; itemIndex < array.Length; ++itemIndex)
				{
					if (allocation.IsArrayItemAllocated.Value)
					{
						var itemAddress = new IntPtr(address.ToInt64() + itemIndex * itemSize);
						itemAddress = Marshal.ReadIntPtr(itemAddress);
						Dispose(ref itemAddress);
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

			if (allocation.Cache is IDisposable)
			{
				var disposable = allocation.Cache as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}

			Marshal.FreeHGlobal(address);
			address = IntPtr.Zero;
		}

		private static bool TryGetAllocationCache(IntPtr address, out object cache)
		{
			cache = null;

			lock (s_Allocations)
			{
				Allocation allocation;
				if (s_Allocations.TryGetValue(address, out allocation))
				{
					cache = allocation.Cache;
					return true;
				}
			}

			return false;
		}

		private static IntPtr AddPinnedBuffer(byte[] buffer, int offset)
		{
			if (buffer == null)
			{
				return IntPtr.Zero;
			}

			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr address = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);

			lock (s_PinnedBuffers)
			{
				// If the item is already pinned, increase the reference count.
				if (s_PinnedBuffers.ContainsKey(address))
				{
					// Since this is a structure, need to copy to modify the element.
					PinnedBuffer pinned = s_PinnedBuffers[address];
					pinned.RefCount++;
					s_PinnedBuffers[address] = pinned;
				}
				else
				{
					s_PinnedBuffers.Add(address, new PinnedBuffer(handle));
				}

				return address;
			}
		}

		private static IntPtr AddPinnedBuffer(Utf8String str)
		{
			if (str == null || str.Bytes == null)
			{
				return IntPtr.Zero;
			}

			return AddPinnedBuffer(str.Bytes, 0);
		}

		internal static IntPtr AddPinnedBuffer(ArraySegment<byte> array)
		{
			if (array == null)
			{
				return IntPtr.Zero;
			}

			return AddPinnedBuffer(array.Array, array.Offset);
		}

		private static void RemovePinnedBuffer(ref IntPtr address)
		{
			if (address == IntPtr.Zero)
			{
				return;
			}

			lock (s_PinnedBuffers)
			{
				PinnedBuffer pinnedBuffer;
				if (s_PinnedBuffers.TryGetValue(address, out pinnedBuffer))
				{
					// Deref the allocation.
					pinnedBuffer.Handle.Free();
					pinnedBuffer.RefCount--;

					// If the reference count is zero, remove the allocation from the list of tracked allocations.
					if (pinnedBuffer.RefCount == 0)
					{
						s_PinnedBuffers.Remove(address);
					}
					else
					{
						// Copy back the structure with the decreased reference count.
						s_PinnedBuffers[address] = pinnedBuffer;
					}
				}
			}

			address = IntPtr.Zero;
		}
	}
}
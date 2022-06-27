// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Linq;

namespace Epic.OnlineServices
{
	public sealed partial class Helper
	{
		/// <summary>
		/// Adds a callback to the wrapper.
		/// </summary>
		/// <param name="clientDataAddress">The generated client data address.</param>
		/// <param name="clientData">The client data of the callback.</param>
		/// <param name="publicDelegate">The public delegate of the callback.</param>
		/// <param name="privateDelegate">The private delegate of the callback.</param>
		/// <param name="structDelegates">Any delegates passed in with input structs.</param>
		internal static void AddCallback(out IntPtr clientDataAddress, object clientData, Delegate publicDelegate, Delegate privateDelegate, params Delegate[] structDelegates)
		{
			lock (s_Callbacks)
			{
				clientDataAddress = AddClientData(clientData);
				s_Callbacks.Add(clientDataAddress, new DelegateHolder(publicDelegate, privateDelegate, structDelegates));
			}
		}

		/// <summary>
		/// Removes a callback from the wrapper.
		/// </summary>
		/// <param name="clientDataAddress">The client data address of the callback.</param>
		private static void RemoveCallback(IntPtr clientDataAddress)
		{
			lock (s_Callbacks)
			{
				s_Callbacks.Remove(clientDataAddress);
				RemoveClientData(clientDataAddress);
			}
		}

		/// <summary>
		/// Tries to get the callback associated with the given internal callback info, and then removes it from the wrapper if applicable.
		/// Single-use callbacks will be cleaned up by this function.
		/// </summary>
		/// <typeparam name="TCallbackInfoInternal">The internal callback info type.</typeparam>
		/// <typeparam name="TCallback">The callback type.</typeparam>
		/// <typeparam name="TCallbackInfo">The callback info type.</typeparam>
		/// <param name="callbackInfoInternal">The internal callback info.</param>
		/// <param name="callback">The callback associated with the internal callback info.</param>
		/// <param name="callbackInfo">The callback info.</param>
		/// <returns>Whether the callback was successfully retrieved.</returns>
		internal static bool TryGetAndRemoveCallback<TCallbackInfoInternal, TCallback, TCallbackInfo>(ref TCallbackInfoInternal callbackInfoInternal, out TCallback callback, out TCallbackInfo callbackInfo)
			where TCallbackInfoInternal : struct, ICallbackInfoInternal, IGettable<TCallbackInfo>
			where TCallback : class
			where TCallbackInfo : struct, ICallbackInfo
		{
			IntPtr clientDataAddress;
			Get(ref callbackInfoInternal, out callbackInfo, out clientDataAddress);

			callback = null;

			lock (s_Callbacks)
			{
				DelegateHolder delegateHolder;
				if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
				{
					callback = delegateHolder.Public as TCallback;
					if (callback != null)
					{
						// If this delegate was added with an AddNotify, we should only ever remove it on RemoveNotify.
						if (delegateHolder.NotificationId.HasValue)
						{
						}

						// If the operation is complete, it's safe to remove.
						else if (callbackInfo.GetResultCode().HasValue && Common.IsOperationComplete(callbackInfo.GetResultCode().Value))
						{
							RemoveCallback(clientDataAddress);
						}

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Tries to get the struct callback associated with the given internal callback info.
		/// </summary>
		/// <typeparam name="TCallbackInfoInternal">The internal callback info type.</typeparam>
		/// <typeparam name="TCallback">The callback type.</typeparam>
		/// <typeparam name="TCallbackInfo">The callback info type.</typeparam>
		/// <param name="callbackInfoInternal">The internal callback info.</param>
		/// <param name="callback">The callback associated with the internal callback info.</param>
		/// <param name="callbackInfo">The callback info.</param>
		/// <returns>Whether the callback was successfully retrieved.</returns>
		internal static bool TryGetStructCallback<TCallbackInfoInternal, TCallback, TCallbackInfo>(ref TCallbackInfoInternal callbackInfoInternal, out TCallback callback, out TCallbackInfo callbackInfo)
			where TCallbackInfoInternal : struct, ICallbackInfoInternal, IGettable<TCallbackInfo>
			where TCallback : class
			where TCallbackInfo : struct
		{
			IntPtr clientDataAddress;
			Get(ref callbackInfoInternal, out callbackInfo, out clientDataAddress);

			callback = null;
			lock (s_Callbacks)
			{
				DelegateHolder delegateHolder;
				if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
				{
					callback = delegateHolder.StructDelegates.FirstOrDefault(structDelegate => structDelegate.GetType() == typeof(TCallback)) as TCallback;
					if (callback != null)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Removes a callback from the wrapper by an associated notification id.
		/// </summary>
		/// <param name="notificationId">The notification id associated with the callback.</param>
		internal static void RemoveCallbackByNotificationId(ulong notificationId)
		{
			lock (s_Callbacks)
			{
				var clientDataAddress = s_Callbacks.SingleOrDefault(pair => pair.Value.NotificationId.HasValue && pair.Value.NotificationId == notificationId);
				RemoveCallback(clientDataAddress.Key);
			}
		}

		/// <summary>
		/// Adds a static callback to the wrapper.
		/// </summary>
		/// <param name="key">The key of the callback.</param>
		/// <param name="publicDelegate">The public delegate of the callback.</param>
		/// <param name="privateDelegate">The private delegate of the callback</param>
		internal static void AddStaticCallback(string key, Delegate publicDelegate, Delegate privateDelegate)
		{
			lock (s_StaticCallbacks)
			{
				s_StaticCallbacks.Remove(key);
				s_StaticCallbacks.Add(key, new DelegateHolder(publicDelegate, privateDelegate));
			}
		}

		/// <summary>
		/// Tries to get the static callback associated with the given key.
		/// </summary>
		/// <typeparam name="TCallback">The callback type.</typeparam>
		/// <param name="key">The key of the callback.</param>
		/// <param name="callback">The callback associated with the key.</param>
		/// <returns>Whether the callback was successfully retrieved.</returns>
		internal static bool TryGetStaticCallback<TCallback>(string key, out TCallback callback)
			where TCallback : class
		{
			callback = null;

			lock (s_StaticCallbacks)
			{
				DelegateHolder delegateHolder;
				if (s_StaticCallbacks.TryGetValue(key, out delegateHolder))
				{
					callback = delegateHolder.Public as TCallback;
					if (callback != null)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Assigns a notification id to a callback by client data address associated with the callback.
		/// </summary>
		/// <param name="clientDataAddress">The client data address associated with the callback.</param>
		/// <param name="notificationId">The notification id to assign.</param>
		internal static void AssignNotificationIdToCallback(IntPtr clientDataAddress, ulong notificationId)
		{
			if (notificationId == 0)
			{
				RemoveCallback(clientDataAddress);
				return;
			}

			lock (s_Callbacks)
			{
				DelegateHolder delegateHolder;
				if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
				{
					delegateHolder.NotificationId = notificationId;
				}
			}
		}

		/// <summary>
		/// Adds client data to the wrapper.
		/// </summary>
		/// <param name="clientData">The client data to add.</param>
		/// <returns>The address of the added client data.</returns>
		private static IntPtr AddClientData(object clientData)
		{
			lock (s_ClientDatas)
			{
				long clientDataId = ++s_LastClientDataId;
				IntPtr clientDataAddress = new IntPtr(clientDataId);
				s_ClientDatas.Add(clientDataAddress, clientData);
				return clientDataAddress;
			}
		}

		/// <summary>
		/// Removes a client data from the wrapper.
		/// </summary>
		/// <param name="clientDataAddress">The address of the client data to remove.</param>
		private static void RemoveClientData(IntPtr clientDataAddress)
		{
			lock (s_ClientDatas)
			{
				s_ClientDatas.Remove(clientDataAddress);
			}
		}

		/// <summary>
		/// Gets client data by its address.
		/// </summary>
		/// <param name="clientDataAddress">The address of the client data.</param>
		/// <returns>Th client data associated with the address.</returns>
		private static object GetClientData(IntPtr clientDataAddress)
		{
			lock (s_ClientDatas)
			{
				object clientData;
				s_ClientDatas.TryGetValue(clientDataAddress, out clientData);
				return clientData;
			}
		}
	}
}
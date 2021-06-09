// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.KWS
{
	public sealed partial class KWSInterface : Handle
	{
		public KWSInterface()
		{
		}

		public KWSInterface(System.IntPtr innerHandle) : base(innerHandle)
		{
		}

		/// <summary>
		/// The most recent version of the <see cref="AddNotifyPermissionsUpdateReceived" /> API.
		/// </summary>
		public const int AddnotifypermissionsupdatereceivedApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="CopyPermissionByIndex" /> API.
		/// </summary>
		public const int CopypermissionbyindexApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="CreateUser" /> API.
		/// </summary>
		public const int CreateuserApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="CreateUser" /> API.
		/// </summary>
		public const int GetpermissionbykeyApiLatest = 1;

		public const int GetpermissionscountApiLatest = 1;

		/// <summary>
		/// Maximum size of the name for the permission
		/// </summary>
		public const int MaxPermissionLength = 32;

		/// <summary>
		/// Maximum number of permissions that may be requested
		/// </summary>
		public const int MaxPermissions = 16;

		/// <summary>
		/// The most recent version of the <see cref="PermissionStatus" /> API.
		/// </summary>
		public const int PermissionstatusApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="QueryAgeGate" /> API.
		/// </summary>
		public const int QueryagegateApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="QueryPermissions" /> API.
		/// </summary>
		public const int QuerypermissionsApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="RequestPermissions" /> API.
		/// </summary>
		public const int RequestpermissionsApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="UpdateParentEmail" /> API.
		/// </summary>
		public const int UpdateparentemailApiLatest = 1;

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Register to receive notifications about KWS permissions changes for any logged in local users
		/// @note must call <see cref="RemoveNotifyPermissionsUpdateReceived" /> to remove the notification
		/// </summary>
		/// <param name="options">Structure containing information about the request.</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate.</param>
		/// <param name="notificationFn">A callback that is fired when a notification is received.</param>
		/// <returns>
		/// handle representing the registered callback
		/// </returns>
		public ulong AddNotifyPermissionsUpdateReceived(AddNotifyPermissionsUpdateReceivedOptions options, object clientData, OnPermissionsUpdateReceivedCallback notificationFn)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyPermissionsUpdateReceivedOptionsInternal, AddNotifyPermissionsUpdateReceivedOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var notificationFnInternal = new OnPermissionsUpdateReceivedCallbackInternal(OnPermissionsUpdateReceivedCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, notificationFnInternal);

			var funcResult = Bindings.EOS_KWS_AddNotifyPermissionsUpdateReceived(InnerHandle, optionsAddress, clientDataAddress, notificationFnInternal);

			Helper.TryMarshalDispose(ref optionsAddress);

			Helper.TryAssignNotificationIdToCallback(clientDataAddress, funcResult);

			return funcResult;
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Fetch a permission for a given by index for a given local user
		/// <seealso cref="CreateUser" />
		/// <seealso cref="QueryPermissions" />
		/// <seealso cref="RequestPermissions" />
		/// <seealso cref="Release" />
		/// </summary>
		/// <param name="options">Structure containing the input parameters</param>
		/// <param name="outPermission">the permission for the given index, if it exists and is valid, use <see cref="Release" /> when finished</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the permission state is known for the given user and index
		/// <see cref="Result.NotFound" /> if the user is not found or the index is invalid
		/// </returns>
		public Result CopyPermissionByIndex(CopyPermissionByIndexOptions options, out PermissionStatus outPermission)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<CopyPermissionByIndexOptionsInternal, CopyPermissionByIndexOptions>(ref optionsAddress, options);

			var outPermissionAddress = System.IntPtr.Zero;

			var funcResult = Bindings.EOS_KWS_CopyPermissionByIndex(InnerHandle, optionsAddress, ref outPermissionAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			if (Helper.TryMarshalGet<PermissionStatusInternal, PermissionStatus>(outPermissionAddress, out outPermission))
			{
				Bindings.EOS_KWS_PermissionStatus_Release(outPermissionAddress);
			}

			return funcResult;
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Create an account with Kids Web Services and associate it with the local Product User ID
		/// </summary>
		/// <param name="options">options required for creating an account such as the local users Product User ID, their data of birth, and parental contact information</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if account creation completes successfully
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// <see cref="Result.TooManyRequests" /> if the number of allowed requests is exceeded
		/// </returns>
		public void CreateUser(CreateUserOptions options, object clientData, OnCreateUserCallback completionDelegate)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<CreateUserOptionsInternal, CreateUserOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new OnCreateUserCallbackInternal(OnCreateUserCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			Bindings.EOS_KWS_CreateUser(InnerHandle, optionsAddress, clientDataAddress, completionDelegateInternal);

			Helper.TryMarshalDispose(ref optionsAddress);
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Fetch the state of a given permission that are cached for a given local user.
		/// <seealso cref="CreateUser" />
		/// <seealso cref="QueryPermissions" />
		/// <seealso cref="RequestPermissions" />
		/// </summary>
		/// <param name="options">Structure containing the input parameters</param>
		/// <param name="outPermission">the permission for the given key, if it exists and is valid</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the permission state is known for the given user and key
		/// <see cref="Result.NotFound" /> if the user or the permission is not found
		/// </returns>
		public Result GetPermissionByKey(GetPermissionByKeyOptions options, out KWSPermissionStatus outPermission)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<GetPermissionByKeyOptionsInternal, GetPermissionByKeyOptions>(ref optionsAddress, options);

			outPermission = Helper.GetDefault<KWSPermissionStatus>();

			var funcResult = Bindings.EOS_KWS_GetPermissionByKey(InnerHandle, optionsAddress, ref outPermission);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Fetch the number of permissions found for a given local user
		/// </summary>
		/// <param name="options">Structure containing the input parameters</param>
		/// <returns>
		/// the number of permissions associated with the given user
		/// </returns>
		public int GetPermissionsCount(GetPermissionsCountOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<GetPermissionsCountOptionsInternal, GetPermissionsCountOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_KWS_GetPermissionsCount(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Query the client's country and age permissions for client side reasoning about the possible need enforce age based restrictions
		/// </summary>
		/// <param name="options">options required for interacting with the age gate system</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the query completes successfully
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// <see cref="Result.TooManyRequests" /> if the number of allowed queries is exceeded
		/// </returns>
		public void QueryAgeGate(QueryAgeGateOptions options, object clientData, OnQueryAgeGateCallback completionDelegate)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<QueryAgeGateOptionsInternal, QueryAgeGateOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new OnQueryAgeGateCallbackInternal(OnQueryAgeGateCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			Bindings.EOS_KWS_QueryAgeGate(InnerHandle, optionsAddress, clientDataAddress, completionDelegateInternal);

			Helper.TryMarshalDispose(ref optionsAddress);
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Query the current state of permissions for a given local Product User ID
		/// </summary>
		/// <param name="options">options required for querying permissions such as the local users Product User ID</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the account query completes successfully
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// <see cref="Result.TooManyRequests" /> if the number of allowed requests is exceeded
		/// </returns>
		public void QueryPermissions(QueryPermissionsOptions options, object clientData, OnQueryPermissionsCallback completionDelegate)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<QueryPermissionsOptionsInternal, QueryPermissionsOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new OnQueryPermissionsCallbackInternal(OnQueryPermissionsCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			Bindings.EOS_KWS_QueryPermissions(InnerHandle, optionsAddress, clientDataAddress, completionDelegateInternal);

			Helper.TryMarshalDispose(ref optionsAddress);
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Unregister from receiving notifications about KWS permissions related to logged in users
		/// </summary>
		/// <param name="inId">Handle representing the registered callback</param>
		public void RemoveNotifyPermissionsUpdateReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);

			Bindings.EOS_KWS_RemoveNotifyPermissionsUpdateReceived(InnerHandle, inId);
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Request new permissions for a given local Product User ID
		/// </summary>
		/// <param name="options">options required for updating permissions such as the new list of permissions</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if contact information update completes successfully
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// <see cref="Result.TooManyRequests" /> if the number of allowed requests is exceeded
		/// <see cref="Result.ParentEmailMissing" /> if the account requesting permissions has no parent email associated with it
		/// <see cref="Result.LimitExceeded" /> if the number of permissions exceeds <see cref="MaxPermissions" />, or if any permission name exceeds <see cref="MaxPermissionLength" />
		/// </returns>
		public void RequestPermissions(RequestPermissionsOptions options, object clientData, OnRequestPermissionsCallback completionDelegate)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<RequestPermissionsOptionsInternal, RequestPermissionsOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new OnRequestPermissionsCallbackInternal(OnRequestPermissionsCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			Bindings.EOS_KWS_RequestPermissions(InnerHandle, optionsAddress, clientDataAddress, completionDelegateInternal);

			Helper.TryMarshalDispose(ref optionsAddress);
		}

		/// <summary>
		/// This interface is not available for general access at this time.
		/// 
		/// Update the parent contact information for a given local Product User ID
		/// </summary>
		/// <param name="options">options required for updating the contact information such as the new email address</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if contact information update completes successfully
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// <see cref="Result.TooManyRequests" /> if the number of allowed requests is exceeded
		/// </returns>
		public void UpdateParentEmail(UpdateParentEmailOptions options, object clientData, OnUpdateParentEmailCallback completionDelegate)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<UpdateParentEmailOptionsInternal, UpdateParentEmailOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new OnUpdateParentEmailCallbackInternal(OnUpdateParentEmailCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			Bindings.EOS_KWS_UpdateParentEmail(InnerHandle, optionsAddress, clientDataAddress, completionDelegateInternal);

			Helper.TryMarshalDispose(ref optionsAddress);
		}

		[MonoPInvokeCallback(typeof(OnCreateUserCallbackInternal))]
		internal static void OnCreateUserCallbackInternalImplementation(System.IntPtr data)
		{
			OnCreateUserCallback callback;
			CreateUserCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<OnCreateUserCallback, CreateUserCallbackInfoInternal, CreateUserCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnPermissionsUpdateReceivedCallbackInternal))]
		internal static void OnPermissionsUpdateReceivedCallbackInternalImplementation(System.IntPtr data)
		{
			OnPermissionsUpdateReceivedCallback callback;
			PermissionsUpdateReceivedCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<OnPermissionsUpdateReceivedCallback, PermissionsUpdateReceivedCallbackInfoInternal, PermissionsUpdateReceivedCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryAgeGateCallbackInternal))]
		internal static void OnQueryAgeGateCallbackInternalImplementation(System.IntPtr data)
		{
			OnQueryAgeGateCallback callback;
			QueryAgeGateCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<OnQueryAgeGateCallback, QueryAgeGateCallbackInfoInternal, QueryAgeGateCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryPermissionsCallbackInternal))]
		internal static void OnQueryPermissionsCallbackInternalImplementation(System.IntPtr data)
		{
			OnQueryPermissionsCallback callback;
			QueryPermissionsCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<OnQueryPermissionsCallback, QueryPermissionsCallbackInfoInternal, QueryPermissionsCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnRequestPermissionsCallbackInternal))]
		internal static void OnRequestPermissionsCallbackInternalImplementation(System.IntPtr data)
		{
			OnRequestPermissionsCallback callback;
			RequestPermissionsCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<OnRequestPermissionsCallback, RequestPermissionsCallbackInfoInternal, RequestPermissionsCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUpdateParentEmailCallbackInternal))]
		internal static void OnUpdateParentEmailCallbackInternalImplementation(System.IntPtr data)
		{
			OnUpdateParentEmailCallback callback;
			UpdateParentEmailCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<OnUpdateParentEmailCallback, UpdateParentEmailCallbackInfoInternal, UpdateParentEmailCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
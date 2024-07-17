// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Lobby
{
	public sealed partial class LobbySearch : Handle
	{
		public LobbySearch()
		{
		}

		public LobbySearch(System.IntPtr innerHandle) : base(innerHandle)
		{
		}

		/// <summary>
		/// The most recent version of the <see cref="CopySearchResultByIndex" /> API.
		/// </summary>
		public const int LobbysearchCopysearchresultbyindexApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="Find" /> API.
		/// </summary>
		public const int LobbysearchFindApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="GetSearchResultCount" /> API.
		/// </summary>
		public const int LobbysearchGetsearchresultcountApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="RemoveParameter" /> API.
		/// </summary>
		public const int LobbysearchRemoveparameterApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetLobbyId" /> API.
		/// </summary>
		public const int LobbysearchSetlobbyidApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetMaxResults" /> API.
		/// </summary>
		public const int LobbysearchSetmaxresultsApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetParameter" /> API.
		/// </summary>
		public const int LobbysearchSetparameterApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetTargetUserId" /> API.
		/// </summary>
		public const int LobbysearchSettargetuseridApiLatest = 1;

		/// <summary>
		/// <see cref="CopySearchResultByIndex" /> is used to immediately retrieve a handle to the lobby information from a given search result.
		/// If the call returns an <see cref="Result.Success" /> result, the out parameter, OutLobbyDetailsHandle, must be passed to <see cref="LobbyDetails.Release" /> to release the memory associated with it.
		/// <seealso cref="LobbySearchCopySearchResultByIndexOptions" />
		/// <seealso cref="LobbyDetails.Release" />
		/// </summary>
		/// <param name="options">Structure containing the input parameters</param>
		/// <param name="outLobbyDetailsHandle">out parameter used to receive the lobby details handle</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the information is available and passed out in OutLobbyDetailsHandle
		/// <see cref="Result.InvalidParameters" /> if you pass an invalid index or a null pointer for the out parameter
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result CopySearchResultByIndex(ref LobbySearchCopySearchResultByIndexOptions options, out LobbyDetails outLobbyDetailsHandle)
		{
			LobbySearchCopySearchResultByIndexOptionsInternal optionsInternal = new LobbySearchCopySearchResultByIndexOptionsInternal();
			optionsInternal.Set(ref options);

			var outLobbyDetailsHandleAddress = System.IntPtr.Zero;

			var funcResult = Bindings.EOS_LobbySearch_CopySearchResultByIndex(InnerHandle, ref optionsInternal, ref outLobbyDetailsHandleAddress);

			Helper.Dispose(ref optionsInternal);

			Helper.Get(outLobbyDetailsHandleAddress, out outLobbyDetailsHandle);

			return funcResult;
		}

		/// <summary>
		/// Find lobbies matching the search criteria setup via this lobby search handle.
		/// When the operation completes, this handle will have the search results that can be parsed
		/// </summary>
		/// <param name="options">Structure containing information about the search criteria to use</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the search operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the find operation completes successfully
		/// <see cref="Result.NotFound" /> if searching for an individual lobby by lobby ID or target user ID returns no results
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// </returns>
		public void Find(ref LobbySearchFindOptions options, object clientData, LobbySearchOnFindCallback completionDelegate)
		{
			LobbySearchFindOptionsInternal optionsInternal = new LobbySearchFindOptionsInternal();
			optionsInternal.Set(ref options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new LobbySearchOnFindCallbackInternal(OnFindCallbackInternalImplementation);
			Helper.AddCallback(out clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			Bindings.EOS_LobbySearch_Find(InnerHandle, ref optionsInternal, clientDataAddress, completionDelegateInternal);

			Helper.Dispose(ref optionsInternal);
		}

		/// <summary>
		/// Get the number of search results found by the search parameters in this search
		/// </summary>
		/// <param name="options">Options associated with the search count</param>
		/// <returns>
		/// return the number of search results found by the query or 0 if search is not complete
		/// </returns>
		public uint GetSearchResultCount(ref LobbySearchGetSearchResultCountOptions options)
		{
			LobbySearchGetSearchResultCountOptionsInternal optionsInternal = new LobbySearchGetSearchResultCountOptionsInternal();
			optionsInternal.Set(ref options);

			var funcResult = Bindings.EOS_LobbySearch_GetSearchResultCount(InnerHandle, ref optionsInternal);

			Helper.Dispose(ref optionsInternal);

			return funcResult;
		}

		/// <summary>
		/// Release the memory associated with a lobby search. This must be called on data retrieved from <see cref="LobbyInterface.CreateLobbySearch" />.
		/// <seealso cref="LobbyInterface.CreateLobbySearch" />
		/// </summary>
		/// <param name="lobbySearchHandle">- The lobby search handle to release</param>
		public void Release()
		{
			Bindings.EOS_LobbySearch_Release(InnerHandle);
		}

		/// <summary>
		/// Remove a parameter from the array of search criteria.
		/// </summary>
		/// <param name="options">a search parameter key name to remove</param>
		/// <returns>
		/// <see cref="Result.Success" /> if removing this search parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the search key is invalid or null
		/// <see cref="Result.NotFound" /> if the parameter was not a part of the search criteria
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result RemoveParameter(ref LobbySearchRemoveParameterOptions options)
		{
			LobbySearchRemoveParameterOptionsInternal optionsInternal = new LobbySearchRemoveParameterOptionsInternal();
			optionsInternal.Set(ref options);

			var funcResult = Bindings.EOS_LobbySearch_RemoveParameter(InnerHandle, ref optionsInternal);

			Helper.Dispose(ref optionsInternal);

			return funcResult;
		}

		/// <summary>
		/// Set a lobby ID to find and will return at most one search result. Setting TargetUserId or SearchParameters will result in <see cref="Find" /> failing
		/// </summary>
		/// <param name="options">A specific lobby ID for which to search</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this lobby ID was successful
		/// <see cref="Result.InvalidParameters" /> if the lobby ID is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetLobbyId(ref LobbySearchSetLobbyIdOptions options)
		{
			LobbySearchSetLobbyIdOptionsInternal optionsInternal = new LobbySearchSetLobbyIdOptionsInternal();
			optionsInternal.Set(ref options);

			var funcResult = Bindings.EOS_LobbySearch_SetLobbyId(InnerHandle, ref optionsInternal);

			Helper.Dispose(ref optionsInternal);

			return funcResult;
		}

		/// <summary>
		/// Set the maximum number of search results to return in the query, can't be more than <see cref="LobbyInterface.MaxSearchResults" />
		/// </summary>
		/// <param name="options">maximum number of search results to return in the query</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting the max results was successful
		/// <see cref="Result.InvalidParameters" /> if the number of results requested is invalid
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetMaxResults(ref LobbySearchSetMaxResultsOptions options)
		{
			LobbySearchSetMaxResultsOptionsInternal optionsInternal = new LobbySearchSetMaxResultsOptionsInternal();
			optionsInternal.Set(ref options);

			var funcResult = Bindings.EOS_LobbySearch_SetMaxResults(InnerHandle, ref optionsInternal);

			Helper.Dispose(ref optionsInternal);

			return funcResult;
		}

		/// <summary>
		/// Add a parameter to an array of search criteria combined via an implicit AND operator. Setting LobbyId or TargetUserId will result in <see cref="Find" /> failing
		/// <seealso cref="AttributeData" />
		/// <seealso cref="ComparisonOp" />
		/// </summary>
		/// <param name="options">a search parameter and its comparison op</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this search parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the search criteria is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetParameter(ref LobbySearchSetParameterOptions options)
		{
			LobbySearchSetParameterOptionsInternal optionsInternal = new LobbySearchSetParameterOptionsInternal();
			optionsInternal.Set(ref options);

			var funcResult = Bindings.EOS_LobbySearch_SetParameter(InnerHandle, ref optionsInternal);

			Helper.Dispose(ref optionsInternal);

			return funcResult;
		}

		/// <summary>
		/// Set a target user ID to find. Setting LobbyId or SearchParameters will result in <see cref="Find" /> failing
		/// a search result will only be found if this user is in a public lobby
		/// </summary>
		/// <param name="options">a specific target user ID to find</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this target user ID was successful
		/// <see cref="Result.InvalidParameters" /> if the target user ID is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetTargetUserId(ref LobbySearchSetTargetUserIdOptions options)
		{
			LobbySearchSetTargetUserIdOptionsInternal optionsInternal = new LobbySearchSetTargetUserIdOptionsInternal();
			optionsInternal.Set(ref options);

			var funcResult = Bindings.EOS_LobbySearch_SetTargetUserId(InnerHandle, ref optionsInternal);

			Helper.Dispose(ref optionsInternal);

			return funcResult;
		}

		[MonoPInvokeCallback(typeof(LobbySearchOnFindCallbackInternal))]
		internal static void OnFindCallbackInternalImplementation(ref LobbySearchFindCallbackInfoInternal data)
		{
			LobbySearchOnFindCallback callback;
			LobbySearchFindCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback(ref data, out callback, out callbackInfo))
			{
				callback(ref callbackInfo);
			}
		}
	}
}
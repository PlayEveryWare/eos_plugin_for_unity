/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace PlayEveryWare.EpicOnlineServices.Tests.Services.Lobby
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Lobby;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Result = Epic.OnlineServices.Result;

    public abstract class LobbyTestBase : EOSTestBase
    {
        protected static void LeaveLobby(string lobbyId)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
            {
                return;
            }
            // Leave the lobby room
            LeaveLobbyOptions options = new()
            {
                LobbyId = lobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
            };

            LeaveLobbyCallbackInfo? leaveLobbyResult = null;
            EOSManager.Instance.GetEOSLobbyInterface().LeaveLobby(
                ref options, null, (ref LeaveLobbyCallbackInfo data) =>
                {
                    leaveLobbyResult = data;
                }
                );

            Task.Run(
                () => new WaitUntilDone(GlobalTestTimeout, () => leaveLobbyResult != null)
            ).Wait();

            Assert.IsNotNull(leaveLobbyResult);
            Assert.That(leaveLobbyResult.Value.ResultCode == Result.Success,
                $"Leave Lobby did not succeed: error code " +
                $"{leaveLobbyResult.Value.ResultCode}");
        }

        protected bool TryCreateLobbySearch(out LobbySearch lobbySearch, uint maxResults = 10)
        {
            CreateLobbySearchOptions options = new()
            {
                MaxResults = maxResults
            };

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref options, out lobbySearch);

            Assert.AreEqual(Result.Success, result, $"Could not create lobby search. Error Code: {result}.");

            return (Result.Success == result);
        }

        protected bool TrySetLobbySearchParameters(ref LobbySearch lobbySearchHandle, string key, string value)
        {
            LobbySearchSetParameterOptions paramOptions = new()
            {
                ComparisonOp = ComparisonOp.Equal
            };

            paramOptions.Parameter = new AttributeData()
            {
                Key = key,
                Value = new AttributeDataValue { AsUtf8 = value }
            };

            Result result = lobbySearchHandle.SetParameter(ref paramOptions);
            Assert.AreEqual(Result.Success, result, $"Failed to update search with the bucket id. Error code: {result}");

            return (Result.Success == result);
        }
        protected async Task<LobbyDetails> FindLobby(string lobbyId)
        {
            // Set specific search options for finding a lobby by its ID.
            var configureSearch = new Action<LobbySearch>((lobbySearch) =>
            {
                LobbySearchSetLobbyIdOptions idOptions = new()
                {
                    LobbyId = lobbyId
                };

                Result result = lobbySearch.SetLobbyId(ref idOptions);
                Assert.AreEqual(Result.Success, result, $"Failed to update search with the lobby id. Error code: {result}");
            });

            // Call the common method with the specific configuration for lobby ID search.
            IList<LobbyDetails> lobbyResults = await FindLobbiesInternal(configureSearch);

            if (lobbyResults.Count == 0)
                return null;
            else
                return lobbyResults[0];
        }

        protected async Task<IList<LobbyDetails>> FindLobbies(string key, string value)
        {
            // Set specific search options for finding lobbies based on key and value.
            var configureSearch = new Action<LobbySearch>((lobbySearch) =>
            {
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    _ = TrySetLobbySearchParameters(ref lobbySearch, key, value);
                }
            });

            // Call the common method with the specific configuration for key-value search.
            return await FindLobbiesInternal(configureSearch);
        }

        protected async Task<IList<LobbyDetails>> FindLobbies()
        {
            // Call the overload of FindLobbies with null key and value.
            return await FindLobbies(null, null);
        }

        private async Task<IList<LobbyDetails>> FindLobbiesInternal(Action<LobbySearch> configureSearch)
        {
            // Create the lobby search object.
            _ = TryCreateLobbySearch(out LobbySearch lobbySearch);

            // Apply the specific configuration passed in.
            configureSearch(lobbySearch);

            // Common logic for finding lobbies and handling callbacks.
            LobbySearchFindOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            var tcs = new TaskCompletionSource<LobbySearchFindCallbackInfo?>();

            lobbySearch.Find(ref options, null, (ref LobbySearchFindCallbackInfo data) =>
            {
                tcs.SetResult(data);
            });

            Task.Run(() => new WaitUntilDone(GlobalTestTimeout, () => tcs.Task.IsCompleted)).Wait();

            // Await the completion of the callback.
            LobbySearchFindCallbackInfo? callbackInfo = await tcs.Task;

            if (callbackInfo == null)
            {
                // Handle the null case appropriately.
                return new List<LobbyDetails>();
            }

            // Retrieve the results of the search.
            LobbySearchGetSearchResultCountOptions countOptions = new();
            uint searchResultCount = lobbySearch.GetSearchResultCount(ref countOptions);
            List<LobbyDetails> lobbiesFound = new();

            for (uint resultIndex = 0; resultIndex < searchResultCount; resultIndex++)
            {
                LobbySearchCopySearchResultByIndexOptions indexOptions = new() { LobbyIndex = resultIndex };
                Result resultOfCopyingSearchResult = lobbySearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails lobbyDetails);
                Assert.AreEqual(Result.Success, resultOfCopyingSearchResult,
                    $"Could not copy search results from index {resultIndex}.");

                lobbiesFound.Add(lobbyDetails);
            }

            return lobbiesFound;
        }

    }
}
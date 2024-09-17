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
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using Result = Epic.OnlineServices.Result;

    public abstract class LobbyTestBase : EOSTestBase
    {
        protected static IEnumerator CleanupLobby(string lobbyId)
        {
            if (!string.IsNullOrWhiteSpace(lobbyId))
            {
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

                yield return new WaitUntilDone(
                    GlobalTestTimeout, () => leaveLobbyResult != null
                    );

                Assert.IsNotNull(leaveLobbyResult);
                Assert.That(leaveLobbyResult.Value.ResultCode == Result.Success,
                    $"Leave Lobby did not succeed: error code " +
                    $"{leaveLobbyResult.Value.ResultCode}");
            }
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

        protected async Task<IList<LobbyDetails>> TryFindLobby(string key, string value)
        {
            _ = TryCreateLobbySearch(out LobbySearch lobbySearch);
            _ = TrySetLobbySearchParameters(ref lobbySearch, key, value);

            LobbySearchFindOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            LobbySearchFindCallbackInfo? temp = null;

            lobbySearch.Find(ref options, null, (ref LobbySearchFindCallbackInfo data) => temp = data);

            // Wait asynchronously until the result is not null
            await Task.Run(() =>
                new WaitUntil(() => temp != null)
            );

            LobbySearchGetSearchResultCountOptions countOptions = new();
            uint searchResultCount = lobbySearch.GetSearchResultCount(ref countOptions);
            List<LobbyDetails> lobbiesFound = new();

            for(uint resultIndex = 0; resultIndex < searchResultCount; resultIndex++)
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
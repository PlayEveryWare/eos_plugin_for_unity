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
    using Tests;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Lobby;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Lobby connection tests that test connecting to an existing lobby.
    /// </summary>
    public class ClientLobbyTests : LobbyTestBase
    {
        private string _lobbyId;
        private NotifyEventHandle _lobbyInviteNotification;

        [SetUp]
        public void Initialize()
        {
            _lobbyId = null;
            _lobbyInviteNotification = null;
        }

        /// <summary>
        /// Leaves the lobby once the test case ends.
        /// </summary>
        [UnityTearDown]
        public IEnumerator ClientLobbyTests_Teardown()
        {
            IEnumerator cleanupEnumerator = CleanupLobby(_lobbyId);
            _lobbyInviteNotification?.Dispose();
            return cleanupEnumerator;
        }

        /// <summary>
        /// Search by the bucket id for the server preset and join it.
        /// </summary>
        [Test]
        [Category(TestCategories.ClientCategory)]
        public async void FindByBucketIdAndJoin()
        {
            IList<LobbyDetails> lobbiesFound = await TryFindLobby(TestCommon.SearchBucketIdKey, TestCommon.LobbyBucketId);

            Assert.AreEqual(1, lobbiesFound.Count, $"There should be only one result, got {lobbiesFound.Count} instead.");

            LobbyDetails lobbyToJoin = lobbiesFound[0];

            // Now that we have the lobby we're looking for, join it
            JoinLobbyOptions joinOptions = new()
            {
                LobbyDetailsHandle = lobbyToJoin,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                PresenceEnabled = false
            };

            JoinLobbyCallbackInfo? joinResult = null;
            EOSManager.Instance.GetEOSLobbyInterface().JoinLobby(
                ref joinOptions, null,
                (ref JoinLobbyCallbackInfo data) => joinResult = data);

            await Task.Run(() =>
                new WaitUntil(() => joinResult != null)
            );

            Assert.AreEqual(Result.Success, joinResult.Value.ResultCode,
                $"Could not join the server lobby. Error code: {joinResult.Value.ResultCode}");

            _lobbyId = joinResult.Value.LobbyId;
        }

        /// <summary>
        /// Search by the bucket id for the private server and shouldn't be able to find it.
        /// </summary>
        [Test]
        [Category(TestCategories.ClientCategory)]
        public async void TryToFindPrivateLobby()
        {
            IList<LobbyDetails> lobbiesFound = await TryFindLobby(TestCommon.SearchBucketIdKey, TestCommon.LobbyPrivateBucketId);

            Assert.AreEqual(0, lobbiesFound.Count, $"There should not be any result, got {lobbiesFound.Count} instead.");
        }
    }
}

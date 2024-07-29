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

namespace PlayEveryWare.EpicOnlineServices.Tests.IntegrationTests
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Sessions;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;
    using PlayEveryWare.EpicOnlineServices.Samples;
    using System;
    using UnityEngine.Events;

    public class EOSSessionsManagerTests : EOSTestBase
    {
        protected EOSSessionsManager ManagerInstance { get; set; } = null;

        [OneTimeSetUp]
        public void OneTimeSetUpEOSSessionsManager()
        {
            ManagerInstance = new EOSSessionsManager();
            Debug.Log($"{nameof(OneTimeSetUpEOSSessionsManager)}");
        }

        [SetUp]
        public void SetUpLogIn()
        {
            ManagerInstance.OnLoggedIn();
            Debug.Log($"{nameof(SetUpLogIn)}");
        }

        [TearDown]
        public void TearDownLogOut()
        {
            ManagerInstance.OnLoggedOut();
            Debug.Log($"{nameof(TearDownLogOut)}");
        }

        /// <summary>
        /// Essential test of the most basic functions of the Manager.
        /// Creates a Session, observes that it was created successfully, leaves the Session.
        /// </summary>
        [UnityTest]
        public IEnumerator LoginCreateSessionLeaveSession()
        {
            const string sessionCreatedName = nameof(LoginCreateSessionLeaveSession);

            Session sessionCreationParameters = new Session();
            sessionCreationParameters.AllowJoinInProgress = false;
            sessionCreationParameters.InvitesAllowed = false;
            sessionCreationParameters.SanctionsEnabled = false;
            sessionCreationParameters.MaxPlayers = 1;
            sessionCreationParameters.Name = sessionCreatedName;
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            SessionAttribute attribute = new SessionAttribute();
            attribute.Key = "Level";
            attribute.AsString = "UNITYTESTS";
            attribute.ValueType = AttributeType.String;
            attribute.Advertisement = SessionAttributeAdvertisementType.Advertise;

            sessionCreationParameters.Attributes.Add(attribute);

            string resultingCreationSessionName = null;
            Result? resultingCreationResult = null;

            UnityAction<string, Result> handleCreationResult = (string relevantSession, Result relevantResult) =>
            {
                resultingCreationSessionName = relevantSession;
                resultingCreationResult = relevantResult;
            };

            ManagerInstance.UIOnSessionCreated.AddListener(handleCreationResult);
            ManagerInstance.CreateSession(sessionCreationParameters);

            yield return new WaitUntil(() => resultingCreationResult.HasValue);

            ManagerInstance.UIOnSessionCreated.RemoveListener(handleCreationResult);

            Assert.AreEqual(Result.Success, resultingCreationResult.Value, $"Failed to create Session.");
            Assert.NotNull(resultingCreationSessionName, $"Created Session name is null.");
            Assert.AreEqual(sessionCreatedName, resultingCreationSessionName);

            bool canFindSession = ManagerInstance.TryGetSession(sessionCreatedName, out Session foundLocalSession);
            Assert.True(canFindSession, $"Could not find Session that was created in local Sessions.");
            Assert.AreEqual(sessionCreationParameters, foundLocalSession, $"Created Session is not identical to resultant created Session.");
            Assert.NotNull(foundLocalSession.ActiveSession, "Created Session does not have ActiveSession information populated after joining.");

            string resultingDeletionSessionName = null;
            Result? resultingDeletionResult = null;

            UnityAction<string, Result> handleDestructionResult = (string relevantSession, Result relevantResult) =>
            {
                resultingDeletionSessionName = relevantSession;
                resultingDeletionResult = relevantResult;
            };

            ManagerInstance.UIOnSessionDestroyed.AddListener(handleDestructionResult);
            ManagerInstance.DestroySession(sessionCreatedName);

            yield return new WaitUntil(() => resultingDeletionResult.HasValue);

            ManagerInstance.UIOnSessionDestroyed.RemoveListener(handleDestructionResult);

            Assert.AreEqual(Result.Success, resultingDeletionResult.Value, $"Failed to destroy Session.");
            Assert.NotNull(resultingDeletionSessionName, $"Deleted Session name is null.");
            Assert.AreEqual(sessionCreatedName, resultingDeletionSessionName);

            bool triedToFindSession = ManagerInstance.TryGetSession(sessionCreatedName, out Session _);
            Assert.IsFalse(triedToFindSession, $"Found local Session that should be destroyed.");
        }
    }
}

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
    using System.Text;
    using System.Collections.Generic;

    public class EOSSessionsManagerTests : EOSTestBase
    {
        protected EOSSessionsManager ManagerInstance { get; set; } = null;

        /// <summary>
        /// The Epic Online Services Game Services back end takes some fuzzy amount of time to process changes.
        /// For example, a created game won't appear immediately in search results, even if the creation operation succeeded.
        /// Tests should use this constant amount of wait time before running online queries.
        /// </summary>
        const float WaitForSearchResultsAfterCreationOrModification = 3f;

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

        [UnityTearDown]
        public IEnumerator TearDownLogOut()
        {
            // If there are any Sessions that are remaining locally, they all need to be left
            // This may not successfully destroy all Sessions, but should at least attempt
            ManagerInstance.LeaveAllSessions();

            yield return new WaitUntilDone(60f, () =>
            {
                return ManagerInstance.GetCurrentSessions().Count == 0;
            });

            ManagerInstance.OnLoggedOut();
        }

        /// <summary>
        /// Essential test of the most basic functions of the Manager.
        /// Creates a Session, observes that it was created successfully, leaves the Session.
        /// </summary>
        [UnityTest]
        public IEnumerator CreateSessionLeaveSession()
        {
            Session randomTestSession = GetGenericSaturatedSession(nameof(CreateSessionLeaveSession));
            string sessionName = randomTestSession.Name;

            string resultingCreationSessionName = null;
            Result? resultingCreationResult = null;

            UnityAction<string, Result> handleCreationResult = (string relevantSession, Result relevantResult) =>
            {
                resultingCreationSessionName = relevantSession;
                resultingCreationResult = relevantResult;
            };

            ManagerInstance.UIOnSessionCreated.AddListener(handleCreationResult);
            ManagerInstance.CreateSession(randomTestSession);

            yield return new WaitUntil(() => resultingCreationResult.HasValue);

            ManagerInstance.UIOnSessionCreated.RemoveListener(handleCreationResult);

            Assert.AreEqual(Result.Success, resultingCreationResult.Value, $"Failed to create Session.");
            Assert.NotNull(resultingCreationSessionName, $"Created Session name is null.");
            Assert.AreEqual(sessionName, resultingCreationSessionName);

            bool canFindSession = ManagerInstance.TryGetSession(sessionName, out Session foundLocalSession);
            Assert.True(canFindSession, $"Could not find Session that was created in local Sessions.");
            Assert.AreEqual(randomTestSession, foundLocalSession, $"Created Session is not identical to resultant created Session.");
            Assert.NotNull(foundLocalSession.ActiveSession, "Created Session does not have ActiveSession information populated after joining.");

            string resultingDeletionSessionName = null;
            Result? resultingDeletionResult = null;

            UnityAction<string, Result> handleDestructionResult = (string relevantSession, Result relevantResult) =>
            {
                resultingDeletionSessionName = relevantSession;
                resultingDeletionResult = relevantResult;
            };

            ManagerInstance.UIOnSessionDestroyed.AddListener(handleDestructionResult);
            ManagerInstance.DestroySession(sessionName);

            yield return new WaitUntil(() => resultingDeletionResult.HasValue);

            ManagerInstance.UIOnSessionDestroyed.RemoveListener(handleDestructionResult);

            Assert.AreEqual(Result.Success, resultingDeletionResult.Value, $"Failed to destroy Session.");
            Assert.NotNull(resultingDeletionSessionName, $"Deleted Session name is null.");
            Assert.AreEqual(sessionName, resultingDeletionSessionName);

            bool triedToFindSession = ManagerInstance.TryGetSession(sessionName, out Session _);
            Assert.IsFalse(triedToFindSession, $"Found local Session that should be destroyed.");
        }

        /// <summary>
        /// This test creates a Session that should be discoverable online.
        /// It then searches for that Session, and should be able to find it.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreatePublicSessionFindSessionWithPublicSearch()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(CreatePublicSessionFindSessionWithPublicSearch));

            // Mark this Session as public
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.PublicAdvertised;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; });
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(WaitForSearchResultsAfterCreationOrModification);

            ManagerInstance.Search(sessionCreationParameters.Attributes, sessionCreationParameters.BucketId);

            bool waiting = true;
            ManagerInstance.GetCurrentSearch().RunOnSearchResultReceived += (Samples.SessionSearch search) => { waiting = false; };
            yield return new WaitUntilDone(10f, () => !waiting);

            PlayEveryWare.EpicOnlineServices.Samples.SessionSearch search = ManagerInstance.GetCurrentSearch();

            Assert.NotNull(search, $"Current search is null.");

            Dictionary<Session, SessionDetails> searchResults = search.GetResults();
            Assert.AreEqual(1, searchResults.Count, $"Result has a count of results other than exactly one.");

            Session createdLocalSession = null;
            foreach (Session curSession in searchResults.Keys)
            {
                if (curSession.Name == sessionCreationParameters.Name)
                {
                    createdLocalSession = curSession;
                    break;
                }
            }

            Assert.NotNull(createdLocalSession, "Could not find locally created Session with the same name in the Search Results.");
        }

        /// <summary>
        /// This test creates a Session that should be not discoverable online.
        /// It then searches for that Session, and should not be able to find it.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreatePrivateSessionFailToFindFindSessionWithPublicSearch()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(CreatePrivateSessionFailToFindFindSessionWithPublicSearch));

            // Mark this Session as not-public
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; });
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(WaitForSearchResultsAfterCreationOrModification);

            ManagerInstance.Search(sessionCreationParameters.Attributes, sessionCreationParameters.BucketId);

            bool waiting = true;
            ManagerInstance.GetCurrentSearch().RunOnSearchResultReceived += (Samples.SessionSearch search) => { waiting = false; };
            yield return new WaitUntilDone(10f, () => { return !waiting; });

            PlayEveryWare.EpicOnlineServices.Samples.SessionSearch search = ManagerInstance.GetCurrentSearch();

            Assert.NotNull(search, $"Current search is null.");

            Dictionary<Session, SessionDetails> searchResults = search.GetResults();
            Assert.AreEqual(0, searchResults.Count, $"Result has a count of results other than exactly zero.");

            Session createdLocalSession = null;
            foreach (Session curSession in searchResults.Keys)
            {
                if (curSession.Name == sessionCreationParameters.Name)
                {
                    createdLocalSession = curSession;
                    break;
                }
            }

            Assert.Null(createdLocalSession, $"Found created Session in Search Results, should not be able to find this Session using public search.");
        }

        /// <summary>
        /// This test creates a Session that should be not discoverable online using public searches.
        /// That Session should then be able to be located by searching for a Session by its exact Id.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreatePrivateSessionThenFindById()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(CreatePrivateSessionThenFindById));

            // Mark this Session as not-public
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; });
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(WaitForSearchResultsAfterCreationOrModification);

            Samples.SessionSearch usedSessionSearch = null;
            ManagerInstance.SearchById(sessionCreationParameters.Id, (Samples.SessionSearch search) =>
            {
                usedSessionSearch = search;
            });

            yield return new WaitUntilDone(10f, () => { return usedSessionSearch != null; });

            Assert.NotNull(usedSessionSearch, $"Current search is null.");

            Dictionary<Session, SessionDetails> searchResults = usedSessionSearch.GetResults();
            Assert.AreEqual(1, searchResults.Count, $"Result has a count of results other than exactly one.");

            Session createdLocalSession = null;
            foreach (Session curSession in searchResults.Keys)
            {
                if (curSession.Name == sessionCreationParameters.Name)
                {
                    createdLocalSession = curSession;
                    break;
                }
            }

            Assert.NotNull(createdLocalSession, $"Unable to find Session while searching by its Id.");
        }

        private Session GetGenericSaturatedSession(string bucketId)
        {
            string randomSessionName = GetRandomSessionName();

            Session sessionCreationParameters = new Session();
            sessionCreationParameters.AllowJoinInProgress = false;
            sessionCreationParameters.InvitesAllowed = false;
            sessionCreationParameters.SanctionsEnabled = false;
            sessionCreationParameters.MaxPlayers = 4;
            sessionCreationParameters.Name = randomSessionName;
            sessionCreationParameters.BucketId = bucketId;
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            SessionAttribute attribute = new SessionAttribute();
            attribute.Key = "Level";
            attribute.AsString = "UNITYTESTS";
            attribute.ValueType = AttributeType.String;
            attribute.Advertisement = SessionAttributeAdvertisementType.Advertise;

            sessionCreationParameters.Attributes.Add(attribute);

            return sessionCreationParameters;
        }

        /// <summary>
        /// Generates a random Session name that is compliant with the rules.
        /// This should be random in the odd case that a Session is not properly cleaned up between tests.
        /// </summary>
        /// <returns>A random name.</returns>
        private string GetRandomSessionName()
        {
            const int numberOfCharacters = 10;
            StringBuilder randomName = new StringBuilder();

            for (int ii = 0; ii < numberOfCharacters; ii++)
            {
                randomName.Append(UnityEngine.Random.Range(0, 10).ToString());
            }

            return randomName.ToString();
        }

        /// <summary>
        /// Helper function for creating a Session for tests.
        /// </summary>
        /// <param name="toCreate">Parameters of the Session to create.</param>
        /// <param name="onComplete">Action to run with result.</param>
        /// <returns>Yieldable instruction that finishes when the Session is created.</returns>
        private IEnumerator CreateSession(Session toCreate, Action<Result> onComplete)
        {
            string resultingCreationSessionName = null;
            Result? resultingCreationResult = null;

            UnityAction<string, Result> handleCreationResult = (string relevantSession, Result relevantResult) =>
            {
                resultingCreationSessionName = relevantSession;
                resultingCreationResult = relevantResult;
            };

            ManagerInstance.UIOnSessionCreated.AddListener(handleCreationResult);
            ManagerInstance.CreateSession(toCreate);

            yield return new WaitUntil(() => resultingCreationResult.HasValue);

            ManagerInstance.UIOnSessionCreated.RemoveListener(handleCreationResult);

            onComplete.Invoke(resultingCreationResult.Value);
        }
    }
}

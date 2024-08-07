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
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Samples;
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class SessionsServiceTests : EOSTestBase
    {
        protected SessionsService ServiceInstance { get; set; } = null;

        /// <summary>
        /// The Epic Online Services Game Services back end takes some fuzzy amount of time to process changes.
        /// For example, a created game won't appear immediately in search results, even if the creation operation succeeded.
        /// Tests should use this constant amount of wait time before running online queries.
        /// </summary>
        const float SecondsWaitAfterSessionModification = 6f;

        static bool[] TestCasesForCreateSessionAndTogglePublicVisibility = { true, false };

        [OneTimeSetUp]
        public void OneTimeSetUpEOSSessionsManager()
        {
            ServiceInstance = new SessionsService();
            Debug.Log($"{nameof(OneTimeSetUpEOSSessionsManager)}");
        }

        [SetUp]
        public void SetUpLogIn()
        {
            ServiceInstance.OnLoggedIn();
            Debug.Log($"{nameof(SetUpLogIn)}");
        }

        [UnityTearDown]
        public IEnumerator TearDownLogOut()
        {
            // If there are any Sessions that are remaining locally, they all need to be left
            // This may not successfully destroy all Sessions, but should at least attempt
            if (ServiceInstance.GetCurrentSessions().Count > 0)
            {
                ServiceInstance.LeaveAllSessions();

                yield return new WaitUntilDone(60f, () =>
                {
                    return ServiceInstance.GetCurrentSessions().Count == 0;
                });

                yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

                ServiceInstance.OnLoggedOut();
            }
        }

        /// <summary>
        /// Essential test of the most basic functions of the Manager.
        /// Creates a Session, observes that it was created successfully, leaves the Session.
        /// </summary>
        [UnityTest]
        public IEnumerator CreateThenDestroy_Succeeds()
        {
            Session randomTestSession = GetGenericSaturatedSession(nameof(CreateThenDestroy_Succeeds));
            string sessionName = randomTestSession.Name;

            string resultingCreationSessionName = null;
            Result? resultingCreationResult = null;

            ServiceInstance.CreateSession(randomTestSession, info =>
            {
                resultingCreationSessionName = info.SessionName;
                resultingCreationResult = info.ResultCode;
            });

            yield return new WaitUntil(() => resultingCreationResult.HasValue);

            if (resultingCreationResult != null)
            {
                Assert.AreEqual(Result.Success, resultingCreationResult.Value, $"Failed to create Session.");
            }

            Assert.NotNull(resultingCreationSessionName, $"Created Session name is null.");
            Assert.AreEqual(sessionName, resultingCreationSessionName);

            bool canFindSession = ServiceInstance.TryGetSession(sessionName, out Session foundLocalSession);
            Assert.True(canFindSession, $"Could not find Session that was created in local Sessions.");
            Assert.AreEqual(randomTestSession, foundLocalSession, $"Created Session is not identical to resultant created Session.");
            Assert.NotNull(foundLocalSession.ActiveSession, "Created Session does not have ActiveSession information populated after joining.");

            string resultingDeletionSessionName = null;
            Result? resultingDeletionResult = null;

            ServiceInstance.DestroySession(sessionName, info =>
            {
                resultingDeletionSessionName = info.SessionName;
                resultingDeletionResult = info.ResultCode;
            });

            yield return new WaitUntil(() => resultingDeletionResult.HasValue);

            Assert.AreEqual(Result.Success, resultingDeletionResult.Value, $"Failed to destroy Session.");
            Assert.NotNull(resultingDeletionSessionName, $"Deleted Session name is null.");
            Assert.AreEqual(sessionName, resultingDeletionSessionName);

            bool triedToFindSession = ServiceInstance.TryGetSession(sessionName, out Session _);
            Assert.IsFalse(triedToFindSession, $"Found local Session that should be destroyed.");
        }

        /// <summary>
        /// This test creates a Session that should be discoverable online.
        /// It then searches for that Session, and should be able to find it.
        /// </summary>
        [UnityTest]
        public IEnumerator Search_PublicVisibility_CanFind()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(Search_PublicVisibility_CanFind));

            // Mark this Session as public
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.PublicAdvertised;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

            ServiceInstance.Search(sessionCreationParameters.Attributes, sessionCreationParameters.BucketId);

            bool waiting = true;
            ServiceInstance.GetCurrentSearch().RunOnSearchResultReceived += (Samples.SessionSearch search) => { waiting = false; };
            yield return new WaitUntilDone(10f, () => !waiting);

            PlayEveryWare.EpicOnlineServices.Samples.SessionSearch search = ServiceInstance.GetCurrentSearch();

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
            Assert.AreEqual(sessionCreationParameters.Id, createdLocalSession.Id, "Session Id was different than expected.");
        }

        /// <summary>
        /// This test creates a Session that should be not discoverable online.
        /// It then searches for that Session, and should not be able to find it.
        /// </summary>
        [UnityTest]
        public IEnumerator Search_PrivateVisibility_CannotFind()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(Search_PrivateVisibility_CannotFind));

            // Mark this Session as not-public
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

            ServiceInstance.Search(sessionCreationParameters.Attributes, sessionCreationParameters.BucketId);

            bool waiting = true;
            ServiceInstance.GetCurrentSearch().RunOnSearchResultReceived += (Samples.SessionSearch search) => { waiting = false; };
            yield return new WaitUntilDone(10f, () => { return !waiting; });

            PlayEveryWare.EpicOnlineServices.Samples.SessionSearch search = ServiceInstance.GetCurrentSearch();

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
        [UnityTest]
        public IEnumerator SearchById_PrivateVisibility_CanFind()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(SearchById_PrivateVisibility_CanFind));

            // Mark this Session as not-public
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

            Samples.SessionSearch usedSessionSearch = null;
            ServiceInstance.SearchById(sessionCreationParameters.Id, (Samples.SessionSearch search) =>
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

        /// <summary>
        /// This test creates a Session that creates a Session with either public or private visibility,
        /// attempts to find the search publically which should either be discoverable or not based on its current status,
        /// then toggles it back and validates the opposite behaviour.
        /// This is repeated twice.
        /// </summary>
        /// <param name="startPublic">
        /// If true, the visibility starts on <see cref="OnlineSessionPermissionLevel.PublicAdvertised"/>.
        /// If false, the visibility starts on <see cref="OnlineSessionPermissionLevel.InviteOnly"/>.
        /// Then the opposite of this variable is used to determine next run's setting, and so on.
        /// </param>
        [UnityTest]
        public IEnumerator Search_ToggleVisibility_FindAsExpected([ValueSource(nameof(TestCasesForCreateSessionAndTogglePublicVisibility))] bool startPublic)
        {
            const int TimesToRunTest = 4;

            bool shouldCurrentlyBePublic = startPublic;
            for (int createdSessionIndex = 0; createdSessionIndex < TimesToRunTest; createdSessionIndex++)
            {
                Session sessionCreationParameters = GetGenericSaturatedSession(nameof(Search_ToggleVisibility_FindAsExpected));

                // Mark this Session to the appropriate public status
                sessionCreationParameters.PermissionLevel = shouldCurrentlyBePublic ? OnlineSessionPermissionLevel.PublicAdvertised : OnlineSessionPermissionLevel.InviteOnly;

                Result? creationResult = null;
                yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
                Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

                yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

                ServiceInstance.Search(sessionCreationParameters.Attributes, sessionCreationParameters.BucketId);

                bool waiting = true;
                ServiceInstance.GetCurrentSearch().RunOnSearchResultReceived += (Samples.SessionSearch search) => { waiting = false; };
                yield return new WaitUntilDone(10f, () => !waiting);

                PlayEveryWare.EpicOnlineServices.Samples.SessionSearch search = ServiceInstance.GetCurrentSearch();

                Assert.NotNull(search, $"Current search is null.");

                Dictionary<Session, SessionDetails> searchResults = search.GetResults();

                if (shouldCurrentlyBePublic)
                {
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
                    Assert.AreEqual(sessionCreationParameters.Id, createdLocalSession.Id, "Session Id was different than expected.");
                }
                else
                {
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

                ServiceInstance.DestroySession(sessionCreationParameters.Name);
                yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

                shouldCurrentlyBePublic = !shouldCurrentlyBePublic;
            }
        }

        /// <summary>
        /// This test creates a Session, then "destroys" it.
        /// The Manager is checked to ensure that it no longer has local references to the Session.
        /// A search online looks for this Session, and it should be absent.
        /// </summary>
        [UnityTest]
        public IEnumerator Destroy_SessionIsUnavailable()
        {
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(Destroy_SessionIsUnavailable));

            // Mark this Session as not-public; we can find it by ID
            sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

            ServiceInstance.DestroySession(sessionCreationParameters.Name);

            yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

            Assert.AreEqual(0, ServiceInstance.GetCurrentSessions().Count, "There are local Sessions remaining, but there should be none.");

            // Search online using the Id to try and find it
            Samples.SessionSearch usedSessionSearch = null;

            ServiceInstance.SearchById(sessionCreationParameters.Id, (Samples.SessionSearch search) =>
            {
                usedSessionSearch = search;
            });

            yield return new WaitUntilDone(10f, () => { return usedSessionSearch != null; });

            Assert.NotNull(usedSessionSearch, $"Current search is null.");

            Dictionary<Session, SessionDetails> searchResults = usedSessionSearch.GetResults();
            Assert.AreEqual(0, searchResults.Count, $"Result has a count of results other than exactly zero.");
        }

        /// <summary>
        /// This test creates multiple Sessions.
        /// None of them are Presence enabled, so the Client should be able to make several.
        /// </summary>
        [UnityTest]
        public IEnumerator CreateMultiple_NonPresence_Succeeds()
        {
            const int CountOfSessionsToMake = 3;

            for (int createdSessionIndex = 0; createdSessionIndex < CountOfSessionsToMake; createdSessionIndex++)
            {
                Session sessionCreationParameters = GetGenericSaturatedSession(nameof(CreateMultiple_NonPresence_Succeeds));
                sessionCreationParameters.Name = sessionCreationParameters.Name + createdSessionIndex.ToString();

                // Mark this Session as not-public; we can find it by ID
                sessionCreationParameters.PermissionLevel = OnlineSessionPermissionLevel.InviteOnly;

                Result? creationResult = null;
                yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
                Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");
            }

            Assert.AreEqual(CountOfSessionsToMake, ServiceInstance.GetCurrentSessions().Count, $"Expected exactly {CountOfSessionsToMake} Sessions, found a different count");
        }

        /// <summary>
        /// This test attempts to make multiple Sessions, all of them set to be Presence enabled.
        /// The first creation should succeed, but subsequent attempts should fail.
        /// </summary>
        [UnityTest]
        public IEnumerator CreateMultiple_Presence_Fails()
        {
            const int CountOfSessionsToMake = 2;

            for (int createdSessionIndex = 0; createdSessionIndex < CountOfSessionsToMake; createdSessionIndex++)
            {
                Session sessionCreationParameters = GetGenericSaturatedSession(nameof(CreateMultiple_Presence_Fails));
                sessionCreationParameters.Name = sessionCreationParameters.Name + createdSessionIndex.ToString();

                Result? creationResult = null;

                if (createdSessionIndex != 0)
                {
                    LogAssert.Expect(LogType.Error, new Regex(".*Only one session can be presence enabled for the local user.*"));
                    LogAssert.Expect(LogType.Error, new Regex(".*Error code: SessionsPresenceSessionExists.*"));
                }

                yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, true);

                if (createdSessionIndex == 0)
                {
                    Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");
                    bool foundPresenceSession = ServiceInstance.TryGetPresenceSession(out Session presenceSession);
                    Assert.IsTrue(foundPresenceSession, "Expected to find presence Session");
                    Assert.AreEqual(sessionCreationParameters, presenceSession, "Expected found presence session to be the one that was created");
                }
                else
                {
                    Assert.AreEqual(Result.SessionsPresenceSessionExists, creationResult.Value, $"Should fail to create Session with particular error code");
                }
            }
        }

        /// <summary>
        /// This test creates a Presence enabled Session, and then Destroys it.
        /// It then tries to make another Presence enabled Session, which should succeed.
        /// </summary>
        [UnityTest]
        public IEnumerator Presence_CanCreateAfterDestroy()
        {
            const int CountOfSessionsToMake = 2;

            for (int createdSessionIndex = 0; createdSessionIndex < CountOfSessionsToMake; createdSessionIndex++)
            {
                Session sessionCreationParameters = GetGenericSaturatedSession(nameof(Presence_CanCreateAfterDestroy));
                sessionCreationParameters.Name = sessionCreationParameters.Name + createdSessionIndex.ToString();

                Result? creationResult = null;
                yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, true);

                Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

                ServiceInstance.DestroySession(sessionCreationParameters.Name);

                yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

                bool foundPresenceSession = ServiceInstance.TryGetPresenceSession(out Session presenceSession);
                Assert.IsFalse(foundPresenceSession, "Expected to not find any presence Session");
            }
        }

        /// <summary>
        /// This test creates a Session, and then validates that it can be found.
        /// Creates multiple Sessions to run the test by, and validates that the returned value is as expected.
        /// </summary>
        [UnityTest]
        public IEnumerator TryGetSession_Succeeds()
        {
            const int CountOfSessionsToMake = 5;

            for (int createdSessionIndex = 0; createdSessionIndex < CountOfSessionsToMake; createdSessionIndex++)
            {
                Session sessionCreationParameters = GetGenericSaturatedSession(nameof(TryGetSession_Succeeds));
                sessionCreationParameters.Name = sessionCreationParameters.Name + createdSessionIndex.ToString();

                Result? creationResult = null;
                yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);

                Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

                bool canGetSessionByName = ServiceInstance.TryGetSession(sessionCreationParameters.Name, out Session foundSessionByName);
                Assert.IsTrue(canGetSessionByName, "Expected that the Session could be found by name locally");
                Assert.AreEqual(sessionCreationParameters, foundSessionByName, "Retrieved Session was not as expected");

                bool canGetSessionById = ServiceInstance.TryGetSessionById(sessionCreationParameters.Id, out Session foundSessionById);
                Assert.IsTrue(canGetSessionById, "Expected that the Session could be found by id locally");
                Assert.AreEqual(sessionCreationParameters, foundSessionById, "Retrieved Session was not as expected");
            }

            Assert.AreEqual(CountOfSessionsToMake, ServiceInstance.GetCurrentSessions().Count, $"Expected {CountOfSessionsToMake} local Sessions, found a different count");
        }

        /// <summary>
        /// This test creates a Session and modifies its <see cref="OnlineSessionState"/> through management functions,
        /// and validates that the status is as expected.
        /// This test runs <see cref="SessionsService.Update"/> while waiting, which is required to get state changes like this.
        /// </summary>
        [UnityTest]
        public IEnumerator StartEnd_OnlineSessionState_AsExpected()
        {
            const float MostAmountOfTimeToWait = 5f;
            Session sessionCreationParameters = GetGenericSaturatedSession(nameof(StartEnd_OnlineSessionState_AsExpected));

            Result? creationResult = null;
            yield return CreateSession(sessionCreationParameters, (Result res) => { creationResult = res; }, false);
            Assert.AreEqual(Result.Success, creationResult.Value, $"Failed to create Session");

            yield return new WaitForSeconds(SecondsWaitAfterSessionModification);

            ServiceInstance.StartSession(sessionCreationParameters.Name);
            yield return WaitWhileUpdatingManager(new WaitUntilDone(MostAmountOfTimeToWait, () => { return sessionCreationParameters.SessionState == OnlineSessionState.InProgress; }));
            Assert.AreEqual(OnlineSessionState.InProgress, sessionCreationParameters.SessionState, $"Expected current state to be {nameof(OnlineSessionState.InProgress)}.");

            ServiceInstance.EndSession(sessionCreationParameters.Name);
            yield return WaitWhileUpdatingManager(new WaitUntilDone(MostAmountOfTimeToWait, () => { return sessionCreationParameters.SessionState == OnlineSessionState.Ended; }));
            Assert.AreEqual(OnlineSessionState.Ended, sessionCreationParameters.SessionState, $"Expected current state to be {nameof(OnlineSessionState.Ended)}.");

            // Validate that the ended Session can then be started again

            ServiceInstance.StartSession(sessionCreationParameters.Name);
            yield return WaitWhileUpdatingManager(new WaitUntilDone(MostAmountOfTimeToWait, () => { return sessionCreationParameters.SessionState == OnlineSessionState.InProgress; }));
            Assert.AreEqual(OnlineSessionState.InProgress, sessionCreationParameters.SessionState, $"Expected current state to be {nameof(OnlineSessionState.InProgress)}.");
        }

        #region Test Utility Functions

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

            for (int createdSessionIndex = 0; createdSessionIndex < numberOfCharacters; createdSessionIndex++)
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
        /// <param name="presence">Indicates the Session should be created as a Presence Session.</param>
        /// <returns>Yieldable instruction that finishes when the Session is created.</returns>
        private IEnumerator CreateSession(Session toCreate, Action<Result> onComplete, bool presence)
        {
            const float MostTimeToWaitForSessionCreation = 5f;

            string resultingCreationSessionName = null;
            Result? resultingCreationResult = null;

            ServiceInstance.CreateSession(toCreate, info =>
            {
                resultingCreationSessionName = info.SessionName;
                resultingCreationResult = info.ResultCode;
            }, presence);

            yield return new WaitUntilDone(MostTimeToWaitForSessionCreation, () => resultingCreationResult.HasValue);

            onComplete.Invoke(resultingCreationResult.Value);
        }

        private IEnumerator WaitWhileUpdatingManager(WaitUntilDone waitUntilDoneInstruction)
        {
            while (waitUntilDoneInstruction.keepWaiting)
            {
                ServiceInstance.Update();
                yield return new WaitForEndOfFrame();
            }
        }

        #endregion
    }
}

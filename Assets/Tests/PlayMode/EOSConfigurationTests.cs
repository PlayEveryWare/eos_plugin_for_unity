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
    using Epic.OnlineServices.Friends;
    using Epic.OnlineServices.TitleStorage;
    using Epic.OnlineServices.Platform;
    using PlayEveryWare.EpicOnlineServices.Tests;
    using static PlayEveryWare.EpicOnlineServices.EOSManager;

    /// <summary>
    /// Tests for values set in <see cref="EOSConfig"/> and how they interact with the EOS SDK.
    /// This test repeatedly sets up and tears down the EOS Libraries, so it doesn't inherit from the <see cref="EOSTestBase"/>.
    /// </summary>
    public class EOSConfigurationTests
    {
        /// <summary>
        /// How long to wait for <see cref="EOSSingleton.Init"/>
        /// </summary>
        public const float InitTimeoutSeconds = 5f;

        /// <summary>
        /// Handle to the EOSManager.
        /// This will be repeatedly set up and torn down.
        /// </summary>
        protected EOSManager _eosManager;

        [OneTimeSetUp]
        protected void OneTimeSetUp()
        {
            // If there's an existing loaded Epic library, we need to shut it down
            TearDown();
        }

        [SetUp]
        protected void SetUp()
        {
            // Make a GameObject, set it to inactive, add the EOSManager component
            // Set the EOSManager to not initialize on awake, so that it can be set up more intentionally inside the tests
            // Then set it to active, which will now not initialize on awake
            GameObject eosManagerHolder = new GameObject();
            eosManagerHolder.SetActive(false);
            _eosManager = eosManagerHolder.AddComponent<EOSManager>();
            _eosManager.InitializeOnAwake = false;
            eosManagerHolder.SetActive(true);

            // Intentionally not initializing the EOSManager here, that should be done inside the tests
        }

        [TearDown]
        protected void TearDown()
        {
            // Between tests, unload the libraries and unset references
            // EOSManager intelligently unloads things based on the platform,
            // but to circumvent those systems we need to be explicit about the teardown process here
            // We need to be careful to only try unloading things if they're actually already loaded
            if (EOSManager.Instance.GetEOSPlatformInterface() != null)
            {
                EOSManager.Instance?.OnShutdown();
                PlatformInterface.Shutdown();

                // Destroy the monobehaviour with the EOSManager on it, so that a new instance can be made
                // and all member variables are forgotten and reset
                if (_eosManager != null)
                {
                    GameObject.Destroy(_eosManager.gameObject);
                }

                // Unset all of the dll loaded mappings, so that they'll be reloaded fully next test
                EOSSingleton.UnloadAllLibraries();
            }
        }

        /// <summary>
        /// Destroys the EOS object which will shutdown the EOSManager.
        /// </summary>
        [OneTimeTearDown]
        public void ShutdownEOS()
        {
            UnityEngine.Object.Destroy(_eosManager.gameObject);
        }

        #region Task Network Timeout Seconds

        /// <summary>
        /// Testing data structure to be used in data sources for <see cref="EOSConfig.taskNetworkTimeoutSeconds"/> tests.
        /// </summary>
        public struct TaskNetworkTimeoutSecondsParameters
        {
            /// <summary>
            /// When creating this struct through <see cref="SecondsToConfigureWithDefaultToleranceThreshold(double)"/>,
            /// this is the value used to determine <see cref="EarliestValidWaitedSeconds"/> and <see cref="LatestValidWaitedSeconds"/>.
            /// Will not cause <see cref="EarliestValidWaitedSeconds"/> to be below 0.
            /// </summary>
            public const float DefaultToleranceThresholdSeconds = 5f;

            /// <summary>
            /// The default value the EOS SDK uses if <see cref="EOSConfig.taskNetworkTimeoutSeconds"/> is <= 0.
            /// </summary>
            public const double EOSSDKDefaultTaskNetworkTimeoutSeconds = 30;

            /// <summary>
            /// The number of seconds to set as the <see cref="EOSConfig.taskNetworkTimeoutSeconds"/>.
            /// If this value is <= 0, the EOS SDK will use a default value of <see cref="EOSSDKDefaultTaskNetworkTimeoutSeconds"/>.
            /// </summary>
            public double SecondsToConfigure;

            /// <summary>
            /// When waiting for a <see cref="Result.TimedOut"/> response during a test,
            /// this is the amount of time waited must be at least this many seconds for the test to pass.
            /// </summary>
            public float EarliestValidWaitedSeconds;

            /// <summary>
            /// When waiting for a <see cref="Result.TimedOut"/> response during a test,
            /// this is the amount of time that will be waited for until the test fails.
            /// </summary>
            public float LatestValidWaitedSeconds;

            /// <summary>
            /// Creates this data structure where the <see cref="SecondsToConfigure"/> is explicitly set.
            /// The <see cref="EarliestValidWaitedSeconds"/> and <see cref="LatestValidWaitedSeconds"/> are set using the argument,
            /// with the bounds being offset by <see cref="DefaultToleranceThresholdSeconds"/>.
            /// </summary>
            /// <param name="secondsToConfigure">
            /// Seconds to use for the EOSConfig.
            /// <seealso cref="SecondsToConfigure"/>
            /// </param>
            /// <returns>Saturated data structure for test parameters.</returns>
            public static TaskNetworkTimeoutSecondsParameters SecondsToConfigureWithDefaultToleranceThreshold(double secondsToConfigure)
            {
                return new TaskNetworkTimeoutSecondsParameters()
                {
                    SecondsToConfigure = secondsToConfigure,
                    EarliestValidWaitedSeconds = Mathf.Max(0, (float)secondsToConfigure - DefaultToleranceThresholdSeconds),
                    LatestValidWaitedSeconds = (float)secondsToConfigure + DefaultToleranceThresholdSeconds
                };
            }

            /// <summary>
            /// Creates this data structure where <see cref="SecondsToConfigure"/> is set to 0.
            /// The EOS SDK will interpret this and use its default value, which is <see cref="EOSSDKDefaultTaskNetworkTimeoutSeconds"/>.
            /// </summary>
            /// <param name="earliestValidSecondsWaited">
            /// Optional earliest boundary.
            /// <seealso cref="EarliestValidWaitedSeconds"/>
            /// </param>
            /// <param name="latestValidSecondsWaited">
            /// Optional latest boundary.
            /// <seealso cref="LatestValidWaitedSeconds"/>
            /// </param>
            /// <returns>Saturated data structure for test parameters.</returns>
            public static TaskNetworkTimeoutSecondsParameters SecondsToConfigureAsSDKDefault(
                float earliestValidSecondsWaited = (float)EOSSDKDefaultTaskNetworkTimeoutSeconds - DefaultToleranceThresholdSeconds,
                float latestValidSecondsWaited = (float)EOSSDKDefaultTaskNetworkTimeoutSeconds + DefaultToleranceThresholdSeconds)
            {
                return new TaskNetworkTimeoutSecondsParameters()
                {
                    SecondsToConfigure = 0,
                    EarliestValidWaitedSeconds = Mathf.Max(0, earliestValidSecondsWaited),
                    LatestValidWaitedSeconds = latestValidSecondsWaited
                };
            }

            public override string ToString()
            {
                return $"{SecondsToConfigure}s ({EarliestValidWaitedSeconds}-{LatestValidWaitedSeconds}s)";
            }
        }

        public static TaskNetworkTimeoutSecondsParameters[] taskNetworkTimeoutSecondsParameters =
        {
            TaskNetworkTimeoutSecondsParameters.SecondsToConfigureWithDefaultToleranceThreshold(1),
            TaskNetworkTimeoutSecondsParameters.SecondsToConfigureWithDefaultToleranceThreshold(5),
            TaskNetworkTimeoutSecondsParameters.SecondsToConfigureWithDefaultToleranceThreshold(50),
            TaskNetworkTimeoutSecondsParameters.SecondsToConfigureAsSDKDefault()
        };

        /// <summary>
        /// Configures <see cref="EOSConfig.taskNetworkTimeoutSeconds"/> and tests that requests time out within an expected range.
        /// Uses the <see cref="FriendsInterface"/> to test connectivity.
        /// </summary>
        [UnityTest]
        [Category(EOSTestBase.TestCategories.SoloCategory)]
        public IEnumerator TaskNetworkTimeoutSecondsConfigurationTimesOutAsExpected([ValueSource(nameof(taskNetworkTimeoutSecondsParameters))] TaskNetworkTimeoutSecondsParameters parameters)
        {
            EOSConfig configData = EpicOnlineServices.Config.Get<EOSConfig>();
            configData.taskNetworkTimeoutSeconds = parameters.SecondsToConfigure;
            EOSTestBase.CoroutineRunner runner = new GameObject().AddComponent<EOSTestBase.CoroutineRunner>();
            EOSManager.Instance.Init(runner, configData);

            yield return new EOSTestBase.WaitUntilDone(InitTimeoutSeconds, () => !runner.CoroutineRunning);

            EOSSingleton.LoadEOSLibraries();

            yield return EOSTestBase.StaticSetupDevAuthLogin();

            PlatformInterface platformInterface = EOSManager.Instance.GetEOSPlatformInterface();
            FriendsInterface friendsInterface = platformInterface.GetFriendsInterface();            

            // Set the EOS SDK to be offline, which should result in a failed call after the expected amount of time
            // The configured TaskNetworkTimeoutSeconds only functions if NetworkStatus is not Online
            Result setOfflineResult = platformInterface.SetNetworkStatus(NetworkStatus.Offline);
            Assert.AreEqual(Result.Success, setOfflineResult, $"Could not set network status to offline.");

            QueryFriendsOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId()
            };

            QueryFriendsCallbackInfo? result = null;

            // Query friends, which should fail after the expected amount of time for the expected reason
            float timeAtBeginningOfQuery = Time.realtimeSinceStartup;
            friendsInterface.QueryFriends(ref options, null, (ref QueryFriendsCallbackInfo data) => { result = data; });
            yield return new EOSTestBase.WaitUntilDone(parameters.LatestValidWaitedSeconds, () => result != null);

            Assert.AreEqual(NetworkStatus.Offline, platformInterface.GetNetworkStatus(), "Network status should remain offline during this test.");
            Assert.IsTrue(result.HasValue, $"Querying the friends list with network offline should return a result.");
            Assert.AreEqual(Result.TimedOut, result.Value.ResultCode, $"Querying friends list while offline should time out.");

            float timeTaken = Time.realtimeSinceStartup - timeAtBeginningOfQuery;
            Debug.Log($"{nameof(EOSConfigurationTests)} ({nameof(TaskNetworkTimeoutSecondsConfigurationTimesOutAsExpected)}): Timeout took {timeTaken} seconds");
            Assert.GreaterOrEqual(timeTaken, parameters.EarliestValidWaitedSeconds, $"Querying friends list should time out only after configured minimum threshold.");
            Assert.LessOrEqual(timeTaken, parameters.LatestValidWaitedSeconds, $"Querying friends list should time out before configured latest threshold.");
        }

        #endregion
    }
}

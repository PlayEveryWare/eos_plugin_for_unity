/*
 * Copyright (c) 2026 Epic Games Inc
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

namespace PlayEveryWare.EpicOnlineServices.Tests
{
    using System;
    using System.Collections;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Auth;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Initial setup and shutdown procedure for all tests.
    /// </summary>
    public partial class EOSTestBase
    {
        /// <summary>
        /// Common constants used in the tests.
        /// </summary>
        protected class TestCategories
        {
            public const string SoloCategory = "Solo";
            public const string ClientCategory = "Client";
        }

        /// <summary>
        /// Custom yield that has a timeout so it will complete when either the
        /// predicate is true or the timeout is reached.
        /// </summary>
        public sealed class WaitUntilDone : CustomYieldInstruction
        {
            private readonly Func<bool> _predicate;

            private float _timeout;

            private bool WaitForDoneProcess()
            {
                _timeout -= Time.deltaTime;
                return _timeout <= 0f || _predicate();
            }

            public override bool keepWaiting => !WaitForDoneProcess();

            public WaitUntilDone(float timeout, Func<bool> predicate)
            {
                this._predicate = predicate;
                this._timeout = timeout;
            }
        }

        protected const float GlobalTestTimeout = 5f;
        protected const float LoginTestTimeout = 30f;

        protected GameObject eosObject;

        // Static so EOS is initialized only once across all test classes in a session.
        private static bool s_EOSSessionActive;
        private static bool s_LoginAttempted;
        private static bool s_LoginSucceeded;
        private static GameObject s_EOSObject;

        /// <summary>
        /// Initialize the EOSManager once for the entire test session.
        /// Subsequent test classes reuse the same instance.
        /// </summary>
        [OneTimeSetUp]
        public void SetupScene()
        {
            // Reinitialize if this is a fresh session (first run or after play mode was stopped in the editor).
            if (!s_EOSSessionActive || s_EOSObject == null)
            {
                s_EOSObject = new GameObject("EOSManager");
                UnityEngine.Object.DontDestroyOnLoad(s_EOSObject);
                var eosManager = s_EOSObject.AddComponent<EOSManager>();
                EOSManager.Instance.Init(eosManager);
                s_EOSSessionActive = true;
                s_LoginAttempted = false;
                s_LoginSucceeded = false;
            }
            eosObject = s_EOSObject;
        }

        /// <summary>
        /// Logs into Epic once for the entire test session. All subsequent test
        /// classes skip this step and reuse the existing session.
        /// </summary>
        [UnitySetUp]
        public IEnumerator SetupDevAuthLogin()
        {
            if (s_LoginAttempted)
            {
                if (!s_LoginSucceeded)
                {
                    Assert.Fail("Initial login didn't work, so not continuing the rest of the tests.");
                }
                yield break;
            }

            s_LoginAttempted = true;

            UnitTestConfig config = EpicOnlineServices.Config.Get<UnitTestConfig>();

            LoginCallbackInfo? loginResult = null;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer,
                                                                $"{config.EOSDevAuthToolIP}:{config.EOSDevAuthToolPort}",
                                                                config.EOSDevAuthToolUserName,
                                                                data => { loginResult = data; });

            yield return new WaitUntilDone(LoginTestTimeout, () => loginResult != null);

            Assert.IsNotNull(loginResult,
                "Could not log into EOS, loginResult was not set.");

            Assert.AreEqual(Result.Success, loginResult.Value.ResultCode,
                $"Login result failed: {loginResult.Value.ResultCode}");

            Epic.OnlineServices.Connect.LoginCallbackInfo? callbackInfo = null;
            EOSManager.Instance.StartConnectLoginWithEpicAccount(loginResult.Value.LocalUserId, data =>
            {
                callbackInfo = data;
            });

            yield return new WaitUntilDone(LoginTestTimeout, () => callbackInfo != null);

            Assert.IsNotNull(callbackInfo,
                "Could not connect with Epic account, callbackInfo was not set.");

            Assert.AreEqual(Result.Success, callbackInfo.Value.ResultCode,
                $"Could not connect with Epic account: {callbackInfo.Value.ResultCode}");

            Assert.That(EOSManager.Instance.GetProductUserId().IsValid(),
                "Current player is invalid.");

            s_LoginSucceeded = true;
        }

        /// <summary>
        /// EOS SDK cannot be reinitialized once shut down within the same process.
        /// The static session state is intentionally kept alive for subsequent test
        /// classes. Resources are released when the test process exits or when the
        /// next play mode session detects a stale s_EOSObject.
        /// </summary>
        [OneTimeTearDown]
        public void ShutdownEOS() { }
    }
}

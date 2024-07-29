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
    public class EOSTestBase
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
        /// Custom yield that has a timeout so it will complete when either the predicate is true or
        /// the timeout is reached.
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

        private bool _initialized;
        private bool _successfulLogin;

        /// <summary>
        /// Initialize the EOSManager once before attempting to log in.
        /// </summary>
        [OneTimeSetUp]
        public void SetupScene()
        {
            eosObject = new GameObject();
            var eosManager = eosObject.AddComponent<EOSManager>();
            EOSManager.Instance.Init(eosManager);
        }

        /// <summary>
        /// Initial setup for logging into Epic before starting the tests as all test rely on connecting online. If this setup step
        /// fails, it will immediately cancel the rest of the tests as there's no reason to continue running without a connection online.
        /// </summary>
        [UnitySetUp]
        public IEnumerator SetupDevAuthLogin()
        {
            // HACK: Only initialize the login once instead of constantly logging in for each test scenario, which can cause
            // unintentional errors.
            // Currently, Unity unit testing doesn't have a Unity version of OneTimeSetUp that allows coroutines,
            // so this is a hacky way to make this work.
            if (_initialized)
            {
                // If there was no successful login, the rest of the tests will fail, so fail immediately here to make it
                // clear that there's a basic login problem that's causing everything else to fail.
                if (!_successfulLogin)
                {
                    Assert.Fail("Initial login didn't work, so not continuing the rest of the tests.");
                }

                yield break;
            }

            _initialized = true;

            UnitTestConfig config = EpicOnlineServices.Config.Get<UnitTestConfig>();

            // Using DevAuth for local testing.
            // Need to make this use Password on a build machine to make it fully automated if possible.
            LoginCallbackInfo? loginResult = null;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer,
                                                                $"{config.EOSDevAuthToolIP}:{config.EOSDevAuthToolPort}",
                                                                config.EOSDevAuthToolUserName,
                                                                data => { loginResult = data; });

            yield return new WaitUntilDone(LoginTestTimeout, () => loginResult != null);

            Assert.IsNotNull(loginResult, "Could not log into EOS, loginResult was not set.");
            Assert.AreEqual(Result.Success, loginResult.Value.ResultCode, $"Login result failed: {loginResult.Value.ResultCode}");

            Epic.OnlineServices.Connect.LoginCallbackInfo? callbackInfo = null;
            EOSManager.Instance.StartConnectLoginWithEpicAccount(loginResult.Value.LocalUserId, data =>
            {
                callbackInfo = data;
            });

            yield return new WaitUntilDone(LoginTestTimeout, () => callbackInfo != null);

            Assert.IsNotNull(callbackInfo, "Could not connect with Epic account, callbackInfo was not set.");
            Assert.AreEqual(Result.Success, callbackInfo.Value.ResultCode, $"Could not connect with Epic account: {callbackInfo.Value.ResultCode}");
            Assert.That(EOSManager.Instance.GetProductUserId().IsValid(), "Current player is invalid.");

            _successfulLogin = true;
        }

        /// <summary>
        /// Destroys the EOS object which will shutdown the EOSManager.
        /// </summary>
        [OneTimeTearDown]
        public void ShutdownEOS()
        {
            EOSManager.Instance?.OnShutdown();
            UnityEngine.Object.Destroy(eosObject);
        }
    }
}

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

namespace PlayEveryWare.EpicOnlineServices.Tests.Connect
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Achievements;
    using Epic.OnlineServices.Stats;
    using NUnit.Framework;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    public class ConnectTests
    {
        GameObject eosObject;
        protected const float LoginTestTimeout = 30f;

        [TearDown]
        public void ShutdownEOS()
        {
            EOSManager.Instance?.OnShutdown();
            UnityEngine.Object.Destroy(eosObject);
        }

        /// <summary>
        /// This test creates a new EOSManager object, and uses it to auth-login.
        /// Then it does a Connect login.
        /// After a successful Connect login, it tries to perform another Connect login.
        /// The result should be as expected.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ConnectLogin_WhileAlreadyLoggedIn_ReturnsExpectedResult()
        {
            eosObject = new GameObject();
            var eosManager = eosObject.AddComponent<EOSManager>();

            UnitTestConfig config = EpicOnlineServices.Config.Get<UnitTestConfig>();

            // Auth login
            // This isn't what this test is testing, but we must first auth login before connect login
            Epic.OnlineServices.Auth.LoginCallbackInfo? loginResult = null;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(Epic.OnlineServices.Auth.LoginCredentialType.Developer,
                                                                $"{config.EOSDevAuthToolIP}:{config.EOSDevAuthToolPort}",
                                                                config.EOSDevAuthToolUserName,
                                                                data => { loginResult = data; });

            yield return new EOSTestBase.WaitUntilDone(LoginTestTimeout, () => loginResult != null);

            Assert.IsNotNull(loginResult,
                "Could not log into EOS, loginResult was not set.");

            Assert.AreEqual(Result.Success, loginResult.Value.ResultCode,
                $"Login result failed: {loginResult.Value.ResultCode}");

            // Now that this is logged in, attempt connect login
            Epic.OnlineServices.Connect.LoginCallbackInfo? callbackInfo = null;
            EOSManager.Instance.StartConnectLoginWithEpicAccount(loginResult.Value.LocalUserId, data =>
            {
                callbackInfo = data;
            });

            yield return new EOSTestBase.WaitUntilDone(LoginTestTimeout, () => callbackInfo != null);

            Assert.IsNotNull(callbackInfo,
                "Could not connect with Epic account, callbackInfo was not set.");

            Assert.AreEqual(Result.Success, callbackInfo.Value.ResultCode,
                $"Could not connect with Epic account: {callbackInfo.Value.ResultCode}");

            Assert.That(EOSManager.Instance.GetProductUserId().IsValid(),
                "Current player is invalid.");

            // Subsequent connect login, check for expected results
            callbackInfo = null;
            EOSManager.Instance.StartConnectLoginWithEpicAccount(loginResult.Value.LocalUserId, data =>
            {
                callbackInfo = data;
            });

            yield return new EOSTestBase.WaitUntilDone(LoginTestTimeout, () => callbackInfo != null);

            Assert.IsNotNull(callbackInfo,
                "Could not connect with Epic account, callbackInfo was not set.");

            Assert.AreEqual(Result.Success, callbackInfo.Value.ResultCode,
                $"Could not connect with Epic account: {callbackInfo.Value.ResultCode}");

            Assert.That(EOSManager.Instance.GetProductUserId().IsValid(),
                "Current player is invalid.");
        }
    }
}
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

namespace PlayEveryWare.EpicOnlineServices.Tests.Auth
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Achievements;
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Stats;
    using NUnit.Framework;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    public class AuthenticationTests
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
        /// Then after a successful login, it tries to auth login again.
        /// The result should be as expected.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AuthLogin_WhileAlreadyLoggedIn_ReturnsExpectedResult()
        {
            eosObject = new GameObject();
            var eosManager = eosObject.AddComponent<EOSManager>();

            UnitTestConfig config = EpicOnlineServices.Config.Get<UnitTestConfig>();

            // Initial Auth login
            LoginCallbackInfo? loginResult = null;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer,
                                                                $"{config.EOSDevAuthToolIP}:{config.EOSDevAuthToolPort}",
                                                                config.EOSDevAuthToolUserName,
                                                                data => { loginResult = data; });

            yield return new EOSTestBase.WaitUntilDone(LoginTestTimeout, () => loginResult != null);

            Assert.IsNotNull(loginResult,
                "Could not log into EOS, loginResult was not set.");

            Assert.AreEqual(Result.Success, loginResult.Value.ResultCode,
                $"Login result failed: {loginResult.Value.ResultCode}");

            // Subsequent Auth login
            loginResult = null;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer,
                                                                $"{config.EOSDevAuthToolIP}:{config.EOSDevAuthToolPort}",
                                                                config.EOSDevAuthToolUserName,
                                                                data => { loginResult = data; });

            yield return new EOSTestBase.WaitUntilDone(LoginTestTimeout, () => loginResult != null);

            Assert.IsNotNull(loginResult,
                "Could not log into EOS, loginResult was not set.");

            Assert.AreEqual(Result.Success, loginResult.Value.ResultCode,
                $"Login result failed: {loginResult.Value.ResultCode}");
        }
    }
}
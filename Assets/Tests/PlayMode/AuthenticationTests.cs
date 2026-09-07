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

namespace PlayEveryWare.EpicOnlineServices.Tests.Auth
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Auth;
    using NUnit.Framework;
    using System.Collections;
    using UnityEngine.TestTools;

    /// <summary>
    /// Tests authentication behavior when EOS is already initialized.
    /// EOS lifecycle is managed by EOSTestBase.
    /// </summary>
    public partial class AuthenticationTests : EOSTestBase
    {
        /// <summary>
        /// Attempts a second auth login while already logged in and verifies
        /// that the result is successful (idempotent login behavior).
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator AuthLogin_WhileAlreadyLoggedIn_ReturnsExpectedResult()
        {
            UnitTestConfig config = EpicOnlineServices.Config.Get<UnitTestConfig>();

            // SetupDevAuthLogin already did the first login; perform a second one.
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
        }
    }
}
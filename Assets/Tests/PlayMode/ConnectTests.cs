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

namespace PlayEveryWare.EpicOnlineServices.Tests.Connect
{
    using Epic.OnlineServices;
    using NUnit.Framework;
    using System.Collections;
    using UnityEngine.TestTools;

    /// <summary>
    /// Tests Connect login behavior when EOS is already initialized.
    /// EOS lifecycle is managed by EOSTestBase.
    /// </summary>
    public partial class ConnectTests : EOSTestBase
    {
        /// <summary>
        /// Performs auth + connect login while already logged in to verify that
        /// the result is successful (idempotent login behavior).
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator ConnectLogin_WhileAlreadyLoggedIn_ReturnsExpectedResult()
        {
            UnitTestConfig config = EpicOnlineServices.Config.Get<UnitTestConfig>();

            // SetupDevAuthLogin already did auth+connect; redo auth to get LocalUserId for a second connect login.
            Epic.OnlineServices.Auth.LoginCallbackInfo? loginResult = null;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(
                Epic.OnlineServices.Auth.LoginCredentialType.Developer,
                $"{config.EOSDevAuthToolIP}:{config.EOSDevAuthToolPort}",
                config.EOSDevAuthToolUserName,
                data => { loginResult = data; });

            yield return new WaitUntilDone(LoginTestTimeout, () => loginResult != null);

            Assert.IsNotNull(loginResult,
                "Could not log into EOS, loginResult was not set.");

            Assert.AreEqual(Result.Success, loginResult.Value.ResultCode,
                $"Login result failed: {loginResult.Value.ResultCode}");

            // Attempt a second Connect login while already connected.
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
        }
    }
}
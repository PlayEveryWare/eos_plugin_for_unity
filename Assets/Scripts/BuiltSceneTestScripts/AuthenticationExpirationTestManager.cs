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
    using Epic.OnlineServices;
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Connect;
    using EpicOnlineServices;
    using Samples;
    using Samples.Steam;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// This is a test of the authentication managers.
    /// Specifically it is to audit the managers that do not handle the reauthorization of their tickets.
    /// At time of writing, this will continuously log in and log out until the authentication fails.
    /// This is assumed to happen, because the tickets are not reauthorized.
    /// When this test has encountered that issue, we can 100% say this is an actual problem.
    /// Then, we can fix it, and use this same test to see if it ever causes a problem with re-authorized code.
    /// After this fix, this test may be unnecessary, but perhaps someone will find value in it.
    /// </summary>
    public class AuthenticationExpirationTestManager : MonoBehaviour
    {
        public enum AuthenticationProviderToTest
        {
            Unset = 0,
            SteamSession = 1,
            SteamApp = 2,
            Discord = 3
        }

        AuthenticationProviderToTest RunningTest { get; set; } = AuthenticationProviderToTest.Unset;

        public UIDebugLog Log;
        public Text NextAction;
        public Text OriginalToken;
        public Text CurrentToken;

        private Coroutine RunningTestCoroutine { get; set; }


        /// <summary>
        /// <see cref="SetAuthenticationProviderAndLogin(AuthenticationProviderToTest)"/>
        /// </summary>
        /// <param name="toSet">
        /// Integer to cast into a <see cref="AuthenticationProviderToTest"/> argument.
        /// Needed for the dropdown to function.
        /// </param>
        public void SetAuthenticationProviderAndLogin(int toSet)
        {
            SetAuthenticationProviderAndLogin((AuthenticationProviderToTest)toSet);
        }

        /// <summary>
        /// Begins a test of the Authentication token reacquisition.
        /// This test validates that the various providers correctly have their
        /// refresh tokens and authentication handled, such that users of the plugin
        /// will have the login refresh itself correctly when it expires.
        /// It performs this test by:
        /// - Logging in using the provider supplied
        /// - Performs a Connect login
        /// - Periodically performs an action in EOS
        ///   If the action fails, the authentication must have failed to refresh.
        ///   Therefore the test will end then, as a failure.
        /// - Listens for the authentication provider's token to change.
        ///   Displays changed tokens, which are generally a proof of this system working.
        /// The test never finishes 'successfully', so it'll have to declare a success if it doesn't
        /// fail within some arbitrary time.
        /// </summary>
        /// <param name="toSet">The provider to test against.</param>
        public void SetAuthenticationProviderAndLogin(AuthenticationProviderToTest toSet)
        {
            if (RunningTestCoroutine != null)
            {
                Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(SetAuthenticationProviderAndLogin)}): Test is already running, cannot run multiple tests in the same instance.");
                return;
            }

            RunningTest = toSet;
            StartAuthenticationExpirationProbe();
        }

        /// <summary>
        /// Begins a test of the Connect system, ensuring that the Connect login
        /// correctly refreshes itself when it notifies about expiring.
        /// This will make sure the users of the plugin stayed logged in and
        /// able to use EOS.
        /// It performs this test by;
        /// - Logging in using the AccountPortal (you'll have to respond to this UI)
        /// - Performs a Connect login
        /// - Subscribes to the Connect Expiration and Login Status Change notifications
        /// - If the login status ever becomes logged out, the test is ended as a failure
        /// The test never finishes 'successfully', so it'll have to declare a success if it doesn't
        /// fail within some arbitrary time.
        /// </summary>
        public void StartConnectTest()
        {
            if (RunningTestCoroutine != null)
            {
                Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(StartConnectTest)}): Test is already running, cannot run multiple tests in the same instance.");
                return;
            }

            RunningTestCoroutine = StartCoroutine(ContinuouslyProbeConnect());
        }

        private void StartAuthenticationExpirationProbe()
        {
            // Placeholder implementations
            RetrieveTokenFunction retrieveTokenFunction = (Action<string> resultingString) =>
            {
                resultingString?.Invoke(string.Empty);
            };
            AttemptAuthenticationFunction attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) =>
            {
                res?.Invoke(Result.Success, null);
            };

            switch (RunningTest)
            {
                case AuthenticationProviderToTest.SteamSession:
                    retrieveTokenFunction = (Action<string> resultingString) =>
                    {
                        resultingString?.Invoke(SteamManager.Instance.GetSessionTicket());
                    };
                    attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) =>
                    {
                        PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance
                            .StartConnectLoginWithSteamSessionTicket(
                                (Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                                {
                                    res?.Invoke(info.ResultCode, info.ContinuanceToken);
                                });
                    };
                    break;
                case AuthenticationProviderToTest.SteamApp:
                    retrieveTokenFunction = SteamManager.Instance.RequestAppTicket;
                    attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) =>
                    {
                        PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance
                            .StartConnectLoginWithSteamAppTicket((Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                            {
                                res?.Invoke(info.ResultCode, info.ContinuanceToken);
                            });
                    };
                    break;
                case AuthenticationProviderToTest.Discord:
                    retrieveTokenFunction = PlayEveryWare.EpicOnlineServices.Samples.Discord.DiscordManager.Instance
                        .RequestOAuth2Token;
                    attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) =>
                    {
                        PlayEveryWare.EpicOnlineServices.Samples.Discord.DiscordManager.Instance.StartConnectLogin(
                            (Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                            {
                                res?.Invoke(info.ResultCode, info.ContinuanceToken);
                            });
                    };
                    break;
            }

            RunningTestCoroutine = StartCoroutine(ContinuouslyProbeTicket(retrieveTokenFunction, attemptAuthenticationFunction));
        }

        private void EndProbeAndLogout(Action afterLogout)
        {
            if (RunningTestCoroutine != null)
            {
                StopCoroutine(RunningTestCoroutine);
                RunningTestCoroutine = null;
            }

            if (EOSManager.Instance.HasLoggedInWithConnect())
            {
                EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(),
                    (ref Epic.OnlineServices.Auth.LogoutCallbackInfo data) =>
                    {
                        afterLogout?.Invoke();
                    });
            }
            else
            {
                afterLogout?.Invoke();
            }

        }

        private IEnumerator ContinuouslyProbeTicket(RetrieveTokenFunction retrieveTokenFunction,
            AttemptAuthenticationFunction attemptAuthenticationFunction)
        {
            // Attempt to login after each hour
            const float secondsBetweenTests = 60f * 60f;
            const float secondsWaitTimeBeforeLogout = 2f;
            const float secondsWaitTimeAfterFailedLoginAttemptBetweenAttempts = 5f;

            // If while logging in there is a Result.UnexpectedError, try logging in up to this many times
            const int unexpectedErrorRetryCount = 3;

            float startTime = Time.realtimeSinceStartup;

            NextAction.text = $"Getting initial token";
            string tokenAtStart = string.Empty;
            bool waiting = true;
            retrieveTokenFunction((string result) =>
            {
                tokenAtStart = result;
                waiting = false;
            });

            while (waiting)
            {
                yield return new WaitForEndOfFrame();
            }

            OriginalToken.text = tokenAtStart;

            if (string.IsNullOrEmpty(tokenAtStart))
            {
                Debug.LogError(
                    $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Initial token is null.");
                yield break;
            }

            Debug.Log(
                $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Ticket at start is {tokenAtStart}.");

            while (true)
            {
                // First log in
                Result resultCode = Result.NotConfigured;
                bool errorHit = false;

                // For my own aesthetic sake this iterator starts at 1
                // If unexpectedErrorRetryCount is 0, there will still be one attempt
                for (int attempt = 1; attempt <= unexpectedErrorRetryCount + 1; attempt++)
                {
                    waiting = true;
                    ContinuanceToken continuanceTokenReceived = default(ContinuanceToken);

                    NextAction.text = $"Attempt #{attempt} at authenticating";

                    attemptAuthenticationFunction((Result res, ContinuanceToken continuanceToken) =>
                    {
                        waiting = false;
                        resultCode = res;
                        continuanceTokenReceived = continuanceToken;
                    });

                    while (waiting)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    if (resultCode == Result.Success)
                    {
                        Debug.Log(
                            $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Login success.");
                        errorHit = false;
                        break;
                    }
                    else if (resultCode == Result.ConnectExternalTokenValidationFailed)
                    {
                        Debug.LogError(
                            $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Login failed with {nameof(Result.ConnectExternalTokenValidationFailed)}, which is the expected connect auth failure. Ending probe.");
                        errorHit = true;
                        break;
                    }
                    else if (resultCode == Result.InvalidUser)
                    {
                        // Invalid user means you need to create an account to connect to it, so let's do that quickly
                        NextAction.text = $"EOS account does not exist, creating connect user with continuance token";

                        waiting = true;
                        EOSManager.Instance.CreateConnectUserWithContinuanceToken(continuanceTokenReceived,
                            (Epic.OnlineServices.Connect.CreateUserCallbackInfo createInfo) =>
                            {
                                resultCode = createInfo.ResultCode;
                                waiting = false;
                            });

                        while (waiting)
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        if (resultCode != Result.Success)
                        {
                            Debug.LogError(
                                $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Failed to create connect user. Result code {resultCode}. Ending probe.");
                            errorHit = true;
                            break;
                        }

                        // If we succeeded, dial it back one and try to relogin
                        attempt--;
                    }
                    else if (resultCode == Result.UnexpectedError)
                    {
                        Debug.LogError(
                            $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Unexpected error on attempt {attempt}. Considering retry.");
                        errorHit = true;
                        NextAction.text =
                            $"Waiting before trying to log in again after failed attempt: {(DateTime.Now.AddSeconds(secondsWaitTimeAfterFailedLoginAttemptBetweenAttempts)).ToLocalTime()}";
                        yield return new WaitForSeconds(secondsWaitTimeAfterFailedLoginAttemptBetweenAttempts);
                        continue;
                    }
                    else
                    {
                        Debug.LogError(
                            $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Unhandled error on attempt {attempt}. Result code {resultCode}. Ending probe.");
                        errorHit = true;
                        break;
                    }
                }

                if (resultCode != Result.Success)
                {
                    Debug.LogError(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Could not successfully log in. Giving up.");
                    errorHit = true;
                    break;
                }

                if (errorHit)
                {
                    break;
                }

                NextAction.text = $"Retrieving token";
                waiting = true;
                string currentToken = string.Empty;

                // Since the login succeeded, let's check our token
                // If it's different (and we succeeded logging in) we must have reauthorized properly
                retrieveTokenFunction((string result) =>
                {
                    currentToken = result;
                    waiting = false;
                });

                while (waiting)
                {
                    yield return new WaitForEndOfFrame();
                }

                if (string.IsNullOrEmpty(currentToken))
                {
                    Debug.LogError(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): New authentication token is null.");
                    break;
                }

                if (!string.Equals(tokenAtStart, currentToken))
                {
                    Debug.LogError(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): The current token ({currentToken}) is different than the starting token ({tokenAtStart}). It must have been reacquired successfully.");
                    break;
                }

                CurrentToken.text = currentToken;
                NextAction.text =
                    $"Waiting until before logging out: {(DateTime.Now.AddSeconds(secondsWaitTimeBeforeLogout)).ToLocalTime()}";
                yield return new WaitForSeconds(secondsWaitTimeBeforeLogout);

                // Logout before the next cycle, then wait a moment
                NextAction.text = $"Logging out";
                waiting = true;

                EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(),
                    (ref Epic.OnlineServices.Auth.LogoutCallbackInfo data) =>
                    {
                        waiting = false;
                    });

                while (waiting)
                {
                    yield return new WaitForEndOfFrame();
                }

                Debug.Log(
                    $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Waiting for {secondsBetweenTests} seconds before next authentication recycle.");
                NextAction.text =
                    $"Waiting until before next cycle: {(DateTime.Now.AddSeconds(secondsBetweenTests)).ToLocalTime()}";
                yield return new WaitForSeconds(secondsBetweenTests);

                Debug.Log(
                    $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Reauthentication cycle completed successfully.");
            }

            float endTime = Time.realtimeSinceStartup;
            float totalTime = endTime - startTime;
            Debug.Log(
                $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): This test took {totalTime} seconds.");
        }

        private IEnumerator ContinuouslyProbeConnect()
        {
            NextAction.text = $"Logging in";

            bool waiting = true;
            Result resultCode = Result.NoChange;
            ContinuanceToken continuanceToken = default(ContinuanceToken);

            EOSManager.Instance.StartLoginWithLoginTypeAndToken(Epic.OnlineServices.Auth.LoginCredentialType.AccountPortal, null, null, (Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
            {
                waiting = false;
                resultCode = loginCallbackInfo.ResultCode;
                continuanceToken = loginCallbackInfo.ContinuanceToken;
            });

            while (waiting)
            {
                yield return new WaitForFixedUpdate();
            }

            if (resultCode == Result.InvalidUser)
            {
                // If the user is invalid, it's because it needs to have an Epic Account made
                // In this scenario, call the account creation
                // Invalid user means you need to create an account to connect to it, so let's do that quickly
                NextAction.text = $"EOS account does not exist, creating connect user with continuance token";

                waiting = true;
                EOSManager.Instance.CreateConnectUserWithContinuanceToken(continuanceToken,
                    (Epic.OnlineServices.Connect.CreateUserCallbackInfo createInfo) =>
                    {
                        resultCode = createInfo.ResultCode;
                        waiting = false;
                    });

                while (waiting)
                {
                    yield return new WaitForEndOfFrame();
                }

                if (resultCode != Result.Success)
                {
                    Debug.LogError(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeConnect)}): Failed to create connect user. Result code {resultCode}. Ending probe.");
                    yield break;
                }
            }
            else if (resultCode != Result.Success)
            {
                Debug.LogError(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeConnect)}): Failed to authenticate. Result code {resultCode}. Ending probe.");
                yield break;
            }

            waiting = true;
            // At this point it is assumed the user is logged in correctly
            EOSManager.Instance.StartConnectLoginWithEpicAccount(EOSManager.Instance.GetLocalUserId(), (Epic.OnlineServices.Connect.LoginCallbackInfo callbackInfo) =>
            {
                waiting = false;
                resultCode = callbackInfo.ResultCode;
            });

            while (waiting)
            {
                yield return new WaitForEndOfFrame();
            }

            if (resultCode != Result.Success)
            {
                Debug.LogError(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeConnect)}): Failed to connect login. Result code {resultCode}. Ending probe.");
                yield break;
            }


            Epic.OnlineServices.LoginStatus? newStatusChange = null;
            Epic.OnlineServices.Connect.AddNotifyLoginStatusChangedOptions options = new Epic.OnlineServices.Connect.AddNotifyLoginStatusChangedOptions();

            Epic.OnlineServices.Connect.OnLoginStatusChangedCallback loginChangedCallback = (ref Epic.OnlineServices.Connect.LoginStatusChangedCallbackInfo data) =>
            {
                newStatusChange = data.CurrentStatus;
            };
            ulong loginStatusChangedCallbackId = EOSManager.Instance.GetEOSConnectInterface().AddNotifyLoginStatusChanged(ref options, null, loginChangedCallback);

            Epic.OnlineServices.Connect.AddNotifyAuthExpirationOptions expOptions = new Epic.OnlineServices.Connect.AddNotifyAuthExpirationOptions();

            OnAuthExpirationCallback authExpirationCallback = (ref Epic.OnlineServices.Connect.AuthExpirationCallbackInfo callbackInfo) =>
            {
                Debug.Log(
                        $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeConnect)}): Authentication expiration notification has proc'd. This is not an error state, as the re-connect method might run on this same notification.");
            };
            ulong authExpirationCallbackId = EOSManager.Instance.GetEOSConnectInterface().AddNotifyAuthExpiration(ref expOptions, null, authExpirationCallback);

            while (true)
            {
                if (newStatusChange.HasValue)
                {
                    if (newStatusChange == LoginStatus.NotLoggedIn)
                    {
                        Debug.LogError(
                            $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeConnect)}): No longer considered logged in. Re-connect must have failed. Ending probe.");
                        break;
                    }
                    else if (newStatusChange == LoginStatus.LoggedIn)
                    {
                        Debug.Log(
                            $"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeConnect)}): Received notification about being Logged In. This may indicate that a connect login succeeded.");
                    }
                }

                yield return new WaitForFixedUpdate();
            }

            EOSManager.Instance.GetEOSConnectInterface().RemoveNotifyLoginStatusChanged(loginStatusChangedCallbackId);
            EOSManager.Instance.GetEOSConnectInterface().RemoveNotifyAuthExpiration(authExpirationCallbackId);
        }

        delegate void RetrieveTokenFunction(Action<string> resultingString);

        delegate void AttemptAuthenticationFunction(Action<Result, ContinuanceToken> callbackWhenDone);

        private void OnDisable()
        {
            EndProbeAndLogout(() => { });
        }
    }
}

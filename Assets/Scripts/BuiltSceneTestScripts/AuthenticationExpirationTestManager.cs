using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using PlayEveryWare.EpicOnlineServices.Samples.Steam;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private Coroutine Probe { get; set; }

    public void SetAuthenticationProviderAndLogin(int toSet)
    {
        SetAuthenticationProviderAndLogin((AuthenticationProviderToTest)toSet);
    }

    public void SetAuthenticationProviderAndLogin(AuthenticationProviderToTest toSet)
    {
        RunningTest = toSet;
        StartProbe();
    }

    private void StartProbe()
    {
        // Placeholder implementations
        RetrieveTokenFunction retrieveTokenFunction = (Action<string> resultingString) => { resultingString?.Invoke(string.Empty); };
        AttemptAuthenticationFunction attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) => { res?.Invoke(Result.Success, null); };

        switch (RunningTest)
        {
            case AuthenticationProviderToTest.SteamSession:
                retrieveTokenFunction = (Action<string> resultingString) => { resultingString?.Invoke(SteamManager.Instance.GetSessionTicket()); };
                attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) =>
                {
                    PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance.StartConnectLoginWithSteamSessionTicket((Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                    {
                        res?.Invoke(info.ResultCode, info.ContinuanceToken);
                    });
                };
                break;
            case AuthenticationProviderToTest.SteamApp:
                retrieveTokenFunction = SteamManager.Instance.RequestAppTicket;
                attemptAuthenticationFunction = (Action<Result, ContinuanceToken> res) =>
                {
                    PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance.StartConnectLoginWithSteamAppTicket((Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                    {
                        res?.Invoke(info.ResultCode, info.ContinuanceToken);
                    });
                };
                break;
            case AuthenticationProviderToTest.Discord:
                retrieveTokenFunction = PlayEveryWare.EpicOnlineServices.Samples.Discord.DiscordManager.Instance.RequestOAuth2Token;
                attemptAuthenticationFunction = (Action<Result,ContinuanceToken> res) =>
                {
                    PlayEveryWare.EpicOnlineServices.Samples.Discord.DiscordManager.Instance.StartConnectLogin((Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                    {
                        res?.Invoke(info.ResultCode, info.ContinuanceToken);
                    });
                };
                break;
        }

        Probe = StartCoroutine(ContinuouslyProbeTicket(retrieveTokenFunction, attemptAuthenticationFunction));
    }

    private void EndProbeAndLogout(Action afterLogout)
    {
        if (Probe != null)
        {
            StopCoroutine(Probe);
            Probe = null;
        }

        if (EOSManager.Instance.HasLoggedInWithConnect())
        {
            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (ref Epic.OnlineServices.Auth.LogoutCallbackInfo data) => 
            {
                afterLogout?.Invoke();
            });
        }
        else
        {
            afterLogout?.Invoke();
        }

    }

    private IEnumerator ContinuouslyProbeTicket(RetrieveTokenFunction retrieveTokenFunction, AttemptAuthenticationFunction attemptAuthenticationFunction)
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
        retrieveTokenFunction((string result) => { tokenAtStart = result; waiting = false; });

        while (waiting)
        {
            yield return new WaitForEndOfFrame();
        }

        OriginalToken.text = tokenAtStart;

        if (string.IsNullOrEmpty(tokenAtStart))
        {
            Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Initial token is null.");
            yield break;
        }

        Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Ticket at start is {tokenAtStart}.");

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
                    Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Login success.");
                    errorHit = false;
                    break;
                }
                else if (resultCode == Result.ConnectExternalTokenValidationFailed)
                {
                    Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Login failed with {nameof(Result.ConnectExternalTokenValidationFailed)}, which is the expected connect auth failure. Ending probe.");
                    errorHit = true;
                    break;
                }
                else if (resultCode == Result.InvalidUser)
                {
                    // Invalid user means you need to create an account to connect to it, so let's do that quickly
                    NextAction.text = $"EOS account does not exist, creating connect user with continuance token";

                    waiting = true;
                    EOSManager.Instance.CreateConnectUserWithContinuanceToken(continuanceTokenReceived, (Epic.OnlineServices.Connect.CreateUserCallbackInfo createInfo) =>
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
                        Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Failed to create connect user. Result code {resultCode}. Ending probe.");
                        errorHit = true;
                        break;
                    }

                    // If we succeeded, dial it back one and try to relogin
                    attempt--;
                }
                else if (resultCode == Result.UnexpectedError)
                {
                    Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Unexpected error on attempt {attempt}. Considering retry.");
                    errorHit = true;
                    NextAction.text = $"Waiting before trying to log in again after failed attempt: {(DateTime.Now.AddSeconds(secondsWaitTimeAfterFailedLoginAttemptBetweenAttempts)).ToLocalTime()}";
                    yield return new WaitForSeconds(secondsWaitTimeAfterFailedLoginAttemptBetweenAttempts);
                    continue;
                }
                else
                {
                    Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Unhandled error on attempt {attempt}. Result code {resultCode}. Ending probe.");
                    errorHit = true;
                    break;
                }
            }

            if (resultCode != Result.Success)
            {
                Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Could not successfully log in. Giving up.");
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
                Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): New authentication token is null.");
                break;
            }

            if (!string.Equals(tokenAtStart, currentToken))
            {
                Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): The current token ({currentToken}) is different than the starting token ({tokenAtStart}). It must have been reacquired successfully.");
                break;
            }

            CurrentToken.text = currentToken;
            NextAction.text = $"Waiting until before logging out: {(DateTime.Now.AddSeconds(secondsWaitTimeBeforeLogout)).ToLocalTime()}";
            yield return new WaitForSeconds(secondsWaitTimeBeforeLogout);

            // Logout before the next cycle, then wait a moment
            NextAction.text = $"Logging out";
            waiting = true;

            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (ref Epic.OnlineServices.Auth.LogoutCallbackInfo data) =>
            {
                waiting = false;
            });

            while (waiting)
            {
                yield return new WaitForEndOfFrame();
            }

            Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Waiting for {secondsBetweenTests} seconds before next authentication recycle.");
            NextAction.text = $"Waiting until before next cycle: {(DateTime.Now.AddSeconds(secondsBetweenTests)).ToLocalTime()}";
            yield return new WaitForSeconds(secondsBetweenTests);

            Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Reauthentication cycle completed successfully.");
        }

        float endTime = Time.realtimeSinceStartup;
        float totalTime = endTime - startTime;
        Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): This test took {totalTime} seconds.");
    }

    delegate void RetrieveTokenFunction(Action<string> resultingString);
    delegate void AttemptAuthenticationFunction(Action<Result, ContinuanceToken> callbackWhenDone);

    private void OnDisable()
    {
        EndProbeAndLogout(() => { });
    }
}

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

    private Coroutine Probe { get; set; }

    public void SetAuthenticationProviderAndLogin(int toSet)
    {
        SetAuthenticationProviderAndLogin((AuthenticationProviderToTest)toSet);
    }

    public void SetAuthenticationProviderAndLogin(AuthenticationProviderToTest toSet)
    {
        RunningTest = toSet;

        switch (toSet)
        {
            case AuthenticationProviderToTest.SteamSession:
                TestSteamSession();
                break;
            case AuthenticationProviderToTest.SteamApp:
                TestSteamApp();
                break;
            case AuthenticationProviderToTest.Discord:
                TestDiscord();
                break;
        }
    }

    void TestSteamSession()
    {
        EndProbeAndLogout(() =>
        {
            PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance.StartConnectLoginWithSteamSessionTicket(ConnectLoginTokenCallback);
        });
    }

    void TestSteamApp()
    {
        EndProbeAndLogout(() =>
        {
            PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance.StartConnectLoginWithSteamAppTicket(ConnectLoginTokenCallback);
        });
    }

    void TestDiscord()
    {
        EndProbeAndLogout(() =>
        {
            PlayEveryWare.EpicOnlineServices.Samples.Discord.DiscordManager.Instance.RequestOAuth2Token(OnDiscordAuthReceived);
        });
    }

    private void ConnectLoginTokenCallback(Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo)
    {
        if (connectLoginCallbackInfo.ResultCode == Result.Success)
        {
            Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ConnectLoginTokenCallback)}): Successful login.");
            StartProbe();
        }
        else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser)
        {
            Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ConnectLoginTokenCallback)}): {nameof(Result.InvalidUser)} returned. Creating connect account and retrying.");
            EOSManager.Instance.CreateConnectUserWithContinuanceToken(connectLoginCallbackInfo.ContinuanceToken, (Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo) =>
            {
                print("Creating new connect user");
                if (createUserCallbackInfo.ResultCode == Result.Success)
                {
                    Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ConnectLoginTokenCallback)}): Successfully made new connect account.");
                    StartProbe();
                }
                else
                {
                    Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ConnectLoginTokenCallback)}): Failed to make new connect account. Result code {createUserCallbackInfo.ResultCode}.");
                }
            });
        }
        else
        {
            Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ConnectLoginTokenCallback)}): Failed to login. Result code {connectLoginCallbackInfo.ResultCode}.");
        }
    }

    private void OnDiscordAuthReceived(string token)
    {
        if (token == null)
        {
            Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ConnectLoginTokenCallback)}): Failed to acquire Discord OAuth2 token.");
        }
        else
        {
            EOSManager.Instance.StartConnectLoginWithOptions(ExternalCredentialType.DiscordAccessToken, token, onloginCallback: ConnectLoginTokenCallback);
        }
    }

    private void StartProbe()
    {
        // Placeholder implementations
        RetrieveTokenFunction retrieveTokenFunction = (Action<string> resultingString) => { resultingString?.Invoke(string.Empty); };
        AttemptAuthenticationFunction attemptAuthenticationFunction = (Action<Result> res) => { res?.Invoke(Result.Success); };

        switch (RunningTest)
        {
            case AuthenticationProviderToTest.SteamSession:
                retrieveTokenFunction = (Action<string> resultingString) => { resultingString?.Invoke(SteamManager.Instance.GetSessionTicket()); };
                attemptAuthenticationFunction = (Action<Result> res) =>
                {
                    PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance.StartConnectLoginWithSteamSessionTicket((Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                    {
                        res?.Invoke(info.ResultCode);
                    });
                };
                break;
            case AuthenticationProviderToTest.SteamApp:
                retrieveTokenFunction = SteamManager.Instance.RequestAppTicket;
                attemptAuthenticationFunction = (Action<Result> res) =>
                {
                    PlayEveryWare.EpicOnlineServices.Samples.Steam.SteamManager.Instance.StartConnectLoginWithSteamAppTicket((Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                    {
                        res?.Invoke(info.ResultCode);
                    });
                };
                break;
            case AuthenticationProviderToTest.Discord:
                retrieveTokenFunction = PlayEveryWare.EpicOnlineServices.Samples.Discord.DiscordManager.Instance.RequestOAuth2Token;
                attemptAuthenticationFunction = (Action<Result> res) =>
                {
                    retrieveTokenFunction((string resultingString) => 
                    {
                        EOSManager.Instance.StartConnectLoginWithOptions(ExternalCredentialType.DiscordAccessToken, resultingString, onloginCallback: (Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                        {
                            res?.Invoke(info.ResultCode);
                        });
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
    }

    private IEnumerator ContinuouslyProbeTicket(RetrieveTokenFunction retrieveTokenFunction, AttemptAuthenticationFunction attemptAuthenticationFunction)
    {
        const float secondsBetweenTests = 20f;
        float startTime = Time.realtimeSinceStartup;

        string tokenAtStart = string.Empty;
        retrieveTokenFunction((string result) => { tokenAtStart = result; });

        while (string.IsNullOrEmpty(tokenAtStart))
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Ticket at start is {tokenAtStart}.");

        while (true)
        {
            yield return new WaitForSeconds(secondsBetweenTests);

            // First, log out. We're assuming we won't re-fetch a new ticket.
            bool waiting = true;

            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (ref Epic.OnlineServices.Auth.LogoutCallbackInfo data) =>
            {
                waiting = false;
            });

            while (waiting)
            {
                yield return new WaitForEndOfFrame();
            }

            waiting = true;
            
            // Now log back in, presumably using the same ticket
            Result resultCode = Result.NotConfigured;

            attemptAuthenticationFunction((Result res) =>
            {
                waiting = false;
                resultCode = res;
            });

            while (waiting)
            {
                yield return new WaitForEndOfFrame();
            }

            // If the login failed, end the test
            if (resultCode != Result.Success)
            {
                Debug.LogError($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): Result of login was {resultCode}.");
                break;
            }

            string currentToken = string.Empty;

            // Since the login succeeded, let's check our token
            // If it's different (and we succeeded logging in) we must have reauthorized properly
            retrieveTokenFunction((string result) => { currentToken = result; });

            while (string.IsNullOrEmpty(currentToken))
            {
                yield return new WaitForEndOfFrame();
            }

            if (!string.Equals(tokenAtStart, currentToken))
            {
                Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): The current token ({currentToken}) is different than the starting token ({tokenAtStart}). It must have been reacquired successfully.");
                break;
            }
        }

        float endTime = Time.realtimeSinceStartup;
        float totalTime = endTime - startTime;
        Debug.Log($"{nameof(AuthenticationExpirationTestManager)} ({nameof(ContinuouslyProbeTicket)}): This test took {totalTime}.");
    }

    delegate void RetrieveTokenFunction(Action<string> resultingString);
    delegate void AttemptAuthenticationFunction(Action<Result> callbackWhenDone);
}

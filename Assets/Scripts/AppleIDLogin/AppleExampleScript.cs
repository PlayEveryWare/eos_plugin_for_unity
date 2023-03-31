using UnityEngine;
using System.Text;

using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;

public class AppleExampleScript : MonoBehaviour
{
    IAppleAuthManager m_AppleAuthManager;
    public string Token { get; private set; }
    public string User { get; private set; }
    public string Error { get; private set; }

    public bool IsAppleAuthModuleInstalled()
    {
#if APPLEAUTH_MODULE
        return true;
#endif
        return false;
    }

    public void Initialize()
    {
        Debug.Log("AppleAuthInit");
        var deserializer = new PayloadDeserializer();
        m_AppleAuthManager = new AppleAuthManager(deserializer);
    }

    public void Update()
    {
        if (m_AppleAuthManager != null)
        {
            m_AppleAuthManager.Update();
        }
    }

    public void LoginToApple(System.Action callback)
    {
        // Initialize the Apple Auth Manager
        if (m_AppleAuthManager == null)
        {
            Initialize();
        }

        // Set the login arguments
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        // Perform the login
        m_AppleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIDCredential = credential as IAppleIDCredential;
                if (appleIDCredential != null)
                {
                    var idToken = Encoding.UTF8.GetString(
                        appleIDCredential.IdentityToken,
                        0,
                        appleIDCredential.IdentityToken.Length);
                    Debug.Log("Sign-in with Apple successfully done. IDToken: " + idToken);
                    Token = idToken;
                    User = appleIDCredential.User;

                    callback?.Invoke();
                }
                else
                {
                    Debug.Log("Sign-in with Apple error. Message: appleIDCredential is null");
                    Error = "Retrieving Apple Id Token failed.";
                }
            },
            error =>
            {
                Debug.Log("Sign-in with Apple error. Message: " + error);
                Error = "Retrieving Apple Id Token failed.";
            }
        );
    }
}
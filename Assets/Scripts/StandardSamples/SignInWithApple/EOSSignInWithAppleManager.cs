using UnityEngine;
using System.Text;

#if APPLEAUTH_MODULE
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif

namespace PlayEveryWare.EpicOnlineServices.Samples.Apple
{
    public class EOSSignInWithAppleManager
    {
#if APPLEAUTH_MODULE
        IAppleAuthManager m_AppleAuthManager;
#endif
        public void Initialize()
        {
#if APPLEAUTH_MODULE
            Debug.Log("AppleAuthInit");
            var deserializer = new PayloadDeserializer();
            m_AppleAuthManager = new AppleAuthManager(deserializer);
#endif
        }

        public void Update()
        {
#if APPLEAUTH_MODULE
            if (m_AppleAuthManager != null)
            {
                m_AppleAuthManager.Update();
            }
#endif
        }

        public void RequestTokenAndUsername(System.Action<string, string> callback)
        {

#if APPLEAUTH_MODULE

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

                        callback?.Invoke(idToken, appleIDCredential.User);
                    }
                    else
                    {
                        Debug.Log("Sign-in with Apple error. Message: appleIDCredential is null");
                    }
                },
                error =>
                {
                    Debug.Log("Sign-in with Apple error. Message: " + error);
                }
            );
#else
            Debug.LogWarning("Missing Sign-in with Apple pacakge : [com.lupidan.apple-signin-unity]");
            callback?.Invoke(null, "package_not_found");
#endif
        }
    }
}

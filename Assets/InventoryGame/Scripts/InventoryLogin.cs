using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.InventoryGame
{
    public class InventoryLogin : MonoBehaviour
    {
        public Dropdown loginTypeDropdown;

        public Text idText;
        public ConsoleInputField idInputField;

        public Text tokenText;
        public ConsoleInputField tokenInputField;

        public Text loginButtonText;
        private string _OriginalloginButtonText;
        public Button loginButton;
        private Coroutine PreventLogIn = null;
        public Button logoutButton;

        public UnityEvent OnLogin;
        public UnityEvent OnLogout;

        LoginCredentialType loginType = LoginCredentialType.Developer;

        // Retain Id/Token inputs across scenes
        public static string IdGlobalCache = string.Empty;
        public static string TokenGlobalCache = string.Empty;

        private void Awake()
        {
            idInputField.InputField.onEndEdit.AddListener(CacheIdInputField);
            tokenInputField.InputField.onEndEdit.AddListener(CacheTokenField);
#if UNITY_EDITOR
            loginType = LoginCredentialType.AccountPortal; // Default in editor
#else
            loginType = LoginCredentialType.AccountPortal; // Default on other platforms
#endif

#if UNITY_EDITOR || (UNITY_STANDALONE_OSX && EOS_PREVIEW_PLATFORM) || UNITY_STANDALONE_WIN || (UNITY_STANDALONE_LINUX && EOS_PREVIEW_PLATFORM)
            idInputField.InputField.text = "localhost:7777"; //default on pc
#endif
        }

        public void Start()
        {
            _OriginalloginButtonText = loginButtonText.text;
            ConfigureUIForLogin();
        }

        private void CacheIdInputField(string value)
        {
            IdGlobalCache = value;
        }

        private void CacheTokenField(string value)
        {
            TokenGlobalCache = value;
        }

        public void OnDropdownChange(int value)
        {
            switch (value)
            {
                case 1:
                    loginType = LoginCredentialType.AccountPortal;
                    break;
                case 2:
                    loginType = LoginCredentialType.PersistentAuth;
                    break;
                case 3:
                    loginType = LoginCredentialType.ExternalAuth;
                    break;
                case 0:
                default:
                    loginType = LoginCredentialType.Developer;
                    break;
            }

            ConfigureUIForLogin();
        }

        private void EnterPressedToLogin()
        {
            if (loginButton.IsActive())
            {
                OnLoginButtonClick();
            }
        }

        private void ConfigureUIForDevAuthLogin()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "Dev Auth");

            if (!string.IsNullOrEmpty(IdGlobalCache))
            {
                idInputField.InputField.text = IdGlobalCache;
            }

            if (!string.IsNullOrEmpty(TokenGlobalCache))
            {
                tokenInputField.InputField.text = TokenGlobalCache;
            }
        }

        private void ConfigureUIForAccountPortalLogin()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "Account Portal");

            // AC/TODO: Reduce duplicated UI code for the different login types
            loginTypeDropdown.gameObject.SetActive(true);

            loginButtonText.text = _OriginalloginButtonText;
            if (PreventLogIn != null)
                StopCoroutine(PreventLogIn);
            loginButton.enabled = true;
            loginButton.gameObject.SetActive(true);
            logoutButton?.gameObject.SetActive(false);
        }

        private void ConfigureUIForPersistentLogin()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "PersistentAuth");
        }

        //-------------------------------------------------------------------------
        private void ConfigureUIForExternalAuth()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "ExternalAuth");
        }

        private void SetNavigationForItems(Selectable loginTypeDown = null)
        {
            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = logoutButton,
                selectOnDown = loginTypeDown != null ? loginTypeDown : loginButton
            };

            loginButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = loginTypeDropdown,
                selectOnDown = logoutButton,
                selectOnLeft = logoutButton
            };
        }

        private void SetupTextFieldsAndNavigation()
        {
            if (loginType == LoginCredentialType.Developer)
            {
                idInputField.gameObject.SetActive(true);
                tokenInputField.gameObject.SetActive(true);
                idText.gameObject.SetActive(true);
                tokenText.gameObject.SetActive(true);

                SetNavigationForItems(idInputField.InputFieldButton);
            }
            else
            {
                idInputField.gameObject.SetActive(false);
                tokenInputField.gameObject.SetActive(false);
                idText.gameObject.SetActive(false);
                tokenText.gameObject.SetActive(false);

                SetNavigationForItems();
            }
        }

        private void ConfigureUIForLogin()
        {
            if (OnLogout != null)
            {
                OnLogout.Invoke();
            }

            loginTypeDropdown.gameObject.SetActive(true);

            loginButtonText.text = _OriginalloginButtonText;
            if (PreventLogIn != null)
                StopCoroutine(PreventLogIn);
            loginButton.enabled = true;
            loginButton.gameObject.SetActive(true);
            logoutButton?.gameObject.SetActive(false);

            switch (loginType)
            {
                case LoginCredentialType.AccountPortal:
                    ConfigureUIForAccountPortalLogin();
                    break;
                case LoginCredentialType.PersistentAuth:
                    ConfigureUIForPersistentLogin();
                    break;
                case LoginCredentialType.ExternalAuth:
                    ConfigureUIForExternalAuth();
                    break;
                case LoginCredentialType.Developer:
                default:
                    ConfigureUIForDevAuthLogin();
                    break;
            }

            SetupTextFieldsAndNavigation();
        }

        private void ConfigureUIForLogout()
        {
            loginTypeDropdown.gameObject.SetActive(false);

            loginButton.gameObject.SetActive(false);
            logoutButton?.gameObject.SetActive(true);

            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);
            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);

            if (OnLogin != null)
            {
                OnLogin.Invoke();
            }
        }

        public void OnLogoutButtonClick()
        {
            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (ref LogoutCallbackInfo data) => {
                if (data.ResultCode == Result.Success)
                {
                    print("Logout Successful. [" + data.ResultCode + "]");
                    ConfigureUIForLogin();
                }
            });
        }

        // For now, the only supported login type that requires a 'username' is the dev auth one
        private bool SelectedLoginTypeRequiresUsername()
        {
            return loginType == LoginCredentialType.Developer;
        }

        // For now, the only supported login type that requires a 'password' is the dev auth one
        private bool SelectedLoginTypeRequiresPassword()
        {
            return loginType == LoginCredentialType.Developer;
        }

        private IEnumerator TurnButtonOnAfter15Sec()
        {
            for (int i = 15; i >= 0; i--)
            {
                yield return new WaitForSecondsRealtime(1);
                loginButtonText.text = _OriginalloginButtonText + " (" + i + ")";
            }
            loginButton.enabled = true;
            loginButtonText.text = _OriginalloginButtonText;
        }

        // Username and password aren't always the username and password
        public void OnLoginButtonClick()
        {
            string usernameAsString = idInputField.InputField.text.Trim();
            string passwordAsString = tokenInputField.InputField.text.Trim();

            if (SelectedLoginTypeRequiresUsername() && usernameAsString.Length <= 0)
            {
                print("Username is missing.");
                return;
            }

            if (SelectedLoginTypeRequiresPassword() && passwordAsString.Length <= 0)
            {
                print("Password is missing.");
                return;
            }

            loginButton.enabled = false;
            if (PreventLogIn != null)
                StopCoroutine(PreventLogIn);
            PreventLogIn = StartCoroutine(TurnButtonOnAfter15Sec());
            //usernameInputField.enabled = false;
            //passwordInputField.enabled = false;
            print("Attempting to login...");

            // Disabled at the moment to work around a crash that happens
            //LoggingInterface.SetCallback((LogMessage logMessage) =>{
            //    print(logMessage.Message);
            //});

            if (loginType == LoginCredentialType.ExternalAuth)
            {
                Debug.LogError("ExternalAuth is not implemented on this platform");
            }
            else if (loginType == LoginCredentialType.PersistentAuth)
            {
                EOSManager.Instance.StartPersistentLogin((Epic.OnlineServices.Auth.LoginCallbackInfo callbackInfo) =>
                {
                    // In this state, it means one needs to login in again with the previous login type, or a new one, as the
                    // tokens are invalid
                    if (callbackInfo.ResultCode != Epic.OnlineServices.Result.Success)
                    {
                        print("Failed to login with Persistent token [" + callbackInfo.ResultCode + "]");
                        loginType = LoginCredentialType.Developer;
                        ConfigureUIForLogin();
                    }
                    else
                    {
                        StartLoginWithLoginTypeAndTokenCallback(callbackInfo);
                    }
                });
            }
            else
            {
                // Deal with other EOS log in issues
                EOSManager.Instance.StartLoginWithLoginTypeAndToken(loginType,
                                                                    usernameAsString,
                                                                    passwordAsString,
                                                                    StartLoginWithLoginTypeAndTokenCallback);
            }
        }

        //-------------------------------------------------------------------------
        private void StartConnectLoginWithLoginCallbackInfo(LoginCallbackInfo loginCallbackInfo)
        {
            EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
            {
                if (connectLoginCallbackInfo.ResultCode == Result.Success)
                {
                    print("Connect Login Successful. [" + loginCallbackInfo.ResultCode + "]");
                    ConfigureUIForLogout();
                }
                else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser)
                {
                    // ask user if they want to connect; sample assumes they do
                    EOSManager.Instance.CreateConnectUserWithContinuanceToken(connectLoginCallbackInfo.ContinuanceToken, (Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo) =>
                    {
                        print("Creating new connect user");
                        EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo retryConnectLoginCallbackInfo) =>
                        {
                            if (retryConnectLoginCallbackInfo.ResultCode == Result.Success)
                            {
                                ConfigureUIForLogout();
                            }
                            else
                            {
                                // For any other error, re-enable the login procedure
                                ConfigureUIForLogin();
                            }
                        });
                    });
                }
            });
        }

        //-------------------------------------------------------------------------
        public void StartLoginWithLoginTypeAndTokenCallback(LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.AuthMFARequired)
            {
                // collect MFA
                // do something to give the MFA to the SDK
                print("MFA Authentication not supported in sample. [" + loginCallbackInfo.ResultCode + "]");
            }
            else if (loginCallbackInfo.ResultCode == Result.AuthPinGrantCode)
            {
                ///TODO(mendsley): Handle pin-grant in a more reasonable way
                Debug.LogError("------------PIN GRANT------------");
                Debug.LogError("External account is not connected to an Epic Account. Use link below");
                Debug.LogError($"URL: {loginCallbackInfo.PinGrantInfo?.VerificationURI}");
                Debug.LogError($"CODE: {loginCallbackInfo.PinGrantInfo?.UserCode}");
                Debug.LogError("---------------------------------");
            }
            else if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
            {
                StartConnectLoginWithLoginCallbackInfo(loginCallbackInfo);
            }
            else if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.InvalidUser)
            {
                print("Trying Auth link with external account: " + loginCallbackInfo.ContinuanceToken);
                EOSManager.Instance.AuthLinkExternalAccountWithContinuanceToken(loginCallbackInfo.ContinuanceToken,
                                                                                LinkAccountFlags.NoFlags,
                                                                                (Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
                                                                                {
                                                                                    StartConnectLoginWithLoginCallbackInfo(loginCallbackInfo);
                                                                                });
            }

            else
            {
                print("Error logging in. [" + loginCallbackInfo.ResultCode + "]");
            }

            // Re-enable the login button and associated UI on any error
            if (loginCallbackInfo.ResultCode != Epic.OnlineServices.Result.Success)
            {
                ConfigureUIForLogin();
            }
        }
    }
}

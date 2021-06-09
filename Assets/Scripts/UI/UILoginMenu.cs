using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.Ecom;

using Epic.OnlineServices.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UILoginMenu : MonoBehaviour
    {
        [Header("Authentication UI")]
        public Text DemoTitle;
        public Dropdown SceneSwitcherDropDown;
        public Dropdown loginTypeDropdown;

        public Text idText;
        public InputField idInputField;

        public Text tokenText;
        public InputField tokenInputField;

        public Button loginButton;
        public Button logoutButton;

        public UnityEvent OnLogin;
        public UnityEvent OnLogout;

        private EventSystem system;

        LoginCredentialType loginType = LoginCredentialType.Developer;

        public void OnDemoSceneChange(int value)
        {
            value = value - 1;
            Debug.LogFormat("UILoginMenu (OnDemoSceneChanged): value = {0}", value);
            SceneManager.LoadScene(value);
        }

        public void OnDropdownChange(int value)
        {
            switch (value)
            {
                case 0:
                    loginType = LoginCredentialType.Developer;
                    ConfigureUIForDevAuthLogin();
                    break;
                case 1:
                    loginType = LoginCredentialType.AccountPortal;
                    ConfigureUIForAccountPortalLogin();
                    break;
                case 2:
                    loginType = LoginCredentialType.PersistentAuth;
                    ConfigureUIForPersistentLogin();
                    break;
                default:
                    loginType = LoginCredentialType.Developer;
                    ConfigureUIForDevAuthLogin();
                    break;
            }
        }

        public void Start()
        {
            ConfigureUIForLogin();

            system = EventSystem.current;

            tokenInputField.onEndEdit.AddListener(EnterPressedToLogin);
        }

        private void EnterPressedToLogin(string arg0)
        {
            OnLoginButtonClick();
        }

        public void Update()
        {
            // Tab between input fields
            if (Input.GetKeyDown(KeyCode.Tab)
                && system.currentSelectedGameObject != null)
            {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                if (next != null)
                {
                    InputField inputfield = next.GetComponent<InputField>();
                    if (inputfield != null)
                    {
                        inputfield.OnPointerClick(new PointerEventData(system));
                    }

                    system.SetSelectedGameObject(next.gameObject);
                }
                else
                {
                    next = FindTopUISelectable();
                    system.SetSelectedGameObject(next.gameObject);
                }
            }
        }

        private Selectable FindTopUISelectable()
        {
            Selectable currentTop = Selectable.allSelectablesArray[0];
            double currentTopXaxis = currentTop.transform.position.x;

            foreach (Selectable s in Selectable.allSelectablesArray)
            {
                if (s.transform.position.x > currentTopXaxis)
                {
                    currentTop = s;
                    currentTopXaxis = s.transform.position.x;
                }
            }

            return currentTop;
        }

        private void ConfigureUIForDevAuthLogin()
        {
            idInputField.gameObject.SetActive(true);
            tokenInputField.gameObject.SetActive(true);
            idText.gameObject.SetActive(true);
            tokenText.gameObject.SetActive(true);
        }

        private void ConfigureUIForAccountPortalLogin()
        {
            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);
        }

        private void ConfigureUIForPersistentLogin()
        {
            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);
        }

        private void ConfigureUIForLogin()
        {
            if (OnLogout != null)
            {
                OnLogout.Invoke();
            }

            SceneSwitcherDropDown.gameObject.SetActive(true);
            DemoTitle.gameObject.SetActive(true);
            loginTypeDropdown.gameObject.SetActive(true);

            loginButton.enabled = true;
            loginButton.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);

            switch (loginType)
            {
                case LoginCredentialType.AccountPortal:
                    ConfigureUIForAccountPortalLogin();
                    break;
                case LoginCredentialType.PersistentAuth:
                    ConfigureUIForPersistentLogin();
                    break;
                case LoginCredentialType.Developer:
                default:
                    ConfigureUIForDevAuthLogin();
                    break;
            }
        }

        private void ConfigureUIForLogout()
        {
            SceneSwitcherDropDown.gameObject.SetActive(false);
            DemoTitle.gameObject.SetActive(false);
            loginTypeDropdown.gameObject.SetActive(false);

            loginButton.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(true);

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
            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (LogoutCallbackInfo data) => {
                if (data.ResultCode == Result.Success)
                {
                    print("Logout Successful. [" + data.ResultCode + "]");
                    ConfigureUIForLogin();
                }

            });
        }

        // For now, the only supported login type that requires a 'username' is the dev auth one
        bool SelectedLoginTypeRequiresUsername()
        {
            return loginType == LoginCredentialType.Developer;
        }

        // For now, the only supported login type that requires a 'password' is the dev auth one
        bool SelectedLoginTypeRequiresPassword()
        {
            return loginType == LoginCredentialType.Developer;
        }

        // Username and password aren't always the username and password
        public void OnLoginButtonClick()
        {
            string usernameAsString = idInputField.text.Trim();
            string passwordAsString = tokenInputField.text.Trim();

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
            //usernameInputField.enabled = false;
            //passwordInputField.enabled = false;
            print("Attempting to login...");

            // Disabled at the moment to work around a crash that happens
            //LoggingInterface.SetCallback((LogMessage logMessage) =>{
            //    print(logMessage.Message);
            //});

            if (loginType == LoginCredentialType.PersistentAuth)
            {
                EOSManager.Instance.StartPersistantLogin((Epic.OnlineServices.Auth.LoginCallbackInfo callbackInfo) =>
                {
                    // In this state, it means one needs to login in again with the previous login type, or a new one, as the
                    // tokens are invalid
                    if (callbackInfo.ResultCode != Epic.OnlineServices.Result.Success)
                    {
                        print("Failed to login with Persistent token [" + callbackInfo.ResultCode + "]");
                        ConfigureUIForDevAuthLogin();
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
        public void StartLoginWithLoginTypeAndTokenCallback(LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.AuthMFARequired)
            {
                // collect MFA
                // do something to give the MFA to the SDK
                print("MFA Authentication not supported. [" + loginCallbackInfo.ResultCode + "]");
            }
            else if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
            {
                EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
                {
                    print("Login Successful. [" + loginCallbackInfo.ResultCode + "]");
                    ConfigureUIForLogout();
                });
            }
            else
            {
                print("Error logging in. [" + loginCallbackInfo.ResultCode + "]");
                ConfigureUIForLogin();
            }
        }

        public void OnExitButtonClick()
        {
            Application.Quit();
        }
    }
}
/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Logging;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        public ConsoleInputField idInputField;

        public Text tokenText;
        public ConsoleInputField tokenInputField;

        public Button loginButton;
        public Button logoutButton;

        public UnityEvent OnLogin;
        public UnityEvent OnLogout;

        [Header("Controller")]
        public GameObject UIFirstSelected;
        public GameObject UIFindSelectable;

        private EventSystem system;
        private GameObject selectedGameObject;

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

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            idInputField.InputField.text = "localhost:7777"; //default on pc
#endif
        }

        private void CacheIdInputField(string value)
        {
            IdGlobalCache = value;
        }

        private void CacheTokenField(string value)
        {
            TokenGlobalCache = value;
        }

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

        public void Start()
        {
            ConfigureUIForLogin();

            system = EventSystem.current;
        }

        private void EnterPressedToLogin()
        {
            if (loginButton.IsActive())
            {
                OnLoginButtonClick();
            }
        }

#if ENABLE_INPUT_SYSTEM
        public void Update()
        {
            var keyboard = Keyboard.current;

            // Disable game input if Overlay is visible and has exclusive input
            if (EventSystem.current != null && EventSystem.current.sendNavigationEvents != !EOSManager.Instance.IsOverlayOpenWithExclusiveInput())
            {
                if (EOSManager.Instance.IsOverlayOpenWithExclusiveInput())
                {
                    Debug.LogWarning("UILoginMenu (Update): Game Input (sendNavigationEvents) Disabled.");
                    EventSystem.current.sendNavigationEvents = false;
                    EventSystem.current.currentInputModule.enabled = false;
                    return;
                }
                else
                {
                    Debug.Log("UILoginMenu (Update): Game Input (sendNavigationEvents) Enabled.");
                    EventSystem.current.sendNavigationEvents = true;
                    EventSystem.current.currentInputModule.enabled = true;
                }
            }

            if (keyboard != null
                && system.currentSelectedGameObject != null)
            {
                // Tab between input fields
                if (keyboard.tabKey.wasPressedThisFrame)
                {
                    Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                    if (keyboard.shiftKey.isPressed)
                    {
                        next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                    }

                    // Make sure the object is active or exit out if no more objects are found
                    while (next != null && !next.gameObject.activeSelf)
                    {
                        next = next.FindSelectableOnDown();
                    }

                    if (next != null)
                    {
                        InputField inputField = next.GetComponent<InputField>();
                        ConsoleInputField consoleInputField = next.GetComponent<ConsoleInputField>();
                        if (inputField != null)
                        {
                            inputField.OnPointerClick(new PointerEventData(system));
                            system.SetSelectedGameObject(next.gameObject);
                        }
                        else if (consoleInputField != null)
                        {
                            consoleInputField.InputField.OnPointerClick(new PointerEventData(system));
                            system.SetSelectedGameObject(consoleInputField.InputField.gameObject);
                        }
                        else
                        {
                            system.SetSelectedGameObject(next.gameObject);
                        }
                    }
                    else
                    {
                        next = FindTopUISelectable();
                        system.SetSelectedGameObject(next.gameObject);
                    }
                }
                else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                {
                    // Enter pressed in an input field
                    InputField inputField = system.currentSelectedGameObject.GetComponent<InputField>();
                    ConsoleInputField consoleInputField = system.currentSelectedGameObject.GetComponent<ConsoleInputField>();
                    if (inputField != null || consoleInputField != null)
                    {
                        EnterPressedToLogin();
                    }
                }
            }

            // Controller: Detect if nothing is selected and controller input detected, and set default
            bool nothingSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null;
            bool inactiveButtonSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && !EventSystem.current.currentSelectedGameObject.activeInHierarchy;

            var gamepad = Gamepad.current;
            if ((nothingSelected || inactiveButtonSelected)
                && gamepad != null && gamepad.wasUpdatedThisFrame)
            {
                if (UIFirstSelected.activeSelf == true)
                {
                    EventSystem.current.SetSelectedGameObject(UIFirstSelected);
                }
                else if (UIFindSelectable.activeSelf == true)
                {
                    EventSystem.current.SetSelectedGameObject(UIFindSelectable);
                }

                Debug.Log("Nothing currently selected, default to UIFirstSelected: EventSystem.current.currentSelectedGameObject = " + EventSystem.current.currentSelectedGameObject);
            }
        }
#else

        public void Update()
        {
            // Prevent Deselection
            if (system.currentSelectedGameObject != null && system.currentSelectedGameObject != selectedGameObject)
            {
                selectedGameObject = system.currentSelectedGameObject;
            }
            else if (selectedGameObject != null && system.currentSelectedGameObject == null)
            {
                system.SetSelectedGameObject(selectedGameObject);
            }

            // Controller: Detect if nothing is selected and controller input detected, and set default
            bool nothingSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null;
            bool inactiveButtonSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && !EventSystem.current.currentSelectedGameObject.activeInHierarchy;

            if ((nothingSelected || inactiveButtonSelected)
                && (Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f))
            {
                if (UIFirstSelected.activeSelf == true)
                {
                    EventSystem.current.SetSelectedGameObject(UIFirstSelected);
                }
                else if (UIFindSelectable.activeSelf == true)
                {
                    EventSystem.current.SetSelectedGameObject(UIFindSelectable);
                }

                Debug.Log("Nothing currently selected, default to UIFirstSelected: EventSystem.current.currentSelectedGameObject = " + EventSystem.current.currentSelectedGameObject);
            }

            // Tab between input fields
            if (Input.GetKeyDown(KeyCode.Tab)
                && system.currentSelectedGameObject != null)
            {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                InputField inputField = system.currentSelectedGameObject.GetComponent<InputField>();

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
#endif

        private Selectable FindTopUISelectable()
        {
            Selectable currentTop = Selectable.allSelectablesArray[0];
            double currentTopYaxis = currentTop.transform.position.y;

            foreach (Selectable s in Selectable.allSelectablesArray)
            {
                if (s.transform.position.y > currentTopYaxis &&
                    s.navigation.mode != Navigation.Mode.None)
                {
                    currentTop = s;
                    currentTopYaxis = s.transform.position.y;
                }
            }

            return currentTop;
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

            idInputField.gameObject.SetActive(true);
            tokenInputField.gameObject.SetActive(true);
            idText.gameObject.SetActive(true);
            tokenText.gameObject.SetActive(true);

            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = SceneSwitcherDropDown,
                selectOnDown = idInputField.InputFieldButton
            };
            
            loginButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = tokenInputField.InputFieldButton,
                selectOnDown = logoutButton,
                selectOnLeft = logoutButton
            };
        }

        private void ConfigureUIForAccountPortalLogin()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "Account Portal");

            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);

            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = SceneSwitcherDropDown,
                selectOnDown = loginButton
            };

            loginButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = loginTypeDropdown,
                selectOnDown = logoutButton,
                selectOnLeft = logoutButton
            };

            // AC/TODO: Reduce duplicated UI code for the different login types
            SceneSwitcherDropDown.gameObject.SetActive(true);
            DemoTitle.gameObject.SetActive(true);
            loginTypeDropdown.gameObject.SetActive(true);

            loginButton.enabled = true;
            loginButton.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);

            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        private void ConfigureUIForPersistentLogin()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "PersistentAuth");

            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);

            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = SceneSwitcherDropDown,
                selectOnDown = loginButton
            };

            loginButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = loginTypeDropdown,
                selectOnDown = logoutButton,
                selectOnLeft = logoutButton
            };
        }

        //-------------------------------------------------------------------------
        private void ConfigureUIForExternalAuth()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "ExternalAuth");

            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);

            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = SceneSwitcherDropDown,
                selectOnDown = loginButton
            };

            loginButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = loginTypeDropdown,
                selectOnDown = logoutButton,
                selectOnLeft = logoutButton
            };
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
                case LoginCredentialType.ExternalAuth:
                    ConfigureUIForExternalAuth();
                    break;
                case LoginCredentialType.Developer:
                default:
                    ConfigureUIForDevAuthLogin();
                    break;
            }

            // Controller
            //EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
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
            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (ref LogoutCallbackInfo data) => {
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

        public void OnExitButtonClick()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }
    }
}
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

        public RectTransform idContainer;
        public Text idText;
        public UIConsoleInputField idInputField;

        public Text tokenText;
        public UIConsoleInputField tokenInputField;

        public RectTransform connectTypeContainer;
        public Dropdown connectTypeDropdown;

        public Text loginButtonText;
        private string _OriginalloginButtonText;
        public Button loginButton;
        private Coroutine PreventLogIn = null;
        public Button logoutButton;

        public UnityEvent OnLogin;
        public UnityEvent OnLogout;

        [Header("Controller")]
        public GameObject UIFirstSelected;
        public GameObject UIFindSelectable;

        private EventSystem system;
        private GameObject selectedGameObject;

        LoginCredentialType loginType = LoginCredentialType.Developer;
        bool useConnectLogin = false;

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
            useConnectLogin = false;

#if UNITY_EDITOR || (UNITY_STANDALONE_OSX && EOS_PREVIEW_PLATFORM) || UNITY_STANDALONE_WIN || (UNITY_STANDALONE_LINUX && EOS_PREVIEW_PLATFORM)
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
            useConnectLogin = false;
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
                case 4:
                    //select unused type to avoid having to modify all loginType checks
                    loginType = LoginCredentialType.Password;
                    useConnectLogin = true;
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
            _OriginalloginButtonText = loginButtonText.text;
            InitConnectDropdown();
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
                else if (UIFindSelectable && UIFindSelectable.activeSelf == true)
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

            idContainer.gameObject.SetActive(true);
            connectTypeContainer.gameObject.SetActive(false);
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

            idContainer.gameObject.SetActive(true);
            connectTypeContainer.gameObject.SetActive(false);
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

            loginButtonText.text = _OriginalloginButtonText;
            if (PreventLogIn != null)
                StopCoroutine(PreventLogIn);
            loginButton.enabled = true;
            loginButton.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);

            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        private void ConfigureUIForPersistentLogin()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "PersistentAuth");

            idContainer.gameObject.SetActive(true);
            connectTypeContainer.gameObject.SetActive(false);
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

            idContainer.gameObject.SetActive(true);
            connectTypeContainer.gameObject.SetActive(false);
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

        private void ConfigureUIForConnectLogin()
        {
            idContainer.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);
            connectTypeContainer.gameObject.SetActive(true);

            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = SceneSwitcherDropDown,
                selectOnDown = connectTypeDropdown
            };

            connectTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = loginTypeDropdown,
                selectOnDown = logoutButton,
                selectOnLeft = logoutButton
            };
        }

        private void InitConnectDropdown()
        {
            List<Dropdown.OptionData> connectOptions = new List<Dropdown.OptionData>();

            List<ExternalCredentialType> credentialTypes = new List<ExternalCredentialType>
            {
                ExternalCredentialType.DeviceidAccessToken,
                //ExternalCredentialType.GogSessionTicket,
                //ExternalCredentialType.AppleIdToken,
                //ExternalCredentialType.GoogleIdToken,
                //ExternalCredentialType.OculusUseridNonce,
                //ExternalCredentialType.ItchioJwt,
                //ExternalCredentialType.ItchioKey,
                //ExternalCredentialType.AmazonAccessToken
            };

#if UNITY_STANDALONE
            credentialTypes.Add(ExternalCredentialType.SteamSessionTicket);
            credentialTypes.Add(ExternalCredentialType.SteamAppTicket);
            
#endif

#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            credentialTypes.Add(ExternalCredentialType.DiscordAccessToken);
#endif

            foreach (var type in credentialTypes)
            {
                connectOptions.Add(new Dropdown.OptionData() { text = type.ToString() });
            }

            connectTypeDropdown.options = connectOptions;
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

            loginButtonText.text = _OriginalloginButtonText;
            if (PreventLogIn != null)
                StopCoroutine(PreventLogIn);
            loginButton.enabled = true;
            loginButton.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);

            if (useConnectLogin)
            {
                ConfigureUIForConnectLogin();
            }
            else
            {
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
            connectTypeContainer.gameObject.SetActive(false);

            if (OnLogin != null)
            {
                OnLogin.Invoke();
            }
        }

        public void OnLogoutButtonClick()
        {
            if (EOSManager.Instance.GetLocalUserId() == null)
            {
                EOSManager.Instance.ClearConnectId(EOSManager.Instance.GetProductUserId());
                ConfigureUIForLogin();
                return;
            }

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

        //-------------------------------------------------------------------------
        private void StartLoginWithSteam()
        {
            var steamManager = Steam.SteamManager.Instance;
            string steamId = steamManager?.GetSteamID();
            string steamToken = steamManager?.GetSessionTicket();
            if(steamId == null)
            {
                Debug.LogError("ExternalAuth failed: Steam ID not valid");
            }
            else if (steamToken == null)
            {
                Debug.LogError("ExternalAuth failed: Steam session ticket not valid");
            }
            else
            {
                EOSManager.Instance.StartLoginWithLoginTypeAndToken(
                        LoginCredentialType.ExternalAuth,
                        ExternalCredentialType.SteamSessionTicket,
                        steamId,
                        steamToken,
                        StartLoginWithLoginTypeAndTokenCallback);
            }
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
            if(PreventLogIn!=null)
                StopCoroutine(PreventLogIn);
            PreventLogIn = StartCoroutine(TurnButtonOnAfter15Sec());
            //usernameInputField.enabled = false;
            //passwordInputField.enabled = false;
            print("Attempting to login...");

            // Disabled at the moment to work around a crash that happens
            //LoggingInterface.SetCallback((LogMessage logMessage) =>{
            //    print(logMessage.Message);
            //});

            if (useConnectLogin)
            {
                string typeName = connectTypeDropdown.options[connectTypeDropdown.value].text;
                if (Enum.TryParse(typeName, out ExternalCredentialType externalType))
                {
                    AcquireTokenForConnectLogin(externalType);
                }
            }
            else if (loginType == LoginCredentialType.ExternalAuth)
            {
                StartLoginWithSteam();
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

        private void AcquireTokenForConnectLogin(ExternalCredentialType externalType)
        {
            switch (externalType)
            {
                case ExternalCredentialType.SteamSessionTicket:
                    ConnectSteamSessionTicket();
                    break;

                case ExternalCredentialType.SteamAppTicket:
                    ConnectSteamAppTicket();
                    break;

                case ExternalCredentialType.DeviceidAccessToken:
                    ConnectDeviceId();
                    break;

                case ExternalCredentialType.DiscordAccessToken:
                    ConnectDiscord();
                    break;

                default:
                    Debug.LogError($"Connect Login for {externalType} not implemented");
                    loginButton.interactable = true;
                    break;
            }
        }

        private void ConnectSteamSessionTicket()
        {
            var steamManager = Steam.SteamManager.Instance;
            string steamToken = steamManager?.GetSessionTicket();
            if (steamToken == null)
            {
                Debug.LogError("Connect Login failed: Steam session ticket not valid");
                ConfigureUIForLogin();
            }
            else
            {
                StartConnectLoginWithToken(ExternalCredentialType.SteamSessionTicket, steamToken);
            }
        }

        private void ConnectSteamAppTicket()
        {
            var steamManager = Steam.SteamManager.Instance;
            if (steamManager == null)
            {
                Debug.LogError("Connect Login failed: Steam module unavailable");
                ConfigureUIForLogin();
            }
            else
            {
                steamManager.RequestAppTicket(OnSteamAppTicketReceived);
            }
        }

        private void OnSteamAppTicketReceived(string token)
        {
            if (token == null)
            {
                Debug.LogError("Connect Login failed: Unable to get Steam app ticket");
                ConfigureUIForLogin();
            }
            else
            {
                StartConnectLoginWithToken(ExternalCredentialType.SteamAppTicket, token);
            }
        }

        private void ConnectDeviceId()
        {
            var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            var options = new Epic.OnlineServices.Connect.CreateDeviceIdOptions()
            {
                DeviceModel = SystemInfo.deviceModel
            };

            connectInterface.CreateDeviceId(ref options, null, CreateDeviceCallback);
        }

        private void CreateDeviceCallback(ref Epic.OnlineServices.Connect.CreateDeviceIdCallbackInfo callbackInfo)
        {
            if (callbackInfo.ResultCode == Result.Success || callbackInfo.ResultCode == Result.DuplicateNotAllowed)
            {
#if UNITY_STANDALONE_WIN
                string displayName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#else
                //TODO: find device appropriate display name for other platforms
                string displayName = "Device User";
#endif
                StartConnectLoginWithToken(ExternalCredentialType.DeviceidAccessToken, null, displayName);
            }
            else
            {
                Debug.LogError("Connect Login failed: Failed to create Device Id");
                ConfigureUIForLogin();
            }
        }

        private void ConnectDiscord()
        {
            var discordManager = Discord.DiscordManager.Instance;
            if (discordManager == null)
            {
                Debug.LogError("Connect Login failed: DiscordManager unavailable");
                ConfigureUIForLogin();
                return;
            }

            Discord.DiscordManager.Instance.RequestOAuth2Token(OnDiscordAuthReceived);
        }

        private void OnDiscordAuthReceived(string token)
        {
            if (token == null)
            {
                Debug.LogError("Connect Login failed: Unable to get Discord OAuth2 token");
                ConfigureUIForLogin();
            }
            else
            {
                StartConnectLoginWithToken(ExternalCredentialType.DiscordAccessToken, token);
            }
        }

        private void StartConnectLoginWithToken(ExternalCredentialType externalType, string token, string displayName = null)
        {
            EOSManager.Instance.StartConnectLoginWithOptions(externalType, token, displayName, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
            {
                if (connectLoginCallbackInfo.ResultCode == Result.Success)
                {
                    print("Connect Login Successful. [" + connectLoginCallbackInfo.ResultCode + "]");
                    ConfigureUIForLogout();
                }
                else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser)
                {
                    // ask user if they want to connect; sample assumes they do
                    EOSManager.Instance.CreateConnectUserWithContinuanceToken(connectLoginCallbackInfo.ContinuanceToken, (Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo) =>
                    {
                        print("Creating new connect user");
                        if (createUserCallbackInfo.ResultCode == Result.Success)
                        {
                            ConfigureUIForLogout();
                        }
                        else
                        {
                            ConfigureUIForLogin();
                        }
                    });
                }
                else
                {
                    ConfigureUIForLogin();
                }
            });
        }

        //-------------------------------------------------------------------------
        private void StartConnectLoginWithLoginCallbackInfo(LoginCallbackInfo loginCallbackInfo)
        {
            EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
            {
                if (connectLoginCallbackInfo.ResultCode == Result.Success)
                {
                    print("Connect Login Successful. [" + connectLoginCallbackInfo.ResultCode + "]");
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
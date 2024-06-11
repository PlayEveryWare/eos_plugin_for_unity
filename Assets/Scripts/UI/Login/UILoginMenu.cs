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
    using System.IO;
    using System.Linq;

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
        public UITooltip tokenTooltip;

        public RectTransform connectTypeContainer;
        public Dropdown connectTypeDropdown;

        public Text loginButtonText;
        private string _OriginalloginButtonText;
        public Button loginButton;
        private Coroutine PreventLogIn = null;
        public Button logoutButton;
        public Button removePersistentTokenButton;

        public UnityEvent OnLogin;
        public UnityEvent OnLogout;

        /// <summary>
        /// This field contains information about the scenes that the user can select from.
        /// </summary>
        [Header("Scene Information")] 
        public SceneData SceneInformation;

        [Header("Controller")]
        public GameObject UIFirstSelected;
        public GameObject UIFindSelectable;

        private GameObject selectedGameObject;

        //use to indicate Connect login instead of Auth
        private const LoginCredentialType connect = (LoginCredentialType)(-1);
        private LoginCredentialType loginType = LoginCredentialType.Developer;
        //default to invalid value
        private const ExternalCredentialType invalidConnectType = (ExternalCredentialType)(-1);
        private ExternalCredentialType connectType = invalidConnectType;

        Apple.EOSSignInWithAppleManager signInWithAppleManager = null;

        // Retain Id/Token inputs across scenes
        public static string IdGlobalCache = string.Empty;
        public static string TokenGlobalCache = string.Empty;

        private void Awake()
        {
            idInputField.InputField.onEndEdit.AddListener(CacheIdInputField);
            tokenInputField.InputField.onEndEdit.AddListener(CacheTokenField);
#if UNITY_EDITOR
            loginType = LoginCredentialType.AccountPortal; // Default in editor
#elif UNITY_SWITCH
            loginType = LoginCredentialType.PersistentAuth; // Default on switch
#elif UNITY_PS4 || UNITY_PS5 || UNITY_GAMECORE
            loginType = LoginCredentialType.ExternalAuth; // Default on other consoles
#else
            loginType = LoginCredentialType.AccountPortal; // Default on other platforms
#endif

        // TODO: This will fail on anything that is mac, windows, or linux, or is an editor version of any of the above
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            idInputField.InputField.text = "localhost:7777"; //default on pc
#endif

#if !ENABLE_INPUT_SYSTEM && (UNITY_XBOXONE || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT || UNITY_PS4 || UNITY_PS5 || UNITY_SWITCH)
            Debug.LogError("Input currently handled by Input Manager. Input System Package is required for controller support on consoles.");
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

        public void OnLoginTypeChanged(int value)
        {
            switch (value)
            {
                case 1:
                    loginType = LoginCredentialType.AccountPortal;
                    ConfigureUIForAccountPortalLogin();
                    break;
                case 2:
                    loginType = LoginCredentialType.PersistentAuth;
                    ConfigureUIForPersistentLogin();
                    break;
                case 3:
                    loginType = LoginCredentialType.ExternalAuth;
                    ConfigureUIForExternalAuth();
                    break;
                case 4:
                    loginType = LoginCredentialType.ExchangeCode;
                    break;
                case 5:
                    loginType = connect;
                    break;
                case 0:
                default:
                    loginType = LoginCredentialType.Developer;
                    ConfigureUIForDevAuthLogin();
                    break;
            }

            if (loginType == connect)
            {
                connectType = GetConnectType();
            }
            else
            {
                connectType = invalidConnectType;
            }

            ConfigureUIForLogin();
        }

        public void OnConnectDropdownChange()
        {
            if (loginType != connect)
            {
                return;
            }

            connectType = GetConnectType();
            ConfigureUIForLogin();
        }

        private ExternalCredentialType GetConnectType()
        {
            string typeName = connectTypeDropdown.options[connectTypeDropdown.value].text;
            if (Enum.TryParse(typeName, out ExternalCredentialType externalType))
            {
                return externalType;
            }
            else
            {
                return invalidConnectType;
            }
        }

        public void Start()
        {
            _OriginalloginButtonText = loginButtonText.text;
            InitConnectDropdown();
            ConfigureUIForLogin();

            // Populate the Scene dropdown.
            SetupSceneDropdown();
        }

        private void SetupSceneDropdown()
        {
            // Get the currently active scene name.
            string currentSceneName = SceneManager.GetActiveScene().name;
            string currentSceneFriendlyName = string.Empty;
            
            // To store the friendly names for the scenes.
            List<string> sceneFriendlyNames = new();

            // Loop through the scene information provided.
            foreach (var sceneInfo in SceneInformation.scenes)
            {
                // TODO: Currently, checking to see if scene is a valid name does not work.
                //       It functions properly in the editor, but there is apparently some
                //       trickery that is unclear regarding getting scene names during runtime.
                //       This should be investigated further, but is fine for the time being.
                // Check to make sure it's a valid scene. This should prevent
                // the scene data asset from ever listing scenes that cannot be loaded.
                //if (!SceneUtility.IsValidSceneName(sceneInfo.sceneName))
                //{
                //    foreach(var name in SceneUtility.GetSceneNames())
                //        Debug.Log($"Scene name: \"{name}\".");
                //    throw new InvalidDataException($"Invalid scene name \"{sceneInfo.sceneName}\" provided in SceneData asset.");
                //}
                // Check to make sure that the scene actually exists
                // look for the friendly name of the current active scene.
                if (sceneInfo.sceneName == currentSceneName)
                    currentSceneFriendlyName = sceneInfo.friendlyName;

                // populate the list of friendly names for the scene.
                sceneFriendlyNames.Add(sceneInfo.friendlyName);
            }

            // Check to make sure that the friendly name for the scene was found.
            if (string.IsNullOrEmpty(currentSceneFriendlyName))
            {
                throw new InvalidDataException($"Cannot find friendly name for scene name \"{currentSceneName}\".");
            }

            // alphabetize the scene names.
            sceneFriendlyNames.Sort();

            // Add the sorted list of friendly names to the dropdown, clearing the options first.
            SceneSwitcherDropDown.ClearOptions();
            SceneSwitcherDropDown.AddOptions(sceneFriendlyNames);

            // Determine which scene to indicate is currently loaded.
            int currentSceneIndex = sceneFriendlyNames.IndexOf(currentSceneFriendlyName);

            // Set that index to be the current value.
            SceneSwitcherDropDown.value = currentSceneIndex;

            // Listen for the value to change, and load the selected scene when it does.
            SceneSwitcherDropDown.onValueChanged.AddListener(index =>
            {
                string friendlySceneNameSelected = SceneSwitcherDropDown.options[index]?.text;

                var sceneToLoad = SceneInformation.scenes
                    .Where(info => info.friendlyName == friendlySceneNameSelected)
                    .Select(info => info.sceneName)
                    .FirstOrDefault();

                Debug.LogFormat("UILoginMenu (OnDemoSceneChanged): value = {0}", sceneToLoad);
                SceneManager.LoadScene(sceneToLoad);
            });
        }

        private void EnterPressedToLogin()
        {
            if (loginButton.IsActive())
            {
                OnLoginButtonClick();
            }
        }

        /// <summary>
        /// If no GameObject is selected, but one was selected before - then determines if the
        /// de-selected GameObject should actually still be detected. If so, brings focus back
        /// to the previously selected game object. If not, then the first selected GameObject
        /// is focused. Broadly speaking, this prevents a focus state wherein no GameObject has
        /// focus.
        /// </summary>
        /// <param name="previouslySelectedGameObject">Reference to the GameObject that had focus on the last update.</param>
        private static void PreventDeselection(ref GameObject previouslySelectedGameObject)
        {
            // Prevent Deselection
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject != previouslySelectedGameObject)
            {
                previouslySelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
            else if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.activeInHierarchy == false)
            {
                // If there is no selected object, or if the currently selected object is not visible.
                if (previouslySelectedGameObject == null || previouslySelectedGameObject.activeInHierarchy == false)
                {
                    // Then set the currently selected object to be the first selected game object.
                    previouslySelectedGameObject = EventSystem.current.firstSelectedGameObject;
                }

                EventSystem.current.SetSelectedGameObject(previouslySelectedGameObject);
            }
        }

        /// <summary>
        /// Determines whether input should be passed to the scene, or if it should be skipped.
        /// This is in-order to prevent input from being handled when the EOS overlay is active.
        /// </summary>
        /// <returns>True if input should be handled, false if not.</returns>
        private static bool ShouldInputBeHandled()
        {
            // Event System isn't found, so main app cannot handle input
            if (null == EventSystem.current)
            {
                Debug.Log("EventSystem is null");
                return false;
            }

            // Main app handles input if overlay isn't open
            bool shouldHandle = !EOSManager.Instance.IsOverlayOpenWithExclusiveInput();

#if ENABLE_INPUT_SYSTEM
            EventSystem.current.currentInputModule.enabled = shouldHandle;
            EventSystem.current.sendNavigationEvents = shouldHandle;
#else
            // TODO: Clarify why this is only evaluated for PS4 and PS5
#if UNITY_PS4 || UNITY_PS5
            EventSystem.current.sendNavigationEvents = shouldHandle;
#endif
#endif

#if ENABLE_DEBUG_INPUT
            LogInputChanged(shouldHandle);
#endif
            return shouldHandle;

        }

#if ENABLE_DEBUG_INPUT
        private static bool previousShouldHandle = false;
        private static void LogInputChanged(bool shouldHandle)
        {
            if (previousShouldHandle != shouldHandle)
            {
                Debug.LogWarning($"Input {(shouldHandle ? "enabled" : "disabled")} for main app");
                previousShouldHandle = shouldHandle;
            }
        }
#endif
        /// <summary>
        /// Determines whether a GameObject needs to be set as selected.
        /// </summary>
        /// <returns>Returns true if a GameObject needs to be set, false otherwise.</returns>
        private static bool ShouldSetSelectedGameObject()
        {
            bool wasInputDetected = InputUtility.WasGamepadUsedLastFrame() || InputUtility.WasMouseUsedLastFrame();

            // If there was no input, or if there is a currently selected game object that is active,
            // then stop the process.
            return (false == wasInputDetected ||
                    (null != EventSystem.current.currentSelectedGameObject &&
                     EventSystem.current.currentSelectedGameObject.activeInHierarchy));
        }


        /// <summary>
        /// Determines which GameObject should be selected.
        /// </summary>
        /// <param name="firstGameObject">Reference to the GameObject that is considered to be the "first" in tab focus order.</param>
        /// <param name="findSelectable">Reference to the GameObject that can be used to find other selectables if doing so is necessary.</param>
        private static void SetSelectedGameObject(ref GameObject firstGameObject, ref GameObject findSelectable)
        {
            if (null != EventSystem.current.currentSelectedGameObject)
                return;

            var nextSelectable = FindObjectsOfType<Selectable>(false)
                .FirstOrDefault(s => s.navigation.mode != Navigation.Mode.None);
            if (null != nextSelectable)
            {
                EventSystem.current.SetSelectedGameObject(nextSelectable.gameObject);
            }
        }

        private static void HandleTabInput()
        {
            // Stop handling if Tab is not pressed, or if there is no current event system.
            if (!InputUtility.TabWasPressed() || null == EventSystem.current || null == EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            // Find the next selectable by selecting up or down based on whether the shift or shift equivalent is pressed.
            Selectable next = (InputUtility.ShiftIsPressed())
                ? EventSystem.current.currentSelectedGameObject
                    .GetComponent<Selectable>().FindSelectableOnUp()
                : EventSystem.current.currentSelectedGameObject
                    .GetComponent<Selectable>().FindSelectableOnDown();

            // NOTE: Previously the following while loop was only executed when ENABLE_INPUT_SYSTEM is set.
            // TODO: Confirm no regressions in functionality.
            while (null != next && !next.gameObject.activeSelf)
            {
                next = InputUtility.ShiftIsPressed() ? next.FindSelectableOnDown() : next.FindSelectableOnUp();
            }

            if (next != null)
            {
                // If the "next" control getting focus has an input field component.
                if (next.TryGetComponent<InputField>(out var inputField))
                {
                    // Then simulate a pointer click on that component.
                    inputField.OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
            else
            {
                // Find the navigable selectable with the highest y position (highest on the
                // screen), and set "next" to that selectable.
                next = Selectable.allSelectablesArray
                    .Where(selectable => selectable.navigation.mode != Navigation.Mode.None && selectable.gameObject.activeInHierarchy && selectable.gameObject.activeSelf)
                    .OrderByDescending(selectable => selectable.transform.position.y)
                    .FirstOrDefault();
            }

            // If a "next" control has been found, then set the selected game object to the
            // game object associated with it.
            if (next != null)
            {
                EventSystem.current.SetSelectedGameObject(next.gameObject);
            }
        }

        /// <summary>
        /// Handles Enter or Enter equivalent to press the login.
        /// </summary>
        private void HandleEnterInput()
        {
            // Skip if enter wasn't pressed, if the event system is null, or if there is no currently selected game object
            if (!InputUtility.WasEnterPressed() || null == EventSystem.current ||
                null == EventSystem.current.currentSelectedGameObject)
            {
                return;
            }
            // NOTE: Previously, this was only checked when the new Input System was being used
            // TODO: Test behavior of "Enter" for both input systems.
            InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            UIConsoleInputField consoleInputField = EventSystem.current.currentSelectedGameObject.GetComponent<UIConsoleInputField>();

            if (inputField != null || consoleInputField != null)
            {
                EnterPressedToLogin();
            }
        }

        /// <summary>
        /// Handles a variety of common input tasks executed every frame.
        /// </summary>
        /// <param name="previouslySelected">The GameObject that was most recently selected.</param>
        /// <param name="firstSelected">The firstselectable (typically UIFirstSelected which is the GameObject controller for the scene.</param>
        /// <param name="findSelectable">The findSelectable.</param>
        private static void HandleInput(ref GameObject previouslySelected, ref GameObject firstSelected,
            ref GameObject findSelectable)
        {
            // Prevents game object from being de-selected.
            // NOTE: This seems to intentionally be called *before* determining if input should be handled.
            // TODO: Determine whether it should be called after determining if input should be handled.
            PreventDeselection(ref previouslySelected);

            // Determines whether or not to handle the input (typically not if the EOS overlay is active).
            // If input should not be handled, then the process stops here.
            if (!ShouldInputBeHandled()) { return; }

            // Set the selected GameObject if doing so is necessary.
            if (ShouldSetSelectedGameObject())
            {
                SetSelectedGameObject(ref firstSelected, ref findSelectable);
            }

            // If tab was pressed, progress the selected control to the next appropriate one.
            HandleTabInput();
        }

        public void Update()
        { 
            HandleInput(ref selectedGameObject, ref UIFirstSelected, ref UIFindSelectable);

            HandleEnterInput();

            if (null != signInWithAppleManager)
            {
                signInWithAppleManager.Update();
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

            idContainer.gameObject.SetActive(true);
            connectTypeContainer.gameObject.SetActive(false);
            idInputField.gameObject.SetActive(true);
            tokenInputField.gameObject.SetActive(true);
            idText.gameObject.SetActive(true);
            tokenText.text = "Username";
            tokenTooltip.Text = "Username configured in EOS Dev Auth Tool";
            tokenText.gameObject.SetActive(true);
            removePersistentTokenButton.gameObject.SetActive(false);

            tokenInputField.InputFieldButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = idInputField.InputFieldButton,
                selectOnDown = loginButton
            };

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
            removePersistentTokenButton.gameObject.SetActive(false);

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
            removePersistentTokenButton.gameObject.SetActive(true);

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
            removePersistentTokenButton.gameObject.SetActive(false);

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

        private void ConfigureUIForExchangeCode()
        {
            loginTypeDropdown.value = loginTypeDropdown.options.FindIndex(option => option.text == "ExchangeCode");

            idContainer.gameObject.SetActive(true);
            connectTypeContainer.gameObject.SetActive(false);
            idInputField.gameObject.SetActive(false);
            tokenInputField.gameObject.SetActive(false);
            idText.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);
            removePersistentTokenButton.gameObject.SetActive(false);

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
            connectTypeContainer.gameObject.SetActive(true);

            tokenInputField.gameObject.SetActive(false);
            tokenText.gameObject.SetActive(false);
            removePersistentTokenButton.gameObject.SetActive(false);

            switch (connectType)// might need to check this better, use ifdefs to turn on platform specific cases that switch off the login button
            {

                //case ExternalCredentialType.GogSessionTicket:
                //case ExternalCredentialType.GoogleIdToken:
                //case ExternalCredentialType.ItchioJwt:
                //case ExternalCredentialType.ItchioKey:
                //case ExternalCredentialType.AmazonAccessToken:

#if !(UNITY_STANDALONE)
                case ExternalCredentialType.SteamSessionTicket:
                case ExternalCredentialType.SteamAppTicket:
                    loginButton.interactable = false;
                    loginButtonText.text = "Platform not set up.";
                    break;
#endif
#if !(UNITY_STANDALONE || UNITY_ANDROID)
                case ExternalCredentialType.OculusUseridNonce:
                    loginButton.interactable = false;
                    loginButtonText.text = "Platform not set up.";
                    break;
#endif
#if !(UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS)
                case ExternalCredentialType.DiscordAccessToken:
                case ExternalCredentialType.OpenidAccessToken:
                    loginButton.interactable = false;
                    loginButtonText.text = "Platform not set up.";
                    break;
#endif
#if !(UNITY_IOS || UNITY_STANDALONE_OSX)
                case ExternalCredentialType.AppleIdToken:
                    loginButton.interactable = false;
                    loginButtonText.text = "Platform not supported.";
                    break;
#endif
                case ExternalCredentialType.DeviceidAccessToken:
                default:
                    break;
            }

            if (connectType == ExternalCredentialType.OpenidAccessToken)
            {
                tokenText.text = "Credentials";
                tokenTooltip.Text = "Credentials for OpenID login sample in the form of username:password";
                tokenInputField.gameObject.SetActive(true);
                tokenText.gameObject.SetActive(true);

                connectTypeDropdown.navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = loginTypeDropdown,
                    selectOnDown = tokenInputField.InputFieldButton,
                    selectOnLeft = logoutButton
                };

                loginButton.navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = tokenInputField.InputFieldButton,
                    selectOnDown = logoutButton,
                    selectOnLeft = logoutButton
                };

                tokenInputField.InputFieldButton.navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = connectTypeDropdown,
                    selectOnDown = loginButton
                };
            }
            else
            {
                connectTypeDropdown.navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = loginTypeDropdown,
                    selectOnDown = loginButton,
                    selectOnLeft = logoutButton
                };

                loginButton.navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = connectTypeDropdown,
                    selectOnDown = logoutButton,
                    selectOnLeft = logoutButton
                };
            }

            loginTypeDropdown.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = SceneSwitcherDropDown,
                selectOnDown = connectTypeDropdown
            };

            
        }

        private void InitConnectDropdown()
        {
            List<string> externalCredentialTypeLabels = (from ExternalCredentialType type in Enum.GetValues(typeof(ExternalCredentialType))
                where type.IsDemoed() 
                select type.ToString()).ToList();

            externalCredentialTypeLabels.Sort();

            connectTypeDropdown.options = externalCredentialTypeLabels.Select(
                label => new Dropdown.OptionData() { text=label }).ToList();
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
            loginButton.interactable = true;
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
                case LoginCredentialType.ExchangeCode:
                    ConfigureUIForExchangeCode();
                    break;
                case connect:
                    ConfigureUIForConnectLogin();
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
            removePersistentTokenButton.gameObject.SetActive(false);

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
            // if the readme is open, then close it.
            UIReadme readme = UIReadme.FindObjectOfType<UIReadme>();
            readme?.CloseReadme();

            if (EOSManager.Instance.GetLocalUserId() == null)
            {
                EOSManager.Instance.ClearConnectId(EOSManager.Instance.GetProductUserId());
                ConfigureUIForLogin();
                return;
            }

            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (ref LogoutCallbackInfo data) => {
                if (data.ResultCode == Result.Success)
                {
#if (UNITY_PS4 || UNITY_PS5) && !UNITY_EDITOR
#if UNITY_PS4
                    var psnManager = EOSManager.Instance.GetOrCreateManager<EOSPSNManagerPS4>();
#elif UNITY_PS5
                    var psnManager = EOSManager.Instance.GetOrCreateManager<EOSPSNManagerPS5>();
#endif

                    ///TODO: Use activating controller index here
                    psnManager.StartLogoutWithPSN(0);
#endif
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

        /// <summary>
        /// Start a login operation using Steam.
        /// </summary>
        private void StartLoginWithSteam()
        {
            var steamManager = Steam.SteamManager.Instance;
            string steamId = steamManager?.GetSteamID();
            string steamToken = steamManager?.GetSessionTicket();

            // Use a coroutine to accomplish a non-UI blocking delay between obtaining
            // the steam session ticket and the login operation that uses it. This delay
            // is implemented here in the Scene instead of the EOSManager. It is up to
            // developers to determine how best to deal with timing issues, and we do not
            // want to introduce delays to code that are outside the direct and explicit
            // control of game developers. When you implement Steam authentication, you 
            // may find that implementing such a delay improves your user's experience.
            StartCoroutine(StartLoginWithSteamDelayed(() =>
            {
                if (steamId == null)
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
            }));
        }

        /// <summary>
        /// This function is to be called by MonoBehavior's "StartCoroutine" function. While it's
        /// signature implies functionality unique to the situation of providing a non-blocking
        /// delay to the steam login operation, it's functionality is generic and might later
        /// be abstracted into a more generic function.
        /// </summary>
        /// <param name="action">The action to execute after the indicated seconds have elapsed.</param>
        /// <param name="secondsToDelay">The seconds to wait before executing the indicated action.</param>
        /// <returns>An enumerator.</returns>
        private IEnumerator StartLoginWithSteamDelayed(Action action, float secondsToDelay = 0.5f)
        {
            yield return new WaitForSeconds(secondsToDelay);

            action();
        }

        //-------------------------------------------------------------------------
        // Username and password aren't always the username and password
        public void OnLoginButtonClick()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("Internet not reachable.");
                return;
            }

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

            if (loginType == connect)
            {
                AcquireTokenForConnectLogin(connectType);
            }
            else if (loginType == LoginCredentialType.ExternalAuth)
            {
#if (UNITY_PS4 || UNITY_PS5) && !UNITY_EDITOR
#if UNITY_PS4
                    var psnManager = EOSManager.Instance.GetOrCreateManager<EOSPSNManagerPS4>();
#elif UNITY_PS5
                    var psnManager = EOSManager.Instance.GetOrCreateManager<EOSPSNManagerPS5>();
#endif

                ///TOO(mendsley): Use activating controller index here
                psnManager.StartLoginWithPSN(0, StartLoginWithLoginTypeAndTokenCallback);
#elif UNITY_SWITCH && !UNITY_EDITOR
                var nintendoManager = EOSManager.Instance.GetOrCreateManager<EOSNintendoManager>();
                nintendoManager.StartLoginWithNSAPreselectedUser(StartLoginWithLoginTypeAndTokenCallback);
#elif UNITY_GAMECORE && !UNITY_EDITOR
                EOSXBLManager xblManager = EOSManager.Instance.GetOrCreateManager<EOSXBLManager>();
                xblManager.StartLoginWithXbl(StartLoginWithLoginTypeAndTokenCallback);
#else
                Steam.SteamManager.Instance.StartLoginWithSteam(StartLoginWithLoginTypeAndTokenCallback);
#endif
            }
            else if (loginType == LoginCredentialType.PersistentAuth)
            {
#if UNITY_SWITCH && !UNITY_EDITOR
                var nintendoManager = EOSManager.Instance.GetOrCreateManager<EOSNintendoManager>();
                nintendoManager.StartLoginWithPersistantAuthPreselectedUser((LoginCallbackInfo callbackInfo) =>
            {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        ConfigureUIForLogout();
                    }
                    else
                    {
                        ConfigureUIForLogin();
                    }
                });
#else
                EOSManager.Instance.StartPersistentLogin((Epic.OnlineServices.Auth.LoginCallbackInfo callbackInfo) =>
                {
                    // In this state, it means one needs to login in again with the previous login type, or a new one, as the
                    // tokens are invalid
                    if (callbackInfo.ResultCode != Epic.OnlineServices.Result.Success)
                    {
                        print("Failed to login with Persistent token [" + callbackInfo.ResultCode + "]");
                        // Other platforms: Fallback to DevAuth login flow
                        loginType = LoginCredentialType.Developer;
                        ConfigureUIForDevAuthLogin();
                    }
                    else
                    {
                        StartLoginWithLoginTypeAndTokenCallback(callbackInfo);
                    }
                });
#endif
            }
            else if (loginType == LoginCredentialType.ExchangeCode) 
            {
                EOSManager.Instance.StartLoginWithLoginTypeAndToken(loginType,
                                                                       null,
                                                                       EOSManager.Instance.GetCommandLineArgsFromEpicLauncher().authPassword,
                                                                       StartLoginWithLoginTypeAndTokenCallback);
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

                case ExternalCredentialType.AppleIdToken:
                    ConnectAppleId();
                    break;

                case ExternalCredentialType.DiscordAccessToken:
                    ConnectDiscord();
                    break;

                case ExternalCredentialType.OpenidAccessToken:
                    ConnectOpenId();
                    break;

                case ExternalCredentialType.OculusUseridNonce:
                    ConnectOculus();
                    break;

                default:
                    if (externalType == invalidConnectType)
                    {
                        Debug.LogError($"Connect type not valid");
                    }
                    else
                    {
                        Debug.LogError($"Connect Login for {externalType} not implemented");
                    }
                    loginButton.interactable = true;
                    break;
            }
        }

        private void ConnectSteamSessionTicket()
        {
            Steam.SteamManager.Instance.StartConnectLoginWithSteamSessionTicket(ConnectLoginTokenCallback);
        }

        private void ConnectSteamAppTicket()
        {
            Steam.SteamManager.Instance.StartConnectLoginWithSteamAppTicket(ConnectLoginTokenCallback);
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
                //this may return "Unknown" on some platforms
                string displayName = Environment.UserName;
                EOSManager.Instance.StartConnectLoginWithOptions(ExternalCredentialType.DeviceidAccessToken, null, displayName, ConnectLoginTokenCallback);
            }
            else
            {
                Debug.LogError("Connect Login failed: Failed to create Device Id");
                ConfigureUIForLogin();
            }
        }

        private void ConnectAppleId()
        {
            signInWithAppleManager = new Apple.EOSSignInWithAppleManager();
            Debug.Log("Start Connect Login with Apple Id");
            
            signInWithAppleManager.RequestTokenAndUsername((string token,string username) =>
            {
                StartConnectLoginWithToken(ExternalCredentialType.AppleIdToken, token, username.Remove(31));
            });
        }
        
        private void ConnectDiscord()
        {
            if (Discord.DiscordManager.Instance == null)
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
                EOSManager.Instance.StartConnectLoginWithOptions(ExternalCredentialType.DiscordAccessToken, token, onloginCallback: ConnectLoginTokenCallback);
            }
        }

        private void ConnectOpenId()
        {
            var tokenParts = tokenInputField.InputField.text.Split(':');
            if (tokenParts.Length >= 2)
            {
                string username = tokenParts[0].Trim();
                string password = tokenParts[1].Trim();
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    OpenId.OpenIdRequestManager.Instance.RequestToken(username, password, OnOpenIdTokenReceived);
                    return;
                }
            }

            Debug.LogError("Connect Login failed: OpenID credentials should be entered as \"username:password\"");
            ConfigureUIForLogin();
        }

        private void OnOpenIdTokenReceived(string username, string token)
        {
            if (token == null)
            {
                Debug.LogError("Connect Login failed: Unable to acquire OpenID token");
                ConfigureUIForLogin();
            }
            else
            {
                EOSManager.Instance.StartConnectLoginWithOptions(ExternalCredentialType.OpenidAccessToken, token, onloginCallback: ConnectLoginTokenCallback);
            }

        }

        private void ConnectOculus()
        {
            if (Oculus.OculusManager.Instance == null)
            {
                Debug.LogError("Connect Login failed: oculusManager unavailable. Is Oculus setup and available for this platform?");
                ConfigureUIForLogin();
                return;
            }

            Oculus.OculusManager.Instance.GetUserProof(OnOculusProofReceived);
        }

        private void OnOculusProofReceived(string idAndNonce, string OculusID)
        {
            if (string.IsNullOrEmpty(idAndNonce) || string.IsNullOrEmpty(OculusID))
            {
                Debug.LogError("Connect Login failed: Unable to get Oculus Proof. Is Oculus setup and available for this platform?");
                ConfigureUIForLogin();
            }
            else
            {
                StartConnectLoginWithToken(ExternalCredentialType.OculusUseridNonce, idAndNonce, OculusID);
            }
        }

        private void StartConnectLoginWithToken(ExternalCredentialType externalType, string token, string displayName = null)
        {
            EOSManager.Instance.StartConnectLoginWithOptions(externalType, token, displayName, ConnectLoginTokenCallback);
        }

        private void ConnectLoginTokenCallback(Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo)
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
        }

        //-------------------------------------------------------------------------
        private void StartConnectLoginWithEpicAccount(EpicAccountId LocalUserId)
        {
            EOSManager.Instance.StartConnectLoginWithEpicAccount(LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
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
                        EOSManager.Instance.StartConnectLoginWithEpicAccount(LocalUserId, (Epic.OnlineServices.Connect.LoginCallbackInfo retryConnectLoginCallbackInfo) =>
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
                StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId);
            }
            else if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.InvalidUser)
            {
                print("Trying Auth link with external account: " + loginCallbackInfo.ContinuanceToken);
                EOSManager.Instance.AuthLinkExternalAccountWithContinuanceToken(loginCallbackInfo.ContinuanceToken, 
#if UNITY_SWITCH
                                                                                LinkAccountFlags.NintendoNsaId,
#else
                                                                                LinkAccountFlags.NoFlags,
#endif
                                                                                (Epic.OnlineServices.Auth.LinkAccountCallbackInfo linkAccountCallbackInfo) =>
                {
                    if (linkAccountCallbackInfo.ResultCode == Result.Success)
                    {
                        StartConnectLoginWithEpicAccount(linkAccountCallbackInfo.LocalUserId);
                    }
                    else
                    {
                        print("Error Doing AuthLink with continuance token in. [" + linkAccountCallbackInfo.ResultCode + "]");
                    }
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

        public void OnRemovePersistentTokenButtonClick()
        {
            EOSManager.Instance.RemovePersistentToken();
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

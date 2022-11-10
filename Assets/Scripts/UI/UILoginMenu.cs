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
    public class UILoginMenu : MonoBehaviour, ILoginNavigation
    {
        [Header("Authentication UI")]
        public Text DemoTitle;
        public Dropdown SceneSwitcherDropDown;

        [Header("Controller")]
        public GameObject UIFirstSelected;
        public GameObject UIFindSelectable;

        private EventSystem system;
        private GameObject selectedGameObject;

        public void OnDemoSceneChange(int value)
        {
            value = value - 1;
            Debug.LogFormat("UILoginMenu (OnDemoSceneChanged): value = {0}", value);
            SceneManager.LoadScene(value);
        }

        public void Start()
        {
            system = EventSystem.current;
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

        public Selectable GetSelectOnUpObject()
        {
            return SceneSwitcherDropDown;
        }

        public void ConfigureUIForLogin()
        {
            SceneSwitcherDropDown.gameObject.SetActive(true);
            DemoTitle.gameObject.SetActive(true);

            // Controller
            //EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void ConfigureUIForLogout()
        {
            SceneSwitcherDropDown.gameObject.SetActive(false);
            DemoTitle.gameObject.SetActive(false);
        }

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

        public void OnExitButtonClick()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }
    }
}
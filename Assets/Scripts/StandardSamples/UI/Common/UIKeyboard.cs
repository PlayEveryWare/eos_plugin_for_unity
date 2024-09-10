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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

    public class UIKeyboard : MonoBehaviour
    {
        public static UIKeyboard instance;

        public GameObject KeyboardPanel;

        [Header("Alpha Keyboard")] public GameObject AlphaKeyboard_ToggleA;
        public GameObject AlphaKeyboard_ToggleB;
        public GameObject AK_KeyboardSwitchButton;

        public GameObject[] AlphaKeyboard_SelectableOnToggle;

        [Header("Numeric Keyboard")] public GameObject NumericKeyboard_ToggleA;
        public GameObject NumericKeyboard_ToggleB;
        public GameObject NK_KeyboardSwitchButton;

        public GameObject[] NumericKeyboard_SelectableOnToggle;

        public InputField KeyboardInput;
        private string currentInput = string.Empty;

        [Header("Controller")] public GameObject UIFirstSelected;

        // Manager Callbacks
        private OnKeyboardCompleted KeyboardCallback;

        public delegate void OnKeyboardCompleted(string result);

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

#if ENABLE_INPUT_SYSTEM
    private void Update()
    {
        var gamepad = Gamepad.current;
        if(gamepad == null)
        {
            return; // No gamepad connected
        }

        if(gamepad.bButton.wasPressedThisFrame)
        {
            KeyBackspace();
        }

        if(gamepad.leftStickButton.wasPressedThisFrame)
        {
            ToggleKeyboard();
        }

        if(gamepad.startButton.wasPressedThisFrame || gamepad.yButton.wasPressedThisFrame)
        {
            KeyboardCompleted();
        }
    }
#endif

        public void ShowKeyboard(string value, OnKeyboardCompleted ShowKeyboardCompleted)
        {
            // Clear previous state & set passed in value
            currentInput = value;
            KeyboardInput.text = currentInput;

            ShowAlphaKeyboard(false);

            KeyboardPanel.SetActive(true);
            KeyboardCallback = ShowKeyboardCompleted;

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void KeyOnClick(string value)
        {
            currentInput += value;
            updateKeyboardInput();
        }

        public void KeyBackspace()
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                updateKeyboardInput();
            }
        }

        private void updateKeyboardInput()
        {
            KeyboardInput.text = currentInput;
        }

        public void ShowAlphaKeyboard(bool selectSwitchButton = true)
        {
            AlphaKeyboard_ToggleA.SetActive(true);
            AlphaKeyboard_ToggleB.SetActive(false);

            NumericKeyboard_ToggleA.SetActive(false);
            NumericKeyboard_ToggleB.SetActive(false);

            if (selectSwitchButton)
            {
                EventSystem.current.SetSelectedGameObject(AK_KeyboardSwitchButton);
            }
        }

        public void ShowNumericKeyboard()
        {
            AlphaKeyboard_ToggleA.SetActive(false);
            AlphaKeyboard_ToggleB.SetActive(false);

            NumericKeyboard_ToggleA.SetActive(true);
            NumericKeyboard_ToggleB.SetActive(false);

            // Controller
            EventSystem.current.SetSelectedGameObject(NK_KeyboardSwitchButton);
        }

        public void ToggleKeyboard()
        {
            if (AlphaKeyboard_ToggleA.activeInHierarchy || AlphaKeyboard_ToggleB.activeInHierarchy)
            {
                if (AlphaKeyboard_ToggleA.activeInHierarchy)
                {
                    AlphaKeyboard_ToggleA.SetActive(false);
                    AlphaKeyboard_ToggleB.SetActive(true);
                }
                else
                {
                    AlphaKeyboard_ToggleA.SetActive(true);
                    AlphaKeyboard_ToggleB.SetActive(false);
                }

                foreach (GameObject toggleButton in AlphaKeyboard_SelectableOnToggle)
                {
                    if (toggleButton.activeInHierarchy)
                    {
                        // Controller
                        EventSystem.current.SetSelectedGameObject(toggleButton);
                    }
                }
            }
            else if (NumericKeyboard_ToggleA.activeInHierarchy || NumericKeyboard_ToggleB.activeInHierarchy)
            {
                if (NumericKeyboard_ToggleA.activeInHierarchy)
                {
                    NumericKeyboard_ToggleA.SetActive(false);
                    NumericKeyboard_ToggleB.SetActive(true);
                }
                else
                {
                    NumericKeyboard_ToggleA.SetActive(true);
                    NumericKeyboard_ToggleB.SetActive(false);
                }

                foreach (GameObject toggleButton in NumericKeyboard_SelectableOnToggle)
                {
                    if (toggleButton.activeInHierarchy)
                    {
                        // Controller
                        EventSystem.current.SetSelectedGameObject(toggleButton);
                    }
                }
            }
        }

        public void KeyboardCompleted()
        {
            KeyboardPanel.SetActive(false);
            KeyboardCallback?.Invoke(KeyboardInput.text);
        }
    }
}
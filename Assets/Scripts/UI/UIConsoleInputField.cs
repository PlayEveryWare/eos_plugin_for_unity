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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System;

public class UIConsoleInputField : MonoBehaviour
{
    public Button InputFieldButton;
    public InputField InputField;

    private void Awake()
    {
        InputField.onEndEdit.AddListener(OnEndEdit);
    }

    public void OnEndEdit(string value)
    {
        if (EventSystem.current != null && !EventSystem.current.alreadySelecting
            && EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject != InputFieldButton.gameObject)
        {
            // Return focus to button
            EventSystem.current.SetSelectedGameObject(InputFieldButton.gameObject);
        }
    }
    public void InputFieldOnClick()
    {
#if ENABLE_INPUT_SYSTEM
        var gamepad = Gamepad.current;
        if (gamepad != null && gamepad.wasUpdatedThisFrame)
        {
            Debug.Log("KeyboardManager.InputFileOnClick(): Gamepad detected.");

            //KeyboardUI.instance.ShowKeyboard(InputField.text, OnKeyboardCompleted);
            EventSystem.current.SetSelectedGameObject(InputField.gameObject);
        }

        if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject == InputField.gameObject)
        {
            Debug.Log("InputField already selected.");
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.wasUpdatedThisFrame)
        {
            Debug.Log("KeyboardManager.InputFileOnClick(): Keyboard detected.");

            EventSystem.current.SetSelectedGameObject(InputField.gameObject);
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.wasUpdatedThisFrame)
        {
            Debug.Log("KeyboardManager.InputFileOnClick(): Mouse detected.");
            EventSystem.current.SetSelectedGameObject(InputField.gameObject);
        }
#else
        // Keyboard & Mouse
        EventSystem.current.SetSelectedGameObject(InputField.gameObject);
#endif
    }

    private void OnKeyboardCompleted(string result)
    {
        // Update Input Field
        InputField.text = result;

        // Return focus to button
        InputField.onEndEdit.Invoke(result);
    }
}

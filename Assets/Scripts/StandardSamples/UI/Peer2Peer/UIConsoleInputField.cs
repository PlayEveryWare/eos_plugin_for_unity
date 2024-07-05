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

using PlayEveryWare.EpicOnlineServices.Samples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIConsoleInputField : MonoBehaviour
{
    public Button InputFieldButton;
    public InputField InputField;

#if UNITY_ANDROID
    private bool keepOldTextInField;
    private string editText, oldEditText;
    private TouchScreenKeyboard.Status prevKeyboardStatus = TouchScreenKeyboard.Status.Done;
#endif

    private void Awake()
    {
        InputField.onEndEdit.AddListener(OnEndEdit);

#if UNITY_ANDROID
        InputField.onValueChanged.AddListener(OnEdit);
#endif
    }

#if UNITY_ANDROID
    private void Update()
    {
        if(InputField.touchScreenKeyboard == null)
        {
            return;
        }

        var kbStatus = InputField.touchScreenKeyboard.status;
        if (kbStatus != prevKeyboardStatus && kbStatus == TouchScreenKeyboard.Status.Canceled)
        {
            keepOldTextInField = true;
        }
        prevKeyboardStatus = kbStatus;
    }

    private void OnEdit(string currentText)
    {
        oldEditText = editText;
        editText = currentText;
    }
#endif

    public void OnEndEdit(string value)
    {
        if (EventSystem.current != null && !EventSystem.current.alreadySelecting
            && EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject != InputFieldButton.gameObject)
        {
            // Return focus to button
            EventSystem.current.SetSelectedGameObject(InputFieldButton.gameObject);
        }

#if UNITY_ANDROID
        if (keepOldTextInField && !string.IsNullOrEmpty(oldEditText))
        {
            editText = oldEditText;
            InputField.text = oldEditText;
            keepOldTextInField = false;
        }
#endif
    }
    public void InputFieldOnClick()
    {
        // If the event system is null
        if (EventSystem.current == null ||
            // If there is no currently selected game object
            EventSystem.current.currentSelectedGameObject == null ||
            // If the InputField is already the selected game object
            (EventSystem.current.currentSelectedGameObject == InputField.gameObject) ||
            // If neither the gamepad, keyboard, or mouse were used in the last frame
            (!InputUtility.WasGamepadUsedLastFrame() && !InputUtility.WasKeyboardUsedLastFrame() &&
             !InputUtility.WasMouseUsedLastFrame()))
        {
            // Don't do anything
            return;
        }

        EventSystem.current.SetSelectedGameObject(InputField.gameObject);
    }

    private void OnKeyboardCompleted(string result)
    {
        // Update Input Field
        InputField.text = result;

        // Return focus to button
        InputField.onEndEdit.Invoke(result);
    }
}

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardUI : MonoBehaviour
{
    [Header("Alpha Keyboard")]
    public GameObject AlphaKeyboard_ToggleA;
    public GameObject AlphaKeyboard_ToggleB;
    public GameObject AK_KeyboardSwitchButton;

    public GameObject[] AlphaKeyboard_SelectableOnToggle;

    [Header("Numeric Keyboard")]
    public GameObject NumericKeyboard_ToggleA;
    public GameObject NumericKeyboard_ToggleB;
    public GameObject NK_KeyboardSwitchButton;

    public GameObject[] NumericKeyboard_SelectableOnToggle;

    public InputField KeyboardInput;
    private string currentInput = string.Empty;

    [Header("Controller")]
    public GameObject UIFirstSelected;

    private void Start()
    {
        ShowKeyboard();
    }

    private void Update()
    {
        // Controller
        if (Input.GetButtonDown("B"))
        {
            KeyBackspace();
        }
    }

    public void ShowKeyboard()
    {
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
        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        updateKeyboardInput();
    }

    private void updateKeyboardInput()
    {
        KeyboardInput.text = currentInput;
    }

    public void ShowAlphaKeyboard()
    {
        AlphaKeyboard_ToggleA.SetActive(true);
        AlphaKeyboard_ToggleB.SetActive(false);

        NumericKeyboard_ToggleA.SetActive(false);
        NumericKeyboard_ToggleB.SetActive(false);

        // Controller
        EventSystem.current.SetSelectedGameObject(AK_KeyboardSwitchButton);
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
        if(AlphaKeyboard_ToggleA.activeInHierarchy || AlphaKeyboard_ToggleB.activeInHierarchy)
        {
            if(AlphaKeyboard_ToggleA.activeInHierarchy)
            {
                AlphaKeyboard_ToggleA.SetActive(false);
                AlphaKeyboard_ToggleB.SetActive(true);
            }
            else
            {
                AlphaKeyboard_ToggleA.SetActive(true);
                AlphaKeyboard_ToggleB.SetActive(false);
            }

            foreach(GameObject toggleButton in AlphaKeyboard_SelectableOnToggle)
            {
                if(toggleButton.activeInHierarchy)
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
}

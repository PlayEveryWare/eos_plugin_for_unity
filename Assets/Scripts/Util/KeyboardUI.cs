using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardUI : MonoBehaviour
{
    public GameObject AlphaKeyboard;
    public GameObject AlphaKeyboard_ToggleA;
    public GameObject AlphaKeyboard_ToggleB;

    public GameObject NumericKeyboard;
    public GameObject NumericKeyboard_ToggleA;
    public GameObject NumericKeyboard_ToggleB;

    public InputField KeyboardInput;
    private string currentInput = string.Empty;

    [Header("Controller")]
    public GameObject UIFirstSelected;

    private void Start()
    {
        // Controller
        EventSystem.current.SetSelectedGameObject(UIFirstSelected);
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
        AlphaKeyboard.SetActive(true);
        NumericKeyboard.SetActive(false);
    }

    public void ShowNumericKeyboard()
    {
        AlphaKeyboard.SetActive(false);
        NumericKeyboard.SetActive(true);
    }

    public void ToggleKeyboard()
    {
        if(AlphaKeyboard.activeInHierarchy)
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
        }
        else if (NumericKeyboard.activeInHierarchy)
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
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIFileNameEntry : MonoBehaviour
    {
        public Button FileNameButton;
        public Text FileNameTxt;

        public Color SelectedColor;

        // Callbacks
        public Action<string> FileNameOnClick;

        public void ShowSelectedColor()
        {
            FileNameButton.GetComponent<Image>().color = SelectedColor;
        }

        public void FileNameOnClickHandler()
        {
            if (FileNameOnClick != null)
            {
                FileNameOnClick(FileNameTxt.text);
            }
            else
            {
                Debug.LogError("UIFileNameEntry: FileNameOnClickCallBack is not defined!");
            }
        }
    }
}
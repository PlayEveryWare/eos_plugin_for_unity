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

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIPlayerDataStorageMenu : MonoBehaviour
    {
        [Header("Player Data Storage UI")]
        public GameObject PlayerDataStorageUIParent;

        public ConsoleInputField NewFileNameTextBox;

        public GameObject FilesContentParent;
        public GameObject UIFileNameEntryPrefab;

        public ConsoleInputField FileContentTextBox;


        [Header("Controller")]
        public GameObject UIFirstSelected;

        private string currentSelectedFile = string.Empty;

        private EOSPlayerDataStorageManager PlayerDataStorageManager;

        public void Start()
        {
            FileContentTextBox.InputField.text = string.Empty;
            PlayerDataStorageManager = EOSManager.Instance.GetOrCreateManager<EOSPlayerDataStorageManager>();
        }

        private int previousFrameStorageDataCount = 0;
        private bool updateUI = false;

        private void Update()
        {
            if (PlayerDataStorageManager.GetCachedStorageData().Count != previousFrameStorageDataCount || updateUI)
            {
                // Destroy current UI member list
                foreach (Transform child in FilesContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (string fileName in PlayerDataStorageManager.GetCachedStorageData().Keys)
                {
                    GameObject fileUIObj = Instantiate(UIFileNameEntryPrefab, FilesContentParent.transform);
                    UIFileNameEntry uiEntry = fileUIObj.GetComponent<UIFileNameEntry>();

                    uiEntry.FileNameTxt.text = fileName;
                    uiEntry.FileNameOnClick = FileListOnClick;

                    if (fileName.Equals(currentSelectedFile, StringComparison.OrdinalIgnoreCase))
                    {
                        uiEntry.ShowSelectedColor();
                        FileContentTextBox.InputField.text = GetLocalFileData(fileName);
                    }
                }

                updateUI = false;
            }

            previousFrameStorageDataCount = PlayerDataStorageManager.GetCachedStorageData().Count;
        }

        public void RefreshButtonOnClick()
        {
            PlayerDataStorageManager.QueryFileList();

            updateUI = true;
        }

        public void UpdateUI()
        {
            updateUI = true;
        }

        public void NewFileButtonOnClick()
        {
            if (string.IsNullOrEmpty(NewFileNameTextBox.InputField.text))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (NewFileButtonOnClick): Invalid File Name!");
                return;
            }

            PlayerDataStorageManager.AddFile(NewFileNameTextBox.InputField.text, string.Empty);

            NewFileNameTextBox.InputField.text = string.Empty;
        }

        public void SaveButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (SaveButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataStorageManager.AddFile(currentSelectedFile, FileContentTextBox.InputField.text, UpdateUI);
        }

        public void DownloadButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DownloadButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataStorageManager.StartFileDataDownload(currentSelectedFile, UpdateUI);
        }

        public void DuplicateButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DuplicateButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataStorageManager.CopyFile(currentSelectedFile, currentSelectedFile + "_Copy");
        }

        public void DeleteButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DeleteButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataStorageManager.DeleteFile(currentSelectedFile);

            currentSelectedFile = string.Empty;
        }

        private void FileListOnClick(string fileName)
        {
            // Store selected
            //PlayerDataStorageManager.SelectFile(fileName);
            currentSelectedFile = fileName;

            // Update File content if local cash exists
            FileContentTextBox.InputField.text = GetLocalFileData(fileName);

            updateUI = true;
        }

        private string GetLocalFileData(string fileName)
        {
            string fileContent = PlayerDataStorageManager.GetCachedFileContent(fileName);

            if (fileContent == null)
            {
                fileContent = "*** Download File Content ***";
            }
            else if (fileContent.Length == 0)
            {
                fileContent = "*** File is empty ***";
            }

            return fileContent;
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSPlayerDataStorageManager>().OnLoggedIn();

            PlayerDataStorageUIParent.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void HideMenu()
        {
            PlayerDataStorageManager?.OnLoggedOut();

            PlayerDataStorageUIParent.gameObject.SetActive(false);
        }
    }
}
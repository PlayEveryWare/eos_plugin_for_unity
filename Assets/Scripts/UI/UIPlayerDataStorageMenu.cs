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
    [Serializable]
    public class PlayerDataInventory
    {
        public const string InventoryType = "INVENTORY";
        public string Type = InventoryType;
        public int Sword = 0;
        public int Potion = 0;
        public int Gold = 0;
        public int Ration = 0;

        public bool IsValid()
        {
            return Type == InventoryType;
        }
    }

    public class UIPlayerDataStorageMenu : MonoBehaviour, ISampleSceneUI
    {
        [Header("Player Data Storage UI")]
        public GameObject PlayerDataStorageUIParent;

        public UIConsoleInputField NewFileNameTextBox;

        public GameObject FilesContentParent;
        public GameObject UIFileNameEntryPrefab;

        public Text RemoteViewText;
        public Text LocalViewText;
        public Text CurrentFileNameText;

        public Dropdown AddItemDropdown;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private string currentSelectedFile = string.Empty;

        private EOSPlayerDataStorageManager PlayerDataStorageManager;
        private PlayerDataInventory currentInventory = null;

        private HashSet<string> fileNames;
        private List<UIFileNameEntry> fileNameUIEntries;

        private void Awake()
        {
            fileNames = new HashSet<string>();
            fileNameUIEntries = new List<UIFileNameEntry>();
            fileNameUIEntries.AddRange(FilesContentParent.GetComponentsInChildren<UIFileNameEntry>(true));
            PlayerDataStorageManager = EOSManager.Instance.GetOrCreateManager<EOSPlayerDataStorageManager>();
        }

        private void Start()
        {
            RemoteViewText.text = string.Empty;
            LocalViewText.text = string.Empty;
            CurrentFileNameText.text = "*No File Selected*";
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSPlayerDataStorageManager>();
        }

        private void UpdateFileListUI()
        {
            if (PlayerDataStorageManager.GetCachedStorageData().Count != fileNameUIEntries.Count)
            {
                // Destroy current UI member list
                foreach (var entry in fileNameUIEntries)
                {
                    Destroy(entry.gameObject);
                }
                fileNameUIEntries.Clear();
                fileNames.Clear();

                foreach (string fileName in PlayerDataStorageManager.GetCachedStorageData().Keys)
                {
                    fileNames.Add(fileName);
                    GameObject fileUIObj = Instantiate(UIFileNameEntryPrefab, FilesContentParent.transform);
                    UIFileNameEntry uiEntry = fileUIObj.GetComponent<UIFileNameEntry>();

                    uiEntry.FileNameTxt.text = fileName;
                    uiEntry.FileNameOnClick = FileListOnClick;

                    fileNameUIEntries.Add(uiEntry);
                }
            }

            if (!fileNames.Contains(currentSelectedFile))
            {
                RemoteViewText.text = string.Empty;
                LocalViewText.text = string.Empty;
                CurrentFileNameText.text = "*No File Selected*";
                currentInventory = null;
            }
        }

        public void RefreshButtonOnClick()
        {
            PlayerDataStorageManager.GetCachedStorageData().Clear();

            PlayerDataStorageManager.QueryFileList();

            if (currentSelectedFile != string.Empty)
            {
                PlayerDataStorageManager.StartFileDataDownload(currentSelectedFile, () => UpdateRemoteView(currentSelectedFile));
            }
        }

        public void NewFileButtonOnClick()
        {
            if (string.IsNullOrEmpty(NewFileNameTextBox.InputField.text))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (NewFileButtonOnClick): Invalid File Name!");
                return;
            }

            string newFileContents = JsonUtility.ToJson(new PlayerDataInventory(), true);

            PlayerDataStorageManager.AddFile(NewFileNameTextBox.InputField.text, newFileContents, UpdateFileListUI);

            NewFileNameTextBox.InputField.text = string.Empty;
        }

        public void SaveButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (SaveButtonOnClick): Need to select a file first.");
                return;
            }

            if (currentInventory == null)
            {
                Debug.LogError("UIPlayerDatatStorageMenu (SaveButtonOnClick): Local file is not a valid inventory.");
                return;
            }

            PlayerDataStorageManager.AddFile(currentSelectedFile, LocalViewText.text, () => UpdateRemoteView(currentSelectedFile));
        }

        public void DownloadButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DownloadButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataInventory newInventory = null;
            try
            {
                newInventory = JsonUtility.FromJson<PlayerDataInventory>(RemoteViewText.text);
            }
            catch (Exception){ }
            if (newInventory != null && newInventory.IsValid())
            {
                currentInventory = newInventory;
                LocalViewText.text = JsonUtility.ToJson(currentInventory, true);
            }
            else
            {
                currentInventory = null;
                LocalViewText.text = "*** File is not valid inventory json ***";
            }
        }

        public void DuplicateButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DuplicateButtonOnClick): Need to select a file first.");
                return;
            }

            string copyName = currentSelectedFile + "_Copy";
            if (fileNames.Contains(copyName))
            {
                int copyIndex = 2;
                while (fileNames.Contains(copyName + copyIndex))
                {
                    copyIndex++;
                }
                copyName = copyName + copyIndex;
            }

            PlayerDataStorageManager.CopyFile(currentSelectedFile, copyName);
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
            CurrentFileNameText.text = "*No File Selected*";
        }

        private void FileListOnClick(string fileName)
        {
            if (currentSelectedFile == fileName)
            {
                return;
            }

            // Store selected
            //PlayerDataStorageManager.SelectFile(fileName);
            currentSelectedFile = fileName;

            // Update File content if local cache exists
            CurrentFileNameText.text = currentSelectedFile;
            UpdateRemoteView(fileName);
            currentInventory = null;
            LocalViewText.text = "*** Click Download button to create a local copy to modify ***";
        }

        private void UpdateRemoteView(string fileName)
        {
            string fileContent = PlayerDataStorageManager.GetCachedFileContent(fileName);

            if (fileContent == null)
            {
                RemoteViewText.text = "*** Downloading content ***";
                PlayerDataStorageManager.StartFileDataDownload(fileName, () => UpdateRemoteView(fileName));
            }
            else if (fileContent.Length == 0)
            {
                RemoteViewText.text = "*** File is empty ***";
            }
            else
            {
                RemoteViewText.text = fileContent;
            }
        }

        public void AddItemOnClick()
        {
            if (currentInventory == null)
            {
                Debug.LogError("UIPlayerDatatStorageMenu (AddItemOnClick): Need to download a valid inventory first.");
                return;
            }

            string itemToAdd = AddItemDropdown.options[AddItemDropdown.value].text;
            switch (itemToAdd)
            {
                case "Sword":
                    currentInventory.Sword++;
                    break;
                case "Potion":
                    currentInventory.Potion++;
                    break;
                case "Gold":
                    currentInventory.Gold++;
                    break;
                case "Ration":
                    currentInventory.Ration++;
                    break;
            }

            LocalViewText.text = JsonUtility.ToJson(currentInventory, true);
        }

        public void ShowMenu()
        {
            UpdateFileListUI();
            PlayerDataStorageManager.AddNotifyFileListUpdated(UpdateFileListUI);
            PlayerDataStorageManager.OnLoggedIn();

            PlayerDataStorageUIParent.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void HideMenu()
        {
            PlayerDataStorageManager?.RemoveNotifyFileListUpdated(UpdateFileListUI);
            PlayerDataStorageManager?.OnLoggedOut();

            PlayerDataStorageUIParent.gameObject.SetActive(false);
            currentSelectedFile = string.Empty;
            currentInventory = null;
        }
    }
}
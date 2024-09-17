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
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using JsonUtility = Utility.JsonUtility;

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

    public class UIPlayerDataStorageMenu : SampleMenu
    {
        [Header("Player Data Storage UI")]

        public UIConsoleInputField NewFileNameTextBox;

        public GameObject FilesContentParent;
        public GameObject UIFileNameEntryPrefab;

        public Text RemoteViewText;
        public Text LocalViewText;
        public Text CurrentFileNameText;

        public Dropdown AddItemDropdown;

        private string currentSelectedFile = string.Empty;

        private PlayerDataInventory currentInventory = null;

        private HashSet<string> fileNames = new();
        private List<UIFileNameEntry> fileNameUIEntries = new();

        protected override void Awake()
        {
            base.Awake();
            fileNameUIEntries.AddRange(FilesContentParent.GetComponentsInChildren<UIFileNameEntry>(true));
        }

        private void Start()
        {
            RemoteViewText.text = string.Empty;
            LocalViewText.text = string.Empty;
            CurrentFileNameText.text = "*No File Selected*";
        }

        private void UpdateFileListUI()
        {
            if (PlayerDataStorageService.Instance.GetLocallyCachedData().Count != fileNameUIEntries.Count)
            {
                // Destroy current UI member list
                foreach (var entry in fileNameUIEntries)
                {
                    Destroy(entry.gameObject);
                }
                fileNameUIEntries.Clear();
                fileNames.Clear();

                foreach (string fileName in PlayerDataStorageService.Instance.GetLocallyCachedData().Keys)
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
            PlayerDataStorageService.Instance.GetLocallyCachedData().Clear();

            PlayerDataStorageService.Instance.QueryFileList();

            if (currentSelectedFile != string.Empty)
            {
                PlayerDataStorageService.Instance.DownloadFile(currentSelectedFile, () => UpdateRemoteView(currentSelectedFile));
            }
        }

        public void NewFileButtonOnClick()
        {
            if (string.IsNullOrEmpty(NewFileNameTextBox.InputField.text))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (NewFileButtonOnClick): Invalid File Name!");
                return;
            }

            // This is to test a typical use case
            string newFileContents = JsonUtility.ToJson(new PlayerDataInventory(), true);

            // TODO: Add this as a test case.
            // Un-comment the following lines to test a large file
            // newFileContents = new string('*', 20000);

            PlayerDataStorageService.Instance.AddFile(NewFileNameTextBox.InputField.text, newFileContents, UpdateFileListUI);

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

            PlayerDataStorageService.Instance.AddFile(currentSelectedFile, LocalViewText.text, () => UpdateRemoteView(currentSelectedFile));
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

            PlayerDataStorageService.Instance.CopyFile(currentSelectedFile, copyName);
        }

        public void DeleteButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DeleteButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataStorageService.Instance.DeleteFile(currentSelectedFile);

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
            string fileContent = PlayerDataStorageService.Instance.GetCachedFileContent(fileName);

            if (fileContent == null)
            {
                RemoteViewText.text = "*** Downloading content ***";
                PlayerDataStorageService.Instance.DownloadFile(fileName, () => UpdateRemoteView(fileName));
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

        public override void Show()
        {
            base.Show();
            UpdateFileListUI();
            PlayerDataStorageService.Instance.OnFileListUpdated += UpdateFileListUI;
        }

        public override void Hide()
        {
            base.Hide();

            currentSelectedFile = string.Empty;
            currentInventory = null;
        }
    }
}
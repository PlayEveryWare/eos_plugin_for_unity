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

    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;

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
        private Dictionary<string, string> localFileNameToContents = new Dictionary<string, string>();

        private PlayerDataInventory currentInventory = null;

        private HashSet<string> fileNames;
        private List<UIFileNameEntry> fileNameUIEntries;

        private void Awake()
        {
            fileNames = new HashSet<string>();
            fileNameUIEntries = new List<UIFileNameEntry>();
            fileNameUIEntries.AddRange(FilesContentParent.GetComponentsInChildren<UIFileNameEntry>(true));
        }

        private void Start()
        {
            RemoteViewText.text = string.Empty;
            LocalViewText.text = string.Empty;
            CurrentFileNameText.text = "*No File Selected*";
        }

        private async void UpdateFileListUI()
        {
            List<string> queriedFileNames = await PlayerDataStorageService.Instance.QueryFileList(EOSManager.Instance.GetProductUserId());

            // Append any local files not in the above list
            // and remove any local files that are in the above list
            foreach (string localKey in new List<string>(localFileNameToContents.Keys))
            {
                if (!queriedFileNames.Contains(localKey))
                {
                    queriedFileNames.Add(localKey);
                }
                else
                {
                    localFileNameToContents.Remove(localKey);
                }
            }

            // Destroy current UI member list
            foreach (var entry in fileNameUIEntries)
            {
                Destroy(entry.gameObject);
            }

            fileNameUIEntries.Clear();
            fileNames.Clear();

            foreach (string fileName in queriedFileNames)
            {
                fileNames.Add(fileName);
                GameObject fileUIObj = Instantiate(UIFileNameEntryPrefab, FilesContentParent.transform);
                UIFileNameEntry uiEntry = fileUIObj.GetComponent<UIFileNameEntry>();

                uiEntry.FileNameTxt.text = fileName;
                uiEntry.FileNameOnClick = FileListOnClick;

                fileNameUIEntries.Add(uiEntry);
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
            if (!string.IsNullOrEmpty(currentSelectedFile))
            {
                UpdateRemoteView(currentSelectedFile);
            }
        }

        public void NewFileButtonOnClick()
        {
            string newFileName = NewFileNameTextBox.InputField.text;

            if (string.IsNullOrEmpty(newFileName))
            {
                Debug.LogError($"{nameof(UIPlayerDataStorageMenu)} ({nameof(NewFileButtonOnClick)}): Invalid File Name!");
                return;
            }

            // This is to test a typical use case
            string newFileContents = JsonUtility.ToJson(new PlayerDataInventory(), true);

            // If it doesn't exist, make room in the dictionary for it
            if (!localFileNameToContents.ContainsKey(newFileName))
            {
                localFileNameToContents.Add(newFileName, string.Empty);
            }

            // Set the value
            localFileNameToContents[newFileName] = newFileContents;

            // Clear the name box, which gives the responsive feeling of the action being committed
            NewFileNameTextBox.InputField.text = string.Empty;

            // Update the list of files to show the new file
            UpdateFileListUI();
        }

        public async void SaveButtonOnClick()
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

            byte[] fileBytes = System.Text.UTF8Encoding.UTF8.GetBytes(LocalViewText.text);

            await PlayerDataStorageService.Instance.UploadFileAsync(currentSelectedFile, fileBytes);
            UpdateRemoteView(currentSelectedFile);
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
            catch (Exception) { }
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

        public async void DuplicateButtonOnClick()
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

            await PlayerDataStorageService.Instance.CopyFile(currentSelectedFile, copyName);
            UpdateFileListUI();
        }

        public async void DeleteButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (DeleteButtonOnClick): Need to select a file first.");
                return;
            }

            localFileNameToContents.Remove(currentSelectedFile);

            await PlayerDataStorageService.Instance.DeleteFile(currentSelectedFile);

            currentSelectedFile = string.Empty;
            CurrentFileNameText.text = "*No File Selected*";
            UpdateFileListUI();
        }

        private void FileListOnClick(string fileName)
        {
            if (currentSelectedFile == fileName)
            {
                return;
            }

            // Store selected
            currentSelectedFile = fileName;
            CurrentFileNameText.text = currentSelectedFile;

            if (localFileNameToContents.TryGetValue(fileName, out string json))
            {
                currentInventory = JsonUtility.FromJson<PlayerDataInventory>(json);
                LocalViewText.text = json;
            }
            else
            {
                currentInventory = null;
                LocalViewText.text = "*** Click Download button to create a local copy to modify ***";
            }

            UpdateRemoteView(fileName);
        }

        private async void UpdateRemoteView(string fileName)
        {
            EOSPlayerDataStorageTransferTask downloadTask = PlayerDataStorageService.Instance.DownloadFile(EOSManager.Instance.GetProductUserId(), fileName);

            // The Service should not respond immediately unless there is an error
            if (downloadTask.ResultCode.HasValue)
            {
                Debug.LogError($"{nameof(UIPlayerDataStorageMenu)} ({nameof(UpdateRemoteView)}): Failed to download remote file. Result code {downloadTask.ResultCode}");
                return;
            }

            RemoteViewText.text = "*** Downloading content ***";

            await downloadTask.InnerTaskCompletionSource.Task;

            if (downloadTask.ResultCode == Result.NotFound)
            {
                // This file wasn't found online - maybe it was created locally and not yet uploaded
                // This is not an error, no need to log
                RemoteViewText.text = "This file was not found online. Press Upload to upload it to Player Data Storage.";
                return;
            }

            if (downloadTask.ResultCode != Result.Success)
            {
                Debug.LogError($"{nameof(UIPlayerDataStorageMenu)} ({nameof(UpdateRemoteView)}): Failed to download remote file. Result code {downloadTask.ResultCode}");
                return;
            }

            byte[] resultingData = downloadTask.Data;

            if (resultingData == null || resultingData.Length == 0)
            {
                Debug.LogError($"{nameof(UIPlayerDataStorageMenu)} ({nameof(UpdateRemoteView)}): Failed to download remote file. The operation succeeded, but the resulting data was either null or empty.");
                return;
            }

            string contents = System.Text.Encoding.UTF8.GetString(resultingData);
            RemoteViewText.text = contents;
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

            PlayerDataStorageUIParent.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void HideMenu()
        {
            PlayerDataStorageUIParent.gameObject.SetActive(false);
            currentSelectedFile = string.Empty;
            currentInventory = null;
        }
    }
}
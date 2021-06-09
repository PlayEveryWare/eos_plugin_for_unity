﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIPlayerDataStorageMenu : MonoBehaviour
    {
        [Header("Player Data Storage UI")]
        public GameObject PlayerDataStorageUIParent;

        public InputField NewFileNameTextBox;

        public GameObject FilesContentParent;
        public GameObject UIFileNameEntryPrefab;

        public InputField FileContentTextBox;

        private string currentSelectedFile = string.Empty;

        private EOSPlayerDataStorageManager PlayerDataStorageManager;

        public void Start()
        {
            FileContentTextBox.text = string.Empty;
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
                        FileContentTextBox.text = GetLocalFileData(fileName);
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
            if (string.IsNullOrEmpty(NewFileNameTextBox.text))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (NewFileButtonOnClick): Invalid File Name!");
                return;
            }

            PlayerDataStorageManager.AddFile(NewFileNameTextBox.text, string.Empty);

            NewFileNameTextBox.text = string.Empty;
        }

        public void SaveButtonOnClick()
        {
            if (string.IsNullOrEmpty(currentSelectedFile))
            {
                Debug.LogError("UIPlayerDatatStorageMenu (SaveButtonOnClick): Need to select a file first.");
                return;
            }

            PlayerDataStorageManager.AddFile(currentSelectedFile, FileContentTextBox.text, UpdateUI);
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
            FileContentTextBox.text = GetLocalFileData(fileName);

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
        }

        public void HideMenu()
        {
            PlayerDataStorageManager?.OnLoggedOut();

            PlayerDataStorageUIParent.gameObject.SetActive(false);
        }
    }
}
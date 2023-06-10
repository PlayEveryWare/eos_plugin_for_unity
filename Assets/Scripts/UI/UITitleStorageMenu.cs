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
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Epic.OnlineServices;
using Epic.OnlineServices.TitleStorage;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Unity UI sample that uses <c>TitleStoragemanager</c> to demo features.  Can be used as a template or starting point for implementing Title Storage features.
    /// </summary>

    public class UITitleStorageMenu : MonoBehaviour, ISampleSceneUI
    {
        [Header("Title Storage UI")]
        public GameObject TitleStorageUIParent;

        public UIConsoleInputField AddTagTextBox;
        public UIConsoleInputField FileNameTextBox;

        public GameObject TagContentParent;
        public GameObject UITagEntryPrefab;

        public GameObject FileNameContentParent;
        public GameObject UIFileNameEntryPrefab;

        public Text FileContent;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private EOSTitleStorageManager TitleStorageManager;

        private List<string> CurrentTags = new List<string>();

        public void Awake()
        {
            HideMenu();
        }

        public void OnEnable()
        {
            if (EOSManager.Instance.IsEncryptionKeyValid())
            {
                FileContent.text = string.Empty;
            }
            else
            {
                FileContent.text = "Valid encryption key not set. Use the EOS Config Editor to add one.";
            }
        }

        private void Start()
        {
            TitleStorageManager = EOSManager.Instance.GetOrCreateManager<EOSTitleStorageManager>();
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSTitleStorageManager>();
        }

        public void AddTagOnClick()
        {
            if (AddTag(AddTagTextBox.InputField.text))
            {
                AddTagTextBox.InputField.text = string.Empty;
            }
        }

        public void ClearTagsOnClick()
        {
            CurrentTags.Clear();

            // Destroy current UI tags list
            foreach (Transform child in TagContentParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public void AddPlatformTagOnClick()
        {

#if UNITY_STANDALONE_WIN
            string platformTag = "PLATFORM_WINDOWS";
#elif UNITY_STANDALONE_OSX && EOS_PREVIEW_PLATFORM
            string platformTag = "PLATFORM_MAC";
#elif UNITY_STANDALONE_LINUX && EOS_PREVIEW_PLATFORM
            string platformTag = "PLATFORM_LINUX";
#elif UNITY_IOS
            string platformTag = "PLATFORM_IOS";
#elif UNITY_ANDROID
            string platformTag = "PLATFORM_ANDROID";
#elif UNITY_PS4
            string platformTag = "PLATFORM_PS4";
#elif UNITY_PS5
            string platformTag = "PLATFORM_PS5";
#elif UNITY_SWITCH
            string platformTag = "PLATFORM_SWITCH";
#elif UNITY_GAMECORE_XBOXONE
            string platformTag = "PLATFORM_XBOXONE";
#elif UNITY_GAMECORE_SCARLETT
            string platformTag = "PLATFORM_XBOXSERIES";
#else
            string platformTag = "PLATFORM_UNKNOWN";
#endif

            AddTag(platformTag);
        }

        private bool AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogError("UITitleStorageMenu - Empty tag cannot be added!");
                return false;
            }

            if (CurrentTags.Contains(tag))
            {
                Debug.LogErrorFormat("UITitleStorageMenu - Tag '{0}' is already in the list.", tag);
                return false;
            }

            CurrentTags.Add(tag);

            GameObject tagUIObj = Instantiate(UITagEntryPrefab, TagContentParent.transform);
            UITagEntry tagEntry = tagUIObj.GetComponent<UITagEntry>();
            if (tagEntry != null)
            {
                tagEntry.TagTxt.text = tag;
            }

            return true;
        }

        public void QueryListOnClick()
        {
            if (CurrentTags.Count == 0)
            {
                Debug.LogErrorFormat("Please enter at least one tag and press 'Add tag'.");
                return;
            }

            TitleStorageManager.QueryFileList(CurrentTags.ToArray(), SetFileListUI);
        }

        private void SetFileListUI(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UITitleStorageMenu - QueryFileList failed: {0}", result);
                return;
            }

            // Destroy current UI files list
            foreach (Transform child in FileNameContentParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (string entry in TitleStorageManager.GetCachedCurrentFileNames())
            {
                GameObject fileNameUIObj = Instantiate(UIFileNameEntryPrefab, FileNameContentParent.transform);
                UIFileNameEntry fileNameEntry = fileNameUIObj.GetComponent<UIFileNameEntry>();
                if (fileNameEntry != null)
                {
                    fileNameEntry.FileNameTxt.text = entry;
                    fileNameEntry.FileNameOnClick = FileNameEntryOnClick;
                }
            }
        }

        private string GetLocalData(string entryName)
        {
            TitleStorageManager.GetCachedStorageData().TryGetValue(entryName, out string data);

            if (!string.IsNullOrEmpty(data))
            {
                return data;
            }

            return string.Empty;
        }

        public void FileNameEntryOnClick(string fileName)
        {
            FileNameTextBox.InputField.text = fileName;
        }

        public void DownloadOnClick()
        {
            if (string.IsNullOrEmpty(FileNameTextBox.InputField.text))
            {
                Debug.LogError("UITitleStorageMenu - Empty FileName cannot be downloaded!");
                return;
            }

            if (!TitleStorageManager.GetCachedCurrentFileNames().Contains(FileNameTextBox.InputField.text))
            {
                Debug.LogError("UITitleStorageMenu - FileName doesn't exist, cannot be downloaded!");
                return;
            }

            // Check if it's already been downloaded
            string cachedData = GetLocalData(FileNameTextBox.InputField.text);
            if (!string.IsNullOrEmpty(cachedData))
            {
                Debug.Log("UITitleStorageMenu - FileName '{0}' already downloaded. Display content.");

                // Update UI
                FileContent.text = cachedData;
                return;
            }

            TitleStorageManager.ReadFile(FileNameTextBox.InputField.text, UpdateFileContent);

            // TODO: Show progress bar
        }

        public void UpdateFileContent(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UITitleStorageMenu - UpdateFileContent failed: {0}", result);
                return;
            }

            if (TitleStorageManager.GetCachedStorageData().TryGetValue(FileNameTextBox.InputField.text, out string fileContent))
            {
                // Update UI
                FileContent.text = fileContent;
            }
            else
            {
                Debug.LogErrorFormat("UITitleStorageMenu - '{0}' file content was not found in cached data storage.", FileNameTextBox.InputField.text);
            }
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSTitleStorageManager>().OnLoggedOut();

            TitleStorageUIParent.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void HideMenu()
        {
            TitleStorageUIParent.gameObject.SetActive(false);

            TitleStorageManager?.OnLoggedOut();
        }
    }
}
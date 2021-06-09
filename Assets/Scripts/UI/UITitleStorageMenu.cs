using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.TitleStorage;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UITitleStorageMenu : MonoBehaviour
    {
        [Header("Title Storage UI")]
        public GameObject TitleStorageUIParent;

        public InputField AddTagTextBox;
        public InputField FileNameTextBox;

        public GameObject TagContentParent;
        public GameObject UITagEntryPrefab;

        public GameObject FileNameContentParent;
        public GameObject UIFileNameEntryPrefab;

        public Text FileContent;

        private EOSTitleStorageManager TitleStorageManager;

        private List<string> CurrentTags = new List<string>();

        public void Awake()
        {
            HideMenu();
            FileContent.text = string.Empty;
        }

        private void Start()
        {
            TitleStorageManager = EOSManager.Instance.GetOrCreateManager<EOSTitleStorageManager>();
        }

        public void AddTagOnClick()
        {
            if (string.IsNullOrEmpty(AddTagTextBox.text))
            {
                Debug.LogError("UITitleStorageMenu - Empty tag cannot be added!");
                return;
            }

            if (CurrentTags.Contains(AddTagTextBox.text))
            {
                Debug.LogErrorFormat("UITitleStorageMenu - Tag '{0}' is already in the list.", AddTagTextBox.text);
                return;
            }

            CurrentTags.Add(AddTagTextBox.text);

            GameObject tagUIObj = Instantiate(UITagEntryPrefab, TagContentParent.transform);
            UITagEntry tagEntry = tagUIObj.GetComponent<UITagEntry>();
            if (tagEntry != null)
            {
                tagEntry.TagTxt.text = AddTagTextBox.text;
            }

            AddTagTextBox.text = string.Empty;
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
            FileNameTextBox.text = fileName;
        }

        public void DownloadOnClick()
        {
            if (string.IsNullOrEmpty(FileNameTextBox.text))
            {
                Debug.LogError("UITitleStorageMenu - Empty FileName cannot be downloaded!");
                return;
            }

            // Check if it's already been downloaded
            string cachedData = GetLocalData(FileNameTextBox.text);
            if (!string.IsNullOrEmpty(cachedData))
            {
                Debug.Log("UITitleStorageMenu - FileName '{0}' already downloaded. Display content.");

                // Update UI
                FileContent.text = cachedData;
                return;
            }

            TitleStorageManager.ReadFile(FileNameTextBox.text, UpdateFileContent);

            // TODO: Show progress bar
        }

        public void UpdateFileContent(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UITitleStorageMenu - UpdateFileContent failed: {0}", result);
                return;
            }

            if (TitleStorageManager.GetCachedStorageData().TryGetValue(FileNameTextBox.text, out string fileContent))
            {
                // Update UI
                FileContent.text = fileContent;
            }
            else
            {
                Debug.LogErrorFormat("UITitleStorageMenu - '{0}' file content was not found in cached data storage.", FileNameTextBox.text);
            }
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSTitleStorageManager>().OnLoggedOut();

            TitleStorageUIParent.gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            TitleStorageUIParent.gameObject.SetActive(false);

            TitleStorageManager?.OnLoggedOut();
        }
    }
}
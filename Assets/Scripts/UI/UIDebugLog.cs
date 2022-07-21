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

using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Epic.OnlineServices.Logging;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIDebugLog : MonoBehaviour
    {
        [Header("Debug Log UI")]
        public bool DisableOnScreenLog = false;
        public const int MAX_LINES_TO_DISPLAY = 7;
        public Text UIDebugLogText;
        public ScrollRect ScrollRect;

        private Queue<string> logCacheList = new Queue<string>();

        private bool _dirty = false;
        private string logCache = string.Empty;
        private string textFilter = string.Empty;

        private float deltaTime_FPS;
        public Text FPSValue;

        public SampleSceneUIContainer DemoSceneContainer;

        public GameObject[] OptionElements;
        private bool optionsVisible;

        private Vector2 initialAnchorMax;
        private Vector2 initialSizeDelta;
        private bool expanded;

        [Header("Log Level Menu")]
        public GameObject LogLevelMenu;
        public Transform LogLevelContentContainer;
        public UIDebugLogLevelMenuItem LogLevelTemplate;
        private UIDebugLogLevelMenuItem allCategoriesMenuItem;
        private List<UIDebugLogLevelMenuItem> logLevelMenuItems;
        private bool ignoreLogLevelChange;

        private void Start()
        {
            initialAnchorMax = (transform as RectTransform).anchorMax;
            initialSizeDelta = (transform as RectTransform).sizeDelta;
            expanded = false;

            ignoreLogLevelChange = true;
            logLevelMenuItems = new List<UIDebugLogLevelMenuItem>();
            BuildLogLevelMenu();
            ignoreLogLevelChange = false;
            LogLevelMenu.SetActive(false);
        }

        private void BuildLogLevelMenu()
        {
            LogLevelTemplate.InitDropdown();

            allCategoriesMenuItem = CreateLogCategoryItem(LogCategory.AllCategories);
            logLevelMenuItems.Add(allCategoriesMenuItem);
            foreach (LogCategory cat in Enum.GetValues(typeof(LogCategory)))
            {
                if (cat != LogCategory.AllCategories)
                {
                    logLevelMenuItems.Add(CreateLogCategoryItem(cat));
                }
            }
        }

        private UIDebugLogLevelMenuItem CreateLogCategoryItem(LogCategory Category)
        {
            var newItem = Instantiate(LogLevelTemplate, LogLevelContentContainer);
            newItem.SetCategory(Category);
            LogLevel catLevel = EOSManager.Instance.GetLogLevel(Category);
            newItem.SetLevel(catLevel);
            newItem.gameObject.SetActive(true);
            return newItem;
        }

        public void OnLogLevelChanged(LogCategory Category, LogLevel Level)
        {
            if (ignoreLogLevelChange)
            {
                //ignore value change events while menu is being initialized or changed here
                return;
            }

            EOSManager.Instance.SetLogLevel(Category, Level);
            ignoreLogLevelChange = true;
            if (Category == LogCategory.AllCategories)
            {
                //update all levels if AllCategories was changed
                foreach (var item in logLevelMenuItems)
                {
                    var cat = item.GetCategory();
                    if (cat != LogCategory.AllCategories)
                    {
                        item.SetLevel(EOSManager.Instance.GetLogLevel(cat));
                    }
                }
            }
            else
            {
                allCategoriesMenuItem.SetLevel(EOSManager.Instance.GetLogLevel(LogCategory.AllCategories));
            }
            ignoreLogLevelChange = false;
        }

        public void ToggleLogLevelMenu()
        {
            LogLevelMenu.SetActive(!LogLevelMenu.activeSelf);
        }

        void OnEnable()
        {
            if(!DisableOnScreenLog)
            {
                Application.logMessageReceived += UpdateLogCache;
            }
            else
            {
                UIDebugLogText.text = "<I>OnScreen Logging Disabled</I>";
            }

            foreach (var element in OptionElements)
            {
                element.SetActive(false);
            }
            optionsVisible = false;
        }

        void OnDisable()
        {
            if (!DisableOnScreenLog)
            {
                Application.logMessageReceived -= UpdateLogCache;
            }
            else
            {
                UIDebugLogText.text = string.Empty;
            }
        }

        void UpdateLogCache(string entry, string stackTrace, LogType type)
        {
            string logEntry = System.DateTime.UtcNow.ToString("HH:mm:ss.fff") + " [" + type + "] : " + entry;
            string color = string.Empty;

            switch (type)
            {
                case LogType.Warning:
                    color = "#FFFF00"; // Yellow
                    break;
                case LogType.Error:
                case LogType.Exception:
                    color = "#FF0000"; // Red
                    break;
            }

            if (!string.IsNullOrEmpty(color))
            {
                logEntry = "<color=" + color + ">" + logEntry + "</color>";
            }

            logCacheList.Enqueue(logEntry);

            if(logCacheList.Count > 100)
            {
                logCacheList.Dequeue();
            }

            _dirty = true;
        }

        string GetLastEntries()
        {
            if(_dirty)
            {
                logCache = string.Empty;

                foreach (string logEntry in logCacheList)
                {
                    if (textFilter != string.Empty && !logEntry.ToLower().Contains(textFilter))
                    {
                        continue;
                    }

                    logCache += '\n' + logEntry;
                }
                _dirty = false;
            }

            return logCache;
        }

        public void OnTextFilterEdit(string newFilter)
        {
            textFilter = newFilter.Trim().ToLower();
            _dirty = true;
        }

        public void ToggleOptions()
        {
            optionsVisible = !optionsVisible;
            foreach (var element in OptionElements)
            {
                element.SetActive(optionsVisible);
            }
        }

        public void ToggleExpand()
        {
            var rt = transform as RectTransform;

            expanded = !expanded;

            if (expanded)
            {
                rt.anchorMax = new Vector2(initialAnchorMax.x, 1);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0);
            }
            else
            {
                rt.anchorMax = initialAnchorMax;
                rt.sizeDelta = initialSizeDelta;
            }
        }

        public void ToggleLogVisibility()
        {
            bool newVisibility = !ScrollRect.gameObject.activeSelf;
            ScrollRect.gameObject.SetActive(newVisibility);
            DemoSceneContainer?.SetFullscreen(!newVisibility);
        }

        private void Update()
        {
            if (!DisableOnScreenLog)
            {
                UIDebugLogText.text = GetLastEntries();
            }

            // FPS
            deltaTime_FPS += (Time.deltaTime - deltaTime_FPS) * 0.1f;
            float fps = 1.0f / deltaTime_FPS;
            FPSValue.text = Mathf.Ceil(fps).ToString();
        }
    }
}
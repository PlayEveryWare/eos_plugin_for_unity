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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIDebugLog : MonoBehaviour
    {
        public bool DisableOnScreenLog = false;
        public const int MAX_LINES_TO_DISPLAY = 7;
        public Text UIDebugLogText;
        public ScrollRect ScrollRect;

        private Queue<string> logCacheList = new Queue<string>();

        private bool _dirty = false;
        private string logCache = string.Empty;

        private float deltaTime_FPS;
        public Text FPSValue;

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
                    logCache += '\n' + logEntry;
                }
                _dirty = false;
            }

            return logCache;
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
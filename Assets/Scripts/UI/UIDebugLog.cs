using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIDebugLog : MonoBehaviour
    {
        public const int MAX_LINES_TO_DISPLAY = 7;
        public Text UIDebugLogText;
        public ScrollRect ScrollRect;

        private Queue<string> logCacheList = new Queue<string>();

        void OnEnable()
        {
            Application.logMessageReceived += UpdateLogCache;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= UpdateLogCache;
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
        }

        string GetLastEntries()
        {
            string log = string.Empty;

            foreach (string logEntry in logCacheList)
            {
                log += '\n' + logEntry;
            }

            return log;
        }

        private void Update()
        {
            UIDebugLogText.text = GetLastEntries();
        }
    }
}
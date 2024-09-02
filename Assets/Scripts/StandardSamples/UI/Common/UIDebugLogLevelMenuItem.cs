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
    
    using Epic.OnlineServices.Logging;

    public class UIDebugLogLevelMenuItem : MonoBehaviour
    {
        public UIDebugLog DebugLog;
        public Text CategoryText;
        public Dropdown LogLevelDropdown;
        private LogCategory logCategory;

        private static LogCategory selectedCategory = (LogCategory)(-1);

        public void SetCategory(LogCategory Category)
        {
            logCategory = Category;
            CategoryText.text = logCategory.ToString();
        }

        public LogCategory GetCategory()
        {
            return logCategory;
        }

        public void SetLevel(LogLevel Level)
        {
            //handle AllCategories with mixed levels
            if (Level == (LogLevel)(-1))
            {
                LogLevelDropdown.captionText.text = "Mixed";
            }

            int levelIndex = LogLevelDropdown.options.FindIndex((Dropdown.OptionData option) => option.text == Level.ToString());
            if (levelIndex >= 0)
            {
                LogLevelDropdown.value = levelIndex;
                LogLevelDropdown.captionText.text = Level.ToString();
            }
        }

        public LogLevel GetLevel()
        {
            var option = LogLevelDropdown.options[LogLevelDropdown.value];
            LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), option.text);
            return level;
        }

        private LogLevel GetLevel(int optionIndex)
        {
            var option = LogLevelDropdown.options[optionIndex];
            LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), option.text);
            return level;
        }

        public void InitDropdown()
        {
            var logLevelOptions = new List<Dropdown.OptionData>();
            foreach (string levelName in Enum.GetNames(typeof(LogLevel)))
            {
                logLevelOptions.Add(new Dropdown.OptionData(levelName));
            }
            LogLevelDropdown.options = logLevelOptions;
        }

        public void OnValueChanged(int newValue)
        {
            DebugLog.OnLogLevelChanged(logCategory, GetLevel(newValue));
        }

        public void OnPointerDown()
        {
            CancelInvoke("ScrollToItem");
        }

        public void OnSelect()
        {
            if (selectedCategory == logCategory)
            {
                return;
            }

            selectedCategory = logCategory;
            Invoke("ScrollToItem", 0);
        }

        private void ScrollToItem()
        {
            DebugLog.ScrollToLogLevelItem(this);
        }
    }
}
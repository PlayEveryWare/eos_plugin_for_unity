/*
 * Copyright (c) 2024 PlayEveryWare
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

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using EpicOnlineServices.Utility;
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class LogLevelWindow : EOSEditorWindow
    {
        LogLevelConfig currentLogLevelConfig;

        string[] categories = LogLevelUtility.LogCategoryStringArray;
        string[] levels = LogLevelUtility.LogLevelStringArray;

        int selectedCategoryIndex = 0;
        bool showAllCategories = false;

        public LogLevelWindow() : base("Log Level Configuration")
        {
        }

        [MenuItem("Tools/EOS Plugin/Log Level Configuration")]
        public static void ShowWindow()
        {
            GetWindow<LogLevelWindow>();
        }
        protected override void Setup()
        {
            currentLogLevelConfig = EpicOnlineServices.Config.Get<LogLevelConfig>();

            // Config.Get<T> only guarantees T to be not null, but not for its members
            if (currentLogLevelConfig.LogCategoryLevelPairs == null) 
            {
                currentLogLevelConfig.LogCategoryLevelPairs = new List<LogCategoryLevelPair>();
            }

            string[] storedCategories = currentLogLevelConfig.LogCategoryLevelPairs.Select(pair => pair.Category).ToArray();

            // Initialize the list if config categories does not match the SDK categories, which might happen when using a different SDK version
            if (!storedCategories.SequenceEqual(categories))
            {
                currentLogLevelConfig.LogCategoryLevelPairs.Clear();

                foreach (var category in categories)
                {
                    currentLogLevelConfig.LogCategoryLevelPairs.Add(new LogCategoryLevelPair(category, "Info"));
                }

                currentLogLevelConfig.Write(true);
            }
        }
        protected override void RenderWindow()
        {
            // Last element in the list is All Categories
            string previousAllCategoryLogLevel = currentLogLevelConfig.LogCategoryLevelPairs[categories.Length - 1].Level;

            showAllCategories = EditorGUILayout.Toggle("Show All Log Settings", showAllCategories);
            EditorGUILayout.LabelField("Category", "Level", GUILayout.Width(200));

            if (showAllCategories)
            {
                foreach (var pair in currentLogLevelConfig.LogCategoryLevelPairs)
                {
                    int selectedLevelIndex = EditorGUILayout.Popup(pair.Category, Array.IndexOf(levels, pair.Level), levels);
                    pair.Level = LogLevelUtility.LogLevelStringArray[selectedLevelIndex];
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categories);

                var selectedPair = currentLogLevelConfig.LogCategoryLevelPairs[selectedCategoryIndex];
                int selectedLevelIndex = EditorGUILayout.Popup(Array.IndexOf(levels, selectedPair.Level), levels);
                selectedPair.Level = LogLevelUtility.LogLevelStringArray[selectedLevelIndex];           
                EditorGUILayout.EndHorizontal();
            }

            // If Level for All Category is "dirty", then update every category
            var newAllCategoryLogLevel = currentLogLevelConfig.LogCategoryLevelPairs[categories.Length - 1].Level;
            if (newAllCategoryLogLevel != previousAllCategoryLogLevel) 
            {
                foreach (var pair in currentLogLevelConfig.LogCategoryLevelPairs)
                {
                    pair.Level = newAllCategoryLogLevel;
                }
            }

            if (GUILayout.Button("Save All Changes"))
            {
                currentLogLevelConfig.Write(true);
            }
        }
    }
}

#endif
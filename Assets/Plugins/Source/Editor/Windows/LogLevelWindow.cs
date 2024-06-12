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

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

#if !EOS_DISABLE
namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    public class LogLevelWindow : EOSEditorWindow
    {
        LogLevelConfig currentLogLevelConfig;

        string[] categories = LogLevelUtility.LogCategoryStringArray;
        string[] levels = LogLevelUtility.LogLevelStringArray;

        int selectedCategoryIndex = 0;
        bool showAllCategories = false;
        string previousAllCategoryLogLevel;

        [MenuItem("Tools/EOS Plugin/Log Level")]
        public static void ShowWindow()
        {
            GetWindow<LogLevelWindow>("Log Level");
        }
        protected override void Setup()
        {
            currentLogLevelConfig = EpicOnlineServices.Config.Get<LogLevelConfig>();

            // Config.Get<T> only guarantees T to be not null, but not for its members
            if (currentLogLevelConfig.logCategoryLevelPairs == null) 
            {
                currentLogLevelConfig.logCategoryLevelPairs = new List<LogCategoryLevelPair>();
            }

            // Initialize the list if config categories does not match the SDK categories, which might happen when using a different SDK version
            if (currentLogLevelConfig.logCategoryLevelPairs.Count != categories.Length)
            {
                currentLogLevelConfig.logCategoryLevelPairs.Clear();

                foreach (var category in categories)
                {
                    currentLogLevelConfig.logCategoryLevelPairs.Add(new LogCategoryLevelPair(category, "Info"));
                }

                currentLogLevelConfig.Write(true);
            }
        }
        protected override void RenderWindow()
        {
            // Last element in the list is All Categories
            previousAllCategoryLogLevel = currentLogLevelConfig.logCategoryLevelPairs[categories.Length - 1].level;

            showAllCategories = EditorGUILayout.Toggle("Show All Log Settings", showAllCategories);
            EditorGUILayout.LabelField("Category", "Level", GUILayout.Width(200));

            if (showAllCategories)
            {
                foreach (var pair in currentLogLevelConfig.logCategoryLevelPairs)
                {
                    int selectedLevelIndex = EditorGUILayout.Popup(pair.category, Array.IndexOf(levels, pair.level), levels);
                    pair.level = LogLevelUtility.LogLevelStringArray[selectedLevelIndex];
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categories);

                var selectedPair = currentLogLevelConfig.logCategoryLevelPairs[selectedCategoryIndex];
                int selectedLevelIndex = EditorGUILayout.Popup(Array.IndexOf(levels, selectedPair.level), levels);
                selectedPair.level = LogLevelUtility.LogLevelStringArray[selectedLevelIndex];           
                EditorGUILayout.EndHorizontal();
            }

            // If Level for All Category is "dirty", then update every category
            var newAllCategoryLogLevel = currentLogLevelConfig.logCategoryLevelPairs[categories.Length - 1].level;
            if (newAllCategoryLogLevel != previousAllCategoryLogLevel) 
            {
                foreach (var pair in currentLogLevelConfig.logCategoryLevelPairs)
                {
                    pair.level = newAllCategoryLogLevel;
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

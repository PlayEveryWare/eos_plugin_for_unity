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


namespace PlayEveryWare.EpicOnlineServices.Editor
{
    public class LogLevelWindow : EOSEditorWindow
    {
        ConfigHandler<LogLevelConfig> logLevelConfigFile;
        LogLevelConfig currentLogLevelConfig;

        string[] categories = LogLevelHelper.LogCategoryStringArray;
        string[] levels = LogLevelHelper.LogLevelStringArray;

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
            logLevelConfigFile = new ConfigHandler<LogLevelConfig>(Application.streamingAssetsPath + "/EOS/log_level_config.json");
            logLevelConfigFile.Read();

            currentLogLevelConfig = logLevelConfigFile.Data;
            if (currentLogLevelConfig.logCategoryLevelPairs == null) 
            {
                currentLogLevelConfig.logCategoryLevelPairs = new List<LogCategoryLevelPair>();
            }

            if (currentLogLevelConfig.logCategoryLevelPairs.Count != categories.Length)
            {
                currentLogLevelConfig.logCategoryLevelPairs.Clear();

                foreach (var category in categories)
                {
                    currentLogLevelConfig.logCategoryLevelPairs.Add(new LogCategoryLevelPair(category, "Info"));
                }

                SaveToJSONConfig(true);
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
                    pair.level = LogLevelHelper.LogLevelStringArray[selectedLevelIndex];
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categories);

                var selectedPair = logLevelConfigFile.Data.logCategoryLevelPairs[selectedCategoryIndex];
                int selectedLevelIndex = EditorGUILayout.Popup(Array.IndexOf(levels, selectedPair.level), levels);
                selectedPair.level = LogLevelHelper.LogLevelStringArray[selectedLevelIndex];           
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
                SaveToJSONConfig(true);
            }
        }

        private void SaveToJSONConfig(bool prettyPrint)
        {
            logLevelConfigFile.Write(prettyPrint);
        }
    }
}

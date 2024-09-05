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

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using Config;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor.AnimatedValues;
    using Utility;
    using Config = EpicOnlineServices.Config;

    /// <summary>
    /// Creates the view for showing the eos plugin editor config values.
    /// </summary>
    [Serializable]
    public class EOSPluginSettingsWindow : EOSEditorWindow
    {
        private List<IConfigEditor> configEditors;

        public EOSPluginSettingsWindow() : base("EOS Plugin Settings")
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var pluginSettingsWindow = CreateInstance<EOSPluginSettingsWindow>();
            pluginSettingsWindow.SetIsEmbedded(true);
            var provider = new SettingsProvider($"Preferences/{pluginSettingsWindow.WindowTitle}", SettingsScope.User)
            {
                label = pluginSettingsWindow.WindowTitle,
                guiHandler = (searchContext) =>
                {
                    pluginSettingsWindow.OnGUI();
                }
            };

            return provider;
        }

        [MenuItem("Tools/EOS Plugin/Plugin Configuration")]
        public static void ShowWindow()
        {
            var window = GetWindow<EOSPluginSettingsWindow>();
            window.SetIsEmbedded(false);
        }

        public static bool IsAsset(string configFilepath)
        {
            var assetDir = new DirectoryInfo(Application.dataPath);
            var fileDir = new DirectoryInfo(configFilepath);
            bool isParent = false;
            while (fileDir.Parent != null)
            {
                if (fileDir.Parent.FullName == assetDir.FullName)
                {
                    isParent = true;
                    break;
                }

                fileDir = fileDir.Parent;
            }
            return isParent;
        }

        protected override async Task AsyncSetup()
        {
            configEditors ??= new List<IConfigEditor>
                {
                    SetupConfigEditor<PrebuildConfig>(),
                    SetupConfigEditor<ToolsConfig>(),
                    SetupConfigEditor<AndroidBuildConfig>(),
                    SetupConfigEditor<LibraryBuildConfig>(),
                    SetupConfigEditor<SigningConfig>(),
                    SetupConfigEditor<PackagingConfig>(),
                    SetupConfigEditor<SteamConfig>()
                };

            foreach (var editor in configEditors)
            {
                await editor.LoadAsync();
            }
        }

        private IConfigEditor SetupConfigEditor<T>() where T : PlayEveryWare.EpicOnlineServices.Config
        {
            ConfigEditor<T> newEditor = new (Repaint);
            newEditor.Expanded += (sender, args) =>
            {
                // Close all the other config editors
                foreach (var editor in configEditors.Where(editor => editor != sender))
                {
                    editor.Collapse();
                }
            };

            return newEditor;
        }

        protected override void RenderWindow()
        {
            if (configEditors.Count > 0)
            {
                foreach (var configurationSectionEditor in configEditors)
                {
                    _ = configurationSectionEditor.RenderAsync();
                }
            }

            GUI.SetNextControlName("Save");
            if (GUILayout.Button("Save All Changes"))
            {
                GUI.FocusControl("Save");
                Save();
            }
        }

        protected override void Teardown()
        {
            base.Teardown();

            foreach (var editor in configEditors)
                editor.Dispose();

            configEditors.Clear();

            Save();
        }

        private void Save()
        {
            foreach (var configurationSectionEditor in configEditors)
            {
                configurationSectionEditor.Save();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
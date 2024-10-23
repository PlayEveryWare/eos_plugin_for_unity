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

// Uncomment the following line to see the experimental new config window
#define ENABLE_NEW_CONFIG_WINDOW

#if ENABLE_NEW_CONFIG_WINDOW
namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Creates the view for showing the eos plugin editor config values.
    /// </summary>
    [Serializable]
    public class NewConfigWindow : EOSEditorWindow
    {
        /// <summary>
        /// The editor for the product information that is shared across all
        /// platforms (represents information that is common to all
        /// circumstances).
        /// </summary>
        private ConfigEditor<ProductConfig> _productConfigEditor = new();

        /// <summary>
        /// Stores the config editors for each of the platforms.
        /// </summary>
        private readonly IList<IConfigEditor> _platformConfigEditors = new List<IConfigEditor>();

        private GUIContent[] _platformTabs;
        private int _selectedTab = 0;

        public NewConfigWindow() : base("EOS Configuration") { }

        [MenuItem("EOS Plugin/[Experimental] New Config")]
        public static void ShowWindow()
        {
            var window = GetWindow<NewConfigWindow>();
            window.SetIsEmbedded(false);
        }

        protected override async Task AsyncSetup()
        {
            await _productConfigEditor.LoadAsync();

            List<GUIContent> tabContents = new();
            foreach (PlatformManager.Platform platform in Enum.GetValues(typeof(PlatformManager.Platform)))
            {
                if (!PlatformManager.TryGetConfigType(platform, out Type configType) || null == configType)
                {
                    continue;
                }

                Type constructedType =
                    typeof(PlatformConfigEditor<>).MakeGenericType(configType);

                if (Activator.CreateInstance(constructedType) is not IPlatformConfigEditor editor)
                {
                    Debug.LogError($"Could not load config editor for platform \"{platform}\".");
                    continue;
                }

                // Do not add the platform if it is not currently available.
                if (!editor.IsPlatformAvailable())
                {
                    continue;
                }

                _platformConfigEditors.Add(editor);

                tabContents.Add(new GUIContent($" {editor.GetLabelText()}", editor.GetPlatformIconTexture()));
            }

            _platformTabs = tabContents.ToArray();
        }

        protected override void RenderWindow()
        {
            // Render the generic product configuration stuff.
            _ = _productConfigEditor.RenderAsync();

            if (_platformTabs != null && _platformConfigEditors.Count != 0)
            {
                _selectedTab = GUILayout.Toolbar(_selectedTab, _platformTabs);
                GUILayout.Space(10);

                _ = _platformConfigEditors[_selectedTab].RenderAsync();
            }

            GUI.SetNextControlName("Save");
            if (GUILayout.Button("Save All Changes"))
            {
                GUI.FocusControl("Save");
                Save();
            }
        }

        private async void Save()
        {
            await _productConfigEditor.Save();

            foreach (IConfigEditor editor in _platformConfigEditors)
            {
                await editor.Save();
            }
        }
    }
}
#endif
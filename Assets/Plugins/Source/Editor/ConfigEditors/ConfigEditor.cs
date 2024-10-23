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

namespace PlayEveryWare.EpicOnlineServices.Editor
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;
    using UnityEngine.Events;
    using Utility;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all
    /// implementing classes.
    /// </summary>
    /// <typeparam name="T">
    /// The type of config that this editor is responsible for providing an
    /// interface to edit for.
    /// </typeparam>
    public class ConfigEditor<T> : IConfigEditor where T :
        EpicOnlineServices.Config
    {
        /// <summary>
        /// The string to use for the label for the config editor.
        /// </summary>
        protected string _labelText;

        /// <summary>
        /// The labels for each group.
        /// </summary>
        protected string[] _groupLabels;

        /// <summary>
        /// Event that triggers when the config editor is expanded.
        /// </summary>
        public event EventHandler Expanded;

        /// <summary>
        /// Used to animate the expansion and collapse of the config editor if
        /// doing so is enabled.
        /// </summary>
        private AnimBool _animExpanded;

        /// <summary>
        /// Indicates whether the config editor is expandable and collapsible
        /// </summary>
        private bool _collapsible;

        /// <summary>
        /// Stores the state of whether the config editor is expanded or
        /// collapsed.
        /// </summary>
        private bool _expanded;

        /// <summary>
        /// A copy of the config that this config editor edits.
        /// </summary>
        protected T config;

        /// <summary>
        /// Create a new config editor.
        /// </summary>
        /// <param name="repaintFn">
        /// The repaint function, used for ConfigEditors that can be expanded or
        /// collapsed, as animating that requires calling the repaint function
        /// that is typically called from within EditorWindow.
        /// </param>
        /// <param name="startsExpanded">
        /// If expandable, will indicate whether it starts expanded or
        /// collapsed.
        /// </param>
        public ConfigEditor(
            UnityAction repaintFn = null,
            bool startsExpanded = false)
        {
            _expanded = startsExpanded;
            _collapsible = false;

            ConfigGroupAttribute attribute = typeof(T).GetCustomAttribute<ConfigGroupAttribute>();

            if (null != attribute)
            {
                _collapsible = attribute.Collapsible;
                _labelText = attribute.Label;
                _groupLabels = attribute.GroupLabels;
            }

            _animExpanded = new(_collapsible);

            // If it's not expandable, then it starts "expanded"
            if (!_collapsible)
            {
                _expanded = true;
            }

            if (null != repaintFn)
            {
                _animExpanded?.valueChanged.AddListener(repaintFn);
            }
        }

        /// <summary>
        /// Expands the ConfigEditor.
        /// </summary>
        public void Expand()
        {
            // Don't do anything if already expanded, or cannot expand
            if (_expanded || !_collapsible)
            {
                return;
            }

            _expanded = true;
            OnExpanded(EventArgs.Empty);
        }

        /// <summary>
        /// Collapse the ConfigEditor.
        /// </summary>
        public void Collapse()
        {
            // Don't do anything if not expanded, or cannot expand.
            if (!_expanded || !_collapsible)
            {
                return;
            }

            _expanded = false;
        }

        protected virtual void OnExpanded(EventArgs e)
        {
            EventHandler handler = Expanded;
            handler?.Invoke(this, e);
        }

        public string GetLabelText()
        {
            return _labelText;
        }

        public async Task LoadAsync()
        {
            // Don't do anything if the config is already loaded.
            if (config != null)
            {
                return;
            }

            config = await EpicOnlineServices.Config.GetAsync<T>();
        }

        public void Load()
        {
            Task.Run(LoadAsync).GetAwaiter().GetResult();
        }

        public async Task Save(bool prettyPrint = true)
        {
            await config.WriteAsync(prettyPrint);
        }

        public virtual void RenderContents()
        {
            if (_collapsible)
            {
                RenderCollapsibleContents();
            }
            else
            {
                GUILayout.Label(GetLabelText(), EditorStyles.boldLabel);
                GUIEditorUtility.RenderInputs(ref config);
            }
        }

        private void RenderCollapsibleContents()
        {
            GUIStyle foldoutStyle = new(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bool isExpanded = EditorGUILayout.Foldout(_expanded, GetLabelText(), true, foldoutStyle);

            // If the state of expansion has changed, take appropriate action.
            if (_expanded != isExpanded)
            {
                if (isExpanded)
                {
                    Expand();
                }
                else
                {
                    Collapse();
                }
            }

            if (null != _animExpanded)
            {
                _animExpanded.target = isExpanded;
                _expanded = _animExpanded.target;
            }

            if (_expanded)
            {
                if (EditorGUILayout.BeginFadeGroup(_animExpanded.faded))
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUIEditorUtility.RenderInputs(ref config);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndFadeGroup();
            }

            EditorGUILayout.EndVertical();
        }

        public async Task RenderAsync()
        {
            if (config == null)
            {
                await LoadAsync();
            }

            RenderContents();
        }

        public void Dispose()
        {
            _animExpanded?.valueChanged.RemoveAllListeners();
        }
    }
}
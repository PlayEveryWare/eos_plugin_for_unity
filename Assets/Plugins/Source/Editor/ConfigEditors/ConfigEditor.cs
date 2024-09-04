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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;
    using UnityEngine.Events;
    using Utility;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all implementing classes.
    /// </summary>
    /// <typeparam name="T">Intended to be a type accepted by the templated class EOSConfigFile.</typeparam>
    public class ConfigEditor<T> : IConfigEditor where T : EpicOnlineServices.Config
    {
        /// <summary>
        /// Event that triggers when the config editor is expanded.
        /// </summary>
        public event EventHandler Expanded;

        /// <summary>
        /// The string to use for the label for the config editor.
        /// </summary>
        protected string _labelText;

        /// <summary>
        /// Used to animate the expansion and collapse of the config editor if
        /// doing so is enabled.
        /// </summary>
        private AnimBool _animExpanded;

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
        public ConfigEditor(UnityAction repaintFn = null, bool startsExpanded = false)
        {
            _expanded = startsExpanded;

            ConfigGroupAttribute attribute = typeof(T).GetCustomAttribute<ConfigGroupAttribute>();

            if (null != attribute)
            {
                _animExpanded = new(attribute.Collapsible);
                _labelText = attribute.Label;
            }

            if (null != repaintFn)
                _animExpanded?.valueChanged.AddListener(repaintFn);
        }

        /// <summary>
        /// Create a config editor, setting the string to use for the label
        /// explicitly.
        /// </summary>
        /// <param name="labelText">The string to use for the label.</param>
        protected ConfigEditor(string labelText) : this()
        {
            // Overrides the label that might be defined in the
            // ConfigGroupAttribute applied to the config that this editor is
            // providing an interface for.
            _labelText = labelText;
        }

        protected ConfigEditor() { }

        /// <summary>
        /// Expands the ConfigEditor.
        /// </summary>
        public void Expand()
        {
            // Don't do anything if already expanded.
            if (_expanded)
                return;

            _expanded = true;
            OnExpanded(EventArgs.Empty);
        }

        /// <summary>
        /// Collapse the ConfigEditor.
        /// </summary>
        public void Collapse()
        {
            // Don't do anything if not expanded.
            if (!_expanded)
                return;

            _expanded = false;
        }

        protected virtual void OnExpanded(EventArgs e)
        {
            EventHandler handler = Expanded;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Use reflection to retrieve a collection of fields that have been
        /// assigned custom ConfigFieldAttribute attributes, grouping by group,
        /// and sorting by group.
        /// </summary>
        /// <returns>A collection of config fields.</returns>
        private static IOrderedEnumerable<IGrouping<int, (FieldInfo FieldInfo, ConfigFieldAttribute FieldDetails)>> GetFieldsByGroup()
        {
            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<ConfigFieldAttribute>() != null)
                .Select(info => (info, info.GetCustomAttribute<ConfigFieldAttribute>()))
                .GroupBy(r => r.Item2.GroupOrder)
                .OrderBy(group => group.Key == -1 ? int.MaxValue : group.Key);
        }

        /// <summary>
        /// Using a collection of FieldInfo and ConfigFieldAttribute classes,
        /// representing the fields in a Config, determine which field in a
        /// given group is the longest, and return that length.
        /// </summary>
        /// <param name="group">
        /// A group of fields that have ConfigFieldAttribute and the same group
        /// number.
        /// </param>
        /// <returns>
        /// The length of the longest label to create 
        /// </returns>
        private static float GetMaximumLabelWidth(IEnumerable<(FieldInfo, ConfigFieldAttribute)> group)
        {
            GUIStyle labelStyle = new(GUI.skin.label);

            float maxWidth = 0f;
            foreach (var field in group)
            {
                string labelText = field.Item2.Label;

                Vector2 labelSize = labelStyle.CalcSize(new GUIContent(labelText));
                if (maxWidth < labelSize.x)
                {
                    maxWidth = labelSize.x;
                }
            }

            return maxWidth;
        }

        private static string GetGroupLabel(IEnumerable<(FieldInfo, ConfigFieldAttribute)> group)
        {
            string groupLabel = null;
            foreach (var field in group)
            {
                if (groupLabel == null)
                {
                    groupLabel = field.Item2.GroupLabel;
                }
                else
                {
                    if (groupLabel != field.Item2.GroupLabel)
                    {
                        Debug.LogWarning($"ConfigFieldAttribute not " +
                                         $"set properly, fields of the same " +
                                         $"group have different labels. " +
                                         $"Please correct the group labels " +
                                         $"for the group that " +
                                         $"{field.Item1.Name} is assigned to.");
                    }
                }
            }

            return groupLabel;
        }

        /// <summary>
        /// Render the config fields for the config that has been set to edit.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Thrown for types that are not yet implemented.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown for types that are not yet implemented, and not accounted for
        /// in the switch statement.
        /// </exception>
        protected void RenderConfigFields()
        {
            foreach (var fieldGroup in GetFieldsByGroup())
            {
                GUILayout.Label(GetGroupLabel(fieldGroup), EditorStyles.boldLabel);
                float labelWidth = GetMaximumLabelWidth(fieldGroup);

                foreach (var field in fieldGroup)
                {
                    switch (field.FieldDetails.FieldType)
                    {
                        case ConfigFieldType.Text:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails, (string)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.FilePath:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails as FilePathField, (string)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.Flag:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails, (bool)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.DirectoryPath:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails as DirectoryPathField, (string)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.Ulong:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails, (ulong)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.TextList:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails, (List<string>)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.Uint:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails, (uint)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.Double:
                            field.FieldInfo.SetValue(config, GUIEditorUtility.RenderInputField(field.FieldDetails, (double)field.FieldInfo.GetValue(config), labelWidth));
                            break;
                        case ConfigFieldType.Button:
                            if (GUILayout.Button(field.FieldDetails.Label) && 
                                field.FieldInfo.GetValue(config) is Action onClick)
                            {
                                onClick();
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public string GetLabelText()
        {
            return _labelText;
        }

        public virtual async Task LoadAsync()
        {   
            config = await EpicOnlineServices.Config.GetAsync<T>();
        }

        public async Task Save(bool prettyPrint)
        {
            await config.WriteAsync(prettyPrint);
        }

        public virtual void RenderContents()
        {
            GUIStyle foldoutStyle = new(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            bool isExpanded = EditorGUILayout.Foldout(_expanded, GetLabelText(), true, foldoutStyle);

            // If the state of expansion has changed
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

            _animExpanded.target = isExpanded;
            _expanded = _animExpanded.target;

            if (_expanded)
            {
                if (EditorGUILayout.BeginFadeGroup(_animExpanded.faded))
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    RenderConfigFields();
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
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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utility;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all implementing classes.
    /// </summary>
    /// <typeparam name="T">Intended to be a type accepted by the templated class EOSConfigFile.</typeparam>
    public class ConfigEditor<T> : IConfigEditor where T : EpicOnlineServices.Config
    {
        private readonly string _labelText;
        protected T config;

        public ConfigEditor()
        {
            Type configType = typeof(T);

            ConfigGroupAttribute attribute = configType.GetCustomAttribute<ConfigGroupAttribute>();

            if (null != attribute)
            {
                _labelText = attribute.Label;
            }
        }

        protected ConfigEditor(string labelText)
        {
            _labelText = labelText;
        }

        private static IOrderedEnumerable<IGrouping<int, (FieldInfo FieldInfo, ConfigFieldAttribute FieldDetails)>> GetFieldsByGroup()
        {
            var returnValue = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<ConfigFieldAttribute>() != null)
                .Select(info => (info, info.GetCustomAttribute<ConfigFieldAttribute>()))
                .GroupBy(r => r.Item2.Group)
                .OrderBy(group => group.Key);

            return returnValue;
        }

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

        protected void RenderConfigFields()
        {
            var fieldGroups = GetFieldsByGroup();
            foreach (var fieldGroup in fieldGroups)
            {
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
                        case ConfigFieldType.Uint:
                            throw new NotImplementedException();
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

        public async Task LoadAsync()
        {

            config = await EpicOnlineServices.Config.GetAsync<T>();
        }

        public async Task Save(bool prettyPrint)
        {
            await config.WriteAsync(prettyPrint);
        }

        public virtual void RenderContents()
        {
            GUILayout.Label(GetLabelText(), EditorStyles.boldLabel);
            GUIEditorUtility.HorizontalLine(Color.white);
            RenderConfigFields();
            EditorGUILayout.Space();
        }

        public async Task RenderAsync()
        {
            if (config == null)
            {
                await LoadAsync();
            }

            RenderContents();
        }

    }
}
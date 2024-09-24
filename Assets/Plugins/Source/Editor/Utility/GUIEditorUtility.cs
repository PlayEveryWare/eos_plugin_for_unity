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

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    using EpicOnlineServices.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class GUIEditorUtility
    {
        private const float MaximumButtonWidth = 100f;

        private static GUIContent CreateGUIContent(string label, string tooltip = null)
        {
            label ??= "";
            return tooltip == null ? new GUIContent(label) : new GUIContent(label, tooltip);
        }

        /// <summary>
        /// Render a foldout.
        /// </summary>
        /// <param name="isOpen">The state of the foldout.</param>
        /// <param name="hideLabel">Text to display when the foldout is shown.</param>
        /// <param name="showLabel">Text to display when the foldout is closed.</param>
        /// <param name="renderContents">Function to call when foldout is open.</param>
        public static void RenderFoldout(ref bool isOpen, string hideLabel, string showLabel, Action renderContents)
        {
            isOpen = EditorGUILayout.Foldout(isOpen, (isOpen) ? hideLabel : showLabel);

            if (!isOpen)
            {
                return;
            }

            // This simulates the foldout being indented
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            renderContents();
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.EndVertical();
        }

        public static void AssigningFlagTextField(string label, ref List<string> flags, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var collectedEOSplatformFlags = String.Join("|", flags ?? new List<string>());
            var platformFlags = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), collectedEOSplatformFlags);
            flags = new List<string>(platformFlags.Split('|'));

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningTextField(string label, ref string value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value ?? "", GUILayout.ExpandWidth(true));
            if (newValue != null)
            {
                value = newValue;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningULongToStringField(string label, ref string value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            try
            {
                EditorGUILayout.BeginHorizontal();
                var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value == null ? "" : value, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.MaxWidth(50)))
                {
                    value = null;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(newValueAsString))
                    {
                        value = null;
                        return;
                    }

                    var valueAsLong = ulong.Parse(newValueAsString);
                    value = valueAsLong.ToString();
                }
            }
            catch (FormatException)
            {
                value = null;
            }
            catch (OverflowException)
            {
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
        
        public static void AssigningBoolField(string label, ref bool value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue = EditorGUILayout.Toggle(CreateGUIContent(label, tooltip), value, GUILayout.ExpandWidth(true));
            value = newValue;

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningFloatToStringField(string label, ref string value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            try
            {
                EditorGUILayout.BeginHorizontal();
                var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value == null ? "" : value, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.MaxWidth(50)))
                {
                    value = null;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(newValueAsString))
                    {
                        value = null;
                        return;
                    }

                    var valueAsFloat = float.Parse(newValueAsString);
                    value = valueAsFloat.ToString();
                }
            }
            catch (FormatException)
            {
                value = null;
            }
            catch (OverflowException)
            {
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningEnumField<T>(string label, ref T value, float labelWidth = -1, string tooltip = null) where T : Enum
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue = (T)EditorGUILayout.EnumFlagsField(CreateGUIContent(label, tooltip), value, GUILayout.ExpandWidth(true));
            value = newValue;

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        #region New methods for rendering input fields

        private delegate T InputRenderDelegate<T>(string label, T value, float labelWidth, string tooltip);

        public static List<string> RenderInputField(ConfigFieldAttribute configFieldDetails, List<string> value,
            float labelWidth, string tooltip = null)
        {
            float currentLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            // Because the list is beneath the label, add a colon if it does
            // not already have one.
            string listLabel = configFieldDetails.Label.EndsWith(":")
                ? configFieldDetails.Label
                : configFieldDetails.Label + ":";

            List<string> newValue = new(value);

            EditorGUIUtility.labelWidth = currentLabelWidth;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(CreateGUIContent(listLabel, configFieldDetails.ToolTip));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.MaxWidth(MaximumButtonWidth)))
            {
                newValue.Add(string.Empty);
            }
            EditorGUILayout.EndHorizontal();
            
            for (var i = 0; i < newValue.Count; ++i)
            {
                bool itemRemoved = false;

                EditorGUILayout.BeginHorizontal();

                newValue[i] = EditorGUILayout.TextField(newValue[i], GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Remove", GUILayout.MaxWidth(MaximumButtonWidth)))
                {
                    newValue.RemoveAt(i);
                    itemRemoved = true;
                }

                EditorGUILayout.EndHorizontal();

                if (itemRemoved)
                    break;
            }

            return newValue;
        }

        public static string RenderInputField(DirectoryPathFieldAttribute configFieldAttributeDetails, string value, float labelWidth,
            string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();

            string filePath = InputRendererWrapper<string>(configFieldAttributeDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip) =>
                {
                    return EditorGUILayout.TextField(CreateGUIContent(configFieldAttributeDetails.Label, tooltip), value,
                        GUILayout.ExpandWidth(true));
                });

            if (GUILayout.Button("Select", GUILayout.MaxWidth(MaximumButtonWidth)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel(configFieldAttributeDetails.Label, "", "");

                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    filePath = selectedPath;
                }
            }

            EditorGUILayout.EndHorizontal();

            return filePath;
        }

        public static string RenderInputField(FilePathFieldAttribute configFieldAttributeDetails, string value, float labelWidth, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();

            string filePath = InputRendererWrapper<string>(configFieldAttributeDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip) =>
                {
                    return EditorGUILayout.TextField(CreateGUIContent(configFieldAttributeDetails.Label, tooltip), value,
                        GUILayout.ExpandWidth(true));
                });

            if (GUILayout.Button("Select", GUILayout.MaxWidth(MaximumButtonWidth)))
            {
                string selectedPath =
                    EditorUtility.OpenFilePanel(configFieldAttributeDetails.Label, "", configFieldAttributeDetails.Extension);

                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    filePath = selectedPath;
                }
            }

            EditorGUILayout.EndHorizontal();

            return filePath;
        }

        public static double RenderInputField(ConfigFieldAttribute configFieldDetails, double value, float labelWidth, string tooltip = null)
        {
            return InputRendererWrapper(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, value1, width, s) =>
                {
                    return EditorGUILayout.DoubleField(
                        CreateGUIContent(configFieldDetails.Label, tooltip),
                        value,
                        GUILayout.ExpandWidth(true));
                });
        }

        public static string RenderInputField(ConfigFieldAttribute configFieldDetails, string value, float labelWidth,
            string tooltip = null)
        {
            return InputRendererWrapper<string>(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip1) =>
                {
                    return EditorGUILayout.TextField(CreateGUIContent(configFieldDetails.Label, tooltip), value,
                        GUILayout.ExpandWidth(true));
                });
        }

        public static ulong RenderInputField(ConfigFieldAttribute configFieldDetails, ulong value, float labelWidth,
            string tooltip = null)
        {
            return InputRendererWrapper(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, value1, width, s) =>
                {
                    _ = SafeTranslatorUtility.TryConvert(value, out long temp);

                    long longValue = EditorGUILayout.LongField(
                            CreateGUIContent(configFieldDetails.Label, tooltip),
                            temp,
                            GUILayout.ExpandWidth(true));

                    _ = SafeTranslatorUtility.TryConvert(longValue, out ulong newValue);

                    return newValue;
                });
        }

        public static uint RenderInputField(ConfigFieldAttribute configFieldDetails, uint value, float labelWidth,
            string tooltip = null)
        {
            return InputRendererWrapper(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, value1, width, s) =>
                {
                    _ = SafeTranslatorUtility.TryConvert(value1, out int temp);
                    
                    int intValue = EditorGUILayout.IntField(
                        CreateGUIContent(configFieldDetails.Label, tooltip),
                        temp,
                        GUILayout.ExpandWidth(true));

                    _ = SafeTranslatorUtility.TryConvert(intValue, out uint newValue);

                    return newValue;
                });
        }

        public static bool RenderInputField(ConfigFieldAttribute configFieldDetails, bool value, float labelWidth, string tooltip = null)
        {
            return InputRendererWrapper<bool>(configFieldDetails.Label, value, labelWidth, tooltip, (s, b, arg3, arg4) =>
            {
                return EditorGUILayout.Toggle(CreateGUIContent(configFieldDetails.Label, tooltip), value, GUILayout.ExpandWidth(true));
            });
        }

        private static T InputRendererWrapper<T>(string label, T value, float labelWidth, string toolTip, InputRenderDelegate<T> renderFn)
        {
            // Store the current label width so that it can be subsequently be restored.
            float currentLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            T newValue = renderFn(label, value, labelWidth, toolTip);

            EditorGUIUtility.labelWidth = currentLabelWidth;

            return newValue;
        }

        #endregion
    }
}
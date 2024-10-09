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
    using Common;
    using EpicOnlineServices.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;

    public static class GUIEditorUtility
    {
        /// <summary>
        /// Maximum width allowed for a button.
        /// </summary>
        private const float MAXIMUM_BUTTON_WIDTH = 100f;

        /// <summary>
        /// Style utilized for hint label overlays.
        /// </summary>
        private static readonly GUIStyle HINT_STYLE = new GUIStyle(GUI.skin.label)
        {
            normal = new GUIStyleState() { textColor = Color.gray }, fontStyle = FontStyle.Italic
        };

        private const int HINT_RECT_ADJUST_X = 2;
        private const int HINT_RECT_ADJUST_Y = 1;

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

        public static void AssigningULongToStringField(string label, ref ulong? value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            EditorGUILayout.BeginHorizontal();

            var guiLabel = CreateGUIContent(label, tooltip);
            string textToDisplay = string.Empty;
            
            if (value.HasValue)
            {
                textToDisplay = value.Value.ToString();
            }
            
            string newTextValue = EditorGUILayout.TextField(guiLabel, textToDisplay, GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(50)))
            {
                value = null;
            }
            else
            {
                if (string.IsNullOrEmpty(newTextValue))
                {
                    value = null;
                } 
                else if (ulong.TryParse(newTextValue, out ulong newLongValue))
                {
                    value = newLongValue;
                }
            }

            EditorGUILayout.EndHorizontal();

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

        public static void AssigningFloatToStringField(string label, ref float? value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            EditorGUILayout.BeginHorizontal();
            var guiLabel = CreateGUIContent(label, tooltip);
            string textToDisplay = string.Empty;

            if (value.HasValue)
            {
                textToDisplay = value.Value.ToString(CultureInfo.InvariantCulture);
            }

            string newTextValue = EditorGUILayout.TextField(guiLabel, textToDisplay, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Clear", GUILayout.MaxWidth(50)))
            {
                value = null;
            }
            else
            {
                if (string.IsNullOrEmpty(newTextValue))
                {
                    value = null;
                }
                else if (float.TryParse(newTextValue, out float newFloatValue))
                {
                    value = newFloatValue;
                }
            }

            GUILayout.EndHorizontal();
        

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

        /// <summary>
        /// Used to describe the function used to render a field of type T.
        /// </summary>
        /// <typeparam name="T">
        /// The type being rendered.
        /// </typeparam>
        /// <param name="value">
        /// The current value to display in the field.
        /// </param>
        /// <param name="options">
        /// Optional parameters that describe the styling of the field to
        /// render.
        /// </param>
        /// <returns>
        /// The value entered.
        /// </returns>
        private delegate T RenderFieldDelegate<T>(T value, params GUILayoutOption[] options);

        /// <summary>
        /// Used to render a field with an overlay hint label when the field has
        /// a value that is considered to be "default".
        /// </summary>
        /// <typeparam name="T">
        /// The type of input the field is expecting and returns.
        /// </typeparam>
        /// <param name="renderFieldFn">
        /// The function used to render the input field.
        /// </param>
        /// <param name="isDefaultFn">
        /// The function used to determine if the value is default or not.
        /// </param>
        /// <param name="value">
        /// The current value to display in the input field.
        /// </param>
        /// <param name="hintText">
        /// The hint text to display over the input field if the value is
        /// considered to be default.
        /// </param>
        /// <returns>
        /// The value entered into the input field.
        /// </returns>
        private static T RenderHintableField<T>(
            RenderFieldDelegate<T> renderFieldFn,
            Func<T, bool> isDefaultFn,
            T value,
            string hintText)
        {
            // Generate a unique control name
            string controlName = Guid.NewGuid().ToString();

            // Set the name of the next field
            GUI.SetNextControlName(controlName);

            var lastId = GUIUtility.GetControlID(FocusType.Passive);

            // Render the field
            T newValue = renderFieldFn(value, GUILayout.ExpandWidth(true));

            // Check if the field is default, and that the control is not
            // focused
            if (isDefaultFn(newValue) && GUI.GetNameOfFocusedControl() != controlName)
            {
                RenderHint(hintText);
            }

            return newValue;
        }

        private static string RenderHintableField(string value, string hintText)
        {
            return RenderHintableField(EditorGUILayout.TextField, string.IsNullOrEmpty, value, hintText);
        }

        /// <summary>
        /// Used to render a label on top of the previously rendered control.
        /// </summary>
        /// <param name="hintText">
        /// The text to display over the last rendered input field.
        /// </param>
        private static void RenderHint(string hintText)
        {
            // Get the rectangle of the last control rendered
            Rect fieldRect = GUILayoutUtility.GetLastRect();
            fieldRect.x += HINT_RECT_ADJUST_X;
            fieldRect.y += HINT_RECT_ADJUST_Y;

            EditorGUI.LabelField(fieldRect, hintText, HINT_STYLE);
        }

        public static SortedSetOfNamed<SandboxId> RenderInput(ConfigFieldAttribute configFieldDetails,
            SortedSetOfNamed<SandboxId> value, float labelWidth, string tooltip = null)
        {
            EditorGUILayout.LabelField(CreateGUIContent(configFieldDetails.Label, tooltip));

            // Used to store a list of sandbox ids that need to be removed.
            List<Named<SandboxId>> toRemove = new();

            foreach (Named<SandboxId> sandboxId in value)
            {
                GUILayout.BeginHorizontal();

                sandboxId.Name = RenderHintableField(sandboxId.Name, "Sandbox Name");
                sandboxId.Value.Value = RenderHintableField(sandboxId.Value.Value, "Sandbox Id");

                if (GUILayout.Button("Delete"))
                {
                    toRemove.Add(sandboxId);
                }
                GUILayout.EndHorizontal();
            }

            foreach (Named<SandboxId> sandboxId in toRemove)
            {
                value.Remove(sandboxId);
            }

            if (GUILayout.Button("Add"))
            {
                if (false == value.Add())
                {
                    // TODO: Tell user that a new value could not be added.
                }
            }

            return value;
        }

        private static Guid GuidField(Guid value, params GUILayoutOption[] options)
        {
            string tempStringName = EditorGUILayout.TextField(value.ToString(), options);

            return Guid.TryParse(tempStringName, out Guid newValue) ? newValue : value;
        }

        public static Named<Guid> RenderInput(ConfigFieldAttribute configFieldDetails, Named<Guid> value, float labelWidth,
            string tooltip = null)
        {
            var newValue = InputRendererWrapper<Named<Guid>>(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip1) =>
                {
                    EditorGUILayout.LabelField(CreateGUIContent(configFieldDetails.Label, tooltip));

                    GUILayout.BeginHorizontal();

                    if (null == value)
                        value = new Named<Guid>();

                    value.Name = RenderHintableField(EditorGUILayout.TextField, string.IsNullOrEmpty, value.Name,
                        "Product Name");

                    value.Value = GuidField(value.Value, GUILayout.ExpandWidth(true));

                    GUILayout.EndHorizontal();

                    return value;
                });

            return newValue;
        }

        public static List<string> RenderInput(ConfigFieldAttribute configFieldDetails, List<string> value,
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
            if (GUILayout.Button("Add", GUILayout.MaxWidth(MAXIMUM_BUTTON_WIDTH)))
            {
                newValue.Add(string.Empty);
            }
            EditorGUILayout.EndHorizontal();
            
            for (var i = 0; i < newValue.Count; ++i)
            {
                bool itemRemoved = false;

                EditorGUILayout.BeginHorizontal();

                newValue[i] = EditorGUILayout.TextField(newValue[i], GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Remove", GUILayout.MaxWidth(MAXIMUM_BUTTON_WIDTH)))
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

        public static string RenderInput(DirectoryPathFieldAttribute configFieldAttributeDetails, string value, float labelWidth,
            string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();

            string filePath = InputRendererWrapper<string>(configFieldAttributeDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip) =>
                {
                    return EditorGUILayout.TextField(CreateGUIContent(configFieldAttributeDetails.Label, tooltip), value,
                        GUILayout.ExpandWidth(true));
                });

            if (GUILayout.Button("Select", GUILayout.MaxWidth(MAXIMUM_BUTTON_WIDTH)))
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

        public static string RenderInput(FilePathFieldAttribute configFieldAttributeDetails, string value, float labelWidth, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();

            string filePath = InputRendererWrapper<string>(configFieldAttributeDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip) =>
                {
                    return EditorGUILayout.TextField(CreateGUIContent(configFieldAttributeDetails.Label, tooltip), value,
                        GUILayout.ExpandWidth(true));
                });

            if (GUILayout.Button("Select", GUILayout.MaxWidth(MAXIMUM_BUTTON_WIDTH)))
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

        public static double RenderInput(ConfigFieldAttribute configFieldDetails, double value, float labelWidth, string tooltip = null)
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

        public static string RenderInput(ConfigFieldAttribute configFieldDetails, string value, float labelWidth,
            string tooltip = null)
        {
            return InputRendererWrapper<string>(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip1) =>
                {
                    return EditorGUILayout.TextField(CreateGUIContent(configFieldDetails.Label, tooltip), value,
                        GUILayout.ExpandWidth(true));
                });
        }

        public static ulong RenderInput(ConfigFieldAttribute configFieldDetails, ulong value, float labelWidth,
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

        public static uint RenderInput(ConfigFieldAttribute configFieldDetails, uint value, float labelWidth,
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

        public static bool RenderInput(ConfigFieldAttribute configFieldDetails, bool value, float labelWidth, string tooltip = null)
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
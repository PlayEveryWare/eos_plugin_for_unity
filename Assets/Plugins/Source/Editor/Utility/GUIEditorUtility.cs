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
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using UnityEditor;
    using UnityEngine;

    public static class GUIEditorUtility
    {
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

        public static void AssigningPath(string label, ref string filePath, string prompt, string directory = "", string extension = "", bool selectFolder = false, bool horizontalLayout = true, float maxButtonWidth = 100, float labelWidth = -1, string tooltip = null)
        {
            if (horizontalLayout)
            {
                EditorGUILayout.BeginHorizontal();
            }

            AssigningTextField(label, ref filePath, labelWidth, tooltip);

            bool buttonPressed = maxButtonWidth > 0 ? GUILayout.Button("Select", GUILayout.MaxWidth(maxButtonWidth)) : GUILayout.Button("Select");

            if (buttonPressed)
            {
                var newFilePath = selectFolder ? EditorUtility.OpenFolderPanel(prompt, "", "") : EditorUtility.OpenFilePanel(prompt, directory, extension);
                if (!string.IsNullOrWhiteSpace(newFilePath))
                {
                    filePath = newFilePath;
                }
            }

            if (horizontalLayout)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        public static void AssigningULongField(string label, ref ulong value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            ulong newValue = value;
            var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value.ToString(), GUILayout.ExpandWidth(true));
            if (string.IsNullOrWhiteSpace(newValueAsString))
            {
                newValueAsString = "0";
            }

            try
            {
                newValue = ulong.Parse(newValueAsString);
                value = newValue;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningUintField(string label, ref uint value, float labelWidth = -1, string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            uint newValue = value;
            var newValueAsString = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value.ToString(), GUILayout.ExpandWidth(true));
            if (string.IsNullOrWhiteSpace(newValueAsString))
            {
                newValueAsString = "0";
            }

            try
            {
                newValue = uint.Parse(newValueAsString);
                value = newValue;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
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

        public static void HorizontalLine(Color color)
        {
            var defaultHorizontalLineStyle = new GUIStyle();
            defaultHorizontalLineStyle.normal.background = EditorGUIUtility.whiteTexture;
            defaultHorizontalLineStyle.margin = new RectOffset(0, 0, 4, 4);
            defaultHorizontalLineStyle.fixedHeight = 1;
            HorizontalLine(color, defaultHorizontalLineStyle);
        }

        public static void HorizontalLine(Color color, GUIStyle guiStyle)
        {
            var currentColor = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, guiStyle);
            GUI.color = currentColor;
        }

    }
}
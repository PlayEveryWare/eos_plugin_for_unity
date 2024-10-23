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
    using System.Linq;
    using UnityEditor;
    using UnityEditorInternal;
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
        private static readonly GUIStyle HINT_STYLE = new(GUI.skin.label)
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

                // Remove focus from the control so it doesn't display "phantom"
                // values
                GUI.FocusControl(null);
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

                // Remove focus from the control so it doesn't display "phantom"
                // values
                GUI.FocusControl(null);
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
        /// Used to describe the function used to render a basic field of type
        /// T.
        /// </summary>
        /// <typeparam name="T">
        /// The type of input being rendered.
        /// </typeparam>
        /// <param name="rect">
        /// The area in which the field should be rendered.
        /// </param>
        /// <param name="value">The value to put in the field.</param>
        /// <returns>The value entered in the field.</returns>
        private delegate T RenderBasicFieldDelegate<T>(Rect rect, T value);


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
        /// <param name="rect">
        /// The area in which to render the field.
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
        private static T RenderFieldWithHint<T>(
            RenderBasicFieldDelegate<T> renderFieldFn,
            Rect rect,
            Func<T, bool> isDefaultFn,
            T value,
            string hintText)
        {
            string controlName = Guid.NewGuid().ToString();

            GUI.SetNextControlName(controlName);

            T newValue = renderFieldFn(rect, value);

            if (isDefaultFn(newValue) && GUI.GetNameOfFocusedControl() != controlName)
            {
                RenderHint(hintText, rect);
            }

            return newValue;
        }

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
        private static T RenderFieldWithHint<T>(
            RenderFieldDelegate<T> renderFieldFn,
            Func<T, bool> isDefaultFn,
            T value,
            string hintText)
        {
            // Generate a unique control name
            string controlName = Guid.NewGuid().ToString();

            // Set the name of the next field
            GUI.SetNextControlName(controlName);

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

        /// <summary>
        /// Used to render a label at the specified rect.
        /// </summary>
        /// <param name="hintText">
        /// The text to display as a hint for input.
        /// </param>
        /// <param name="rect">
        /// The location in which to draw the hint.
        /// </param>
        private static void RenderHint(string hintText, Rect rect)
        {
            EditorGUI.LabelField(rect, hintText, HINT_STYLE);
        }

        /// <summary>
        /// Renders the built-in help icon, and opens a browser to the indicated
        /// url when clicked on.
        /// </summary>
        /// <param name="area">The area in which to render the icon.</param>
        /// <param name="url">The url to open when the icon is clicked on.</param>
        private static void RenderHelpIcon(Rect area, string url)
        {
            GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");

            Color hoverColor = !EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.8f, 0.8f, 0.8f);

            Texture2D hoverTexture = new(1, 1);
            hoverTexture.SetPixel(0, 0, hoverColor);
            hoverTexture.Apply();

            GUIStyle helpButtonStyle = new(EditorStyles.label)
            {
                padding = new RectOffset(0, 0, 0, 0),
                fixedWidth = 20,
                fixedHeight = 20,
                normal = { background = Texture2D.redTexture }, // No background by default
                hover = { background = Texture2D.redTexture }, // Gray back
            };

            if (GUI.Button(area, helpIcon, helpButtonStyle))
            {
                Application.OpenURL(url);
            }
        }

        private static void RenderSetOfNamed<T>(
            string label,
            string tooltip,
            string helpUrl,
            SetOfNamed<T> value,
            Action<Rect, Named<T>> renderItemFn,
            Action addNewItemFn,
            Action<Named<T>> removeItemFn
        ) where T : IEquatable<T>
        {
            List<Named<T>> items = value.ToList();

            ReorderableList list = new(items, typeof(Named<T>))
            {
                draggable = false,
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(new(rect.x, rect.y, rect.width - 20f, rect.height), 
                        CreateGUIContent(label, tooltip));
                    if (!string.IsNullOrEmpty(helpUrl))
                    {
                        RenderHelpIcon(new(rect.x + rect.width - 20f, rect.y, 20f, rect.height), helpUrl);
                    }
                },

                onAddCallback = (_) => addNewItemFn(),

                drawElementCallback = (rect, index, _, _) =>
                {
                    rect.y += 2f;
                    rect.height = EditorGUIUtility.singleLineHeight;

                    renderItemFn(rect, items[index]);
                }
            };

            list.onRemoveCallback = (list) =>
            {
                if (list.index < 0 || list.index >= items.Count)
                {
                    return;
                }

                removeItemFn(items[list.index]);
            };

            list.DoLayoutList();
        }

        private static void RenderDeploymentInputs(ref ProductionEnvironments value)
        {
            ProductionEnvironments productionEnvironmentsCopy = value;

            RenderSetOfNamed(
                "Deployments",
                "Enter your deployments here as they appear in the Epic Dev Portal.",
                "https://dev.epicgames.com/docs/dev-portal/product-management#deployments",
                productionEnvironmentsCopy.Deployments,
                (rect, item) =>
                {
                    float firstFieldWidth = (rect.width - 5f) * 0.25f;
                    float middleFieldWidth = (rect.width - 5f) * 0.50f;
                    float endFieldWidth = (rect.width - 5f) * 0.25f;

                    item.Name = RenderFieldWithHint(
                        EditorGUI.TextField,
                        new Rect(rect.x, rect.y, firstFieldWidth, rect.height),
                        string.IsNullOrEmpty,
                        item.Name,
                        "Sandbox Name");

                    item.Value.DeploymentId = GuidField(
                        new Rect(rect.x + firstFieldWidth + 5f, rect.y, middleFieldWidth, rect.height),
                        item.Value.DeploymentId);

                    List<string> sandboxLabelList = new();
                    int labelIndex = 0;
                    int selectedIndex = 0;
                    foreach (Named<SandboxId> sandbox in productionEnvironmentsCopy.Sandboxes)
                    {
                        sandboxLabelList.Add(sandbox.Name);
                        if (sandbox.Value.Equals(item.Value.SandboxId))
                        {
                            selectedIndex = labelIndex;
                        }

                        labelIndex++;
                    }

                    int newSelectedIndex = EditorGUI.Popup(new Rect(rect.x + firstFieldWidth + 5f + middleFieldWidth + 5f, rect.y, endFieldWidth, rect.height),
                        selectedIndex, sandboxLabelList.ToArray());
                    string newSelectedSandboxLabel = sandboxLabelList[newSelectedIndex];
                    foreach (Named<SandboxId> sandbox in productionEnvironmentsCopy.Sandboxes)
                    {
                        if (newSelectedSandboxLabel != sandbox.Name)
                        {
                            continue;
                        }

                        item.Value.SandboxId = sandbox.Value;
                        break;
                    }
                },
                () => productionEnvironmentsCopy.AddNewDeployment(),
                (item) =>
                {
                    if (!productionEnvironmentsCopy.RemoveDeployment(item))
                    {
                        // TODO: Tell user why deployment could not be removed
                        //       from the Production Environments.
                    }
                });
        }

        private static void RenderSandboxInputs(ref ProductionEnvironments value)
        {
            ProductionEnvironments productionEnvironmentsCopy = value;

            GUILayout.Label("Enter one or more of the Sandbox Ids for your " +
                            "game from the Epic Dev Portal below.");

            RenderSetOfNamed(
                "Sandboxes",
                "Enter your sandboxes here, as they appear in the Epic Dev Portal.",
                "https://dev.epicgames.com/docs/dev-portal/product-management#sandboxes",
                productionEnvironmentsCopy.Sandboxes,
                (rect, item) =>
                {
                    float fieldWidth = (rect.width - 5f) / 2f;

                    item.Name = RenderFieldWithHint(
                        EditorGUI.TextField,
                        new Rect(rect.x, rect.y, fieldWidth, rect.height),
                        string.IsNullOrEmpty,
                        item.Name,
                        "Sandbox Name");

                    item.Value.Value = RenderFieldWithHint(
                        EditorGUI.TextField,
                        new Rect(rect.x + fieldWidth + 5f, rect.y, fieldWidth, rect.height),
                        string.IsNullOrEmpty,
                        item.Value.Value,
                        "Sandbox Id");
                },
                () => productionEnvironmentsCopy.AddNewSandbox(),
                (item) =>
                {
                    if (!productionEnvironmentsCopy.RemoveSandbox(item))
                    {
                        // TODO: Tell user why the sandbox could not be removed.
                    }
                }
            );
        }

        private static Guid GuidField(Guid value, params GUILayoutOption[] options)
        {
            string tempStringName = EditorGUILayout.TextField(value.ToString(), options);

            return Guid.TryParse(tempStringName, out Guid newValue) ? newValue : value;
        }

        private static Guid GuidField(Rect rect, Guid value)
        {
            string tempStringName = EditorGUI.TextField(rect, value.ToString());
            return Guid.TryParse(tempStringName, out Guid newValue) ? newValue : value;
        }

        private static Version VersionField(Version value, params GUILayoutOption[] options)
        {
            string tempStringVersion = EditorGUILayout.TextField(value.ToString(), options);
            return Version.TryParse(tempStringVersion, out Version newValue) ? newValue : value;
        }

        public static Version RenderInput(ConfigFieldAttribute configFieldAttribute, Version value, float labelWidth,
            string tooltip = null)
        {
            return InputRendererWrapper(configFieldAttribute.Label, value, labelWidth, tooltip, ((label, version, width, s) =>
            {
                return RenderFieldWithHint(VersionField, (v) => v == null, value, "Version");
            }));
        }

        public static SetOfNamed<EOSClientCredentials> RenderInput(ConfigFieldAttribute configFieldAttribute,
            SetOfNamed<EOSClientCredentials> value, float labelWidth, string tooltip = null)
        {
            EditorGUILayout.Space();

            RenderSetOfNamed(
                "Clients",
                "Enter your client information here as it appears in the Epic Dev Portal.",
                "https://dev.epicgames.com/docs/dev-portal/product-management#clients",
                value,
                (rect, item) =>
                {
                    float firstFieldWidth = (rect.width - 5f) * 0.18f;
                    float middleFieldWidth = (rect.width - 5f) * 0.34f;
                    float endFieldWidth = (rect.width - 5f) * 0.48f;

                    item.Name = RenderFieldWithHint(
                        EditorGUI.TextField,
                        new Rect(rect.x, rect.y, firstFieldWidth, rect.height),
                        string.IsNullOrEmpty,
                        item.Name,
                        "Client Name");

                    item.Value ??= new();

                    item.Value.ClientId = RenderFieldWithHint(
                        EditorGUI.TextField,
                        new Rect(rect.x + firstFieldWidth + 5f, rect.y, middleFieldWidth, rect.height),
                        string.IsNullOrEmpty,
                        item.Value.ClientId,
                        "Client ID"
                    );

                    item.Value.ClientSecret = RenderFieldWithHint(
                        EditorGUI.TextField,
                        new Rect(rect.x + firstFieldWidth + 5f + middleFieldWidth + 5f, rect.y, endFieldWidth, rect.height),
                        string.IsNullOrEmpty,
                        item.Value.ClientSecret,
                        "Client Secret");

                },
                () => value.Add(),
                (item) =>
                {
                    if (value.Remove(item))
                    {
                        // TODO: Tell user that credentials could not be removed.
                    }
                });

            return value;
        }

        public static ProductionEnvironments RenderInput(ConfigFieldAttribute configFieldAttribute,
            ProductionEnvironments value, float labelWidth, string tooltip = null)
        {
            value ??= new();

            EditorGUILayout.Space();

            // Render the list of sandboxes
            RenderSandboxInputs(ref value);

            EditorGUILayout.Space();

            // Check to see if there are any sandboxes - if there aren't any
            // then you cannot add a deployment.
            if (value.Sandboxes.Count == 0)
            {
                GUI.enabled = false;
            }

            // Render the list of deployments
            RenderDeploymentInputs(ref value);

            // Ensure that the GUI is always enabled before leaving scope
            GUI.enabled = true;

            return value;
        }

        public static Named<Guid> RenderInput(ConfigFieldAttribute configFieldDetails, Named<Guid> value, float labelWidth,
            string tooltip = null)
        {
            var newValue = InputRendererWrapper<Named<Guid>>(configFieldDetails.Label, value, labelWidth, tooltip,
                (label, s, width, tooltip1) =>
                {
                    EditorGUILayout.LabelField(CreateGUIContent(configFieldDetails.Label, tooltip));

                    GUILayout.BeginHorizontal();

                    value ??= new Named<Guid>();

                    value.Name = RenderFieldWithHint(EditorGUILayout.TextField, string.IsNullOrEmpty, value.Name, "Product Name");

                    value.Value = RenderFieldWithHint(GuidField, guid => guid.Equals(Guid.Empty), value.Value, "Product Id");

                    GUILayout.EndHorizontal();

                    return value;
                });

            return newValue;
        }

        public static List<string> RenderInput(ConfigFieldAttribute configFieldDetails, List<string> value,
            float labelWidth)
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
            return InputRendererWrapper(configFieldDetails.Label, value, labelWidth, tooltip,
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
            return InputRendererWrapper(configFieldDetails.Label, value, labelWidth, tooltip, (s, b, arg3, arg4) =>
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
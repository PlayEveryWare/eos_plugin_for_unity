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
    using Epic.OnlineServices.Platform;
    using EpicOnlineServices.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using Config = EpicOnlineServices.Config;

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

        private static GUIContent CreateGUIContent(string label, string tooltip = null, bool bold = false)
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
        public static bool RenderFoldout(bool isOpen, string hideLabel, string showLabel, Action renderContents, string tooltip = null)
        {
            isOpen = EditorGUILayout.Foldout(isOpen, isOpen ? hideLabel : showLabel);

            if (!isOpen)
            {
                return false;
            }

            // This simulates the foldout being indented
            //GUILayout.BeginVertical();
            //GUILayout.BeginHorizontal();
            //GUILayout.Space(20f);
            renderContents();
            //GUILayout.EndHorizontal();
            //GUILayout.Space(10f);
            //GUILayout.EndVertical();
            return true;
        }

        public static void AssigningTextField(string label, ref string value, float labelWidth = -1,
            string tooltip = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue = EditorGUILayout.TextField(CreateGUIContent(label, tooltip), value ?? "",
                GUILayout.ExpandWidth(true));
            if (newValue != null)
            {
                value = newValue;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public static void AssigningULongToStringField(string label, ref ulong? value, float labelWidth = -1,
            string tooltip = null)
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

        public static void AssigningBoolField(string label, ref bool value, float labelWidth = -1,
            string tooltip = null)
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

        public static void AssigningFloatToStringField(string label, ref float? value, float labelWidth = -1,
            string tooltip = null)
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

        public static void AssigningEnumField<T>(string label, ref T value, float labelWidth = -1,
            string tooltip = null) where T : Enum
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            if (labelWidth >= 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }

            var newValue =
                (T)EditorGUILayout.EnumFlagsField(CreateGUIContent(label, tooltip), value, GUILayout.ExpandWidth(true));
            value = newValue;

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        #region New methods for rendering input fields

        /// <summary>
        /// Use reflection to retrieve a collection of fields that have been
        /// assigned custom ConfigFieldAttribute attributes, grouping by group,
        /// and sorting by group.
        /// </summary>
        /// <returns>A collection of config fields.</returns>
        private static IOrderedEnumerable<IGrouping<int, (FieldInfo FieldInfo, ConfigFieldAttribute FieldDetails)>> GetFieldsByGroup<T>()
        {
            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<ConfigFieldAttribute>() != null)
                .Select(info => (info, info.GetCustomAttribute<ConfigFieldAttribute>()))
                .GroupBy(r => r.Item2.Group)
                .OrderBy(group => group.Key);
        }

        private static IOrderedEnumerable<IGrouping<int, (MemberInfo MemberInfo, ConfigFieldAttribute FieldDetails)>> GetMembersByGroup<T>()
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var members = fields.Cast<MemberInfo>().Concat(properties.Cast<MemberInfo>());

            return members
                .Where(member => member.GetCustomAttribute<ConfigFieldAttribute>() != null)
                .Select(member => (MemberInfo: member, FieldDetails: member.GetCustomAttribute<ConfigFieldAttribute>()))
                .GroupBy(r => r.FieldDetails.Group)
                .OrderBy(group => group.Key);
        }

        delegate object RenderInputDelegate(ConfigFieldAttribute attribute, object value, float labelWidth);

        static readonly Dictionary<Type, RenderInputDelegate> RenderInputMethods = new()
        {
            { typeof(Deployment), (attr, val, width) => RenderInput(attr, (Deployment)val, width) },
            { typeof(EOSClientCredentials), (attr, val, width) => RenderInput(attr, (EOSClientCredentials)val, width) },
            { typeof(SetOfNamed<EOSClientCredentials>), (attr, val, width) => RenderInput(attr, (SetOfNamed<EOSClientCredentials>)val, width) },
            { typeof(string), (attr, val, width) => RenderInput(attr, (string)val, width) },
            { typeof(ulong), (attr, val, width) => RenderInput(attr, (ulong)val, width) },
            { typeof(uint), (attr, val, width) => RenderInput(attr, (uint)val, width) },
            { typeof(ProductionEnvironments), (attr, val, width) => RenderInput(attr, (ProductionEnvironments)val, width) },
            { typeof(float), (attr, val, width) => RenderInput(attr, (float)val, width) },
            { typeof(double), (attr, val, width) => RenderInput(attr, (double)val, width) },
            { typeof(WrappedInitializeThreadAffinity), (attr, val, width) => RenderInput(attr, (WrappedInitializeThreadAffinity)val, width) },
            { typeof(bool), (attr, val, width) => RenderInput(attr, (bool)val, width) },
            // Add other specific types as needed
        };

        static readonly Dictionary<ConfigFieldType, FieldHandler> FieldHandlers = new()
        {
            { ConfigFieldType.Text, HandleField<string> },
            { ConfigFieldType.FilePath, (target, fieldDetails, getValue, setValue, labelWidth) =>
                HandleField<string>(target, (FilePathFieldAttribute)fieldDetails, getValue, setValue, labelWidth) },
            { ConfigFieldType.Flag, HandleField<bool> },
            { ConfigFieldType.DirectoryPath, (target, fieldDetails, getValue, setValue, labelWidth) =>
                HandleField<string>(target, (DirectoryPathFieldAttribute)fieldDetails, getValue, setValue, labelWidth) },
            { ConfigFieldType.Ulong, HandleField<ulong> },
            { ConfigFieldType.Double, HandleField<double> },
            { ConfigFieldType.TextList, HandleField<List<string>> },
            { ConfigFieldType.Uint, HandleField<uint> },
            { ConfigFieldType.Float, HandleField<float> },
            { ConfigFieldType.ProductionEnvironments, HandleField<ProductionEnvironments> },
            { ConfigFieldType.SetOfClientCredentials, HandleField<SetOfNamed<EOSClientCredentials>> },
            { ConfigFieldType.NamedGuid, HandleField<Named<Guid>> },
            { ConfigFieldType.Version, HandleField<Version> },
            { ConfigFieldType.Deployment, HandleField<Deployment> },
            { ConfigFieldType.ClientCredentials, HandleField<EOSClientCredentials> },
            { ConfigFieldType.InitializeThreadAffinity, HandleField<WrappedInitializeThreadAffinity> },
            { ConfigFieldType.Button, HandleButtonField },
            { ConfigFieldType.Enum, HandleEnumField },
            // Add other field types as needed
        };

        private static Dictionary<int, bool> _foldoutStates = new();

        private static T RenderInput<T>(ConfigFieldAttribute attribute, T value, float labelWidth)
        {
            if (typeof(T) == typeof(WrappedInitializeThreadAffinity))
            {
                // Create a foldout label with a tooltip
                GUIContent foldoutContent = new(attribute.Label, attribute.ToolTip);

                if (!_foldoutStates.ContainsKey(attribute.GetHashCode()))
                {
                    _foldoutStates.Add(attribute.GetHashCode(), false);
                }

                GUIStyle boldFoldoutStyle = new(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };

                if (!string.IsNullOrEmpty(attribute.HelpURL))
                {
                    EditorGUILayout.BeginHorizontal();
                }

                _foldoutStates[attribute.GetHashCode()] = EditorGUILayout.Foldout(_foldoutStates[attribute.GetHashCode()], foldoutContent, true, boldFoldoutStyle);

                if (!string.IsNullOrEmpty(attribute.HelpURL))
                {
                    GUILayout.FlexibleSpace();
                    RenderHelpIcon(attribute.HelpURL);
                    EditorGUILayout.EndHorizontal();
                }

                if (_foldoutStates[attribute.GetHashCode()])
                {
                    RenderInputs(ref value);
                }
            }
            return value;
        }

        static void HandleField<TField>(
            object target,
            ConfigFieldAttribute fieldDetails,
            Func<object, object> getValue,
            Action<object, object> setValue,
            float labelWidth)
        {
            var currentValue = getValue(target);

            object newValue;

            if (RenderInputMethods.TryGetValue(typeof(TField), out var renderMethod))
            {
                newValue = renderMethod(fieldDetails, currentValue, labelWidth);
            }
            else
            {
                newValue = RenderInput(fieldDetails, (TField)currentValue, labelWidth);
            }

            setValue(target, newValue);
        }


        static void HandleButtonField(
            object target,
            ConfigFieldAttribute fieldDetails,
            Func<object, object> getValue,
            Action<object, object> setValue,
            float labelWidth)
        {
            if (GUILayout.Button(fieldDetails.Label) && getValue(target) is Action onClick)
            {
                onClick();
            }
        }

        static void HandleEnumField(
            object target,
            ConfigFieldAttribute fieldDetails,
            Func<object, object> getValue,
            Action<object, object> setValue,
            float labelWidth)
        {
            var enumValue = getValue(target);
            Type enumType = enumValue.GetType();
            var method = typeof(GUIEditorUtility).GetMethod("RenderEnumInput", BindingFlags.Public | BindingFlags.Static);
            var genericMethod = method.MakeGenericMethod(enumType);
            var newValue = genericMethod.Invoke(null, new object[] { fieldDetails, enumValue, labelWidth });
            setValue(target, newValue);
        }



        delegate void FieldHandler(
            object target,
            ConfigFieldAttribute fieldDetails,
            Func<object, object> getValue,
            Action<object, object> setValue,
            float labelWidth);

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
        public static void RenderInputs<T>(ref T value)
        {
            string[] groupLabels = typeof(T).GetCustomAttribute<ConfigGroupAttribute>()?.GroupLabels;

            foreach (var fieldGroup in GetMembersByGroup<T>().ToList())
            {
                List<string> labelsInGroup = fieldGroup.Select(field => field.FieldDetails.Label).ToList();
                float labelWidth = MeasureLongestLabelWidth(labelsInGroup);

                // If there is a label for the field group, then display it.
                if (0 <= fieldGroup.Key && groupLabels?.Length > fieldGroup.Key)
                {
                    GUILayout.Label(groupLabels[fieldGroup.Key], EditorStyles.boldLabel);
                }

                foreach (var member in fieldGroup)
                {
                    // Check if the config value should be disabled.
                    if (value is PlatformConfig platformConfig &&
                        (member.FieldDetails.PlatformsEnabledOn & platformConfig.Platform) == 0)
                    {
                        GUI.enabled = false;
                        // TODO: Consider whether it makes more sense to simply not display the input
                        member.FieldDetails.ToolTip = $"These options are not available on {PlatformManager.GetFullName(platformConfig.Platform)}.";
                    }

                    // Retrieve GetValue and SetValue functions
                    Func<object, object> GetValueFn = null;
                    Action<object, object> SetValueFn = null;

                    if (member.MemberInfo is FieldInfo fieldInfo)
                    {
                        GetValueFn = fieldInfo.GetValue;
                        SetValueFn = fieldInfo.SetValue;
                    }
                    else if (member.MemberInfo is PropertyInfo propertyInfo)
                    {
                        GetValueFn = propertyInfo.GetValue;
                        SetValueFn = propertyInfo.SetValue;
                    }
                    else
                    {
                        continue; // Skip if MemberInfo is neither FieldInfo nor PropertyInfo
                    }

                    // Use the handler from the dictionary
                    if (FieldHandlers.TryGetValue(member.FieldDetails.FieldType, out var handler))
                    {
                        handler(value, member.FieldDetails, GetValueFn, SetValueFn, labelWidth);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException($"Unhandled field type: {member.FieldDetails.FieldType}");
                    }

                    GUI.enabled = true;
                }
            }
        }

        public static float MeasureLongestLabelWidth(List<string> labels)
        {
            GUIStyle labelStyle = new(GUI.skin.label);

            string longestString = string.Empty;
            foreach (string label in labels)
            {
                if (label.Length <= longestString.Length)
                    continue;

                longestString = label;
            }

            return labelStyle.CalcSize(new GUIContent(longestString)).x;
        }

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
            RenderHelpIconInternal(url, (content, style) => GUI.Button(area, content, style));
        }

        private static void RenderHelpIcon(string url)
        {
            RenderHelpIconInternal(url, (content, style) => GUILayout.Button(content, style));
        }

        private static void RenderHelpIconInternal(string url, Func<GUIContent, GUIStyle, bool> renderButtonFn)
        {
            GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");

            Color hoverColor = !EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.8f, 0.8f, 0.8f);

            Texture2D hoverTexture = new(1, 1);
            hoverTexture.SetPixel(0, 0, hoverColor);
            hoverTexture.Apply();

            GUIStyle helpButtonStyle = new(EditorStyles.label)
            {
                padding = new RectOffset(0, 0, 0, 0),
                fixedWidth = EditorGUIUtility.singleLineHeight,
                fixedHeight = EditorGUIUtility.singleLineHeight,
                normal = { background = Texture2D.redTexture }, // No background by default
                hover = { background = Texture2D.redTexture }, // Gray back
            };

            if (renderButtonFn(helpIcon, helpButtonStyle))
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
                },
                onRemoveCallback = (list) =>
                {
                    if (list.index < 0 || list.index >= items.Count)
                    {
                        return;
                    }

                    removeItemFn(items[list.index]);
                }
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

                    int newSelectedIndex = EditorGUI.Popup(
                        new Rect(rect.x + firstFieldWidth + 5f + middleFieldWidth + 5f, rect.y, endFieldWidth,
                            rect.height),
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

        private static Version VersionField(GUIContent label, Version value, params GUILayoutOption[] options)
        {
            string tempStringVersion = EditorGUILayout.TextField(label, value.ToString(), options);
            return Version.TryParse(tempStringVersion, out Version newValue) ? newValue : value;
        }

        private static Version VersionField(Version value, params GUILayoutOption[] options)
        {
            string tempStringVersion = EditorGUILayout.TextField(value.ToString(), options);
            return Version.TryParse(tempStringVersion, out Version newValue) ? newValue : value;
        }

        public static EOSClientCredentials RenderInput(ConfigFieldAttribute configFieldAttribute,
            EOSClientCredentials value,
            float labelWidth)
        {
            return InputRendererWithAlignedLabel(labelWidth, () =>
            {
                List<Named<EOSClientCredentials>> credentials = Config.Get<ProductConfig>().Clients.ToList();
                List<string> credentialsLabels = new();
                int selectedIndex = -1;
                int currentIndex = 0;
                foreach (Named<EOSClientCredentials> cred in credentials)
                {
                    if (cred.Value.Equals(value))
                    {
                        selectedIndex = currentIndex;
                    }

                    credentialsLabels.Add($"{cred.Name} : {cred.Value.ClientId}");

                    currentIndex++;
                }

                int newIndex = EditorGUILayout.Popup(
                    CreateGUIContent(configFieldAttribute.Label, configFieldAttribute.ToolTip),
                    selectedIndex,
                    credentialsLabels.ToArray());

                return (newIndex >= 0 && newIndex < credentials.Count) ? credentials[newIndex].Value : value;
            });
        }

        public static Deployment RenderInput(ConfigFieldAttribute configFieldAttribute, Deployment value, float labelWidth)
        {
            return InputRendererWithAlignedLabel(labelWidth, () =>
            {
                List<Named<Deployment>> deployments = Config.Get<ProductConfig>().Environments.Deployments.ToList();
                List<string> deploymentLabels = new();
                int selectedIndex = -1;
                int currentIndex = 0;
                foreach (Named<Deployment> deployment in deployments)
                {
                    if (value.DeploymentId == deployment.Value.DeploymentId)
                        selectedIndex = currentIndex;

                    deploymentLabels.Add($"{deployment.Name}: {deployment.Value.DeploymentId}");

                    currentIndex++;
                }

                int newIndex = EditorGUILayout.Popup(
                    CreateGUIContent(configFieldAttribute.Label, configFieldAttribute.ToolTip),
                    selectedIndex,
                    deploymentLabels.ToArray());

                return (newIndex >= 0 && newIndex < deployments.Count) ? deployments[newIndex].Value : value;
            });
        }

        public static TEnum RenderEnumInput<TEnum>(ConfigFieldAttribute configFieldAttribute, TEnum value, float labelWidth) where TEnum : Enum
        {
            return InputRendererWrapper(configFieldAttribute.Label, configFieldAttribute.ToolTip, labelWidth, value,
                EnumFlagsField, configFieldAttribute.HelpURL);
        }

        private static TEnum EnumFlagsField<TEnum>(GUIContent label, TEnum value, params GUILayoutOption[] options) where TEnum : Enum
        {
            return (TEnum)EditorGUILayout.EnumFlagsField(label, value, options);
        }

        public static Version RenderInput(ConfigFieldAttribute configFieldAttribute, Version value, float labelWidth)
        {
            return InputRendererWrapper(configFieldAttribute.Label, configFieldAttribute.ToolTip, labelWidth, value,
                VersionField);
        }

        public static SetOfNamed<EOSClientCredentials> RenderInput(ConfigFieldAttribute configFieldAttribute,
            SetOfNamed<EOSClientCredentials> value, float labelWidth)
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

        public static WrappedInitializeThreadAffinity RenderInput(ConfigFieldAttribute attribute, WrappedInitializeThreadAffinity value)
        {
            EditorGUILayout.LabelField(CreateGUIContent(attribute.Label, attribute.ToolTip), new GUIStyle() {fontStyle = FontStyle.Bold});
            RenderInputs(ref value);
            return value;
        }

        public static ProductionEnvironments RenderInput(ConfigFieldAttribute configFieldAttribute,
            ProductionEnvironments value, float labelWidth)
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

        public static Named<Guid> RenderInput(ConfigFieldAttribute configFieldDetails, Named<Guid> value,
            float labelWidth)
        {
            EditorGUILayout.LabelField(CreateGUIContent(configFieldDetails.Label, configFieldDetails.ToolTip));

            GUILayout.BeginHorizontal();

            value ??= new Named<Guid>();

            value.Name = RenderFieldWithHint(EditorGUILayout.TextField, string.IsNullOrEmpty, value.Name,
                "Product Name");

            value.Value = RenderFieldWithHint(GuidField, guid => guid.Equals(Guid.Empty), value.Value, "Product Id");

            GUILayout.EndHorizontal();

            return value;
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

        public static string RenderInput(DirectoryPathFieldAttribute configFieldAttributeDetails, string value, float labelWidth, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();

            string filePath = InputRendererWrapper(configFieldAttributeDetails.Label, value, labelWidth, tooltip, EditorGUILayout.TextField, configFieldAttributeDetails.HelpURL);

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

            string filePath = InputRendererWrapper(configFieldAttributeDetails.Label, value, labelWidth, tooltip, EditorGUILayout.TextField, configFieldAttributeDetails.HelpURL);

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

        public static double RenderInput(ConfigFieldAttribute configFieldDetails, double value, float labelWidth)
        {
            return InputRendererWrapper(configFieldDetails.Label, configFieldDetails.ToolTip, labelWidth, value, EditorGUILayout.DoubleField, configFieldDetails.HelpURL);
        }

        public static float RenderInput(ConfigFieldAttribute configFieldDetails, float value, float labelWidth)
        {
            return InputRendererWrapper(configFieldDetails.Label, configFieldDetails.ToolTip, labelWidth, value, EditorGUILayout.FloatField, configFieldDetails.HelpURL);
        }

        public static string RenderInput(ConfigFieldAttribute configFieldDetails, string value, float labelWidth)
        {
            return InputRendererWrapper(configFieldDetails.Label, configFieldDetails.ToolTip, labelWidth, value, EditorGUILayout.TextField, configFieldDetails.HelpURL);
        }

        public static ulong RenderInput(ConfigFieldAttribute configFieldDetails, ulong value, float labelWidth)
        {
            _ = SafeTranslatorUtility.TryConvert(value, out long temp);

            long longValue = InputRendererWrapper(configFieldDetails.Label, configFieldDetails.ToolTip, labelWidth,
                temp, EditorGUILayout.LongField);

            return SafeTranslatorUtility.TryConvert(longValue, out ulong newValue) ? newValue : value;
        }

        private static ulong RenderInput(string label, string tooltip, ulong value, float labelWidth)
        {
            _ = SafeTranslatorUtility.TryConvert(value, out long temp);

            long longValue = InputRendererWrapper(label, tooltip, labelWidth,
                temp, EditorGUILayout.LongField);

            return SafeTranslatorUtility.TryConvert(longValue, out ulong newValue) ? newValue : value;
        }

        public static uint RenderInput(ConfigFieldAttribute configFieldDetails, uint value, float labelWidth)
        {
            _ = SafeTranslatorUtility.TryConvert(value, out int temp);

            int intValue = InputRendererWrapper(configFieldDetails.Label, configFieldDetails.ToolTip, labelWidth, temp,
                EditorGUILayout.IntField);
            
            return SafeTranslatorUtility.TryConvert(intValue, out uint newValue) ? newValue : value;
        }

        public static bool RenderInput(ConfigFieldAttribute configFieldDetails, bool value, float labelWidth)
        {
            return InputRendererWrapper(
                configFieldDetails.Label, configFieldDetails.ToolTip, labelWidth,
                value, EditorGUILayout.Toggle);
        }

        public delegate T TestDelegate<T>(GUIContent label, T value, params GUILayoutOption[] options);

        private static T InputRendererWithAlignedLabel<T>(float labelWidth, Func<T> renderFn)
        {
            float currentLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            T newValue = renderFn();

            EditorGUIUtility.labelWidth = currentLabelWidth;

            return newValue;
        }

        private static T InputRendererWrapper<T>(string label, string toolTip, float labelWidth, T value, TestDelegate<T> renderFn, string helpURL = null)
        {
            return InputRendererWithAlignedLabel(labelWidth, () =>
            {
                if (!string.IsNullOrEmpty(helpURL))
                {
                    EditorGUILayout.BeginHorizontal();
                }

                T newValue = renderFn(CreateGUIContent(label, toolTip), value, GUILayout.ExpandWidth(true));

                if (!string.IsNullOrEmpty(helpURL))
                {
                    RenderHelpIcon(helpURL);
                    EditorGUILayout.EndHorizontal();
                }

                return newValue;
            });
        }

        #endregion
    }
}
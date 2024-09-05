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

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;

    [Serializable]
    public abstract class EOSEditorWindow : EditorWindow
    {
        /// <summary>
        /// Used to mark fields in deriving classes as values that should be saved when the window is subsequently opened.
        /// </summary>
        protected class RetainPreference : Attribute
        {
            public RetainPreference(string key)
            {
                Key = key;
            }

            public string Key { get; }
        }

        /// <summary>
        /// Determines whether or not the editor window resizes itself.
        /// </summary>
        private bool _autoResize = true;

        /// <summary>
        /// Determines whether or not the window contents are padded. Typically
        /// this will always stay true, but if the window is being rendered within a SettingsWindow,
        /// then this will be turned off.
        /// </summary>
        private bool _isPadded = true;

        /// <summary>
        /// Padding used in a variety of places.
        /// </summary>
        protected const float Padding = 10.0f;

        /// <summary>
        /// Absolute minimum height for any window
        /// </summary>
        private readonly float AbsoluteMinimumWindowHeight;

        /// <summary>
        /// Absolute minimum width for any window
        /// </summary>
        private readonly float AbsoluteMinimumWindowWidth;

        /// <summary>
        /// Determines whether the window is being rendered within another window (like the Preferences window)
        /// </summary>
        private bool? _isEmbedded;

        /// <summary>
        /// Stores the prefix to use when this window saves preferences to EditorPrefs. This value can be passed to the
        /// constructor if it needs to be overridden for some reason, but by default it is set to the typename of the window
        /// that extends this base class.
        /// </summary>
        private readonly string _preferencesKey;

        /// <summary>
        /// Used to asynchronously carry out any tasks associated with initializing the window.
        /// </summary>
        private Task _initializeTask;

        /// <summary>
        /// Indicates whether the window has been initialized.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Static reference to a mono font, used in a variety of editor
        /// windows.
        /// </summary>
        protected static Font MonoFont;

        protected EOSEditorWindow(string windowTitle, float minimumHeight = 50f, float minimumWidth = 50f,
            string preferencesOverrideKey = null)
        {
            // Set the preferences key either to the full name of the deriving type, or the provided override value.
            _preferencesKey = string.IsNullOrEmpty(preferencesOverrideKey)
                ? GetType().FullName
                : preferencesOverrideKey;

            AbsoluteMinimumWindowHeight = minimumHeight;
            AbsoluteMinimumWindowWidth = minimumWidth;

            WindowTitle = windowTitle;
        }

        /// <summary>
        /// String value for the title of the window.
        /// </summary>
        public string WindowTitle { get; }

        private void OnEnable()
        {
            _initializeTask = Initialize();
            EditorApplication.update += CheckForInitialized;
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckForInitialized;
        }

        /// <summary>
        /// Checks to see if the windows has been initialized.
        /// </summary>
        private void CheckForInitialized()
        {
            if (!_initializeTask.IsCompleted || _initialized)
            {
                return;
            }

            _initialized = true;
            Repaint();
            EditorApplication.update -= CheckForInitialized;

            if (_initializeTask.IsCompletedSuccessfully)
            {
                return;
            }

            StringBuilder errorMessageBuilder = new($"There were some exceptions initializing the window: \n");

            if (_initializeTask.Exception?.InnerExceptions != null)
            {
                foreach (var e in _initializeTask.Exception?.InnerExceptions)
                {
                    errorMessageBuilder.AppendLine(e.Message);
                }
            }

            string errorMessage = errorMessageBuilder.ToString();

            Debug.LogError(errorMessage);
        }

        /// <summary>
        /// Sets whether or not the editor window resizes itself.
        /// </summary>
        /// <param name="autoResize"></param>
        protected void SetAutoResize(bool autoResize)
        {
            _autoResize = autoResize;
        }

        /// <summary>
        /// Async function. Base implementation is done, but completes the process of setup by marking
        /// the window as initialized, so it is important that deriving classes call the base implementation
        /// at the END of their overriding function.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task AsyncSetup()
        {
            // default implementation does nothing.
            await Task.Run(() => { });
        }

        /// <summary>
        /// Implement this method to initialize the contents of the editor window. Be sure to always call the base implementation first if you override it.
        /// </summary>
        protected virtual void Setup()
        {
            // Load the mono font
            if (null == MonoFont)
            {
                MonoFont = Resources.Load<Font>("SourceCodePro");
            }

            LoadWindowPreferences();
        }

        /// <summary>
        /// Override this method to perform tasks when the window is being closed. Be sure to call the base implementation before adding your own implementation.
        /// </summary>
        protected virtual void Teardown()
        {
            SaveWindowPreferences();
        }

        /// <summary>
        /// Implement this method to define the rendering behavior of the window.
        /// </summary>
        protected abstract void RenderWindow();

        /// <summary>
        /// Wrapper function to call the setup function defined by deriving classes. This allows keeping track of whether the window has been set up.
        /// </summary>
        private async Task Initialize()
        {
            Setup();

            await AsyncSetup();
        }

        /// <summary>
        /// Sets whether the window is embedded - such as is the case when being rendered within the Preferences Unity window.
        /// In such a case, the padding and auto-resize functionality is disabled.
        /// </summary>
        /// <param name="isEmbedded">Whether or not the window is being rendered within another.</param>
        protected void SetIsEmbedded(bool? isEmbedded = null)
        {
            _isEmbedded = isEmbedded;

            if (_isEmbedded == null)
            {
                return;
            }

            _isPadded = !_isEmbedded.Value;
            _autoResize = !_isEmbedded.Value;
        }

        /// <summary>
        /// Called by Unity to render the window. Should not be overridden by deriving classes.
        /// </summary>
        public void OnGUI()
        {
            // don't do anything if not initialized
            if (!_initialized)
                return;

            // if padding should be applied to the window
            if (_isPadded)
            {
                // The area in which to add controls - this keeps all content padded within the window.
                Rect paddedArea = new(
                    Padding,
                    Padding,
                    position.width - (2 * Padding),
                    position.height - (2 * Padding));

                // Begin the padded area
                GUILayout.BeginArea(paddedArea);
            }

            // Explicitly set the window title, because sometimes Unity will
            // change it inexplicably to be the fully-qualified class name
            titleContent = new GUIContent(WindowTitle);

            // Call the implemented method to render the window
            RenderWindow();

            // After the window has been rendered, adjust the window size to fit the contents
            if (_autoResize)
            {
                AdjustWindowSize();
            }

            // we only need to close the area if we are padded, because otherwise it will not have 
            // been opened.
            if (_isPadded)
            {
                // End the padded area
                GUILayout.EndArea();
            }
        }

        public void OnDestroy()
        {
            Teardown();
        }

        /// <summary>
        /// Determines if the size of the window needs to be updated, and if it does, adjusts it. 
        /// </summary>
        private void AdjustWindowSize()
        {
            // We only want to adjust the window size in the event of a repaint operation
            if (EventType.Repaint != Event.current.type)
                return;

            // tolerance so as to responsibly compare float equality
            const float tolerance = 0.0000001f;

            // golden ratio
            const float aspectRatio = 1.618f;

            // Get the rect of the last drawn element
            Rect lastRect = GUILayoutUtility.GetLastRect();

            // Calculate the total height (Y position + height of the last element)
            // added to both values is padding twice (one for padding on either side)
            float minHeight = lastRect.y + lastRect.height + (2 * Padding);
            minHeight = Math.Max(minHeight, AbsoluteMinimumWindowHeight);

            // Using the calculated minimum height, apply the aspect ratio to determine minimum width
            float minWidth = minHeight / aspectRatio + (2 * Padding);
            minWidth = Math.Max(minWidth, AbsoluteMinimumWindowWidth);

            // Only change the window size if the calculated size has changed
            if (!(Math.Abs(minHeight - this.minSize.y) > tolerance) &&
                !(Math.Abs(minHeight - this.maxSize.y) > tolerance))
            {
                return;
            }

            // Set the height to be the new minimumHeight
            Rect currentPosition = position;
            currentPosition.height = minHeight;

            position = currentPosition;

            this.minSize = new Vector2(minWidth, minHeight);
            this.maxSize = new Vector2(Screen.width, minHeight);
        }

        #region Preferences

        /// <summary>
        /// Enumerates the fields that have the Retainable attribute set on them.
        /// </summary>
        private IEnumerable<(FieldInfo Field, string Key)> RetainableFields
        {
            get
            {
                return from field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    let attribute = field.GetCustomAttribute<RetainPreference>()
                    where attribute != null
                    select (Field: field, KeyInfo: ((RetainPreference)attribute).Key);
            }
        }

        /// <summary>
        /// Saves preferences for the window, if there are any.
        /// </summary>
        protected void SaveWindowPreferences()
        {
            foreach (var (field, key) in RetainableFields)
            {
                SavePreference(key, field.GetValue(this));
            }
        }

        /// <summary>
        /// Loads preferences for the window, if there are any.
        /// </summary>
        protected void LoadWindowPreferences()
        {
            foreach (var (field, key) in RetainableFields)
            {
                if (field.FieldType.IsValueType)
                {
                    if (typeof(int) != field.FieldType) { continue; }

                    if (TryReadPreference(key, out int value))
                    {
                        field.SetValue(this, value);
                    }
                }
                else
                {
                    MethodInfo tryReadPreferenceMethod = GetType().GetMethod("TryReadObjectPreference",
                            BindingFlags.NonPublic | BindingFlags.Instance)?
                        .MakeGenericMethod(field.FieldType);

                    object[] args = { key, null };

                    bool success = (bool)tryReadPreferenceMethod?.Invoke(this, args)!;
                    if (success)
                    {
                        field.SetValue(this, args[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Saves an object to editor preferences using a given key.
        /// </summary>
        /// <typeparam name="T">The type to store in the preferences (should be serializable by JsonUtility).</typeparam>
        /// <param name="key">They key to store the value in the preferences with. This value will be prepended with the name of the most derived class type, so that key values are scoped on a per-editor window basis.</param>
        /// <param name="value">The value to store in the EditorPrefs (should be serializable by JsonUtility).</param>
        protected void SavePreference<T>(string key, T value) where T : class
        {
            try
            {
                string jsonString = JsonUtility.ToJson(value);
                SavePreference(key, jsonString);
            }
            catch (Exception)
            {
                Debug.LogWarning($"Was not able to save object of type \"{typeof(T).Name}\" to preferences.");
            }
        }

        /// <summary>
        /// Reads an object from the editor preferences using a given key.
        /// </summary>
        /// <typeparam name="T">The type to read from the editor preferences. Should be serializable/deserializable by JsonUtility</typeparam>
        /// <param name="key">They key at which the value is stored.</param>
        /// <param name="value">The value stored at the indicated key.</param>
        /// <returns>The value stored in the editor preferences.</returns>
        protected bool TryReadObjectPreference<T>(string key, out T value) where T : class, new()
        {
            value = new T();

            try
            {
                if (TryReadPreference(key, out string jsonString))
                {
                    value = JsonUtility.FromJson<T>(jsonString);
                    return true;
                }
                else
                {
                    Debug.Log($"No stored preference for key value \"{key}\".");
                }
            }
            catch (Exception)
            {
                Debug.LogWarning($"Was not able to read preference at key \"{key}\".");
            }

            return false;
        }

        /// <summary>
        /// Reads a string preference from the EditorPrefs.
        /// </summary>
        /// <param name="key">They key to read from (note that this key is scoped to the window, so different windows are allowed to use the same key, because under the hood the key at which the value will actually be stored will be prepended with the full name of the type of window).</param>
        /// <param name="value">The string stored at the EditorPrefs key value indicated.</param>
        /// <returns>True if the preference was successfully read, false otherwise.</returns>
        protected bool TryReadPreference(string key, out string value)
        {
            value = EditorPrefs.GetString($"{_preferencesKey}.{key}");
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Saves a string preference to the EditorPrefs.
        /// </summary>
        /// <param name="key">The key at which to write the value.</param>
        /// <param name="value">The string value to write to the editor preferences at the indicated key.</param>
        protected void SavePreference(string key, string value)
        {
            EditorPrefs.SetString($"{_preferencesKey}.{key}", value);
        }

        protected void SavePreference(string key, int value)
        {
            EditorPrefs.SetInt(key, value);
        }

        protected bool TryReadPreference(string key, out int value)
        {
            value = EditorPrefs.GetInt(key);
            return true;
        }

        #endregion

    }
}
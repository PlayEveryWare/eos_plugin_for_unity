/*
 * Copyright (c) 2023 PlayEveryWare
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

using UnityEditor;

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using UnityEngine;

    public abstract class EOSEditorWindow : EditorWindow
    {
        /// <summary>
        /// Keeps track of whether the window has been initialized.
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Determines whether or not the editor window resizes itself.
        /// </summary>
        private bool _autoResize = true;

        /// <summary>
        /// Padding used in a variety of places.
        /// </summary>
        protected const float Padding = 10.0f;

        private const float AbsoluteMinimumWindowHeight = 50f;
        private const float AbsoluteMinimumWindowWidth = 50f;

        /// <summary>
        /// Sets whether or not the editor window resizes itself.
        /// </summary>
        /// <param name="autoResize"></param>
        protected void SetAutoResize(bool autoResize)
        {
            _autoResize = autoResize;
        }

        /// <summary>
        /// Implement this method to initialize the contents of the editor window.
        /// </summary>
        protected virtual void Setup() { }

        /// <summary>
        /// Implement this method to define the rendering behavior of the window.
        /// </summary>
        protected abstract void RenderWindow();

        /// <summary>
        /// Wrapper function to call the setup function defined by deriving classes. This allows keeping track of whether the window has been set up.
        /// </summary>
        private void Initialize()
        {
            // return if the window has already been initialized.
            if (_initialized) return;

            // Otherwise, run the setup function, and mark the window as being initialized
            Setup();
            _initialized = true;
        }

        public void OnGUI()
        {
            // Call initialize, in case it hasn't already happened
            Initialize();

            // The area in which to add controls - this keeps all content padded within the window.
            Rect paddedArea = new Rect(Padding, Padding, position.width - (2 * Padding),
                position.height - (2 * Padding));

            // Begin the padded area
            GUILayout.BeginArea(paddedArea);

            // Call the implemented method to render the window
            RenderWindow();

            // After the window has been rendered, adjust the window size to fit the contents
            if (_autoResize)
            {
                AdjustWindowSize();
            }

            // End the padded area
            GUILayout.EndArea();
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

    }
}
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

namespace PlayEveryWare.EpicOnlineServices.Editor.Controls
{
    using Codice.Client.GameUI.Explorer;
    using EpicOnlineServices.Utility;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;

    public class CancellableAsyncProgressBar
    {
        private static Font s_MonoFont;

        public static void Render(float progress, string text, CancellationTokenSource cancellationToken, Action onCancel)
        {
            // Load the font resource if it has not already been loaded
            if (null == s_MonoFont)
            {
                s_MonoFont = Resources.Load<Font>("SourceCodePro");
            }

            GUILayout.BeginVertical();
            GUILayout.Space(20f);
            GUI.enabled = true;

            // Make a taller progress bar
            var progressBarRect = EditorGUILayout.GetControlRect();
            progressBarRect.height *= 2;

            GUIStyle customLabelStyle = new(EditorStyles.label)
            {
                font = s_MonoFont,
                fontSize = 14,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold
            };

            Vector2 labelSize = customLabelStyle.CalcSize(new GUIContent(text));

            Rect labelRect = new(
                progressBarRect.x + (progressBarRect.width - labelSize.x) / 2,
                progressBarRect.y + (progressBarRect.height - labelSize.y) / 2,
                labelSize.x,
                labelSize.y);

            EditorGUI.ProgressBar(progressBarRect, progress, "");

            GUI.Label(labelRect, text, customLabelStyle);

            GUILayout.Space(20f);
            if (GUILayout.Button("Cancel"))
            {
                cancellationToken?.Cancel();
                onCancel?.Invoke();
            }
            GUILayout.EndVertical();
        }
    }
}
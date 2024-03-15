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
    using UnityEditor;
    using UnityEngine;
    using Utility;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all ConfigEditors that represent the configuration options for a specific platform.
    /// </summary>
    /// <typeparam name="T">Intended to be a type accepted by the templated class ConfigHandler.</typeparam>
    public abstract class PlatformConfigEditor<T> : ConfigEditor<T> where T : PlatformConfig, new()
    {
        /// <summary>
        /// The platform that this PlatformConfigEditor represents.
        /// </summary>
        protected PlatformManager.Platform Platform;

        protected PlatformConfigEditor(PlatformManager.Platform platform) :
            base(PlatformManager.GetFullName(platform))
        {
            this.Platform = platform;
        }

        /// <summary>
        /// Given that most platform configurations allow for override values of a specific subset of the standard
        /// options applied to all platforms, the rendering of these options is shared by all PlatformConfigEditor implementations.
        ///
        /// TODO: Consider the scenario where there are values that need to be overriden for one platform, but not for another. How would this work?
        /// NOTE: Currently, all platforms override the same set of values, so this is not currently an issue.
        /// </summary>
        public virtual void RenderOverrides()
        {
            GUILayout.Label($"{PlatformManager.GetFullName(Platform)} Override Configuration Values",
                EditorStyles.boldLabel);

            GUIEditorUtility.AssigningFlagTextField("Override Platform Flags (Separated by '|')", ref config.overrideValues.platformOptionsFlags, 250);

            GUIEditorUtility.AssigningFloatToStringField("Override initial button delay for overlay", ref config.overrideValues.initialButtonDelayForOverlay, 250);

            GUIEditorUtility.AssigningFloatToStringField("Override repeat button delay for overlay", ref config.overrideValues.repeatButtonDelayForOverlay, 250);

            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: networkWork", ref config.overrideValues.ThreadAffinity_networkWork);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: storageIO", ref config.overrideValues.ThreadAffinity_storageIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: webSocketIO", ref config.overrideValues.ThreadAffinity_webSocketIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: P2PIO", ref config.overrideValues.ThreadAffinity_P2PIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: HTTPRequestIO", ref config.overrideValues.ThreadAffinity_HTTPRequestIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: RTCIO", ref config.overrideValues.ThreadAffinity_RTCIO);

        }

        public override sealed void RenderContents()
        {
            RenderOverrides();
        }
    }
}
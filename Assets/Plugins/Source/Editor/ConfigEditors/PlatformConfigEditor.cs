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
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utility;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all
    /// ConfigEditors that represent the configuration options for a specific
    /// platform.
    /// </summary>
    /// <typeparam name="T">Intended to be a type accepted by the templated class ConfigHandler.</typeparam>
    public abstract class PlatformConfigEditor<T> : ConfigEditor<T>, IPlatformConfigEditor where T : PlatformConfig
    {
        protected PlatformConfigEditor()
        {
            Load();

            // The label should always be the full name of the platform.
            _labelText = PlatformManager.GetFullName(config.Platform);
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
            GUILayout.Label($"{PlatformManager.GetFullName(config.Platform)} Override Configuration Values",
                EditorStyles.boldLabel);

            GUIEditorUtility.AssigningFlagTextField("Integrated Platform Management Flags (Separated by '|')", ref config.flags, 345);

            GUIEditorUtility.AssigningFlagTextField("Override Platform Flags (Separated by '|')", ref config.overrideValues.platformOptionsFlags, 250);

            GUIEditorUtility.AssigningFloatToStringField("Override initial button delay for overlay", ref config.overrideValues.initialButtonDelayForOverlay, 250);

            GUIEditorUtility.AssigningFloatToStringField("Override repeat button delay for overlay", ref config.overrideValues.repeatButtonDelayForOverlay, 250);

            // TODO: As far as can be determined, it appears that the following
            //       values are the only ones within "overrideValues" that are
            //       actually being used to override the otherwise defined
            //       values within EOSConfig. Changing the values in the fields
            //       for which the input fields are rendered above does not 
            //       seem to have any affect. 
            // 
            //       This is a bug, but is not relevant to the current task as 
            //       of this writing, which is to simply add the field member
            //       "integratedPlatformManagementFlags" to EOSConfig, and make
            //       sure that field is editable within the editor.

            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: networkWork", ref config.overrideValues.ThreadAffinity_networkWork);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: storageIO", ref config.overrideValues.ThreadAffinity_storageIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: webSocketIO", ref config.overrideValues.ThreadAffinity_webSocketIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: P2PIO", ref config.overrideValues.ThreadAffinity_P2PIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: HTTPRequestIO", ref config.overrideValues.ThreadAffinity_HTTPRequestIO);
            GUIEditorUtility.AssigningULongToStringField("Thread Affinity: RTCIO", ref config.overrideValues.ThreadAffinity_RTCIO);
        }

        public bool IsPlatformAvailable()
        {
            return PlatformManager.GetAvailableBuildTargets().Contains(config.Platform);
        }

        public virtual void RenderPlatformSpecificOptions() { }

        public override sealed void RenderContents()
        {
            RenderPlatformSpecificOptions();
            RenderOverrides();
        }
    }
}
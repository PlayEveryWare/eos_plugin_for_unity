/*
* Copyright (c) 2021 PlayEveryWare
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

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{

    public class PlatformSpecificConfigEditor_iOS : IPlatformSpecificConfigEditor
    {
        public static string ConfigFilename = "eos_ios_config.json";
        EOSConfigFile<EOS_iOSConfig> configFile;

        [InitializeOnLoadMethod]
        static void Register()
        {
            EpicOnlineServicesConfigEditor.AddPlatformSpecificConfigEditor(new PlatformSpecificConfigEditor_iOS());
        }

        public string GetNameForMenu()
        {
            return "iOS";
        }

        public void Awake()
        {
            var configFilenamePath = EpicOnlineServicesConfigEditor.GetConfigPath(ConfigFilename);
            configFile = new EOSConfigFile<EOS_iOSConfig>(configFilenamePath);
        }

        public void LoadConfigFromDisk()
        {
            configFile.LoadConfigFromDisk();
        }

        public void OnGUI()
        {
            GUILayout.Label("iOS Configuration Values", EditorStyles.boldLabel);

            EOSConfig overrideValues = null;
            if (configFile.currentEOSConfig.overrideValues == null)
            {
                overrideValues = new EOSConfig();
            }
            else
            {
                overrideValues = configFile.currentEOSConfig.overrideValues;
            }

            EpicOnlineServicesConfigEditor.AssigningFlagTextField("Override Platform Flags (Seperated by '|')", ref overrideValues.platformOptionsFlags, 250);

            EpicOnlineServicesConfigEditor.AssigningFloatToStringField("Override initial button delay for overlay", ref overrideValues.initialButtonDelayForOverlay, 250);

            EpicOnlineServicesConfigEditor.AssigningFloatToStringField("Override repeat button delay for overlay", ref overrideValues.repeatButtonDelayForOverlay, 250);

            EpicOnlineServicesConfigEditor.AssigningBoolField("HACK: force send input without delay", ref overrideValues.hackForceSendInputDirectlyToSDK, 250);

            //EpicOnlineServicesConfigEditor.AssigningBoolField("Use Eased Restriction NRO path", 250, ref configFile.currentEOSConfig.useEasedRestrictionNROPath);

            EpicOnlineServicesConfigEditor.AssigningULongToStringField("Thread Affinity: networkWork", ref overrideValues.ThreadAffinity_networkWork);
            EpicOnlineServicesConfigEditor.AssigningULongToStringField("Thread Affinity: storageIO", ref overrideValues.ThreadAffinity_storageIO);
            EpicOnlineServicesConfigEditor.AssigningULongToStringField("Thread Affinity: webSocketIO", ref overrideValues.ThreadAffinity_webSocketIO);
            EpicOnlineServicesConfigEditor.AssigningULongToStringField("Thread Affinity: P2PIO", ref overrideValues.ThreadAffinity_P2PIO);
            EpicOnlineServicesConfigEditor.AssigningULongToStringField("Thread Affinity: HTTPRequestIO", ref overrideValues.ThreadAffinity_HTTPRequestIO);
            EpicOnlineServicesConfigEditor.AssigningULongToStringField("Thread Affinity: RTCIO", ref overrideValues.ThreadAffinity_RTCIO);

            if (!overrideValues.IsEmpty())
            {
                configFile.currentEOSConfig.overrideValues = overrideValues;
            }

        }

        public void SaveToJSONConfig(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }
    }
}

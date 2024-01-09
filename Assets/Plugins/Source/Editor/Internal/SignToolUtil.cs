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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace PlayEveryWare.EpicOnlineServices
{
    public class EOSPluginEditorSigningConfig : ICloneableGeneric<EOSPluginEditorSigningConfig>, IEmpty
    {
        public string pathToSignTool;
        public string pathToPFX;
        public string pfxPassword;
        public string timestampURL;
        public List<string> dllPaths;

        public EOSPluginEditorSigningConfig Clone()
        {
            return (EOSPluginEditorSigningConfig)this.MemberwiseClone();
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(pathToSignTool)
                && string.IsNullOrEmpty(pathToPFX)
                && string.IsNullOrEmpty(timestampURL)
                && dllPaths == null || dllPaths.Count == 0
                ;
        }
    }

    public class SignToolConfigEditor : ConfigEditor<EOSPluginEditorSigningConfig>
    {
        public SignToolConfigEditor() : base("Code Signing", "eos_plugin_signing_config.json") { }

        [MenuItem("Tools/EOS Plugin/Sign DLLs")]
        static void SignAllDLLs()
        {
            var signTool = new SignToolConfigEditor();
            signTool.Read();

            // stop if there are no dlls to sign
            if (signTool.GetConfig().currentEOSConfig.dllPaths == null)
            {
                return;
            }

            foreach (var dllPath in signTool.GetConfig().currentEOSConfig.dllPaths)
            {
                SignDLL(signTool.GetConfig().currentEOSConfig, dllPath);
            }
        }

        [MenuItem("Tools/EOS Plugin/Sign DLLs", true)]
        static bool CanSignDLLs()
        {
#if UNITY_EDITOR_WIN
            var configSection = new SignToolConfigEditor();
            configSection.Read();

            return (configSection.GetConfig().currentEOSConfig.dllPaths != null);
#else
            return false;
#endif
        }

        static void SignDLL(EOSPluginEditorSigningConfig config, string dllPath)
        {
            var procInfo = new System.Diagnostics.ProcessStartInfo()
            {
                ArgumentList = {
                    "sign",
                    "/f",
                    config.pathToPFX,
                    "/p",
                    config.pfxPassword,
                    "/t",
                    config.timestampURL,
                    dllPath
                }
            };
            procInfo.FileName = config.pathToSignTool;
            procInfo.UseShellExecute = false;
            procInfo.WorkingDirectory = Path.Combine(Application.dataPath, "..");
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            var process = new System.Diagnostics.Process { StartInfo = procInfo };
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
                if (!EmptyPredicates.IsEmptyOrNull(e.Data))
                {
                    Debug.Log(e.Data);
                }
            });

            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
                if (!EmptyPredicates.IsEmptyOrNull(e.Data))
                {
                    Debug.LogError(e.Data);
                }
            });

            bool didStart = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        }

        public override void OnGUI()
        {
            string pathToSigntool = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToSignTool);
            string pathToPFX = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pathToPFX);
            string pfxPassword = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.pfxPassword);
            string timestampURL = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.timestampURL);
            GUIEditorHelper.AssigningPath("Path to SignTool", ref pathToSigntool, "Select SignTool", extension: "exe");
            GUIEditorHelper.AssigningPath("Path to PFX key", ref pathToPFX, "Select PFX key", extension: "pfx");
            GUIEditorHelper.AssigningTextField("PFX password", ref pfxPassword);
            GUIEditorHelper.AssigningTextField("Timestamp Authority URL", ref timestampURL);

            if (configFile.currentEOSConfig.dllPaths == null)
            {
                configFile.currentEOSConfig.dllPaths = new List<string>();
            }
            EditorGUILayout.LabelField("Target DLL Paths");
            for (int i = 0; i < configFile.currentEOSConfig.dllPaths.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                string dllPath = EmptyPredicates.NewIfNull(configFile.currentEOSConfig.dllPaths[i]);
                GUIEditorHelper.AssigningTextField("", ref dllPath);
                configFile.currentEOSConfig.dllPaths[i] = dllPath;
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(100)))
                {
                    configFile.currentEOSConfig.dllPaths.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                configFile.currentEOSConfig.dllPaths.Add("");
            }

            configFile.currentEOSConfig.pathToSignTool = pathToSigntool;
            configFile.currentEOSConfig.pathToPFX = pathToPFX;
            configFile.currentEOSConfig.pfxPassword = pfxPassword;
            configFile.currentEOSConfig.timestampURL = timestampURL.Trim();
        }
    }
}
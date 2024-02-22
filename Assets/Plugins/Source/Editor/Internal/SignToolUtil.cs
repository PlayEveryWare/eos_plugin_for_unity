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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace PlayEveryWare.EpicOnlineServices
{
    public class SigningConfigEditor : ConfigEditor<SigningConfig>
    {
        public SigningConfigEditor() : base("Code Signing", "eos_plugin_signing_config.json") { }

        [MenuItem("Tools/EOS Plugin/Sign DLLs")]
        static void SignAllDLLs()
        {
            var signTool = new SigningConfigEditor();
            signTool.Load();

            // stop if there are no dlls to sign
            if (signTool.GetConfig().Data.dllPaths == null)
            {
                return;
            }

            foreach (var dllPath in signTool.GetConfig().Data.dllPaths)
            {
                SignDLL(signTool.GetConfig().Data, dllPath);
            }
        }

        [MenuItem("Tools/EOS Plugin/Sign DLLs", true)]
        static bool CanSignDLLs()
        {
#if UNITY_EDITOR_WIN
            var configSection = new SigningConfigEditor();
            configSection.Load();

            return (configSection.GetConfig().Data.dllPaths != null);
#else
            return false;
#endif
        }

        static void SignDLL(SigningConfig config, string dllPath)
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
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.Log(e.Data);
                }
            });

            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
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

        public override void RenderContents()
        {
            string pathToSigntool = (ConfigHandler.Data.pathToSignTool ?? "");
            string pathToPFX = (ConfigHandler.Data.pathToPFX ?? "");
            string pfxPassword = (ConfigHandler.Data.pfxPassword ?? "");
            string timestampURL = (ConfigHandler.Data.timestampURL ?? "");
            GUIEditorHelper.AssigningPath("Path to SignTool", ref pathToSigntool, "Select SignTool", extension: "exe");
            GUIEditorHelper.AssigningPath("Path to PFX key", ref pathToPFX, "Select PFX key", extension: "pfx");
            GUIEditorHelper.AssigningTextField("PFX password", ref pfxPassword);
            GUIEditorHelper.AssigningTextField("Timestamp Authority URL", ref timestampURL);

            if (ConfigHandler.Data.dllPaths == null)
            {
                ConfigHandler.Data.dllPaths = new List<string>();
            }
            EditorGUILayout.LabelField("Target DLL Paths");
            for (int i = 0; i < ConfigHandler.Data.dllPaths.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                string dllPath = (ConfigHandler.Data.dllPaths[i]);
                GUIEditorHelper.AssigningTextField("", ref dllPath);
                ConfigHandler.Data.dllPaths[i] = dllPath;
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(100)))
                {
                    ConfigHandler.Data.dllPaths.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                ConfigHandler.Data.dllPaths.Add("");
            }

            ConfigHandler.Data.pathToSignTool = pathToSigntool;
            ConfigHandler.Data.pathToPFX = pathToPFX;
            ConfigHandler.Data.pfxPassword = pfxPassword;
            ConfigHandler.Data.timestampURL = timestampURL.Trim();
        }
    }
}
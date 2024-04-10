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

namespace PlayEveryWare.EpicOnlineServices.Editor
{
    using Config;
    using System.Threading.Tasks;
    using Utility;

    public class SigningConfigEditor : ConfigEditor<SigningConfig>
    {
        public SigningConfigEditor() : base("Code Signing") { }

        [MenuItem("Tools/EOS Plugin/Sign DLLs")]
        static async Task SignAllDLLs()
        {
            var signConfig = await EpicOnlineServices.Config.GetAsync<SigningConfig>();

            // stop if there are no dlls to sign
            if (signConfig.dllPaths == null)
            {
                return;
            }

            foreach (var dllPath in signConfig.dllPaths)
            {
                SignDLL(signConfig, dllPath);
            }
        }

        [MenuItem("Tools/EOS Plugin/Sign DLLs", true)]
        static bool CanSignDLLs()
        {
#if UNITY_EDITOR_WIN
            var signConfig = EpicOnlineServices.Config.Get<SigningConfig>();
            return (signConfig.dllPaths != null);
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
            string pathToSigntool = (config.pathToSignTool ?? "");
            string pathToPFX = (config.pathToPFX ?? "");
            string pfxPassword = (config.pfxPassword ?? "");
            string timestampURL = (config.timestampURL ?? "");
            GUIEditorUtility.AssigningPath("Path to SignTool", ref pathToSigntool, "Select SignTool", extension: "exe");
            GUIEditorUtility.AssigningPath("Path to PFX key", ref pathToPFX, "Select PFX key", extension: "pfx");
            GUIEditorUtility.AssigningTextField("PFX password", ref pfxPassword);
            GUIEditorUtility.AssigningTextField("Timestamp Authority URL", ref timestampURL);

            if (config.dllPaths == null)
            {
                config.dllPaths = new List<string>();
            }
            EditorGUILayout.LabelField("Target DLL Paths");
            for (int i = 0; i < config.dllPaths.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                string dllPath = (config.dllPaths[i]);
                GUIEditorUtility.AssigningTextField("", ref dllPath);
                config.dllPaths[i] = dllPath;
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(100)))
                {
                    config.dllPaths.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                config.dllPaths.Add("");
            }

            config.pathToSignTool = pathToSigntool;
            config.pathToPFX = pathToPFX;
            config.pfxPassword = pfxPassword;
            config.timestampURL = timestampURL.Trim();
        }
    }
}
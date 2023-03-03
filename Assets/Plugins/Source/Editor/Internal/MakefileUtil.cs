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

using UnityEngine;
using UnityEditor;
using System.IO;

namespace PlayEveryWare.EpicOnlineServices
{
    public static class MakefileUtil
    {
        [MenuItem("Tools/Build Libraries/Win32")]
        public static void BuildLibrariesWin32()
        {
#if UNITY_EDITOR_WIN
            BuildWindows("x86");
#endif
        }

        [MenuItem("Tools/Build Libraries/Win64")]
        public static void BuildLibrariesWin64()
        {
#if UNITY_EDITOR_WIN
            BuildWindows("x64");
#endif
        }

        [MenuItem("Tools/Build Libraries/Win32", true)]
        [MenuItem("Tools/Build Libraries/Win64", true)]
        public static bool CanBuildLibrariesWindows()
        {
#if UNITY_EDITOR_WIN
            return true;
#else
            return false;
#endif
        }

        private static void BuildWindows(string platform)
        {
            if (RunProcess("where", "msbuild /q") != 0)
            {
                //msbuild must be in PATH
                Debug.LogError("msbuild not found");
            }
            else
            {
                RunProcess("msbuild", $"DynamicLibraryLoaderHelper.sln /p:Configuration=Release /p:Platform={platform}");
            }
        }

        private static int RunProcess(string processPath, string arguments)
        {
            var procInfo = new System.Diagnostics.ProcessStartInfo()
            {
                Arguments = arguments
            };
            procInfo.FileName = processPath;
            procInfo.UseShellExecute = false;
            procInfo.WorkingDirectory = Path.Join(Application.dataPath, "../NativeCode/DynamicLibraryLoaderHelper");
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
            int exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        }
    }
}
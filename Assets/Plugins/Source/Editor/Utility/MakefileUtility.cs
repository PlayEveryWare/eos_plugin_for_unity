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

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using Config;
    using System.Threading.Tasks;
    using Config = EpicOnlineServices.Config;
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Text.RegularExpressions;

    [InitializeOnLoad]
    public static partial class MakefileUtility
    {
        private static Regex WarningRegex;
        private static Regex ErrorRegex;

        static MakefileUtility()
        {
            WarningRegex = new Regex(@"warning [a-zA-z0-9]*:", RegexOptions.IgnoreCase);
            ErrorRegex = new Regex(@"error [a-zA-z0-9]*:", RegexOptions.IgnoreCase);
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Win32")]
        public static void BuildLibrariesWin32()
        {
#if UNITY_EDITOR_WIN
            (new WindowsBuilder32()).BuildNativeCode();
#endif
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Win64")]
        public static void BuildLibrariesWin64()
        {
#if UNITY_EDITOR_WIN
            (new WindowsBuilder64()).BuildNativeCode();
#endif
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Win32", true)]
        [MenuItem("Tools/EOS Plugin/Build Libraries/Win64", true)]
        public static bool CanBuildLibrariesWindows()
        {
#if UNITY_EDITOR_WIN
            return true;
#else
            return false;
#endif
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Mac")]
        public static void BuildLibrariesMac()
        {
#if UNITY_EDITOR_OSX
            Task.Run(BuildMac);
#endif
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Mac", true)]
        public static bool CanBuildLibrariesMac()
        {
#if UNITY_EDITOR_OSX
            return true;
#else
            return false;
#endif
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Linux")]
        public static void BuildLibrariesLinux()
        {
#if UNITY_EDITOR_LINUX
            Task.Run(BuildLinux);
#endif
        }

        [MenuItem("Tools/EOS Plugin/Build Libraries/Linux", true)]
        public static bool CanBuildLibrariesLinux()
        {
#if UNITY_EDITOR_LINUX
            return true;
#else
            return false;
#endif
        }

        private static async Task<string> GetMakePath()
        {
            var libraryConfig = await Config.GetAsync<LibraryBuildConfig>();

            if (!string.IsNullOrWhiteSpace(libraryConfig.makePath))
            {
                return libraryConfig.makePath;
            }
            else if (RunProcess("which", "make", printOutput: false, printError: false) != 0)
            {
                return "make";
            }
            else
            {
                return null;
            }
        }

        private static async Task RunMake(string makefileDir)
        {
            string makePath = await GetMakePath();
            if (string.IsNullOrWhiteSpace(makePath))
            {
                Debug.LogError("make command not found");
            }
            else
            {
                RunProcess(makePath, "install", makefileDir);
            }
        }

        private static async Task BuildMac()
        {
            await RunMake("lib/NativeCode/DynamicLibraryLoaderHelper_macOS");
        }

        private static async Task BuildLinux()
        {
            await RunMake("lib/NativeCode/DynamicLibraryLoaderHelper_Linux");
        }

        private static int RunProcess(string processPath, string arguments, string workingDir = "", bool printOutput = true, bool printError = true)
        {
            var procInfo = new System.Diagnostics.ProcessStartInfo()
            {
                Arguments = arguments
            };
            procInfo.FileName = processPath;
            procInfo.UseShellExecute = false;
            procInfo.WorkingDirectory = Path.Combine(Application.dataPath, "..", workingDir);
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            var process = new System.Diagnostics.Process { StartInfo = procInfo };
            if (printOutput)
            {
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                    {
                        return;
                    }

                    if (ErrorRegex.IsMatch(e.Data))
                    {
                        Debug.LogError(e.Data);
                    }
                    else if (WarningRegex.IsMatch(e.Data))
                    {
                        Debug.LogWarning(e.Data);
                    }
                    else
                    {
                        Debug.Log(e.Data);
                    }
                });
            }

            if (printError)
            {
                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.LogError(e.Data);
                    }
                });
            }

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
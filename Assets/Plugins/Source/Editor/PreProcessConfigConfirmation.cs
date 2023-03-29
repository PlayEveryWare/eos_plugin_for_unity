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

using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

class PreProcessConfigConfirmation : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return int.MaxValue; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        if (EOSPreprocessUtilities.isEOSDisableScriptingDefineEnabled(report))
        {
            return;
        }

        Debug.Log("PreProcessConfigConfirmation.OnPreprocessBuild for target: " + report.summary.platform);
        BuildTarget target = report.summary.platform;

        Dictionary<BuildTarget, string> fileDictionary = new Dictionary<BuildTarget, string>();
        fileDictionary.Add(BuildTarget.StandaloneWindows,   "Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json");
        fileDictionary.Add(BuildTarget.StandaloneWindows64, "Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json");
        fileDictionary.Add(BuildTarget.iOS,                 "Assets/StreamingAssets/EOS/eos_ios_config.json");
        fileDictionary.Add(BuildTarget.StandaloneOSX,       "Assets/StreamingAssets/EOS/eos_macos_config.json");
        fileDictionary.Add(BuildTarget.StandaloneLinux64,   "Assets/StreamingAssets/EOS/eos_linux_config.json");
        fileDictionary.Add(BuildTarget.Android,             "Assets/StreamingAssets/EOS/eos_android_config.json");
        /* Unupported cases(Playstation and XBox)
        fileDictionary.Add(BuildTarget.PS4,             "Assets/StreamingAssets/EOS/.json");
        fileDictionary.Add(BuildTarget.PS5,             "Assets/StreamingAssets/EOS/.json");
        fileDictionary.Add(BuildTarget.Xbox,             "Assets/StreamingAssets/EOS/.json");
        */

        if (fileDictionary.ContainsKey(target))
        {
            Debug.Log("PreProcessConfigConfirmation.OnPreprocessBuild for target: " + target + "Target is supported");
            if (System.IO.File.Exists(fileDictionary[target]))
            {
                Debug.Log("PreProcessConfigConfirmation.OnPreprocessBuild for target: " + target + " config file exists");
            }
            else
            {
                Debug.LogError("PreProcessConfigConfirmation.OnPreprocessBuild for target: " + target + " config file is missing");
                throw new BuildFailedException("Config file for " + target + " is missing");
            }
        }
        else
        {
            Debug.LogError("PreProcessConfigConfirmation.OnPreprocessBuild for target: " + target + ". Target is not supported");
            //throw new BuildFailedException( target + " is not supported, refer to above message");
        }
    }
}

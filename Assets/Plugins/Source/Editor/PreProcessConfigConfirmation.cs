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

class PreProcessConfigConfirmation : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("PreProcessConfigConfirmation_Windows.OnPreprocessBuild for target " + report.summary.platform + " at path " + System.IO.Directory.GetCurrentDirectory());
        BuildTarget target = report.summary.platform;

        switch (target)
        {
            case BuildTarget.StandaloneWindows:

                if(System.IO.File.Exists("Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            case BuildTarget.StandaloneWindows64:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            case BuildTarget.iOS:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_ios_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            case BuildTarget.StandaloneOSX:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_macos_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            case BuildTarget.StandaloneLinux64:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_linux_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            case BuildTarget.Android:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_android_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            /*   Unupported cases(Playstation and XBox
            case BuildTarget.PS4:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_android_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            case BuildTarget.PS5:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_android_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;

            //Xbox case
            case BuildTarget.:
                if (System.IO.File.Exists("Assets/StreamingAssets/EOS/eos_android_config.json"))
                {
                    configFileExist(target);
                }
                else
                {
                    configFileDoesNotExist(target);
                }
                break;*/
        }

    }

    private void configFileExist(BuildTarget target)
    {
        Debug.Log("PreProcessConfigConfirmation_Windows.OnPreprocessBuild for target " + target + " config file exists");
    }

    private void configFileDoesNotExist(BuildTarget target)
    {
        Debug.LogError("PreProcessConfigConfirmation_Windows.OnPreprocessBuild for target " + target + " config file is missing");
        throw new BuildFailedException("Config file is missing, refer to above message");
    }

}

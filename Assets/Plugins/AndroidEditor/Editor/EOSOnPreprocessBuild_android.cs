/*
* Copyright (c) 2022 PlayEveryWare
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

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;

public class EOSOnPreprocessBuild_android : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 3; } }

    //-------------------------------------------------------------------------
    private static string GetAndroidEOSValuesConfigPath()
    {
        string assetsPathname = Path.Combine(Application.dataPath, "Plugins/Android/EOS/");
        return Path.Combine(assetsPathname, "eos_dependencies.androidlib/res/values/eos_values.xml");
    }

    //-------------------------------------------------------------------------
    private static string GetPackageName()
    {
        return "com.playeveryware.eos";
    }


    //-------------------------------------------------------------------------
    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
        {
            InstallEOSDependentLibrary();
            ConfigureEOSDependentLibrary();
        }
    }

    //-------------------------------------------------------------------------
    static private void OverwriteCopy(string fileToInstallPathName, string destPathname)
    {
        if (File.Exists(destPathname))
        {
            File.SetAttributes(destPathname, File.GetAttributes(destPathname) & ~FileAttributes.ReadOnly);
        }

        File.Copy(fileToInstallPathName, destPathname, true);
    }

    //-------------------------------------------------------------------------
    static private void InstallFiles(string[] filenames,  string pathToInstallFrom, string pathToInstallTo)
    {

        if (!EmptyPredicates.IsEmptyOrNull(pathToInstallFrom))
        {
            foreach (var fileToInstall in filenames)
            {
                string fileToInstallPathName = Path.Combine(pathToInstallFrom, fileToInstall);

                if (File.Exists(fileToInstallPathName))
                {
                    string fileToInstallParentDirectory = Path.GetDirectoryName(Path.Combine(pathToInstallTo, fileToInstall));

                    if (!Directory.Exists(fileToInstallParentDirectory))
                    {
                        Directory.CreateDirectory(fileToInstallParentDirectory);
                    }
                    string destPathname = Path.Combine(fileToInstallParentDirectory, Path.GetFileName(fileToInstallPathName));

                    OverwriteCopy(fileToInstallPathName, destPathname);
                }
                else
                {
                    Debug.LogError("Missing platform specific file: " + fileToInstall);
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    private string GetPlatformSpecificAssetsPath(string subpath)
    {
        string packagePathname = Path.GetFullPath("Packages/" + GetPackageName() + "/PlatformSpecificAssets~/" + subpath);
        string streamingAssetsSamplesPathname = Path.Combine(Application.dataPath, "../PlatformSpecificAssets/" + subpath);
        string pathToInstallFrom = "";

        if (Directory.Exists(packagePathname))
        {
            // Install from package path
            pathToInstallFrom = packagePathname;
        }
        else if (Directory.Exists(streamingAssetsSamplesPathname))
        {
            pathToInstallFrom = streamingAssetsSamplesPathname;
        }
        return pathToInstallFrom;
    }

    //-------------------------------------------------------------------------
    bool DoesGradlePropertiesContainSetting(string gradleTemplatePathname, string setting)
    {
        // check if it contains the android.useAndroidX=true
        foreach (string line in File.ReadAllLines(gradleTemplatePathname))
        {
            if (line.Contains(setting) && !line.StartsWith("#"))
            {
                return true;
            }
        }
        return false;
    }

    //-------------------------------------------------------------------------
    void ReplaceOrSetGradleProperty(string gradleTemplatePathname, string setting, string value)
    {
        var gradleTemplateToWrite = new List<string>();
        bool wasAdded = false;

        foreach(string line in File.ReadAllLines(gradleTemplatePathname))
        {
            if (line.Contains(setting) && !line.StartsWith("#"))
            {
                gradleTemplateToWrite.Add($"{setting}={value}");
                wasAdded = true;
            }
            else
            {
                gradleTemplateToWrite.Add(line);
            }
        }

        if (!wasAdded)
        {
            gradleTemplateToWrite.Add($"{setting}={value}");
        }
        File.WriteAllLines(gradleTemplatePathname, gradleTemplateToWrite.ToArray());
    }

    //-------------------------------------------------------------------------
    public void InstallEOSDependentLibrary()
    {
        string packagedPathname = GetPlatformSpecificAssetsPath("EOS/Android/");

        if (Directory.Exists(packagedPathname))
        {
            string assetsPathname = Path.Combine(Application.dataPath, "Plugins/Android/EOS/");
            string[] filenames =
            {
                "eos_dependencies.androidlib/AndroidManifest.xml",
                "eos_dependencies.androidlib/build.gradle",
                "eos_dependencies.androidlib/project.properties",
                "eos_dependencies.androidlib/res/values/eos_values.xml",
                "eos_dependencies.androidlib/res/values/styles.xml"

            };
            InstallFiles(filenames, packagedPathname, assetsPathname);

            // Unity has a fixed location for the gradleTemplate.properties file. (as of 2021)
            string gradleTemplatePathname = Path.Combine(Application.dataPath, "Plugins/Android/gradleTemplate.properties");
            if (File.Exists(gradleTemplatePathname))
            {
                if (!DoesGradlePropertiesContainSetting(gradleTemplatePathname, "android.useAndroidX=true"))
                {
                    ReplaceOrSetGradleProperty(gradleTemplatePathname, "android.useAndroidX", "true");
                }
            }
            else
            {
                // Use one we have bundled
                string bundledGradleTemplatePathname = GetPlatformSpecificAssetsPath("EOS/Android/gradleTemplate.properties");
                File.Copy(bundledGradleTemplatePathname, gradleTemplatePathname);
            }
        }
    }

    //-------------------------------------------------------------------------
    public void ConfigureEOSDependentLibrary()
    {
        string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSManager.ConfigFileName);
        var eosConfigFile = new EOSConfigFile<EOSConfig>(configFilePath);
        eosConfigFile.LoadConfigFromDisk();
        string clientIDAsLower = eosConfigFile.currentEOSConfig.clientID.ToLower();

        var pathToEOSValuesConfig = GetAndroidEOSValuesConfigPath();
        var currentEOSValuesConfigAsXML = new System.Xml.XmlDocument();
        currentEOSValuesConfigAsXML.Load(pathToEOSValuesConfig);

        var node = currentEOSValuesConfigAsXML.DocumentElement.SelectSingleNode("/resources");

        if (node != null)
        {
            string eosProtocolScheme = node.InnerText;
            string storedClientID = eosProtocolScheme.Split(".").Last();

            if (storedClientID != clientIDAsLower)
            {
                node.InnerText = $"eos.{clientIDAsLower}";
                currentEOSValuesConfigAsXML.Save(pathToEOSValuesConfig);
            }
        }
    }

}
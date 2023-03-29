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
using UnityEditor.Android;
using UnityEngine;
using System.IO;
using System.Linq;
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_ANDROID
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
    public void OnPreprocessBuild(BuildReport report)
    {
        if (EOSPreprocessUtilities.isEOSDisableScriptingDefineEnabled(report))
        {
            return;
        }

        if (report.summary.platform == BuildTarget.Android)
        {
            InstallEOSDependentLibrary();
            ConfigureGradleTemplateProperties();
            ConfigureEOSDependentLibrary();

            DetermineLibraryLinkingMethod();
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
        string packagePathname = Path.GetFullPath(Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "PlatformSpecificAssets~", subpath));
        string streamingAssetsSamplesPathname = Path.Combine(Application.dataPath, "..", "PlatformSpecificAssets", subpath);
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
        else
        {
            Debug.LogError("PreprocessBuildError : EOS Plugin Package Missing");
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
    void DisableGradleProperty(string gradleTemplatePathname, string setting)
    {
        var gradleTemplateToWrite = new List<string>();
        
        foreach (string line in File.ReadAllLines(gradleTemplatePathname))
        {
            if (line.Contains(setting) && !line.StartsWith("#"))
            {
            }
            else
            {
                gradleTemplateToWrite.Add(line);
            }
        }
        File.WriteAllLines(gradleTemplatePathname, gradleTemplateToWrite.ToArray());
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
    int GetTargetAPI()
    {
        var playerApiTarget = PlayerSettings.Android.targetSdkVersion;
        if (playerApiTarget == AndroidSdkVersions.AndroidApiLevelAuto)
        {
            int maxVersion = 0;
            var apiRegex = new Regex(@"android-(\d+)");
            //find max installed android api
            foreach (var dir in Directory.GetDirectories(Path.Combine(AndroidExternalToolsSettings.sdkRootPath, "platforms")))
            {
                var dirName = Path.GetFileName(dir);
                var match = apiRegex.Match(dirName);
                if (match.Success && match.Groups.Count == 2)
                {
                    if (int.TryParse(match.Groups[1].Value, out int matchVal))
                    {
                        if (matchVal > maxVersion)
                        {
                            maxVersion = matchVal;
                        }
                    }
                }
            }
            if (maxVersion == 0)
            {
                return 29;
            }
            return maxVersion;
        }
        else
        {
            return (int)playerApiTarget;
        }
    }

    //-------------------------------------------------------------------------
    string GetBuildTools()
    {
        var toolsRegex = new Regex(@"(\d+)\.(\d+)\.(\d+)");
        int maxMajor = 0, maxMinor = 0, maxPatch = 0;
        //find highest usable build tools version
#if UNITY_2022_2_OR_NEWER
        const int highestVersion = 32;
#else
        const int highestVersion = 30;
#endif
        foreach (var dir in Directory.GetDirectories(Path.Combine(AndroidExternalToolsSettings.sdkRootPath, "build-tools")))
        {
            var dirName = Path.GetFileName(dir);
            var match = toolsRegex.Match(dirName);
            int majorVersion = 0, minorVersion = 0, patchVersion = 0;
            bool success = match.Success && match.Groups.Count == 4 &&
                int.TryParse(match.Groups[1].Value, out majorVersion) &&
                int.TryParse(match.Groups[2].Value, out minorVersion) &&
                int.TryParse(match.Groups[3].Value, out patchVersion);
            if (success)
            {
                if (majorVersion > highestVersion)
                {
                    continue;
                }
                if (majorVersion > maxMajor ||
                    (majorVersion == maxMajor && minorVersion > maxMinor) ||
                    (majorVersion == maxMajor && minorVersion == maxMinor && patchVersion > maxPatch))
                {
                    maxMajor = majorVersion;
                    maxMinor = minorVersion;
                    maxPatch = patchVersion;
                }
            }
        }
        if (maxMajor == 0)
        {
            return "30.0.3";
        }
        else
        {
            return $"{maxMajor}.{maxMinor}.{maxPatch}";
        }
    }
    //-------------------------------------------------------------------------
    void WriteConfigMacros(string filepath)
    {
        var contents = File.ReadAllText(filepath);
        string apiVersion = GetTargetAPI().ToString();
        string buildTools = GetBuildTools();
        string newContents = contents.Replace("**APIVERSION**", apiVersion).Replace("**TARGETSDKVERSION**", apiVersion).Replace("**BUILDTOOLS**", buildTools);
        File.WriteAllText(filepath, newContents);
    }

    //-------------------------------------------------------------------------
    public void InstallEOSDependentLibrary()
    {
        string packagedPathname = GetPlatformSpecificAssetsPath("EOS/Android/");

        if (Directory.Exists(packagedPathname))
        {
            string assetsPathname = Path.Combine(Application.dataPath, "Plugins", "Android", "EOS");
            string buildGradlePath = "eos_dependencies.androidlib/build.gradle";
            string[] filenames =
            {
                "eos_dependencies.androidlib/AndroidManifest.xml",
                buildGradlePath,
                "eos_dependencies.androidlib/project.properties",
                "eos_dependencies.androidlib/res/values/eos_values.xml",
                "eos_dependencies.androidlib/res/values/styles.xml"

            };
            InstallFiles(filenames, packagedPathname, assetsPathname);

            WriteConfigMacros(Path.Combine(assetsPathname, buildGradlePath));
        }
    }
    //-------------------------------------------------------------------------
    public void ConfigureGradleTemplateProperties()
    {   
        // Unity has a fixed location for the gradleTemplate.properties file. (as of 2021)
        string gradleTemplatePathname = Path.Combine(Application.dataPath, "Plugins", "Android", "gradleTemplate.properties");

        // If the custom gradle template properties option is disabled, delete gradleTemplate.properties.DISABLED
        File.Delete(gradleTemplatePathname + ".DISABLED");

        // Then create a copy of gradleTemplate.properties in the target folder
        // Once gradleTemplate.properties file exists, the custom gradle template properties option is automatically enabled 
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
            string bundledGradleTemplatePathname = Path.Combine(GetPlatformSpecificAssetsPath("EOS/Android/"), "gradleTemplate.properties");
            File.Copy(bundledGradleTemplatePathname, gradleTemplatePathname);
        }
#if UNITY_2022_2_OR_NEWER
        DisableGradleProperty(gradleTemplatePathname, "android.enableR8");
#endif
    }
    //-------------------------------------------------------------------------
    public void ConfigureEOSDependentLibrary()
    {
        string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSPackageInfo.ConfigFileName);
        var eosConfigFile = new EOSConfigFile<EOSConfig>(configFilePath);
        eosConfigFile.LoadConfigFromDisk();
        string clientIDAsLower = eosConfigFile.currentEOSConfig.clientID.ToLower();

        var pathToEOSValuesConfig = GetAndroidEOSValuesConfigPath();
        var currentEOSValuesConfigAsXML = new System.Xml.XmlDocument();
        currentEOSValuesConfigAsXML.Load(pathToEOSValuesConfig);

        var node = currentEOSValuesConfigAsXML.DocumentElement.SelectSingleNode("/resources");
        if (node == null) { return; }

        var node2 = node.SelectSingleNode("string[@name=\"eos_login_protocol_scheme\"]");
        if (node2 == null) { return; }

        string eosProtocolScheme = node2.InnerText;
        string storedClientID = eosProtocolScheme.Split('.').Last();

        if (storedClientID != clientIDAsLower)
        {
            node2.InnerText = $"eos.{clientIDAsLower}";
            currentEOSValuesConfigAsXML.Save(pathToEOSValuesConfig);
        }
    }

    private void DetermineLibraryLinkingMethod()
    {
        var androidBuildConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorAndroidBuildConfigSection>();
        androidBuildConfigSection?.Awake();
        if (androidBuildConfigSection != null)
        {
            if (androidBuildConfigSection.GetCurrentConfig() == null)
            {
                androidBuildConfigSection.LoadConfigFromDisk();
            }
        }

        string packagePath = Path.GetFullPath("Packages/" + EOSPackageInfo.GetPackageName() + "/PlatformSpecificAssets~/EOS/Android/");
        string androidAssetFilepath = Path.Combine(Application.dataPath, "../PlatformSpecificAssets/EOS/Android/");

        string sourcePath = Path.Combine(
            Directory.Exists(packagePath) ? packagePath : androidAssetFilepath,   //From Package or From Assets(EOS Plugin Repo)
            androidBuildConfigSection.GetCurrentConfig().DynamicallyLinkEOSLibrary ? "dynamic-stdc++" : "static-stdc++", //Dynamic or Static
            "aar");
        string destPath = "Assets/Plugins/Android";

        CopyFromSourceToPluginFolder_Android(sourcePath, "eos-sdk.aar", destPath);
        CopyFromSourceToPluginFolder_Android(sourcePath, "eos-sdk.aar.meta", destPath);
    }

    private void CopyFromSourceToPluginFolder_Android(string sourcePath, string filename, string destPath)
    {
        File.Copy(Path.Combine(sourcePath, filename),
           Path.Combine(destPath, filename), true);
    }
}
#endif
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
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;
using System;

public class EOSOnPostprocessBuild_Windows:  IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    private string[] postBuildFiles = {
    };

    private string[] postBuildFilesEAC = {
        "EACLauncher.exe",
        //optional override config file for EAC CDN
        "[ExeName].eac",
        "EasyAntiCheat/EasyAntiCheat_EOS_Setup.exe",
        "EasyAntiCheat/Settings.json",
        "EasyAntiCheat/SplashScreen.png"
    };

    private string[] postBuildDirectories = {
    };

    private string[] postBuildDirectoriesEAC = {
        "EasyAntiCheat/Licenses",
        "EasyAntiCheat/Localization"
    };

    private HashSet<string> postBuildFilesOptional = new HashSet<string>(){
        "[ExeName].eac",
    };

    //files with contents that need string vars replaced
    private HashSet<string> postBuildFilesWithVars = new HashSet<string>(){
        "EasyAntiCheat/Settings.json"
    };

    private EOSConfig eosConfig = null;
    private string buildExeName = null;

    //-------------------------------------------------------------------------
    private static string GetPathToEOSBin()
    {
        string projectPathToBin = Path.Combine(Application.dataPath, "../bin/");
        string packagePathToBin = Path.GetFullPath("Packages/" + EOSPackageInfo.GetPackageName() + "/bin~/");

        if (Directory.Exists(packagePathToBin))
        {
            return packagePathToBin;
        }
        else if (Directory.Exists(projectPathToBin))
        {
            return projectPathToBin;
        }
        return "";
    }

    //-------------------------------------------------------------------------
    private static string GetPathToPlatformSepecificAssetsForWindows()
    {
        string packagePathname = Path.GetFullPath("Packages/" + EOSPackageInfo.GetPackageName() + "/PlatformSpecificAssets~/EOS/Windows/");
        string platformSpecificPathname = Path.Combine(Application.dataPath, "../PlatformSpecificAssets/EOS/Windows/");
        string pathToInstallFrom = "";
        // If the Plugin is installed with StreamAssets, install them
        if (Directory.Exists(packagePathname))
        {
            // Install from package path
            pathToInstallFrom = packagePathname;
        }
        else if (Directory.Exists(platformSpecificPathname))
        {
            pathToInstallFrom = platformSpecificPathname;
        }

        return pathToInstallFrom;
    }

    //-------------------------------------------------------------------------
    private static void InstallBootStrapper(BuildReport report, string pathToEOSBootStrapperTool, string bootstrapperFileName)
    {
        string appFilenameExe = Path.GetFileName(report.summary.outputPath);
        string installDirectory = Path.GetDirectoryName(report.summary.outputPath);
        string installPathForEOSBootStrapper = Path.Combine(installDirectory, bootstrapperFileName);
        string workingDirectory = GetPathToEOSBin();
        string bootStrapperArgs = ""
           + " --output-path " + "\"" + installPathForEOSBootStrapper + "\""
           + " --app-path "  + "\"" + appFilenameExe + "\""
        ;

        var procInfo = new System.Diagnostics.ProcessStartInfo();
        procInfo.FileName = pathToEOSBootStrapperTool;
        procInfo.Arguments = bootStrapperArgs;
        procInfo.UseShellExecute = false;
        procInfo.WorkingDirectory = workingDirectory;
        procInfo.RedirectStandardOutput = true;
        procInfo.RedirectStandardError = true;

        var process = new System.Diagnostics.Process { StartInfo = procInfo };
        process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
            if(!EmptyPredicates.IsEmptyOrNull(e.Data))
            {
                Debug.Log(e.Data);
            }
        });

        process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>{
            if(!EmptyPredicates.IsEmptyOrNull(e.Data))
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

    //use anticheat_integritytool to hash protected files and generate certificate for EAC
    private static void GenerateIntegrityCert(BuildReport report, string pathToEACIntegrityTool, string productID, string keyFileName, string certFileName)
    {
        string installPathForExe = report.summary.outputPath;
        string installDirectory = Path.GetDirectoryName(installPathForExe);
        string toolDirectory = Path.GetDirectoryName(pathToEACIntegrityTool);
        string integrityToolArgs = string.Format("-productid {0} -inkey \"{1}\" -incert \"{2}\" -target_game_dir \"{3}\"", productID, keyFileName, certFileName, installDirectory);

        var procInfo = new System.Diagnostics.ProcessStartInfo();
        procInfo.FileName = pathToEACIntegrityTool;
        procInfo.Arguments = integrityToolArgs;
        procInfo.UseShellExecute = false;
        procInfo.WorkingDirectory = toolDirectory;
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

    public static string GetRelativePath(string relativeTo, string path)
    {
        var uri = new Uri(relativeTo);
        var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
        {
            rel = $".{ Path.DirectorySeparatorChar }{ rel }";
        }
        return rel;
    }

    //-------------------------------------------------------------------------
    private void InstallFiles(BuildReport report, bool useEAC)
    {
        string destDir = Path.GetDirectoryName(report.summary.outputPath);
        string pathToInstallFrom = GetPathToPlatformSepecificAssetsForWindows();

        List<string> filestoInstall = new List<string>(postBuildFiles);
        List<string> directoriesToInstall = new List<string>(postBuildDirectories);
        if (useEAC)
        {
            filestoInstall.AddRange(postBuildFilesEAC);
            directoriesToInstall.AddRange(postBuildDirectoriesEAC);
        }

        //add all files in postBuildDirectories to list of files to copy (non-recursive)
        foreach (string directoryToInstall in directoriesToInstall)
        {
            string dirToInstallPathName = Path.Combine(pathToInstallFrom, directoryToInstall);
            if (Directory.Exists(dirToInstallPathName))
            {
                var dirFiles = Directory.GetFiles(dirToInstallPathName);
                foreach (string dirFile in dirFiles)
                {
                    string relativePath = GetRelativePath(pathToInstallFrom, dirFile);
                    filestoInstall.Add(relativePath);
                }
            }
        }

        if (!EmptyPredicates.IsEmptyOrNull(pathToInstallFrom))
        {
            foreach (var fileToInstall in filestoInstall)
            {
                string fileToInstallPathName = Path.Combine(pathToInstallFrom, fileToInstall);

                if (File.Exists(fileToInstallPathName))
                {
                    string fileToInstallParentDirectory = Path.GetDirectoryName(Path.Combine(destDir, fileToInstall));

                    if (!Directory.Exists(fileToInstallParentDirectory))
                    {
                        Directory.CreateDirectory(fileToInstallParentDirectory);
                    }
                    string destPathname = Path.Combine(fileToInstallParentDirectory, Path.GetFileName(fileToInstallPathName));

                    destPathname = ReplaceFileNameVars(destPathname);

                    if (File.Exists(destPathname))
                    {
                        File.SetAttributes(destPathname, File.GetAttributes(destPathname) & ~FileAttributes.ReadOnly);
                    }

                    File.Copy(fileToInstallPathName, destPathname, true);

                    if (postBuildFilesWithVars.Contains(fileToInstall))
                    {
                        ReplaceFileContentVars(destPathname);
                    }
                }
                else if(!postBuildFilesOptional.Contains(fileToInstall))
                {
                    Debug.LogError("Missing platform specific file: " + fileToInstall);
                }
            }
        }
    }

    private EOSConfig GetEOSConfig()
    {
        if (eosConfig != null)
        {
            return eosConfig;
        }

        string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSPackageInfo.ConfigFileName);
        var configDataAsString = File.ReadAllText(configFilePath);
        var configData = JsonUtility.FromJson<EOSConfig>(configDataAsString);
        eosConfig = configData;
        return configData;
    }

    private string ReplaceFileNameVars(string filename)
    {
        filename = filename.Replace("[UnityProductName]", Application.productName);
        filename = filename.Replace("[ExeName]", buildExeName);
        return filename;
    }

    private void ReplaceFileContentVars(string filepath)
    {
        var fileContents = File.ReadAllText(filepath);
        EOSConfig eosConfig = GetEOSConfig();

        var sb = new System.Text.StringBuilder(fileContents);

        sb.Replace("<UnityProductName>", Application.productName);
        sb.Replace("<ExeName>", buildExeName);
        sb.Replace("<ProductName>", eosConfig.productName);
        sb.Replace("<ProductID>", eosConfig.productID);
        sb.Replace("<SandboxID>", eosConfig.sandboxID);
        sb.Replace("<DeploymentID>", eosConfig.deploymentID);

        fileContents = sb.ToString();

        File.WriteAllText(filepath, fileContents);
    }

    //-------------------------------------------------------------------------
    public void OnPostprocessBuild(BuildReport report)
    {
        // Get the output path, and install the launcher if on a target that supports it
        if (report.summary.platform == BuildTarget.StandaloneWindows || report.summary.platform == BuildTarget.StandaloneWindows64)
        {
            var editorToolsConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorToolsConfigSection>();
            string bootstrapperName = "";
            bool useEAC = false;

            if (editorToolsConfigSection != null)
            {
                editorToolsConfigSection.Awake();
                editorToolsConfigSection.LoadConfigFromDisk();

                var editorToolConfig = editorToolsConfigSection.GetCurrentConfig();
                bootstrapperName = editorToolConfig.bootstrapperNameOverride;
                useEAC = editorToolConfig.useEAC;
            }

            if (string.IsNullOrWhiteSpace(bootstrapperName))
            {
                bootstrapperName = "EOSBootstrapper.exe";
            }

            if (!bootstrapperName.EndsWith(".exe"))
            {
                bootstrapperName += ".exe";
            }
            buildExeName = bootstrapperName;

            InstallFiles(report, useEAC);
            
            string pathToEOSBootStrapperTool = Path.Combine(GetPathToEOSBin(), "EOSBootstrapperTool.exe");
            
            InstallBootStrapper(report, pathToEOSBootStrapperTool, bootstrapperName);


            if (editorToolsConfigSection != null)
            {
                editorToolsConfigSection.Awake();
                editorToolsConfigSection.LoadConfigFromDisk();
                var editorToolConfig = editorToolsConfigSection.GetCurrentConfig();
                if (useEAC &&
                    editorToolConfig != null &&
                    editorToolConfig.pathToEACIntegrityTool != null &&
                    editorToolConfig.pathToEACPrivateKey != null &&
                    editorToolConfig.pathToEACCertificate != null)
                {
                    GenerateIntegrityCert(report, editorToolConfig.pathToEACIntegrityTool, GetEOSConfig().productID, editorToolConfig.pathToEACPrivateKey, editorToolConfig.pathToEACCertificate);
                }
            }
        }
    }
}

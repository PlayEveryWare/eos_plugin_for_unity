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

public class EOSOnPostprocessBuild_Standalone:  IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    private HashSet<string> postBuildFilesOptional = new HashSet<string>(){
        "[ExeName].eac",
        "EasyAntiCheat/SplashScreen.png"
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
    private static string GetPathToPlatformSpecificAssets(BuildReport report)
    {
        string platformDirectoryName = null;
        switch (report.summary.platform)
        {
            case BuildTarget.StandaloneLinux64:
                platformDirectoryName = "Linux";
                break;

            case BuildTarget.StandaloneOSX:
                platformDirectoryName = "Mac";
                break;

            default:
                platformDirectoryName = "Windows";
                break;
        }

        string packagePathname = Path.GetFullPath("Packages/" + EOSPackageInfo.GetPackageName() + "/PlatformSpecificAssets~/EOS/"+ platformDirectoryName + "/");
        string platformSpecificPathname = Path.Combine(Application.dataPath, "../PlatformSpecificAssets/EOS/"+ platformDirectoryName + "/");
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
#if UNITY_EDITOR_WIN
    private static void InstallBootStrapper(string appFilenameExe, string installDirectory, string pathToEOSBootStrapperTool, string bootstrapperFileName)
    {
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
#endif

    private string GetDefaultIntegrityToolPath()
    {
        string toolPath = Path.Combine(GetPathToEOSBin(), "EAC");
#if UNITY_EDITOR_WIN
        toolPath = Path.Combine(toolPath, "Windows", "anticheat_integritytool64.exe");
#elif UNITY_EDITOR_OSX
        toolPath = Path.Combine(toolPath, "Mac", "anticheat_integritytool");
#elif UNITY_EDITOR_LINUX
        toolPath = Path.Combine(toolPath, "Linux", "anticheat_integritytool");
#else
        toolPath = null;
#endif
        return toolPath;
    }

    private string GetDefaultIntegrityConfigPath()
    {
        string projectPathToCfg = Path.Combine(Application.dataPath, "Plugins/Standalone/Editor/anticheat_integritytool.cfg");
        string packagePathToCfg = Path.GetFullPath(Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "Editor/Standalone/anticheat_integritytool.cfg"));

        if (File.Exists(packagePathToCfg))
        {
            return packagePathToCfg;
        }
        else if (File.Exists(projectPathToCfg))
        {
            return projectPathToCfg;
        }
        return null;
    }

    //use anticheat_integritytool to hash protected files and generate certificate for EAC
    private void GenerateIntegrityCert(BuildReport report, string pathToEACIntegrityTool, string productID, string keyFileName, string certFileName, string configFile = null)
    {
        string installPathForExe = report.summary.outputPath;
        string installDirectory = Path.GetDirectoryName(installPathForExe);
        string toolDirectory = Path.GetDirectoryName(pathToEACIntegrityTool);

        string originalCfg = configFile;
        if (string.IsNullOrWhiteSpace(originalCfg))
        {
            originalCfg = Path.Combine(toolDirectory, "anticheat_integritytool.cfg");
        }

        string newCfgPath = Path.Combine(Application.temporaryCachePath, $"eac_integritytool_{Guid.NewGuid()}.cfg");
        File.Copy(originalCfg, newCfgPath, true);
        try
        {
            ReplaceFileContentVars(newCfgPath);
            configFile = newCfgPath;

            string integrityToolArgs = string.Format("-productid {0} -inkey \"{1}\" -incert \"{2}\" -target_game_dir \"{3}\"", productID, keyFileName, certFileName, installDirectory);
            if (!string.IsNullOrWhiteSpace(configFile))
            {
                integrityToolArgs += string.Format(" \"{0}\"", configFile);
            }

            var procInfo = new System.Diagnostics.ProcessStartInfo();
            procInfo.FileName = pathToEACIntegrityTool;
            procInfo.Arguments = integrityToolArgs;
            procInfo.UseShellExecute = false;
            procInfo.WorkingDirectory = toolDirectory;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            var process = new System.Diagnostics.Process { StartInfo = procInfo };
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>
            {
                if (!EmptyPredicates.IsEmptyOrNull(e.Data))
                {
                    if (e.Data.StartsWith("[Err!]"))
                    {
                        Debug.LogError(e.Data);
                    }
                    else if (e.Data.StartsWith("[Warn]"))
                    {
                        Debug.LogWarning(e.Data);
                    }
                    else
                    {
                        Debug.Log(e.Data);
                    }
                }
            });

            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>
            {
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
        finally
        {
            File.Delete(newCfgPath);
        }
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

    public static List<string> GetPostBuildFiles(BuildReport report, bool useEAC)
    {
        List<string> files = new List<string>();

        if (useEAC)
        {
            //optional override config file for EAC CDN
            files.Add("[ExeName].eac");
            files.Add("EasyAntiCheat/Settings.json");
            files.Add("EasyAntiCheat/SplashScreen.png");

            switch (report.summary.platform)
            {
                case BuildTarget.StandaloneLinux64:
                    files.Add("eac_launcher");
                    break;

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    files.Add("EACLauncher.exe");
                    files.Add("EasyAntiCheat/EasyAntiCheat_EOS_Setup.exe");
                    break;
            }
        }

        return files;
    }

    public static List<string> GetPostBuildDirectories(BuildReport report, bool useEAC)
    {
        List<string> directories = new List<string>();

        if (useEAC)
        {
            directories.Add("EasyAntiCheat/Licenses");
            directories.Add("EasyAntiCheat/Localization");

            switch (report.summary.platform)
            {
                case BuildTarget.StandaloneOSX:
                    directories.Add("eac_launcher.app/Contents");
                    directories.Add("eac_launcher.app/Contents/_CodeSignature");
                    directories.Add("eac_launcher.app/Contents/MacOS");
                    directories.Add("eac_launcher.app/Contents/Resources");
                    break;
            }
        }

        return directories;
    }

        //-------------------------------------------------------------------------
    private void InstallFiles(BuildReport report, bool useEAC)
    {
        string destDir = Path.GetDirectoryName(report.summary.outputPath);
        string pathToInstallFrom = GetPathToPlatformSpecificAssets(report);

        List<string> filestoInstall = GetPostBuildFiles(report, useEAC);
        List<string> directoriesToInstall = GetPostBuildDirectories(report, useEAC);

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

    private void CopySplashImage(BuildReport report, string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError("Specified EAC splash image not found");
            return;
        }

        string destPath = Path.Combine(Path.GetDirectoryName(report.summary.outputPath), "EasyAntiCheat/SplashScreen.png");
        if (File.Exists(destPath))
        {
            File.SetAttributes(destPath, File.GetAttributes(destPath) & ~FileAttributes.ReadOnly);
        }
        File.Copy(imagePath, destPath, true);
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
        sb.Replace("<ExeNameNoExt>", Path.GetFileNameWithoutExtension(buildExeName));
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
        if (EOSPreprocessUtilities.isEOSDisableScriptingDefineEnabled(report.summary.platform))
        {
            return;
        }

        // Get the output path, and install the launcher if on a target that supports it
        if (report.summary.platform == BuildTarget.StandaloneWindows ||
            report.summary.platform == BuildTarget.StandaloneWindows64 ||
            report.summary.platform == BuildTarget.StandaloneOSX ||
            report.summary.platform == BuildTarget.StandaloneLinux64)
        {
            var editorToolsConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorToolsConfigSection>();
            EOSPluginEditorToolsConfig editorToolConfig = null;
            
            bool useEAC = false;

            if (editorToolsConfigSection != null)
            {
                editorToolsConfigSection.Awake();
                editorToolsConfigSection.LoadConfigFromDisk();

                editorToolConfig = editorToolsConfigSection.GetCurrentConfig();
                if (editorToolConfig != null)
                {
                    useEAC = editorToolConfig.useEAC;
                }
            }
            
            buildExeName = Path.GetFileName(report.summary.outputPath);

            InstallFiles(report, useEAC);

            if (useEAC && !string.IsNullOrWhiteSpace(editorToolConfig.pathToEACSplashImage))
            {
                CopySplashImage(report, editorToolConfig.pathToEACSplashImage);
            }

#if UNITY_EDITOR_WIN
            if (report.summary.platform == BuildTarget.StandaloneWindows || report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                string bootstrapperName = null;
                if (editorToolConfig != null)
                {
                    bootstrapperName = editorToolConfig.bootstrapperNameOverride;
                }

                if (string.IsNullOrWhiteSpace(bootstrapperName))
                {
                    bootstrapperName = "EOSBootstrapper.exe";
                }

                if (!bootstrapperName.EndsWith(".exe"))
                {
                    bootstrapperName += ".exe";
                }

                string pathToEOSBootStrapperTool = Path.Combine(GetPathToEOSBin(), "EOSBootstrapperTool.exe");

                string installDirectory = Path.GetDirectoryName(report.summary.outputPath);

                string bootstrapperTarget = useEAC ? "EACLauncher.exe" : buildExeName;

                InstallBootStrapper(bootstrapperTarget, installDirectory, pathToEOSBootStrapperTool, bootstrapperName);
            }
#endif

            if (useEAC &&
                editorToolConfig != null &&
                !string.IsNullOrWhiteSpace(editorToolConfig.pathToEACPrivateKey) &&
                !string.IsNullOrWhiteSpace(editorToolConfig.pathToEACCertificate))
            {
                bool defaultTool = false;
                string toolPath = editorToolConfig.pathToEACIntegrityTool;
                if (string.IsNullOrWhiteSpace(toolPath))
                {
                    toolPath = GetDefaultIntegrityToolPath();
                    defaultTool = true;
                }
                string cfgPath = editorToolConfig.pathToEACIntegrityConfig;
                if (string.IsNullOrWhiteSpace(cfgPath) && defaultTool)
                {
                    //use default cfg if no cfg is specified and default tool path is used
                    cfgPath = GetDefaultIntegrityConfigPath();
                }
                if (!string.IsNullOrWhiteSpace(toolPath))
                {
                    GenerateIntegrityCert(report, toolPath, GetEOSConfig().productID, editorToolConfig.pathToEACPrivateKey, editorToolConfig.pathToEACCertificate, cfgPath);
                }
            }
        }
    }
}

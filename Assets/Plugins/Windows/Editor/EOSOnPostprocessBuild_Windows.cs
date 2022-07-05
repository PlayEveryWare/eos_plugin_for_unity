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
        "EACLauncher.exe",
        //optional override config file for EAC CDN
        "[UnityProductName].exe.eac",
        "EasyAntiCheat/EasyAntiCheat_EOS_Setup.exe",
        "EasyAntiCheat/Settings.json",
        "EasyAntiCheat/SplashScreen.png"
    };

    private string[] postBuildDirectories = {
        "EasyAntiCheat/Licenses",
        "EasyAntiCheat/Localization"
    };

    private HashSet<string> postBuildFilesOptional = new HashSet<string>(){
        "[UnityProductName].exe.eac"
    };

    //files with contents that need string vars replaced
    private HashSet<string> postBuildFilesWithVars = new HashSet<string>(){
        "EasyAntiCheat/Settings.json"
    };

    private EOSConfig eosConfig = null;

    //-------------------------------------------------------------------------
    private static string GetPackageName()
    {
        return "com.playeveryware.eos";
    }

    //-------------------------------------------------------------------------
    private static string GetPathToEOSBin()
    {
        string projectPathToBin = Path.Combine(Application.dataPath, "../bin/");
        string packagePathToBin = Path.GetFullPath("Packages/" + GetPackageName() + "/bin~/");

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
        string packagePathname = Path.GetFullPath("Packages/" + GetPackageName() + "/PlatformSpecificAssets~/EOS/Windows/");
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
    private static void InstallBootStrapper(BuildReport report, string pathToEOSBootStrapperTool, string pathToEOSBootStrapper)
    {
        string installPathForExe = report.summary.outputPath;
        string installDirectory = Path.GetDirectoryName(installPathForExe);
        string installPathForEOSBootStrapper = Path.Combine(installDirectory, "EOSBootStrapper.exe");
        string bootStrapperArgs = ""
           + " --source-bootstrapper-path " + "\"" + pathToEOSBootStrapper + "\""
           + " --target-bootstrapper-path " + "\"" + installPathForEOSBootStrapper + "\""
           + " --target-application-path "  + "\"" + installPathForExe + "\""
        ;

        var procInfo = new System.Diagnostics.ProcessStartInfo();
        procInfo.FileName = pathToEOSBootStrapperTool;
        procInfo.Arguments = bootStrapperArgs;
        procInfo.UseShellExecute = false;
        procInfo.WorkingDirectory = installDirectory;
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
    private void InstallFiles(BuildReport report)
    {
        string destDir = Path.GetDirectoryName(report.summary.outputPath);
        string pathToInstallFrom = GetPathToPlatformSepecificAssetsForWindows();

        List<string> filestoInstall = new List<string>(postBuildFiles);

        //add all files in postBuildDirectories to list of files to copy (non-recursive)
        foreach (string directoryToInstall in postBuildDirectories)
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

        string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSManager.ConfigFileName);
        var configDataAsString = File.ReadAllText(configFilePath);
        var configData = JsonUtility.FromJson<EOSConfig>(configDataAsString);
        eosConfig = configData;
        return configData;
    }

    private static string ReplaceFileNameVars(string filename)
    {
        filename = filename.Replace("[UnityProductName]", Application.productName);
        return filename;
    }

    private void ReplaceFileContentVars(string filepath)
    {
        var fileContents = File.ReadAllText(filepath);
        EOSConfig eosConfig = GetEOSConfig();

        var sb = new System.Text.StringBuilder(fileContents);

        sb.Replace("<UnityProductName>", Application.productName);
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
            InstallFiles(report);
            
            string pathToEOSBootStrapperTool = GetPathToEOSBin() + "/EOSBootstrapperTool.exe";
            string pathToEOSBootStrapper = GetPathToEOSBin() + "/EOSBootStrapper.exe";
            string pathToEACIntegrityTool = GetPathToEOSBin() + "/EAC/anticheat_integritytool.exe";
            InstallBootStrapper(report, pathToEOSBootStrapperTool, pathToEOSBootStrapper);
            GenerateIntegrityCert(report, pathToEACIntegrityTool, GetEOSConfig().productID, "base_private.key", "base_public.cer");
        }
    }
}

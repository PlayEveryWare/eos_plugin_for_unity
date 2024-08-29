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

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using UnityEditor.Build.Reporting;
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using PlayEveryWare.EpicOnlineServices;
    using System.Collections.Generic;
    using System;
    using PlayEveryWare.EpicOnlineServices.Editor.Config;
    using System.Threading.Tasks;

    public class EACUtility
    {
        private static readonly HashSet<string> postBuildFilesOptional =
            new() { "[ExeName].eac", "EasyAntiCheat/SplashScreen.png" };

        //files with contents that need string vars replaced
        private static readonly HashSet<string> postBuildFilesWithVars = new() { "EasyAntiCheat/Settings.json" };

        // TODO: From an organizational perspective - this function ought to be implemented
        //       somewhere other than EACUtility, as it's functionality is used in several
        //       places in the build process that have little to do with EAC.
        public static string GetPathToEOSBin()
        {
            string projectPathToBin = Path.Combine(Application.dataPath, "../tools/bin/");
            string packagePathToBin = Path.GetFullPath("Packages/" + EOSPackageInfo.PackageName + "/bin~/");

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

        private static string GetPathToPlatformSpecificAssets(BuildReport report)
        {
            string platformDirectoryName;
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

            string packagePathname = Path.GetFullPath("Packages/" + EOSPackageInfo.PackageName +
                                                      "/PlatformSpecificAssets~/EOS/" + platformDirectoryName + "/");
            string platformSpecificPathname = Path.Combine(Application.dataPath,
                "../etc/PlatformSpecificAssets/EOS/" + platformDirectoryName + "/");
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

        private static string GetDefaultIntegrityToolPath()
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

        private static string GetDefaultIntegrityConfigPath()
        {
            string projectPathToCfg = Path.Combine(Application.dataPath,
                "Plugins/Standalone/Editor/anticheat_integritytool.cfg");
            string packagePathToCfg = Path.GetFullPath(Path.Combine("Packages", EOSPackageInfo.PackageName,
                "Editor/Standalone/anticheat_integritytool.cfg"));

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
        private static void GenerateIntegrityCert(BuildReport report, string pathToEACIntegrityTool, string productID,
            string keyFileName, string certFileName, string configFile = null)
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

            string buildExeName = Path.GetFileName(report.summary.outputPath);

            try
            {
                ReplaceFileContentVars(newCfgPath, buildExeName);
                configFile = newCfgPath;

                string integrityToolArgs =
                    string.Format("-productid {0} -inkey \"{1}\" -incert \"{2}\" -target_game_dir \"{3}\"", productID,
                        keyFileName, certFileName, installDirectory);
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
                    if (!string.IsNullOrEmpty(e.Data))
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
            finally
            {
                File.Delete(newCfgPath);
            }
        }

        private static string GetRelativePath(string relativeTo, string path)
        {
            Uri uri = new(relativeTo);
            var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString())
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
            {
                rel = $".{Path.DirectorySeparatorChar}{rel}";
            }

            return rel;
        }

        private static List<string> GetPostBuildFiles(BuildReport report)
        {
            List<string> files = new()
            {
                //optional override config file for EAC CDN
                "[ExeName].eac", "EasyAntiCheat/Settings.json", "EasyAntiCheat/SplashScreen.png"
            };

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

            return files;
        }

        private static List<string> GetPostBuildDirectories(BuildReport report)
        {
            List<string> directories = new() { "EasyAntiCheat/Licenses", "EasyAntiCheat/Localization" };

            switch (report.summary.platform)
            {
                case BuildTarget.StandaloneOSX:
                    directories.Add("eac_launcher.app/Contents");
                    directories.Add("eac_launcher.app/Contents/_CodeSignature");
                    directories.Add("eac_launcher.app/Contents/MacOS");
                    directories.Add("eac_launcher.app/Contents/Resources");
                    break;
            }

            return directories;
        }

        private static void InstallEACFiles(BuildReport report)
        {
            string destDir = Path.GetDirectoryName(report.summary.outputPath);
            string pathToInstallFrom = GetPathToPlatformSpecificAssets(report);

            if (string.IsNullOrEmpty(pathToInstallFrom))
            {
                Debug.LogError($"Error installing Easy Anti Cheat files - the path to install from was empty.");
                return;
            }

            List<string> filestoInstall = GetPostBuildFiles(report);
            List<string> directoriesToInstall = GetPostBuildDirectories(report);

            //add all files in postBuildDirectories to list of files to copy (non-recursive)
            foreach (string directoryToInstall in directoriesToInstall)
            {
                string dirToInstallPathName = Path.Combine(pathToInstallFrom, directoryToInstall);

                if (!Directory.Exists(dirToInstallPathName))
                    continue;

                var dirFiles = Directory.GetFiles(dirToInstallPathName);
                foreach (string dirFile in dirFiles)
                {
                    string relativePath = GetRelativePath(pathToInstallFrom, dirFile);
                    filestoInstall.Add(relativePath);
                }
            }

            string buildExeName = Path.GetFileName(report.summary.outputPath);

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

                    string destPathname = Path.Combine(fileToInstallParentDirectory,
                        Path.GetFileName(fileToInstallPathName));

                    destPathname = ReplaceFileNameVars(destPathname, buildExeName);

                    if (File.Exists(destPathname))
                    {
                        File.SetAttributes(destPathname, File.GetAttributes(destPathname) & ~FileAttributes.ReadOnly);
                    }

                    File.Copy(fileToInstallPathName, destPathname, true);

                    if (postBuildFilesWithVars.Contains(fileToInstall))
                    {
                        ReplaceFileContentVars(destPathname, buildExeName);
                    }
                }
                else if (!postBuildFilesOptional.Contains(fileToInstall))
                {
                    Debug.LogError("Missing platform specific file: " + fileToInstall);
                }
            }
        }

        private static void CopySplashImage(BuildReport report, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Debug.LogError("Specified EAC splash image not found");
                return;
            }

            string destPath = Path.Combine(Path.GetDirectoryName(report.summary.outputPath),
                "EasyAntiCheat/SplashScreen.png");
            if (File.Exists(destPath))
            {
                File.SetAttributes(destPath, File.GetAttributes(destPath) & ~FileAttributes.ReadOnly);
            }

            File.Copy(imagePath, destPath, true);
        }

        private static string ReplaceFileNameVars(string filename, string buildExeName)
        {
            filename = filename.Replace("[UnityProductName]", Application.productName);
            filename = filename.Replace("[ExeName]", buildExeName);
            return filename;
        }

        private static void ReplaceFileContentVars(string filepath, string buildExeName)
        {
            using StreamReader reader = new(filepath);
            string fileContents = reader.ReadToEnd();
            reader.Close();

            EOSConfig eosConfig = Config.Get<EOSConfig>();

            var sb = new System.Text.StringBuilder(fileContents);

            sb.Replace("<UnityProductName>", Application.productName);
            sb.Replace("<ExeName>", buildExeName);
            sb.Replace("<ExeNameNoExt>", Path.GetFileNameWithoutExtension(buildExeName));
            sb.Replace("<ProductName>", eosConfig.productName);
            sb.Replace("<ProductID>", eosConfig.productID);
            sb.Replace("<SandboxID>", eosConfig.sandboxID);
            sb.Replace("<DeploymentID>", eosConfig.deploymentID);

            fileContents = sb.ToString();

            using StreamWriter writer = new(filepath);
            writer.Write(fileContents);
        }

        public static void ConfigureEAC(BuildReport report)
        {
            ToolsConfig toolsConfig = Config.Get<ToolsConfig>();
            
            // if EAC is not supposed to be installed, then stop here
            if (!toolsConfig.useEAC)
            {
                return;
            }

            InstallEACFiles(report);

            if (!string.IsNullOrWhiteSpace(toolsConfig.pathToEACSplashImage))
            {
                CopySplashImage(report, toolsConfig.pathToEACSplashImage);
            }

            if (!string.IsNullOrWhiteSpace(toolsConfig.pathToEACPrivateKey) &&
                !string.IsNullOrWhiteSpace(toolsConfig.pathToEACCertificate))
            {
                bool defaultTool = false;
                string toolPath = toolsConfig.pathToEACIntegrityTool;
                if (string.IsNullOrWhiteSpace(toolPath))
                {
                    toolPath = GetDefaultIntegrityToolPath();
                    defaultTool = true;
                }

                string cfgPath = toolsConfig.pathToEACIntegrityConfig;
                if (string.IsNullOrWhiteSpace(cfgPath) && defaultTool)
                {
                    //use default cfg if no cfg is specified and default tool path is used
                    cfgPath = GetDefaultIntegrityConfigPath();
                }

                if (!string.IsNullOrWhiteSpace(toolPath))
                {
                    var productId = (Config.Get<EOSConfig>()).productID;
                    GenerateIntegrityCert(report, toolPath, productId,
                        toolsConfig.pathToEACPrivateKey, toolsConfig.pathToEACCertificate, cfgPath);
                }
            }
        }
    }
}
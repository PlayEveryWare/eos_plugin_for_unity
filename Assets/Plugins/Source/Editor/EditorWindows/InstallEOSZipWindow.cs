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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using Build;
    using EpicOnlineServices.Utility;
    using System.Threading.Tasks;
    using Utility;

    public class InstallEOSZipWindow : EOSEditorWindow
    {
        private const string PlatformImportInfoListFileName = "eos_platform_import_info_list.json";
        public InstallEOSZipWindow() : base("Install EOS Zip") { }

        [Serializable]
        private class PlatformImportInfo
        {
            [SerializeField]
            public string platform;

            [SerializeField]
            public string descPath;

            [SerializeField]
            public bool isGettingImported;
        }

        [Serializable]
        private class PlatformImportInfoList
        {
            [SerializeField]
            public List<PlatformImportInfo> platformImportInfoList;

        }

        private string pathToJSONPackageDescription;
        private string pathToZipFile;

        private string pathToImportDescDirectory;
        private PlatformImportInfoList importInfoList;

        [MenuItem("Tools/EOS Plugin/Install EOS zip")]
        public static void ShowWindow()
        {
            GetWindow<InstallEOSZipWindow>();
        }

        static public void UnzipEntry(ZipArchiveEntry zipEntry, string pathName)
        {
            using var destStream = File.OpenWrite(pathName);
            zipEntry.Open().CopyTo(destStream);
        }


        static public void UnzipFile(string pathToZipFile, string dest)
        {
            // unzip files
            using var filestream = new FileStream(pathToZipFile, FileMode.Open);
            using var zipArchive = new ZipArchive(filestream);
            string extraPath = "";
            //search for guaranteed file to check if SDK root is inside any extraneous subfolders
            foreach (var zipEntry in zipArchive.Entries)
            {
                if (zipEntry.FullName.EndsWith("SDK/Tools/EOSBootstrapper.exe"))
                {
                    extraPath = zipEntry.FullName.Replace("SDK/Tools/EOSBootstrapper.exe", "");
                    break;
                }
            }

            int zipCount = zipArchive.Entries.Count;
            float i = 0.0f;
            foreach (var zipEntry in zipArchive.Entries)
            {
                if (string.IsNullOrWhiteSpace(zipEntry.Name))
                {
                    i += 1.0f;
                    continue;
                }

                EditorUtility.DisplayProgressBar("Unzipping file", "Unzipping " + Path.GetFileName(pathToZipFile), i / zipCount);
                string targetPath = zipEntry.FullName;
                if (!string.IsNullOrEmpty(extraPath))
                {
                    targetPath = targetPath.Replace(extraPath, "");
                }
                string pathName = Path.Combine(dest, targetPath);
                string parentDirectory = Path.GetDirectoryName(pathName);
                if (!Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                UnzipEntry(zipEntry, pathName);
                i += 1.0f;
            }
            EditorUtility.ClearProgressBar();
        }

        protected override void Setup()
        {
            pathToImportDescDirectory = Path.Combine(FileSystemUtility.GetProjectPath(), "etc/EOSImportDesriptions");
            importInfoList = JsonUtility.FromJsonFile<PlatformImportInfoList>(Path.Combine(pathToImportDescDirectory, PlatformImportInfoListFileName));
        }

        private void DrawPresets()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Public", GUILayout.MaxWidth(100)))
            {
                foreach (var platformImportInfo in importInfoList.platformImportInfoList)
                {
                    if (platformImportInfo.platform == "iOS" ||
                        platformImportInfo.platform == "Android" ||
                        platformImportInfo.platform == "Windows" ||
                        platformImportInfo.platform == "Mac" ||
                        platformImportInfo.platform == "Linux")
                    {
                        platformImportInfo.isGettingImported = true;
                    }
                    else
                    {
                        platformImportInfo.isGettingImported = false;
                    }
                }
            }

            if (GUILayout.Button("Mobile", GUILayout.MaxWidth(100)))
            {
                foreach (var platformImportInfo in importInfoList.platformImportInfoList)
                {
                    if (platformImportInfo.platform == "iOS" ||
                        platformImportInfo.platform == "Android")
                    {
                        platformImportInfo.isGettingImported = true;
                    }
                    else
                    {
                        platformImportInfo.isGettingImported = false;
                    }
                }
            }

            if (GUILayout.Button("Select All", GUILayout.MaxWidth(100)))
            {
                foreach (var platformImportInfo in importInfoList.platformImportInfoList)
                {
                    platformImportInfo.isGettingImported = true;
                }
            }

            if (GUILayout.Button("Clear All", GUILayout.MaxWidth(100)))
            {
                foreach (var platformImportInfo in importInfoList.platformImportInfoList)
                {
                    platformImportInfo.isGettingImported = false;
                }
            }
            GUILayout.EndHorizontal();
        }

        protected override void RenderWindow()
        {
            GUILayout.Label("Install EOS Files into project");

            DrawPresets();
            foreach (var platformImportInfo in importInfoList.platformImportInfoList)
            {
                GUIEditorUtility.AssigningBoolField(platformImportInfo.platform,
                    ref platformImportInfo.isGettingImported, 300);
            }

            GUILayout.Label("");
            GUILayout.Label("Select Zip Path");
            GUILayout.BeginHorizontal(GUIStyle.none);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                pathToZipFile = EditorUtility.OpenFilePanel("Pick Zip File", "", "zip");
            }

            GUILayout.Label(pathToZipFile);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Install") && FileSystemUtility.TryGetTempDirectory(out string tmpDir))
            {
                try
                {
                    UnzipFile(pathToZipFile, tmpDir);

                    var toConvert = new List<string>();
                    // Convert files to a consistant line ending
                    foreach (var entity in Directory.EnumerateFiles(tmpDir, "*", SearchOption.AllDirectories))
                    {
                        if (Path.GetExtension(entity) == ".cs")
                        {
                            toConvert.Add(entity);
                        }
                    }

                    for (int i = 0; i < toConvert.Count; ++i)
                    {
                        var entity = toConvert[i];
                        EditorUtility.DisplayProgressBar("Converting line endings", Path.GetFileName(entity),
                            (float)i / toConvert.Count);
                        FileSystemUtility.ConvertDosToUnixLineEndings(entity);
                    }

                    EditorUtility.ClearProgressBar();


                    foreach (var platformImportInfo in importInfoList.platformImportInfoList)
                    {
                        if (platformImportInfo.isGettingImported)
                        {
                            string path = Path.Combine(pathToImportDescDirectory, platformImportInfo.descPath);
                            var packageDescription =
                                JsonUtility.FromJsonFile<PackageDescription>(path);

                            var fileResults =
                                PackageFileUtility.FindPackageFiles(tmpDir,
                                    packageDescription);
                            // This should be the correct directory
                            var projectDir = FileSystemUtility.GetProjectPath();
                            // TODO: Async not tested here.
                            _ = PackageFileUtility.CopyFilesToDirectory(projectDir, fileResults);
                        }
                    }

                }
                finally
                {
                    //clean up unzipped files on success or error
                    Directory.Delete(tmpDir, true);
                }
            }

        }
    }
}
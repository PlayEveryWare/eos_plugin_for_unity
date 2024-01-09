using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Playeveryware.Editor;
using System.Threading.Tasks;
using System.IO.Compression;

namespace PlayEveryWare.EpicOnlineServices
{
    public class InstallEpicOnlineServiceZip : EOSEditorWindow
    {
        private string pathToJSONPackageDescription;
        private string pathToZipFile;

        private string pathToImportDescDirectory;
        private PlatformImportInfoList importInfoList;

        [MenuItem("Tools/EOS Plugin/Install EOS zip")]
        public static void ShowWindow()
        {
            GetWindow<InstallEpicOnlineServiceZip>("Install EOS Zip");
        }

        static public void UnzipEntry(ZipArchiveEntry zipEntry, string pathName)
        {
            using (var destStream = File.OpenWrite(pathName))
            {
                zipEntry.Open().CopyTo(destStream);
            }
        }

        
        static public void UnzipFile(string pathToZipFile, string dest)
        {
            // unzip files
            using (var filestream = new FileStream(pathToZipFile, FileMode.Open))
            {
                using (var zipArchive = new ZipArchive(filestream))
                {
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
            }
        }
        
        protected override void Setup()
        {
            pathToImportDescDirectory = Application.dataPath + "/../etc/EOSImportDesriptions/";
            var JSONPackageDescription = File.ReadAllText(pathToImportDescDirectory + "eos_platform_import_info_list.json");
            importInfoList = JsonUtility.FromJson<PlatformImportInfoList>(JSONPackageDescription);
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
                GUIEditorHelper.AssigningBoolField(platformImportInfo.platform, ref platformImportInfo.isGettingImported, 300);
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

            if (GUILayout.Button("Install"))
            {
                string tmpDir = PackageFileUtils.GenerateTemporaryBuildPath();

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
                        EditorUtility.DisplayProgressBar("Converting line endings", Path.GetFileName(entity), (float)i / toConvert.Count);
                        PackageFileUtils.Dos2UnixLineEndings(entity);
                    }
                    EditorUtility.ClearProgressBar();


                    foreach (var platformImportInfo in importInfoList.platformImportInfoList)
                    {
                        if (platformImportInfo.isGettingImported)
                        {
                            var JSONPackageDescription = File.ReadAllText(pathToImportDescDirectory + platformImportInfo.descPath);
                            var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);

                            var fileResults = PackageFileUtils.GetFileInfoMatchingPackageDescription(tmpDir, packageDescription);
                            // This should be the correct directory
                            var projectDir = PackageFileUtils.GetProjectPath();
                            PackageFileUtils.CopyFilesToDirectory(projectDir, fileResults);
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

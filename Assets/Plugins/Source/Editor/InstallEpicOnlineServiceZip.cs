using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using Playeveryware.Editor;
using System.Threading.Tasks;

namespace PlayEveryWare.EpicOnlineServices
{
    public class InstallEpicOnlineServiceZip : UnityEditor.EditorWindow
    {
        private string pathToJSONPackageDescription;
        private string pathToZipFile;

        [UnityEditor.MenuItem("Tools/Install EOS zip")]
        public static void ShowWindow()
        {
            GetWindow(typeof(InstallEpicOnlineServiceZip));
        }

        // static public void UnzipEntry(ZipArchiveEntry zipEntry, string pathName)
        // {
        //     using (var destStream = File.OpenWrite(pathName))
        //     {
        //         zipEntry.Open().CopyTo(destStream);
        //     }
        // }

        //-------------------------------------------------------------------------
        static public void UnzipFile(string pathToZipFile, string dest)
        {
            // // unzip files
            // using (var filestream = new FileStream(pathToZipFile, FileMode.Open))
            // {
            //     using (var zipArchive = new ZipArchive(filestream))
            //     {
            //         int zipCount = zipArchive.Entries.Count;
            //         float i = 0.0f;
            //         foreach (var zipEntry in zipArchive.Entries)
            //         {
            //             EditorUtility.DisplayProgressBar("Unzipping file", "Unzipping " + Path.GetFileName(pathToZipFile), i / zipCount);
            //             string pathName = Path.Combine(dest, zipEntry.FullName);
            //             string parentDirectory = Path.GetDirectoryName(pathName);
            //             if (!Directory.Exists(parentDirectory))
            //             {
            //                 Directory.CreateDirectory(parentDirectory);
            //             }
            // 
            //             UnzipEntry(zipEntry, pathName);
            //             i += 1.0f;
            //         }
            //         EditorUtility.ClearProgressBar();
            //     }
            // }
        }
        //-------------------------------------------------------------------------
        public string ToCapitalize(string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }
            return str.ToUpper();
        }

        //-------------------------------------------------------------------------
        private void Awake()
        {
            
        }

        //-------------------------------------------------------------------------
        private void OnGUI()
        {
            GUILayout.Label("Install EOS Files into project");

            GUILayout.Label("JSON Description Path");
            GUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label(pathToJSONPackageDescription);
            if(GUILayout.Button("select"))
            {
                pathToJSONPackageDescription = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
            }
            GUILayout.EndHorizontal();


            GUILayout.Label("Select Zip Path");
            GUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label(pathToZipFile);
            if (GUILayout.Button("Select"))
            {
                pathToZipFile = EditorUtility.OpenFilePanel("Pick Zip File", "", "zip");
            }
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Install"))
            {
                var JSONPackageDescription = File.ReadAllText(pathToJSONPackageDescription);
                var packageDescription = JsonUtility.FromJson<PackageDescription>(JSONPackageDescription);
                string tmpDir = FileUtils.GenerateTemporaryBuildPath();

                UnzipFile(pathToZipFile, tmpDir);

                var toConvert = new List<string>();
                // Convert files to a consistant line ending
                foreach(var entity in Directory.EnumerateFiles(tmpDir, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(entity) == ".cs")
                    {
                        toConvert.Add(entity);
                    }
                }

                for(int i = 0; i < toConvert.Count; ++i)
                {
                    var entity = toConvert[i];
                    EditorUtility.DisplayProgressBar("Converting line endings", Path.GetFileName(entity), (float)i / toConvert.Count);
                    FileUtils.Dos2UnixLineEndings(entity);
                }
                EditorUtility.ClearProgressBar();

                var fileResults = FileUtils.GetFileInfoMatchingPackageDescription(tmpDir, packageDescription);

                // This should be the correct directory
                var projectDir = FileUtils.GetProjectPath();
                FileUtils.CopyFilesToDirectory(projectDir, fileResults);

            }
        }

    }
}
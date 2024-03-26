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

using UnityEngine;
using UnityEditor;

// make lines a little shorter
using UPMUtility = PlayEveryWare.EpicOnlineServices.Editor.Utility.UnityPackageCreationUtility;

using System;

namespace PlayEveryWare.EpicOnlineServices.Editor.Windows
{
    using Config;
    using EpicOnlineServices.Utility;
    using System.IO;
    using System.Threading.Tasks;
    using Utility;
    using Config = EpicOnlineServices.Config;

    [Serializable]
    public class CreatePackageWindow : EOSEditorWindow
    {
        const string DEFAULT_OUTPUT_DIRECTORY = "Build";
        private const string DefaultPackageDescription = "etc/PackageConfigurations/eos_package_description.json";
        
        [RetainPreference("ShowAdvanced")]
        private bool _showAdvanced = false;

        [RetainPreference("CleanBeforeCreate")]
        private bool _cleanBeforeCreate = true;

        [RetainPreference("IgnoreGitWhenCleaning")]
        private bool _ignoreGitWhenCleaning = true;

        private PackagingConfig packagingConfig;

        [MenuItem("Tools/EOS Plugin/Create Package")]
        public static void ShowWindow()
        {
            GetWindow<CreatePackageWindow>("Create Package");
        }

        protected override async Task AsyncSetup()
        {
            packagingConfig = await Config.Get<PackagingConfig>();

            if (string.IsNullOrEmpty(packagingConfig.pathToJSONPackageDescription))
            {
                packagingConfig.pathToJSONPackageDescription =
                    Path.Combine(FileUtility.GetProjectPath(), DefaultPackageDescription);
                await packagingConfig.WriteAsync();
            }
            await base.AsyncSetup();
        }

        protected override void RenderWindow()
        {
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            var outputPath = packagingConfig.pathToOutput;
            GUIEditorUtility.AssigningTextField("Output Path", ref outputPath);
            if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
            {
                var outputDir = EditorUtility.OpenFolderPanel("Pick Output Directory", "", "");
                if (!string.IsNullOrWhiteSpace(outputDir))
                {
                    outputPath = outputDir;
                }
            }

            if (packagingConfig.pathToOutput != outputPath)
            {
                packagingConfig.pathToOutput = outputPath;
                packagingConfig.Write(true, false);
            }

            
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();

            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced");
            if (_showAdvanced)
            {
                GUILayout.Space(10f);

                GUILayout.BeginVertical();
                GUIEditorUtility.AssigningBoolField("Clean target directory", ref _cleanBeforeCreate, 200f,
                    "Cleans the output target directory before creating the package.");

                GUIEditorUtility.AssigningBoolField("Don't clean .git directory", ref _ignoreGitWhenCleaning, 200f, "" +
                    "When cleaning the output target directory, don't delete any .git files.");

                var jsonPackageFile = packagingConfig.pathToJSONPackageDescription;

                GUILayout.BeginHorizontal();
                GUIEditorUtility.AssigningTextField("JSON Description Path", ref jsonPackageFile);
                if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
                {
                    var jsonFile = EditorUtility.OpenFilePanel("Pick JSON Package Description", "", "json");
                    if (!string.IsNullOrWhiteSpace(jsonFile))
                    {
                        jsonPackageFile = jsonFile;
                    }
                }
                GUILayout.EndHorizontal();

                if (jsonPackageFile != packagingConfig.pathToJSONPackageDescription)
                {
                    packagingConfig.pathToJSONPackageDescription = jsonPackageFile;
                    packagingConfig.Write(true, false);
                }

                GUILayout.EndVertical();
                GUILayout.Space(10f);
                
            }

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.FlexibleSpace();

            if (_createPackageTask != null)
            {
                GUI.enabled = _createPackageTask.IsCompleted != false;
            }

            if (GUILayout.Button("Export UPM Directory", GUILayout.MaxWidth(200)))
            {
                EditorApplication.update += CheckForPackageCreated;
                _createPackageTask = UPMUtility.CreatePackage(UPMUtility.PackageType.UPM, _cleanBeforeCreate, _ignoreGitWhenCleaning);
                OnPackageCreated(packagingConfig.pathToOutput);
            }

            if (GUILayout.Button("Create UPM Tarball", GUILayout.MaxWidth(200)))
            {
                EditorApplication.update += CheckForPackageCreated;
                _createPackageTask = UPMUtility.CreatePackage(UPMUtility.PackageType.UPMTarball, _cleanBeforeCreate, _ignoreGitWhenCleaning);
                OnPackageCreated(packagingConfig.pathToOutput);
            }

            GUI.enabled = false; // Disable UPM .unitypackage for the time being.
            if (GUILayout.Button("Create .unitypackage", GUILayout.MaxWidth(200)))
            {
                EditorApplication.update += CheckForPackageCreated;
                _createPackageTask = UPMUtility.CreatePackage(UPMUtility.PackageType.DotUnity, _cleanBeforeCreate, _ignoreGitWhenCleaning);
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
        }

        private Task _createPackageTask;
        private bool _packageCreated = false;

        private void CheckForPackageCreated()
        {
            if (_createPackageTask.IsCompleted && !_packageCreated)
            {
                _packageCreated = true;
                EditorApplication.update -= CheckForPackageCreated;
            }
        }

        private void OnPackageCreated(string outputPath)
        {
            EditorUtility.DisplayDialog(
                "Package created",
                $"Package was successfully created at \"{outputPath}\"",
                "Ok");
        }

        private bool OnEmptyOutputPath(ref string output)
        {
            // Display dialog saying no output path was provided, and offering to default to the 'Build' directory.
            if (EditorUtility.DisplayDialog(
                    "Empty output path",
                    $"No output path was provided, do you want to use {DEFAULT_OUTPUT_DIRECTORY}?",
                    "Yes", "Cancel"))
            {
                output = DEFAULT_OUTPUT_DIRECTORY;
                return true;
            }

            return false;
        }
    }
}

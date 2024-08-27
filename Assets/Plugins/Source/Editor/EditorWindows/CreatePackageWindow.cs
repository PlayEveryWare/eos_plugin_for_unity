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
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditorInternal;
    using Utility;
    using Config = EpicOnlineServices.Config;

    [Serializable]
    public class CreatePackageWindow : EOSEditorWindow
    {
        private const string DefaultPackageDescription = "etc/PackageConfigurations/eos_package_description.json";
        
        [RetainPreference("ShowAdvanced")]
        private bool _showAdvanced = false;

        [RetainPreference("CleanBeforeCreate")]
        private bool _cleanBeforeCreate = true;

        [RetainPreference("IgnoreGitWhenCleaning")]
        private bool _ignoreGitWhenCleaning = true;

        private PackagingConfig _packagingConfig;

        private CancellationTokenSource _createPackageCancellationTokenSource;
        
        private bool _operationInProgress;

        public CreatePackageWindow() : base("Create Package") { }

        #region Progress Bar Stuff
        
        private float _actualProgress;
        private float _displayedProgress;
        private string _progressText;
        private object _progressLock = new object();
        private Thread _progressUpdateThread;

        #endregion

        [MenuItem("Tools/EOS Plugin/Create Package")]
        public static void ShowWindow()
        {
            GetWindow<CreatePackageWindow>();
        }

        protected override async Task AsyncSetup()
        {
            _packagingConfig = await Config.GetAsync<PackagingConfig>();

            if (string.IsNullOrEmpty(_packagingConfig.pathToJSONPackageDescription))
            {
                _packagingConfig.pathToJSONPackageDescription =
                    Path.Combine(FileSystemUtility.GetProjectPath(), DefaultPackageDescription);
                await _packagingConfig.WriteAsync();
            }
            await base.AsyncSetup();
        }

        protected static bool SelectOutputDirectory(ref string path)
        {
            string selectedPath = EditorUtility.OpenFolderPanel(
                "Pick output directory",
                Path.GetDirectoryName(FileSystemUtility.GetProjectPath()),
                "");

            if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                return false;
            }

            path = selectedPath;
            return true;
        }

        protected override void Teardown()
        {
            base.Teardown();

            if (_createPackageCancellationTokenSource != null)
            {
                _createPackageCancellationTokenSource.Cancel();
                _createPackageCancellationTokenSource.Dispose();
                _createPackageCancellationTokenSource = null;
            }
        }

        protected override void RenderWindow()
        {
            if (_operationInProgress)
            {
                GUI.enabled = false;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            var outputPath = _packagingConfig.pathToOutput;
            GUIEditorUtility.AssigningTextField("Output Path", ref outputPath, 100f);
            if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
            {
                if (SelectOutputDirectory(ref outputPath))
                {
                    _packagingConfig.pathToOutput = outputPath;
                    _packagingConfig.Write();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUIEditorUtility.RenderFoldout(ref _showAdvanced, "Hide Advanced Options", "Show Advanced Options", RenderAdvanced);

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            List<(string buttonLabel, UPMUtility.PackageType packageToMake, bool enableButton)> buttons = new()
            {
                ("UPM Directory", UPMUtility.PackageType.UPM,        true),
                ("UPM Tarball",   UPMUtility.PackageType.UPMTarball, true),
                (".unitypackage", UPMUtility.PackageType.DotUnity,   false)
            };

            foreach ((string buttonLabel, UPMUtility.PackageType packageToMake, bool enabled) in buttons)
            {
                GUI.enabled = enabled && !_operationInProgress;
                if (GUILayout.Button($"Export {buttonLabel}", GUILayout.MaxWidth(200)))
                {
                    StartCreatePackageAsync(packageToMake, _cleanBeforeCreate);
                }
                GUI.enabled = _operationInProgress;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();

            /*
             * NOTES:
             *
             * There are several things here that need to be fixed:
             *
             * 1. All this fanciness around label positioning and progress bar, etc. Really needs to be moved out
             *    of this class and abstracted into static contexts.
             * 2. For exporting a UPM Tarball, none of the progress indicators capture the work that is done to compress
             *    the output. Basically, it just shows the progress of copying the files to the temporary directory, then
             *    it will stop showing progress (appearing to be completed) when in reality the compressed tgz file will
             *    continue to be created.
             * 3. Currently, the "Ignore .git directory" options default to true, and the UI
             *    does not change the behavior.
             */

            if (_operationInProgress)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(20f);
                GUI.enabled = true;

                // Make a taller progress bar
                var progressBarRect = EditorGUILayout.GetControlRect();
                progressBarRect.height *= 2;

                GUIStyle customLabelStyle = new(EditorStyles.label)
                {
                    font = MonoFont, fontSize = 14, normal = { textColor = Color.white }, fontStyle = FontStyle.Bold
                };

                Vector2 labelSize = customLabelStyle.CalcSize(new GUIContent(_progressText));
                
                Rect labelRect = new(
                    progressBarRect.x + (progressBarRect.width - labelSize.x) / 2,
                    progressBarRect.y + (progressBarRect.height - labelSize.y) / 2,
                    labelSize.x,
                    labelSize.y);

                lock (_progressLock)
                {
                    _displayedProgress = Mathf.Lerp(_displayedProgress, _actualProgress, Time.deltaTime * 2);
                }

                EditorGUI.ProgressBar(progressBarRect, _displayedProgress, "");

                GUI.Label(labelRect, _progressText, customLabelStyle);

                GUILayout.Space(20f);
                if (GUILayout.Button("Cancel"))
                {
                    _actualProgress = 0.0f;
                    _progressText = "";
                    _createPackageCancellationTokenSource?.Cancel();
                    FileSystemUtility.CleanDirectory(_packagingConfig.pathToOutput);
                }
                GUILayout.EndVertical();
            }
        }

        private void SmoothingDelay(CancellationToken cancellationToken)
        {
            const int smoothingFactor = 500;
            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(smoothingFactor);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        protected void RenderAdvanced()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5f);
            var jsonPackageFile = _packagingConfig.pathToJSONPackageDescription;
            
            GUILayout.BeginHorizontal();
            GUIEditorUtility.AssigningTextField("JSON Description Path", ref jsonPackageFile, 150f);
            if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
            {
                var jsonFile = EditorUtility.OpenFilePanel(
                    "Pick JSON Package Description",
                    Path.Combine(FileSystemUtility.GetProjectPath(), Path.GetDirectoryName(DefaultPackageDescription)),
                    "json");

                if (!string.IsNullOrWhiteSpace(jsonFile))
                {
                    jsonPackageFile = jsonFile;
                }
            }
            GUILayout.EndHorizontal();

            if (jsonPackageFile != _packagingConfig.pathToJSONPackageDescription)
            {
                _packagingConfig.pathToJSONPackageDescription = jsonPackageFile;
                _packagingConfig.Write(true, false);
            }

            GUIEditorUtility.AssigningBoolField("Clean target directory", ref _cleanBeforeCreate, 150f,
                "Cleans the output target directory before creating the package.");

            GUIEditorUtility.AssigningBoolField("Don't clean .git directory", ref _ignoreGitWhenCleaning, 150f,
                "When cleaning the output target directory, don't delete any .git files.");

            GUILayout.EndVertical();
            GUILayout.Space(10f);
        }

        private async void StartCreatePackageAsync(UPMUtility.PackageType type, bool clean)
        {
            try
            {
                string outputPath = _packagingConfig.pathToOutput;

                // if the output path is empty or doesn't exist, prompt for the user to select one
                if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
                {
                    if (SelectOutputDirectory(ref outputPath))
                    {
                        _packagingConfig.pathToOutput = outputPath;
                        _packagingConfig.Write();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Package Export Canceled",
                            "An output directory was not selected, so package export has been canceled.",
                            "ok");
                        return;
                    }
                }

                _createPackageCancellationTokenSource = new();
                _operationInProgress = true;

                _progressUpdateThread = new Thread(() => SmoothingDelay(_createPackageCancellationTokenSource.Token));
                _progressUpdateThread.Start();

                var progressHandler = new Progress<FileSystemUtility.CopyFileProgressInfo>(value =>
                {
                    var fileCountStrSize = value.TotalFilesToCopy.ToString().Length;
                    string filesCopiedStrFormat = "{0," + fileCountStrSize + "}";
                    var filesCopiedCountStr = String.Format(filesCopiedStrFormat, value.FilesCopied);
                    var filesToCopyCountStr = String.Format(filesCopiedStrFormat, value.TotalFilesToCopy);

                    lock (_progressLock)
                    {
                        // Ternary statement here to prevent a divide by zero problem
                        // ever happening, despite how odd it would be in this case.
                        float newActualProgress = (0.0f < value.TotalBytesToCopy)
                            ? value.BytesCopied / (float)value.TotalBytesToCopy
                            : 0;

                        // Just to guarantee that the progress is increasing
                        if (newActualProgress > _actualProgress)
                        {
                            _actualProgress = newActualProgress;
                        }

                        _progressText = $"{filesCopiedCountStr} out of {filesToCopyCountStr} files copied";
                    }

                    Repaint();
                });

                await UPMUtility.CreatePackage(type, clean, progressHandler, _createPackageCancellationTokenSource.Token);

                if (EditorUtility.DisplayDialog("Package Created", "Package was successfully created",
                        "Open Output Path", "Close"))
                {
                    FileSystemUtility.OpenDirectory(outputPath);
                }
            }
            catch (OperationCanceledException ex)
            {
                _progressText = $"Operation Canceled: {ex.Message}";
            }
            finally
            {
                _operationInProgress = false;
                _progressText = "";
                _actualProgress = 0f;
                _displayedProgress = 0f;
                _createPackageCancellationTokenSource?.Dispose();
                _createPackageCancellationTokenSource = null;
            }
        }
    }
}

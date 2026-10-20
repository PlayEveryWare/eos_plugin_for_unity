/*
 * Copyright (c) 2026 Epic Games Inc
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using PlayEveryWare.EpicOnlineServices.Tests.Editor;
using UnityEditor.TestTools;
using UnityEngine.TestTools;

[assembly: TestPlayerBuildModifier(typeof(TestPlayerModeSetup))]
[assembly: PostBuildCleanup(typeof(TestPlayerModeSetup))]
namespace PlayEveryWare.EpicOnlineServices.Tests.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.TestTools;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Custom build that creates a standalone player running tests to be
    /// deployed to different machines and platforms, overriding the run/build
    /// tests on the test runner window.
    /// </summary>
    public class TestPlayerModeSetup : ITestPlayerBuildModifier, IPostBuildCleanup
    {
        /// <summary>
        /// Indicates whether player tests are currently being run.
        /// </summary>
        private static bool s_RunningPlayerTests;

        /// <summary>
        /// The output directory of the test player build, saved so the
        /// post-build step can copy required files there.
        /// </summary>
        private static string s_TestBuildDirectory;

        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
        {
            // If the test going through run tests, then autorun will be active
            // and we can use that to provide a default test location instead of
            // the temp directory it creates. If it's not running, then a
            // non-temp path will be provided.
            if ((playerOptions.options & BuildOptions.AutoRunPlayer) > 0)
            {
                var testBuildLocation = Path.GetFullPath(Path.Combine(Application.dataPath, $"./../Build/PlayModeTestPlayer/{playerOptions.target}"));
                var fileName = Path.GetFileName(playerOptions.locationPathName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    testBuildLocation = Path.Combine(testBuildLocation, fileName);
                }

                playerOptions.locationPathName = testBuildLocation;
            }

            // Resolve the output directory (locationPathName may be a .exe path or a folder).
            string resolvedPath = Path.GetFullPath(playerOptions.locationPathName);
            s_TestBuildDirectory = Path.HasExtension(resolvedPath)
                ? Path.GetDirectoryName(resolvedPath)
                : resolvedPath;

            // Do not launch the player after the build completes.
            playerOptions.options &= ~BuildOptions.AutoRunPlayer;
            playerOptions.options |= BuildOptions.Development;

            // If not building a standalone windows test player, then disable
            // connecting to host.
            if (playerOptions.target != BuildTarget.StandaloneWindows &&
                playerOptions.target != BuildTarget.StandaloneWindows64)
            {
                playerOptions.options &= ~BuildOptions.ConnectToHost;
            }

            // Instruct the cleanup to exit the Editor if the run came from the
            // command line. The variable is static because the cleanup is being
            // invoked in a new instance of the class.
            s_RunningPlayerTests = true;
            return playerOptions;
        }

        public void Cleanup()
        {
            if (!string.IsNullOrEmpty(s_TestBuildDirectory))
            {
                CopyTestConfigToPlayer(s_TestBuildDirectory);
                s_TestBuildDirectory = null;
            }

            if (s_RunningPlayerTests && IsRunningTestsFromCommandLine())
            {
                // Exit the Editor on the next update, allowing for other
                // PostBuildCleanup steps to run.
                EditorApplication.update += () => { EditorApplication.Exit(0); };
            }
        }

        private static void CopyTestConfigToPlayer(string buildDirectory)
        {
            string sourceFile = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../etc/config/eos_automated_test_config.json"));

            if (!File.Exists(sourceFile))
            {
                Debug.LogWarning($"[TestPlayerModeSetup] Test config not found, skipping copy: {sourceFile}");
                return;
            }

            string destDir = Path.Combine(buildDirectory, "etc", "config");
            Directory.CreateDirectory(destDir);
            string destFile = Path.Combine(destDir, "eos_automated_test_config.json");
            File.Copy(sourceFile, destFile, overwrite: true);
            Debug.Log($"[TestPlayerModeSetup] Copied test config to: {destFile}");
        }

        private static bool IsRunningTestsFromCommandLine()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            return commandLineArgs.Any(value => value == "-runTests");
        }
    }
}
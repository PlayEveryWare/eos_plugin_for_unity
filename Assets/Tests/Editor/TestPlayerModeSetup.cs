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
    using System.Linq;
    using UnityEditor;
    using UnityEditor.TestTools;
    using UnityEngine;
    using Utility;
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
        
        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
        {
            // If the test going through run tests, then autorun will be active
            // and we can use that to provide a default test location instead of
            // the temp directory it creates. If it's not running, then a
            // non-temp path will be provided.
            if ((playerOptions.options & BuildOptions.AutoRunPlayer) > 0)
            {
                var testBuildLocation = FileUtility.GetFullPath(FileUtility.CombinePaths(Application.dataPath, $"./../Build/PlayModeTestPlayer/{playerOptions.target}"));
                var fileName = FileUtility.GetFileName(playerOptions.locationPathName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    testBuildLocation = FileUtility.CombinePaths(testBuildLocation, fileName);
                }

                playerOptions.locationPathName = testBuildLocation;
            }

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
            if (s_RunningPlayerTests && IsRunningTestsFromCommandLine())
            {
                // Exit the Editor on the next update, allowing for other
                // PostBuildCleanup steps to run.
                EditorApplication.update += () => { EditorApplication.Exit(0); };
            }
        }

        private static bool IsRunningTestsFromCommandLine()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            return commandLineArgs.Any(value => value == "-runTests");
        }
    }
}
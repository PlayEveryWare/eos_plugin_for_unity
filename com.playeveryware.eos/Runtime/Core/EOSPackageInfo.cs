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

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using UnityEngine;
    using Utility;
    using JsonUtility = Utility.JsonUtility;

    public static class EOSPackageInfo
    {
        public static readonly string ConfigFileName = "EpicOnlineServicesConfig.json";

        /// <summary>
        /// Path to the package.json file that contains information like version number.
        /// </summary>
        private static readonly string PACKAGE_JSON_FILE_PATH = FileSystemUtility.GetProjectPath() + "com.playeveryware.eos/package.json";

        /// <summary>
        /// Private backing field member allows for caching of the version so it
        /// only needs to be read from the package.json file once.
        /// </summary>
        private static PackageJson _packageJsonFileContents;

        private static void ReadAndCachePackageJsonFile()
        {
            try
            {
                // Read the package.json file content.
                if (FileSystemUtility.FileExists(PACKAGE_JSON_FILE_PATH))
                {
                    string jsonContent = FileSystemUtility.ReadAllText(PACKAGE_JSON_FILE_PATH);

                    // Parse the JSON content using JsonUtility.
                    _packageJsonFileContents = JsonUtility.FromJson<PackageJson>(jsonContent);
                }
                else
                {
                    Debug.LogError($"The package.json file could not be found at: {PACKAGE_JSON_FILE_PATH}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while reading the package.json file: " + ex.Message);
            }
        }

        /// <summary>
        /// The current string representation of the version of the plugin as
        /// defined within the package.json file.
        /// </summary>
        public static string Version
        {
            get
            {
                if (null != _packageJsonFileContents)
                {
                    ReadAndCachePackageJsonFile();
                }

                return _packageJsonFileContents?.version;
            }
        }

        /// <summary>
        /// The current string representation of the version of the plugin as
        /// defined within the package.json file.
        /// </summary>
        public static string PackageName
        {
            get
            {
                if (null == _packageJsonFileContents)
                {
                    ReadAndCachePackageJsonFile();
                }

                return _packageJsonFileContents?.name;
            }
        }

        /// <summary>
        /// Class representing the part of the package.json file that contains
        /// the version information.
        /// </summary>
        [Serializable]
        private class PackageJson
        {
            public string version;
            public string name;
        }
    }
}

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

namespace PlayEveryWare.EpicOnlineServices
{
    using System.Collections.Generic;
    using System;
    using System.IO;

#if UNITY_EDITOR
    using UnityEditor;

#endif

    using UnityEngine;

    public static class PlatformManager
    {
        /// <summary>
        /// Enum that stores the possible platforms
        /// </summary>
        public enum Platform
        {
            Unknown,
            Windows,
            Android,
            XboxOne,
            XboxSeriesX,
            iOS,
            Linux,
            macOS,
            PS4,
            PS5,
            Switch,
            Steam
        }

        private struct PlatformInfo
        {
            public string FullName;
            public string ConfigFileName;
            public Type ConfigType;
            public string DynamicLibraryExtension;
        }

        /// <summary>
        /// Private collection to store associations between a platform type, it's human readable name, the file in which the configuration is stored, and the type of the config object
        /// </summary>
        private static IDictionary<Platform, PlatformInfo> PlatformInformation =
            new Dictionary<Platform, PlatformInfo>();

        /// <summary>
        /// Backing value for the CurrentPlatform property.
        /// </summary>
        private static Platform s_CurrentPlatform;

        /// <summary>
        /// Returns the current platform. In-order to reduce the number of places in the build pipeline
        /// where build defines are if-branched, this value is designed to be set from outside of the
        /// static class "PlatformManager."
        /// </summary>
        public static Platform CurrentPlatform
        {
            get { return s_CurrentPlatform; }
            set
            {
                // This is to ensure that the platform is only ever determined once
                if (CurrentPlatform == Platform.Unknown)
                {
                    s_CurrentPlatform = value;
                    Debug.Log($"CurrentPlatform has been assigned as {GetFullName(s_CurrentPlatform)}.");
                }
                else
                {
                    Debug.Log($"CurrentPlatform has already been assigned as {GetFullName(s_CurrentPlatform)}.");
                }

            }
        }

        static PlatformManager()
        {
            AddPlatformInfo(Platform.Android, "Android", "eos_android_config.json", typeof(AndroidConfig), ".so");
            AddPlatformInfo(Platform.iOS, "iOS", "eos_ios_config.json", typeof(IOSConfig), ".dylib");
            AddPlatformInfo(Platform.Linux, "Linux", "eos_linux_config.json", typeof(LinuxConfig), ".so");
            AddPlatformInfo(Platform.macOS, "macOS", "eos_macos_config.json", typeof(MacOSConfig), ".dylib");
            AddPlatformInfo(Platform.Steam, "Steam", "eos_steam_config.json", typeof(EOSSteamConfig), "");
            //// TODO: Currently, there is no special config that is utilized for Windows - instead current implementation simply
            //// relies on EpicOnlineServicesConfig.json, so for now this entry is different. What is commented below is what it *should* be to be consistent.
            //// AddPlatformInfo(Platform.Windows,     "Windows",         "eos_windows_config.json", typeof(EOSWindowsConfig), ".dll");
            //// For the time being, this is the entry for the Windows platform
            AddPlatformInfo(Platform.Windows, "Windows", "EpicOnlineServicesConfig.json", typeof(EOSConfig), ".dll");
        }

        private static void AddPlatformInfo(Platform platform, string fullName, string configFileName, Type configType, string dynamicLibraryExtension)
        {
            PlatformInformation.Add(new KeyValuePair<Platform, PlatformInfo>(platform,
                new PlatformInfo()
                {
                    FullName = fullName,
                    ConfigFileName = configFileName,
                    ConfigType = configType,
                    DynamicLibraryExtension = dynamicLibraryExtension
                }));
        }

#if UNITY_EDITOR
        /// <summary>
        /// Maps Unity BuildTarget to Platform
        /// </summary>
        private static readonly IDictionary<BuildTarget, Platform> TargetToPlatformsMap =
            new Dictionary<BuildTarget, Platform>()
            {
                { BuildTarget.Android,             Platform.Android     },
                { BuildTarget.GameCoreXboxOne,     Platform.XboxOne     },
                { BuildTarget.GameCoreXboxSeries,  Platform.XboxSeriesX },
                { BuildTarget.iOS,                 Platform.iOS         },
                { BuildTarget.StandaloneLinux64,   Platform.Linux       },
                { BuildTarget.PS4,                 Platform.PS4         },
                { BuildTarget.PS5,                 Platform.PS5         },
                { BuildTarget.Switch,              Platform.Switch      },
                { BuildTarget.StandaloneWindows,   Platform.Windows     },
                { BuildTarget.StandaloneWindows64, Platform.Windows     }
            };

        /// <summary>
        /// Get the config type for the current platform.
        /// </summary>
        /// <returns>The config type for the current platform.</returns>
        public static Type GetConfigType()
        {
            return GetConfigType(PlatformManager.CurrentPlatform);
        }

        /// <summary>
        /// Returns the type of the PlatformConfig that holds configuration values for the indicated Platform.
        /// </summary>
        /// <param name="platform">The Platform to get the specific PlatformConfig type of.</param>
        /// <returns>Type of the specific PlatformConfig that represents the indicated Platform.</returns>
        public static Type GetConfigType(Platform platform)
        {
            return PlatformInformation[platform].ConfigType;
        }

#endif
        /// <summary>
        /// Return a string that represents the file extension used by the indicated platform for dynamic library files.
        /// </summary>
        /// <param name="platform">The platform to get the specific file extension for.</param>
        /// <returns>File extension for the dynamic library used by the indicated platform.</returns>
        public static string GetDynamicLibraryExtension(Platform platform)
        {
            return PlatformInformation[platform].DynamicLibraryExtension;
        }


        /// <summary>
        /// Get the file extension used by the current platform for dynamic library files.
        /// </summary>
        /// <returns>A string containing the file extension used for dynamic library files on the current platform.</returns>
        public static string GetDynamicLibraryExtension()
        {
            return GetDynamicLibraryExtension(CurrentPlatform);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns the type of the PlatformConfig that holds configuration values for the indicated BuildTarget
        /// </summary>
        /// <param name="target">The BuildTarget to get the specific PlatformConfig type of.</param>
        /// <returns>Type of the specific PlatformConfig that represents the indicated Platform.</returns>
        public static Type GetConfigType(BuildTarget target)
        {
            Platform platform = TargetToPlatformsMap[target];
            return GetConfigType(platform);
        }

        /// <summary>
        /// Return the fully qualified path to the configuration file for the given build target.
        /// </summary>
        /// <param name="target">The build target to get the configuration file for.</param>
        /// <returns>Fully qualified path.</returns>
        private static string GetConfigFilePath(BuildTarget target)
        {
            var platform = TargetToPlatformsMap[target];
            return GetConfigFilePath(platform);
        }
#endif
        /// <summary>
        /// Return the fully qualified path to the configuration file for the current platform.
        /// </summary>
        /// <returns>Fully qualified path.</returns>
        public static string GetConfigFilePath()
        {
            return GetConfigFilePath(CurrentPlatform);
        }

        /// <summary>
        /// Return the fully qualified path to the configuration file for the given platform.
        /// </summary>
        /// <param name="platform">The platform to get the configuration file for.</param>
        /// <returns>Fully qualified path.</returns>
        public static string GetConfigFilePath(Platform platform)
        {
            return Path.Combine(
                Application.streamingAssetsPath,
                "EOS",
                GetConfigFileName(platform)
                );
        }

        /// <summary>
        /// Try to retrieve the config file path for the indicated platform.
        /// </summary>
        /// <param name="platform">The platform to get the configuration file for.</param>
        /// <param name="configFilePath">The filepath to the configuration file.</param>
        /// <returns>True if there is a config file path for the indicated platform.</returns>
        public static bool TryGetConfigFilePath(Platform platform, out string configFilePath)
        {
            if (PlatformInformation.ContainsKey(platform))
            {
                configFilePath = GetConfigFilePath(platform);
                return true;
            }

            configFilePath = "";
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Try to retrieve the config file path for the indicated BuildTarget.
        /// </summary>
        /// <param name="target">The BuildTarget to get the configuration file for.</param>
        /// <param name="configFilePath">The filepath to the configuration file.</param>
        /// <returns>True if there is a config file path for the indicated BuildTarget.</returns>
        public static bool TryGetConfigFilePath(BuildTarget target, out string configFilePath)
        {
            var platform = TargetToPlatformsMap[target];
            return TryGetConfigFilePath(platform, out configFilePath);
        }
#endif

        /// <summary>
        /// Returns the name of the JSON file that contains configuration values for the given platform.
        /// </summary>
        /// <param name="platform">The platform to get the JSON file name of.</param>
        /// <returns>The JSON file that contains configuration values for the given platform.</returns>
        public static string GetConfigFileName(Platform platform)
        {
            return PlatformInformation[platform].ConfigFileName;
        }

        /// <summary>
        /// Get the name for the indicated platform.
        /// </summary>
        /// <param name="platform">The platform to get the full name for.</param>
        /// <returns>Full name of platform.</returns>
        public static string GetFullName(Platform platform)
        {
            return PlatformInformation[platform].FullName;
        }
    }
}
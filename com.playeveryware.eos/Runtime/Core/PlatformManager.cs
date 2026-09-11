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

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices
{
    using System.Collections.Generic;
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    using PlayEveryWare.Common.Utility;
#endif

#if !EXTERNAL_TO_UNITY
    using UnityEngine;
#endif
    using Utility;
    
    public static partial class PlatformManager
    {
        /// <summary>
        /// Enum that stores the possible platforms
        /// </summary>
        [Flags]
        public enum Platform
        {
            Unknown = 0x0,
            Windows = 0x1,
            Android = 0x2,
            XboxOne = 0x4,
            XboxSeriesX = 0x8,
            iOS = 0x10,
            Linux = 0x20,
            macOS = 0x40,
            PS4 = 0x80,
            PS5 = 0x100,
            Switch = 0x200,
            Steam = 0x400,
            Switch2 = 0x800,
            Console = PS4 | PS5 | XboxOne | XboxSeriesX | Switch | Switch2,
            Any = Unknown | Windows | Android | XboxOne | XboxSeriesX | iOS | Linux | macOS | PS4 | PS5 | Switch | Switch2 | Steam
        }

        internal readonly struct PlatformInfo
        {
            public string FullName { get;  }
            public string ConfigFileName { get; }
            public string DynamicLibraryExtension { get; }
            public string PlatformIconLabel { get; }
            public Func<PlatformConfig> GetConfigFunction { get; }
            public Type ConfigType { get; }

            private PlatformInfo(Func<PlatformConfig> getConfigFunction, Type configType, string fullName, string configFileName, string dynamicLibraryExtension,
                string platformIconLabel)
            {
                FullName = fullName;
                ConfigFileName = configFileName;
                DynamicLibraryExtension = dynamicLibraryExtension;
                PlatformIconLabel = platformIconLabel;
                GetConfigFunction = getConfigFunction;
                ConfigType = configType;
            }

            public static PlatformInfo Create<T>(string fullName, string configFileName, string dynamicLibraryExtension, string platformIconLabel) where T : PlatformConfig
            {
                return new PlatformInfo(Config.Get<T>, typeof(T), fullName, configFileName, dynamicLibraryExtension,
                    platformIconLabel);
            }
        }

        /// <summary>
        /// Internal collection to store information about each platform.
        /// </summary>
        internal static IDictionary<Platform, PlatformInfo> PlatformInformation = new Dictionary<Platform, PlatformInfo>();

        /// <summary>
        /// Returns a list of platforms for which configuration values can be
        /// set.
        /// </summary>
        public static IEnumerable<Platform> ConfigurablePlatforms
        {
            get
            {
                return PlatformInformation.Keys;
            }
        }

        /// <summary>
        /// Tries to retrieve an instance of the platform config for the
        /// indicated platform.
        /// </summary>
        /// <param name="platform">
        /// The platform to get the config for.
        /// </param>
        /// <param name="platformConfig">
        /// The PlatformConfig for the indicated platform.
        /// </param>
        /// <returns>
        /// True if a PlatformConfig instance was retrieved, false otherwise.
        /// </returns>
        public static bool TryGetConfig(Platform platform, out PlatformConfig platformConfig)
        {
            platformConfig = null;
            if (!PlatformInformation.TryGetValue(platform, out PlatformInfo info))
            {
                return false;
            }

            platformConfig = info.GetConfigFunction();
            return true;
        }

        /// <summary>
        /// Backing value for the CurrentPlatform property.
        /// </summary>
        private static Platform s_CurrentPlatform;

        /// <summary>
        /// Used to cache the PlatformConfig that pertains to the current
        /// platform.
        /// </summary>
        private static PlatformConfig s_platformConfig = null;

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
                    // Note that s_CurrentPlatform is not set in this context - making sure it's only set once.
                    // TODO: Investigate whether this has unintended consequences - where setting the value is
                    //       expected.
                    Debug.Log($"CurrentPlatform has already been assigned as {GetFullName(s_CurrentPlatform)}.");
                }

            }
        }

        /// <summary>
        /// Backing value for the CurrentTargetedPlatform property.
        /// </summary>
        private static Platform s_CurrentTargetedPlatform;

        // This compile conditional is here because this property is only 
        // meaningful in the context of the Unity Editor running.
#if UNITY_EDITOR
        /// <summary>
        /// Used to indicate what platform is currently being targeted for
        /// compilation. Used primarily to select the appropriate platform in
        /// config editors.
        /// </summary>
        public static Platform CurrentTargetedPlatform
        {
            get
            {
                if (!TryGetPlatform(EditorUserBuildSettings.activeBuildTarget, out Platform targetedPlatform))
                {
                    return Platform.Unknown;
                }

                return targetedPlatform;
            }
        }
#endif

        /// <summary>
        /// To be accessible to the platform manager, the static constructors 
        /// for each platform config need to be executed, this function ensures
        /// that happens.
        /// </summary>
        private static void InitializePlatformConfigs()
        {
            // This compile conditional is here because in the editor, it is
            // acceptable to use reflection to make sure all the
            // PlatformConfigs have their static constructors executed.
#if UNITY_EDITOR
            ReflectionUtility.CallStaticConstructorsOnDerivingClasses<PlatformConfig>();
#endif

#if !EXTERNAL_TO_UNITY
            PlatformInformation.Add(Platform.Android, PlatformInfo.Create<AndroidConfig>("Android", "eos_android_config.json", null, "Android"));
            PlatformInformation.Add(Platform.iOS, PlatformInfo.Create<IOSConfig>("iOS", "eos_ios_config.json", null, "iPhone"));
            PlatformInformation.Add(Platform.Linux, PlatformInfo.Create<LinuxConfig>("Linux", "eos_linux_config.json", ".so", "Standalone"));
            PlatformInformation.Add(Platform.macOS, PlatformInfo.Create<MacOSConfig>("macOS", "eos_macos_config.json", ".dylib", "Standalone"));
#endif
            PlatformInformation.Add(Platform.Windows, PlatformInfo.Create<WindowsConfig>("Windows", "eos_windows_config.json", ".dll", "Standalone"));
        }

        /// <summary>
        /// This partial method is defined here so that a partial class
        /// definition can provide implementation that initializes other
        /// platform configs.
        /// </summary>
        static partial void InitializeProprietaryPlatformConfigs();

        static PlatformManager()
        {
            InitializePlatformConfigs();
            InitializeProprietaryPlatformConfigs();

            // If external to unity, then we know that the current platform
            // is Windows.
#if EXTERNAL_TO_UNITY
            CurrentPlatform = Platform.Windows;
#else
            // If the Unity editor is _not_ currently running, then the "active"
            // platform is whatever the runtime application says it is
            if (TryGetPlatform(Application.platform, out Platform platform))
            {
                CurrentPlatform = platform;
            }
            else
            {
                CurrentPlatform = Platform.Unknown;
                Debug.LogWarning("Platform could not be determined.");
            }
#endif
        }

        public static PlatformConfig GetPlatformConfig()
        {
            if (s_platformConfig != null)
            {
                return s_platformConfig;
            }

            if (!PlatformInformation.TryGetValue(CurrentPlatform, out PlatformInfo platformInfo) || null == platformInfo.GetConfigFunction)
            {
                Debug.LogError($"Could not get platform config for platform \"{CurrentPlatform}\".");
                return null;
            }

            s_platformConfig = platformInfo.GetConfigFunction();

            return s_platformConfig;
        }

#if !EXTERNAL_TO_UNITY
        /// <summary>
        /// Maps Unity RuntimePlatform to Platform
        /// </summary>
        private static readonly IDictionary<RuntimePlatform, Platform> RuntimeToPlatformsMap =
            new Dictionary<RuntimePlatform, Platform>()
            {
                { RuntimePlatform.Android,            Platform.Android},
                { RuntimePlatform.IPhonePlayer,       Platform.iOS},
                { RuntimePlatform.PS4,                Platform.PS4},
                { RuntimePlatform.PS5,                Platform.PS5},
                { RuntimePlatform.GameCoreXboxOne,    Platform.XboxOne},
                { RuntimePlatform.XboxOne,            Platform.XboxOne},
                { RuntimePlatform.Switch,             Platform.Switch},
#if UNITY_6000_0_OR_NEWER
                { RuntimePlatform.Switch2,            Platform.Switch2},
#endif
                { RuntimePlatform.GameCoreXboxSeries, Platform.XboxSeriesX},
                { RuntimePlatform.LinuxPlayer,        Platform.Linux},
                { RuntimePlatform.LinuxEditor,        Platform.Linux},
                { RuntimePlatform.EmbeddedLinuxX64,   Platform.Linux},
                { RuntimePlatform.EmbeddedLinuxX86,   Platform.Linux},
                { RuntimePlatform.LinuxServer,        Platform.Linux},
                { RuntimePlatform.WindowsServer,      Platform.Windows},
                { RuntimePlatform.WindowsPlayer,      Platform.Windows},
                { RuntimePlatform.WindowsEditor,      Platform.Windows},
                { RuntimePlatform.OSXEditor,          Platform.macOS},
                { RuntimePlatform.OSXPlayer,          Platform.macOS},
                { RuntimePlatform.OSXServer,          Platform.macOS},
            };

        /// <summary>
        /// Get the platform that matches the given runtime platform.
        /// </summary>
        /// <param name="runtimePlatform">The active RuntimePlatform</param>
        /// <param name="platform">The platform for that RuntimePlatform.</param>
        /// <returns>True if platform was determined, false otherwise.</returns>
        public static bool TryGetPlatform(RuntimePlatform runtimePlatform, out Platform platform)
        {
            return RuntimeToPlatformsMap.TryGetValue(runtimePlatform, out platform);
        }
#endif

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
#if UNITY_6000_0_OR_NEWER
                { BuildTarget.Switch2,            Platform.Switch2},
#endif
                { BuildTarget.StandaloneOSX,       Platform.macOS       },
                { BuildTarget.StandaloneWindows,   Platform.Windows     },
                { BuildTarget.StandaloneWindows64, Platform.Windows     },
            };

        /// <summary>
        /// Returns an IEnumerable of the platforms for which the current Unity
        /// Editor is able to build projects for.
        /// </summary>
        /// <returns>
        /// IEnumerable of the platforms for which the current Unity Editor is
        /// able to build projects for.
        /// </returns>
        public static IEnumerable<Platform> GetAvailableBuildTargets()
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);

                if (!BuildPipeline.IsBuildTargetSupported(buildTargetGroup, target))
                    continue;

                if (!TargetToPlatformsMap.TryGetValue(target, out Platform supportedPlatform))
                    continue;

                yield return supportedPlatform;
            }
        }

        /// <summary>
        /// Get the platform that matches the given build target.
        /// </summary>
        /// <param name="target">The build target being built for.</param>
        /// <param name="platform">The platform for that build target.</param>
        /// <returns>True if platform was determined, false otherwise.</returns>
        public static bool TryGetPlatform(BuildTarget target, out Platform platform)
        {
            return TargetToPlatformsMap.TryGetValue(target, out platform);
        }

        public static bool TryGetConfigType(Platform platform, out Type configType)
        {
            configType = null;

            bool typeFound = PlatformInformation.TryGetValue(platform, out PlatformInfo value);

            if (typeFound)
            {
                configType = value.ConfigType;
            }

            return typeFound;
        }

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

        /// <summary>
        /// Return the built-in icon for the indicated platform.
        /// </summary>
        /// <param name="platform">The platform to get the icon for.</param>
        /// <returns>An icon texture representing the platform.</returns>
        public static Texture GetPlatformIcon(Platform platform)
        {
            return EditorGUIUtility.IconContent($"BuildSettings.{PlatformInformation[platform].PlatformIconLabel}.Small").image;
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
            return FileSystemUtility.CombinePaths(
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
            if (PlatformInformation.TryGetValue(platform, out PlatformInfo value))
            {
                return value.FullName;
            }
            return platform.ToString();
        }
    }
}

#endif
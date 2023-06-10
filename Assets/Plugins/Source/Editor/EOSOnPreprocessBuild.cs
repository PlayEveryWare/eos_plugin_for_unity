using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using PlayEveryWare.EpicOnlineServices;

public class EOSOnPreprocessBuild : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        //if (report.summary.platform == BuildTarget.StandaloneWindows || report.summary.platform == BuildTarget.StandaloneWindows64)
        if (EOSPreprocessUtilities.isEOSDisableScriptingDefineEnabled(report))
        {
            return;
        }

        Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);

        if (report.summary.platform == BuildTarget.StandaloneOSX)
        {
            
            string macOSPacakgePluginFolder = Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "Runtime", "macOS");
            if (!File.Exists(Path.Combine(macOSPacakgePluginFolder, "libDynamicLibraryLoaderHelper.dylib")) || !File.Exists(Path.Combine(macOSPacakgePluginFolder, "MicrophoneUtility_macos.dylib")))
            {
                string macOSPluginFolder = Path.Combine(Application.dataPath, "Plugins", "macOS");
                if (!File.Exists(Path.Combine(macOSPluginFolder, "libDynamicLibraryLoaderHelper.dylib")) || !File.Exists(Path.Combine(macOSPluginFolder, "MicrophoneUtility_macos.dylib")))
                {

                    Debug.LogError("Custom native libraries missing for mac build, use the makefile in NativeCode/DynamicLibraryLoaderHelper_macOS to install the libraries");
                }
            }
        }

        AutoSetProductVersion();
    }

    public void AutoSetProductVersion()
    {
#if !EOS_DISABLE
        var eosVersionConfigSection = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorPrebuildConfigSection>();

        if (eosVersionConfigSection == null)
        {
            return;
        }

        eosVersionConfigSection.Awake();

        string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSPackageInfo.ConfigFileName);
        var eosConfigFile = new EOSConfigFile<EOSConfig>(configFilePath);
        eosConfigFile.LoadConfigFromDisk();

        var previousProdVer = eosConfigFile.currentEOSConfig.productVersion;
        var currentSectionConfig = eosVersionConfigSection.GetCurrentConfig();

        if (currentSectionConfig == null)
        {
            return;
        }

        if (currentSectionConfig.useAppVersionAsProductVersion)
        {
            eosConfigFile.currentEOSConfig.productVersion = Application.version;
        }

        if (previousProdVer != eosConfigFile.currentEOSConfig.productVersion)
        {
            eosConfigFile.SaveToJSONConfig(true);
        }
#endif
    }
}
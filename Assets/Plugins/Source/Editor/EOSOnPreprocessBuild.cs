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

        Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);

        AutoSetProductVersion();
    }

    public void AutoSetProductVersion()
    {
        var eosVersionConfigSession = EOSPluginEditorConfigEditor.GetConfigurationSectionEditor<EOSPluginEditorPrebuildConfigSection>();

        string configFilePath = Path.Combine(Application.streamingAssetsPath, "EOS", EOSManager.ConfigFileName);
        var eosConfigFile = new EOSConfigFile<EOSConfig>(configFilePath);
        eosConfigFile.LoadConfigFromDisk();

        var previousProdVer = eosConfigFile.currentEOSConfig.productVersion;

        if (eosVersionConfigSession.GetCurrentConfig().useAppVersionAsProductVersion)
        {
            eosConfigFile.currentEOSConfig.productVersion = Application.version;
        }

        if (previousProdVer != eosConfigFile.currentEOSConfig.productVersion)
        {
            eosConfigFile.SaveToJSONConfig(true);
        }
    }
}
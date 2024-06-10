using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Editor
{
    [Serializable]
    public class PlatformExportInfo
    {
        [SerializeField]
        public string platform;

        [SerializeField]
        public string descPath;

        [SerializeField]
        public bool isGettingExported;
    }

    [Serializable]
    public class PlatformExportInfoList
    {
        [SerializeField]
        public List<PlatformExportInfo> platformExportInfoList;

    }

    [Serializable]
    public class SampleExportInfo
    {
        [SerializeField]
        public string sample;

        [SerializeField]
        public string descPath;

        [SerializeField]
        public bool isGettingExported;
    }

    [Serializable]
    public class SampleExportInfoList
    {
        [SerializeField]
        public List<SampleExportInfo> sampleExportInfoList;

    }

    [Serializable]
    public class PackagePreset
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public string packageIdentifierPath;

        [SerializeField]
        public List<string> platformList;

        [SerializeField]
        public List<string> sampleList;
    }

    [Serializable]
    public class PackagePresetList
    {
        [SerializeField]
        public List<PackagePreset> packagePresetList;
    }
}

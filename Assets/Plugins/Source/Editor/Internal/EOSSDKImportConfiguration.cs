using System;
using System.Collections.Generic;
using UnityEngine;

namespace Playeveryware.Editor
{
    [Serializable]
    public class PlatformImportInfo
    {
        [SerializeField]
        public string platform;

        [SerializeField]
        public string descPath;

        [SerializeField]
        public bool isGettingImported;
    }

    [Serializable]
    public class PlatformImportInfoList
    {
        [SerializeField]
        public List<PlatformImportInfo> platformImportInfoList;

    }
}

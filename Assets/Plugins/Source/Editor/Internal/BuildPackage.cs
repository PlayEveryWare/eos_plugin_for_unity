using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public static class BuildPackage
{
    private const string OUTPUT_DIRECTORY = "Build";

    public enum PackageType 
    {
        UPM,
        DotUnityPackage,
        Asset,
    };

    //[MenuItem("Tools/Create Package/Export .UPM")]
    public static void ExportPackageUPM() 
    {
        ExportPackage(PackageType.UPM);
    }

    //[MenuItem("Tools/Create Package/Export .unitypackage")]
    public static void ExportPackageUnityPackage() 
    {
        ExportPackage(PackageType.DotUnityPackage);
    }

    //[MenuItem("Tools/Create Package/Export .UPM (Uncompressed)")]
    public static void ExportPackageUPMUncompressed() 
    {
        ExportPackage(PackageType.Asset);    
    }

    private static void ExportPackage(PackageType Type)
    {
        Debug.Log("Exporting " + Type.ToString() + " type package.");
    }

    public static void ExportPlugin() 
    {
    }
}

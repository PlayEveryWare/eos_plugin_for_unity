using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

public static class BuildPackage
{
    /// <summary>
    /// For the time being and for simplicity we will hard-code these values
    /// </summary>
    private const string OUTPUT_DIRECTORY = "Build";

    /// <summary>
    /// This is the JSON file that defines which files to put into the UPM directory.
    /// @TODO: Handle the scenario where whomever is developing the UPM wants to introduce a new file.
    /// Is there a way that we could intelligently include that and add it to this JSON without things
    /// getting really messy?
    /// </summary>
    private const string RELATIVE_JSON_DESC_PATH = "PackageDescriptionConfigs/eos_package_description.json";

    /// <summary>
    /// Note that this is a Non-Unity defined command line argument.
    /// </summary>
    private const string OUTPUT_ARG_FLAG = "-EOS_CLI_Build";


    /// <summary>
    /// Return the project root directory path
    /// </summary>
    /// <returns>The path to the root of the project.</returns>
    private static string GetProjectRoot()
    {
        string DataPath = Application.dataPath;
        return DataPath.Substring(0, DataPath.LastIndexOf("/Assets"));
    }

    /// <summary>
    /// Returns the relative path 
    /// </summary>
    /// <returns></returns>
    private static string GetOutputPath()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; ++i)
        {
            if (OUTPUT_ARG_FLAG == args[i] && i + 1 < args.Length)
            {
                // We return the argument provided as-is. 
                return args[i + 1];
            }
        }

        // In this case we want the absolute path, because we don't
        // want any code down-stream to try and use it as a relative path.
        // Note that it's resolved using the project's root directory.
        // TODO: Handle scenario where path does not exist, or the output
        // directory exists, but is not empty.
        return System.IO.Path.GetFullPath(
            System.IO.Path.Join(
                GetProjectRoot(),
                OUTPUT_DIRECTORY
            )
        );
    } 

    /// <summary>
    ///Does the exporting of the plugin to a UPM directory 
    /// </summary>
    public static void ExportPlugin() 
    {
        // Debug: Print Out command line arguments
        foreach (var arg in System.Environment.GetCommandLineArgs())
        {
            Debug.Log("CLA: " + arg);
        }

        // Get output directory
        string OutputDirectory = GetOutputPath();

        // if the output directory is not already fully qualified,
        // then we will assume it's relative to the place the "Unity.exe" 
        // command was executed.
        if (!System.IO.Path.IsPathFullyQualified(OutputDirectory))
        {
            OutputDirectory = System.IO.Path.GetFullPath(
                System.IO.Path.Join(
                    System.IO.Directory.GetCurrentDirectory(),
                    OutputDirectory
                )
            );
        }

        string jsonFile = System.IO.Path.Join(
            GetProjectRoot(),
            RELATIVE_JSON_DESC_PATH
        );

        // Validate file paths
        if (!System.IO.File.Exists(jsonFile))
        {
            ExportError("JSON file \"" + jsonFile + "\"");
        }

        if (!System.IO.Directory.Exists(OutputDirectory))
        {
            System.IO.Directory.CreateDirectory(OutputDirectory);
            Debug.LogWarning("Output Directory: \"" + OutputDirectory + "\" created.");
        } 
        else
        {
            // TODO: Handle scenario where the directory exists, and is not empty.
        }

        Debug.Log("Exporting EOS Plugin UPM to \"" + OutputDirectory + "\"");

        // Create package
        UnityPackageCreationUtility.CreateUPMPackage(
            OutputDirectory,
            jsonFile
        );
    }

    /// <summary>
    /// Helper method to log and throw fatal errors that may occur during plugin export.
    /// </summary>
    /// <param name="message">The message to log / throw</param>
    private static void ExportError(string message)
    {
        Debug.LogError(message);
        throw new BuildFailedException(message);
    }
}
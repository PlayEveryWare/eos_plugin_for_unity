using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class StreamingAssetsFileEnumerator : MonoBehaviour
{
    public string relativePath = "StreamingAssets"; // The relative path within the APK

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded()
    {
        // Create a new GameObject that will persist through the scene and attach the enumerator
        GameObject enumeratorObject = new GameObject("StreamingAssetsFileEnumerator");
        enumeratorObject.AddComponent<StreamingAssetsFileEnumerator>();
        DontDestroyOnLoad(enumeratorObject);
    }

    void Start()
    {
        string[] paths = new[]
        {
            Application.dataPath,
            Application.streamingAssetsPath,
            Application.persistentDataPath,
            Application.consoleLogPath,
            ""
        };

        List<string> allPaths = new();

        foreach (string path in paths)
        {
            string pathToUse = path;
            if (!pathToUse.EndsWith("/"))
                pathToUse += "/";

            allPaths.Add(pathToUse);
        }

        foreach (string path in allPaths)
        {
            StartCoroutine(GetFilesInStreamingAssets(path));
        }
    }

    IEnumerator GetFilesInStreamingAssets(string basePath, string path = null)
    {
        string streamingAssetsPath = (path != null) ? Path.Combine(basePath, path) : Path.Combine(basePath);

        Debug.Log("Reading files from: " + streamingAssetsPath);

        // Create a UnityWebRequest to get the directory listing as a file.
        UnityWebRequest request = UnityWebRequest.Get(streamingAssetsPath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load: " + request.error);
            yield break;
        }

        // The response text should contain the list of files. (In case you have an index file)
        string[] files = request.downloadHandler.text.Split('\n');

        foreach (var file in files)
        {
            string filePath = Path.Combine(streamingAssetsPath, file.Trim());

            // Recursively load subdirectories
            if (file.EndsWith("/")) // Assuming directories end with '/'
            {
                StartCoroutine(GetFilesInStreamingAssets(basePath, filePath));
            }
            else
            {
                Debug.Log("Found file: " + filePath);
                // You can add your logic here to process the file (e.g., load or display)
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AndroidFileIOHelper : MonoBehaviour
{
    public static string ReadAllText(string filePath) 
    {
        if (Platform.IS_ANDROID && Platform.IS_EOS_PREVIEW_ENABLED)
        {
#pragma warning disable CS0162 // Unreachable code on some platforms
            using (var request = UnityEngine.Networking.UnityWebRequest.Get(filePath))
#pragma warning restore CS0162 // Unreachable code on some platforms
            {
                request.timeout = 2; //seconds till timeout
                request.SendWebRequest();

                //Wait till webRequest completed
                while (!request.isDone) { }

                if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.Log("Requesting " + filePath + ", please make sure it exists and is a valid config");
                    throw new Exception("UnityWebRequest didn't succeed, Result : " + request.result);
                }

                return request.downloadHandler.text;
            }
        }
        else
        {
#pragma warning disable CS0162 // Unreachable code on some platforms
            Debug.LogError("Attempting to access Android storage while not on the Android platform");
#pragma warning restore CS0162 // Unreachable code on some platforms
            return "not on android";
        }
    }
}

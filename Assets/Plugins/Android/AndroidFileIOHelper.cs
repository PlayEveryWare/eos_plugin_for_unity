using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AndroidFileIOHelper : MonoBehaviour
{
    public static string ReadAllText(string filePath) 
    {
        using (var request = UnityEngine.Networking.UnityWebRequest.Get(filePath))
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
}

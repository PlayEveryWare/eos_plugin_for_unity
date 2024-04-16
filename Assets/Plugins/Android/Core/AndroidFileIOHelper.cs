/*
 * Copyright (c) 2024 PlayEveryWare
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

namespace PlayEveryWare.EpicOnlineServices
{
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

#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.Log("Requesting " + filePath + ", please make sure it exists and is a valid config");
                    throw new Exception("UnityWebRequest didn't succeed, Result : " + request.result);
                }
#else
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log("Requesting " + filePath + ", please make sure it exists and is a valid config");
                throw new Exception("UnityWebRequest didn't succeed : Network or HTTP Error");
            }
#endif
                return request.downloadHandler.text;
            }
        }
    }
}

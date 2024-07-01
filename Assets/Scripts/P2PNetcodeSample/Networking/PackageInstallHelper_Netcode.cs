/*
* Copyright (c) 2023 PlayEveryWare
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
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace PlayEveryWare.EpicOnlineServices.Samples.Network
{
    [InitializeOnLoad]
    public class PackageInstallHelper_Netcode
    {
        static PackageInstallHelper_Netcode()
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogWarning("Package : [com.unity.netcode.gameobjects] required, attempting to install...");

            AddRequest request;
            request = Client.Add("com.unity.netcode.gameobjects@1.0.2");
            while (request.Status == StatusCode.InProgress)
            {
            }
            if (request.Result != null)
            { 
                Debug.Log("[com.unity.netcode.gameobjects@1.0.2] successfully installed"); 
            }
            else
            {
                Debug.Log("[com.unity.netcode.gameobjects@1.0.2] Request Failed : " + request.Error.ToString());
            }
#endif
        }
    }
}
#endif


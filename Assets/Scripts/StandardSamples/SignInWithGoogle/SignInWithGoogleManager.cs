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

using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class SignInWithGoogleManager : MonoBehaviour
    {
        AndroidJavaObject loginObject;

        public void GetGoogleIdToken(System.Action<string, string> callback)
        {
            using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
            {
                Debug.LogError("EOSAndroid: activity context is null!");
                return;
            }

            using AndroidJavaClass loginClass = new AndroidJavaClass("com.playeveryware.googlelogin.login");
            if (loginClass == null)
            {
                Debug.LogError("Java Login Class is null!");
                return;
            }

            loginObject = loginClass.CallStatic<AndroidJavaObject>("instance");

            /// Create the proxy class and pass instances to be used by the callback
            EOSCredentialManagerCallback javaCallback = new EOSCredentialManagerCallback();
            javaCallback.loginObject = loginObject;
            javaCallback.callback = callback;

            AndroidConfig config = AndroidConfig.Get<AndroidConfig>();

            if (string.IsNullOrEmpty(config.GoogleLoginClientID))
            {
                Debug.LogError("Client ID is null, needs to be configured for Google ID connect login");
                return;
            }

            /// SignInWithGoogle(String clientID, String nonce, Context context, CredentialManagerCallback callback)
            loginObject.Call("SignInWithGoogle", config.GoogleLoginClientID, config.GoogleLoginNonce, activity, javaCallback);
        }

        class EOSCredentialManagerCallback : AndroidJavaProxy
        {
            public AndroidJavaObject loginObject;
            public System.Action<string, string> callback;

            /// <summary>
            /// Proxy class to receive Android callbacks in C#
            /// </summary>
            public EOSCredentialManagerCallback() : base("androidx.credentials.CredentialManagerCallback") { }

            /// <summary>
            /// Succeeding Callback of GetCredentialAsync  
            /// GetCredentialAsync is called in com.playeveryware.googlelogin.login.SignInWithGoogle)
            /// </summary>
            /// <param name="credentialResponseResult"></param>
            public void onResult(AndroidJavaObject credentialResponseResult)
            {
                /// Parses the response resilt into google credentials
                loginObject.Call("handleSignIn", credentialResponseResult);

                /// Invoke Connect Login with fetched Google ID
                callback.Invoke(loginObject.Call<string>("getResultIdToken"), loginObject.Call<string>("getResultName"));
            }

            /// <summary>
            /// Failing Callback of GetCredentialAsync  
            /// GetCredentialAsync is called in com.playeveryware.googlelogin.login.SignInWithGoogle)
            /// </summary>
            /// <param name="credentialException"></param>
            public void onError(AndroidJavaObject credentialException)
            {
                loginObject.Call("handleFailure", credentialException);
                callback.Invoke(null, null);
            }
        }
    }
}
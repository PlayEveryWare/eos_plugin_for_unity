/*
* Copyright (c) 2021 PlayEveryWare
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

#if !OCULUS_MODULE
#define DISABLEOCULUS
#endif

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

#if !DISABLEOCULUS
using OculusWrapper = Oculus; //if erroring Dont forget to import Oculus' .unitypackage
#endif

namespace PlayEveryWare.EpicOnlineServices.Samples.Oculus
{
    [DisallowMultipleComponent]
    public class OculusManager : MonoBehaviour
    {
#if !DISABLEOCULUS
        protected static OculusManager s_instance;
        public static OculusManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    return new GameObject("OculusManager").AddComponent<OculusManager>();
                }
                else
                {
                    return s_instance;
                }
            }
        }

        protected bool m_bInitialized = false;
        public static bool Initialized
        {
            get
            {
                return Instance.m_bInitialized;
            }
        }

        private OculusWrapper.Platform.Models.User u;

#if UNITY_2019_3_OR_NEWER
        // In case of disabled Domain Reload, reset static members before entering Play Mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            s_instance = null;
        }
#endif

        protected virtual void Awake()
        {
            // Only one instance of OculusManager at a time!
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;

            // We want our OculusManager Instance to persist across scenes.
            DontDestroyOnLoad(gameObject);

            m_bInitialized = true;
        }

        protected virtual void OnEnable()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }

            if (!m_bInitialized)
            {
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            s_instance = null;
        }


        public void GetUserProof(Action<string, string> callback)
        {

            //OculusWrapper.Platform.Core.Initialize("[APP ID here]");

            if (OculusWrapper.Platform.Core.IsInitialized())    //set the above APP ID to the one specified in the Oculus dev portal for your application, this is for test purposes, and not needed if youre initializing Oculus elsewhere.
            {
                OculusWrapper.Platform.Users.GetLoggedInUser().OnComplete(
                    (OculusWrapper.Platform.Message<OculusWrapper.Platform.Models.User> user) => 
                        { u = user.Data; }
                    );
                OculusWrapper.Platform.Users.GetUserProof().OnComplete(
                    (OculusWrapper.Platform.Message<OculusWrapper.Platform.Models.UserProof> msg) => 
                        { userProofCallback(msg, callback); }
                    );
            }
            else
            {
                Debug.LogError("Oculus core not IsInitialized yet");
                userProofCallback(null, callback);
            }
        }


        
        private void userProofCallback(OculusWrapper.Platform.Message<OculusWrapper.Platform.Models.UserProof> msg, Action<string,string> callback)
        {
            if (msg!=null && !msg.IsError)
            {
                OculusWrapper.Platform.Models.UserProof userNonce = msg.Data;
                print("Received user nonce generation success. Nonce: " + userNonce.Value); print("id: " + u.ID); print("ocuid: " + u.OculusID);

                callback?.Invoke($"{u.ID}|{userNonce.Value}", u.OculusID);
            }
            else
            {
                if(msg!=null){
                    OculusWrapper.Platform.Models.Error error = msg.GetError();
                    print("Received user nonce generation error. Error: " + error.Message);
                }
                callback?.Invoke(null,null);
            }

        }


#else
        public static OculusManager Instance
        {
            get
            {
                return null;
            }
        }

        public void GetUserProof(Action<string, string> callback)
        {
            callback?.Invoke(null,null);
        }
#endif
    }
}
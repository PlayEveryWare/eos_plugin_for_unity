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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !EOS_DISABLE
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.UserInfo;
using Epic.OnlineServices.UI;
#endif

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class EOSHostManager : MonoBehaviour, IEOSCoroutineOwner
    {
        void Awake()
        {
#if !EOS_DISABLE

#if UNITY_PS5 && !UNITY_EDITOR
            EOSPSNManager.EnsurePS5Initialized();
#endif

            EOSManager.Instance.Init(this);
#endif
        }

        void IEOSCoroutineOwner.StartCoroutine(IEnumerator routine)
        {
            base.StartCoroutine(routine);
        }
    }
}
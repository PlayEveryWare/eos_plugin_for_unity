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

using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

#if !EOS_DISABLE
using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;
#endif

#if !UNITY_STANDALONE_WIN || !UNITY_EDITOR_WIN || !UNITY_WSA_10_0 || !UNITY_SWITCH || !UNITY_PS4 || !UNITY_PS5 || !UNITY_ANDROID

//-------------------------------------------------------------------------
// Default impl for new platforms.
// Most platforms will have their actual init code in natie code, and thus won't need
// to do this.
//namespace PlayEveryWare.EpicOnlineServices
//{
//    public partial class EOSManager
//    {
//        //-------------------------------------------------------------------------
//        static string GetTempDir()
//        {
//            return Application.temporaryCachePath;
//        }


//        //-------------------------------------------------------------------------
//        static private void ConfigureSystemInitOptions(ref InitializeOptions initializeOptions)
//        {
//        }

//        //-------------------------------------------------------------------------
//        static private InitializeOptions CreateSystemInitOptions()
//        {
//            return new InitializeOptions();
//        }

//        //-------------------------------------------------------------------------
//        // TODO merge this with the ConfigureSystemPlatformCreateOptions?
//        static private Options CreateSystemPlatformOption()
//        {
//            var createOptions = new Options();

//            return createOptions;
//        }

//        //-------------------------------------------------------------------------
//        static private void ConfigureSystemPlatformCreateOptions(ref Options createOptions)
//        {
//        }
//    }
//}
#endif


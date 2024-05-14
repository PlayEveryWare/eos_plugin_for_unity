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

#if !EOS_DISABLE
using UnityEngine;
using UnityEngine.Scripting;
using System.Runtime.InteropServices;

// If iOS and not editor.
#if UNITY_IOS && !UNITY_EDITOR

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Platform;

    public class EOSCreateOptions
    {
        public Epic.OnlineServices.Platform.Options options;
    }

    public class EOSInitializeOptions
    {
        public Epic.OnlineServices.Platform.InitializeOptions options;
    }

    public class IOSPlatformSpecifics : PlatformSpecifics<IOSConfig>
    {
        public IOSPlatformSpecifics() : base(PlatformManager.Platform.iOS, ".dylib") { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static public void Register()
        {
            EOSManagerPlatformSpecificsSingleton.SetEOSManagerPlatformSpecificsInterface(new IOSPlatformSpecifics());
        }
        
        /// <summary>
        /// Set Default Audio Session for iOS (Category to AVAudioSessionCategoryPlayAndRecord)
        /// </summary>
        [DllImport("__Internal")]
        static private extern void MicrophoneUtility_set_default_audio_session();

        /// <summary>
        /// Set Default Audio Session for iOS
        /// </summary>
        public override void SetDefaultAudioSession()
        {
            MicrophoneUtility_set_default_audio_session();
        }
    }
}
#endif
#endif
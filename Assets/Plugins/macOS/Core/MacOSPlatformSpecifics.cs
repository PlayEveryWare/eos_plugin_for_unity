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

#if !EOS_DISABLE

using UnityEngine;
using UnityEngine.Scripting;

// If standalone osx and not editor, or the osx editor.
#if (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_EDITOR_OSX

namespace PlayEveryWare.EpicOnlineServices 
{
    using Epic.OnlineServices.Platform;

    public class EOSCreateOptions
    {
        public Options options;
    }

    public class EOSInitializeOptions
    {
        public InitializeOptions options;
    }

    //-------------------------------------------------------------------------
    public class MacOSPlatformSpecifics : PlatformSpecifics<MacOSConfig>
    {
        public MacOSPlatformSpecifics() : base(PlatformManager.Platform.macOS, ".dylib") { }

        //-------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static public void Register()
        {
            EOSManagerPlatformSpecificsSingleton.SetEOSManagerPlatformSpecificsInterface(new MacOSPlatformSpecifics());
        }
    }
}

#endif // (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX) && !UNITY_EDITOR_WIN
#endif // !EOS_DISABLE
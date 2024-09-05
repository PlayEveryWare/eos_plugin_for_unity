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

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;

    //-------------------------------------------------------------------------
    public interface IPlatformSpecifics
    {
#if !EOS_DISABLE
        string GetTempDir();

        void AddPluginSearchPaths(ref List<string> pluginPaths);

        string GetDynamicLibraryExtension();

//#if EOS_DYNAMIC_BINDINGS
        // Only called if EOS_DYNAMIC_BINDINGS is defined
        void LoadDelegatesWithEOSBindingAPI();
//#endif

        void ConfigureSystemInitOptions(ref EOSInitializeOptions initializeOptions);

        void ConfigureSystemPlatformCreateOptions(ref EOSCreateOptions createOptions);

        void InitializeOverlay(IEOSCoroutineOwner owner);

        void RegisterForPlatformNotifications();

        bool IsApplicationConstrainedWhenOutOfFocus();

        Int32 IsReadyForNetworkActivity();

        /// <summary>
        /// Sets the default audio session.
        /// NOTE: This is only implemented for iOS
        /// </summary>
        void SetDefaultAudioSession();

        void UpdateNetworkStatus();
#endif
    }
}

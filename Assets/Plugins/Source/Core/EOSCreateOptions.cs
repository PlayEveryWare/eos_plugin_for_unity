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
namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Platform;
    using Epic.OnlineServices;
    using System;
    using Epic.OnlineServices.IntegratedPlatform;

    public class EOSCreateOptions //: IEOSCreateOptions
    {
        #region Platform-Specific "option" field declaration

        public
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Epic.OnlineServices.Platform.WindowsOptions
#else
        Epic.OnlineServices.Platform.Options
#endif
        options;

        #endregion

        #region Platform-Specific properties

#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
        public IntegratedPlatformOptionsContainer IntegratedPlatformOptionsContainerHandle { get => options.IntegratedPlatformOptionsContainerHandle; set => options.IntegratedPlatformOptionsContainerHandle = value; }
#endif

        #endregion
    }
}

#endif //EOS_DISABLED
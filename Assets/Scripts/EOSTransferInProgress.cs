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


namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using UnityEngine;
    using Utility;

    /// <summary>
    /// Class <c>EOSTransferInProgress</c> is used in <c>EOSTitleStorageManager</c> and <c>PlayerDataStorageService</c> to keep track of downloaded cached file data.
    /// </summary>
    public class EOSTransferInProgress
    {
        // per EOS SDK documentation, the maximum size for a file is 200MB, or this many bytes.
        public const int FileMaxSizeBytes = 200000000;

        public bool Download = true;
        public uint CurrentIndex = 0;
        private byte[] _data;

        public byte[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                // If the file sizer is larger than the maximum allowable size,
                // then log an error, but do not throw an exception, since
                // throwing an exception from a property setter is a little
                // confusing, and there will be an opportunity to catch the 
                // mistake when EOS returns an error code.
                if (null != value && value.Length > FileMaxSizeBytes)
                {
                    Debug.LogError($"Maximum file size is 200MB.");
                }

                _data = value;
            }
        }

        public uint TotalSize
        {
            get
            {
                if (null == _data)
                    return 0;

                _ = SafeTranslatorUtility.TryConvert(_data.Length, out uint totalSize);

                return totalSize;
            }
        }

        public bool IsDone()
        {
            return TotalSize == CurrentIndex;
        }
    }
}
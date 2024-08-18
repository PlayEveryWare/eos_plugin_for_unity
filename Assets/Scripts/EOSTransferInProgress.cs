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

using System.Collections.Generic;
using Epic.OnlineServices.PlayerDataStorage;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using Editor.Utility;

    /// <summary>
    /// Class <c>EOSTransferInProgress</c> is used in <c>EOSTitleStorageManager</c> and <c>EOSPlayerDataStorageManager</c> to keep track of downloaded cached file data.
    /// </summary>
    public class EOSTransferInProgress
    {
        // Per EOS SDK documentation, the maximum size for a file is 200MB.
        private const int FileMaxSizeBytes = 200000000;

        /// <summary>
        /// Indicates whether the transfer is a download or an upload.
        /// </summary>
        public bool Download = true;

        /// <summary>
        /// Current index of the file transfer.
        /// </summary>
        public uint CurrentIndex = 0;

        /// <summary>
        /// Stores the data for the transfer.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Property containing the Data to transfer. 
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                // If the data is too large, then log an error, but allow the 
                // property to still be set, since this would be a confusing
                // place to crash at, and because EOS will throw an error if 
                // an attempt is made to upload the file.
                if (null != value && value.Length > FileMaxSizeBytes)
                {
                    Debug.LogError($"Cannot transfer a file larger than 200MB.");
                }

                _data = value;
            }
        }

        /// <summary>
        /// The total size of the data to transfer, in bytes.
        /// </summary>
        public uint TotalSize
        {
            get
            {
                if (null == Data)
                    return 0;

                _ = SafeTranslatorUtility.TryConvert(Data.Length, out uint totalSize);
                return totalSize;
            }
        }

        /// <summary>
        /// Indicates whether the data is done being transferred.
        /// </summary>
        /// <returns>
        /// True if the data base been transferred, false otherwise.
        /// </returns>
        public bool IsDone()
        {
            return TotalSize == CurrentIndex;
        }
    }
}
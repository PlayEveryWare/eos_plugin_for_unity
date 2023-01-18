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
    /// <summary>
    /// Class <c>EOSTransferInProgress</c> is used in <c>EOSTitleStorageManager</c> and <c>EOSPlayerDataStorageManager</c> to keep track of downloaded cached file data.
    /// </summary>

    public class EOSTransferInProgress
    {
        public bool Download = true;
        public uint CurrentIndex = 0;
        public byte[] Data;
        private uint transferSize = 0;

        public uint TotalSize
        {
            get
            {
                return transferSize;
            }
            set
            {
                transferSize = value;

                if (transferSize > PlayerDataStorageInterface.FileMaxSizeBytes)
                {
                    Debug.LogError("[EOS SDK] Player data storage: data transfer size exceeds max file size.");
                    transferSize = PlayerDataStorageInterface.FileMaxSizeBytes;
                }
            }
        }

        public bool Done()
        {
            return TotalSize == CurrentIndex;
        }
    }
}
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
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using Editor.Utility;
    using System.Text;

    /// <summary>
    /// Base class to handle functionality common to data storage managers.
    /// </summary>
    public abstract class DataStorageManager
    {
        /// <summary>
        /// The maximum number of bytes to transfer at a time.
        /// </summary>
        protected const uint MAX_CHUNK_SIZE = 4096;

        protected const uint MaxFileSizeToDisplay = 5 * 1024 * 1024;

        /// <summary>
        /// Keeps track of the active transfers that are in progress.
        /// </summary>
        protected Dictionary<string, EOSTransferInProgress> _transfersInProgress = new();

        /// <summary>
        /// Stores the data to be managed locally.
        /// </summary>
        protected Dictionary<string, string> _storageCachedData = new();

        /// <summary>
        /// The name of the file currently being transfered.
        /// NOTE: This is a problematic field member to have if there are
        ///       going to be multiple transfers active at the same time.
        /// </summary>
        protected string _currentTransferName;

        /// <summary>
        /// Returns the locally cached data.
        /// </summary>
        /// <returns>
        /// Data cached locally, where the key is the file name, and the value
        /// is the contents for that file.
        /// </returns>
        public Dictionary<string, string> GetCachedStorageData()
        {
            return _storageCachedData; 
        }

        /// <summary>
        /// Used internally to provide implementation that is agnostic regarding
        /// whether the storage is player data or title data.
        /// </summary>
        protected enum FileRequestStatus
        {
            /// <summary>
            /// Indicates that the file request should continue.
            /// </summary>
            ContinueReading,

            /// <summary>
            /// Indicates that the request has failed.
            /// </summary>
            FailRequest,

            /// <summary>
            /// Indicates that the request has been canceled.
            /// </summary>
            CancelRequest
        }

        /// <summary>
        /// Used to handle when data is received.
        /// </summary>
        /// <param name="fileName">
        /// The filename the data is being received for.
        /// </param>
        /// <param name="data">
        /// The data being received.
        /// </param>
        /// <param name="totalSize">
        /// The number of bytes that will be transferred in total when the data
        /// has finished being received.
        /// </param>
        /// <returns>
        /// The status of the file request.
        /// </returns>
        protected FileRequestStatus ReceiveData(string fileName, ArraySegment<byte> data, uint totalSize)
        {
            if (data == null)
            {
                Debug.LogError("Could not receive data: Data pointer is null.");
                return FileRequestStatus.FailRequest;
            }

            if (!_transfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                return FileRequestStatus.CancelRequest;
            }

            if (!transfer.Download)
            {
                Debug.LogError("Can't load file data: the transfer is marked as an upload, not a download.");
                return FileRequestStatus.FailRequest;
            }

            // Is this the first update?
            if (transfer.CurrentIndex == 0)
            {
                // Allocate the new byte array.
                transfer.Data = new byte[totalSize];
            }

            // If more data has been received than was anticipated, fail the request
            if (transfer.TotalSize < transfer.CurrentIndex + data.Count)
            {
                Debug.LogError("Could not receive data: More data was returned than expected.");
                return FileRequestStatus.FailRequest;
            }

            // Copy the data 
            data.Array?.CopyTo(transfer.Data, transfer.CurrentIndex);

            // Advance the index by the amount of data received.
            transfer.CurrentIndex += (uint)data.Count;

            // Keep reading
            return FileRequestStatus.ContinueReading;
        }
    }
}
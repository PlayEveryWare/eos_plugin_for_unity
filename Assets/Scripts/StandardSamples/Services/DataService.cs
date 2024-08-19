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
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 */

namespace PlayEveryWare.EpicOnlineServices
{
    using Samples;
    using System;
    using System.Collections.Generic;
    using Debug = UnityEngine.Debug;
    using Epic.OnlineServices;

    public abstract class DataService<T> : EOSService where T : IFileTransferRequest
    {
        protected T CurrentTransferHandle;

        /// <summary>
        /// Used to describe the results of a file transfer agnostic of
        /// whether the transfer is with player storage or title storage.
        /// </summary>
        protected enum FileTransferResult
        {
            FailRequest,
            CancelRequest,
            ContinueReading
        }

        /// <summary>
        /// Indicates the maximum number of bytes that can be read at a time
        /// during data transfer operations.
        /// </summary>
        protected const uint MaxChunkSize = 4096;

        /// <summary>
        /// Stores the file transfers that are currently in progress for the
        /// given data service.
        /// </summary>
        protected Dictionary<string, EOSTransferInProgress> _transfersInProgress = new();

        /// <summary>
        /// Stores the locally cached copy of the data that the service manages.
        /// </summary>
        protected Dictionary<string, string> _locallyCachedData = new();

        /// <summary>
        /// Stores the name of the currently transferring file.
        /// </summary>
        protected string CurrentTransferName;

        /// <summary>
        /// Clears all the current transfers that the data service is managing.
        /// </summary>
        protected void ClearCurrentTransfer()
        {
            CurrentTransferName = string.Empty;
            CurrentTransferHandle?.Release();
            CurrentTransferHandle?.Dispose();
        }

        /// <summary>
        /// Retrieves the current local cache of data.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetLocallyCachedData()
        {
            return _locallyCachedData;
        }

        /// <summary>
        /// Clears local storage and clears the current transfers that are
        /// being managed by the data storage service. If overridden, make
        /// certain to first call this method within the overridden
        /// implementation.
        /// </summary>
        protected override void OnLoggedOut()
        {
            // TODO: Need to clear / cancel currently active transfer tasks.
            _locallyCachedData.Clear();
            ClearCurrentTransfer();
        }

        /// <summary>
        /// Attempts to cancel the "current" transfer handle (needs to be a
        /// field member added to derived classes).
        /// </summary>
        /// <returns>
        /// True if the transfer handle is not null, and canceling it was
        /// successful, false otherwise.
        /// </returns>
        protected bool TryCancelTransferHandle()
        {
            // Default to success, because if the transfer handle is null, then
            // the task of canceling the transfer can be considered to be a
            // success.
            if (null == CurrentTransferHandle)
            {
                return false;
            }

            // Cancel the transfer request handle, if it fails, then indicate
            // as much by returning false.
            Result cancelResult = CurrentTransferHandle.CancelRequest();
            if (Result.Success != cancelResult)
            {
                Debug.LogWarning($"Failed to cancel the transfer " +
                                 $"request. Result code: {cancelResult}.");
                return false;
            }

            // Return true if the result of canceling the request was
            // successful, and after releasing the handle and setting it to
            // null.
            CurrentTransferHandle.Release();
            CurrentTransferHandle.Dispose();
            return true;
        }

        protected void CancelCurrentTransfer()
        {
            // Log if the transfer handle could not be canceled.
            if (!TryCancelTransferHandle())
            {
                Debug.LogWarning($"Could not cancel the current transfer.");
                return;
            }

            // If the transfer does not exist in the list of transfers in 
            // progress, then log a warning and stop.
            if (!_transfersInProgress.TryGetValue(CurrentTransferName, out EOSTransferInProgress transfer) || null == transfer)
            {
                Debug.LogWarning($"Could not find a current transfer matching filename \"{CurrentTransferName}\" to cancel.");
                return;
            }

            // Remove the transfer from the collection of current transfers
            _transfersInProgress.Remove(CurrentTransferName);

            // Clear the name of the currently transferring file name
            ClearCurrentTransfer();
        }

        /// <summary>
        /// Called when receiving data.
        /// </summary>
        /// <param name="fileName">
        /// Name of the file for which data is being received.
        /// </param>
        /// <param name="data">
        /// The data being received for the file in question.
        /// </param>
        /// <param name="totalSize">
        /// The total size of the file for which data is currently being
        /// received.
        /// </param>
        /// <returns>
        /// A FileTransferResult indicating the current state of the request.
        /// </returns>
        protected FileTransferResult ReceiveData(string fileName, ArraySegment<byte> data, uint totalSize)
        {
            // Fail request if data received is null.
            if (null == data)
            {
                Debug.LogError("Data received is null");
                return FileTransferResult.FailRequest;
            }

            // Fail request if file that data being received for is not in the
            // list of file transfers.
            if (!_transfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                Debug.LogError($"Receiving data for file " +
                               $"\"{{fileName}},\" but that file is not in " +
                               $"the list of current transfers.");
                return FileTransferResult.CancelRequest;
            }

            // Fail if the file transfer for which data is being received is 
            // marked as an upload instead of a download.
            if (!transfer.Download)
            {
                Debug.LogError($"Receiving data for file, " +
                               $"\"{fileName},\" but that file transfer is " +
                               $"marked as an upload, not a download.");
                return FileTransferResult.FailRequest;
            }

            // If this is the first update (indicated by the CurrentIndex being
            // 0), then allocate the byte array for the file transfer
            if (0 == transfer.CurrentIndex)
            {
                transfer.Data = new byte[totalSize];
            }

            // If the amount of data being recieved exceeds the amount expected
            // and allocated, then fail the request
            if (transfer.TotalSize < transfer.CurrentIndex + data.Count)
            {
                Debug.LogError($"Could not continue to receive data " +
                               $"for file, \"{fileName},\" as more data was " +
                               $"received than there is space allocated for " +
                               $"the transfer.");
                return FileTransferResult.FailRequest;
            }

            // Copy received data into the Data field member of the transfer
            // object.
            data.Array?.CopyTo(transfer.Data, transfer.CurrentIndex);

            // Increment the current index of the data being received.
            transfer.CurrentIndex += (uint)data.Count;

            return FileTransferResult.ContinueReading;
        }
    }
}
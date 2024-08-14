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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Epic.OnlineServices;
    using Epic.OnlineServices.PlayerDataStorage;
    using UnityEngine;

    public abstract class EOSTransferTask
    {
        /// <summary>
        /// Per EOS SDK documentation, the maximum size for a file is 200MB, or this many bytes.
        /// If a file transfer is started that exceeds this value, it will likely fail.
        /// When setting <see cref="TotalSize"/> this value is checked and warned against.
        /// </summary>
        public const int FILE_MAX_SIZE_BYTES = 200000000;

        /// <summary>
        /// This is the most amount of bytes that can be written at once while writing.
        /// </summary>
        public const uint MAX_CHUNK_SIZE = 4096;

        /// <summary>
        /// When an EOSTransferTask is made, it either is an immediate failure or an inner task should be set.
        /// This keeps a reference to the entire task completion source so that it can complete it when done.
        /// </summary>
        public TaskCompletionSource<byte[]> InnerTaskCompletionSource;

        /// <summary>
        /// Delegate signature for <see cref="OnProgressUpdated"/>.
        /// </summary>
        /// <param name="progress">The current percentage completion of the task, by number of bytes to work on.</param>
        public delegate void OnProgressUpdatedDelegate(float progress);

        /// <summary>
        /// An optional task that can be utilized to inform the user of file transfer progress.
        /// </summary>
        public event OnProgressUpdatedDelegate OnProgressUpdated;

        /// <summary>
        /// If this task is a download task, this is where the data will be put when the task is complete.
        /// If this is an upload task, this is where the data should be put.
        /// If for some reason a download fails, this should remain null.
        /// </summary>
        public byte[] Data { get; protected set; } = null;

        /// <summary>
        /// If this task has run to completion, or encountered an error, this ResultCode should be set.
        /// You can determine that an EOSTransferTask is complete if it has any result code.
        /// </summary>
        public Result? ResultCode;

        /// <summary>
        /// Percentage indicator of the work's completeness.
        /// Either read from this value or subscribe to <see cref="OnProgressUpdated"/> to receive updates on progress amounts.
        /// </summary>
        public float Progress;

        /// <summary>
        /// Inner backing for the total size of the transfer.
        /// </summary>
        private uint transferSize = 0;

        /// <summary>
        /// Indexes the current byte count that has been processed.
        /// </summary>
        private uint currentIndex = 0;

        /// <summary>
        /// The total size of the transfer operation in bytes.
        /// </summary>
        public uint TotalSize
        {
            get
            {
                return transferSize;
            }
            private set
            {
                transferSize = value;

                if (transferSize > FILE_MAX_SIZE_BYTES)
                {
                    Debug.LogError("[EOS SDK] Player data storage: data transfer size exceeds max file size.");
                    transferSize = FILE_MAX_SIZE_BYTES;
                }
            }
        }

        /// <summary>
        /// Constructor for download scenarios.
        /// </summary>
        /// <param name="innerTask">A task completion source that can be used to indicate when the task is completed.</param>
        public EOSTransferTask(TaskCompletionSource<byte[]> innerTask)
        {
            InnerTaskCompletionSource = innerTask;
        }

        /// <summary>
        /// Constructor for upload scenarios.
        /// </summary>
        /// <param name="innerTask">A task completion source that can be used to indicate when the task is completed.</param>
        /// <param name="data">The data to upload, in byte[] form.</param>
        public EOSTransferTask(TaskCompletionSource<byte[]> innerTask, byte[] data)
        {
            InnerTaskCompletionSource = innerTask;
            Data = data;
            TotalSize = (uint)data.Length;
        }

        /// <summary>
        /// Constructor for immediate completion scenarios.
        /// </summary>
        /// <param name="resultCode">The result code to set.</param>
        /// <param name="failureException">An optional exception that explains the result.</param>
        public EOSTransferTask(Result resultCode, Exception failureException = null)
        {
            InnerTaskCompletionSource = new TaskCompletionSource<byte[]>();
            ResultCode = resultCode;

            if (failureException != null)
            {
                InnerTaskCompletionSource.SetException(failureException);
            }
            else
            {
                InnerTaskCompletionSource.SetCanceled();
            }
        }

        /// <summary>
        /// Indicates if the operation is complete and ready to read the results out of.
        /// </summary>
        /// <returns>True if the operatoin is complete.</returns>
        public bool IsDone()
        {
            return ResultCode.HasValue || InnerTaskCompletionSource.Task.IsCompleted || (TotalSize > 0 && TotalSize == currentIndex);
        }

        /// <summary>
        /// Internal processor for incoming file work.
        /// This should be assigned to <see cref="ReadFileOptions.ReadFileDataCallback"/>.
        /// </summary>
        /// <param name="data">Data from the EOS SDK about the incoming chunk of data.</param>
        /// <returns>A command to either stop or continue reading the incoming data.</returns>
        internal ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data)
        {
            // (data.Filename, data.DataChunk, data.TotalFileSizeBytes, data.IsLastChunk);
            // (string fileName, ArraySegment<byte> data, uint totalSize, bool isLastChunk)
            if (data.DataChunk == null)
            {
                ResultCode = Result.UnexpectedError;
                InnerTaskCompletionSource.SetException(new Exception($"{nameof(EOSTransferTask)} ({nameof(OnFileDataReceived)}): Player data storage: could not receive data: Data pointer is null."));
                return ReadResult.FailRequest;
            }

            // First update
            if (currentIndex == 0 && TotalSize == 0)
            {
                TotalSize = data.TotalFileSizeBytes;

                if (TotalSize == 0)
                {
                    return ReadResult.ContinueReading;
                }
            }

            // If more data has been received than was anticipated, fail the request
            if (TotalSize < currentIndex + data.DataChunk.Count)
            {
                ResultCode = Result.UnexpectedError;
                InnerTaskCompletionSource.SetException(new Exception($"{nameof(EOSTransferTask)} ({nameof(OnFileDataReceived)}): Could not receive data. Size exceeded expected download size."));
                return ReadResult.FailRequest;
            }

            // Copy the data 
            data.DataChunk.Array?.CopyTo(Data, currentIndex);

            // Advance the index by the amount of data received.
            currentIndex += (uint)(data.DataChunk.Count);

            // Keep reading
            return ReadResult.ContinueReading;
        }

        /// <summary>
        /// Method that dispatches information about the progress of an upload or download.
        /// Should be set in either <see cref="WriteFileOptions.FileTransferProgressCallback"/>
        /// or in <see cref="ReadFileOptions.FileTransferProgressCallback"/>.
        /// </summary>
        /// <param name="data">EOS SDK data about the latest chunk of information.</param>
        internal void OnFileTransferProgressUpdated(ref FileTransferProgressCallbackInfo data)
        {
            if (data.TotalFileSizeBytes > 0)
            {
                Progress = data.BytesTransferred / data.TotalFileSizeBytes;
                PlayerDataStorageService.Log($"{nameof(EOSTransferTask)} ({nameof(OnFileTransferProgressUpdated)}): Player data storage: transfer progress {data.BytesTransferred} / {data.TotalFileSizeBytes}");

                OnProgressUpdated?.Invoke(Progress);
            }
        }

        /// <summary>
        /// Method that is called when a file transfer download has completed.
        /// </summary>
        /// <param name="data">EOS SDK data about the file transfer.</param>
        internal void OnFileReceived(ref ReadFileCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                ResultCode = data.ResultCode;
                InnerTaskCompletionSource.SetException(new Exception($"{nameof(EOSTransferTask)} ({nameof(OnFileReceived)}): Could not receive data. Result code {data.ResultCode}"));
                return;
            }

            ResultCode = Result.Success;
            InnerTaskCompletionSource.SetResult(Data);
        }

        /// <summary>
        /// Internal processor for writing chunks of data during file upload.
        /// </summary>
        /// <param name="data">Metadata about the file upload.</param>
        /// <param name="outDataBuffer">Output buffer for this next chunk of data.</param>
        /// <returns>A command to either continue or finish writing data.</returns>
        internal WriteResult OnFileDataSend(ref WriteFileDataCallbackInfo data, out ArraySegment<byte> outDataBuffer)
        {
            outDataBuffer = new ArraySegment<byte>();

            if (IsDone())
            {
                return WriteResult.CompleteRequest;
            }

            uint bytesToWrite = Math.Min(MAX_CHUNK_SIZE, TotalSize - currentIndex);

            if (bytesToWrite > 0)
            {
                outDataBuffer = new ArraySegment<byte>(Data, (int)currentIndex, (int)bytesToWrite);
            }

            currentIndex += bytesToWrite;

            if (IsDone())
            {
                return WriteResult.CompleteRequest;
            }
            else
            {
                return WriteResult.ContinueWriting;
            }
        }

        /// <summary>
        /// Method for handling when a file upload has completed.
        /// </summary>
        /// <param name="data">EOS SDK data about the file transfer.</param>
        internal void OnFileSent(ref WriteFileCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                ResultCode = data.ResultCode;
                InnerTaskCompletionSource.SetException(new Exception($"{nameof(EOSTransferTask)} ({nameof(OnFileSent)}): Could not send data. Result code {data.ResultCode}"));
                return;
            }

            ResultCode = Result.Success;
            InnerTaskCompletionSource.SetResult(Data);
        }

        /// <summary>
        /// Sets the size of the buffer that will receive a download.
        /// The appropriate value can be retrieved by using <see cref="PlayerDataStorageInterface.CopyFileMetadataByFilename(ref CopyFileMetadataByFilenameOptions, out FileMetadata?)"/>,
        /// which will return the total size of the file to download.
        /// </summary>
        /// <param name="size">The size of the buffer to set.</param>
        internal void SetDataBufferSize(uint size)
        {
            Data = new byte[size];
            TotalSize = size;
        }

        /// <summary>
        /// Sets the result of this transfer task.
        /// This will set the <see cref="InnerTaskCompletionSource"/>'s result, which should resolve a task.
        /// </summary>
        /// <param name="result">Optional result code to set.</param>
        /// <param name="innerTaskException">
        /// Optional exception to set.
        /// This is bubbled up to the <see cref="InnerTaskCompletionSource"/>.
        /// If not provided, the result will be set as null, and the task will have considered to be successful as far as the Task itself is concerned.
        /// Could still be handled as an error if <paramref name="result"/> is passed in as not null.
        /// </param>
        internal void SetResult(Result? result = null, Exception innerTaskException = null)
        {
            ResultCode = result;

            if (innerTaskException != null)
            {
                InnerTaskCompletionSource.SetException(innerTaskException);
            }
            else
            {
                InnerTaskCompletionSource.SetResult(null);
            }
        }
    }
}
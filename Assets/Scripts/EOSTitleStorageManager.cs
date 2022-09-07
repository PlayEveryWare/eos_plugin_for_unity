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

﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.TitleStorage;
using System.Text;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>Class <c>EOSTitleStorageManager</c> is a simplified wrapper for EOS [TitleStorage Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/TitleStorage/index.html).</summary>
    public class EOSTitleStorageManager : IEOSSubManager
    {
        public const uint MAX_CHUNK_SIZE = 4 * 4 * 4096;

        private string CurrentTransferName;
        private TitleStorageFileTransferRequest CurrentTransferHandle = null;
        private float CurrentTransferProgress;  // TODO: use for progress UI
        private Dictionary<string, string> StorageData = new Dictionary<string, string>();
        private Dictionary<string, EOSTransferInProgress> TransfersInProgress = new Dictionary<string, EOSTransferInProgress>();

        private List<string> CurrentFileNames = new List<string>();

        // Manager Callbacks
        public OnQueryFileListCallback QueryListCallback { get; private set; }
        public OnReadFileCallback ReadFileCallback { get; private set; }

        public delegate void OnQueryFileListCallback(Result result);
        public delegate void OnReadFileCallback(Result result);

        public EOSTitleStorageManager()
        {
            TransfersInProgress = new Dictionary<string, EOSTransferInProgress>();
            StorageData = new Dictionary<string, string>();

            QueryListCallback = null;
            ReadFileCallback = null;
        }

        public Dictionary<string, string> GetCachedStorageData()
        {
            return StorageData;
        }

        public List<string> GetCachedCurrentFileNames()
        {
            return CurrentFileNames;
        }

        /// <summary>User Logged In actions</summary>
        /// <list type="bullet">
        ///     <item><description><c>NA</c></description></item>
        /// </list>
        public void OnLoggedIn()
        {
            
        }

        /// <summary>User Logged Out actions</summary>
        /// <list type="bullet">
        ///     <item><description>Clear StorageData Cache and Current Transfer</description></item>
        /// </list>
        public void OnLoggedOut()
        {
            StorageData.Clear();
            ClearCurrentTransfer();
        }

        /// <summary>(async) Query list of files.</summary>
        public void QueryFileList(string[] tags, OnQueryFileListCallback QueryFileListCompleted)
        {
            Utf8String[] utf8StringTags = new Utf8String[tags.Length];


            for (int i = 0; i < tags.Length; ++i)
            {
                utf8StringTags[i] = tags[i];
            }
            QueryFileList(utf8StringTags, QueryFileListCompleted);
        }

        public void QueryFileList(Utf8String[] tags, OnQueryFileListCallback QueryFileListCompleted)
        {
            QueryFileListOptions queryOptions = new QueryFileListOptions();
            queryOptions.ListOfTags = tags;
            queryOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            QueryListCallback = QueryFileListCompleted;

            TitleStorageInterface titleStorageHandle = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            titleStorageHandle.QueryFileList(ref queryOptions, null, OnQueryFileListCompleted);
        }

        private void OnQueryFileListCompleted(ref QueryFileListCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Title storage: OnFileListRetrieved data == null!");
            //    QueryListCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Title storage: file list retrieval error: {0}", data.ResultCode);
                QueryListCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Title storage file list is successfully retrieved.");

            uint fileCount = data.FileCount;
            TitleStorageInterface titleStorageHandle = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
            CurrentFileNames.Clear();

            for (uint fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                CopyFileMetadataAtIndexOptions copyFileOptions = new CopyFileMetadataAtIndexOptions();
                copyFileOptions.Index = fileIndex;
                copyFileOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

                titleStorageHandle.CopyFileMetadataAtIndex(ref copyFileOptions, out FileMetadata? fileMetadata);

                if (fileMetadata != null)
                {
                    if (!string.IsNullOrEmpty(fileMetadata?.Filename))
                    {
                        CurrentFileNames.Add(fileMetadata?.Filename);
                    }
                }
            }

            QueryListCallback?.Invoke(Result.Success);
        }

        public void ReadFile(string fileName, OnReadFileCallback ReadFileCompleted)
        {
            // StartFileDataDownload

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return;
            }

            var queryFileOptions = new QueryFileOptions { Filename = fileName, LocalUserId = localUserId };
            EOSManager.Instance.GetEOSTitleStorageInterface().QueryFile(ref queryFileOptions, null, (ref QueryFileCallbackInfo data) => 
            {
                if(data.ResultCode == Result.Success)
                {
                    var copyFileMetadataByFilenameOptions = new CopyFileMetadataByFilenameOptions { Filename = fileName, LocalUserId = localUserId };
                    EOSManager.Instance.GetEOSTitleStorageInterface().CopyFileMetadataByFilename(ref copyFileMetadataByFilenameOptions, out FileMetadata? fileMetadata);


                    ReadFileOptions fileReadOptions = new ReadFileOptions();
                    fileReadOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
                    fileReadOptions.Filename = fileName;
                    fileReadOptions.ReadChunkLengthBytes = MAX_CHUNK_SIZE;

                    fileReadOptions.ReadFileDataCallback = OnFileDataReceived;
                    fileReadOptions.FileTransferProgressCallback = OnFileTransferProgressUpdated;

                    // ReadFile Callback
                    ReadFileCallback = ReadFileCompleted;

                    TitleStorageInterface titleStorageHandle = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
                    TitleStorageFileTransferRequest transferReq = titleStorageHandle.ReadFile(ref fileReadOptions, null, OnFileReceived);

                    CancelCurrentTransfer();
                    CurrentTransferHandle = transferReq;

                    EOSTransferInProgress newTransfer = new EOSTransferInProgress();
                    newTransfer.Download = true;
                    newTransfer.Data = new byte[(uint)fileMetadata?.FileSizeBytes];


                    TransfersInProgress.Add(fileName, newTransfer);

                    CurrentTransferProgress = 0.0f;
                    CurrentTransferName = fileName;
                }
            });
        }

        private void OnFileReceived(ref ReadFileCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Title storage: OnReadFileComplete data == null!");
            //    ReadFileCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Title storage: OnFileReceived error: {0}", data.ResultCode);
                FinishFileDownload(data.Filename, false, data.ResultCode);
                return;
            }

            FinishFileDownload(data.Filename, true, data.ResultCode);
        }

        public void FinishFileDownload(string fileName, bool success, Result result)
        {
            Debug.LogFormat("Title storage: FinishFileDownload '{0}', success = {1}", fileName, success);

            if (!TransfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                Debug.LogErrorFormat("[EOS SDK] Title storage: '{0}' was not found in TransfersInProgress.", fileName);
                ReadFileCallback?.Invoke(result);
                return;
            }

            if (!transfer.Download)
            {
                Debug.LogError("[EOS SDK] Title storage: error while file read operation: can't finish because of download/upload mismatch.");
                ReadFileCallback?.Invoke(result);
                return;
            }

            if (!transfer.Done() || success)
            {
                if (!transfer.Done())
                {
                    Debug.LogError("[EOS SDK] Title storage: error while file read operation: expecting more data. File can be corrupted.");
                }

                TransfersInProgress.Remove(fileName);
                if (fileName == CurrentTransferName)
                {
                    ClearCurrentTransfer();
                }
            }

            string fileData = string.Empty;
            if (transfer.TotalSize > 0)
            {
                fileData = System.Text.Encoding.UTF8.GetString(transfer.Data);
            }

            StorageData.Add(fileName, fileData);

            Debug.LogFormat("[EOS SDK] Title storage: file read finished: '{0}' Size: {1}.", fileName, fileData.Length);

            TransfersInProgress.Remove(fileName);

            if (fileName.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
            {
                ClearCurrentTransfer();
            }

            ReadFileCallback?.Invoke(result);
        }

        private void CancelCurrentTransfer()
        {
            if (CurrentTransferHandle != null)
            {
                Result cancelResult = CurrentTransferHandle.CancelRequest();

                if (cancelResult == Result.Success)
                {
                    TransfersInProgress.TryGetValue(CurrentTransferName, out EOSTransferInProgress transfer);

                    if (transfer != null)
                    {
                        if (transfer.Download)
                        {
                            Debug.Log("Title storage: CancelCurrentTransfer - Download is canceled");
                        }
                        else
                        {
                            Debug.Log("Title storage: CancelCurrentTransfer - Upload is canceled");
                        }

                        TransfersInProgress.Remove(CurrentTransferName);
                    }

                    // TODO: Hide Progress UI
                }
            }

            ClearCurrentTransfer();
        }

        private void ClearCurrentTransfer()
        {
            CurrentTransferName = string.Empty;
            CurrentTransferProgress = 0.0f;

            if (CurrentTransferHandle != null)
            {
                CurrentTransferHandle = null;
            }
        }

        private void OnFileTransferProgressUpdated(ref FileTransferProgressCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Title storage: OnFileTransferProgressUpdated data == null!");
            //    return;
            //}

            if (data.TotalFileSizeBytes > 0)
            {
                UpdateProgress(data.Filename, data.BytesTransferred / data.TotalFileSizeBytes);
                Debug.LogFormat("Title storage: transfer progress {0} / {1}", data.BytesTransferred, data.TotalFileSizeBytes);
            }
        }

        private void UpdateProgress(string filename, uint progress)
        {
            if (filename.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
            {
                CurrentTransferProgress = progress;
            }
        }

        private ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data)
        {

            return ReceiveData(data.Filename, data.DataChunk, data.TotalFileSizeBytes);

            //return ReadResult.RrFailrequest;
        }

        private ReadResult ReceiveData(string fileName, ArraySegment<byte> data, uint totalSize)
        {
            if (data == null)
            {
                Debug.LogError("[EOS SDK] Title storage: could not receive data: Data pointer is null.");
                return ReadResult.RrFailrequest;
            }

            TransfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer);

            if (transfer != null)
            {
                if (!transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Title storage: can't load file data: download/upload mismatch.");
                    return ReadResult.RrFailrequest;
                }

                // First update
                if (transfer.CurrentIndex == 0 && transfer.TotalSize == 0)
                {
                    transfer.TotalSize = totalSize;

                    if (transfer.TotalSize == 0)
                    {
                        return ReadResult.RrContinuereading;
                    }
                }

                // Make sure we have enough space
                if (transfer.TotalSize - transfer.CurrentIndex >= data.Count)
                {
                    data.Array.CopyTo(transfer.Data, transfer.CurrentIndex);
                    transfer.CurrentIndex += (uint)data.Count;

                    return ReadResult.RrContinuereading;
                }
                else
                {
                    Debug.LogError("[EOS SDK] Title storage: could not receive data: too much of it.");
                    return ReadResult.RrFailrequest;
                }
            }

            return ReadResult.RrCancelrequest;
        }

    }
}
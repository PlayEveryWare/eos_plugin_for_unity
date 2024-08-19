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
    using System.Threading.Tasks;

    /// <summary>Class <c>EOSTitleStorageManager</c> is a simplified wrapper for EOS [TitleStorage Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/TitleStorage/index.html).</summary>
    public class EOSTitleStorageManager : DataService<TitleStorageFileTransferRequestWrapper>
    {   
        private List<string> CurrentFileNames = new List<string>();

        // Manager Callbacks
        public OnQueryFileListCallback QueryListCallback { get; private set; } = null;
        public OnReadFileCallback ReadFileCallback { get; private set; } = null;

        public delegate void OnQueryFileListCallback(Result result);
        public delegate void OnReadFileCallback(Result result);

        public List<string> GetCachedCurrentFileNames()
        {
            return CurrentFileNames;
        }

        /// <summary>User Logged In actions</summary>
        /// <list type="bullet">
        ///     <item><description><c>NA</c></description></item>
        /// </list>
        protected override void OnLoggedIn()
        {
        }

        protected override Task InternalRefreshAsync()
        {
            // TODO: Needs implementation
            return Task.CompletedTask;
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
                    fileReadOptions.ReadChunkLengthBytes = MaxChunkSize;

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


                    _transfersInProgress.Add(fileName, newTransfer);

                    CurrentTransferName = fileName;
                }
            });
        }

        private void OnFileReceived(ref ReadFileCallbackInfo data)
        {
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

            if (!_transfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
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

                _transfersInProgress.Remove(fileName);
                if (fileName == CurrentTransferName)
                {
                    ClearCurrentTransfer();
                }
            }

            string fileData = string.Empty;
            if (transfer.TotalSize > 0)
            {
                fileData = System.Text.Encoding.UTF8.GetString(transfer.Data, 0, (int)transfer.TotalSize);
            }

            _locallyCachedData.Add(fileName, fileData);

            Debug.LogFormat("[EOS SDK] Title storage: file read finished: '{0}' Size: {1}.", fileName, fileData.Length);

            _transfersInProgress.Remove(fileName);

            if (fileName.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
            {
                ClearCurrentTransfer();
            }

            ReadFileCallback?.Invoke(result);
        }

        private void OnFileTransferProgressUpdated(ref FileTransferProgressCallbackInfo data)
        {
            if (data.TotalFileSizeBytes > 0)
            {
                Debug.LogFormat("Title storage: transfer progress {0} / {1}", data.BytesTransferred, data.TotalFileSizeBytes);
            }
        }

        private ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data)
        {
            return ReceiveData(data.Filename, data.DataChunk, data.TotalFileSizeBytes) switch
            {
                FileTransferResult.FailRequest => ReadResult.RrFailrequest,
                FileTransferResult.CancelRequest => ReadResult.RrCancelrequest,
                FileTransferResult.ContinueReading => ReadResult.RrContinuereading,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
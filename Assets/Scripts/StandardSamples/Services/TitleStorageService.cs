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
    using Events;
    using System.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Epic.OnlineServices;
    using Epic.OnlineServices.TitleStorage;

    /// <summary>Class <c>TitleStorageService</c> is a simplified wrapper for EOS [TitleStorage Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/TitleStorage/index.html).</summary>
    public class TitleStorageService : StorageService<TitleStorageFileTransferRequestWrapper>
    {   
        private List<string> CurrentFileNames = new List<string>();

        #region Singleton Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the TitleStorageService
        /// </summary>
        private static readonly Lazy<TitleStorageService> s_LazyInstance = new(() => new TitleStorageService());

        /// <summary>
        /// Accessor for the instance.
        /// </summary>
        public static TitleStorageService Instance
        {
            get
            {
                return s_LazyInstance.Value;
            }
        }

        /// <summary>
        /// Private constructor guarantees adherence to thread-safe singleton
        /// pattern.
        /// </summary>
        private TitleStorageService() { }

        #endregion

        // Manager Callbacks
        public EOSResultEventHandler QueryListCallback { get; private set; } = null;
        
        public List<string> GetCachedCurrentFileNames()
        {
            return CurrentFileNames;
        }

        protected override Task InternalRefreshAsync()
        {
            // TODO: Needs implementation
            return Task.CompletedTask;
        }

        public void QueryFileList(string[] tags, EOSResultEventHandler QueryFileListCompleted)
        {
            Utf8String[] utf8StringTags = null;
            if (tags != null && tags.Length > 0)
            {
                utf8StringTags = new Utf8String[tags.Length];

                for (int i = 0; i < tags.Length; ++i)
                {
                    utf8StringTags[i] = tags[i];
                }
            }

            QueryFileListOptions queryOptions = new()
            {
                ListOfTags = utf8StringTags, 
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

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

        public void DownloadFile(string fileName, EOSResultEventHandler ReadFileCompleted)
        {
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
                    OnFileDownloaded += ReadFileCompleted;

                    TitleStorageInterface titleStorageHandle = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
                    TitleStorageFileTransferRequest transferReq = titleStorageHandle.ReadFile(ref fileReadOptions, null, OnFileReceived);

                    CancelCurrentTransfer();
                    CurrentTransferHandle = transferReq;

                    EOSTransferInProgress newTransfer = new EOSTransferInProgress();
                    newTransfer.Download = true;
                    newTransfer.Data = new byte[(uint)fileMetadata?.FileSizeBytes];


                    _transfersInProgress[fileName] = newTransfer;

                    CurrentTransferName = fileName;
                }
            });
        }

        private void OnFileReceived(ref ReadFileCallbackInfo data)
        {
            FinishFileDownload(data.Filename, data.ResultCode);
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
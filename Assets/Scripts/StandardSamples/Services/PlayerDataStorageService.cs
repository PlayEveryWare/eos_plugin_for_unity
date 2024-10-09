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
    using System.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Epic.OnlineServices;
    using Epic.OnlineServices.PlayerDataStorage;

    /// <summary>Class <c>PlayerDataStorageService</c> is a simplified wrapper for EOS [PlayerDataStorage Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/PlayerDataStorage/index.html).</summary>
    public class PlayerDataStorageService

        : StorageService<PlayerDataStorageFileTransferRequestWrapper>
    {   
        public event Action OnFileListUpdated;

        #region Singleton Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the TitleStorageService
        /// </summary>
        private static readonly Lazy<PlayerDataStorageService> s_LazyInstance = new(() => new PlayerDataStorageService());

        /// <summary>
        /// Accessor for the instance.
        /// </summary>
        public static PlayerDataStorageService Instance
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
        private PlayerDataStorageService() { }

        #endregion


        //-------------------------------------------------------------------------
        /// <summary>(async) Query list of files.</summary>
        public void QueryFileList()
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return;
            }

            QueryFileListOptions options = new QueryFileListOptions
            {
                LocalUserId = localUserId
            };

            EOSManager.Instance.GetPlayerDataStorageInterface().QueryFileList(ref options, null, OnQueryFileListCompleted);
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file data download.</summary>
        /// <param name="fileName">Name of file.</param>
        /// <param name="downloadCompletedCallback">Function called when download is completed.</param>
        public void DownloadFile(string fileName, Action downloadCompletedCallback = null)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return;
            }

            ReadFileOptions options = new ReadFileOptions
            {
                LocalUserId = localUserId,
                Filename = fileName,
                ReadChunkLengthBytes = MAX_CHUNK_SIZE,
                ReadFileDataCallback = OnFileDataReceived,
                FileTransferProgressCallback = OnFileTransferProgressUpdated
            };

            var queryFileOptions = new QueryFileOptions { Filename = fileName, LocalUserId = localUserId };
            EOSManager.Instance.GetPlayerDataStorageInterface().QueryFile(ref queryFileOptions, null, (ref QueryFileCallbackInfo data) =>
            {
                if(data.ResultCode == Result.Success)
                {

                    var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions { Filename = fileName, LocalUserId = localUserId };
                    EOSManager.Instance.GetPlayerDataStorageInterface().CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

                    PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetPlayerDataStorageInterface().ReadFile(ref options, downloadCompletedCallback, OnFileReceived);
                    if (req == null)
                    {
                        Debug.LogErrorFormat("[EOS SDK] Player data storage: can't start file download, bad handle returned for filename '{0}'", fileName);
                        return;
                    }

                    CancelCurrentTransfer();
                    CurrentTransferHandle = req;

                    EOSTransferInProgress newTransfer = new EOSTransferInProgress()
                    {
                        Download = true,
                        Data = new byte[(uint)fileMetadata?.FileSizeBytes],
                    };

                    _transfersInProgress[fileName] = newTransfer;

                    CurrentTransferName = fileName;
                }
                else
                {
                    Debug.LogErrorFormat("[EOS SDK] Player data storage: can't start file download, unable to query file info {0}", fileName);
                }
            });
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file data upload.</summary>
        /// <param name="fileName">Name of file.</param>
        /// <returns>True if upload started</returns>
        public bool StartFileDataUpload(string fileName, Action fileCreatedCallback = null)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return false;
            }

            string fileData = null;
            if (_locallyCachedData.TryGetValue(fileName, out string entry))
            {
                fileData = entry;
            }

            WriteFileOptions options = new WriteFileOptions()
            {
                LocalUserId = localUserId,
                Filename = fileName,
                ChunkLengthBytes = MAX_CHUNK_SIZE,
                WriteFileDataCallback = OnFileDataSend,
                FileTransferProgressCallback = OnFileTransferProgressUpdated
            };

            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetPlayerDataStorageInterface().WriteFile(ref options, fileCreatedCallback, OnFileSent);
            if (req == null)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: can't start file download, bad handle returned for filename '{0}'", fileName);
                return false;
            }

            CancelCurrentTransfer();
            CurrentTransferHandle = req;

            EOSTransferInProgress newTransfer = new()
            {
                Download = false
            };

            if (null != fileData)
            {
                newTransfer.Data = System.Text.Encoding.UTF8.GetBytes(fileData);
            }
            
            newTransfer.CurrentIndex = 0;

            _transfersInProgress[fileName] = newTransfer;
            
            CurrentTransferName = fileName;

            return true;
        }

        //-------------------------------------------------------------------------
        private void SetFileList(List<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (!_locallyCachedData.TryGetValue(fileName, out string fileData))
                {
                    // New file, no cache data
                    _locallyCachedData.Add(fileName, null);
                }
            }

            // Remove files no longer in cloud
            List<string> toRemove = new List<string>();

            foreach (string localFileName in _locallyCachedData.Keys)
            {
                if (!fileNames.Contains(localFileName))
                {
                    toRemove.Add(localFileName);
                }
            }

            foreach (string removerFile in toRemove)
            {
                _locallyCachedData.Remove(removerFile);
            }
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file data upload.</summary>
        /// <param name="fileName">Name of file.</param>
        /// <param name="fileContent">File content.</param>
        /// <param name="fileCreatedCallback">Function called when file creation and upload is completed.</param>
        public void AddFile(string fileName, string fileContent, Action fileCreatedCallback = null)
        {
            _locallyCachedData[fileName] = fileContent;

            if (!StartFileDataUpload(fileName, fileCreatedCallback))
            {
                EraseLocalData(fileName);
            }
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file data copy.</summary>
        /// <param name="sourceFileName">Name of source file in EOS backend.</param>
        /// <param name="destinationFileName">Name of target file to copy to.</param>
        public void CopyFile(string sourceFileName, string destinationFileName)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return;
            }

            DuplicateFileOptions options = new DuplicateFileOptions()
            {
                LocalUserId = localUserId,
                SourceFilename = sourceFileName,
                DestinationFilename = destinationFileName
            };

            EOSManager.Instance.GetPlayerDataStorageInterface().DuplicateFile(ref options, null, OnFileCopied);
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file delete.</summary>
        /// <param name="fileName">Name of source file to delete.</param>
        public void DeleteFile(string fileName)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return;
            }

            EraseLocalData(fileName);

            DeleteFileOptions options = new DeleteFileOptions()
            {
                LocalUserId = localUserId,
                Filename = fileName
            };

            EOSManager.Instance.GetPlayerDataStorageInterface().DeleteFile(ref options, null, OnFileRemoved);
        }

        //-------------------------------------------------------------------------
        /// <summary>Get cached file content for specified file name.</summary>
        /// <param name="fileName">Name of file.</param>
        /// <returns>File content</returns>
        public string GetCachedFileContent(string fileName)
        {
            _locallyCachedData.TryGetValue(fileName, out string data);

            return data;
        }

        private bool EraseLocalData(string entryName)
        {
            return _locallyCachedData.Remove(entryName);
        }

        //-------------------------------------------------------------------------
        private WriteResult SendData(string fileName, out ArraySegment<byte> data)
        {
            data = new ArraySegment<byte>();

            if (_transfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                if (transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Player data storage: can't send file data: download/upload mismatch.");
                    return WriteResult.FailRequest;
                }

                if (transfer.IsDone())
                {
                    return WriteResult.CompleteRequest;
                }

                uint bytesToWrite = Math.Min(MAX_CHUNK_SIZE, transfer.TotalSize - transfer.CurrentIndex);

                if (bytesToWrite > 0)
                {
                    data = new ArraySegment<byte>(transfer.Data, (int)transfer.CurrentIndex, (int)bytesToWrite);
                }

                transfer.CurrentIndex += bytesToWrite;

                if (transfer.IsDone())
                {
                    return WriteResult.CompleteRequest;
                }
                else
                {
                    return WriteResult.ContinueWriting;
                }
            }
            else
            {
                Debug.LogError("[EOS SDK] Player data storage: could not send data as this file is not being uploaded at the moment.");
                return WriteResult.CancelRequest;
            }
        }
        
        private void FinishFileUpload(string fileName, Action fileUploadCallback = null)
        {
            if (_transfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                if (transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Player data storage: error while file write operation: can't finish because of download/upload mismatch.");
                    return;
                }

                if (!transfer.IsDone())
                {
                    Debug.LogError("[EOS SDK] Player data storage: error while file write operation: unexpected end of transfer.");
                }

                _transfersInProgress.Remove(fileName);

                if (fileName.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
                {
                    ClearCurrentTransfer();
                }

                fileUploadCallback?.Invoke();
            }
        }

        /// <summary>User Logged In actions</summary>
        /// <list type="bullet">
        ///     <item><description><c>QueryFileList()</c></description></item>
        /// </list>
        protected override void OnLoggedIn(AuthenticationListener.LoginChangeKind changeType)
        {
            if (changeType == AuthenticationListener.LoginChangeKind.Connect)
            {
                QueryFileList();
            }
        }

        protected override Task InternalRefreshAsync()
        {
            // TODO: Needs implementation
            return Task.CompletedTask;
        }

        //-------------------------------------------------------------------------
        private void OnQueryFileListCompleted(ref QueryFileListCallbackInfo data)
        {
            if (data.ResultCode != Result.Success && data.ResultCode != Result.NotFound)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: file list retrieval error: {0}", data.ResultCode);
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                return;
            }

            Debug.Log("[EOS SDK] Player data storage file list is successfully retrieved.");

            uint fileCount = data.FileCount;

            PlayerDataStorageInterface playerStorageHandle = EOSManager.Instance.GetPlayerDataStorageInterface();
            List<string> fileNames = new List<string>();

            for (uint fileIndex = 0; fileIndex < data.FileCount; fileIndex++)
            {
                CopyFileMetadataAtIndexOptions options = new CopyFileMetadataAtIndexOptions()
                {
                    LocalUserId = localUserId,
                    Index = fileIndex
                };

                Result result = playerStorageHandle.CopyFileMetadataAtIndex(ref options, out FileMetadata? fileMetadata);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Player Data Storage (OnFileListRetrieved): CopyFileMetadataAtIndex returned error result = {0}", result);
                    return;
                }
                else if (fileMetadata != null)
                {
                    if (!string.IsNullOrEmpty(fileMetadata?.Filename))
                    {
                        fileNames.Add(fileMetadata?.Filename);
                    }
                }
            }

            SetFileList(fileNames);
            OnFileListUpdated?.Invoke();
        }

        //-------------------------------------------------------------------------
        private ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data)
        {
            return ReceiveData(data.Filename, data.DataChunk, data.TotalFileSizeBytes) switch
            {
                FileTransferResult.FailRequest => ReadResult.FailRequest,
                FileTransferResult.CancelRequest => ReadResult.CancelRequest,
                FileTransferResult.ContinueReading => ReadResult.ContinueReading,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        //-------------------------------------------------------------------------
        private void OnFileReceived(ref ReadFileCallbackInfo data)
        {
            FinishFileDownload(data.Filename, data.ResultCode);

            var callback = data.ClientData as Action;
            callback?.Invoke();
        }

        private WriteResult OnFileDataSend(ref WriteFileDataCallbackInfo data, out ArraySegment<byte> outDataBuffer)
        {
            WriteResult result = SendData(data.Filename, out ArraySegment<byte> dataBuffer);

            if (dataBuffer != null)
            {
                outDataBuffer = dataBuffer;
            }
            else
            {
                outDataBuffer = new ArraySegment<byte>();
            }

            return result;
        }

        private void OnFileSent(ref WriteFileCallbackInfo data)
        {
            var callback = data.ClientData as Action;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: could not upload file: {0}", data.ResultCode);
                FinishFileUpload(data.Filename, callback);
                return;
            }

            FinishFileUpload(data.Filename, callback);
        }

        private void OnFileTransferProgressUpdated(ref FileTransferProgressCallbackInfo data)
        {
            if (data.TotalFileSizeBytes > 0)
            {
                Debug.LogFormat("[EOS SDK] Player data storage: transfer progress {0} / {1}", data.BytesTransferred, data.TotalFileSizeBytes);
            }
        }

        private void OnFileCopied(ref DuplicateFileCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: error while copying the file: {0}", data.ResultCode);
                return;
            }

            QueryFileList();
        }

        private void OnFileRemoved(ref DeleteFileCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: error while removing file: {0}", data.ResultCode);
                return;
            }

            QueryFileList();
        }
    }
}
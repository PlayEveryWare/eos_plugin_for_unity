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
using Epic.OnlineServices.PlayerDataStorage;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>Class <c>EOSPlayerDataStorageManager</c> is a simplified wrapper for EOS [PlayerDataStorage Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/PlayerDataStorage/index.html).</summary>
    public class EOSPlayerDataStorageManager : IEOSSubManager
    {
        private const uint MAX_CHUNK_SIZE = 4096;
        private PlayerDataStorageFileTransferRequest CurrentTransferHandle;
        private Dictionary<string, EOSTransferInProgress> TransfersInProgress;
        private float CurrentTransferProgress;
        private string CurrentTransferName;

        private Dictionary<string, string> StorageData;

        public List<Action> FileListUpdateCallbacks;

        public EOSPlayerDataStorageManager()
        {
            TransfersInProgress = new Dictionary<string, EOSTransferInProgress>();
            StorageData = new Dictionary<string, string>();
            FileListUpdateCallbacks = new List<Action>();
        }

        //-------------------------------------------------------------------------
        /// <summary>Returns cached Storage Data list.</summary>
        /// <returns>Dictionary(string, string) cached StorageData where FileName is key and FileContent is value.</returns>
        public Dictionary<string, string> GetCachedStorageData()
        {
            return StorageData;
        }

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

            EOSManager.Instance.GetPlayerDataStorageInterface().QueryFileList(ref options, null, OnFileListRetrieved);
        }

        //-------------------------------------------------------------------------
        /// <summary>Add listener callback that will be called when the queried file list is updated.</summary>
        /// <param name="downloadCompletedCallback">Function called when file list is updated.</param>
        public void AddNotifyFileListUpdated(Action fileListUpdatedCallback)
        {
            FileListUpdateCallbacks.Add(fileListUpdatedCallback);
        }

        public void RemoveNotifyFileListUpdated(Action fileListUpdatedCallback)
        {
            FileListUpdateCallbacks.Remove(fileListUpdatedCallback);
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file data download.</summary>
        /// <param name="fileName">Name of file.</param>
        /// <param name="downloadCompletedCallback">Function called when download is completed.</param>
        public void StartFileDataDownload(string fileName, Action downloadCompletedCallback = null)
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

                    TransfersInProgress[fileName] = newTransfer;

                    CurrentTransferProgress = 0.0f;
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
            if (StorageData.TryGetValue(fileName, out string entry))
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

            EOSTransferInProgress newTransfer = new EOSTransferInProgress();
            newTransfer.Download = false;

            newTransfer.TotalSize = (uint)fileData.Length;
            if (newTransfer.TotalSize > 0)
            {
                byte[] utf8ByteArray = System.Text.Encoding.UTF8.GetBytes(fileData);

                newTransfer.Data = utf8ByteArray;
            }
            newTransfer.CurrentIndex = 0;

            TransfersInProgress[fileName] = newTransfer;

            CurrentTransferProgress = 0.0f;
            CurrentTransferName = fileName;

            return true;
        }

        //-------------------------------------------------------------------------
        /// <summary>Get cached file list.</summary>
        /// <returns>List<string> fileList</string></returns>
        public List<string> GetCachedFileList()
        {
            List<string> fileList = null;

            if (StorageData.Count > 0)
            {
                fileList = new List<string>(StorageData.Keys);
            }

            return fileList;
        }

        //-------------------------------------------------------------------------
        private void SetFileList(List<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (!StorageData.TryGetValue(fileName, out string fileData))
                {
                    // New file, no cache data
                    StorageData.Add(fileName, null);
                }
            }

            // Remove files no longer in cloud
            List<string> toRemove = new List<string>();

            foreach (string localFileName in StorageData.Keys)
            {
                if (!fileNames.Contains(localFileName))
                {
                    toRemove.Add(localFileName);
                }
            }

            foreach (string removerFile in toRemove)
            {
                StorageData.Remove(removerFile);
            }
        }

        //-------------------------------------------------------------------------
        /// <summary>(async) Begin file data upload.</summary>
        /// <param name="fileName">Name of file.</param>
        /// <param name="fileContent">File content.</param>
        /// <param name="fileCreatedCallback">Function called when file creation and upload is completed.</param>
        public void AddFile(string fileName, string fileContent, Action fileCreatedCallback = null)
        {
            SetLocalData(fileName, fileContent);

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
            StorageData.TryGetValue(fileName, out string data);

            return data;
        }

        private void SetLocalData(string fileName, string data)
        {
            if (StorageData.ContainsKey(fileName))
            {
                StorageData[fileName] = data;
            }
            else
            {
                StorageData.Add(fileName, data);
            }
        }

        private bool EraseLocalData(string entryName)
        {
            return StorageData.Remove(entryName);
        }

        private void CancelCurrentTransfer()
        {
            if (CurrentTransferHandle != null)
            {
                Result result = CurrentTransferHandle.CancelRequest();
                CurrentTransferHandle.Release();
                CurrentTransferHandle = null;

                if (result == Result.Success)
                {
                    if (TransfersInProgress.TryGetValue(CurrentTransferName, out EOSTransferInProgress transfer))
                    {
                        TransfersInProgress.Remove(CurrentTransferName);
                    }
                }
            }

            ClearCurrentTransfer();
        }

        private ReadResult ReceiveData(string fileName, ArraySegment<byte> data, uint totalSize, bool isLastChunk)
        {
            if (data == null)
            {
                Debug.LogError("[EOS SDK] Player data storage: could not receive data: Data pointer is null.");
                return ReadResult.FailRequest;
            }

            if (TransfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                if (!transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Player data storage: can't load file data: download/upload mismatch.");
                    return ReadResult.FailRequest;
                }

                // First update
                if (transfer.CurrentIndex == 0 && transfer.TotalSize == 0)
                {
                    transfer.TotalSize = totalSize;

                    if (transfer.TotalSize == 0)
                    {
                        return ReadResult.ContinueReading;
                    }
                }

                if (isLastChunk)//transfer.TotalSize - transfer.CurrentIndex >= numBytes)
                {
                    data.Array.CopyTo(transfer.Data, transfer.CurrentIndex);

                    transfer.CurrentIndex = transfer.TotalSize; // Done

                    return ReadResult.ContinueReading;
                }
                else
                {
                    Debug.LogError("[EOS SDK] Player data storage: could not receive data: too much of it.");
                    return ReadResult.FailRequest;
                }
            }

            return ReadResult.CancelRequest;
        }

        //-------------------------------------------------------------------------
        private WriteResult SendData(string fileName, out ArraySegment<byte> data)
        {
            data = new ArraySegment<byte>();

            if (TransfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                if (transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Player data storage: can't send file data: download/upload mismatch.");
                    return WriteResult.FailRequest;
                }

                if (transfer.Done())
                {
                    return WriteResult.CompleteRequest;
                }

                uint bytesToWrite = Math.Min(MAX_CHUNK_SIZE, transfer.TotalSize - transfer.CurrentIndex);

                if (bytesToWrite > 0)
                {
                    data = new ArraySegment<byte>(transfer.Data, (int)transfer.CurrentIndex, (int)bytesToWrite);
                }

                transfer.CurrentIndex += bytesToWrite;

                if (transfer.Done())
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

        private void UpdateProgress(string fileName, float progress)
        {
            if (fileName.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
            {
                CurrentTransferProgress = progress;
            }
        }

        private void FinishFileDownload(string fileName, bool success)
        {
            if (TransfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                if (!transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Player data storage: error while file read operation: can't finish because of download/upload mismatch.");
                    return;
                }

                if (!transfer.Done() || !success)
                {
                    if (!transfer.Done())
                    {
                        Debug.LogError("[EOS SDK] Player data storage: error while file read operation: expecting more data. File can be corrupted.");
                    }
                    TransfersInProgress.Remove(fileName);
                    if (fileName.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
                    {
                        ClearCurrentTransfer();
                    }
                    return;
                }

                // Files larger than 5 MB
                string fileData = null;
                if (transfer.TotalSize > 5 * 1024 * 1024)
                {
                    fileData = "*** File is too large to be viewed in this sample. ***";
                }
                else if (transfer.TotalSize > 0)
                {
                    fileData = System.Text.Encoding.UTF8.GetString(transfer.Data);
                }
                else
                {
                    fileData = string.Empty;
                }

                // TODO check for binary data and don't display

                StorageData[fileName] = fileData;

                int fileSize = 0;
                if (fileData != null)
                {
                    fileSize = fileData.Length;
                }

                Debug.LogFormat("[EOS SDK] Player data storage: file read finished: '{0}' Size: {1}.", fileName, fileSize);

                TransfersInProgress.Remove(fileName);

                if (fileName.Equals(CurrentTransferName, StringComparison.OrdinalIgnoreCase))
                {
                    ClearCurrentTransfer();
                }
            }
        }

        private void FinishFileUpload(string fileName, Action fileUploadCallback = null)
        {
            if (TransfersInProgress.TryGetValue(fileName, out EOSTransferInProgress transfer))
            {
                if (transfer.Download)
                {
                    Debug.LogError("[EOS SDK] Player data storage: error while file write operation: can't finish because of download/upload mismatch.");
                    return;
                }

                if (!transfer.Done())
                {
                    Debug.LogError("[EOS SDK] Player data storage: error while file write operation: unexpected end of transfer.");
                }

                TransfersInProgress.Remove(fileName);

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
        public void OnLoggedIn()
        {
            QueryFileList();
        }

        /// <summary>User Logged Out actions</summary>
        /// <list type="bullet">
        ///     <item><description>Clear Cache and Current Transfer</description></item>
        /// </list>
        public void OnLoggedOut()
        {
            StorageData.Clear();
            ClearCurrentTransfer();
        }

        private void ClearCurrentTransfer()
        {
            CurrentTransferName = string.Empty;
            CurrentTransferProgress = 0.0f;

            if (CurrentTransferHandle != null)
            {
                CurrentTransferHandle.Release();
                CurrentTransferHandle = null;
            }
        }

        //-------------------------------------------------------------------------
        private void OnFileListRetrieved(ref QueryFileListCallbackInfo data)
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
            foreach (var callback in FileListUpdateCallbacks)
            {
                callback?.Invoke();
            }
        }

        //-------------------------------------------------------------------------
        private ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileDataReceived): data parameter is null!");
            //    return ReadResult.FailRequest;
            //}

            return ReceiveData(data.Filename, data.DataChunk, data.TotalFileSizeBytes, data.IsLastChunk);
        }

        //-------------------------------------------------------------------------
        private void OnFileReceived(ref ReadFileCallbackInfo data)
        {
            var callback = data.ClientData as Action;
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileReceived): data parameter is null!");
            //    FinishFileDownload(data.Filename, false);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: could not download file: {0}", data.ResultCode);
                FinishFileDownload(data.Filename, false);
            }

            FinishFileDownload(data.Filename, true);

            callback?.Invoke();
        }

        private WriteResult OnFileDataSend(ref WriteFileDataCallbackInfo data, out ArraySegment<byte> outDataBuffer)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileDataSend): data parameter is null!");
            //    outDataBuffer = new byte[0];
            //    return WriteResult.FailRequest;
            //}

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
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileSent): data parameter is null!");
            //    return;
            //}

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
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileTransferProgressUpdated): data parameter is null!");
            //    return;
            //}

            if (data.TotalFileSizeBytes > 0)
            {
                UpdateProgress(data.Filename, data.BytesTransferred / data.TotalFileSizeBytes);
                Debug.LogFormat("[EOS SDK] Player data storage: transfer progress {0} / {1}", data.BytesTransferred, data.TotalFileSizeBytes);
            }
        }

        private void OnFileCopied(ref DuplicateFileCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileCopied): data parameter is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: error while copying the file: {0}", data.ResultCode);
                return;
            }

            QueryFileList();
        }

        private void OnFileRemoved(ref DeleteFileCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Player Data Storage (OnFileRemoved): data parameter is null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: error while removing file: {0}", data.ResultCode);
                return;
            }

            QueryFileList();
        }
    }
}
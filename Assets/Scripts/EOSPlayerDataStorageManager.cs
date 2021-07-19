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
    /// <summary>Class <c>EOSPlayerDataStorageManager</c> is a simplified wrapper for EOS PlayerDataStorageInterface.</summary>
    public class EOSPlayerDataStorageManager : IEOSSubManager
    {
        private const uint MAX_CHUNK_SIZE = 4096;
        private PlayerDataStorageFileTransferRequest CurrentTransferHandle;
        private Dictionary<string, EOSTransferInProgress> TransfersInProgress;
        private float CurrentTransferProgress;
        private string CurrentTransferName;
        public Action UIOnFileContentUpdated { get; private set; }

        private Dictionary<string, string> StorageData;

        public EOSPlayerDataStorageManager()
        {
            TransfersInProgress = new Dictionary<string, EOSTransferInProgress>();
            StorageData = new Dictionary<string, string>();
        }

        /// <summary>Returns cached Storage Data list.</summary>
        /// <returns>Dictionary<string, string> cached StorageData where FileName is key and FileContent is value.</returns>
        public Dictionary<string, string> GetCachedStorageData()
        {
            return StorageData;
        }

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

            EOSManager.Instance.GetEOSPlatformInterface().GetPlayerDataStorageInterface().QueryFileList(options, null, OnFileListRetrieved);
        }

        /// <summary>(async) Begin file data download.</summary>
        /// <param name"fileName">Name of file.</param>
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

            if (downloadCompletedCallback != null)
            {
                UIOnFileContentUpdated = downloadCompletedCallback;
            }

            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetEOSPlatformInterface().GetPlayerDataStorageInterface().ReadFile(options, null, OnFileReceived);
            if (req == null)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: can't start file download, bad handle returned for filename '{0}'", fileName);
                return;
            }

            CancelCurrentTransfer();
            CurrentTransferHandle = req;

            EOSTransferInProgress newTransfer = new EOSTransferInProgress()
            {
                Download = true
            };

            TransfersInProgress[fileName] = newTransfer;

            CurrentTransferProgress = 0.0f;
            CurrentTransferName = fileName;
        }

        /// <summary>(async) Begin file data upload.</summary>
        /// <param name"fileName">Name of file.</param>
        /// <returns>True if upload started</returns>
        public bool StartFileDataUpload(string fileName)
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

            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetEOSPlatformInterface().GetPlayerDataStorageInterface().WriteFile(options, null, OnFileSent);
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
                newTransfer.Data = new List<char>(fileData.ToCharArray());
            }
            newTransfer.CurrentIndex = 0;

            TransfersInProgress[fileName] = newTransfer;

            CurrentTransferProgress = 0.0f;
            CurrentTransferName = fileName;

            return true;
        }

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

        /// <summary>(async) Begin file data upload.</summary>
        /// <param name"fileName">Name of file.</param>
        /// <param name"fileContent">File content.</param>
        /// <param name"fileCreatedCallback">Function called when file creation and upload is completed.</param>
        public void AddFile(string fileName, string fileContent, Action fileCreatedCallback = null)
        {
            SetLocalData(fileName, fileContent);

            if (fileCreatedCallback != null)
            {
                UIOnFileContentUpdated = fileCreatedCallback;
            }

            if (!StartFileDataUpload(fileName))
            {
                EraseLocalData(fileName);
            }
        }

        /// <summary>(async) Begin file data upload.</summary>
        /// <param name"sourceFileName">Name of source file in EOS backend.</param>
        /// <param name"destinationFileName">Name of target file to copy to.</param>
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

            EOSManager.Instance.GetEOSPlatformInterface().GetPlayerDataStorageInterface().DuplicateFile(options, null, OnFileCopied);
        }

        /// <summary>(async) Begin file delete.</summary>
        /// <param name"fileName">Name of source file to delete.</param>
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

            EOSManager.Instance.GetEOSPlatformInterface().GetPlayerDataStorageInterface().DeleteFile(options, null, OnFileRemoved);
        }

        /// <summary>Get cached file content for specified file name.</summary>
        /// <param name"fileName">Name of file.</param>
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

        private ReadResult ReceiveData(string fileName, byte[] data, uint totalSize, bool isLastChunk)
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
                    //byte[] byteArray = ObjectToByteArray(data);
                    char[] charArray = System.Text.Encoding.UTF8.GetChars(data); //transfer.Data.ToArray();

                    transfer.Data.AddRange(charArray);

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

        private WriteResult SendData(string fileName, out byte[] data)
        {
            data = null;

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
                    char[] charArray = transfer.Data.ToArray();
                    data = System.Text.Encoding.UTF8.GetBytes(charArray);
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
                    fileData = new string(transfer.Data.ToArray());
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

        private void FinishFileUpload(string fileName)
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

                if (UIOnFileContentUpdated != null)
                {
                    UIOnFileContentUpdated();
                    UIOnFileContentUpdated = null;
                }
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
        ///     <item><description>Clear Cache & Current Transfer</description></item>
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

        private void OnFileListRetrieved(QueryFileListCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileListRetrieved): data parameter is null!");
                return;
            }

            if (data.ResultCode != Result.Success)
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

            PlayerDataStorageInterface playerStorageHandle = EOSManager.Instance.GetEOSPlatformInterface().GetPlayerDataStorageInterface();
            List<string> fileNames = new List<string>();

            for (uint fileIndex = 0; fileIndex < data.FileCount; fileIndex++)
            {
                CopyFileMetadataAtIndexOptions options = new CopyFileMetadataAtIndexOptions()
                {
                    LocalUserId = localUserId,
                    Index = fileIndex
                };

                Result result = playerStorageHandle.CopyFileMetadataAtIndex(options, out FileMetadata fileMetadata);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("Player Data Storage (OnFileListRetrieved): CopyFileMetadataAtIndex returned error result = {0}", result);
                    return;
                }
                else if (fileMetadata != null)
                {
                    if (!string.IsNullOrEmpty(fileMetadata.Filename))
                    {
                        fileNames.Add(fileMetadata.Filename);
                    }
                }
            }

            SetFileList(fileNames);
        }

        private ReadResult OnFileDataReceived(ReadFileDataCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileDataReceived): data parameter is null!");
                return ReadResult.FailRequest;
            }

            return ReceiveData(data.Filename, data.DataChunk, data.TotalFileSizeBytes, data.IsLastChunk);
        }

        private void OnFileReceived(ReadFileCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileReceived): data parameter is null!");
                FinishFileDownload(data.Filename, false);
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: could not download file: {0}", data.ResultCode);
                FinishFileDownload(data.Filename, false);
            }

            FinishFileDownload(data.Filename, true);

            if (UIOnFileContentUpdated != null)
            {
                UIOnFileContentUpdated();
                UIOnFileContentUpdated = null;
            }
        }

        private WriteResult OnFileDataSend(WriteFileDataCallbackInfo data, out byte[] outDataBuffer)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileDataSend): data parameter is null!");
                outDataBuffer = new byte[0];
                return WriteResult.FailRequest;
            }

            WriteResult result = SendData(data.Filename, out byte[] dataBuffer);

            if (dataBuffer != null)
            {
                outDataBuffer = dataBuffer;
            }
            else
            {
                outDataBuffer = new byte[0];
            }

            return result;
        }

        private void OnFileSent(WriteFileCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileSent): data parameter is null!");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: could not upload file: {0}", data.ResultCode);
                FinishFileUpload(data.Filename);
                return;
            }

            FinishFileUpload(data.Filename);
        }

        private void OnFileTransferProgressUpdated(FileTransferProgressCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileTransferProgressUpdated): data parameter is null!");
                return;
            }

            if (data.TotalFileSizeBytes > 0)
            {
                UpdateProgress(data.Filename, data.BytesTransferred / data.TotalFileSizeBytes);
                Debug.LogFormat("[EOS SDK] Player data storage: transfer progress {0} / {1}", data.BytesTransferred, data.TotalFileSizeBytes);
            }
        }

        private void OnFileCopied(DuplicateFileCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileCopied): data parameter is null!");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: error while copying the file: {0}", data.ResultCode);
                return;
            }

            QueryFileList();
        }

        private void OnFileRemoved(DeleteFileCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Player Data Storage (OnFileRemoved): data parameter is null!");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("[EOS SDK] Player data storage: error while removing file: {0}", data.ResultCode);
                return;
            }

            QueryFileList();
        }
    }
}
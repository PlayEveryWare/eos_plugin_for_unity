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

//#define ENABLE_DEBUG_PLAYERDATASTORAGE_SERVICE

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    using UnityEngine;
    using UnityEngine.Networking;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Achievements;
    using System.Threading.Tasks;
    using Epic.OnlineServices.PlayerDataStorage;
    using System.Threading;

    /// <summary>
    /// Class <c>AchievementsService</c> is a simplified wrapper for
    /// EOS [Achievements Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Achievements/index.html).
    /// </summary>
    public class PlayerDataStorageService : EOSService
    {
        private const uint MAX_CHUNK_SIZE = 4096;

        /// <summary>
        /// The EOS SDK can only handle one incoming or outgoing request at a time for several functions.
        /// This semaphore can be used to gate calls, waiting until they're ready to be processed.
        /// </summary>
        private SemaphoreSlim _playerDataStorageSemaphore = new SemaphoreSlim(1);

        #region Singleton Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the PlayerDataStorageService
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
        private PlayerDataStorageService() : base(true) { }

        #endregion

        protected override Task InternalRefreshAsync()
        {
            return Task.CompletedTask;
        }

        protected override void OnPlayerLogin(ProductUserId productUserId)
        {

        }

        /// <summary>
        /// Conditionally executed proxy function for Unity's log function.
        /// </summary>
        /// <param name="toPrint">The message to log.</param>
        [Conditional("ENABLE_DEBUG_PLAYERDATASTORAGE_SERVICE")]
        internal static void Log(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }

        /// <summary>
        /// Returns a list of all files associated with the provided user.
        /// </summary>
        /// <param name="userId">The product user to query for.</param>
        /// <returns>
        /// A task containing the name of each file associated with the product user.
        /// If there were any errors during the query, the task will only contain an exception.
        /// </returns>
        public Task<List<string>> QueryFileList(ProductUserId userId)
        {
            if (!userId.IsValid())
            {
                return Task.FromException<List<string>>(new ArgumentException($"{nameof(PlayerDataStorageService)} ({nameof(QueryFileList)}): {nameof(userId)} is not valid."));
            }

            TaskCompletionSource<List<string>> tcs = new();

            Task.Factory.StartNew<Task>(async () =>
            {
                // Wait for the semaphore; only one operation allowed at a time
                await _playerDataStorageSemaphore.WaitAsync();

                QueryFileListOptions options = new QueryFileListOptions
                {
                    LocalUserId = userId
                };

                // Query the file list, and on callback set the TaskCompletionSource's result if possible
                EOSManager.Instance.GetPlayerDataStorageInterface().QueryFileList(ref options, null, (ref QueryFileListCallbackInfo callbackInfo) =>
                {
                    try
                    {
                        // If there are no files for this user, the query will return "not found", so give an empty list
                        if (callbackInfo.ResultCode == Result.NotFound)
                        {
                            tcs.SetResult(new List<string>());
                            return;
                        }

                        if (callbackInfo.ResultCode != Result.Success)
                        {
                            tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(QueryFileList)}): Failed to query file list. Result code {callbackInfo.ResultCode}"));
                            return;
                        }

                        uint fileCount = callbackInfo.FileCount;
                        PlayerDataStorageInterface playerStorageHandle = EOSManager.Instance.GetPlayerDataStorageInterface();
                        List<string> fileNames = new List<string>();

                        // With the retrieved files, get the name of each file (which is in the metadata),
                        // and compose a list of the file names
                        for (uint fileIndex = 0; fileIndex < callbackInfo.FileCount; fileIndex++)
                        {
                            CopyFileMetadataAtIndexOptions options = new CopyFileMetadataAtIndexOptions()
                            {
                                LocalUserId = userId,
                                Index = fileIndex
                            };

                            Result result = playerStorageHandle.CopyFileMetadataAtIndex(ref options, out FileMetadata? fileMetadata);

                            if (result != Result.Success)
                            {
                                tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(DownloadFile)}): CopyFileMetadataAtIndex returned error. Result code {result}"));
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

                        tcs.SetResult(fileNames);
                    }
                    finally
                    {
                        _playerDataStorageSemaphore.Release();
                    }
                });
            });

            return tcs.Task;
        }

        /// <summary>
        /// Starts a download from the player data storage.
        /// This version calls <see cref="DownloadFile(ProductUserId, string)"/>,
        /// and simplifies the return down to just an awaitable Task if the wrapper class data is not needed.
        /// If an exception occurs, the returned task will propagate that exception.
        /// </summary>
        /// <param name="userId">The user id that owns the player data storage file to download.</param>
        /// <param name="fileName">The name of the file to download.</param>
        /// <returns>An awaitable task containing either the result or an exception.</returns>
        public async Task<byte[]> DownloadFileAsync(ProductUserId userId, string fileName)
        {
            EOSPlayerDataStorageTransferTask transferTask = DownloadFile(userId, fileName);

            await transferTask.InnerTaskCompletionSource.Task;

            if (transferTask.InnerTaskCompletionSource.Task.IsFaulted)
            {
                return await Task<byte[]>.FromException<byte[]>(new AggregateException($"{nameof(PlayerDataStorageService)} ({nameof(DownloadFileAsync)}): Failed to download file. Result code {transferTask.ResultCode}", transferTask.InnerTaskCompletionSource.Task.Exception));
            }

            return transferTask.Data;
        }

        /// <summary>
        /// Starts a file download from the player data storage.
        /// The returned object can be used to await for it to be completed, and note its progress.
        /// </summary>
        /// <param name="userId">The user id that owns the player data storage file to download.</param>
        /// <param name="fileName">The name of the file to download.</param>
        /// <returns>An object containing the working task and eventually the result.</returns>
        public EOSPlayerDataStorageTransferTask DownloadFile(ProductUserId userId, string fileName)
        {
            if (userId == null || !userId.IsValid())
            {
                return new EOSPlayerDataStorageTransferTask(
                    Result.InvalidProductUserID, 
                    new ArgumentException($"{nameof(PlayerDataStorageService)} ({nameof(DownloadFile)}): User id is not valid")
                    );
            }

            TaskCompletionSource<byte[]> tcs = new();
            EOSPlayerDataStorageTransferTask downloadTask = new EOSPlayerDataStorageTransferTask(tcs);

            Task.Factory.StartNew<Task>(async () =>
            {
                // Wait for the semaphore; only one operation allowed at a time
                await _playerDataStorageSemaphore.WaitAsync();

                var queryFileOptions = new QueryFileOptions { Filename = fileName, LocalUserId = userId };

                EOSManager.Instance.GetPlayerDataStorageInterface().QueryFile(ref queryFileOptions, null, (ref QueryFileCallbackInfo data) =>
                {
                    if (data.ResultCode == Result.NotFound)
                    {
                        // This file wasn't found online. It's not an error, just pass along that it was missing
                        downloadTask.ResultCode = data.ResultCode;
                        tcs.SetResult(null);
                        _playerDataStorageSemaphore.Release();
                        return;
                    }

                    if (data.ResultCode != Result.Success)
                    {
                        downloadTask.ResultCode = data.ResultCode;
                        tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(DownloadFile)}): Failed to download file. Result code {data.ResultCode}"));
                        _playerDataStorageSemaphore.Release();
                        return;
                    }

                    var copyFileMetadataOptions = new CopyFileMetadataByFilenameOptions { Filename = fileName, LocalUserId = userId };
                    Result metadataRetrievalResult = EOSManager.Instance.GetPlayerDataStorageInterface().CopyFileMetadataByFilename(ref copyFileMetadataOptions, out FileMetadata? fileMetadata);

                    if (metadataRetrievalResult != Result.Success)
                    {
                        downloadTask.ResultCode = metadataRetrievalResult;
                        tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(DownloadFile)}): Failed to retrieve file metadata. Result code {metadataRetrievalResult}"));
                        _playerDataStorageSemaphore.Release();
                        return;
                    }

                    downloadTask.SetDataBufferSize((uint)fileMetadata?.FileSizeBytes);

                    // The returned EOSPlayerDataStorageTransferTask holds the callbacks for file progress and finishing
                    ReadFileOptions readOptions = new ReadFileOptions
                    {
                        LocalUserId = userId,
                        Filename = fileName,
                        ReadChunkLengthBytes = MAX_CHUNK_SIZE,
                        ReadFileDataCallback = downloadTask.OnFileDataReceived,
                        FileTransferProgressCallback = downloadTask.OnFileTransferProgressUpdated
                    };

                    downloadTask.TransferRequest = EOSManager.Instance.GetPlayerDataStorageInterface().ReadFile(ref readOptions, null,
                        (ref ReadFileCallbackInfo info) => 
                    {
                        downloadTask.OnFileReceived(ref info);
                        _playerDataStorageSemaphore.Release();
                    });

                    // If no valid handle is returned, then error out
                    if (downloadTask.TransferRequest == null)
                    {
                        downloadTask.ResultCode = Result.UnexpectedError;
                        tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(DownloadFile)}): Bad handle returned for reading file, can not download file."));
                        _playerDataStorageSemaphore.Release();
                        return;
                    }
                });
            });

            return downloadTask;
        }

        /// <summary>
        /// Starts a file upload to the player data storage.
        /// This version calls <see cref="UploadFile(string, byte[])"/>,
        /// and simplifies the return down to just an awaitable Task if the wrapper class data is not needed.
        /// If an exception occurs, the returned task will propagate that exception.
        /// </summary>
        /// <param name="fileName">The name of the file to download.</param>
        /// <param name="dataToUpload">The data to upload.</param>
        /// <returns>An awaitable task containing either the result or an exception.</returns>
        public async Task<byte[]> UploadFileAsync(string fileName, byte[] dataToUpload)
        {
            EOSPlayerDataStorageTransferTask transferTask = UploadFile(fileName, dataToUpload);

            await transferTask.InnerTaskCompletionSource.Task;

            if (transferTask.InnerTaskCompletionSource.Task.IsFaulted)
            {
                return await Task<byte[]>.FromException<byte[]>(new AggregateException($"{nameof(PlayerDataStorageService)} ({nameof(UploadFileAsync)}): Failed to upload file. Result code {transferTask.ResultCode}", transferTask.InnerTaskCompletionSource.Task.Exception));
            }

            return transferTask.Data;
        }

        /// <summary>
        /// Starts a file upload to the player data storage.
        /// The returned object can be used to await for it to be completed, and note its progress.
        /// </summary>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="dataToUpload">The data to upload.</param>
        /// <returns>An object containing the working task and eventually the result of the operation..</returns>
        public EOSPlayerDataStorageTransferTask UploadFile(string fileName, byte[] dataToUpload)
        {
            ProductUserId localProductUserId = EOSManager.Instance.GetProductUserId();

            if (localProductUserId == null || !localProductUserId.IsValid())
            {
                return new EOSPlayerDataStorageTransferTask(
                    Result.InvalidProductUserID,
                    new ArgumentException($"{nameof(PlayerDataStorageService)} ({nameof(UploadFile)}): User id is not valid")
                    );
            }

            TaskCompletionSource<byte[]> tcs = new();
            EOSPlayerDataStorageTransferTask uploadTask = new EOSPlayerDataStorageTransferTask(tcs, dataToUpload);

            Task.Factory.StartNew<Task>(async () =>
            {
                // Wait for the semaphore; only one operation allowed at a time
                await _playerDataStorageSemaphore.WaitAsync();

                var queryFileOptions = new QueryFileOptions { Filename = fileName, LocalUserId = localProductUserId };

                // Note that the dataToUpload is not passed in as a WriteFileOptions;
                // it is the responsibility of the EOSTransferTask to respond with bytes to upload when requested,
                // which is covered in the below function callbacks
                WriteFileOptions options = new WriteFileOptions()
                {
                    LocalUserId = localProductUserId,
                    Filename = fileName,
                    ChunkLengthBytes = MAX_CHUNK_SIZE,
                    WriteFileDataCallback = uploadTask.OnFileDataSend,
                    FileTransferProgressCallback = uploadTask.OnFileTransferProgressUpdated
                };

                uploadTask.TransferRequest = EOSManager.Instance.GetPlayerDataStorageInterface().WriteFile(ref options, null, (ref WriteFileCallbackInfo info) => 
                { 
                    uploadTask.OnFileSent(ref info); 
                    _playerDataStorageSemaphore.Release(); 
                });

                // If no valid handle is returned, then error out
                if (uploadTask.TransferRequest == null)
                {
                    uploadTask.ResultCode = Result.UnexpectedError;
                    tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(UploadFile)}): Bad handle returned for uploading file, can not upload file."));
                    _playerDataStorageSemaphore.Release();
                    return;
                }
            });

            return uploadTask;
        }

        /// <summary>
        /// Deletes a file from Player Data Storage.
        /// </summary>
        /// <param name="fileName">The file name to delete.</param>
        /// <returns>An awaitable task. Contains an exception if an error occurs.</returns>
        public Task DeleteFile(string fileName)
        {
            TaskCompletionSource<Result> tcs = new();

            ProductUserId localProductUserId = EOSManager.Instance.GetProductUserId();

            if (localProductUserId == null || !localProductUserId.IsValid())
            {
                return Task.FromException(new ArgumentException($"{nameof(PlayerDataStorageService)} ({nameof(DeleteFile)}): User id is not valid"));
            }

            Task.Factory.StartNew<Task>(async () =>
            {
                // Wait for the semaphore; only one operation allowed at a time
                await _playerDataStorageSemaphore.WaitAsync();

                DeleteFileOptions options = new DeleteFileOptions()
                {
                    LocalUserId = localProductUserId,
                    Filename = fileName
                };

                EOSManager.Instance.GetPlayerDataStorageInterface().DeleteFile(ref options, null, (ref DeleteFileCallbackInfo info) =>
                {
                    if (info.ResultCode != Result.Success)
                    {
                        tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(DeleteFile)}): Failed to delete file. Result code {info.ResultCode}"));
                    }

                    _playerDataStorageSemaphore.Release();
                    tcs.SetResult(info.ResultCode);
                });
            });

            return tcs.Task;
        }

        /// <summary>
        /// Copies a file in Player Data Storage.
        /// </summary>
        /// <param name="fileNameSource">The name of the file to copy.</param>
        /// <param name="fileNameDestination">The resulting name of the copied file.</param>
        /// <returns>An awaitalbe task. Contains an exception if an error occurs.</returns>
        public Task CopyFile(string fileNameSource, string fileNameDestination)
        {
            TaskCompletionSource<Result> tcs = new();

            ProductUserId localProductUserId = EOSManager.Instance.GetProductUserId();

            if (localProductUserId == null || !localProductUserId.IsValid())
            {
                return Task.FromException(new ArgumentException($"{nameof(PlayerDataStorageService)} ({nameof(DeleteFile)}): User id is not valid"));
            }

            Task.Factory.StartNew<Task>(async () =>
            {
                // Wait for the semaphore; only one operation allowed at a time
                await _playerDataStorageSemaphore.WaitAsync();
                DuplicateFileOptions options = new DuplicateFileOptions()
                {
                    LocalUserId = localProductUserId,
                    SourceFilename = fileNameSource,
                    DestinationFilename = fileNameDestination
                };

                EOSManager.Instance.GetPlayerDataStorageInterface().DuplicateFile(ref options, null, (ref DuplicateFileCallbackInfo info) =>
                {
                    if (info.ResultCode != Result.Success)
                    {
                        tcs.SetException(new Exception($"{nameof(PlayerDataStorageService)} ({nameof(DeleteFile)}): Failed to delete file. Result code {info.ResultCode}"));
                    }

                    _playerDataStorageSemaphore.Release();
                    tcs.SetResult(info.ResultCode);
                });
            });

            return tcs.Task;
        }
    }
}
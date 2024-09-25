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
 */

namespace PlayEveryWare.EpicOnlineServices.Tests.Services.PlayerStorage
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.PlayerDataStorage;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Tests for the player storage functionality of EOS.
    /// </summary>
    public partial class PlayerStorageTests : EOSTestBase
    {
        private const uint MAX_CHUNK_SIZE = 4096;
        private static readonly System.Random random = new();

        private string _currentFilename;
        private uint _totalSize = 0;
        private uint _currentIndex = 0;
        private List<char> _testFileData = new();
        private List<char> _receiveFileData = new();

        /// <summary>
        /// Class of parameters for easily testing different types of data to upload
        /// </summary>
        public class FileTestParameters
        {
            public string Content { get; set; }
            public string Filename { get; set; }
        }

        // Provides different test cases by changing one of the parameters in each test case.
        public static FileTestParameters[] fileParameters = {
            new() { Content = "Sample integration test file content.", Filename = "IntegrationFileTest" },
            new() { Content = GenerateRandomTestString(5000), Filename = "IntegrationLargeFileTest" },
        };

        /// <summary>
        /// Cleans up the file, if any, when the test ends. This prevents files from remaining if a test failed after uploading.
        /// </summary>
        [UnityTearDown]
        public IEnumerator CleanupFile()
        {
            if (!string.IsNullOrWhiteSpace(_currentFilename))
            {
                // Delete the file to reset everything
                DeleteFileOptions deleteOptions = new()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Filename = _currentFilename,
                };

                DeleteFileCallbackInfo? deleteResult = null;
                EOSManager.Instance.GetPlayerDataStorageInterface().DeleteFile(ref deleteOptions, null, (ref DeleteFileCallbackInfo data) => { deleteResult = data; });

                // No need to check the result as it's possible a test case may fail to upload the file
                yield return new WaitUntil(() => deleteResult != null);
            }

            _currentFilename = string.Empty;
        }

        /// <summary>
        /// Queries the initial state of the player storage, which should be blank.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator PlayerStorageShouldBeBlank()
        {
            var listOptions = new QueryFileListOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
            };

            QueryFileListCallbackInfo? queryResult = null;
            EOSManager.Instance.GetPlayerDataStorageInterface().QueryFileList(ref listOptions, null, (ref QueryFileListCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);

            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
                Assert.AreEqual(0, queryResult.Value.FileCount);
            }
        }

        /// <summary>
        /// Adds, downloads, and deletes a test file.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator AddDownloadDeleteFileInPlayerStorage([ValueSource(nameof(fileParameters))] FileTestParameters parameters)
        {
            _currentFilename = parameters.Filename;
            _testFileData = new List<char>(parameters.Content.ToCharArray());
            _totalSize = (uint)parameters.Content.Length;
            _receiveFileData.Clear();
            _currentIndex = 0;

            // First, upload the file
            WriteFileOptions writeOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = parameters.Filename,
                ChunkLengthBytes = MAX_CHUNK_SIZE,
                WriteFileDataCallback = OnFileDataSend,
                FileTransferProgressCallback = null,
            };

            WriteFileCallbackInfo? writeResult = null;
            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetPlayerDataStorageInterface().WriteFile(
                ref writeOptions,
                null,
                (ref WriteFileCallbackInfo data) => { writeResult = data; });
            Assert.IsNotNull(req, $"Player data storage: can't start file upload, bad handle returned for filename {writeOptions.Filename}");

            yield return new WaitUntil(() => writeResult != null);

            if (writeResult != null)
            {
                Assert.AreEqual(Result.Success, writeResult.Value.ResultCode);
            }

            // Now download the file and verify the contents are the same as uploaded
            _currentIndex = 0;
            _totalSize = 0;

            ReadFileOptions readOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = _currentFilename,
                ReadChunkLengthBytes = MAX_CHUNK_SIZE,
                ReadFileDataCallback = OnFileDataReceived,
                FileTransferProgressCallback = null,
            };

            ReadFileCallbackInfo? downloadResult = null;
            req = EOSManager.Instance.GetPlayerDataStorageInterface().ReadFile(
                ref readOptions,
                null,
                (ref ReadFileCallbackInfo data) => { downloadResult = data; });
            Assert.IsNotNull(req, $"Player data storage: can't start file download, bad handle returned for filename {readOptions.Filename}");

            yield return new WaitUntil(() => downloadResult != null);

            if (downloadResult != null)
            {
                Assert.AreEqual(Result.Success, downloadResult.Value.ResultCode);
            }

            // Combine the char array into a string for easier comparison
            string receive = string.Join("", _receiveFileData.ToArray());
            Assert.AreEqual(parameters.Content, receive, $"Downloaded file differs from the upload for filename {_currentFilename}.");
        }

        /// <summary>
        /// Attempt to upload a file with the wrong specified chunk size.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator AddFileWithWrongChunkSize()
        {
            const string testChunkContent = "TestChunkContent";

            _currentFilename = "IntegrationTestIncorrectChunk";
            _testFileData = new List<char>(testChunkContent.ToCharArray());
            _totalSize = (uint)testChunkContent.Length;
            _receiveFileData.Clear();
            _currentIndex = 0;

            // First, upload the file
            WriteFileOptions writeOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = _currentFilename,
                ChunkLengthBytes = 4,
                WriteFileDataCallback = (ref WriteFileDataCallbackInfo data, out ArraySegment<byte> outDataBuffer) =>
                {
                    // Callback will attempt to upload the whole thing instead of by chunk
                    Assert.IsNotNull(data, "Data is null when sending file.");
                    char[] charArray = _testFileData.ToArray();
                    outDataBuffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(charArray));

                    return WriteResult.CompleteRequest;
                },
                FileTransferProgressCallback = null,
            };

            WriteFileCallbackInfo? writeResult = null;
            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetPlayerDataStorageInterface().WriteFile(
                ref writeOptions,
                null,
                (ref WriteFileCallbackInfo data) => { writeResult = data; });
            Assert.IsNotNull(req, $"Player data storage: can't start file upload, bad handle returned for filename {writeOptions.Filename}");

            yield return new WaitUntil(() => writeResult != null);

            if (writeResult != null)
            {
                Assert.AreNotEqual(Result.Success, writeResult.Value.ResultCode);
            }
        }

        /// <summary>
        /// Tests duplicating a file already in storage.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator DuplicateFileTest()
        {
            const string testChunkContent = "TestChunkContent";
            const string sourceFileName = "IntegrationTestFirstFile";
            const string destinationFileName = "IntegrationTestCopyFile";

            _currentFilename = sourceFileName;
            _testFileData = new List<char>(testChunkContent.ToCharArray());
            _totalSize = (uint)testChunkContent.Length;
            _receiveFileData.Clear();
            _currentIndex = 0;

            // Upload the first file
            WriteFileOptions writeOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = _currentFilename,
                ChunkLengthBytes = MAX_CHUNK_SIZE,
                WriteFileDataCallback = OnFileDataSend,
                FileTransferProgressCallback = null,
            };

            WriteFileCallbackInfo? writeResult = null;
            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetPlayerDataStorageInterface().WriteFile(
                ref writeOptions,
                null,
                (ref WriteFileCallbackInfo data) => { writeResult = data; });
            Assert.IsNotNull(req, $"Player data storage: can't start file upload, bad handle returned for filename {writeOptions.Filename}");

            yield return new WaitUntil(() => writeResult != null);

            if (writeResult != null)
			{
				Assert.AreEqual(Result.Success, writeResult.Value.ResultCode);
			}

            // Copy the file on the player storage
            DuplicateFileOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SourceFilename = sourceFileName,
                DestinationFilename = destinationFileName
            };

            DuplicateFileCallbackInfo? duplicateResult = null;
            EOSManager.Instance.GetPlayerDataStorageInterface().DuplicateFile(
                ref options,
                null,
                (ref DuplicateFileCallbackInfo data) => { duplicateResult = data; });

            yield return new WaitUntil(() => duplicateResult != null);

            if (duplicateResult != null)
            {
                Assert.AreEqual(Result.Success, duplicateResult.Value.ResultCode);
            }

            // Now download the file and verify the contents are the same as uploaded
            _currentIndex = 0;
            _totalSize = 0;

            ReadFileOptions readOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = destinationFileName,
                ReadChunkLengthBytes = MAX_CHUNK_SIZE,
                ReadFileDataCallback = OnFileDataReceived,
                FileTransferProgressCallback = null,
            };

            ReadFileCallbackInfo? downloadResult = null;
            req = EOSManager.Instance.GetPlayerDataStorageInterface().ReadFile(
                ref readOptions,
                null,
                (ref ReadFileCallbackInfo data) => { downloadResult = data; });
            Assert.IsNotNull(req, $"Player data storage: can't start file download, bad handle returned for filename {readOptions.Filename}");

            yield return new WaitUntil(() => downloadResult != null);

            if (downloadResult != null)
            {
                Assert.AreEqual(Result.Success, downloadResult.Value.ResultCode);
            }

            // Combine the char array into a string for easier comparison
            string receiveFileContent = string.Join("", _receiveFileData.ToArray());
            Assert.AreEqual(testChunkContent, receiveFileContent, $"Downloaded file differs from the upload for filename {destinationFileName}.");

            // Remove the copied file, the UnityTearDown will handle the first file
            DeleteFileOptions deleteOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = destinationFileName,
            };

            DeleteFileCallbackInfo? deleteResult = null;
            EOSManager.Instance.GetPlayerDataStorageInterface().DeleteFile(ref deleteOptions, null, (ref DeleteFileCallbackInfo data) => { deleteResult = data; });

            yield return new WaitUntil(() => deleteResult != null);

            if (deleteResult != null)
            {
                Assert.AreEqual(Result.Success, deleteResult.Value.ResultCode);
            }
        }

        /// <summary>
        /// Tests querying for files uploaded by the player.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryUploadedFileTest()
        {
            const string testChunkContent = "TestChunkContent";
            const string queryFileName = "IntegrationTestQueryFileOne";

            _currentFilename = queryFileName;
            _testFileData = new List<char>(testChunkContent.ToCharArray());
            _totalSize = (uint)testChunkContent.Length;
            _receiveFileData.Clear();
            _currentIndex = 0;

            // Upload the first file
            WriteFileOptions writeOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Filename = _currentFilename,
                ChunkLengthBytes = MAX_CHUNK_SIZE,
                WriteFileDataCallback = OnFileDataSend,
                FileTransferProgressCallback = null,
            };

            WriteFileCallbackInfo? writeResult = null;
            PlayerDataStorageFileTransferRequest req = EOSManager.Instance.GetPlayerDataStorageInterface().WriteFile(
                ref writeOptions,
                null,
                (ref WriteFileCallbackInfo data) => { writeResult = data; });
            Assert.IsNotNull(req, $"Player data storage: can't start file upload, bad handle returned for filename {writeOptions.Filename}");

            yield return new WaitUntil(() => writeResult != null);

            if (writeResult != null)
            {
                Assert.AreEqual(Result.Success, writeResult.Value.ResultCode);
            }

            // Get a list of files and verify the proper amount is there. There should be one file.
            QueryFileListOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            QueryFileListCallbackInfo? queryResult = null;
            EOSManager.Instance.GetPlayerDataStorageInterface().QueryFileList(ref options, null, (ref QueryFileListCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);

            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
                Assert.AreEqual(1, queryResult.Value.FileCount, "There should be only one file in player storage.");
            }

            // Verify the file there is the one uploaded
            PlayerDataStorageInterface playerStorageHandle = EOSManager.Instance.GetPlayerDataStorageInterface();
            CopyFileMetadataAtIndexOptions metaOptions = new()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Index = 0
            };

            Result metaResult = playerStorageHandle.CopyFileMetadataAtIndex(ref metaOptions, out FileMetadata? fileMetadata);
            Assert.AreEqual(Result.Success, metaResult);
            Assert.IsNotNull(fileMetadata);
            Assert.AreEqual(_currentFilename, fileMetadata.Value.Filename.ToString());
        }

        /// <summary>
        /// Helper method to generate data to upload for testing.
        /// </summary>
        /// <param name="length">Amount of characters in the string to generate.</param>
        /// <returns>A string of random characters of the specified size.</returns>
        private static string GenerateRandomTestString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Helper callback for writing a file to the EOS system.
        /// </summary>
        /// <param name="data"><see cref="WriteFileDataCallbackInfo"/> information from the write operation.</param>
        /// <param name="outDataBuffer">Output byte array to upload.</param>
        /// <returns><see cref="WriteResult"/> enum, CompleteRequest if the upload is done, ContinueWriting if not.</returns>
        private WriteResult OnFileDataSend(ref WriteFileDataCallbackInfo data, out ArraySegment<byte> outDataBuffer)
        {
            Assert.IsNotNull(data, "Data is null when sending file.");

            uint bytesToWrite = Math.Min(MAX_CHUNK_SIZE, _totalSize - _currentIndex);

            if (bytesToWrite > 0)
            {
                char[] charArray = _testFileData.GetRange((int)_currentIndex, (int)bytesToWrite).ToArray();
                outDataBuffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(charArray));
            }
            else
            {
                outDataBuffer = new ArraySegment<byte>();
            }
            _currentIndex += bytesToWrite;

            return (_currentIndex == _totalSize) ? WriteResult.CompleteRequest : WriteResult.ContinueWriting;
        }

        /// <summary>
        /// Helper callback for downloading a file from the EOS system.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ReadResult OnFileDataReceived(ref ReadFileDataCallbackInfo data)
        {
            Assert.IsNotNull(data, "Data is null when downloading the file.");

            // First update
            if (_currentIndex == 0 && _totalSize == 0)
            {
                _totalSize = data.TotalFileSizeBytes;
            }

            char[] charArray = System.Text.Encoding.UTF8.GetChars(data.DataChunk.ToArray());
            _receiveFileData.AddRange(charArray);
            
            return ReadResult.ContinueReading;
        }
    }
}

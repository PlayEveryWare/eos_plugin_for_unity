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

namespace PlayEveryWare.EpicOnlineServices.Tests.IntegrationTests
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Sessions;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;
    using PlayEveryWare.EpicOnlineServices.Samples;
    using System;
    using UnityEngine.Events;
    using System.Text;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class PlayerDataStorageServiceTests : EOSTestBase
    {
        static int[] NumberOfFilesToMakeFor_UploadedFiles_CanBeQueried = { 1, 3, 10 };

        /// <summary>
        /// When setting up tests for the first time, all files in player storage
        /// should be deleted to ensure a clean slate for the upcoming tests.
        /// </summary>
        [OneTimeSetUp]
        public IEnumerator OneTimeSetUp()
        {
            yield return DestroyAllFiles();
        }

        /// <summary>
        /// Every test should delete all created files upon completion.
        /// </summary>
        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return DestroyAllFiles();
        }

        /// <summary>
        /// Queries all files in storage, and deletes them one at a time.
        /// </summary>
        private IEnumerator DestroyAllFiles()
        {
            // Get all of the files from the server, and delete each of them
            Task<List<string>> queryTask = PlayerDataStorageService.Instance.QueryFileList(EOSManager.Instance.GetProductUserId());
            yield return new WaitUntil(() => queryTask.IsCompleted);
            if (!queryTask.IsCompletedSuccessfully)
            {
                Debug.LogError($"{nameof(PlayerDataStorageServiceTests)} ({nameof(UnityTearDown)}): Failed to query files during clean up. There may be files remaining in storage.");
            }

            foreach (string fileName in queryTask.Result)
            {
                Task deletionTask = PlayerDataStorageService.Instance.DeleteFile(fileName);
                yield return new WaitUntil(() => deletionTask.IsCompleted);
                if (!deletionTask.IsCompletedSuccessfully)
                {
                    Debug.LogError($"{nameof(PlayerDataStorageServiceTests)} ({nameof(UnityTearDown)}): Error encountered while attempting to delete file with name '{fileName}'. It may remain in storage.");
                }
            }
        }

        /// <summary>
        /// Assuming there are no files in storage for the test user, querying for
        /// files should result in a NotFound error.
        /// 
        /// TODO: The Services paradigm doesn't allow for returning of messages.
        /// So the expected behavior is no exception thrown, but do return an empty list.
        /// </summary>
        [UnityTest]
        public IEnumerator QueryTask_NotFound()
        {
            Task<List<string>> queryTask = PlayerDataStorageService.Instance.QueryFileList(EOSManager.Instance.GetProductUserId());
            yield return new WaitUntil(() => queryTask.IsCompleted);
            Assert.IsTrue(queryTask.IsCompletedSuccessfully, "Task did not complete successfully. Should not be an error, even if no entries are found.");
            Assert.NotNull(queryTask.Result, "Task did not return an empty list as expected. Result should not be null.");
            Assert.AreEqual(0, queryTask.Result.Count, $"Returned list is expected to be empty, has {queryTask.Result.Count} entries.");
        }

        /// <summary>
        /// Creates a file, uploads it, and then checks that it exists in the list.
        /// </summary>
        [UnityTest]
        public IEnumerator UploadedFiles_CanBeQueried([ValueSource(nameof(NumberOfFilesToMakeFor_UploadedFiles_CanBeQueried))] int numberOfFilesToMake)
        {
            List<string> randomNames = new List<string>();

            for (int fileIndex = 0; fileIndex < numberOfFilesToMake; fileIndex++)
            {
                // Generate a random name until we hit something that isn't already done
                string randomName = "";
                while (randomName.Contains(randomName))
                {
                    randomName = UnityEngine.Random.Range(0, 9999).ToString();
                }

                byte[] fileBytes = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5 };
                Task uploadTask = PlayerDataStorageService.Instance.UploadFileAsync(randomName, fileBytes);
                yield return new WaitUntil(() => uploadTask.IsCompleted);
                Assert.IsTrue(uploadTask.IsCompletedSuccessfully, "Upload task did not complete successfully.");
            }

            Task<List<string>> queryTask = PlayerDataStorageService.Instance.QueryFileList(EOSManager.Instance.GetProductUserId());
            yield return new WaitUntil(() => queryTask.IsCompleted);
            Assert.IsTrue(queryTask.IsCompletedSuccessfully, "Task did not complete successfully. Should not be an error, even if no entries are found.");
            Assert.NotNull(queryTask.Result, "Query task returned null, should never be null when successful.");
            Assert.AreEqual(numberOfFilesToMake, queryTask.Result.Count, $"Returned list is expected to contain exactly the number of created files, has {queryTask.Result.Count} entries.");

            foreach (string randomName in randomNames)
            {
                Assert.Contains(randomName, queryTask.Result, $"Name expected to be in the file list ('{randomName}') was not present.");
            }
        }

        /// <summary>
        /// Creates a file, uploads it, then downloads it.
        /// The contents are checked for being identical.
        /// </summary>
        [UnityTest]
        public IEnumerator UploadedFile_HasSameContents()
        {
            byte[] fileBytes = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5 };
            Task uploadTask = PlayerDataStorageService.Instance.UploadFileAsync(nameof(UploadedFile_HasSameContents), fileBytes);
            yield return new WaitUntil(() => uploadTask.IsCompleted);
            Assert.IsTrue(uploadTask.IsCompletedSuccessfully, "Upload task did not complete successfully.");

            Task<byte[]> downloadTask = PlayerDataStorageService.Instance.DownloadFileAsync(EOSManager.Instance.GetProductUserId(), nameof(UploadedFile_HasSameContents));
            yield return new WaitUntil(() => downloadTask.IsCompleted);
            Assert.IsTrue(downloadTask.IsCompletedSuccessfully, "Download task did not complete successfully.");
            Assert.NotNull(downloadTask.Result, "Downloaded file is null.");

            Assert.AreEqual(fileBytes.Length, downloadTask.Result.Length, "Downloaded file had a different length than the uploaded file.");

            for (int ii = 0; ii < fileBytes.Length; ii++)
            {
                Assert.AreEqual(fileBytes[ii], downloadTask.Result[ii], "Downloaded file has different contents than uploaded file.");
            }
        }
    }
}

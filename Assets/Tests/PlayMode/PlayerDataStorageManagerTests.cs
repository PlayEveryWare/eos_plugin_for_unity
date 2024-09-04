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

    public class PlayerDataStorageManagerTests : EOSTestBase
    {
        EOSPlayerDataStorageManager playerDataStorageManager;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            playerDataStorageManager = new EOSPlayerDataStorageManager();

            yield return DestroyAllFiles();
        }

        [UnitySetUp]
        public IEnumerator UnityTearDown()
        {
            yield return DestroyAllFiles();

            playerDataStorageManager.RemoveCallbacks();
        }

        private IEnumerator DestroyAllFiles()
        {
            // To know when the Query operation is done, subscribe to the file list being updated
            bool waiting = true;
            Action setWaiting = () => waiting = false;
            playerDataStorageManager.OnFileListUpdated += setWaiting;
            playerDataStorageManager.QueryFileList();

            yield return new WaitUntilDone(10f, () => waiting == false );

            Dictionary<string, string> localCache = playerDataStorageManager.GetLocallyCachedData();

            // The key is the file name; we need to delete all of these files
            foreach (string localCacheKey in localCache.Keys)
            {
                // To know when the DeleteFile operation is complete, we also listen to the file list being updated
                waiting = true;
                playerDataStorageManager.DeleteFile(localCacheKey);
                yield return new WaitUntilDone(10f, () => waiting == false);
            }

            // Unsubscribe from file list updates before exiting
            playerDataStorageManager.OnFileListUpdated -= setWaiting;
        }

        /// <summary>
        /// Assuming there are no files in storage for the test user,
        /// the user should be able to successfully query for items, even if there are none to be found.
        /// The resulting cached data should be empty.
        /// </summary>
        [UnityTest]
        public IEnumerator Query_NoResults()
        {
            bool waiting = true;
            Action setWaiting = () => waiting = false;
            playerDataStorageManager.OnFileListUpdated += setWaiting;
            playerDataStorageManager.QueryFileList();

            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = playerDataStorageManager.GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be an empty dictionary.");
            Assert.AreEqual(0, localCache.Keys.Count, "Local cache contains more than zero items, should be empty.");
        }

        /// <summary>
        /// This is used as a datasource for <see cref="UploadedFiles_CanBeQueried"/>.
        /// Determines how many files to make.
        /// </summary>
        static int[] ValueSource_NumberOfFilesToMakeFor_UploadedFiles_CanBeQueried = { 1, 3, 10 };

        /// <summary>
        /// Creates a file, uploads it, and then checks that it exists in the list.
        /// </summary>
        /// <param name="numberOfFilesToMake">
        /// How many files to create.
        /// This ensures that the manager can handle single file and multi file scenarios appropriately.
        /// </param>
        [UnityTest]
        public IEnumerator UploadedFiles_CanBeQueried([ValueSource(nameof(ValueSource_NumberOfFilesToMakeFor_UploadedFiles_CanBeQueried))] int numberOfFilesToMake)
        {
            HashSet<string> randomNames = new HashSet<string>();

            for (int fileIndex = 0; fileIndex < numberOfFilesToMake; fileIndex++)
            {
                // Generate a random name until we hit something that isn't already taken
                // Having the same name would work as an overwrite
                string randomName = "";
                do
                {
                    randomName = UnityEngine.Random.Range(0, 9999).ToString();
                } while (randomNames.Contains(randomName));

                bool waitingForAdd = true;
                Action doneWaiting = () => waitingForAdd = false;
                byte[] fileBytes = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5 };
                playerDataStorageManager.AddFile(randomName, Encoding.UTF8.GetString(fileBytes), doneWaiting);

                yield return new WaitUntilDone(10f, () => waitingForAdd == false);

                waitingForAdd = true;
                playerDataStorageManager.StartFileDataUpload(randomName, doneWaiting);

                yield return new WaitUntilDone(10f, () => waitingForAdd == false);
            }

            bool waiting = true;
            Action setWaiting = () => waiting = false;
            playerDataStorageManager.OnFileListUpdated += setWaiting;
            playerDataStorageManager.QueryFileList();

            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = playerDataStorageManager.GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be a dictionary with data.");
            Assert.AreEqual(numberOfFilesToMake, localCache.Keys.Count, "Local cache doesn't contain the expected number of items. Should have one item for each uploaded file.");

            foreach (string randomName in randomNames)
            {
                Assert.IsTrue(localCache.ContainsKey(randomName), $"Local cache doesn't contain a file with the randomly generated name {randomName}. Should have the identified file.");
            }
        }

        /// <summary>
        /// Creates and uploads a file, then tries to download it.
        /// The file that is downloaded should have the same contents as was used to create the file.
        /// </summary>
        [UnityTest]
        public IEnumerator UploadedFile_HasSameContents()
        {
            const int LengthOfUploadedBytes = 100;

            bool waiting = true;
            Action doneWaiting = () => waiting = false;
            byte[] uploadedFileBytes = new byte[LengthOfUploadedBytes];

            for (int byteIndex = 0; byteIndex < LengthOfUploadedBytes; byteIndex++)
            {
                uploadedFileBytes[byteIndex] = (byte)UnityEngine.Random.Range(0, byte.MaxValue);
            }

            playerDataStorageManager.AddFile(nameof(UploadedFile_HasSameContents), Encoding.UTF8.GetString(uploadedFileBytes), doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            waiting = true;
            playerDataStorageManager.StartFileDataUpload(nameof(UploadedFile_HasSameContents), doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            waiting = true;
            playerDataStorageManager.DownloadFile(nameof(UploadedFile_HasSameContents), doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = playerDataStorageManager.GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be a dictionary with data.");
            Assert.AreEqual(1, localCache.Keys.Count, "Local cache doesn't contain a single item. Should only contain exactly the one uploaded file.");
            Assert.IsTrue(localCache.ContainsKey(nameof(UploadedFile_HasSameContents)), "Local cache doesn't contain a file with the uploaded file's name.");

            string downloadedFileString = localCache[nameof(UploadedFile_HasSameContents)];

            Assert.IsFalse(string.IsNullOrEmpty(downloadedFileString), "Downloaded file's contents is null or empty, should contain data.");
            byte[] downloadedFileBytes = Encoding.UTF8.GetBytes(downloadedFileString);
            Assert.AreEqual(uploadedFileBytes.Length, downloadedFileBytes.Length, "Downloaded file's length is different than the uploaded file.");

            for (int byteIndex = 0; byteIndex < uploadedFileBytes.Length; byteIndex++)
            {
                Assert.AreEqual(uploadedFileBytes[byteIndex], downloadedFileBytes[byteIndex], "Downloaded file's contents are different than the uploaded file, should be identical.");
            }
        }
    }
}

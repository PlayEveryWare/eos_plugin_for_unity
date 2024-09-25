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
    using NUnit.Framework;
    using System.Collections;
    using UnityEngine.TestTools;
    using PlayEveryWare.EpicOnlineServices.Samples;
    using System;
    using System.Text;
    using System.Collections.Generic;

    public partial class PlayerDataStorageServiceTests
        : EOSTestBase
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return DestroyAllFiles();
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return DestroyAllFiles();
        }

        private IEnumerator DestroyAllFiles()
        {
            // To know when the Query operation is done, subscribe to the file list being updated
            bool waiting = true;
            Action setWaiting = () => waiting = false;
            PlayerDataStorageService.Instance.OnFileListUpdated += setWaiting;
            PlayerDataStorageService.Instance.QueryFileList();

            yield return new WaitUntilDone(10f, () => waiting == false );

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance.GetLocallyCachedData();

            // The key is the file name; we need to delete all of these files
            foreach (string localCacheKey in new List<string>(localCache.Keys))
            {
                // To know when the DeleteFile operation is complete, we also listen to the file list being updated
                waiting = true;
                PlayerDataStorageService.Instance.DeleteFile(localCacheKey);
                yield return new WaitUntilDone(10f, () => waiting == false);
            }

            // Unsubscribe from file list updates before exiting
            PlayerDataStorageService.Instance.OnFileListUpdated -= setWaiting;
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
            PlayerDataStorageService.Instance.OnFileListUpdated += setWaiting;
            PlayerDataStorageService.Instance.QueryFileList();

            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance.GetLocallyCachedData();

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
                string randomName = Guid.NewGuid().ToString();

                bool waitingForAdd = true;
                Action doneWaiting = () => waitingForAdd = false;
                byte[] fileBytes = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5 };
                PlayerDataStorageService.Instance.AddFile(randomName, Encoding.UTF8.GetString(fileBytes), doneWaiting);
                
                yield return new WaitUntilDone(10f, () => waitingForAdd == false);
            }
			
            bool waiting = true;
            Action setWaiting = () => waiting = false;
            PlayerDataStorageService.Instance.OnFileListUpdated += setWaiting;
            PlayerDataStorageService.Instance.QueryFileList();

            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance.GetLocallyCachedData();

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
            const int LengthOfUploadedBytes = byte.MaxValue;

            bool waiting = true;
            Action doneWaiting = () => waiting = false;
            string uploadedFileString = "";

            for (int byteIndex = 0; byteIndex < LengthOfUploadedBytes; byteIndex++)
            {
                uploadedFileString += UnityEngine.Random.Range(0, byte.MaxValue);
            }

            PlayerDataStorageService.Instance.AddFile(nameof(UploadedFile_HasSameContents), uploadedFileString, doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            waiting = true;
            PlayerDataStorageService.Instance.DownloadFile(nameof(UploadedFile_HasSameContents), doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance.GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be a dictionary with data.");
            Assert.AreEqual(1, localCache.Keys.Count, "Local cache doesn't contain a single item. Should only contain exactly the one uploaded file.");
            Assert.IsTrue(localCache.ContainsKey(nameof(UploadedFile_HasSameContents)), "Local cache doesn't contain a file with the uploaded file's name.");

            string downloadedFileString = localCache[nameof(UploadedFile_HasSameContents)];

            Assert.IsFalse(string.IsNullOrEmpty(downloadedFileString), "Downloaded file's contents is null or empty, should contain data.");
            Assert.AreEqual(uploadedFileString.Length, downloadedFileString.Length, "Downloaded file's length is different than the uploaded file.");

            for (int byteIndex = 0; byteIndex < uploadedFileString.Length; byteIndex++)
            {
                Assert.AreEqual(uploadedFileString[byteIndex], downloadedFileString[byteIndex], "Downloaded file's contents are different than the uploaded file, should be identical.");
            }
        }

        /// <summary>
        /// Creates a file, uploads it, then deletes it.
        /// Requeries all files. The file should not be present in the cache.
        /// </summary>
        [UnityTest]
        public IEnumerator UploadedFile_CanBeDeleted()
        {
            bool waiting = true;
            Action doneWaiting = () => waiting = false;
            byte[] fileBytes = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5 };
            PlayerDataStorageService.Instance.AddFile(nameof(UploadedFile_CanBeDeleted), Encoding.UTF8.GetString(fileBytes), doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            // We know the delete operation is complete when the file list is next updated
            waiting = true;
            PlayerDataStorageService.Instance.OnFileListUpdated += doneWaiting;
            PlayerDataStorageService.Instance.DeleteFile(nameof(UploadedFile_CanBeDeleted));
            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance.GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be an empty non-null dictionary.");
            Assert.AreEqual(0, localCache.Keys.Count, "Local cache contains items. It should be empty.");
        }

        /// <summary>
        /// Creates a file, and uploads it.
        /// Then after it is uploaded, copioes that file.
        /// When queried, the copied file should be identical to the uploaded file.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UploadedFile_CanBeCopied()
        {
            const string DestinationFileName = nameof(UploadedFile_CanBeCopied) + "copyfile";

            bool waiting = true;
            Action doneWaiting = () => waiting = false;
            byte[] fileBytes = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5 };
            PlayerDataStorageService.Instance.AddFile(nameof(UploadedFile_CanBeCopied), Encoding.UTF8.GetString(fileBytes), doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            // We know the copy operation is complete when the file list is next updated
            waiting = true;
            PlayerDataStorageService.Instance.OnFileListUpdated += doneWaiting;
            PlayerDataStorageService.Instance.CopyFile(nameof(UploadedFile_CanBeCopied), DestinationFileName);
            yield return new WaitUntilDone(10f, () => waiting == false);

            // Now download the file
            waiting = true;
            PlayerDataStorageService.Instance.DownloadFile(DestinationFileName, doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance.GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be a dictionary.");
            Assert.AreEqual(2, localCache.Keys.Count, "Local cache doesn't contain the expected amount of items. Should contain exactly the original file and its copy.");
            Assert.IsTrue(localCache.ContainsKey(nameof(UploadedFile_CanBeCopied)), "Local cache does not contain the original file.");
            Assert.IsTrue(localCache.ContainsKey(DestinationFileName), "Local cache does not contain the copy file.");

            string downloadedFileString = localCache[DestinationFileName];

            Assert.IsFalse(string.IsNullOrEmpty(downloadedFileString), "Downloaded copy file's contents is null or empty, should contain data.");
            byte[] downloadedFileBytes = Encoding.UTF8.GetBytes(downloadedFileString);
            Assert.AreEqual(fileBytes.Length, downloadedFileBytes.Length, "Downloaded copy file's length is different than the uploaded file.");

            for (int byteIndex = 0; byteIndex < fileBytes.Length; byteIndex++)
            {
                Assert.AreEqual(fileBytes[byteIndex], downloadedFileBytes[byteIndex], "Downloaded copy file's contents are different than the original uploaded file, should be identical.");
            }
        }

        /// <summary>
        /// Creates a file, uploads it, then creates another file, and uploads it with the same name.
        /// The resulting file is downloaded. It should have the contents of the overriding file.
        /// Implicitly checks for any errors resulting from trying to upload a file with the same name.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UploadingFile_WithSameName_Overrides()
        {
            const int lengthOfOriginalString = 10;
            const int lengthOfNewString = 20;

            bool waiting = true;
            Action doneWaiting = () => waiting = false;
            string originalFileString = "";

            for (int byteIndex = 0; byteIndex < lengthOfOriginalString; byteIndex++)
            {
                originalFileString += UnityEngine.Random.Range(0, byte.MaxValue);
            }

            PlayerDataStorageService.Instance.AddFile(nameof(UploadingFile_WithSameName_Overrides), originalFileString, doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            // Now create an identically named file but with different contents
            string newFileString = "";

            for (int byteIndex = 0; byteIndex < lengthOfNewString; byteIndex++)
            {
                newFileString += UnityEngine.Random.Range(0, byte.MaxValue);
            }

            PlayerDataStorageService.Instance.AddFile(nameof(UploadingFile_WithSameName_Overrides), newFileString, doneWaiting);
            yield return new WaitUntilDone(10f, () => waiting == false);

            Dictionary<string, string> localCache = PlayerDataStorageService.Instance   .GetLocallyCachedData();

            Assert.NotNull(localCache, "Local cache is null, should be a dictionary.");
            Assert.AreEqual(1, localCache.Keys.Count, "Local cache doesn't contain the expected amount of items. Should contain exactly the overriding file.");
            Assert.IsTrue(localCache.ContainsKey(nameof(UploadingFile_WithSameName_Overrides)), "Local cache does not contain the overriding file.");

            string downloadedFileString = localCache[nameof(UploadingFile_WithSameName_Overrides)];

            Assert.IsFalse(string.IsNullOrEmpty(downloadedFileString), "Downloaded file's contents is null or empty, should contain data.");
            Assert.AreEqual(newFileString.Length, downloadedFileString.Length, "Downloaded file's length is different than the overriding file.");

            for (int byteIndex = 0; byteIndex < newFileString.Length; byteIndex++)
            {
                Assert.AreEqual(newFileString[byteIndex], downloadedFileString[byteIndex], "Downloaded file's contents are different than the overriding file, should be identical.");
            }
        }
    }
}

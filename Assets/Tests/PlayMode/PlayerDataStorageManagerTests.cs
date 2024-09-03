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
    }
}

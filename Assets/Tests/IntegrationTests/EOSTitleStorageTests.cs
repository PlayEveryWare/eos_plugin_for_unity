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
    using Epic.OnlineServices.TitleStorage;
    using NUnit.Framework;
    using EpicOnlineServices;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.TestTools;

    /// <summary>
    /// Tests for the title storage functionality of EOS.
    /// </summary>
    public class EOSTitleStorageTests : EOSTestBase
    {
        private TitleStorageInterface _titleStorageHandle;

        /// <summary>
        /// Initialize the title storage interface in the beginning before running any tests related to title storage.
        /// </summary>
        [SetUp]
        public void SetupTitleStorage()
        {
            _titleStorageHandle = EOSManager.Instance.GetEOSPlatformInterface().GetTitleStorageInterface();
        }

        /// <summary>
        /// Queries the title storage with a tag that doesn't exist.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryListWithUnknownTag()
        {
            var listOptions = new QueryFileListOptions
            {
                ListOfTags = new Utf8String[] { "NonexistentTag" },
                LocalUserId = EOSManager.Instance.GetProductUserId(),
            };

            QueryFileListCallbackInfo? queryResult = null;
            _titleStorageHandle.QueryFileList(ref listOptions, null, (ref QueryFileListCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);

            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
                Assert.AreEqual(0, queryResult.Value.FileCount, "Somehow found files with the NonexistentTag.");
            }
        }

        /// <summary>
        /// Queries the title storage with known tags that should retrieve the default items.
        /// </summary>
        [UnityTest]
        [Category(TestCategories.SoloCategory)]
        public IEnumerator QueryListWithValidTags()
        {
            var listOptions = new QueryFileListOptions
            {
                ListOfTags = new Utf8String[] { "TXT", "Tag1" },
                LocalUserId = EOSManager.Instance.GetProductUserId(),
            };

            QueryFileListCallbackInfo? queryResult = null;
            _titleStorageHandle.QueryFileList(ref listOptions, null, (ref QueryFileListCallbackInfo data) => { queryResult = data; });

            yield return new WaitUntil(() => queryResult != null);

            if (queryResult != null)
            {
                Assert.AreEqual(Result.Success, queryResult.Value.ResultCode);
                Assert.AreEqual(3, queryResult.Value.FileCount, "There should be only 3 files in title storage.");
            }
        }
    }
}

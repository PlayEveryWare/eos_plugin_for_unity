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

namespace PlayEveryWare.EpicOnlineServices.Tests.Extensions
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using PlayEveryWare.EpicOnlineServices.Extensions;
    using System.Collections.Generic;

    [TestFixture]
    public class FileInfoExtensionsTests
    {
        private string tempDir;

        [SetUp]
        public void SetUp()
        {
            tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        private FileInfo CreateFile(string fileName, string content = null)
        {
            string filePath = Path.Combine(tempDir, fileName);
            if (content != null)
            {
                File.WriteAllText(filePath, content);
            }

            return new FileInfo(filePath);
        }

        private void AssertSemanticEquality(string content1, string content2, bool expected,
            bool createFirstFile = true, bool createSecondFile = true)
        {
            FileInfo fileInfo1 = createFirstFile ? CreateFile("file1.txt", content1) : CreateFile("nonexistent1.txt");
            FileInfo fileInfo2 = createSecondFile ? CreateFile("file2.txt", content2) : CreateFile("nonexistent2.txt");

            bool result = fileInfo1.AreContentsSemanticallyEqual(fileInfo2);
            Assert.AreEqual(expected, result,
                $"Expected files with content \"{content1}\" and \"{content2}\" to be semantically {(expected ? "equal" : "not equal")}.");
        }

        [Test]
        public void ComputeSHA_CorrectlyComputesSHAOfFile()
        {
            Dictionary<string, string> hashToContents = new()
            {
                { "0a0a9f2a6772942557ab5355d76af442f8f65e01", "Hello, World!" },
                // Test a string that is only slightly different
                { "907d14fb3af2b0d4f18c2d46abe8aedce17367bd", "Hello, World" },
                { "9be35056f6abcc721f8514f526d865f08fa88403", "So long, and thanks for all the fish!" }
            };

            foreach (string hash in hashToContents.Keys)
            {
                Assert.AreEqual(
                    hash,
                    CreateFile(hashToContents[hash], hashToContents[hash]).ComputeSHA(),
                    "The computed SHA should match the expected SHA."
                );
            }
        }

        [TestCase("Same content", "Same content", true)]
        [TestCase("Content A", "Content B", false)]
        [TestCase("Short", "Longer content", false)]
        [TestCase(null, null, true, false, false)] // Neither file exists
        [TestCase("Some content", null, false, true, false)] // One file exists, other does not
        [TestCase("Some content", "Some content", true, true, true)] // Same file path
        public void AreContentsSemanticallyEqual_VariousScenarios_ReturnsExpected(string content1, string content2,
            bool expected, bool createFirstFile = true, bool createSecondFile = true)
        {
            AssertSemanticEquality(content1, content2, expected, createFirstFile, createSecondFile);
        }
    }
}
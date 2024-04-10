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

namespace PlayEveryWare.EpicOnlineServices.Extensions
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class FileInfoExtensions
    {
        /// <summary>
        /// The number of bytes to read into two buffers respectively to evaluate the equivalency of two files.
        /// </summary>
        private const int ReadBytesBufferSize = sizeof(Int64);

        /// <summary>
        /// Calculate the SHA of a file.
        /// </summary>
        /// <param name="fileInfo">The file to compute the sha of.</param>
        /// <returns>A string SHA of the contents of the file.</returns>
        public static string ComputeSHA(this FileInfo fileInfo)
        {
            using SHA1 fileSHA = SHA1.Create();

            using var srcFileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

            srcFileStream.Position = 0;
            byte[] computedHash = fileSHA.ComputeHash(srcFileStream);

            StringBuilder formattedString = new(computedHash.Length * 2);
            foreach (byte b in computedHash)
            {
                formattedString.AppendFormat("{0:x2}", b);
            }

            return formattedString.ToString();
        }

        /// <summary>
        /// Determines whether the contents of the two files are semantically equivalent. Semantically meaning
        /// that the files are considered equal if neither file exists, and files are considered equal
        /// if a string comparison using StringComparison.OrdinalIgnoreCase between the two
        /// files filepaths indicates that the file paths are equal.
        /// </summary>
        /// <param name="fileInfo">The fileInfo object to "add" the extension method to.</param>
        /// <param name="other">The file to compare the file to.</param>
        /// <returns>True if the files are semantically equal, false otherwise.</returns>
        public static bool AreContentsSemanticallyEqual(this FileInfo fileInfo, FileInfo other)
        {
            // If one file exists, and the other does not, then they are not equal.
            if (fileInfo.Exists != other.Exists)
                return false;

            // For our purposes, if neither file exists, they are considered semantically equal
            if (!fileInfo.Exists && !other.Exists)
                return true;

            // If the size on disk for the two files is different, they are not considered equal
            if (fileInfo.Length != other.Length)
                return false;

            // If the path of both files is the same, by definition they must be equal
            if (string.Equals(fileInfo.FullName, other.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)fileInfo.Length / ReadBytesBufferSize);
            using FileStream fs1 = fileInfo.OpenRead();
            using FileStream fs2 = other.OpenRead();

            byte[] one = new byte[ReadBytesBufferSize];
            byte[] two = new byte[ReadBytesBufferSize];

            for (int i = 0; i < iterations; ++i)
            {
                fs1.Read(one, 0, ReadBytesBufferSize);
                fs2.Read(two, 0, ReadBytesBufferSize);

                // If (when converted to Int64 value) the byte arrays are not equal, then the files
                // cannot be equal.
                if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                    return false;
            }

            // If this code has been reached, then all tests have passed and the files can 
            // be considered semantically equal.
            return true;
        }
    }
}
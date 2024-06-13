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

// Comment or uncomment the following definition to turn on or off debug statements
// #define ENABLE_FILEINFO_EXTENSIONS_DEBUG

namespace PlayEveryWare.EpicOnlineServices.Extensions
{
    using System;
    using System.Diagnostics;
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

        [Conditional("ENABLE_FILEINFO_EXTENSIONS_DEBUG")]
        private static void LogInequalityReason(FileInfo one, FileInfo two, string reason)
        {
            StringBuilder sb = new();
            sb.AppendLine($"The following files were inequal because: {reason}");
            sb.AppendLine($"File #1: \"{one.FullName}\"");
            sb.AppendLine($"File #2: \"{two.FullName}\"");

            UnityEngine.Debug.Log(sb.ToString());
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
            {
                LogInequalityReason(fileInfo, other, "Because one of them exists, and the other does not.");
                return false;
            }

            // For our purposes, if neither file exists, they are considered semantically equal
            if (!fileInfo.Exists && !other.Exists)
                return true;

            // If the size on disk for the two files is different, they are not considered equal
            if (fileInfo.Length != other.Length)
            {
                LogInequalityReason(fileInfo, other, $"Size of file #1 is {fileInfo.Length}, and size of file #2 is {other.Length}.");
                return false;
            }

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
                int bytesReadF1 = fs1.Read(one, 0, ReadBytesBufferSize);
                int bytesReadF2 = fs2.Read(two, 0, ReadBytesBufferSize);

                // If the number of bytes _actually_ read from each file is 
                // different, then the files are not equal.
                if (bytesReadF1 != bytesReadF2)
                {
                    LogInequalityReason(fileInfo, other, $"An unequal number of bytes were read between byte index {iterations * ReadBytesBufferSize} and {iterations * ReadBytesBufferSize + ReadBytesBufferSize}.");
                    return false;
                }

                // If (when converted to Int64 value) the byte arrays are not equal, then the files
                // cannot be equal.
                if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                {
                    LogInequalityReason(fileInfo, other, $"An inequality was found between byte index {iterations * ReadBytesBufferSize} and {iterations * ReadBytesBufferSize + ReadBytesBufferSize}.");
                    return false;
                }
            }

            // If this code has been reached, then all tests have passed and the files can 
            // be considered semantically equal.
            return true;
        }
    }
}
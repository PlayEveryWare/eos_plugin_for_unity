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

namespace PlayEveryWare.EpicOnlineServices.Utility.Extensions
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class FileInfoExtensions
    {
        private const int ReadBufferSize = sizeof(Int64);

        /// <summary>
        /// Reads the contents of the given file into memory as a string, and replaces all DOS line endings with Unix line endings.
        /// </summary>
        /// <param name="fileInfo">The file to convert the line endings of.</param>
        public static void ConvertDosToUnixLineEndings(this FileInfo fileInfo)
        {
            string originalContents = File.ReadAllText(fileInfo.FullName);
            string unixLineEndings = originalContents.Replace("\r\n", "\n");
            File.WriteAllText(fileInfo.FullName, unixLineEndings);
        }

        /// <summary>
        /// Determines if two files are semantically equivalent. The distinction between being truly equal and being
        /// semantically equal is that if neither file exists, they are semantically equivalent. This function will
        /// check a number of pre-conditions before reading each file a certain number of bytes at a time, stopping
        /// at the first occurrence of a difference. Semantic equality will only be determined after every byte of
        /// each file has been read and compared to the corresponding byte of the other file.
        /// </summary>
        /// <param name="first">The first file to consider.</param>
        /// <param name="second">The second file to consider.</param>
        /// <returns>True if the files are semantically equivalent, false otherwise.</returns>
        public static bool AreSemanticallyEqual(this FileInfo first, FileInfo second)
        {
            // If one of the files exists, and the other does not, then they cannot be equal.
            if (first.Exists != second.Exists)
                return false;

            // If neither file exists, then (semantically) they are considered to be equal.
            if (!first.Exists && !second.Exists)
                return true;

            // If the size on disk of each file is not the same, then they cannot be equal.
            if (first.Length != second.Length)
                return false;

            // If an ordinal, case ignorant comparison of the fully-qualified paths for each file are
            // equal, then the two instances of FileInfo reference the same file, so they must be equal.
            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Determine how many iterations are required to read the contents of each file
            // "ReadBufferSize" bytes at a time.
            int iterations = (int)Math.Ceiling((double)first.Length / ReadBufferSize);

            // Open filestreams for both files.
            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                // Create byte buffer arrays to store the read contents of each file.
                byte[] one = new byte[ReadBufferSize];
                byte[] two = new byte[ReadBufferSize];

                for (int i = 0; i < iterations; i++)
                {
                    // Read from each file, and record the number of bytes that were actually read
                    // (because the number of bytes read may be less than the number of bytes *requested*
                    // to be read.
                    int fs1ReadSize = fs1.Read(one, 0, ReadBufferSize);
                    int fs2ReadSize = fs2.Read(two, 0, ReadBufferSize);

                    // If a different number of bytes was read from either stream, then the files
                    // are not equal. 
                    if (fs1ReadSize != fs2ReadSize)
                        return false;

                    // Once read, if the byte arrays (when converted to Int64) have different values, 
                    // then the files cannot be equal.
                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            // If all tests have passed, then the files are considered to be equal.
            return true;
        }

        /// <summary>
        /// Generate a SHA1 string for the file's contents.
        /// </summary>
        /// <param name="fileInfo">The FileInfo for the file to calculate the SHA1 of.</param>
        /// <returns>A string representation of the SHA1 calculated for the indicated file.</returns>
        public static string CalculateSHA1(this FileInfo fileInfo)
        {
            // to store the computed SHA1 in.
            string computedSHA = "";

            // Create a new SHA1
            using (SHA1 fileSHA = SHA1.Create())
            {
                // to store the computed hash
                byte[] computedHash = null;

                // Open the file
                using (var srcFileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // set the position to zero
                    srcFileStream.Position = 0;
                    
                    // compute the hash.
                    computedHash = fileSHA.ComputeHash(srcFileStream);
                }

                // The formatted string will be twice as long as the hash itself.
                StringBuilder formattedStr = new(computedHash.Length * 2);

                // Generate the string representation of the SHA1
                foreach (byte b in computedHash)
                {
                    formattedStr.AppendFormat("{0:x2}", b);
                }
                computedSHA = formattedStr.ToString();
            }

            return computedSHA;
        }
    }
}
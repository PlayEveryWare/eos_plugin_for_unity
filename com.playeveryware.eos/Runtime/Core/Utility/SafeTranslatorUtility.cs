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

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    /// <summary>
    /// Used to safely convert between signed and unsigned values.
    /// </summary>
    public static class SafeTranslatorUtility
    {
        /// <summary>
        /// Try to convert an int value to a uint.
        /// </summary>
        /// <param name="value">The int value to convert.</param>
        /// <param name="output">The uint equivalent to the int provided</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        public static bool TryConvert(int value, out uint output)
        {
            output = unchecked((uint)value);
            return value >= 0;
        }

        /// <summary>
        /// Try to convert uint value to an int.
        /// </summary>
        /// <param name="value">The uint value to convert.</param>
        /// <param name="output">The int equivalent.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        public static bool TryConvert(uint value, out int output)
        {
            output = unchecked((int)value);
            return value <= int.MaxValue;
        }

        /// <summary>
        /// Try to convert ulong to long.
        /// </summary>
        /// <param name="value">The ulong value to convert.</param>
        /// <param name="output">The long equivalent.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        public static bool TryConvert(ulong value, out long output)
        {
            output = unchecked((long)value);
            return value <= long.MaxValue;
        }

        /// <summary>
        /// Safely convert from a long value to a ulong value.
        /// </summary>
        /// <param name="value">The long value to convert.</param>
        /// <param name="output">The ulong equivalent.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        public static bool TryConvert(long value, out ulong output)
        {
            output = unchecked((ulong)value);
            return value >= 0;
        }
    }
}
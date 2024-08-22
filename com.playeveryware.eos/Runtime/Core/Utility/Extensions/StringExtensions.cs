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

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices.Extensions
{
    using Epic.OnlineServices.Platform;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods to assist in the interaction of PlatformFlag
    /// operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// String override to simplify the conversion from string to ulong, and
        /// falling back to an optional default value if the parsing fails.
        /// </summary>
        /// <param name="value">The string to parse into a ulong.</param>
        /// <param name="defaultValue">
        /// The default value to assign if the parsing fails.
        /// </param>
        /// <returns>
        /// ulong value resulting from parsing the string value.
        /// </returns>
        public static ulong ToUInt64(this string value, ulong defaultValue = 0L)
        {
            if (!ulong.TryParse(value, out ulong longValue))
            {
                longValue = defaultValue;
            }

            return longValue;
        }
    }
}
#endif
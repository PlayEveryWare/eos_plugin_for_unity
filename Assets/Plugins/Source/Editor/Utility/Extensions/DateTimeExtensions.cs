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

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Try to determine if seconds have elapsed. If they have, reset the datetime value to now, and return true.
        /// </summary>
        /// <param name="datetime">DateTime object to use as timer.</param>
        /// <param name="seconds">Seconds per lap.</param>
        /// <returns>True if seconds have elapsed since the indicated DateTime, false otherwise.</returns>
        public static bool TryLap(ref this DateTime datetime, double seconds)
        {
            return datetime.TryLap(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Determine if timespan has elapsed. If it has, reset the DateTime value to now, and return true.
        /// </summary>
        /// <param name="datetime">DateTime object to use as timer.</param>
        /// <param name="lapTime">The TimeSpan per lap.</param>
        /// <returns>True if TimeSpan has elapsed since the indicated DateTime, false otherwise.</returns>
        public static bool TryLap(ref this DateTime datetime, TimeSpan lapTime)
        {
            if (DateTime.Now - datetime >= lapTime)
            {
                return false;
            }

            datetime = DateTime.Now;
            return true;
        }
    }
}
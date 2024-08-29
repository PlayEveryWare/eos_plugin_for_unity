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
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Contains extension methods for lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Threadsafe Random class.
        /// </summary>
        private static class ThreadSafeRandom
        {
            private static readonly ThreadLocal<Random> threadLocalRandom = new(() => new Random());

            public static Random Instance => threadLocalRandom.Value;
        }

        /// <summary>
        /// Uses the Fisher-Yates algorithm to efficiently and uniformly shuffle
        /// the contents of an object that implements the generic list
        /// interface.
        /// </summary>
        /// <typeparam name="T">The template parameter for the list.</typeparam>
        /// <param name="list">List to shuffle the contents of.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                // Lock to ensure things work as expected in threaded
                // circumstances.
                lock (ThreadSafeRandom.Instance)
                {
                    // Get the next random index to shuffle
                    int j = ThreadSafeRandom.Instance.Next(i, n);
                    // Use deconstruction syntax to perform the swap.
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
        }
    }
}
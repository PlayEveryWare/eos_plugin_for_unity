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
    /// Provides extension methods to classes that implement the IList generic interface.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// This creates a thread-safe static instance of a random number generator,
        /// so that shuffling in multi-threaded scenarios works in a consistent manner.
        /// Each time the value of this is read, a different instance of Random is returned,
        /// one that is both static, and safe to use in the thread that is reading the value.
        /// </summary>
        private static readonly ThreadLocal<Random> s_threadLocalRandom =
            new(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// Shuffles a list using the Fisher-Yates shuffle algorithm; which is much faster
        /// than generating a Guid for each item, then sorting by Guid.
        /// </summary>
        /// <typeparam name="T">Type of the list.</typeparam>
        /// <param name="list">The list to shuffle the contents of.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            Random rng = s_threadLocalRandom.Value;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
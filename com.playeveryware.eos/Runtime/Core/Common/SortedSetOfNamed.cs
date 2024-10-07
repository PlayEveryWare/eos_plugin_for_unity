/*
 * Copyright (c) 2021 PlayEveryWare
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

namespace PlayEveryWare.Common
{
    using System.Collections.Generic;
    using System;

    public class SortedSetOfNamed<T> : SortedSet<Named<T>> where T : IEquatable<T>
    {
        private readonly string _defaultNamePattern;

        public SortedSetOfNamed(string defaultNamePattern = "NamedItem")
        {
            _defaultNamePattern = defaultNamePattern;
        }

        public bool Add(T value)
        {
            string newItemName = _defaultNamePattern;
            int increment = 0;
            while (ContainsName(newItemName))
            {
                increment++;
                newItemName = $"{_defaultNamePattern} {increment}";
            }

            return Add(newItemName, value);
        }


        public bool Add(string name, T value)
        {
            return !ContainsName(name) && base.Add(Named<T>.FromValue(value, name));
        }

        public bool ContainsName(string name)
        {
            foreach (Named<T> item in this)
            {
                if (item.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(T item)
        {
            Named<T> temp = Named<T>.FromValue(item, "temp");
            return Contains(temp);
        }
    }

}
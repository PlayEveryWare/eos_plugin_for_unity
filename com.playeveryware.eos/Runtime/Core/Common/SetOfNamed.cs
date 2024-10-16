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

namespace PlayEveryWare.Common
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Class that stores a unique set of values, where each value has a name
    /// associated to it. Items are sorted by the name. No two items of the same
    /// value can be added (regardless of if they have different names). Also,
    /// no two items can have the same name (regardless of if they have
    /// different values).
    /// </summary>
    /// <typeparam name="T">The type being wrapped with a name.</typeparam>
    public class SetOfNamed<T> : HashSet<Named<T>> where T : IEquatable<T>
    {
        /// <summary>
        /// When items without a name are added to the SortedSet, this is the
        /// default name to give to the newly added item. When an item with the
        /// default name already exists in the set, then the default name is
        /// appended with an increasing number until the resulting name does
        /// not exist in the collection.
        /// </summary>
        private readonly string _defaultNamePattern;

        /// <summary>
        /// Creates a new SortedSetOfNamed.
        /// </summary>
        /// <param name="defaultNamePattern">
        /// The default name pattern to apply to items being added to the set.
        /// </param>
        public SetOfNamed(string defaultNamePattern = "NamedItem")
        {
            _defaultNamePattern = defaultNamePattern;
        }

        /// <summary>
        /// Adds an item to the sorted set with a default name.
        /// </summary>
        /// <param name="value">
        /// The value to add to the SortedSet.
        /// </param>
        /// <returns>
        /// True if the item was able to be added, false otherwise.
        /// </returns>
        public bool Add(T value)
        {
            // Determines the 
            string newItemName = GetNewItemName();

            return Add(newItemName, value);
        }

        /// <summary>
        /// Returns the name of a new item to be added to the collection.
        /// </summary>
        /// <returns>The name to give to the added set.</returns>
        private string GetNewItemName()
        {
            string newItemName = _defaultNamePattern;
            int increment = 0;
            while (ContainsName(newItemName))
            {
                increment++;
                newItemName = $"{_defaultNamePattern} ({increment})";
            }

            return newItemName;
        }

        /// <summary>
        /// Adds a new default value to the sorted set. This can usually only
        /// be done once, until the added item has it's value and/or it's name
        /// changed.
        /// </summary>
        /// <returns>
        /// True if the add was successful, false otherwise.
        /// </returns>
        public bool Add()
        {
            return Add(default);
        }

        /// <summary>
        /// Adds a new item with the indicated name to the SortedSetOfNamed
        /// items.
        /// </summary>
        /// <param name="name">
        /// The name of the item to add to the collection.
        /// </param>
        /// <param name="value">
        /// The value to add to the collection.
        /// </param>
        /// <returns>
        /// True if the add was successful, false otherwise.
        /// </returns>
        public bool Add(string name, T value)
        {
            // Add to the collection using the base implementation, so long as
            // the name for the new item is not used for another named item 
            // already in the collection, and so long as the value is not the
            // same as the value of another item in the collection.
            return !ContainsName(name) &&
                   !Contains(value) &&
                   base.Add(Named<T>.FromValue(value, name));
        }

        /// <summary>
        /// Determines whether there is an item in the collection with the
        /// indicated name.
        /// </summary>
        /// <param name="name">
        /// The name to check.
        /// </param>
        /// <returns>
        /// True if there is an item in the collection with the given name,
        /// false otherwise.
        /// </returns>
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

        /// <summary>
        /// Determines whether there is an item in the collection with the
        /// indicated value.
        /// </summary>
        /// <param name="item">
        /// The value to check.
        /// </param>
        /// <returns>
        /// True if there is an item in the collection with the indicated value,
        /// false otherwise.
        /// </returns>
        public bool Contains(T item)
        {
            foreach (Named<T> namedItemInSet in this)
            {
                if (namedItemInSet.Value.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }
    }

}
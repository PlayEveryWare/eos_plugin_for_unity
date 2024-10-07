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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Used to associate a name with a particular type of value.
    /// </summary>
    public class Named<T> : IEquatable<Named<T>>, IComparable<Named<T>>, IEquatable<T> where T : IEquatable<T>
    {
        /// <summary>
        /// The name of the value (typically used for things like UI labels)
        /// </summary>
        public string Name;

        /// <summary>
        /// The value itself.
        /// </summary>
        public T Value;

        /// <summary>
        /// Creates a Named object from the given value and with a given name.
        /// </summary>
        /// <param name="value">
        /// The value to give a name to.
        /// </param>
        /// <param name="name">
        /// The name to associate with the value.
        /// </param>
        /// <returns>
        /// A Named object that associates a name with a value.
        /// </returns>
        public static Named<T> FromValue(T value, string name)
        {
            return new Named<T>() { Name = name, Value = value };
        }

        /// <summary>
        /// Compares one named value to another, for sorting purposes.
        /// </summary>
        /// <param name="other">
        /// The other named object (must be of same type).
        /// </param>
        /// <returns>
        /// Result of a comparison between the names of each Named object.
        /// </returns>
        public int CompareTo(Named<T> other)
        {
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public bool Equals(T other)
        {
            if (Value == null && other == null)
                return true;

            if (Value == null)
                return false;

            return Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is Named<T> other && Equals(other);
        }

        public bool Equals(Named<T> other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }

        public override string ToString()
        {
            return $"{Name} : {Value}";
        }
    }
}
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
    using System;

    /// <summary>
    /// Class that assists in wrapping a struct in such a way that it is both a
    /// reference type and enforces the overriding of the Equals functionality
    /// for that struct. This is useful for circumstances where a custom
    /// equality operator needs to be implemented for a type that is outside of
    /// the assembly or otherwise not editable.
    /// </summary>
    /// <typeparam name="T">
    /// The struct type to wrap.
    /// </typeparam>
    public abstract class WrappedStruct<T> : IEquatable<WrappedStruct<T>> where T : struct
    {
        /// <summary>
        /// The value of the struct being wrapped.
        /// </summary>
        protected T _value;

        /// <summary>
        /// Abstract base constructor that takes a struct value to wrap.
        /// </summary>
        /// <param name="value">
        /// The value to wrap in this instance.
        /// </param>
        protected WrappedStruct(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Overrides the Equals function, by casting the given object to the
        /// baser abstract type if possible, and executing the Equality function
        /// that the implementing class is required to implement.
        /// </summary>
        /// <param name="obj">
        /// The object to compare equality for.
        /// </param>
        /// <returns>
        /// True if object is equal to the given instance.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is WrappedStruct<T> wrappedStruct && Equals(wrappedStruct);
        }

        /// <summary>
        /// Overrides the GetHashCode function in keeping with best practices
        /// when overriding the Equals function.
        /// </summary>
        /// <returns>
        /// A hash code value for the wrapped struct.
        /// </returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// This method must be implemented by deriving classes to define the
        /// implementation to determine equality between two struct values.
        /// </summary>
        /// <param name="other">The other wrapped struct.</param>
        /// <returns>
        /// True if the instance is equal to the other, false otherwise.
        /// </returns>
        public abstract bool Equals(WrappedStruct<T> other);

        /// <summary>
        /// Returns the value that is wrapped in the reference type.
        /// </summary>
        /// <returns>
        /// The value of the struct that is being wrapped by this instance.
        /// </returns>
        public T Unwrap()
        {
            return _value;
        }
    }
}
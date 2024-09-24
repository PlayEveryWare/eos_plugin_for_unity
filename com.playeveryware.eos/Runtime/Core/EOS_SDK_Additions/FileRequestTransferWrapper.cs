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
 *
 */

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices;
    using System;

    /// <summary>
    /// Base implementation for file transfer wrapper classes, used to wrap the
    /// various file transfer request handles that the EOS SDK provides.
    /// </summary>
    /// <typeparam name="T">
    /// Interface defining what functionality a wrapper should define.
    /// </typeparam>
    public abstract class FileRequestTransferWrapper<T> : IFileTransferRequest
        where T : class
    {
        /// <summary>
        /// Wrapped instance.
        /// </summary>
        protected T _instance;

        /// <summary>
        /// Used to prevent redundant calls to the dispose method.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of the FileRequestTransferWrapper.
        /// </summary>
        /// <param name="instance">
        /// The class being wrapped.
        /// </param>
        protected FileRequestTransferWrapper(T instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Destructor is defined to catch cases where Dispose was not properly
        /// called before the service ceases to exist.
        /// </summary>
        ~FileRequestTransferWrapper()
        {
            Dispose();
        }

        #region Equality operator overloads

        /*
         * The following section contains methods that are used to seemlessly
         * provide wrapper functionality for the classes that this class wraps,
         * allowing the wrapping to be transparent when assigning values to
         * instances of the wrapper, or when comparing the instance to null (for
         * instance in scenarios where the instance being wrapped is null, but
         * the wrapper is not null - in those cases we still want to say it's
         * equal to null to be as transparent of a wrapper as possible).
         */

        public static bool operator ==(FileRequestTransferWrapper<T> wrapper, object obj)
        {
            if (wrapper is null || wrapper._instance is null)
            {
                return obj is null;
            }

            return wrapper._instance.Equals(obj);
        }

        public static bool operator !=(FileRequestTransferWrapper<T> wrapper, object obj)
        {
            return !(wrapper == obj);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(_instance, obj)) return true;

            if (obj is FileRequestTransferWrapper<T> otherWrapper)
            {
                return Equals(_instance, otherWrapper._instance);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _instance != null ? _instance.GetHashCode() : 0;
        }

        #endregion

        /// <summary>
        /// Used to call the CancelRequest function on the wrapped instance.
        /// </summary>
        /// <returns>
        /// The result of canceling the wrapped request.
        /// </returns>
        public abstract Result CancelRequest();

        /// <summary>
        /// Implement this in derived classes to release the instance variable.
        /// </summary>
        public abstract void Release();

        public void Dispose()
        {
            Dispose(true);
            // Prevent collection until the dispose pattern is followed
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern. If this method is
        /// overridden in implementing classes, this base implementation should
        /// be called at the end of the overridden implementation.
        /// </summary>
        /// <param name="disposing">
        /// Indicates that disposing is taking place.
        /// </param>
        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Release();
                _instance = null;
            }

            _disposed = true;
        }
    }
}

#endif
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

using System.Runtime.ConstrainedExecution;
using System;

namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    /// <summary>
    /// A Generic Safe Handle. To use it, inherit from GenericSafeHandle with the HandleType being the type of the handle.
    /// Then, override the ReleaseHandleMethod in the derived class
    /// </summary>
    /// <typeparam name="HandleType"></typeparam>        
    public abstract class GenericSafeHandle<HandleType> : CriticalFinalizerObject, IDisposable
    {
        protected HandleType handleObject;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public GenericSafeHandle(HandleType handle)
        {
            handleObject = handle;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Dispose(false);
        }

        protected abstract void ReleaseHandle();
        public abstract bool IsValid();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ReleaseHandle();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~GenericSafeHandle()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

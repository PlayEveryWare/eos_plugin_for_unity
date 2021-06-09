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

using System;

namespace Veldrid.SampleGallery
{
    public abstract class DisposeBase : IDisposable
    {
        public bool IsDisposed { get; private set; } = false;

        ~DisposeBase()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (!IsDisposed)
            {
                IsDisposed = true;
                DisposeCore();
            }
        }

        protected virtual void DisposeCore() { }
    }
}

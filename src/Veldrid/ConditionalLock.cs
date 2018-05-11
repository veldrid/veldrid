using System;
using System.Threading;

namespace Veldrid
{
    internal class ConditionalLock
    {
        public DisposableLockEntry Lock(bool enabled) => new DisposableLockEntry(enabled, this);

        public struct DisposableLockEntry : IDisposable
        {
            private readonly object _lock;
            private bool _lockTaken;

            public DisposableLockEntry(bool enabled, object lockObject)
            {
                _lockTaken = false;
                _lock = lockObject;

                if (enabled)
                {
                    Monitor.Enter(_lock, ref _lockTaken);
                }
            }

            public void Dispose()
            {
                if (_lockTaken)
                {
                    Monitor.Exit(_lock);
                }
            }
        }
    }
}

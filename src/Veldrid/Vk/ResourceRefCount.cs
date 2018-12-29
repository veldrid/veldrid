using System;
using System.Threading;

namespace Veldrid.Vk
{
    internal class ResourceRefCount
    {
        private readonly Action _disposeAction;
        private int _refCount;

        public ResourceRefCount(Action disposeAction)
        {
            _disposeAction = disposeAction;
            _refCount = 1;
        }

        public void Increment()
        {
            Interlocked.Increment(ref _refCount);
        }

        public void Decrement()
        {
            if (Interlocked.Increment(ref _refCount) <= 0)
            {
                _disposeAction();
            }
        }
    }
}
using System;
using System.Collections.Generic;
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

        public int Increment()
        {
            int ret = Interlocked.Increment(ref _refCount);
#if VALIDATE_USAGE
            if (ret == 0)
            {
                throw new VeldridException("An attempt was made to reference a disposed resource.");
            }
#endif
            return ret;
        }

        public int Decrement()
        {
            int ret = Interlocked.Decrement(ref _refCount);
            if (ret == 0)
            {
                _disposeAction();
            }

            return ret;
        }

        public class EqualityComparer : IEqualityComparer<ResourceRefCount>
        {
            public bool Equals(ResourceRefCount x, ResourceRefCount y)
            {
                if (x != null && y != null)
                {
                    return x.Equals(y);
                }

                return false;
            }

            public int GetHashCode(ResourceRefCount obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return obj.GetHashCode ();
            }
        }
    }
}

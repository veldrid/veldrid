using System.Diagnostics;
using System.Threading;

namespace Veldrid.Vulkan
{
    [DebuggerDisplay($"ResourceRefCount ({{{nameof(_target)},nq}})")]
    internal class ResourceRefCount
    {
        private readonly IResourceRefCountTarget _target;
        private int _refCount;

        public ResourceRefCount(IResourceRefCountTarget target)
        {
            _target = target;
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
                _target.RefZeroed();
            }

            return ret;
        }
    }
}

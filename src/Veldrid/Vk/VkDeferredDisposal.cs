using System.Threading;

namespace Veldrid.Vk
{
    internal interface VkDeferredDisposal
    {
        ReferenceTracker ReferenceTracker { get; }
        void DestroyResources();
    }

    internal class ReferenceTracker
    {
        private int _count;

        public int ReferenceCount => _count;

        public int Increment()
        {
            return Interlocked.Increment(ref _count);
        }

        public int Decrement()
        {
            return Interlocked.Decrement(ref _count);
        }
    }
}

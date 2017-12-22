using System;
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
            int result = Interlocked.Decrement(ref _count);
#if DEBUG
            if (result < 0)
            {
                System.Diagnostics.Debug.Fail("Reference count fell below 0.");
            }
#endif
            if (result == 0 && _decrementedToZero != null)
            {
                _decrementedToZero();
            }
            return result;
        }

        private Action _decrementedToZero;

        public event Action DecrementedToZero
        {
            add
            {
                _decrementedToZero += value;
                if (ReferenceCount == 0)
                {
                    _decrementedToZero();
                }
            }
            remove
            {
                _decrementedToZero -= value;
            }
        }
    }
}

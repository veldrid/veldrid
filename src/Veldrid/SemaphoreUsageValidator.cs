#if VALIDATE_USAGE
using System.Collections.Generic;

namespace Veldrid
{
    internal class SemaphoreUsageValidator
    {
        private readonly object _lock = new object();
        private readonly HashSet<Semaphore> _submittedSemaphores = new HashSet<Semaphore>();

        public void Signaled(Semaphore[] semaphores)
        {
            lock (_lock)
            {
                foreach (Semaphore s in semaphores)
                {
                    SignalCore(s);
                }
            }
        }

        public void Signaled(Semaphore s)
        {
            lock (_lock)
            {
                SignalCore(s);
            }
        }

        private void SignalCore(Semaphore s)
        {
            if (!_submittedSemaphores.Add(s))
            {
                throw new VeldridException(
                    $"A CommandList was submitted which signals a {nameof(Semaphore)} ({s}) which has been previously signaled, but was never waited on.");
            }
        }

        public void Waited(Semaphore[] semaphores)
        {
            lock (_lock)
            {
                foreach (Semaphore s in semaphores)
                {
                    WaitCore(s);
                }
            }
        }

        public void Waited(Semaphore s)
        {
            lock (_lock)
            {
                WaitCore(s);
            }
        }

        private void WaitCore(Semaphore s)
        {
            if (!_submittedSemaphores.Remove(s))
            {
                throw new VeldridException(
                    $"A CommandList was submitted which waits on a {nameof(Semaphore)} ({s}) which will not be signaled by any current work submissions.");
            }
        }
    }
}
#endif
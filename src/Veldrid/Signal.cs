using System;

namespace Veldrid
{
    // A GPU-GPU sync point
    public abstract class Semaphore : DeviceResource, IDisposable
    {
        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        public abstract void Dispose();
    }

    // A GPU-CPU sync point
    public abstract class Fence : DeviceResource, IDisposable
    {
        public abstract bool Signaled { get; }

        public abstract void Reset();

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        public abstract void Dispose();
    }
}

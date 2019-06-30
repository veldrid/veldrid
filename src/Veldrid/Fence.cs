using System;

namespace Veldrid
{
    // A GPU-CPU sync point
    /// <summary>
    /// A synchronization primitive which allows the GPU to communicate when submitted work items have finished executing.
    /// </summary>
    public abstract class Fence : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;

        internal Fence(GraphicsDevice gd)
        {
            _gd = gd;
        }

        /// <summary>
        /// Gets a value indicating whether the Fence is currently signaled. A Fence is signaled after a CommandList finishes
        /// execution after it was submitted with a Fence instance.
        /// </summary>
        public abstract bool Signaled { get; }

        /// <summary>
        /// Sets this instance to the unsignaled state.
        /// </summary>
        public abstract void Reset();

        public void Wait() => _gd.WaitForFence(this);
        public void Wait(TimeSpan timeout) => _gd.WaitForFence(this, timeout);

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}

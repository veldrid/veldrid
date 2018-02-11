using System;

namespace Veldrid
{
    public abstract class Swapchain : DeviceResource, IDisposable
    {
        public abstract Framebuffer Framebuffer { get; }
        public abstract void Resize(uint width, uint height);
        public abstract bool SyncToVerticalBlank { get; set; }

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

using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary graphics data in various formats.
    /// The size of a <see cref="Buffer"/> is fixed upon creation, and resizing is not possible.
    /// See <see cref="BufferDescription"/>.
    /// </summary>
    public abstract class Buffer : DeviceResource, BindableResource, IDisposable
    {
        /// <summary>
        /// The total capacity, in bytes, of the buffer. This value is fixed upon creation.
        /// </summary>
        public abstract ulong SizeInBytes { get; }

        /// <summary>
        /// The <see cref="BufferUsage"/> flags this instance was created with. Limits how the Buffer is permitted to be used.
        /// </summary>
        public abstract BufferUsage Usage { get; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}

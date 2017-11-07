using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary graphics data in various formats.
    /// The size of a <see cref="Buffer"/> is fixed upon creation, and resizing is not possible.
    /// </summary>
    public interface Buffer : DeviceResource, IDisposable
    {
        /// <summary>
        /// The total capacity, in bytes, of the buffer. This value is fixed upon creation.
        /// </summary>
        ulong SizeInBytes { get; }
    }
}

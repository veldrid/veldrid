using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A <see cref="DeviceBuffer"/> storing index information.
    /// </summary>
    public interface IndexBuffer: DeviceBuffer, IDisposable
    {
        /// <summary>
        /// Stores the given index data into the device-side buffer.
        /// </summary>
        /// <param name="indices">The index data.</param>
        void SetIndices(uint[] indices);
        /// <summary>
        /// Stores the given index data into the device-side buffer.
        /// </summary>
        /// <param name="indices">The index data.</param>
        /// <param name="stride">The stride of the data.</param>
        /// <param name="elementOffset">The number of elements to skip in the destination buffer.</param>
        void SetIndices(uint[] indices, int stride, int elementOffset);

        /// <summary>
        /// Stores the given index data into the device-side buffer.
        /// </summary>
        /// <param name="indices">The index data.</param>
        void SetIndices(ushort[] indices);
        /// <summary>
        /// Stores the given index data into the device-side buffer.
        /// </summary>
        /// <param name="indices">The index data.</param>
        /// <param name="stride">The stride of the data.</param>
        /// <param name="elementOffset">The number of elements to skip in the destination buffer.</param>
        void SetIndices(ushort[] indices, int stride, int elementOffset);

        /// <summary>
        /// Stores the given index data into the device-side buffer.
        /// </summary>
        /// <param name="indices">The index data.</param>
        /// <param name="format">The format of the index data.</param>
        /// <param name="elementSizeInBytes">The size of individual elements, in bytes.</param>
        /// <param name="count">The number of elements to store in the buffer.</param>
        void SetIndices(IntPtr indices, IndexFormat format, int count);
        /// <summary>
        /// Stores the given index data into the device-side buffer.
        /// </summary>
        /// <param name="indices">The index data.</param>
        /// <param name="format">The format of the index data.</param>
        /// <param name="elementSizeInBytes">The size of individual elements, in bytes.</param>
        /// <param name="count">The number of elements to store in the buffer.</param>
        /// <param name="elementOffset">The number of elements to skip in the destination buffer.</param>
        void SetIndices(IntPtr indices, IndexFormat format, int count, int elementOffset);
    }
}
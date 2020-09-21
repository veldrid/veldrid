using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary graphics data in various formats.
    /// The size of a <see cref="DeviceBuffer"/> is fixed upon creation, and resizing is not possible.
    /// See <see cref="BufferDescription"/>.
    /// </summary>
    public abstract class DeviceBuffer : DeviceResource, BindableResource, MappableResource, IDisposable
    {
        private readonly GraphicsDevice _gd;

        public DeviceBuffer(GraphicsDevice gd)
        {
            _gd = gd;
        }

        /// <summary>
        /// The total capacity, in bytes, of the buffer. This value is fixed upon creation.
        /// </summary>
        public abstract uint SizeInBytes { get; }

        /// <summary>
        /// A bitmask indicating how this instance is permitted to be used.
        /// </summary>
        public abstract BufferUsage Usage { get; }

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Updates a region of this <see cref="DeviceBuffer"/> with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/> storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">The value to upload.</param>
        public unsafe void Update<T>(
            uint bufferOffsetInBytes,
            T source) where T : struct
            => _gd.UpdateBuffer(this, bufferOffsetInBytes, source);

        /// <summary>
        /// Updates a region of this <see cref="DeviceBuffer"/> with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the single value to upload.</param>
        public unsafe void Update<T>(
            uint bufferOffsetInBytes,
            ref T source) where T : struct
            => _gd.UpdateBuffer(this, bufferOffsetInBytes, ref source);

        /// <summary>
        /// Updates a region of this <see cref="DeviceBuffer"/> with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the first of a series of values to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public unsafe void Update<T>(
            uint bufferOffsetInBytes,
            ref T source,
            uint sizeInBytes) where T : struct
            => _gd.UpdateBuffer(this, bufferOffsetInBytes, ref source, sizeInBytes);

        /// <summary>
        /// Updates a region of this <see cref="DeviceBuffer"/> with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">An array containing the data to upload.</param>
        public unsafe void Update<T>(
            uint bufferOffsetInBytes,
            T[] source) where T : struct
            => _gd.UpdateBuffer(this, bufferOffsetInBytes, source);

        /// <summary>
        /// Updates a region of this <see cref="DeviceBuffer"/> with new data.
        /// </summary>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public void Update(
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes)
            => _gd.UpdateBuffer(this, bufferOffsetInBytes, source, sizeInBytes);

        /// <summary>
        /// A bool indicating whether this instance has been disposed.
        /// </summary>
        public abstract bool IsDisposed { get; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}

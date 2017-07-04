using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A generic <see cref="DeviceBuffer"/>, which is used to store generic data on the graphics device.
    /// </summary>
    public interface DeviceBuffer : IDisposable
    {
        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="data">An array containing the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total size of the data to transfer, in bytes.</param>
        void SetData<T>(T[] data) where T : struct;

        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="data">An array containing the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total size of the data to transfer, in bytes.</param>
        /// <param name="destinationOffsetInBytes">An offset into the <see cref="DeviceBuffer"/> at which data is stored.</param>
        void SetData<T>(T[] data, int destinationOffsetInBytes) where T : struct;

        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="data">An <see cref="ArraySegment{T}"/> containing the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total size of the data to transfer, in bytes.</param>
        /// <param name="destinationOffsetInBytes">An offset into the <see cref="DeviceBuffer"/> at which data is stored.</param>
        void SetData<T>(ArraySegment<T> data, int destinationOffsetInBytes) where T : struct;

        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="data">A reference to the first element of the starting location of the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total size in bytes to transfer.</param>
        void SetData<T>(ref T data, int dataSizeInBytes) where T : struct;

        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="data">A reference to the first element of the starting location of the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total size of the data to transfer, in bytes.</param>
        /// <param name="destinationOffsetInBytes">An offset into the <see cref="DeviceBuffer"/> at which data is stored.</param>
        void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct;

        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>. 
        /// </summary>
        /// <param name="data">A pointer to the beginning of the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total number of bytes to transfer.</param>
        void SetData(IntPtr data, int dataSizeInBytes);

        /// <summary>
        /// Transfers the given data into the <see cref="DeviceBuffer"/>. 
        /// </summary>
        /// <param name="data">A pointer to the beginning of the data to transfer.</param>
        /// <param name="dataSizeInBytes">The total number of bytes to transfer.</param>
        /// <param name="destinationOffsetInBytes">An offset into the <see cref="DeviceBuffer"/> at which data is stored.</param>
        void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes);

        /// <summary>
        /// Retrieves data from the <see cref="DeviceBuffer"/> into a CPU-side array.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="storageLocation">The CPU-side array to store the retrieved data.</param>
        /// <param name="storageSizeInBytes">The total size of thef CPU storage array, in bytes.</param>
        void GetData<T>(T[] storageLocation) where T : struct;

        /// <summary>
        /// Retrieves data from the <see cref="DeviceBuffer"/> into a CPU-side array.
        /// </summary>
        /// <typeparam name="T">The type of data; must be a value type.</typeparam>
        /// <param name="storageLocation">A reference to the beginning of the CPU-side storage location.</param>
        /// <param name="storageSizeInBytes">The total size of thef CPU storage array, in bytes.</param>
        void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct;

        /// <summary>
        /// Retrieves data from the <see cref="DeviceBuffer"/> into a CPU-side array.
        /// </summary>
        /// <param name="storageLocation">A pointer to the beginning of the CPU-side storage location.</param>
        /// <param name="storageSizeInBytes">The total size of thef CPU storage array, in bytes.</param>
        void GetData(IntPtr storageLocation, int storageSizeInBytes);

        /// <summary>
        /// Maps the <see cref="DeviceBuffer"/> into an accessible CPU buffer which can be read and written to.
        /// </summary>
        /// <param name="numBytes">The total number of bytes to map.</param>
        /// <returns>A pointer to the beginning of the mapped region.</returns>
        IntPtr MapBuffer(int numBytes);

        /// <summary>
        /// Unmaps the <see cref="DeviceBuffer"/>, invalidating the mapped region and propogating changes to the GPU.
        /// This function must be called only after <see cref="MapBuffer(int)"/> has already been called.
        /// </summary>
        void UnmapBuffer();
    }
}

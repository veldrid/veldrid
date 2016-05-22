using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A generic device buffer, which is used to store generic data on the graphics device.
    /// </summary>
    public interface DeviceBuffer
    {
        void SetData<T>(T[] data, int dataSizeInBytes) where T : struct;
        void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct;
        void SetData<T>(ref T data, int dataSizeInBytes) where T : struct;
        void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct;
        void SetData(IntPtr data, int dataSizeInBytes);
        void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes);

        void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct;
        void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct;
        void GetData(IntPtr storageLocation, int storageSizeInBytes);

        IntPtr MapBuffer(int numBytes);
        void UnmapBuffer();
    }
}

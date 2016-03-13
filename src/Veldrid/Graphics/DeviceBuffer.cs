using System;

namespace Veldrid.Graphics
{
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
    }
}

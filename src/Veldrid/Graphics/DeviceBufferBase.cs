using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    public abstract class DeviceBufferBase : DeviceBuffer
    {
        public void GetData<T>(T[] storageLocation) where T : struct
        {
            int sizeofT = Unsafe.SizeOf<T>();
            GCHandle gcHandle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            GetData(gcHandle.AddrOfPinnedObject(), sizeofT * storageLocation.Length);
            gcHandle.Free();
        }

        public unsafe void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct
        {
            IntPtr storagePtr = (IntPtr)Unsafe.AsPointer(ref storageLocation);
            GetData(storagePtr, storageSizeInBytes);
        }

        public unsafe void SetData<T>(T[] data) where T : struct
        {
            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int dataSizeInBytes = data.Length * Unsafe.SizeOf<T>();
            SetData(gcHandle.AddrOfPinnedObject(), dataSizeInBytes, 0);
            gcHandle.Free();
        }

        public unsafe void SetData<T>(T[] data, int destinationOffsetInBytes) where T : struct
        {
            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int dataSizeInBytes = data.Length * Unsafe.SizeOf<T>();
            SetData(gcHandle.AddrOfPinnedObject(), dataSizeInBytes, destinationOffsetInBytes);
            gcHandle.Free();
        }

        public unsafe void SetData<T>(ArraySegment<T> data, int destinationOffsetInBytes) where T : struct
        {
            GCHandle gcHandle = GCHandle.Alloc(data.Array, GCHandleType.Pinned);
            byte* arrayPtr = (byte*)gcHandle.AddrOfPinnedObject();
            int sizeofT = Unsafe.SizeOf<T>();
            int byteOffset = (data.Offset * sizeofT);
            int dataSizeInBytes = data.Count * sizeofT;
            SetData(new IntPtr(arrayPtr + byteOffset), dataSizeInBytes, destinationOffsetInBytes);
            gcHandle.Free();
        }

        public unsafe void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
        {
            ref byte byteRef = ref Unsafe.As<T, byte>(ref data);
            fixed (byte* dataPtr = &byteRef)
            {
                SetData((IntPtr)dataPtr, dataSizeInBytes, 0);
            }
        }

        public unsafe void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetData((IntPtr)Unsafe.AsPointer(ref data), dataSizeInBytes, destinationOffsetInBytes);
        }

        public unsafe void SetData(IntPtr data, int dataSizeInBytes)
        {
            SetData(data, dataSizeInBytes, 0);
        }

        public abstract void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes);
        public abstract void GetData(IntPtr storageLocation, int storageSizeInBytes);

        public abstract IntPtr MapBuffer(int numBytes);
        public abstract void UnmapBuffer();

        public abstract void Dispose();
    }
}

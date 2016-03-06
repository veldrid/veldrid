using SharpDX.Direct3D11;
using SharpDX;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DBuffer : DeviceBuffer, System.IDisposable
    {
        public Buffer Buffer { get; }
        protected Device Device { get; }

        private readonly int _bufferSizeInBytes;

        public D3DBuffer(Device device, int sizeInBytes, BindFlags bindFlags, ResourceUsage resourceUsage)
        {
            Device = device;
            Buffer = new Buffer(device, new BufferDescription(sizeInBytes, bindFlags, resourceUsage));
            _bufferSizeInBytes = sizeInBytes;
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
        {
            Device.ImmediateContext.UpdateSubresource(data, Buffer);
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
        {
            Device.ImmediateContext.UpdateSubresource(ref data, Buffer);
        }

        public unsafe void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct
        {
            DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.Read, MapFlags.None);
            GCHandle storageHandle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            System.Buffer.MemoryCopy(
                db.DataPointer.ToPointer(),
                storageHandle.AddrOfPinnedObject().ToPointer(),
                storageSizeInBytes,
                _bufferSizeInBytes);
            storageHandle.Free();
            Device.ImmediateContext.UnmapSubresource(Buffer, 0);
        }

        public unsafe void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct
        {
            DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.Read, MapFlags.None);
            GCHandle storageHandle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            System.Buffer.MemoryCopy(
                db.DataPointer.ToPointer(),
                storageHandle.AddrOfPinnedObject().ToPointer(),
                storageSizeInBytes,
                _bufferSizeInBytes);
            storageHandle.Free();
            Device.ImmediateContext.UnmapSubresource(Buffer, 0);
        }
    }
}

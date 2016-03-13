using SharpDX.Direct3D11;
using SharpDX;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DBuffer : DeviceBuffer, System.IDisposable
    {
        private readonly BindFlags _bindFlags;
        private readonly ResourceUsage _resourceUsage;
        private int _bufferSizeInBytes;

        protected Device Device { get; }

        public Buffer Buffer { get; private set; }

        public D3DBuffer(Device device, int sizeInBytes, BindFlags bindFlags, ResourceUsage resourceUsage)
        {
            _bindFlags = bindFlags;
            _resourceUsage = resourceUsage;

            Device = device;
            InitializeDeviceBuffer(device, sizeInBytes, bindFlags, resourceUsage);
        }

        private void InitializeDeviceBuffer(Device device, int sizeInBytes, BindFlags bindFlags, ResourceUsage resourceUsage)
        {
            Buffer = new Buffer(device, new BufferDescription(sizeInBytes, bindFlags, resourceUsage));
            _bufferSizeInBytes = sizeInBytes;
        }

        public unsafe void SetData<T>(T[] data, int dataSizeInBytes) where T : struct => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            EnsureBufferSize(dataSizeInBytes);

            using (var pinnedArray = data.Pin())
            {
                ResourceRegion subregion = new ResourceRegion()
                {
                    Left = destinationOffsetInBytes,
                    Right = dataSizeInBytes + destinationOffsetInBytes,
                    Bottom = 1,
                    Back = 1
                };

                Device.ImmediateContext.UpdateSubresource(data, Buffer, region: subregion);
            }
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct => SetData(ref data, dataSizeInBytes, 0);
        public void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            if (destinationOffsetInBytes != 0)
            {
                throw new System.NotImplementedException();
            }

            EnsureBufferSize(dataSizeInBytes);
            Device.ImmediateContext.UpdateSubresource(ref data, Buffer);
        }

        public void SetData(System.IntPtr data, int dataSizeInBytes) => SetData(data, dataSizeInBytes, 0);
        public void SetData(System.IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            if (destinationOffsetInBytes != 0)
            {
                throw new System.NotImplementedException();
            }

            EnsureBufferSize(dataSizeInBytes);
            Device.ImmediateContext.UpdateSubresource(new DataBox(data), Buffer);
        }

        private void EnsureBufferSize(int dataSizeInBytes)
        {
            if (_bufferSizeInBytes < dataSizeInBytes)
            {
                Buffer oldBuffer = Buffer;
                InitializeDeviceBuffer(Device, dataSizeInBytes, _bindFlags, _resourceUsage);
                Device.ImmediateContext.CopyResource(oldBuffer, Buffer);
                oldBuffer.Dispose();
                _bufferSizeInBytes = dataSizeInBytes;
            }
        }

        public unsafe void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct
        {
            Buffer stagingBuffer = new Buffer(Device, new BufferDescription()
            {
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                SizeInBytes = _bufferSizeInBytes
            });

            Device.ImmediateContext.CopyResource(Buffer, stagingBuffer);

            DataBox db = Device.ImmediateContext.MapSubresource(stagingBuffer, 0, MapMode.Read, MapFlags.None);
            GCHandle storageHandle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            System.Buffer.MemoryCopy(
                db.DataPointer.ToPointer(),
                storageHandle.AddrOfPinnedObject().ToPointer(),
                storageSizeInBytes,
                _bufferSizeInBytes);
            storageHandle.Free();
            Device.ImmediateContext.UnmapSubresource(Buffer, 0);
        }

        public void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct
        {
            GCHandle storageHandle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);

            System.IntPtr storagePtr = storageHandle.AddrOfPinnedObject();
            GetData(storagePtr, storageSizeInBytes);

            storageHandle.Free();
        }

        public unsafe void GetData(System.IntPtr storageLocation, int storageSizeInBytes)
        {
            DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.Read, MapFlags.None);
            System.Buffer.MemoryCopy(
                db.DataPointer.ToPointer(),
                storageLocation.ToPointer(),
                storageSizeInBytes,
                _bufferSizeInBytes);
            Device.ImmediateContext.UnmapSubresource(Buffer, 0);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}

using SharpDX.Direct3D11;
using SharpDX;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.Direct3D
{
    public abstract class D3DBuffer : DeviceBuffer, System.IDisposable
    {
        private readonly BindFlags _bindFlags;
        private readonly ResourceUsage _resourceUsage;
        private readonly CpuAccessFlags _cpuFlags;
        private int _bufferSizeInBytes;

        protected Device Device { get; }

        public Buffer Buffer { get; private set; }

        public D3DBuffer(Device device, int sizeInBytes, BindFlags bindFlags, ResourceUsage resourceUsage, CpuAccessFlags cpuFlags)
        {
            _bindFlags = bindFlags;
            _resourceUsage = resourceUsage;
            _cpuFlags = cpuFlags;
            _bufferSizeInBytes = sizeInBytes;

            Device = device;
            InitializeDeviceBuffer();
        }

        private BufferDescription GetBufferDescription()
        {
            return new BufferDescription(_bufferSizeInBytes, _resourceUsage, _bindFlags, _cpuFlags, ResourceOptionFlags.None, 0);
        }

        protected void InitializeDeviceBuffer()
        {
            BufferDescription bd = GetBufferDescription();
            Buffer = new Buffer(Device, bd);
            _bufferSizeInBytes = bd.SizeInBytes;
        }

        public unsafe void SetData<T>(T[] data, int dataSizeInBytes) where T : struct => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);

            ResourceRegion subregion = new ResourceRegion()
            {
                Left = destinationOffsetInBytes,
                Right = dataSizeInBytes + destinationOffsetInBytes,
                Bottom = 1,
                Back = 1
            };

            Device.ImmediateContext.UpdateSubresource(data, Buffer, region: subregion);
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct => SetData(ref data, dataSizeInBytes, 0);
        public void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            if (destinationOffsetInBytes != 0)
            {
                throw new System.NotImplementedException();
            }

            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
            Device.ImmediateContext.UpdateSubresource(ref data, Buffer);
        }

        public void SetData(System.IntPtr data, int dataSizeInBytes) => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData(System.IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);

            if (_resourceUsage == ResourceUsage.Dynamic)
            {
                DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteNoOverwrite, MapFlags.None);
                Utilities.CopyMemory(
                    new System.IntPtr((byte*)db.DataPointer.ToPointer() + destinationOffsetInBytes),
                    data,
                    dataSizeInBytes);
                Device.ImmediateContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                ResourceRegion subregion = new ResourceRegion()
                {
                    Left = destinationOffsetInBytes,
                    Right = dataSizeInBytes + destinationOffsetInBytes,
                    Bottom = 1,
                    Back = 1
                };

                Device.ImmediateContext.UpdateSubresource(new DataBox(data), Buffer, 0, region: subregion);
            }
        }

        private void EnsureBufferSize(int dataSizeInBytes)
        {
            if (_bufferSizeInBytes < dataSizeInBytes)
            {
                System.Console.WriteLine("TOO SMALL, MAKING BIGGER, NEED " + dataSizeInBytes);
                Buffer oldBuffer = Buffer;
                int previousWidth = _bufferSizeInBytes;
                _bufferSizeInBytes = dataSizeInBytes;
                InitializeDeviceBuffer();
                ResourceRegion region = new ResourceRegion(0, 0, 0, previousWidth, 1, 1);
                Device.ImmediateContext.CopySubresourceRegion(oldBuffer, 0, region, Buffer, 0);

                oldBuffer.Dispose();
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
            Device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
            stagingBuffer.Dispose();
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
            Buffer stagingBuffer = new Buffer(Device, new BufferDescription()
            {
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                SizeInBytes = _bufferSizeInBytes
            });

            Device.ImmediateContext.CopyResource(Buffer, stagingBuffer);

            DataBox db = Device.ImmediateContext.MapSubresource(stagingBuffer, 0, MapMode.Read, MapFlags.None);
            System.Buffer.MemoryCopy(
                db.DataPointer.ToPointer(),
                storageLocation.ToPointer(),
                storageSizeInBytes,
                _bufferSizeInBytes);
            Device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
            stagingBuffer.Dispose();
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}

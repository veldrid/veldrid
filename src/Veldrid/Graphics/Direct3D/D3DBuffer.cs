using SharpDX.Direct3D11;
using SharpDX;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public abstract class D3DBuffer : DeviceBuffer, IDisposable
    {
        private readonly BindFlags _bindFlags;
        private readonly ResourceUsage _resourceUsage;
        private readonly CpuAccessFlags _cpuFlags;
        private int _bufferSizeInBytes;

        protected Device Device { get; }

        public SharpDX.Direct3D11.Buffer Buffer { get; private set; }

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
            Buffer = new SharpDX.Direct3D11.Buffer(Device, bd);
            _bufferSizeInBytes = bd.SizeInBytes;
        }

        public unsafe void SetData<T>(T[] data, int dataSizeInBytes) where T : struct => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetArrayDataCore(data, 0, dataSizeInBytes, destinationOffsetInBytes);
        }

        public void SetData<T>(ArraySegment<T> data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetArrayDataCore(data.Array, data.Offset, dataSizeInBytes, destinationOffsetInBytes);
        }

        private unsafe void SetArrayDataCore<T>(T[] data, int startIndex, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            if (dataSizeInBytes == 0)
            {
                return;
            }

            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);

            if (_resourceUsage == ResourceUsage.Dynamic)
            {
                DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                using (var pin = data.Pin())
                {
                    SharpDX.Utilities.CopyMemory(
                        new IntPtr((byte*)db.DataPointer.ToPointer() + destinationOffsetInBytes),
                        new IntPtr(((byte*)pin.Ptr.ToPointer()) + (startIndex * Unsafe.SizeOf<T>())),
                        dataSizeInBytes);
                }
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

                Device.ImmediateContext.UpdateSubresource(data, Buffer, region: subregion);
            }
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct => SetData(ref data, dataSizeInBytes, 0);
        public unsafe void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            if (destinationOffsetInBytes != 0)
            {
                throw new NotImplementedException();
            }

            if (_resourceUsage == ResourceUsage.Dynamic)
            {
                DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteNoOverwrite, MapFlags.None);
                Unsafe.CopyBlock(
                        (byte*)db.DataPointer.ToPointer() + destinationOffsetInBytes,
                        Unsafe.AsPointer(ref data),
                        (uint)dataSizeInBytes);
                Device.ImmediateContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
                Device.ImmediateContext.UpdateSubresource(ref data, Buffer);
            }
        }

        public void SetData(IntPtr data, int dataSizeInBytes) => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);

            if (_resourceUsage == ResourceUsage.Dynamic)
            {
                DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                SharpDX.Utilities.CopyMemory(
                    new IntPtr((byte*)db.DataPointer.ToPointer() + destinationOffsetInBytes),
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
                SharpDX.Direct3D11.Buffer oldBuffer = Buffer;
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
            SharpDX.Direct3D11.Buffer stagingBuffer = new SharpDX.Direct3D11.Buffer(Device, new BufferDescription()
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

            IntPtr storagePtr = storageHandle.AddrOfPinnedObject();
            GetData(storagePtr, storageSizeInBytes);

            storageHandle.Free();
        }

        public unsafe void GetData(IntPtr storageLocation, int storageSizeInBytes)
        {
            SharpDX.Direct3D11.Buffer stagingBuffer = new SharpDX.Direct3D11.Buffer(Device, new BufferDescription()
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

        public IntPtr MapBuffer(int numBytes)
        {
            EnsureBufferSize(numBytes);
            var db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            return db.DataPointer;
        }

        public void UnmapBuffer()
        {
            Device.ImmediateContext.UnmapSubresource(Buffer, 0);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}

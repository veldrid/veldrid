using SharpDX.Direct3D11;
using SharpDX;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public abstract class D3DBuffer : DeviceBufferBase
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

        public unsafe override void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            if (dataSizeInBytes == 0)
            {
                return;
            }

            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);

            if (_resourceUsage == ResourceUsage.Dynamic)
            {
                DataBox db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                {
                    SharpDX.Utilities.CopyMemory(
                        new IntPtr((byte*)db.DataPointer.ToPointer() + destinationOffsetInBytes),
                        new IntPtr((byte*)data),
                        dataSizeInBytes);
                }
                Device.ImmediateContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                ResourceRegion? subregion = null;
                if ((_bindFlags & BindFlags.ConstantBuffer) != BindFlags.ConstantBuffer)
                {
                    // For a shader-constant buffer; set pDstBox to null. It is not possible to use
                    // this method to partially update a shader-constant buffer

                    subregion = new ResourceRegion()
                    {
                        Left = destinationOffsetInBytes,
                        Right = dataSizeInBytes + destinationOffsetInBytes,
                        Bottom = 1,
                        Back = 1
                    };
                }

                Device.ImmediateContext.UpdateSubresource(Buffer, 0, subregion, data, 0, 0);
            }
        }

        public override IntPtr MapBuffer(int numBytes)
        {
            EnsureBufferSize(numBytes);
            var db = Device.ImmediateContext.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            return db.DataPointer;
        }

        public override void UnmapBuffer()
        {
            Device.ImmediateContext.UnmapSubresource(Buffer, 0);
        }

        public unsafe override void GetData(IntPtr storageLocation, int storageSizeInBytes)
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

        public override void Dispose()
        {
            Buffer.Dispose();
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
    }

}

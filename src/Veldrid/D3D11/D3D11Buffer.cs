using System;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace Veldrid.D3D11
{
    internal class D3D11Buffer : Buffer
    {
        private readonly SharpDX.Direct3D11.Buffer _buffer;

        public override uint SizeInBytes { get; }

        public override BufferUsage Usage { get; }

        public SharpDX.Direct3D11.Buffer Buffer => _buffer;

        public UnorderedAccessView UnorderedAccessView { get; }

        internal ShaderResourceView ShaderResourceView { get; }

        public D3D11Buffer(Device device, uint sizeInBytes, BufferUsage usage, uint structureByteStride)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            SharpDX.Direct3D11.BufferDescription bd = new SharpDX.Direct3D11.BufferDescription(
                (int)sizeInBytes,
                D3D11Formats.VdToD3D11BindFlags(usage),
                ResourceUsage.Default);
            if ((usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly
                || (usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                bd.OptionFlags = ResourceOptionFlags.BufferStructured;
                bd.StructureByteStride = (int)structureByteStride;
            }
            if ((usage & BufferUsage.IndirectBuffer) == BufferUsage.IndirectBuffer)
            {
                bd.OptionFlags = ResourceOptionFlags.DrawIndirectArguments;
            }

            if ((usage & BufferUsage.Dynamic) == BufferUsage.Dynamic)
            {
                bd.Usage = ResourceUsage.Dynamic;
                bd.CpuAccessFlags = CpuAccessFlags.Write;
            }
            else if ((usage & BufferUsage.Staging) == BufferUsage.Staging)
            {
                bd.Usage = ResourceUsage.Staging;
                bd.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            }

            _buffer = new SharpDX.Direct3D11.Buffer(device, bd);

            if ((usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite
                || (usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly)
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer
                };
                srvDesc.BufferEx.ElementCount = 1024;
                ShaderResourceView = new ShaderResourceView(device, _buffer, srvDesc);
            }

            if ((usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription
                {
                    Dimension = UnorderedAccessViewDimension.Buffer
                };

                uavDesc.Buffer.ElementCount = (int)(SizeInBytes / structureByteStride);
                uavDesc.Format = SharpDX.DXGI.Format.Unknown;

                UnorderedAccessView = new UnorderedAccessView(device, _buffer, uavDesc);
            }
        }

        public override void Dispose()
        {
            ShaderResourceView?.Dispose();
            UnorderedAccessView?.Dispose();
            _buffer.Dispose();
        }
    }
}

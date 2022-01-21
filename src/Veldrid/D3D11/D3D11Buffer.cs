using System;
using Vortice.Direct3D11;
using System.Collections.Generic;
using Vortice.DXGI;
using Vortice.Direct3D;

namespace Veldrid.D3D11
{
    internal class D3D11Buffer : DeviceBuffer
    {
        private readonly ID3D11Device _device;
        private readonly ID3D11Buffer _buffer;
        private readonly object _accessViewLock = new object();
        private readonly Dictionary<OffsetSizePair, ID3D11ShaderResourceView> _srvs
            = new Dictionary<OffsetSizePair, ID3D11ShaderResourceView>();
        private readonly Dictionary<OffsetSizePair, ID3D11UnorderedAccessView> _uavs
            = new Dictionary<OffsetSizePair, ID3D11UnorderedAccessView>();
        private readonly uint _structureByteStride;
        private readonly bool _rawBuffer;
        private string _name;

        public override uint SizeInBytes { get; }

        public override BufferUsage Usage { get; }

        public override bool IsDisposed => _buffer.NativePointer == IntPtr.Zero;

        public ID3D11Buffer Buffer => _buffer;

        public D3D11Buffer(ID3D11Device device, uint sizeInBytes, BufferUsage usage, uint structureByteStride, bool rawBuffer)
        {
            _device = device;
            SizeInBytes = sizeInBytes;
            Usage = usage;
            _structureByteStride = structureByteStride;
            _rawBuffer = rawBuffer;

            Vortice.Direct3D11.BufferDescription bd = new Vortice.Direct3D11.BufferDescription(
                (int)sizeInBytes,
                D3D11Formats.VdToD3D11BindFlags(usage),
                ResourceUsage.Default);
            if ((usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly
                || (usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                if (rawBuffer)
                {
                    bd.OptionFlags = ResourceOptionFlags.BufferAllowRawViews;
                }
                else
                {
                    bd.OptionFlags = ResourceOptionFlags.BufferStructured;
                    bd.StructureByteStride = (int)structureByteStride;
                }
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

            _buffer = device.CreateBuffer(bd);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                Buffer.DebugName = value;
                foreach (KeyValuePair<OffsetSizePair, ID3D11ShaderResourceView> kvp in _srvs)
                {
                    kvp.Value.DebugName = value + "_SRV";
                }
                foreach (KeyValuePair<OffsetSizePair, ID3D11UnorderedAccessView> kvp in _uavs)
                {
                    kvp.Value.DebugName = value + "_UAV";
                }
            }
        }

        public override void Dispose()
        {
            foreach (KeyValuePair<OffsetSizePair, ID3D11ShaderResourceView> kvp in _srvs)
            {
                kvp.Value.Dispose();
            }
            foreach (KeyValuePair<OffsetSizePair, ID3D11UnorderedAccessView> kvp in _uavs)
            {
                kvp.Value.Dispose();
            }
            _buffer.Dispose();
        }

        internal ID3D11ShaderResourceView GetShaderResourceView(uint offset, uint size)
        {
            lock (_accessViewLock)
            {
                OffsetSizePair pair = new OffsetSizePair(offset, size);
                if (!_srvs.TryGetValue(pair, out ID3D11ShaderResourceView srv))
                {
                    srv = CreateShaderResourceView(offset, size);
                    _srvs.Add(pair, srv);
                }

                return srv;
            }
        }

        internal ID3D11UnorderedAccessView GetUnorderedAccessView(uint offset, uint size)
        {
            lock (_accessViewLock)
            {
                OffsetSizePair pair = new OffsetSizePair(offset, size);
                if (!_uavs.TryGetValue(pair, out ID3D11UnorderedAccessView uav))
                {
                    uav = CreateUnorderedAccessView(offset, size);
                    _uavs.Add(pair, uav);
                }

                return uav;
            }
        }

        private ID3D11ShaderResourceView CreateShaderResourceView(uint offset, uint size)
        {
            if (_rawBuffer)
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription(_buffer,
                    Format.R32_Typeless,
                    (int)offset / 4,
                    (int)size / 4,
                    BufferExtendedShaderResourceViewFlags.Raw);

                return _device.CreateShaderResourceView(_buffer, srvDesc);
            }
            else
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    ViewDimension = ShaderResourceViewDimension.Buffer
                };
                srvDesc.Buffer.NumElements = (int)(size / _structureByteStride);
                srvDesc.Buffer.ElementOffset = (int)(offset / _structureByteStride);
                return _device.CreateShaderResourceView(_buffer, srvDesc);
            }
        }

        private ID3D11UnorderedAccessView CreateUnorderedAccessView(uint offset, uint size)
        {
            if (_rawBuffer)
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription(_buffer,
                    Format.R32_Typeless,
                    (int)offset / 4,
                    (int)size / 4,
                    BufferUnorderedAccessViewFlags.Raw);

                return _device.CreateUnorderedAccessView(_buffer, uavDesc);
            }
            else
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription(_buffer,
                    Format.Unknown,
                    (int)(offset / _structureByteStride),
                    (int)(size / _structureByteStride)
                    );

                return _device.CreateUnorderedAccessView(_buffer, uavDesc);
            }
        }

        private struct OffsetSizePair : IEquatable<OffsetSizePair>
        {
            public readonly uint Offset;
            public readonly uint Size;

            public OffsetSizePair(uint offset, uint size)
            {
                Offset = offset;
                Size = size;
            }

            public bool Equals(OffsetSizePair other) => Offset.Equals(other.Offset) && Size.Equals(other.Size);
            public override int GetHashCode() => HashHelper.Combine(Offset.GetHashCode(), Size.GetHashCode());
        }
    }
}

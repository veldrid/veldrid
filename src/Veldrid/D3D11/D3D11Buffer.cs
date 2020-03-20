using System;
using SharpDX.Direct3D11;
using System.Diagnostics;
using System.Collections.Generic;

namespace Veldrid.D3D11
{
    internal class D3D11Buffer : DeviceBuffer
    {
        private readonly Device _device;
        private readonly SharpDX.Direct3D11.Buffer _buffer;
        private readonly object _accessViewLock = new object();
        private readonly Dictionary<OffsetSizePair, ShaderResourceView> _srvs
            = new Dictionary<OffsetSizePair, ShaderResourceView>();
        private readonly Dictionary<OffsetSizePair, UnorderedAccessView> _uavs
            = new Dictionary<OffsetSizePair, UnorderedAccessView>();
        private readonly uint _structureByteStride;
        private readonly bool _rawBuffer;
        private string _name;

        public override uint SizeInBytes { get; }

        public override BufferUsage Usage { get; }

        public override bool IsDisposed => _buffer.IsDisposed;

        public SharpDX.Direct3D11.Buffer Buffer => _buffer;

        public D3D11Buffer(Device device, uint sizeInBytes, BufferUsage usage, uint structureByteStride, bool rawBuffer)
        {
            _device = device;
            SizeInBytes = sizeInBytes;
            Usage = usage;
            _structureByteStride = structureByteStride;
            _rawBuffer = rawBuffer;
            SharpDX.Direct3D11.BufferDescription bd = new SharpDX.Direct3D11.BufferDescription(
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

            _buffer = new SharpDX.Direct3D11.Buffer(device, bd);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                Buffer.DebugName = value;
                foreach (KeyValuePair<OffsetSizePair, ShaderResourceView> kvp in _srvs)
                {
                    kvp.Value.DebugName = value + "_SRV";
                }
                foreach (KeyValuePair<OffsetSizePair, UnorderedAccessView> kvp in _uavs)
                {
                    kvp.Value.DebugName = value + "_UAV";
                }
            }
        }

        public override void Dispose()
        {
            foreach (KeyValuePair<OffsetSizePair, ShaderResourceView> kvp in _srvs)
            {
                kvp.Value.Dispose();
            }
            foreach (KeyValuePair<OffsetSizePair, UnorderedAccessView> kvp in _uavs)
            {
                kvp.Value.Dispose();
            }
            _buffer.Dispose();
        }

        internal ShaderResourceView GetShaderResourceView(uint offset, uint size)
        {
            lock (_accessViewLock)
            {
                OffsetSizePair pair = new OffsetSizePair(offset, size);
                if (!_srvs.TryGetValue(pair, out ShaderResourceView srv))
                {
                    srv = CreateShaderResourceView(offset, size);
                    _srvs.Add(pair, srv);
                }

                return srv;
            }
        }

        internal UnorderedAccessView GetUnorderedAccessView(uint offset, uint size)
        {
            lock (_accessViewLock)
            {
                OffsetSizePair pair = new OffsetSizePair(offset, size);
                if (!_uavs.TryGetValue(pair, out UnorderedAccessView uav))
                {
                    uav = CreateUnorderedAccessView(offset, size);
                    _uavs.Add(pair, uav);
                }

                return uav;
            }
        }

        private ShaderResourceView CreateShaderResourceView(uint offset, uint size)
        {
            if (_rawBuffer)
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer,
                    Format = SharpDX.DXGI.Format.R32_Typeless
                };
                srvDesc.BufferEx.ElementCount = (int)size / 4;
                srvDesc.BufferEx.Flags = ShaderResourceViewExtendedBufferFlags.Raw;
                srvDesc.BufferEx.FirstElement = (int)offset / 4;
                return new ShaderResourceView(_device, _buffer, srvDesc);
            }
            else
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer
                };
                srvDesc.Buffer.ElementCount = (int)(size / _structureByteStride);
                srvDesc.Buffer.ElementOffset = (int)(offset / _structureByteStride);
                return new ShaderResourceView(_device, _buffer, srvDesc);
            }
        }

        private UnorderedAccessView CreateUnorderedAccessView(uint offset, uint size)
        {
            if (_rawBuffer)
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription
                {
                    Dimension = UnorderedAccessViewDimension.Buffer
                };

                uavDesc.Buffer.ElementCount = (int)size / 4;
                uavDesc.Buffer.Flags = UnorderedAccessViewBufferFlags.Raw;
                uavDesc.Format = SharpDX.DXGI.Format.R32_Typeless;
                uavDesc.Buffer.FirstElement = (int)offset / 4;

                return new UnorderedAccessView(_device, _buffer, uavDesc);

            }
            else
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription
                {
                    Dimension = UnorderedAccessViewDimension.Buffer
                };

                uavDesc.Buffer.ElementCount = (int)(size / _structureByteStride);
                uavDesc.Format = SharpDX.DXGI.Format.Unknown;
                uavDesc.Buffer.FirstElement = (int)(offset / _structureByteStride);

                return new UnorderedAccessView(_device, _buffer, uavDesc);
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

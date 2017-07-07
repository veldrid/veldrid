using System;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DIndexBuffer : D3DBuffer, IndexBuffer
    {
        private readonly Device _device;
        private SharpDX.DXGI.Format _format;
        private int _offset = 0;

        public D3DIndexBuffer(Device device, int sizeInBytes, bool isDynamic, SharpDX.DXGI.Format format = SharpDX.DXGI.Format.Unknown)
            : base(
                device,
                sizeInBytes,
                BindFlags.IndexBuffer,
                isDynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                isDynamic ? CpuAccessFlags.Write : CpuAccessFlags.None)
        {
            _format = format;
            _device = device;
        }

        public void Apply()
        {
            _device.ImmediateContext.InputAssembler.SetIndexBuffer(Buffer, _format, _offset);
        }

        public void SetIndices(ushort[] indices) => SetIndices(indices, 0, 0);

        public void SetIndices(ushort[] indices, int stride, int elementOffset)
        {
            _format = SharpDX.DXGI.Format.R16_UInt;
            SetData(indices, elementOffset * sizeof(ushort));
        }

        public void SetIndices(uint[] indices) => SetIndices(indices, 0, 0);
        public void SetIndices(uint[] indices, int stride, int elementOffset)
        {
            _format = SharpDX.DXGI.Format.R32_UInt;
            SetData(indices, elementOffset * sizeof(uint));
        }

        public void SetIndices(IntPtr indices, IndexFormat format, int count)
            => SetIndices(indices, format, count, 0);
        public void SetIndices(IntPtr indices, IndexFormat format, int count, int elementOffset)
        {
            int elementSizeInBytes = format == IndexFormat.UInt16 ? sizeof(ushort) : sizeof(uint);
            SetData(indices, elementSizeInBytes * count, elementSizeInBytes * elementOffset);
            SharpDX.DXGI.Format dxgiFormat = D3DFormats.VeldridToD3DIndexFormat(format);
            _format = dxgiFormat;
        }
    }
}
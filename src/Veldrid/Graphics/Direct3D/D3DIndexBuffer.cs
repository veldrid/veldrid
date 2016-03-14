using System;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DIndexBuffer : D3DBuffer, IndexBuffer
    {
        private readonly Device _device;
        private SharpDX.DXGI.Format _format;
        private int _offset = 0;

        public D3DIndexBuffer(Device device, int sizeInBytes) : base(device, sizeInBytes, BindFlags.IndexBuffer, ResourceUsage.Default, CpuAccessFlags.None)
        {
            _device = device;
        }

        public void Apply()
        {
            _device.ImmediateContext.InputAssembler.SetIndexBuffer(Buffer, _format, _offset);
        }

        public void SetIndices(int[] indices) => SetIndices(indices, 0, 0);
        public void SetIndices(int[] indices, int stride, int elementOffset)
        {
            _format = SharpDX.DXGI.Format.R32_UInt;
            SetData(indices, indices.Length * 4, elementOffset * sizeof(int));
        }

        public void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count)
            => SetIndices(indices, format, elementSizeInBytes, count, 0);
        public void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count, int elementOffset)
        {
            SetData(indices, elementSizeInBytes * count, elementSizeInBytes * elementOffset);
            _format = D3DFormats.ConvertIndexFormat(format);
        }
    }
}
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DIndexBuffer : D3DBuffer, IndexBuffer
    {
        private readonly Device _device;
        private int _offset;

        public D3DIndexBuffer(Device device, int sizeInBytes) : base(device, sizeInBytes, BindFlags.IndexBuffer, ResourceUsage.Default)
        {
            _device = device;
        }

        public void Apply()
        {
            _device.ImmediateContext.InputAssembler.SetIndexBuffer(Buffer, SharpDX.DXGI.Format.R32_UInt, _offset);
        }

        public void SetIndices(int[] indices) => SetIndices(indices, 0, System.IntPtr.Zero);
        public void SetIndices(int[] indices, int stride, System.IntPtr offset)
        {
            _offset = offset.ToInt32();
            SetData(indices, indices.Length * 4);
        }
    }
}
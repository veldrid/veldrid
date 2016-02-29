using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DVertexBuffer : D3DBuffer, VertexBuffer
    {
        private int _stride;

        public D3DVertexBuffer(Device device) : base(device, BindFlags.VertexBuffer, ResourceUsage.Default) { }

        public void Apply()
        {
            Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Buffer, _stride, 0));
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct
        {
            _stride = descriptor.VertexSizeInBytes;
            UpdateBufferData(vertexData);
        }
    }
}
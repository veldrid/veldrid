using System;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DVertexBuffer : D3DBuffer, VertexBuffer
    {
        private int _stride;

        public D3DVertexBuffer(Device device, int sizeInBytes) : base(device, sizeInBytes, BindFlags.VertexBuffer, ResourceUsage.Default) { }

        public void Apply()
        {
            Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Buffer, _stride, 0));
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct => SetVertexData(vertexData, descriptor, 0);
        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct
        {
            _stride = descriptor.VertexSizeInBytes;
            SetData(vertexData, descriptor.VertexSizeInBytes * vertexData.Length, destinationOffsetInVertices * descriptor.VertexSizeInBytes);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices) => SetVertexData(vertexData, descriptor, numVertices, 0);
        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices, int destinationOffsetInVertices)
        {
            _stride = descriptor.VertexSizeInBytes;
            SetData(vertexData, descriptor.VertexSizeInBytes * numVertices, destinationOffsetInVertices * descriptor.VertexSizeInBytes);
        }
    }
}
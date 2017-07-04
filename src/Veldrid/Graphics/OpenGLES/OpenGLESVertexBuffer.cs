using System;
using OpenTK.Graphics.ES30;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESVertexBuffer : OpenGLESBuffer, VertexBuffer
    {
        public OpenGLESVertexBuffer(bool isDynamic)
            : base(BufferTarget.ArrayBuffer)
        { }

        public void Apply()
        {
            Bind();
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * vertexData.Length);
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * destinationOffsetInVertices);
        }

        public void SetVertexData<T>(ArraySegment<T> vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * destinationOffsetInVertices);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices)
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * numVertices);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices, int destinationOffsetInVertices)
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * numVertices, descriptor.VertexSizeInBytes * destinationOffsetInVertices);
        }
    }
}

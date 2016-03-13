using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLVertexBuffer : OpenGLBuffer, VertexBuffer
    {
        public OpenGLVertexBuffer() : base(BufferTarget.ArrayBuffer) { }

        public void Apply()
        {
            Bind();
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * vertexData.Length);
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor, int offset) where T : struct
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * vertexData.Length, offset);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices)
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * numVertices);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices, int destinationOffsetInBytes)
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * numVertices, destinationOffsetInBytes);
        }
    }
}

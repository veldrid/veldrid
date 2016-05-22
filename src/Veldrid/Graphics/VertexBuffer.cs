using System;

namespace Veldrid.Graphics
{
    public interface VertexBuffer : IDisposable
    {
        void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct;
        void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct;
        void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices);
        void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices, int destinationOffsetInVertices);
    }
}
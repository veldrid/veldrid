using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-side buffer for storing vertex data.
    /// </summary>
    public interface VertexBuffer : DeviceBuffer, IDisposable
    {
        /// <summary>
        /// Sets the data in the VertexBuffer.
        /// </summary>
        void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct;

        /// <summary>
        /// Sets the data in the VertexBuffer.
        /// </summary>
        void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct;

        /// <summary>
        /// Sets the data in the VertexBuffer.
        /// </summary>
        void SetVertexData<T>(ArraySegment<T> vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct;

        /// <summary>
        /// Sets the data in the VertexBuffer.
        /// </summary>
        void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices);

        /// <summary>
        /// Sets the data in the VertexBuffer.
        /// </summary>
        void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices, int destinationOffsetInVertices);
    }
}
using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A structure describing the overall shape of a vertex element.
    /// </summary>
    public struct VertexDescriptor
    {
        /// <summary>
        /// The size of a single vertex.
        /// </summary>
        public readonly byte VertexSizeInBytes;
        /// <summary>
        /// The number of vertices stored in a buffer.
        /// </summary>
        public readonly byte ElementCount;
        /// <summary>
        /// The stride, in bytes, between the beginning of contiguous vertex elements.
        /// </summary>
        public readonly byte Stride;
        /// <summary>
        /// Indicates that vertex data starts at a given byte offset from the beginning of the buffer.
        /// </summary>
        public readonly IntPtr Offset;

        /// <summary>
        /// Constructs a new <see cref="VertexDescriptor"/>.
        /// </summary>
        /// <param name="vertexSizeInBytes">The total size of an individual vertex.</param>
        /// <param name="elementCount">The number of distinct elements (position, normal, color, etc.) in a vertex element.</param>
        public VertexDescriptor(byte vertexSizeInBytes, byte elementCount)
            : this(vertexSizeInBytes, elementCount, 0, IntPtr.Zero) { }

        /// <summary>
        /// Constructs a new <see cref="VertexDescriptor"/>.
        /// </summary>
        /// <param name="vertexSizeInBytes">The total size of an individual vertex.</param>
        /// <param name="elementCount">The number of distinct elements (position, normal, color, etc.) in a vertex element.</param>
        /// <param name="offset">Indicates that vertex data starts at a given byte offset from the beginning of the buffer.</param>
        /// <param name="stride">The stride, in bytes, between the beginning of contiguous vertex elements.</param>
        public VertexDescriptor(byte vertexSizeInBytes, byte elementCount, byte stride, IntPtr offset)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            ElementCount = elementCount;
            Stride = stride;
            Offset = offset;
        }
    }
}

using System;

namespace Veldrid.Graphics
{
    public struct VertexDescriptor
    {
        public readonly byte VertexSizeInBytes;
        public readonly byte ElementCount;
        public readonly byte Stride;
        public readonly IntPtr Offset;

        public VertexDescriptor(byte vertexSizeInBytes, byte elementCount, byte stride, IntPtr offset)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            ElementCount = elementCount;
            Stride = stride;
            Offset = offset;
        }
    }
}

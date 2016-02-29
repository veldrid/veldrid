using System;
using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public struct VertexDescriptor
    {
        public readonly byte VertexSizeInBytes;
        public readonly byte ElementCount;
        public readonly byte Stride;
        public readonly IntPtr Offset;

        public VertexDescriptor(byte vertexSize, byte elementCount, byte stride, IntPtr offset)
        {
            VertexSizeInBytes = vertexSize;
            ElementCount = elementCount;
            Stride = stride;
            Offset = offset;
        }
    }
}

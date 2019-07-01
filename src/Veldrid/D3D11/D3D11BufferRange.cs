using System;

namespace Veldrid.D3D11
{
    internal struct D3D11BufferRange : IEquatable<D3D11BufferRange>
    {
        public readonly D3D11Buffer Buffer;
        public readonly uint Offset;
        public readonly uint Size;

        public bool IsFullRange => Offset == 0 && Size == Buffer.SizeInBytes;

        public D3D11BufferRange(D3D11Buffer buffer, uint offset, uint size)
        {
            Buffer = buffer;
            Offset = offset;
            Size = size;
        }

        public bool Equals(D3D11BufferRange other)
        {
            return Buffer == other.Buffer && Offset.Equals(other.Offset) && Size.Equals(other.Size);
        }
    }
}

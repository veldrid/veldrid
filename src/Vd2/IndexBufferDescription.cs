using System;

namespace Vd2
{
    public struct IndexBufferDescription : IEquatable<IndexBufferDescription>
    {
        public ulong SizeInBytes;
        public IndexFormat Format;
        public bool Dynamic;

        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
            Dynamic = false;
        }

        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
            Dynamic = dynamic;
        }

        public bool Equals(IndexBufferDescription other)
        {
            return SizeInBytes.Equals(other.SizeInBytes) && Format == other.Format && Dynamic.Equals(other.Dynamic);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(SizeInBytes.GetHashCode(), Format.GetHashCode(), Dynamic.GetHashCode());
        }
    }
}

using System;

namespace Vd2
{
    public struct BufferDescription : IEquatable<BufferDescription>
    {
        public ulong SizeInBytes;
        public bool Dynamic;

        public BufferDescription(ulong sizeInBytes)
        {
            SizeInBytes = sizeInBytes;
            Dynamic = false;
        }

        public BufferDescription(ulong sizeInBytes, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
            Dynamic = dynamic;
        }

        public bool Equals(BufferDescription other)
        {
            return SizeInBytes.Equals(other.SizeInBytes) && Dynamic.Equals(other.Dynamic);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(SizeInBytes.GetHashCode(), Dynamic.GetHashCode());
        }
    }
}

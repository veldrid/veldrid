using System;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    public struct SpecializationConstant : IEquatable<SpecializationConstant>
    {
        public uint ID;
        public uint DataSize;
        public ulong Data;

        public SpecializationConstant(uint id, uint dataSize, ulong data)
        {
            ID = id;
            DataSize = dataSize;
            Data = data;
        }

        public SpecializationConstant(uint id, bool value) : this(id, sizeof(uint), Store(value ? 1u : 0u)) { }
        public SpecializationConstant(uint id, uint value) : this(id, sizeof(uint), Store(value)) { }
        public SpecializationConstant(uint id, int value) : this(id, sizeof(int), Store(value)) { }
        public SpecializationConstant(uint id, ulong value) : this(id, sizeof(ulong), Store(value)) { }
        public SpecializationConstant(uint id, long value) : this(id, sizeof(long), Store(value)) { }
        public SpecializationConstant(uint id, float value) : this(id, sizeof(float), Store(value)) { }
        public SpecializationConstant(uint id, double value) : this(id, sizeof(double), Store(value)) { }

        internal static unsafe ulong Store<T>(T value)
        {
            ulong ret;
            Unsafe.Write(&ret, value);
            return ret;
        }

        public bool Equals(SpecializationConstant other)
        {
            return ID.Equals(other.ID) && DataSize.Equals(other.DataSize) && Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(ID.GetHashCode(), DataSize.GetHashCode(), Data.GetHashCode());
        }
    }
}

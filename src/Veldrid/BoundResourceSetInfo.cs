using System;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    internal struct BoundResourceSetInfo : IEquatable<BoundResourceSetInfo>
    {
        public ResourceSet Set;
        public SmallFixedOrDynamicArray Offsets;

        public BoundResourceSetInfo(ResourceSet set, uint offsetsCount, ref uint offsets)
        {
            Set = set;
            Offsets = new SmallFixedOrDynamicArray(offsetsCount, ref offsets);
        }

        public bool Equals(ResourceSet set, uint offsetsCount, ref uint offsets)
        {
            if (set != Set || offsetsCount != Offsets.Count) { return false; }

            for (uint i = 0; i < Offsets.Count; i++)
            {
                if (Unsafe.Add(ref offsets, (int)i) != Offsets.Get(i)) { return false; }
            }

            return true;
        }

        public bool Equals(BoundResourceSetInfo other)
        {
            if (Set != other.Set || Offsets.Count != other.Offsets.Count)
            {
                return false;
            }

            for (uint i = 0; i < Offsets.Count; i++)
            {
                if (Offsets.Get(i) != other.Offsets.Get(i))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

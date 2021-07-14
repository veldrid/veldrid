using System;

namespace Veldrid
{
    internal struct BoundResourceSetInfo
    {
        public ResourceSet Set;
        public SmallFixedOrDynamicArray Offsets;

        public BoundResourceSetInfo(ResourceSet set, ReadOnlySpan<uint> offsets)
        {
            Set = set;
            Offsets = new SmallFixedOrDynamicArray(offsets);
        }

        public unsafe bool Equals(ResourceSet set, ReadOnlySpan<uint> offsets)
        {
            if (set != Set || offsets.Length != Offsets.Count)
            {
                return false;
            }

            for (uint i = 0; i < offsets.Length; i++)
            {
                if (offsets[(int)i] != Offsets.Get(i))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

using System;
using System.Buffers;

namespace Veldrid
{
    internal unsafe struct SmallFixedOrDynamicArray : IDisposable
    {
        private const int MaxFixedValues = 5;

        public readonly uint Count;
        private fixed uint FixedData[MaxFixedValues];
        public readonly uint[] Data;

        public uint Get(uint i)
        {
            return Count > MaxFixedValues ? Data[i] : FixedData[i];
        }

        public SmallFixedOrDynamicArray(ReadOnlySpan<uint> offsets)
        {
            if (offsets.Length > MaxFixedValues)
            {
                Data = ArrayPool<uint>.Shared.Rent(offsets.Length);
            }
            else
            {
                for (int i = 0; i < offsets.Length; i++)
                {
                    FixedData[i] = offsets[i];
                }

                Data = null;
            }

            Count = (uint)offsets.Length;
        }

        public void Dispose()
        {
            if (Data != null)
            {
                ArrayPool<uint>.Shared.Return(Data);
            }
        }
    }
}

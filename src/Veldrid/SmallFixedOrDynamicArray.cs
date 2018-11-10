using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    internal unsafe struct SmallFixedOrDynamicArray : IDisposable
    {
        private const int MaxFixedValues = 5;

        public readonly uint Count;
        private fixed uint FixedData[MaxFixedValues];
        public readonly uint[] Data;

        public uint Get(uint i) => Count > MaxFixedValues ? Data[i] : FixedData[i];

        public SmallFixedOrDynamicArray(uint count, ref uint data)
        {
            if (count > MaxFixedValues)
            {
                Data = ArrayPool<uint>.Shared.Rent((int)count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    FixedData[i] = Unsafe.Add(ref data, i);
                }

                Data = null;
            }

            Count = count;
        }

        public void Dispose()
        {
            if (Data != null) { ArrayPool<uint>.Shared.Return(Data); }
        }
    }
}

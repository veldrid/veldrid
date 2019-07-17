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

        public SmallFixedOrDynamicArray(Span<uint> offsets)
        {
            uint count = (uint)offsets.Length;
            if (count > MaxFixedValues)
            {
                Data = ArrayPool<uint>.Shared.Rent((int)count);
                for (int i = 0; i < count; i++)
                {
                    Data[i] = offsets[i];
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    FixedData[i] = offsets[i];
                }

                Data = null;
            }

            Count = count;
        }

        public SmallFixedOrDynamicArray(uint count, ref uint data)
        {
            if (count > MaxFixedValues)
            {
                Data = ArrayPool<uint>.Shared.Rent((int)count);
                for (int i = 0; i < count; i++)
                {
                    Data[i] = Unsafe.Add(ref data, i);
                }
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

    internal unsafe struct SmallFixedOrDynamicArray<T>
    {
        private const int MaxFixedValues = 4;

        public readonly uint Count;
        private T _fixedData0;
        private T _fixedData1;
        private T _fixedData2;
        private T _fixedData3;
        private readonly T[] Data;

        public T Get(uint i)
        {
            if (Count < MaxFixedValues)
            {
                switch (i)
                {
                    case 0: return _fixedData0;
                    case 1: return _fixedData1;
                    case 2: return _fixedData2;
                    case 3: return _fixedData3;
                    default:
                        throw new InvalidOperationException("Failed to get element.");
                }
            }
            else
            {
                return Data[i];
            }
        }

        public void Set(uint i, T value)
        {
            if (Data != null)
            {
                Data[i] = value;
            }
            else
            {
                switch (i)
                {
                    case 0: _fixedData0 = value; break;
                    case 1: _fixedData1 = value; break;
                    case 2: _fixedData2 = value; break;
                    case 3: _fixedData3 = value; break;
                }
            }
        }

        public SmallFixedOrDynamicArray(uint count)
        {
            Count = count;
            _fixedData0 = default;
            _fixedData1 = default;
            _fixedData2 = default;
            _fixedData3 = default;
            if (Count > MaxFixedValues)
            {
                Data = new T[count];
            }
            else
            {
                Data = null;
            }
        }

        public SmallFixedOrDynamicArray(T value)
        {
            Count = 1;
            Data = null;
            _fixedData0 = value;
            _fixedData1 = default;
            _fixedData2 = default;
            _fixedData3 = default;
        }

        public SmallFixedOrDynamicArray(Span<T> values)
        {
            _fixedData0 = default;
            _fixedData1 = default;
            _fixedData2 = default;
            _fixedData3 = default;

            Count = (uint)values.Length;
            if (values.Length > MaxFixedValues)
            {
                Data = new T[Count];
                for (uint i = 0; i < Count; i++)
                {
                    Data[i] = values[(int)i];
                }
            }
            else
            {
                for (uint i = 0; i < Count; i++)
                {
                    T value = values[(int)i];
                    switch (i)
                    {
                        case 0: _fixedData0 = value; break;
                        case 1: _fixedData1 = value; break;
                        case 2: _fixedData2 = value; break;
                        case 3: _fixedData3 = value; break;
                    }
                }
                Data = null;
            }
        }
    }
}

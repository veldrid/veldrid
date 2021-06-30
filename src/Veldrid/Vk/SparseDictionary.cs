using System;

namespace Veldrid.Vk
{
    internal class SparseDictionary<T1, T2>
    {
        private uint _count;
        private uint[] _dense;
        private uint[] _sparse;

        public T1[] Values1;
        public T2[] Values2;

        public uint Count => _count;

        public SparseDictionary(int baseCapacity)
        {
            _dense = new uint[baseCapacity];
            _sparse = new uint[baseCapacity];
            Values1 = new T1[baseCapacity];
            Values2 = new T2[baseCapacity];
        }

        private void Expand()
        {
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _sparse, newSize);
            Array.Resize(ref Values1, newSize);
            Array.Resize(ref Values2, newSize);
        }

        public void Add(uint key)
        {
            if (key >= _dense.Length)
            {
                Expand();
            }

            uint index = _count;
            _dense[index] = key;
            _sparse[key] = index;
            _count++;
        }

        public void Remove(uint key)
        {
            uint index = _sparse[key];
            uint lastKey = _dense[_count - 1];
            _dense[index] = lastKey;
            _sparse[lastKey] = index;
            _count--;
        }

        public bool Contains(uint key)
        {
            if (key >= _dense.Length)
            {
                return false;
            }

            uint index = _sparse[key];
            return index < _count && _dense[index] == key;
        }

        public void Clear()
        {
            _count = 0;
        }
    }
}

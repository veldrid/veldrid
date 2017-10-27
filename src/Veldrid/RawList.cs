using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if !VALIDATE
using System.Diagnostics;
#endif

// This is a copy of code from Veldrid.Collections. In the future, this code should be deleted from here.

namespace Veldrid
{
    /// <summary>
    /// A resizable, generic list which exposes direct access to its underlying array.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the list.</typeparam>
    public class RawList<T> : IEnumerable<T>
    {
        private T[] _items;
        private uint _count;

        public const uint DefaultCapacity = 4;
        private const float GrowthFactor = 2f;

        public RawList() : this(DefaultCapacity) { }

        public RawList(uint capacity)
        {
#if VALIDATE
            if (capacity > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
#else
            Debug.Assert(capacity <= int.MaxValue);
#endif
            _items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public uint Count
        {
            get => _count;
            set
            {
                Resize(value);
            }
        }


        public T[] Items => _items;

        public ArraySegment<T> ArraySegment => new ArraySegment<T>(_items, 0, (int)_count);

        public ref T this[uint index]
        {
            get
            {
                ValidateIndex(index);
                return ref _items[index];
            }
        }

        public ref T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return ref _items[index];
            }
        }

        public void Add(ref T item)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, (int)(_items.Length * GrowthFactor));
            }

            _items[_count] = item;
            _count += 1;
        }

        public void Add(T item)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, (int)(_items.Length * GrowthFactor));
            }

            _items[_count] = item;
            _count += 1;
        }

        public void AddRange(T[] items)
        {
#if VALIDATE
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
#else
            Debug.Assert(items != null);
#endif

            int requiredSize = (int)(_count + items.Length);
            if (requiredSize > _items.Length)
            {
                Array.Resize(ref _items, (int)(requiredSize * GrowthFactor));
            }

            Array.Copy(items, 0, _items, (int)_count, items.Length);
            _count += (uint)items.Length;
        }

        public void AddRange(IEnumerable<T> items)
        {
#if VALIDATE
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
#else
            Debug.Assert(items != null);
#endif

            foreach (T item in items)
            {
                Add(item);
            }
        }

        public void Replace(uint index, ref T item)
        {
            ValidateIndex(index);
            _items[index] = item;
        }

        public void Resize(uint count)
        {
            Array.Resize(ref _items, (int)count);
            _count = count;
        }

        public void Replace(uint index, T item) => Replace(index, ref item);

        public bool Remove(ref T item)
        {
            bool contained = GetIndex(item, out uint index);
            if (contained)
            {
                CoreRemoveAt(index);
            }

            return contained;
        }


        public bool Remove(T item)
        {
            bool contained = GetIndex(item, out uint index);
            if (contained)
            {
                CoreRemoveAt(index);
            }

            return contained;
        }

        public void RemoveAt(uint index)
        {
            ValidateIndex(index);
            CoreRemoveAt(index);
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _items.Length);
        }

        public bool GetIndex(T item, out uint index)
        {
            int signedIndex = Array.IndexOf(_items, item);
            index = (uint)signedIndex;
            return signedIndex != -1;
        }

        public void Sort() => Sort(null);

        public void Sort(IComparer<T> comparer)
        {
#if VALIDATE
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
#else
            Debug.Assert(comparer != null);
#endif
            Array.Sort(_items, comparer);
        }

        public void TransformAll(Func<T, T> transformation)
        {
#if VALIDATE
            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }
#else
            Debug.Assert(transformation != null);
#endif

            for (int i = 0; i < _count; i++)
            {
                _items[i] = transformation(_items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CoreRemoveAt(uint index)
        {
            _count -= 1;
            Array.Copy(_items, (int)index + 1, _items, (int)index, (int)(_count - index));
            _items[_count] = default(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateIndex(uint index)
        {
#if VALIDATE
            if (index >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
#else
            Debug.Assert(index < _count);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateIndex(int index)
        {
#if VALIDATE
            if (index < 0 || index >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
#else
            Debug.Assert(index >= 0 && index < _count);
#endif
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private RawList<T> _list;
            private uint _currentIndex;

            public Enumerator(RawList<T> list)
            {
                _list = list;
                _currentIndex = uint.MaxValue;
            }

            public T Current => _list._items[_currentIndex];
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _currentIndex += 1;
                return _currentIndex < _list._count;
            }

            public void Reset()
            {
                _currentIndex = 0;
            }

            public void Dispose() { }
        }
    }
}

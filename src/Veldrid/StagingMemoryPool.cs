using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    internal unsafe sealed class StagingMemoryPool : IDisposable
    {
        private readonly List<StagingBlock> _storage;
        private readonly SortedList<uint, uint> _availableBlocks;
        private object _lock = new object();

        public StagingMemoryPool()
        {
            _storage = new List<StagingBlock>();
            _availableBlocks = new SortedList<uint, uint>(new CapacityComparer());
        }

        public StagingBlock Stage(IntPtr source, uint sizeInBytes)
        {
            Rent(sizeInBytes, out StagingBlock block);
            Unsafe.CopyBlock(block.Data, source.ToPointer(), sizeInBytes);
            return block;
        }

        public StagingBlock Stage(byte[] bytes)
        {
            Rent((uint)bytes.Length, out StagingBlock block);
            Marshal.Copy(bytes, 0, (IntPtr)block.Data, bytes.Length);
            return block;
        }

        public StagingBlock GetStagingBlock(uint sizeInBytes)
        {
            Rent(sizeInBytes, out StagingBlock block);
            Unsafe.InitBlock(block.Data, 0, sizeInBytes);
            return block;
        }

        public StagingBlock RetrieveById(uint id)
        {
            return _storage[(int)id];
        }

        private void Rent(uint size, out StagingBlock block)
        {
            lock (_lock)
            {
                SortedList<uint, uint> available = _availableBlocks;
                IList<uint> indices = available.Values;
                for (int i = 0; i < available.Count; i++)
                {
                    int index = (int)indices[i];
                    StagingBlock current = _storage[index];
                    if (current.Capacity >= size)
                    {
                        available.RemoveAt(i);
                        current.SizeInBytes = size;
                        block = current;
                        _storage[index] = current;
                        return;
                    }
                }

                Allocate(size, out block);
            }
        }

        private void Allocate(uint sizeInBytes, out StagingBlock stagingBlock)
        {
            IntPtr ptr = Marshal.AllocHGlobal((int)sizeInBytes);
            uint id = (uint)_storage.Count;
            stagingBlock = new StagingBlock(id, (void*)ptr, sizeInBytes, this);
            _storage.Add(stagingBlock);
        }

        public void Free(StagingBlock block)
        {
            lock (_lock)
            {
                if (block.Id < _storage.Count)
                {
                    _availableBlocks.Add(block.Capacity, block.Id);
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _availableBlocks.Clear();
                foreach (StagingBlock block in _storage)
                {
                    Marshal.FreeHGlobal((IntPtr)block.Data);
                }
                _storage.Clear();
            }
        }

        private class CapacityComparer : IComparer<uint>
        {
            public int Compare(uint x, uint y)
            {
                return x >= y ? 1 : -1;
            }
        }
    }

    internal unsafe struct StagingBlock
    {
        public readonly uint Id;
        public readonly void* Data;
        public readonly uint Capacity;
        public uint SizeInBytes;
        public readonly StagingMemoryPool Pool;

        public StagingBlock(uint id, void* data, uint capacity, StagingMemoryPool pool)
        {
            Id = id;
            Data = data;
            Capacity = capacity;
            SizeInBytes = capacity;
            Pool = pool;
        }

        public void Free()
        {
            Pool.Free(this);
        }
    }
}

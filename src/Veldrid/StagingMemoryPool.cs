using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    internal unsafe class StagingMemoryPool
    {
        private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

        public StagingBlock Stage(IntPtr source, uint sizeInBytes)
        {
            byte[] array = _arrayPool.Rent((int)sizeInBytes);
            fixed (byte* arrayPtr = &array[0])
            {
                Debug.Assert(array.Length >= sizeInBytes);
                Unsafe.CopyBlock(arrayPtr, source.ToPointer(), sizeInBytes);
            }

            return new StagingBlock(array, sizeInBytes, this);
        }

        public StagingBlock Stage(byte[] bytes)
        {
            byte[] array = _arrayPool.Rent(bytes.Length);
            Array.Copy(bytes, array, bytes.Length);
            return new StagingBlock(array, (uint)bytes.Length, this);
        }

        public FixedStagingBlock GetFixedStagingBlock(uint sizeInBytes)
        {
            byte[] array = _arrayPool.Rent((int)sizeInBytes);
            return new FixedStagingBlock(array, sizeInBytes, this);
        }

        public void Free(StagingBlock block)
        {
            bool clearArray = false;
#if DEBUG
            clearArray = true;
#endif
            _arrayPool.Return(block.Array, clearArray);
        }

        public void Free(FixedStagingBlock block)
        {
            bool clearArray = false;
#if DEBUG
            clearArray = true;
#endif
            _arrayPool.Return(block.Array, clearArray);
        }

        public void Free(byte[] array)
        {
            bool clearArray = false;
#if DEBUG
            clearArray = true;
#endif
            _arrayPool.Return(array, clearArray);
        }
    }

    internal unsafe struct StagingBlock
    {
        public readonly byte[] Array;
        public readonly uint SizeInBytes;
        public readonly StagingMemoryPool Pool;

        public StagingBlock(byte[] array, uint sizeInBytes, StagingMemoryPool pool)
        {
            Debug.Assert(array != null);
            Debug.Assert(array.Length > 0);
            Debug.Assert(sizeInBytes > 0);
            Array = array;
            SizeInBytes = sizeInBytes;
            Pool = pool;
        }

        internal void Free()
        {
            Pool.Free(this);
        }
    }

    internal unsafe struct FixedStagingBlock
    {
        public readonly byte[] Array;
        public readonly uint SizeInBytes;
        public readonly StagingMemoryPool Pool;
        public readonly GCHandle GCHandle;
        public readonly void* Data;

        public FixedStagingBlock(byte[] array, uint sizeInBytes, StagingMemoryPool pool)
        {
            Debug.Assert(array != null);
            Debug.Assert(array.Length > 0);
            Debug.Assert(sizeInBytes > 0);
            Array = array;
            SizeInBytes = sizeInBytes;
            Pool = pool;
            GCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            Data = (void*)GCHandle.AddrOfPinnedObject();
        }

        internal void Free()
        {
            GCHandle.Free();
            Pool.Free(this);
        }
    }
}
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL
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

        public void Free(StagingBlock block)
        {
            _arrayPool.Return(block.Array);
        }

        public void Free(byte[] array)
        {
            _arrayPool.Return(array);
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

    internal unsafe struct HandleTrackedStagingBlock
    {
        public readonly GCHandle GCHandle;

        public byte[] Array => (byte[])GCHandle.Target;

        public uint SizeInBytes { get; }

        public HandleTrackedStagingBlock(StagingBlock stagingBlock)
        {
            GCHandle = GCHandle.Alloc(stagingBlock.Array, GCHandleType.Pinned);
            SizeInBytes = stagingBlock.SizeInBytes;
        }
    }
}
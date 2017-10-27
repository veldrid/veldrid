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
            //byte[] array = _arrayPool.Rent((int)sizeInBytes);
            byte[] array = new byte[(int)sizeInBytes];
            fixed (byte* arrayPtr = &array[0])
            {
                Unsafe.CopyBlock(arrayPtr, source.ToPointer(), sizeInBytes);
            }
            return new StagingBlock(array, sizeInBytes, this);
        }

        public StagingBlock Stage(byte[] bytes)
        {
            //byte[] array = _arrayPool.Rent(bytes.Length);
            byte[] array = new byte[bytes.Length];
            Array.Copy(bytes, array, bytes.Length);
            return new StagingBlock(array, (uint)bytes.Length, this);
        }

        public void Free(StagingBlock block)
        {
            //_arrayPool.Return(block.Array);
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
        public readonly StagingBlock StagingBlock;
        public GCHandle GCHandle;

        public HandleTrackedStagingBlock(StagingBlock stagingBlock)
        {
            StagingBlock = stagingBlock;
            GCHandle = GCHandle.Alloc(stagingBlock.Array, GCHandleType.Pinned);
        }
    }
}
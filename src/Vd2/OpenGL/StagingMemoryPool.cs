using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vd2.OpenGL
{
    internal unsafe class StagingMemoryPool
    {
        private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

        public StagingBlock Stage(IntPtr source, uint sizeInBytes)
        {
            byte[] array = _arrayPool.Rent((int)sizeInBytes);
            GCHandle gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            Unsafe.CopyBlock(gcHandle.AddrOfPinnedObject().ToPointer(), source.ToPointer(), sizeInBytes);
            return new StagingBlock(array, gcHandle, sizeInBytes, this);
        }

        public StagingBlock Stage(byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                return Stage((IntPtr)ptr, (uint)bytes.Length);
            }
        }

        public void Free(StagingBlock block)
        {
            block.GCHandle.Free();
            _arrayPool.Return(block.Array);
        }
    }

    internal unsafe struct StagingBlock
    {
        public readonly byte[] Array;
        public readonly GCHandle GCHandle;
        public readonly uint SizeInBytes;
        public readonly StagingMemoryPool Pool;
        public readonly void* Data;

        public StagingBlock(byte[] array, GCHandle gcHandle, uint sizeInBytes, StagingMemoryPool pool)
        {
            Array = array;
            GCHandle = gcHandle;
            SizeInBytes = sizeInBytes;
            Pool = pool;
            Data = GCHandle.AddrOfPinnedObject().ToPointer();
        }

        internal void Free()
        {
            Pool.Free(this);
        }
    }
}
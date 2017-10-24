using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vd2.OpenGL
{
    internal unsafe class StagingMemoryPool
    {
        public StagingBlock Stage(IntPtr source, uint sizeInBytes)
        {
            IntPtr data = Marshal.AllocHGlobal((int)sizeInBytes);
            Unsafe.CopyBlock(data.ToPointer(), source.ToPointer(), sizeInBytes);
            return new StagingBlock(data, sizeInBytes, this);
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
            Marshal.FreeHGlobal(block.Data);
        }
    }

    internal unsafe struct StagingBlock
    {
        public readonly IntPtr Data;
        public readonly uint SizeInBytes;
        public readonly StagingMemoryPool Pool;

        public StagingBlock(IntPtr data, uint sizeInBytes, StagingMemoryPool pool)
        {
            Data = data;
            SizeInBytes = sizeInBytes;
            Pool = pool;
        }

        internal void Free()
        {
            Pool.Free(this);
        }
    }
}
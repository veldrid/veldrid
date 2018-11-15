using System;

namespace Veldrid.MetalBindings
{
    public unsafe struct BlockLiteral
    {
        public IntPtr isa;
        public int flags;
        public int reserved;
        public IntPtr invoke;
        public BlockDescriptor* descriptor;
    };

    public unsafe struct BlockDescriptor
    {
        public ulong reserved;
        public ulong Block_size;
    }
}


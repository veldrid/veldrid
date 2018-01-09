using System;

namespace Veldrid.MetalBindings
{
    public struct MTLScissorRect
    {
        public UIntPtr x;
        public UIntPtr y;
        public UIntPtr width;
        public UIntPtr height;

        public MTLScissorRect(uint x, uint y, uint width, uint height)
        {
            this.x = (UIntPtr)x;
            this.y = (UIntPtr)y;
            this.width = (UIntPtr)width;
            this.height = (UIntPtr)height;
        }
    }
}
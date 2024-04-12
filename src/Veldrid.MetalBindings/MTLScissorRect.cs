using System;

namespace Veldrid.MetalBindings
{
    public struct MTLScissorRect : IEquatable<MTLScissorRect>
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

        public bool Equals(MTLScissorRect other)
            => x == other.x
               && y == other.y
               && width == other.width
               && height == other.height;
    }
}

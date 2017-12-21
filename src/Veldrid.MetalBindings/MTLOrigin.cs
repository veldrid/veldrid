using System;

namespace Veldrid.MetalBindings
{
    public struct MTLOrigin
    {
        public UIntPtr x;
        public UIntPtr y;
        public UIntPtr z;

        public MTLOrigin(uint x, uint y, uint z)
        {
            this.x = (UIntPtr)x;
            this.y = (UIntPtr)y;
            this.z = (UIntPtr)z;
        }
    }
}
using System;

namespace Veldrid.MetalBindings
{
    public struct NSRange
    {
        public UIntPtr location;
        public UIntPtr length;

        public NSRange(UIntPtr location, UIntPtr length)
        {
            this.location = location;
            this.length = length;
        }

        public NSRange(uint location, uint length)
        {
            this.location = (UIntPtr)location;
            this.length = (UIntPtr)length;
        }
    }
}
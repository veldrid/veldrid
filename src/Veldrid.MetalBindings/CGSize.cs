using System;
using System.Runtime.CompilerServices;

namespace Veldrid.MetalBindings
{
    public struct CGSize
    {
        public double width;
        public double height;

        public CGSize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
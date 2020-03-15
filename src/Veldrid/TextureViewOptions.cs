using System;

namespace Veldrid
{
    [Flags]
    public enum TextureViewOptions
    {
        None = 0,
        /// <summary>
        /// View covers only the stencil channel of the image, as R8_UInt.
        /// </summary>
        Stencil = 1 << 0,
    }
}

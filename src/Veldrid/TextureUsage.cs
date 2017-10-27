using System;

namespace Veldrid
{
    [Flags]
    public enum TextureUsage : byte
    {
        Sampled = 1 << 0,
        RenderTarget = 1 << 1,
        DepthStencil = 1 << 2,
    }
}

using System;

namespace Vd2.NeoDemo
{
    [Flags]
    public enum RenderPasses : byte
    {
        Standard = 1 << 0,
        AlphaBlend = 1 << 1,
        Overlay = 1 << 2,
        ShadowMap = 1 << 3,
    }
}

using System;

namespace Veldrid.NeoDemo
{
    [Flags]
    public enum RenderPasses : byte
    {
        Standard = 1 << 0,
        AlphaBlend = 1 << 1,
        Overlay = 1 << 2,
        ShadowMapNear = 1 << 3,
        ShadowMapMid = 1 << 4,
        ShadowMapFar = 1 << 5,
        AllShadowMap = ShadowMapNear | ShadowMapMid | ShadowMapFar,
    }
}

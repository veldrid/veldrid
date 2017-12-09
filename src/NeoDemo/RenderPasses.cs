using System;

namespace Veldrid.NeoDemo
{
    [Flags]
    public enum RenderPasses : int
    {
        Standard = 1 << 0,
        AlphaBlend = 1 << 1,
        Overlay = 1 << 2,
        ShadowMapNear = 1 << 3,
        ShadowMapMid = 1 << 4,
        ShadowMapFar = 1 << 5,
        Duplicator = 1 << 6,
        SwapchainOutput = 1 << 7,
        ReflectionMap = 1 << 8,
        AllShadowMap = ShadowMapNear | ShadowMapMid | ShadowMapFar,
    }
}

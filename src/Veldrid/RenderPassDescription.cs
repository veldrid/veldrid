using System;

namespace Veldrid
{
    internal readonly struct RenderPassDescription
    {
        public readonly Framebuffer Framebuffer;
        public readonly StoreAction StoreAction;
        public readonly LoadAction LoadAction;
        public readonly RgbaFloat ClearColor;
        public readonly float ClearDepth;
        public readonly SmallFixedOrDynamicArray<Texture> ResolveTextures;

        public RenderPassDescription(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth,
            Span<Texture> resolveTextures)
        {
            Framebuffer = framebuffer;
            LoadAction = loadAction;
            StoreAction = storeAction;
            ClearColor = clearColor;
            ClearDepth = clearDepth;
            ResolveTextures = new SmallFixedOrDynamicArray<Texture>(resolveTextures);
        }
    }
}

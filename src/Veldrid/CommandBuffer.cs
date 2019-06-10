using System;

namespace Veldrid
{
    public abstract class CommandBuffer : IDisposable
    {
        public abstract void Dispose();

        public void BeginRenderPass(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth)
            => BeginRenderPassCore(
                new RenderPassDescription(framebuffer, loadAction, storeAction, clearColor, clearDepth, Span<Texture>.Empty));

        public void BeginRenderPass(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth,
            Span<Texture> resolveTextures)
            => BeginRenderPassCore(
                new RenderPassDescription(framebuffer, loadAction, storeAction, clearColor, clearDepth, resolveTextures));

        internal abstract void BeginRenderPassCore(in RenderPassDescription rpi);
        public abstract void BindVertexBuffer(uint index, DeviceBuffer buffer, uint offset);
        public abstract void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset);
        public abstract void BindPipeline(Pipeline pipeline);
        public void BindResourceSet(uint slot, ResourceSet resourceSet)
            => BindResourceSet(slot, resourceSet, Span<uint>.Empty);
        public abstract void BindResourceSet(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets);
        public void DrawIndexed(uint indexCount) => DrawIndexed(indexCount, 1, 0, 0, 0);
        public abstract void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart);
        public void Draw(uint vertexCount) => Draw(vertexCount, 1, 0, 0);
        public abstract void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart);
        public abstract void SetViewport(uint index, Viewport viewport);
        public abstract void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        public abstract void EndRenderPass();

        public abstract void InsertDebugMarker(string name);
        public abstract void PushDebugGroup(string name);
        public abstract void PopDebugGroup();
    }

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

    public enum LoadAction
    {
        DontCare,
        Load,
        Clear,
    }

    public enum StoreAction
    {
        DontCare,
        Store,
        Resolve,
        StoreAndResolve
    }
}

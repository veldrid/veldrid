using System;

namespace Veldrid
{
    public abstract class CommandBuffer : IDisposable
    {
        public abstract void Dispose();

        public RenderEncoder BeginRenderPass(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth)
            => BeginRenderPassCore(
                new RenderPassDescription(framebuffer, loadAction, storeAction, clearColor, clearDepth, Span<Texture>.Empty));

        public RenderEncoder BeginRenderPass(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth,
            Span<Texture> resolveTextures)
            => BeginRenderPassCore(
                new RenderPassDescription(framebuffer, loadAction, storeAction, clearColor, clearDepth, resolveTextures));

        internal abstract RenderEncoder BeginRenderPassCore(in RenderPassDescription rpi);
        internal abstract void BindVertexBuffer(uint index, DeviceBuffer buffer, uint offset);
        internal abstract void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset);
        internal abstract void BindPipeline(Pipeline pipeline);
        internal void BindResourceSet(uint slot, ResourceSet resourceSet)
            => BindResourceSet(slot, resourceSet, Span<uint>.Empty);
        internal abstract void BindResourceSet(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets);
        internal abstract void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart);
        internal abstract void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart);
        internal abstract void SetViewport(uint index, Viewport viewport);
        internal abstract void SetScissorRect(uint index, uint x, uint y, uint width, uint height);

        internal abstract void InsertDebugMarker(string name);
        internal abstract void PushDebugGroup(string name);
        internal abstract void PopDebugGroup();

        internal abstract void End(RenderEncoder renderEncoder);
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

    public readonly struct RenderEncoder
    {
        private readonly CommandBuffer _cb;
        private readonly RenderPassDescription _rpDesc;

        internal RenderEncoder(CommandBuffer cb, in RenderPassDescription rpDesc)
        {
            _cb = cb;
            _rpDesc = rpDesc;
        }

        public void BindVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
            => _cb.BindVertexBuffer(index, buffer, offset);
        public void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
            => _cb.BindIndexBuffer(buffer, format, offset);
        public void BindPipeline(Pipeline pipeline) => _cb.BindPipeline(pipeline);
        public void BindResourceSet(uint slot, ResourceSet resourceSet)
            => _cb.BindResourceSet(slot, resourceSet);
        public void BindResourceSet(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
            => _cb.BindResourceSet(slot, resourceSet, dynamicOffsets);
        public void DrawIndexed(uint indexCount) => DrawIndexed(indexCount, 1, 0, 0, 0);
        public void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart)
            => _cb.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        public void Draw(uint vertexCount) => Draw(vertexCount, 1, 0, 0);
        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
            => _cb.Draw(vertexCount, instanceCount, vertexStart, instanceStart);
        public void SetViewport(uint index, Viewport viewport) => _cb.SetViewport(index, viewport);
        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
            => _cb.SetScissorRect(index, x, y, width, height);
        public void End() => _cb.End(this);

        public void InsertDebugMarker(string name) => _cb.InsertDebugMarker(name);
        public void PushDebugGroup(string name) => _cb.PushDebugGroup(name);
        public void PopDebugGroup() => _cb.PopDebugGroup();
    }
}

using System;

namespace Veldrid.OpenGL
{
    internal abstract class GLCommandExecutor
    {
        public abstract void Begin();
        public abstract void ClearColorTarget(uint index, RgbaFloat clearColor);
        public abstract void ClearDepthStencil(float depth, byte stencil);
        public abstract void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes);
        public abstract void CopyTexture(Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer, Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer, uint width, uint height, uint depth, uint layerCount);
        public abstract void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
        public abstract void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset);
        public abstract void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart);
        public abstract void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);
        public abstract void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride);
        public abstract void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride);
        public abstract void End();
        public abstract void GenerateMipmaps(Texture texture);
        public abstract void InsertDebugMarker(string name);
        public abstract void BlitTexture(
            Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight,
            Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight,
            bool linearFilter);
        public abstract void PopDebugGroup();
        public abstract void PushDebugGroup(string name);
        public abstract void ResolveTexture(Texture source, Texture destination);
        public abstract void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets);
        public abstract void SetFramebuffer(Framebuffer fb);
        public abstract void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets);
        public abstract void SetIndexBuffer(DeviceBuffer ib, IndexFormat format, uint offset);
        public abstract void SetPipeline(Pipeline pipeline);
        public abstract void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        public abstract void SetVertexBuffer(uint index, DeviceBuffer vb, uint offset);
        public abstract void SetViewport(uint index, ref Viewport viewport);
        public abstract void SetViewport(uint index, Viewport viewport);
        public abstract void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr dataPtr, uint sizeInBytes);
        public abstract void MemoryBarrier(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage);
        public abstract void EndRenderPass();
        public abstract void BeginRenderPass(in RenderPassDescription rpd);
    }
}

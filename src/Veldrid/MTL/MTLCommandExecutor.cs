using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Veldrid.CommandRecording;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal unsafe class MTLCommandExecutor : RecordedCommandExecutor, IDisposable
    {
        private readonly MTLGraphicsDevice _gd;
        private readonly MTLCommandBuffer _cb;

        public MTLCommandBuffer CommandBuffer => _cb;

        public MTLCommandExecutor(MTLGraphicsDevice gd)
        {
            _gd = gd;
            _cb = (MTLCommandBuffer)gd.ResourceFactory.CreateCommandBuffer();
        }

        public override void Begin()
        {
            throw new NotSupportedException();
        }

        internal MetalBindings.MTLCommandBuffer PrepareForSubmission()
        {
            return _cb.PrepareForSubmission();
        }

        public override void BeginRenderPass(in RenderPassDescription rpd) => _cb.BeginRenderPass(rpd);

        public override void EndRenderPass() => _cb.EndRenderPass();
        public override void BlitTexture(
            Texture source,
            uint srcX, uint srcY,
            uint srcWidth, uint srcHeight,
            Framebuffer destination,
            uint dstX, uint dstY,
            uint dstWidth, uint dstHeight,
            bool linearFilter)
            => _cb.BlitTexture(
                source,
                srcX, srcY,
                srcWidth, srcHeight,
                destination,
                dstX, dstY,
                dstWidth, dstHeight,
                linearFilter);

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            throw new NotSupportedException();
        }

        public override void ClearDepthStencil(float depth, byte stencil)
        {
            throw new NotSupportedException();
        }

        public override void CopyBuffer(
            DeviceBuffer source,
            uint sourceOffset,
            DeviceBuffer destination,
            uint destinationOffset,
            uint sizeInBytes)
            => _cb.CopyBuffer(source, sourceOffset, destination, destinationOffset, sizeInBytes);

        public override void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel, uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
            => _cb.CopyTexture(
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth, layerCount);

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
            => _cb.Dispatch(groupCountX, groupCountY, groupCountZ);

        public override void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
            => _cb.DispatchIndirect(indirectBuffer, offset);

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
            => _cb.Draw(vertexCount, instanceCount, vertexStart, instanceStart);

        public override void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart)
            => _cb.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);

        public override void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
            => _cb.DrawIndexedIndirect(indirectBuffer, offset, drawCount, stride);

        public override void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
            => _cb.DrawIndirect(indirectBuffer, offset, drawCount, stride);

        public override void End()
        {
            throw new NotSupportedException();
        }

        public override void GenerateMipmaps(Texture texture) => _cb.GenerateMipmaps(texture);

        public override void InsertDebugMarker(string name) => _cb.InsertDebugMarker(name);

        public override void MemoryBarrier(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage, ShaderStages destinationStage)
            => _cb.MemoryBarrier(
                texture,
                baseMipLevel, levelCount,
                baseArrayLayer, layerCount,
                sourceStage, destinationStage);

        public override void PopDebugGroup() => _cb.PopDebugGroup();

        public override void PushDebugGroup(string name) => _cb.PushDebugGroup(name);

        public override void ResolveTexture(Texture source, Texture destination)
        {
            throw new NotSupportedException();
        }

        public override void SetComputeResourceSet(
            uint slot, ResourceSet rs,
            uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
        }

        public override void SetFramebuffer(Framebuffer fb)
        {
            throw new NotSupportedException();
        }

        public override void SetGraphicsResourceSet(
            uint slot,
            ResourceSet rs,
            uint dynamicOffsetCount,
            ref uint dynamicOffsets)
            => _cb.BindGraphicsResourceSetCore(slot, rs, dynamicOffsetCount, ref dynamicOffsets);

        public override void SetIndexBuffer(DeviceBuffer ib, IndexFormat format, uint offset)
            => _cb.BindIndexBuffer(ib, format, offset);

        public override void SetPipeline(Pipeline pipeline) => _cb.BindPipeline(pipeline);

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
            => _cb.SetScissorRect(index, x, y, width, height);

        public override void SetViewport(uint index, ref Viewport viewport)
            => _cb.SetViewport(index, viewport);

        public override void UpdateBuffer(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr dataPtr,
            uint sizeInBytes)
            => _cb.UpdateBuffer(buffer, bufferOffsetInBytes, dataPtr, sizeInBytes);

        public override void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
            => _cb.BindVertexBuffer(index, buffer, offset);

        public void Dispose()
        {
            _cb.Dispose();
        }
    }
}

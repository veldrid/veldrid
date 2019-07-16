using System;
using System.Diagnostics;
using Veldrid.CommandRecording;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal unsafe class MTLCommandExecutor : RecordedCommandExecutor
    {
        private readonly MTLGraphicsDevice _gd;

        public MTLCommandExecutor(MTLGraphicsDevice gd)
        {
            _gd = gd;
        }

        public override void Begin()
        {
            throw new NotSupportedException();
        }

        public override void BeginRenderPass(in RenderPassDescription rpd)
        {
            throw new NotImplementedException();
        }

        public override void EndRenderPass()
        {
            throw new NotImplementedException();
        }

        public override void BlitTexture(Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight, Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight, bool linearFilter)
        {
            throw new NotImplementedException();
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            throw new NotSupportedException();
        }

        public override void ClearDepthStencil(float depth, byte stencil)
        {
            throw new NotSupportedException();
        }

        public override void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public override void CopyTexture(Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer, Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer, uint width, uint height, uint depth, uint layerCount)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            throw new NotImplementedException();
        }

        public override void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            throw new NotImplementedException();
        }

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            throw new NotImplementedException();
        }

        public override void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            throw new NotImplementedException();
        }

        public override void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            throw new NotImplementedException();
        }

        public override void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            throw new NotImplementedException();
        }

        public override void End()
        {
            throw new NotSupportedException();
        }

        public override void GenerateMipmaps(Texture texture)
        {
            throw new NotImplementedException();
        }

        public override void InsertDebugMarker(string name)
        {
            throw new NotImplementedException();
        }

        public override void MemoryBarrier(Texture texture, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ShaderStages sourceStage, ShaderStages destinationStage)
        {
            throw new NotImplementedException();
        }

        public override void PopDebugGroup()
        {
            throw new NotImplementedException();
        }

        public override void PushDebugGroup(string name)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(Texture source, Texture destination)
        {
            throw new NotSupportedException();
        }

        public override void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            throw new NotImplementedException();
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
        {
            throw new NotImplementedException();
        }

        public void SetGraphicsResourceSet(
            uint slot,
            ResourceSet rs,
            Span<uint> dynamicOffsets)
        {
            throw new NotImplementedException();
        }

        public override void SetIndexBuffer(DeviceBuffer ib, IndexFormat format, uint offset)
        {
            throw new NotImplementedException();
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            throw new NotImplementedException();
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            throw new NotImplementedException();
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr dataPtr, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public override void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using static WebGLDotNET.WebGLRenderingContextBase;

namespace Veldrid.WebGL
{
    internal class WebGLCommandBuffer : CommandBuffer
    {
        private readonly WebGLGraphicsDevice _gd;
        private RgbaFloat _clearColor;

        public WebGLCommandBuffer(WebGLGraphicsDevice gd, ref CommandBufferDescription description)
            : base(gd.Features)
        {
            _gd = gd;
        }

        public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void InsertDebugMarker(string name)
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

        internal override void BeginRenderPassCore(in RenderPassDescription rpi)
        {
            _clearColor = rpi.ClearColor;
        }

        internal override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            throw new NotImplementedException();
        }

        private protected override void BindComputeResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            throw new NotImplementedException();
        }

        private protected override void BindGraphicsResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            throw new NotImplementedException();
        }

        private protected override void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            throw new NotImplementedException();
        }

        private protected override void BindPipelineCore(Pipeline pipeline)
        {
            throw new NotImplementedException();
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            throw new NotImplementedException();
        }

        private protected override void CopyBufferCore(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }

        private protected override void CopyTextureCore(Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer, Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer, uint width, uint height, uint depth, uint layerCount)
        {
            throw new NotImplementedException();
        }

        private protected override void DispatchCore(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            throw new NotImplementedException();
        }

        private protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            throw new NotImplementedException();
        }

        private protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            throw new NotImplementedException();
        }

        private protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            throw new NotImplementedException();
        }

        private protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            throw new NotImplementedException();
        }

        private protected override void EndRenderPassCore()
        {
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            throw new NotImplementedException();
        }

        private protected override void MemoryBarrierCore(ShaderStages sourceStage, ShaderStages destinationStage)
        {
            throw new NotImplementedException();
        }

        internal void ExecuteAll()
        {
            _gd.Ctx.ClearColor(_clearColor.R, _clearColor.G, _clearColor.B, _clearColor.A);
            _gd.Ctx.Clear(COLOR_BUFFER_BIT);
        }

        private protected override void MemoryBarrierCore(Texture texture, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ShaderStages sourceStage, ShaderStages destinationStage)
        {
            throw new NotImplementedException();
        }

        private protected override void SetScissorRectCore(uint index, uint x, uint y, uint width, uint height)
        {
            throw new NotImplementedException();
        }

        private protected override void SetViewportCore(uint index, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        private protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }
    }
}

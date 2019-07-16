using System;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLCommandBuffer : CommandBuffer
    {
        private readonly MTLGraphicsDevice _gd;
        private State _recordingState = State.Initial;
        private MetalBindings.MTLCommandBuffer _cb;
        private MTLRenderCommandEncoder _rce;

        public MTLCommandBuffer(MTLGraphicsDevice gd, ref CommandBufferDescription desc)
            : base(gd.Features)
        {
            Debug.Assert((desc.Flags & CommandBufferFlags.Reusable) == 0);
            _gd = gd;
        }

        public override string Name { get; set; }

        private void BeginRecording()
        {
            if (_recordingState == State.Executing)
            {
                throw new VeldridException("Cannot record new commands into CommandBuffer while it is executing.");
            }

            if (_recordingState == State.Disposed)
            {
                throw new VeldridException("Cannot record new commands into a disposed CommandBuffer.");
            }

            if (_recordingState != State.Recording)
            {
                if (_cb.NativePtr != IntPtr.Zero)
                {
                    ObjectiveCRuntime.release(_cb.NativePtr);
                }

                using (NSAutoreleasePool.Begin())
                {
                    _cb = _gd.CommandQueue.commandBuffer();
                    ObjectiveCRuntime.retain(_cb.NativePtr);
                }

                _recordingState = State.Recording;
            }
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
            BeginRecording();

            MTLFramebufferBase mtlFB = Util.AssertSubtype<Framebuffer, MTLFramebufferBase>(rpi.Framebuffer);
            MTLRenderPassDescriptor descriptor = mtlFB.CreateRenderPassDescriptor(rpi);
            _rce = _cb.renderCommandEncoderWithDescriptor(descriptor);
        }

        private protected override void EndRenderPassCore()
        {
            _rce.endEncoding();
            ObjectiveCRuntime.release(_rce.NativePtr);
            _rce = default;
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
            MTLPipeline mtlPipeline = Util.AssertSubtype<Pipeline, MTLPipeline>(pipeline);
            if (pipeline.IsComputePipeline)
            {
                throw new NotImplementedException();
            }
            else
            {
                _rce.setRenderPipelineState(mtlPipeline.RenderPipelineState);
            }
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            throw new NotImplementedException();
        }

        private protected override void BlitTextureCore(Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight, Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight, bool linearFilter)
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

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            throw new NotImplementedException();
        }

        private protected override void MemoryBarrierCore(ShaderStages sourceStage, ShaderStages destinationStage)
        {
            throw new NotImplementedException();
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

        private enum State
        {
            Initial,
            Recording,
            ReadyToExecute,
            Executing,
            Disposed,
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
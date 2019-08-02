using System;
using System.Collections.Generic;
using Veldrid.CommandRecording;

namespace Veldrid.OpenGL
{
    internal class OpenGLCommandBuffer : ReusableCommandBuffer
    {
        private readonly List<ResourceRefCount> _refCounts = new List<ResourceRefCount>();

        public OpenGLCommandBuffer(OpenGLGraphicsDevice gd, ref CommandBufferDescription description)
            : base(gd.Features)
        {
        }

        public override void SubmittedToGraphicsDevice()
        {
            foreach (ResourceRefCount refCount in _refCounts)
            {
                refCount.Increment();
            }
        }

        public override void EndExecuting()
        {
            foreach (ResourceRefCount refCount in _refCounts)
            {
                refCount.Decrement();
            }
            base.EndExecuting();
        }

        protected override void RecordingStarted()
        {
            _refCounts.Clear();
        }

        internal override void BeginRenderPassCore(in RenderPassDescription rpd)
        {
            if (rpd.Framebuffer is OpenGLFramebuffer glFB)
            {
                _refCounts.Add(glFB.RefCount);
            }
            else
            {
                OpenGLSwapchainFramebuffer glSCFB = (OpenGLSwapchainFramebuffer)rpd.Framebuffer;
                _refCounts.Add(glSCFB.RefCount);
            }
            base.BeginRenderPassCore(rpd);
        }

        private protected override void BindComputeResourceSetCore(
            uint slot,
            ResourceSet resourceSet,
            Span<uint> dynamicOffsets)
        {
            OpenGLResourceSet glRS = (OpenGLResourceSet)resourceSet;
            foreach (ResourceRefCount refCount in glRS.RefCounts)
            {
                _refCounts.Add(refCount);
            }

            base.BindComputeResourceSetCore(slot, resourceSet, dynamicOffsets);
        }

        private protected override void BindGraphicsResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            OpenGLResourceSet glRS = (OpenGLResourceSet)resourceSet;
            foreach (ResourceRefCount refCount in glRS.RefCounts)
            {
                _refCounts.Add(refCount);
            }

            base.BindGraphicsResourceSetCore(slot, resourceSet, dynamicOffsets);
        }

        private protected override void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            _refCounts.Add(((OpenGLBuffer)buffer).RefCount);
            base.BindIndexBufferCore(buffer, format, offset);
        }

        private protected override void BindPipelineCore(Pipeline pipeline)
        {
            _refCounts.Add(((OpenGLPipeline)pipeline).RefCount);
            base.BindPipelineCore(pipeline);
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            _refCounts.Add(((OpenGLBuffer)buffer).RefCount);
            base.BindVertexBufferCore(index, buffer, offset);
        }

        private protected override void BlitTextureCore(
            Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight,
            Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight,
            bool linearFilter)
        {
            _refCounts.Add(((OpenGLTexture)source).RefCount);
            _refCounts.Add(((OpenGLFramebuffer)destination).RefCount);
            base.BlitTextureCore(source, srcX, srcY, srcWidth, srcHeight, destination, dstX, dstY, dstWidth, dstHeight, linearFilter);
        }

        private protected override void CopyBufferCore(
            DeviceBuffer source, uint sourceOffset,
            DeviceBuffer destination, uint destinationOffset,
            uint sizeInBytes)
        {
            _refCounts.Add(((OpenGLBuffer)source).RefCount);
            _refCounts.Add(((OpenGLBuffer)destination).RefCount);
            base.CopyBufferCore(source, sourceOffset, destination, destinationOffset, sizeInBytes);
        }

        private protected override void CopyTextureCore(
            Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer,
            Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth, uint layerCount)
        {
            _refCounts.Add(((OpenGLTexture)source).RefCount);
            _refCounts.Add(((OpenGLTexture)destination).RefCount);
            base.CopyTextureCore(
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth, layerCount);
        }

        private protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            _refCounts.Add(((OpenGLBuffer)indirectBuffer).RefCount);
            base.DispatchIndirectCore(indirectBuffer, offset);
        }

        private protected override void DrawIndexedIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride)
        {
            _refCounts.Add(((OpenGLBuffer)indirectBuffer).RefCount);
            base.DrawIndexedIndirectCore(indirectBuffer, offset, drawCount, stride);
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            _refCounts.Add(((OpenGLTexture)texture).RefCount);
            base.GenerateMipmapsCore(texture);
        }

        private protected override void DrawIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride)
        {
            _refCounts.Add(((OpenGLBuffer)indirectBuffer).RefCount);
            base.DrawIndirectCore(indirectBuffer, offset, drawCount, stride);
        }

        private protected override void MemoryBarrierCore(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage)
        {
            _refCounts.Add(((OpenGLTexture)texture).RefCount);
            base.MemoryBarrierCore(
                texture,
                baseMipLevel, levelCount,
                baseArrayLayer, layerCount,
                sourceStage,
                destinationStage);
        }

        private protected override void UpdateBufferCore(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes)
        {
            _refCounts.Add(((OpenGLBuffer)buffer).RefCount);
            base.UpdateBufferCore(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }
    }
}

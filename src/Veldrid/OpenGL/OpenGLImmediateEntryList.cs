using System;
using System.Collections.Generic;
using System.Text;

namespace Veldrid.OpenGL
{
    internal class OpenGLImmediateEntryList : OpenGLCommandEntryList
    {
        public OpenGLCommandList Parent { get; }

        private readonly OpenGLCommandExecutor _executor;

        public OpenGLImmediateEntryList(OpenGLCommandList cl, OpenGLCommandExecutor executor)
        {
            Parent = cl;
            _executor = executor;
        }

        public void Begin() { }

        public void ClearColorTarget(uint index, RgbaFloat clearColor) => _executor.ClearColorTarget(index, clearColor);

        public void ClearDepthTarget(float depth, byte stencil) => _executor.ClearDepthStencil(depth, stencil);

        public void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
            => _executor.CopyBuffer(source, sourceOffset, destination, destinationOffset, sizeInBytes);

        public void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel, uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height,
            uint depth, uint layerCount)
            => _executor.CopyTexture(
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth, layerCount);

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
            => _executor.Dispatch(groupCountX, groupCountY, groupCountZ);

        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
            => _executor.DispatchIndirect(indirectBuffer, offset);

        public void Dispose() { }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
            => _executor.Draw(vertexCount, instanceCount, vertexStart, instanceStart);

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
            => _executor.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);

        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
            => _executor.DrawIndexedIndirect(indirectBuffer, offset, drawCount, stride);

        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
            => _executor.DrawIndirect(indirectBuffer, offset, drawCount, stride);

        public void End() { }

        public void ExecuteAll(OpenGLCommandExecutor executor) { }

        public void Reset() { }

        public void ResolveTexture(Texture source, Texture destination) => _executor.ResolveTexture(source, destination);

        public void SetComputeResourceSet(uint slot, ResourceSet rs) => _executor.SetComputeResourceSet(slot, rs);

        public void SetFramebuffer(Framebuffer fb) => _executor.SetFramebuffer(fb);

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs) => _executor.SetGraphicsResourceSet(slot, rs);

        public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format) => _executor.SetIndexBuffer(buffer, format);

        public void SetPipeline(Pipeline pipeline) => _executor.SetPipeline(pipeline);

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
            => _executor.SetScissorRect(index, x, y, width, height);

        public void SetVertexBuffer(uint index, DeviceBuffer buffer) => _executor.SetVertexBuffer(index, buffer);

        public void SetViewport(uint index, ref Viewport viewport) => _executor.SetViewport(index, ref viewport);

        public void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
            => _executor.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);

        public void GenerateMipmaps(Texture texture) => _executor.GenerateMipmaps(texture);
    }
}

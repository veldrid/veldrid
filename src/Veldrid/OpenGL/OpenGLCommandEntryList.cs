using System;

namespace Veldrid.OpenGL
{
    internal interface OpenGLCommandEntryList
    {
        OpenGLCommandList Parent { get; }
        void Begin();
        void ClearColorTarget(uint index, RgbaFloat clearColor);
        void ClearDepthTarget(float depth, byte stencil);
        void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart);
        void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);
        void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride);
        void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride);
        void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
        void End();
        void SetFramebuffer(Framebuffer fb);
        void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset);
        void SetPipeline(Pipeline pipeline);
        void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets);
        void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets);
        void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset);
        void SetViewport(uint index, ref Viewport viewport);
        void ResolveTexture(Texture source, Texture destination);
        void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes);
        void ExecuteAll(OpenGLCommandExecutor executor);
        void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset);
        void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes);
        void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount);

        void GenerateMipmaps(Texture texture);
        void PushDebugGroup(string name);
        void PopDebugGroup();
        void InsertDebugMarker(string name);

        void Reset();
        void Dispose();
    }
}

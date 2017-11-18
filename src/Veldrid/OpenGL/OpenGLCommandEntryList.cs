using System;

namespace Veldrid.OpenGL
{
    internal interface OpenGLCommandEntryList
    {
        void Begin();
        void ClearColorTarget(uint index, RgbaFloat clearColor);
        void ClearDepthTarget(float depth);
        void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart);
        void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);
        void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
        void End();
        void SetFramebuffer(Framebuffer fb);
        void SetIndexBuffer(Buffer buffer, IndexFormat format);
        void SetPipeline(Pipeline pipeline);
        void SetGraphicsResourceSet(uint slot, ResourceSet rs);
        void SetComputeResourceSet(uint slot, ResourceSet rs);
        void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        void SetVertexBuffer(uint index, Buffer buffer);
        void SetViewport(uint index, ref Viewport viewport);
        void ResolveTexture(Texture source, Texture destination);
        void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes);
        void UpdateTexture(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer);
        void UpdateTextureCube(
            Texture textureCube,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer);
        void ExecuteAll(OpenGLCommandExecutor executor);
        void Reset();
    }
}
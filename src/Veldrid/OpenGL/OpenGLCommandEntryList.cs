using System;

namespace Veldrid.OpenGL
{
    internal interface OpenGLCommandEntryList
    {
        void Begin();
        void ClearColorTarget(uint index, RgbaFloat clearColor);
        void ClearDepthTarget(float depth);
        void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);
        void End();
        void SetFramebuffer(Framebuffer fb);
        void SetIndexBuffer(IndexBuffer ib);
        void SetPipeline(Pipeline pipeline);
        void SetResourceSet(uint slot, ResourceSet rs);
        void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        void SetVertexBuffer(uint index, VertexBuffer vb);
        void SetViewport(uint index, ref Viewport viewport);
        void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes);
        void UpdateTexture2D(Texture2D texture2D, IntPtr source, uint sizeInBytes, uint x, uint y, uint width, uint height, uint mipLevel, uint arrayLayer);
        void UpdateTextureCube(TextureCube textureCube, IntPtr source, uint sizeInBytes, CubeFace face, uint x, uint y, uint width, uint height, uint mipLevel, uint arrayLayer);

        void ExecuteAll(OpenGLCommandExecutor executor);
        void Reset();
    }
}
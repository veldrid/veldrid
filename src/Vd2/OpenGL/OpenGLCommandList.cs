using System;

namespace Vd2.OpenGL
{
    internal class OpenGLCommandList : CommandList
    {
        private readonly OpenGLCommandEntryList _commands = new OpenGLCommandEntryList();

        public OpenGLCommandEntryList Commands => _commands;

        public OpenGLCommandList(ref CommandListDescription description) : base(ref description)
        {

        }

        public override void Begin()
        {
            _commands.Begin();
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            _commands.ClearColorTarget(index, clearColor);
        }

        public override void ClearDepthTarget(float depth)
        {
            _commands.ClearDepthTarget(depth);
        }

        public override void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _commands.Draw(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        public override void End()
        {
            _commands.End();
        }

        public override void SetFramebuffer(Framebuffer fb)
        {
            _commands.SetFramebuffer(fb);
        }

        public override void SetIndexBuffer(IndexBuffer ib)
        {
            _commands.SetIndexBuffer(ib);
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            _commands.SetPipeline(pipeline);
        }

        public override void SetResourceSet(ResourceSet rs)
        {
            _commands.SetResourceSet(rs);
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _commands.SetScissorRect(index, x, y, width, height);
        }

        public override void SetVertexBuffer(uint index, VertexBuffer vb)
        {
            _commands.SetVertexBuffer(index, vb);
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            _commands.SetViewport(index, ref viewport);
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            _commands.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        public override void UpdateTexture2D(
            Texture2D texture2D,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            _commands.UpdateTexture2D(texture2D, source, sizeInBytes, x, y, width, height, mipLevel, arrayLayer);
        }

        public override void UpdateTextureCube(
            TextureCube textureCube,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            _commands.UpdateTextureCube(textureCube, source, sizeInBytes, face, x, y, width, height, mipLevel, arrayLayer);
        }

        public override void Dispose()
        {
        }
    }
}
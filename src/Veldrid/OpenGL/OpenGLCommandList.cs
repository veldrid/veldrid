using System;
using Veldrid.OpenGL.NoAllocEntryList;

namespace Veldrid.OpenGL
{
    internal class OpenGLCommandList : CommandList
    {
        private readonly OpenGLCommandEntryList _commands = new OpenGLNoAllocCommandEntryList();

        internal OpenGLCommandEntryList Commands => _commands;

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

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            _commands.Draw(vertexCount, instanceCount, vertexStart, instanceStart);
        }

        public override void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _commands.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            _commands.Dispatch(groupCountX, groupCountY, groupCountZ);
        }

        protected override void ResolveTextureCore(Texture source, Texture destination)
        {
            _commands.ResolveTexture(source, destination);
        }

        public override void End()
        {
            _commands.End();
        }

        protected override void SetFramebufferCore(Framebuffer fb)
        {
            _commands.SetFramebuffer(fb);
        }

        protected override void SetIndexBufferCore(Buffer buffer, IndexFormat format)
        {
            _commands.SetIndexBuffer(buffer, format);
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            _commands.SetPipeline(pipeline);
        }

        protected override void SetGraphicsResourceSetCore(uint slot, ResourceSet rs)
        {
            _commands.SetGraphicsResourceSet(slot, rs);
        }

        protected override void SetComputeResourceSetCore(uint slot, ResourceSet rs)
        {
            _commands.SetComputeResourceSet(slot, rs);
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _commands.SetScissorRect(index, x, y, width, height);
        }

        protected override void SetVertexBufferCore(uint index, Buffer buffer)
        {
            _commands.SetVertexBuffer(index, buffer);
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            _commands.SetViewport(index, ref viewport);
        }

        internal void Reset()
        {
            _commands.Reset();
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            _commands.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        public override void UpdateTexture(
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
            uint arrayLayer)
        {
            _commands.UpdateTexture(texture, source, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
        }

        public override void UpdateTextureCube(
            Texture textureCube,
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
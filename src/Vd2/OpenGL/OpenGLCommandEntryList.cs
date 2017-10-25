using System;
using System.Collections.Generic;

namespace Vd2.OpenGL
{
    internal class OpenGLCommandEntryList
    {
        private readonly List<OpenGLCommandEntry> _commands = new List<OpenGLCommandEntry>();
        private readonly StagingMemoryPool _memoryPool = new StagingMemoryPool();

        public IReadOnlyList<OpenGLCommandEntry> Commands => _commands;

        internal void Reset()
        {
            _commands.Clear();
        }

        internal void Begin()
        {
            _commands.Add(new BeginEntry());
        }

        internal void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            _commands.Add(new ClearColorTargetEntry(index, clearColor));
        }

        internal void ClearDepthTarget(float depth)
        {
            _commands.Add(new ClearDepthTargetEntry(depth));
        }

        internal void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _commands.Add(new DrawEntry(indexCount, instanceCount, indexStart, vertexOffset, instanceStart));
        }

        internal void End()
        {
            _commands.Add(new EndEntry());
        }

        internal void SetFramebuffer(Framebuffer fb)
        {
            _commands.Add(new SetFramebufferEntry(fb));
        }

        internal void SetIndexBuffer(IndexBuffer ib)
        {
            _commands.Add(new SetIndexBufferEntry(ib));
        }

        internal void SetPipeline(Pipeline pipeline)
        {
            _commands.Add(new SetPipelineEntry(pipeline));
        }

        internal void SetResourceSet(ResourceSet rs)
        {
            _commands.Add(new SetResourceSetEntry(rs));
        }

        internal void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _commands.Add(new SetScissorRectEntry(index, x, y, width, height));
        }

        internal void SetVertexBuffer(uint index, VertexBuffer vb)
        {
            _commands.Add(new SetVertexBufferEntry(index, vb));
        }

        internal void SetViewport(uint index, ref Viewport viewport)
        {
            _commands.Add(new SetViewportEntry(index, ref viewport));
        }

        internal void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _commands.Add(new UpdateBufferEntry(buffer, bufferOffsetInBytes, stagingBlock));
        }

        internal void UpdateTexture2D(
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
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _commands.Add(new UpdateTexture2DEntry(texture2D, stagingBlock, x, y, width, height, mipLevel, arrayLayer));
        }

        internal void UpdateTextureCube(
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
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _commands.Add(new UpdateTextureCubeEntry(textureCube, stagingBlock, face, x, y, width, height, mipLevel, arrayLayer));
        }
    }
}

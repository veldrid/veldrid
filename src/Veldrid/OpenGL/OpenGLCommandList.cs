using System;
using Veldrid.OpenGL.NoAllocEntryList;

namespace Veldrid.OpenGL
{
    internal class OpenGLCommandList : CommandList
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly OpenGLCommandEntryList _commands = new OpenGLNoAllocCommandEntryList();

        internal OpenGLCommandEntryList Commands => _commands;

        public override string Name { get; set; }

        public OpenGLCommandList(OpenGLGraphicsDevice gd, ref CommandListDescription description) : base(ref description)
        {
            _gd = gd;
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

        protected override void DrawIndirectCore(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            _commands.DrawIndirect(indirectBuffer, offset, drawCount, stride);
        }

        protected override void DrawIndexedIndirectCore(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            _commands.DrawIndexedIndirect(indirectBuffer, offset, drawCount, stride);
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            _commands.Dispatch(groupCountX, groupCountY, groupCountZ);
        }

        protected override void DispatchIndirectCore(Buffer indirectBuffer, uint offset)
        {
            _commands.DispatchIndirect(indirectBuffer, offset);
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

        protected override void CopyBufferCore(
            Buffer source,
            uint sourceOffset,
            Buffer destination,
            uint destinationOffset,
            uint sizeInBytes)
        {
            _commands.CopyBuffer(source, sourceOffset, destination, destinationOffset, sizeInBytes);
        }

        protected override void CopyTextureCore(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            _commands.CopyTexture(
                source,
                srcX, srcY, srcZ,
                srcMipLevel,
                srcBaseArrayLayer,
                destination,
                dstX, dstY, dstZ,
                dstMipLevel,
                dstBaseArrayLayer,
                width, height, depth,
                layerCount);
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyResources()
        {
            _commands.Dispose();
        }
    }
}
using System;
using System.Runtime.CompilerServices;

namespace Vd2
{
    public abstract class CommandBuffer : IDisposable
    {
        public CommandBuffer(ref CommandBufferDescription description)
        {
        }

        public abstract void Begin();
        public abstract void End();

        public abstract void BindPipeline(Pipeline pipeline);
        public abstract void BindVertexBuffer(uint index, VertexBuffer vb, uint vertexStrideInBytes);
        public abstract void BindIndexBuffer(IndexBuffer ib);
        public abstract void BindResourceSet(ResourceSet rs);
        public abstract void SetFramebuffer(Framebuffer fb);
        public abstract void ClearColorTarget(uint index, RgbaFloat clearColor);
        public abstract void ClearDepthTarget(float depth);
        public void SetViewport(uint index, Viewport viewport) => SetViewport(index, ref viewport);
        public abstract void SetViewport(uint index, ref Viewport viewport);
        public abstract void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        public abstract void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);
        public abstract void ExecuteCommands(CommandBuffer cb);

        // Resource Update
        public abstract void UpdateTexture2D(
            Texture2D texture2D,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer);

        public unsafe void UpdateBuffer<T>(
            Buffer buffer,
            ref T source,
            int sizeInBytes,
            int bufferOffsetInBytes) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, (IntPtr)ptr, sizeInBytes, bufferOffsetInBytes);
            }
        }

        public abstract void UpdateBuffer(
            Buffer buffer,
            IntPtr source,
            int sizeInBytes,
            int bufferOffsetInBytes);

        public abstract void Dispose();
    }
}

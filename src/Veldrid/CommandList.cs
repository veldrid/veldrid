using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    public abstract class CommandList : DeviceResource, IDisposable
    {
        public CommandList(ref CommandListDescription description)
        {
        }

        public abstract void Begin();
        public abstract void End();

        public abstract void SetPipeline(Pipeline pipeline);
        public abstract void SetVertexBuffer(uint index, VertexBuffer vb);
        public abstract void SetIndexBuffer(IndexBuffer ib);
        public abstract void SetResourceSet(uint slot, ResourceSet rs);
        public abstract void SetFramebuffer(Framebuffer fb);
        public abstract void ClearColorTarget(uint index, RgbaFloat clearColor);
        public abstract void ClearDepthTarget(float depth);
        public void SetViewport(uint index, Viewport viewport) => SetViewport(index, ref viewport);
        public abstract void SetViewport(uint index, ref Viewport viewport);
        public abstract void SetScissorRect(uint index, uint x, uint y, uint width, uint height);
        public abstract void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);

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

        // Resource Update
        public abstract void UpdateTextureCube(
            TextureCube textureCube,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer);

        public unsafe void UpdateBuffer<T>(
            Buffer buffer,
            uint bufferOffsetInBytes,
            T source) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, (uint)Unsafe.SizeOf<T>());
            }
        }

        public unsafe void UpdateBuffer<T>(
            Buffer buffer,
            uint bufferOffsetInBytes,
            ref T source) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, Util.USizeOf<T>());
            }
        }

        public unsafe void UpdateBuffer<T>(
            Buffer buffer,
            uint bufferOffsetInBytes,
            ref T source,
            uint sizeInBytes) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, sizeInBytes);
            }
        }

        public unsafe void UpdateBuffer<T>(
            Buffer buffer,
            uint bufferOffsetInBytes,
            T[] source) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(source, GCHandleType.Pinned);
            UpdateBuffer(buffer, bufferOffsetInBytes, gch.AddrOfPinnedObject(), (uint)(Unsafe.SizeOf<T>() * source.Length));
            gch.Free();
        }

        public abstract void UpdateBuffer(
            Buffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes);

        public abstract void Dispose();
    }
}

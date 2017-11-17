using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    /// <summary>
    /// A device resource which allows the recording of graphics commands, which can later be executed by a
    /// <see cref="GraphicsDevice"/>.
    /// Before graphics commands can be issued, the <see cref="Begin"/> method must be invoked.
    /// When the <see cref="CommandList"/> is ready to be executed, <see cref="End"/> must be invoked, and then
    /// <see cref="GraphicsDevice.ExecuteCommands(CommandList)"/> should be used.
    /// NOTE: The use of <see cref="CommandList"/> is not thread-safe. Access to the <see cref="CommandList"/> must be
    /// externally synchronized.
    /// There are some limitations dictating proper usage and ordering of graphics commands. For example, a
    /// <see cref="Framebuffer"/>, <see cref="Pipeline"/>, vertex buffer, and index buffer must all be
    /// bound before a call to <see cref="Draw(uint, uint, uint, int, uint)"/> will succeed.
    /// These limitations are described in each function, where applicable.
    /// <see cref="CommandList"/> instances cannot be executed multiple times per-recording. When executed by a
    /// <see cref="GraphicsDevice"/>, they must be reset and commands must be issued again.
    /// See <see cref="CommandListDescription"/>.
    /// </summary>
    public abstract class CommandList : DeviceResource, IDisposable
    {
        /// <summary>
        /// The active <see cref="Framebuffer"/>.
        /// </summary>
        protected Framebuffer _framebuffer;

        internal CommandList(ref CommandListDescription description)
        {
        }

        internal void ClearCachedState()
        {
            _framebuffer = null;
        }

        /// <summary>
        /// Puts this <see cref="CommandList"/> into the initial state.
        /// This function must be called before other graphics commands can be issued.
        /// Begin must only be called if it has not been previously called, if <see cref="End"/> has been called,
        /// or if <see cref="GraphicsDevice.ExecuteCommands(CommandList)"/> has been called on this instance.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Completes this list of graphics commands, putting it into an executable state for a <see cref="GraphicsDevice"/>.
        /// This function must only be called after <see cref="Begin"/> has been called.
        /// It is an error to call this function in succession, unless <see cref="Begin"/> has been called in between invocations.
        /// </summary>
        public abstract void End();

        /// <summary>
        /// Sets the active <see cref="Pipeline"/> used for rendering.
        /// When drawing, the active <see cref="Pipeline"/> must be compatible with the bound <see cref="Framebuffer"/>,
        /// <see cref="ResourceSet"/>, and <see cref="Buffer"/> objects.
        /// </summary>
        /// <param name="pipeline">The new <see cref="Pipeline"/> object.</param>
        public abstract void SetPipeline(Pipeline pipeline);

        /// <summary>
        /// Sets the active <see cref="Buffer"/> for the given index.
        /// When drawing, the bound <see cref="Buffer"/> objects must be compatible with the bound <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="index">The buffer slot.</param>
        /// <param name="buffer">The new <see cref="Buffer"/>.</param>
        public void SetVertexBuffer(uint index, Buffer buffer)
        {
#if VALIDATE_USAGE
            if ((buffer.Usage & BufferUsage.VertexBuffer) == 0)
            {
                throw new VeldridException(
                    $"Buffer cannot be bound as a vertex buffer because it was not created with BufferUsage.VertexBuffer.");
            }
#endif
            SetVertexBufferCore(index, buffer);
        }

        // TODO: private protected
        protected abstract void SetVertexBufferCore(uint index, Buffer buffer);

        /// <summary>
        /// Sets the active <see cref="Buffer"/>.
        /// When drawing, an <see cref="Buffer"/> must be bound.
        /// </summary>
        /// <param name="buffer">The new <see cref="Buffer"/>.</param>
        /// <param name="format">The format of data in the <see cref="Buffer"/>.</param>
        public void SetIndexBuffer(Buffer buffer, IndexFormat format)
        {
#if VALIDATE_USAGE
            if ((buffer.Usage & BufferUsage.IndexBuffer) == 0)
            {
                throw new VeldridException(
                    $"Buffer cannot be bound as an index buffer because it was not created with BufferUsage.IndexBuffer.");
            }
#endif
            SetIndexBufferCore(buffer, format);
        }

        // TODO: private protected
        protected abstract void SetIndexBufferCore(Buffer buffer, IndexFormat format);

        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        public void SetResourceSet(uint slot, ResourceSet rs)
        {
            SetResourceSetCore(slot, rs);
        }

        // TODO: private protected
        protected abstract void SetResourceSetCore(uint slot, ResourceSet rs);

        /// <summary>
        /// Sets the active <see cref="Framebuffer"/> which will be rendered to.
        /// When drawing, the active <see cref="Framebuffer"/> must be compatible with the active <see cref="Pipeline"/>.
        /// A compatible <see cref="Pipeline"/> has the same number of output attachments with matching formats.
        /// </summary>
        /// <param name="fb">The new <see cref="Framebuffer"/>.</param>
        public void SetFramebuffer(Framebuffer fb)
        {
            if (_framebuffer != fb)
            {
                _framebuffer = fb;
                SetFramebufferCore(fb);
            }
        }

        /// <summary>
        /// Performs API-specific handling of the <see cref="Framebuffer"/> resource.
        /// </summary>
        /// <param name="fb"></param>
        protected abstract void SetFramebufferCore(Framebuffer fb);

        /// <summary>
        /// Clears the color target at the given index of the active <see cref="Framebuffer"/>.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="clearColor">The value to clear the target to.</param>
        public abstract void ClearColorTarget(uint index, RgbaFloat clearColor);

        /// <summary>
        /// Clears the depth target of the active <see cref="Framebuffer"/>.
        /// The active <see cref="Framebuffer"/> must have a depth attachment.
        /// </summary>
        /// <param name="depth">The value to clear the depth target to.</param>
        public abstract void ClearDepthTarget(float depth);

        /// <summary>
        /// Sets all active viewports to cover the entire active <see cref="Framebuffer"/>.
        /// </summary>
        public void SetFullViewports()
        {
            SetViewport(0, new Viewport(0, 0, _framebuffer.Width, _framebuffer.Height, 0, 1));

            for (uint index = 1; index < _framebuffer.ColorTargets.Count; index++)
            {
                SetViewport(index, new Viewport(0, 0, _framebuffer.Width, _framebuffer.Height, 0, 1));
            }
        }

        /// <summary>
        /// Sets the active viewport at the given index to cover the entire active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        public void SetFullViewport(uint index)
        {
            SetViewport(index, new Viewport(0, 0, _framebuffer.Width, _framebuffer.Height, 0, 1));
        }

        /// <summary>
        /// Sets the active <see cref="Viewport"/> at the given index.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="viewport">The new <see cref="Viewport"/>.</param>
        public void SetViewport(uint index, Viewport viewport) => SetViewport(index, ref viewport);

        /// <summary>
        /// Sets the active <see cref="Viewport"/> at the given index.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="viewport">The new <see cref="Viewport"/>.</param>
        public abstract void SetViewport(uint index, ref Viewport viewport);

        /// <summary>
        /// Sets all active scissor rectangles to cover the active <see cref="Framebuffer"/>.
        /// </summary>
        public void SetFullScissorRects()
        {
            SetScissorRect(0, 0, 0, _framebuffer.Width, _framebuffer.Height);

            for (uint index = 1; index < _framebuffer.ColorTargets.Count; index++)
            {
                SetScissorRect(index, 0, 0, _framebuffer.Width, _framebuffer.Height);
            }
        }

        /// <summary>
        /// Sets the active scissor rectangle at the given index to cover the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        public void SetFullScissorRect(uint index)
        {
            SetScissorRect(index, 0, 0, _framebuffer.Width, _framebuffer.Height);
        }

        /// <summary>
        /// Sets the active scissor rectangle at the given index.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="x">The X value of the scissor rectangle.</param>
        /// <param name="y">The Y value of the scissor rectangle.</param>
        /// <param name="width">The width of the scissor rectangle.</param>
        /// <param name="height">The height of the scissor rectangle.</param>
        public abstract void SetScissorRect(uint index, uint x, uint y, uint width, uint height);

        /// <summary>
        /// Draws primitives from the currently-bound state in this <see cref="CommandList"/>.
        /// </summary>
        /// <param name="indexCount">The number of indices.</param>
        /// <param name="instanceCount">The number of instances.</param>
        /// <param name="indexStart">The number of indices to skip in the active index buffer.</param>
        /// <param name="vertexOffset">The base vertex value, which is added to each index value read from the index buffer.</param>
        /// <param name="instanceStart">The starting instance value.</param>
        public abstract void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);

        /// <summary>
        /// Resolves a multisampled source <see cref="Texture"/> into a non-multisampled destination <see cref="Texture"/>.
        /// </summary>
        /// <param name="source">The source of the resolve operation. Must be a multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> > 1).</param>
        /// <param name="destination">The destination of the resolve operation. Must be a non-multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> == 1).</param>
        public void ResolveTexture(Texture source, Texture destination)
        {
#if VALIDATE_USAGE
            if (source.SampleCount == TextureSampleCount.Count1)
            {
                throw new VeldridException(
                    $"The {nameof(source)} parameter of {nameof(ResolveTexture)} must be a multisample texture.");
            }
            if (destination.SampleCount != TextureSampleCount.Count1)
            {
                throw new VeldridException(
                    $"The {nameof(destination)} parameter of {nameof(ResolveTexture)} must be a non-multisample texture. Instead, it is a texture with {FormatHelpers.GetSampleCountUInt32(source.SampleCount)} samples.");
            }
#endif

            ResolveTextureCore(source, destination);
        }

        /// <summary>
        /// Resolves a multisampled source <see cref="Texture"/> into a non-multisampled destination <see cref="Texture"/>.
        /// </summary>
        /// <param name="source">The source of the resolve operation. Must be a multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> > 1).</param>
        /// <param name="destination">The destination of the resolve operation. Must be a non-multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> == 1).</param>
        protected abstract void ResolveTextureCore(Texture source, Texture destination);

        /// <summary>
        /// Updates a portion of a <see cref="Texture"/> resource with new data.
        /// Cube textures should instead use 
        /// <see cref="UpdateTextureCube(Texture, IntPtr, uint, CubeFace, uint, uint, uint, uint, uint, uint)"/>.
        /// </summary>
        /// <param name="texture">The resource to update.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The number of bytes to upload. This value must match the total size of the texture region specified.</param>
        /// <param name="x">The minimum X value of the updated region.</param>
        /// <param name="y">The minimum Y value of the updated region.</param>
        /// <param name="z">The minimum Z value of the updated region.</param>
        /// <param name="width">The width of the updated region, in texels.</param>
        /// <param name="height">The height of the updated region, in texels.</param>
        /// <param name="depth">The depth of the updated region, in texels.</param>
        /// <param name="mipLevel">The mipmap level to update. Must be less than the total number of mipmaps contained in the
        /// <see cref="Texture"/>.</param>
        /// <param name="arrayLayer">The array layer to update. Must be less than the total array layer count contained in the
        /// <see cref="Texture"/>.</param>
        public abstract void UpdateTexture(
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

        /// <summary>
        /// Updates a portion of a <see cref="Texture"/> resource with new data. This function operates on cubemap textures.
        /// </summary>
        /// <param name="texture">The resource to update.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The number of bytes to upload. This value must match the total size of the texture region specified.</param>
        /// <param name="face">The <see cref="CubeFace"/> into which the texture data is uploaded.</param>
        /// <param name="x">The minimum X value of the updated region.</param>
        /// <param name="y">The minimum Y value of the updated region.</param>
        /// <param name="width">The width of the updated region, in texels.</param>
        /// <param name="height">The height of the updated region, in texels.</param>
        /// <param name="mipLevel">The mipmap level to update. Must be less than the total number of mipmaps contained in the
        /// <see cref="Texture"/>.</param>
        /// <param name="arrayLayer">The array layer to update. Must be less than the total array layer count contained in the
        /// <see cref="Texture"/>.</param>
        public abstract void UpdateTextureCube(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer);

        /// <summary>
        /// Updates a <see cref="Buffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="Buffer"/> storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">The value to upload.</param>
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

        /// <summary>
        /// Updates a <see cref="Buffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="Buffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the single value to upload.</param>
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

        /// <summary>
        /// Updates a <see cref="Buffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="Buffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the first of a series of values to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
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

        /// <summary>
        /// Updates a <see cref="Buffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="Buffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">An array containing the data to upload.</param>
        public unsafe void UpdateBuffer<T>(
            Buffer buffer,
            uint bufferOffsetInBytes,
            T[] source) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(source, GCHandleType.Pinned);
            UpdateBuffer(buffer, bufferOffsetInBytes, gch.AddrOfPinnedObject(), (uint)(Unsafe.SizeOf<T>() * source.Length));
            gch.Free();
        }

        /// <summary>
        /// Updates a <see cref="Buffer"/> region with new data.
        /// </summary>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="Buffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public abstract void UpdateBuffer(
            Buffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes);

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}

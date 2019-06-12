using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    public abstract class CommandBuffer : DeviceResource, IDisposable
    {
        private readonly GraphicsDeviceFeatures _features;

        private Pipeline _graphicsPipeline;
        private DeviceBuffer _indexBuffer;
        private IndexFormat _indexFormat;
        private Framebuffer _framebuffer;

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        internal CommandBuffer(GraphicsDeviceFeatures features)
        {
            _features = features;
        }

        public abstract void Dispose();

        public void BeginRenderPass(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth)
            => BeginRenderPass(
                framebuffer, loadAction, storeAction, clearColor, clearDepth, Span<Texture>.Empty);

        public void BeginRenderPass(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth,
            Span<Texture> resolveTextures)
        {
            RequireNoRenderPass();
            _framebuffer = framebuffer;
            BeginRenderPassCore(
                new RenderPassDescription(framebuffer, loadAction, storeAction, clearColor, clearDepth, resolveTextures));
        }

        internal abstract void BeginRenderPassCore(in RenderPassDescription rpi);

        public void BindVertexBuffer(uint index, DeviceBuffer buffer) => BindVertexBuffer(index, buffer, 0);
        public void BindVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
            RequireRenderPass();
            BindVertexBufferCore(index, buffer, offset);
        }

        private protected abstract void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset);

        public void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format) => BindIndexBuffer(buffer, format, 0);
        public void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            RequireRenderPass();
            _indexBuffer = buffer;
            _indexFormat = format;
            BindIndexBufferCore(buffer, format, offset);
        }

        private protected abstract void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset);

        public void BindPipeline(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline)
            {
                RequireRenderPass();
                _graphicsPipeline = pipeline;
            }
            BindPipelineCore(pipeline);
        }

        private protected abstract void BindPipelineCore(Pipeline pipeline);

        public void BindGraphicsResourceSet(uint slot, ResourceSet resourceSet)
            => BindGraphicsResourceSet(slot, resourceSet, Span<uint>.Empty);

        public void BindGraphicsResourceSet(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            RequireRenderPass();
            BindGraphicsResourceSetCore(slot, resourceSet, dynamicOffsets);
        }

        private protected abstract void BindGraphicsResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets);

        public void Draw(uint vertexCount) => Draw(vertexCount, 1, 0, 0);

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            RequireRenderPass();
            DrawCore(vertexCount, instanceCount, vertexStart, instanceStart);
        }

        private protected abstract void DrawCore(
            uint vertexCount,
            uint instanceCount,
            uint vertexStart,
            uint instanceStart);

        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            RequireRenderPass();
            ValidateDrawIndirectSupport();
            ValidateIndirectBuffer(indirectBuffer);
            ValidateIndirectOffset(offset);
            ValidateIndirectStride(stride, Unsafe.SizeOf<IndirectDrawArguments>());
            PreDrawValidation();
            DrawIndirectCore(indirectBuffer, offset, drawCount, stride);
        }

        private protected abstract void DrawIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride);

        public void DrawIndexed(uint indexCount) => DrawIndexed(indexCount, 1, 0, 0, 0);

        public void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart)
        {
            RequireRenderPass();
            DrawIndexedCore(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        private protected abstract void DrawIndexedCore(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart);

        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            RequireRenderPass();
            ValidateDrawIndirectSupport();
            ValidateIndirectBuffer(indirectBuffer);
            ValidateIndirectOffset(offset);
            ValidateIndirectStride(stride, Unsafe.SizeOf<IndirectDrawIndexedArguments>());
            PreDrawValidation();
            DrawIndexedIndirectCore(indirectBuffer, offset, drawCount, stride);
        }

        private protected abstract void DrawIndexedIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride);

        public void SetViewport(uint index, Viewport viewport)
        {
            RequireRenderPass();
            SetViewportCore(index, viewport);
        }

        private protected abstract void SetViewportCore(uint index, Viewport viewport);

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            RequireRenderPass();
            SetScissorRectCore(index, x, y, width, height);
        }

        private protected abstract void SetScissorRectCore(uint index, uint x, uint y, uint width, uint height);

        public void EndRenderPass()
        {
            RequireRenderPass();
            EndRenderPassCore();
            _framebuffer = null;
        }

        private protected abstract void EndRenderPassCore();

        public void MemoryBarrier(ShaderStages sourceStage, ShaderStages destinationStage)
        {
            MemoryBarrierCore(sourceStage, destinationStage);
        }

        private protected abstract void MemoryBarrierCore(
            ShaderStages sourceStage,
            ShaderStages destinationStage);

        public void MemoryBarrier(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage)
        {
            MemoryBarrierCore(
                texture,
                baseMipLevel, levelCount,
                baseArrayLayer, layerCount,
                sourceStage,
                destinationStage);
        }

        private protected abstract void MemoryBarrierCore(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage);

        public void BindComputeResourceSet(uint slot, ResourceSet resourceSet)
            => BindComputeResourceSet(slot, resourceSet, Span<uint>.Empty);

        public void BindComputeResourceSet(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            RequireNoRenderPass();
            BindComputeResourceSetCore(slot, resourceSet, dynamicOffsets);
        }

        private protected abstract void BindComputeResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets);

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            RequireNoRenderPass();
            DispatchCore(groupCountX, groupCountY, groupCountZ);
        }

        private protected abstract void DispatchCore(uint groupCountX, uint groupCountY, uint groupCountZ);

        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            RequireNoRenderPass();
            ValidateIndirectBuffer(indirectBuffer);
            ValidateIndirectOffset(offset);
            DispatchIndirectCore(indirectBuffer, offset);
        }

        internal abstract void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset);

        public void CopyBuffer(
            DeviceBuffer source, uint sourceOffset,
            DeviceBuffer destination, uint destinationOffset,
            uint sizeInBytes)
        {
            RequireNoRenderPass();
            CopyBufferCore(source, sourceOffset, destination, destinationOffset, sizeInBytes);
        }

        private protected abstract void CopyBufferCore(
            DeviceBuffer source, uint sourceOffset,
            DeviceBuffer destination, uint destinationOffset,
            uint sizeInBytes);

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/> storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">The value to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            T source) where T : struct
        {
            RequireNoRenderPass();
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, (uint)Unsafe.SizeOf<T>());
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the single value to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ref T source) where T : struct
        {
            RequireNoRenderPass();
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, Util.USizeOf<T>());
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the first of a series of values to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ref T source,
            uint sizeInBytes) where T : struct
        {
            RequireNoRenderPass();
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, sizeInBytes);
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">An array containing the data to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            T[] source) where T : unmanaged
        {
            RequireNoRenderPass();
            fixed (T* sourcePtr = source)
            {
                UpdateBuffer(
                    buffer,
                    bufferOffsetInBytes,
                    (IntPtr)sourcePtr,
                    (uint)(Unsafe.SizeOf<T>() * source.Length));
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// </summary>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public void UpdateBuffer(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes)
        {
            RequireNoRenderPass();
            if (bufferOffsetInBytes + sizeInBytes > buffer.SizeInBytes)
            {
                throw new VeldridException(
                    $"The DeviceBuffer's capacity ({buffer.SizeInBytes}) is not large enough to store the amount of " +
                    $"data specified ({sizeInBytes}) at the given offset ({bufferOffsetInBytes}).");
            }
            if (sizeInBytes == 0)
            {
                return;
            }

            UpdateBufferCore(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        private protected abstract void UpdateBufferCore(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes);

        public void CopyTexture(
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
            RequireNoRenderPass();
            CopyTextureCore(
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth,
                layerCount);
        }

        private protected abstract void CopyTextureCore(
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

        public void GenerateMipmaps(Texture texture)
        {
            if ((texture.Usage & TextureUsage.GenerateMipmaps) == 0)
            {
                throw new VeldridException(
                    $"{nameof(GenerateMipmaps)} requires a target Texture with {nameof(TextureUsage)}.{nameof(TextureUsage.GenerateMipmaps)}");
            }

            if (texture.MipLevels > 1)
            {
                GenerateMipmapsCore(texture);
            }
        }

        private protected abstract void GenerateMipmapsCore(Texture texture);

        public abstract void InsertDebugMarker(string name);

        public abstract void PushDebugGroup(string name);

        public abstract void PopDebugGroup();

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateIndirectOffset(uint offset)
        {
            if ((offset % 4) != 0)
            {
                throw new VeldridException($"{nameof(offset)} must be a multiple of 4.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private void ValidateDrawIndirectSupport()
        {
            if (!_features.DrawIndirect)
            {
                throw new VeldridException($"Indirect drawing is not supported by this device.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateIndirectBuffer(DeviceBuffer indirectBuffer)
        {
            if ((indirectBuffer.Usage & BufferUsage.IndirectBuffer) != BufferUsage.IndirectBuffer)
            {
                throw new VeldridException(
                    $"{nameof(indirectBuffer)} parameter must have been created with BufferUsage.IndirectBuffer. Instead, it was {indirectBuffer.Usage}.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateIndirectStride(uint stride, int argumentSize)
        {
            if (stride < argumentSize || ((stride % 4) != 0))
            {
                throw new VeldridException(
                    $"{nameof(stride)} parameter must be a multiple of 4, and must be larger than the size of the corresponding argument structure.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private void ValidateIndexBuffer(uint indexCount)
        {
#if VALIDATE_USAGE
            if (_indexBuffer == null)
            {
                throw new VeldridException($"An index buffer must be bound before {nameof(CommandList)}.{nameof(DrawIndexed)} can be called.");
            }

            uint indexFormatSize = _indexFormat == IndexFormat.UInt16 ? 2u : 4u;
            uint bytesNeeded = indexCount * indexFormatSize;
            if (_indexBuffer.SizeInBytes < bytesNeeded)
            {
                throw new VeldridException(
                    $"The active index buffer does not contain enough data to satisfy the given draw command. {bytesNeeded} bytes are needed, but the buffer only contains {_indexBuffer.SizeInBytes}.");
            }
#endif
        }

        [Conditional("VALIDATE_USAGE")]
        private void PreDrawValidation()
        {
#if VALIDATE_USAGE

            if (_graphicsPipeline == null)
            {
                throw new VeldridException($"A graphics {nameof(Pipeline)} must be set in order to issue draw commands.");
            }
            if (_framebuffer == null)
            {
                throw new VeldridException($"A {nameof(Framebuffer)} must be set in order to issue draw commands.");
            }
            if (!_graphicsPipeline.GraphicsOutputDescription.Equals(_framebuffer.OutputDescription))
            {
                throw new VeldridException($"The {nameof(OutputDescription)} of the current graphics {nameof(Pipeline)} is not compatible with the current {nameof(Framebuffer)}.");
            }
#endif
        }

        [Conditional("VALIDATE_USAGE")]
        private void RequireRenderPass()
        {
#if VALIDATE_USAGE
            if (_framebuffer == null)
            {
                throw new VeldridException("This method must be called within an active render pass.");
            }
#endif
        }

        [Conditional("VALIDATE_USAGE")]
        private void RequireNoRenderPass()
        {
#if VALIDATE_USAGE
            if (_framebuffer != null)
            {
                throw new VeldridException("This method must not be called within an active render pass.");
            }
#endif
        }
    }

    internal readonly struct RenderPassDescription
    {
        public readonly Framebuffer Framebuffer;
        public readonly StoreAction StoreAction;
        public readonly LoadAction LoadAction;
        public readonly RgbaFloat ClearColor;
        public readonly float ClearDepth;
        public readonly SmallFixedOrDynamicArray<Texture> ResolveTextures;

        public RenderPassDescription(
            Framebuffer framebuffer,
            LoadAction loadAction,
            StoreAction storeAction,
            RgbaFloat clearColor,
            float clearDepth,
            Span<Texture> resolveTextures)
        {
            Framebuffer = framebuffer;
            LoadAction = loadAction;
            StoreAction = storeAction;
            ClearColor = clearColor;
            ClearDepth = clearDepth;
            ResolveTextures = new SmallFixedOrDynamicArray<Texture>(resolveTextures);
        }
    }
}

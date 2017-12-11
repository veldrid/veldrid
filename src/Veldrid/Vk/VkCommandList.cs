using System;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;

namespace Veldrid.Vk
{
    internal unsafe class VkCommandList : CommandList
    {
        private readonly VkGraphicsDevice _gd;
        private VkCommandPool _pool;
        private VkCommandBuffer _cb;

        private List<VkImage> _imagesToDestroy;
        private List<Vulkan.VkBuffer> _buffersToDestroy;
        private List<VkMemoryBlock> _memoriesToFree;

        private bool _commandBufferBegun;
        private bool _commandBufferEnded;
        private VkRect2D[] _scissorRects = Array.Empty<VkRect2D>();

        // Graphics State
        private VkFramebufferBase _currentFramebuffer;
        private VkRenderPass _activeRenderPass;
        private VkPipeline _currentGraphicsPipeline;
        private VkResourceSet[] _currentGraphicsResourceSets = Array.Empty<VkResourceSet>();
        private bool[] _graphicsResourceSetsChanged;

        // Compute State
        private VkPipeline _currentComputePipeline;
        private VkResourceSet[] _currentComputeResourceSets = Array.Empty<VkResourceSet>();
        private bool[] _computeResourceSetsChanged;

        public VkCommandPool CommandPool => _pool;
        public VkCommandBuffer CommandBuffer => _cb;

        internal void CollectDisposables(List<Vulkan.VkBuffer> buffers, List<VkImage> images, List<VkMemoryBlock> memories)
        {
            if (_buffersToDestroy != null)
            {
                foreach (Vulkan.VkBuffer buffer in _buffersToDestroy)
                {
                    buffers.Add(buffer);
                }
                _buffersToDestroy.Clear();
            }

            if (_imagesToDestroy != null)
            {
                foreach (VkImage image in _imagesToDestroy)
                {
                    images.Add(image);
                }
                _imagesToDestroy.Clear();
            }

            if (_memoriesToFree != null)
            {
                foreach (VkMemoryBlock memory in _memoriesToFree)
                {
                    memories.Add(memory);
                }
                _memoriesToFree.Clear();
            }
        }

        public VkCommandList(VkGraphicsDevice gd, ref CommandListDescription description)
            : base(ref description)
        {
            _gd = gd;
            VkCommandPoolCreateInfo poolCI = VkCommandPoolCreateInfo.New();
            poolCI.queueFamilyIndex = gd.GraphicsQueueIndex;
            VkResult result = vkCreateCommandPool(_gd.Device, ref poolCI, null, out _pool);
            CheckResult(result);

            VkCommandBufferAllocateInfo cbAI = VkCommandBufferAllocateInfo.New();
            cbAI.commandPool = _pool;
            cbAI.commandBufferCount = 1;
            cbAI.level = VkCommandBufferLevel.Primary;
            result = vkAllocateCommandBuffers(gd.Device, ref cbAI, out _cb);
            CheckResult(result);
        }

        public override void Begin()
        {
            if (_commandBufferBegun)
            {
                throw new VeldridException(
                    "CommandList must be in its initial state, or End() must have been called, for Begin() to be valid to call.");
            }
            if (_commandBufferEnded)
            {
                _commandBufferEnded = false;
                vkResetCommandPool(_gd.Device, _pool, VkCommandPoolResetFlags.None);
            }

            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            vkBeginCommandBuffer(_cb, ref beginInfo);
            _commandBufferBegun = true;

            ClearCachedState();
            _currentFramebuffer = null;
            _currentGraphicsPipeline = null;
            Util.ClearArray(_currentGraphicsResourceSets);
            Util.ClearArray(_scissorRects);

            _currentComputePipeline = null;
            Util.ClearArray(_currentComputeResourceSets);
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            EnsureRenderPassActive();

            VkClearValue clearValue = new VkClearValue { color = new VkClearColorValue(clearColor.R, clearColor.G, clearColor.B, clearColor.A) };
            VkClearAttachment clearAttachment = new VkClearAttachment
            {
                colorAttachment = index,
                aspectMask = VkImageAspectFlags.Color,
                clearValue = clearValue
            };

            Texture colorTex = _currentFramebuffer.ColorTargets[(int)index].Target;
            VkClearRect clearRect = new VkClearRect
            {
                baseArrayLayer = 0,
                layerCount = 1,
                rect = new VkRect2D(0, 0, colorTex.Width, colorTex.Height)
            };

            vkCmdClearAttachments(_cb, 1, ref clearAttachment, 1, ref clearRect);
        }

        public override void ClearDepthTarget(float depth)
        {
            EnsureRenderPassActive();

            VkClearValue clearValue = new VkClearValue { depthStencil = new VkClearDepthStencilValue(depth, 0) };
            VkClearAttachment clearAttachment = new VkClearAttachment
            {
                aspectMask = VkImageAspectFlags.Depth,
                clearValue = clearValue
            };

            Texture depthTex = _currentFramebuffer.DepthTarget.Value.Target;
            VkClearRect clearRect = new VkClearRect
            {
                baseArrayLayer = 0,
                layerCount = 1,
                rect = new VkRect2D(0, 0, depthTex.Width, depthTex.Height)
            };

            vkCmdClearAttachments(_cb, 1, ref clearAttachment, 1, ref clearRect);
        }

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDraw(_cb, vertexCount, instanceCount, vertexStart, instanceStart);
        }

        public override void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDrawIndexed(_cb, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        protected override void DrawIndirectCore(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(indirectBuffer);
            vkCmdDrawIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        protected override void DrawIndexedIndirectCore(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(indirectBuffer);
            vkCmdDrawIndexedIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        private void PreDrawCommand()
        {
            EnsureRenderPassActive();

            // TODO: This should only call vkCmdBindDescriptorSets once, or rather, only the ones that changed.
            for (int i = 0; i < _graphicsResourceSetsChanged.Length; i++)
            {
                if (_graphicsResourceSetsChanged[i])
                {
                    _graphicsResourceSetsChanged[i] = false;
                    VkDescriptorSet ds = _currentGraphicsResourceSets[i].DescriptorSet;
                    vkCmdBindDescriptorSets(
                        _cb,
                         VkPipelineBindPoint.Graphics,
                         _currentGraphicsPipeline.PipelineLayout,
                         (uint)i,
                         1,
                         ref ds,
                         0,
                         null);
                }
            }

            if (!_currentGraphicsPipeline.ScissorTestEnabled)
            {
                SetFullScissorRects();
            }
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            EnsureNoRenderPass();

            // TODO: This should only call vkCmdBindDescriptorSets once, or rather, only the ones that changed.
            for (int i = 0; i < _computeResourceSetsChanged.Length; i++)
            {
                if (_computeResourceSetsChanged[i])
                {
                    _computeResourceSetsChanged[i] = false;
                    VkDescriptorSet ds = _currentComputeResourceSets[i].DescriptorSet;
                    vkCmdBindDescriptorSets(
                        _cb,
                         VkPipelineBindPoint.Compute,
                         _currentComputePipeline.PipelineLayout,
                         (uint)i,
                         1,
                         ref ds,
                         0,
                         null);
                }
            }

            vkCmdDispatch(_cb, groupCountX, groupCountY, groupCountZ);
        }

        protected override void DispatchIndirectCore(Buffer indirectBuffer, uint offset)
        {
            EnsureNoRenderPass();

            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(indirectBuffer);
            vkCmdDispatchIndirect(_cb, vkBuffer.DeviceBuffer, offset);
        }

        protected override void ResolveTextureCore(Texture source, Texture destination)
        {
            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }

            VkTexture vkSource = Util.AssertSubtype<Texture, VkTexture>(source);
            VkTexture vkDestination = Util.AssertSubtype<Texture, VkTexture>(destination);
            VkImageAspectFlags aspectFlags = ((source.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
                ? VkImageAspectFlags.Depth
                : VkImageAspectFlags.Color;
            VkImageResolve region = new VkImageResolve
            {
                extent = new VkExtent3D { width = source.Width, height = source.Height, depth = source.Depth },
                srcSubresource = new VkImageSubresourceLayers { layerCount = 1, aspectMask = aspectFlags },
                dstSubresource = new VkImageSubresourceLayers { layerCount = 1, aspectMask = aspectFlags }
            };

            vkCmdResolveImage(
                _cb,
                vkSource.DeviceImage,
                vkSource.ImageLayouts[0],
                vkDestination.DeviceImage,
                vkDestination.ImageLayouts[0],
                1,
                ref region);
        }

        public override void End()
        {
            if (!_commandBufferBegun)
            {
                throw new VeldridException("CommandBuffer must have been started before End() may be called.");
            }

            _commandBufferBegun = false;
            _commandBufferEnded = true;

            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }

            vkEndCommandBuffer(_cb);
        }

        protected override void SetFramebufferCore(Framebuffer fb)
        {
            if (_activeRenderPass.Handle != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
                // Place a barrier between RenderPasses, so that color / depth outputs
                // can be read in subsequent passes.
                vkCmdPipelineBarrier(
                    _cb,
                    VkPipelineStageFlags.ColorAttachmentOutput,
                    VkPipelineStageFlags.TopOfPipe,
                    VkDependencyFlags.ByRegion,
                    0,
                    null,
                    0,
                    null,
                    0,
                    null);
            }

            VkFramebufferBase vkFB = Util.AssertSubtype<Framebuffer, VkFramebufferBase>(fb);
            _currentFramebuffer = vkFB;
            Util.EnsureArraySize(ref _scissorRects, Math.Max(1, (uint)vkFB.ColorTargets.Count));

            BeginCurrentRenderPass();
        }

        private void EnsureRenderPassActive()
        {
            if (_activeRenderPass == VkRenderPass.Null)
            {
                BeginCurrentRenderPass();
            }
        }

        private void EnsureNoRenderPass()
        {
            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }
        }

        private void BeginCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass == VkRenderPass.Null);

            VkRenderPassBeginInfo renderPassBI = VkRenderPassBeginInfo.New();
            renderPassBI.framebuffer = _currentFramebuffer.CurrentFramebuffer;
            renderPassBI.renderPass = _currentFramebuffer.RenderPass;
            renderPassBI.renderArea = new VkRect2D(_currentFramebuffer.RenderableWidth, _currentFramebuffer.RenderableHeight);
            vkCmdBeginRenderPass(_cb, ref renderPassBI, VkSubpassContents.Inline);
            _activeRenderPass = _currentFramebuffer.RenderPass;
        }

        private void EndCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass != VkRenderPass.Null);
            vkCmdEndRenderPass(_cb);
            _activeRenderPass = VkRenderPass.Null;
        }

        protected override void SetVertexBufferCore(uint index, Buffer buffer)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(buffer);
            Vulkan.VkBuffer deviceBuffer = vkBuffer.DeviceBuffer;
            ulong offset = 0;
            vkCmdBindVertexBuffers(_cb, index, 1, ref deviceBuffer, ref offset);
        }

        protected override void SetIndexBufferCore(Buffer buffer, IndexFormat format)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(buffer);
            vkCmdBindIndexBuffer(_cb, vkBuffer.DeviceBuffer, 0, VkFormats.VdToVkIndexFormat(format));
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _currentGraphicsPipeline != pipeline)
            {
                VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
                Util.EnsureArraySize(ref _currentGraphicsResourceSets, vkPipeline.ResourceSetCount);
                Util.ClearArray(_currentGraphicsResourceSets);
                Util.EnsureArraySize(ref _graphicsResourceSetsChanged, vkPipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Graphics, vkPipeline.DevicePipeline);
                _currentGraphicsPipeline = vkPipeline;
            }
            else if (pipeline.IsComputePipeline && _currentComputePipeline != pipeline)
            {
                VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
                Util.EnsureArraySize(ref _currentComputeResourceSets, vkPipeline.ResourceSetCount);
                Util.ClearArray(_currentComputeResourceSets);
                Util.EnsureArraySize(ref _computeResourceSetsChanged, vkPipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Compute, vkPipeline.DevicePipeline);
                _currentComputePipeline = vkPipeline;
            }
        }

        protected override void SetGraphicsResourceSetCore(uint slot, ResourceSet rs)
        {
            if (_currentGraphicsResourceSets[slot] != rs)
            {
                VkResourceSet vkRS = Util.AssertSubtype<ResourceSet, VkResourceSet>(rs);
                _currentGraphicsResourceSets[slot] = vkRS;
                _graphicsResourceSetsChanged[slot] = true;
            }
        }

        protected override void SetComputeResourceSetCore(uint slot, ResourceSet rs)
        {
            if (_currentComputeResourceSets[slot] != rs)
            {
                VkResourceSet vkRS = Util.AssertSubtype<ResourceSet, VkResourceSet>(rs);
                _currentComputeResourceSets[slot] = vkRS;
                _computeResourceSetsChanged[slot] = true;
            }
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            VkRect2D scissor = new VkRect2D((int)x, (int)y, (int)width, (int)height);
            if (_scissorRects[index] != scissor)
            {
                _scissorRects[index] = scissor;
                vkCmdSetScissor(_cb, index, 1, ref scissor);
            }
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            VkViewport vkViewport = new VkViewport
            {
                x = viewport.X,
                y = viewport.Y,
                width = viewport.Width,
                height = viewport.Height,
                minDepth = viewport.MinDepth,
                maxDepth = viewport.MaxDepth
            };

            vkCmdSetViewport(_cb, index, 1, ref vkViewport);
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(buffer);
            VkMemoryBlock memoryBlock = null;
            Vulkan.VkBuffer copySrcBuffer = Vulkan.VkBuffer.Null;
            IntPtr mappedPtr;
            bool isPersistentMapped = vkBuffer.Memory.IsPersistentMapped;
            if (isPersistentMapped)
            {
                mappedPtr = (IntPtr)vkBuffer.Memory.BlockMappedPointer;
            }
            else
            {
                VkBufferCreateInfo bufferCI = VkBufferCreateInfo.New();
                bufferCI.usage = VkBufferUsageFlags.TransferSrc;
                bufferCI.size = vkBuffer.BufferMemoryRequirements.size;
                VkResult result = vkCreateBuffer(_gd.Device, ref bufferCI, null, out copySrcBuffer);
                CheckResult(result);

                vkGetBufferMemoryRequirements(_gd.Device, copySrcBuffer, out VkMemoryRequirements memReqs);

                memoryBlock = _gd.MemoryManager.Allocate(
                    _gd.PhysicalDeviceMemProperties,
                    memReqs.memoryTypeBits,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                    true,
                    memReqs.size,
                    memReqs.alignment);

                result = vkBindBufferMemory(_gd.Device, copySrcBuffer, memoryBlock.DeviceMemory, memoryBlock.Offset);
                CheckResult(result);

                mappedPtr = (IntPtr)memoryBlock.BlockMappedPointer;
            }

            byte* destPtr = (byte*)mappedPtr + bufferOffsetInBytes;
            Unsafe.CopyBlock(destPtr, source.ToPointer(), sizeInBytes);

            if (!isPersistentMapped)
            {
                EnsureNoRenderPass();
                VkBufferCopy copyRegion = new VkBufferCopy { size = vkBuffer.BufferMemoryRequirements.size };
                vkCmdCopyBuffer(_cb, copySrcBuffer, vkBuffer.DeviceBuffer, 1, ref copyRegion);

                if (_buffersToDestroy == null)
                {
                    _buffersToDestroy = new List<Vulkan.VkBuffer>();
                }
                _buffersToDestroy.Add(copySrcBuffer);

                if (_memoriesToFree == null)
                {
                    _memoriesToFree = new List<VkMemoryBlock>();
                }
                _memoriesToFree.Add(memoryBlock);
            }
        }

        private IntPtr MapBuffer(VkBuffer buffer, uint numBytes)
        {
            if (buffer.Memory.IsPersistentMapped)
            {
                return (IntPtr)buffer.Memory.BlockMappedPointer;
            }
            else
            {
                void* mappedPtr;
                VkResult result = vkMapMemory(_gd.Device, buffer.Memory.DeviceMemory, buffer.Memory.Offset, numBytes, 0, &mappedPtr);
                CheckResult(result);
                return (IntPtr)mappedPtr;
            }
        }

        private void UnmapBuffer(VkBuffer buffer)
        {
            if (!buffer.Memory.IsPersistentMapped)
            {
                vkUnmapMemory(_gd.Device, buffer.Memory.DeviceMemory);
            }
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
            VkTexture tex = Util.AssertSubtype<Texture, VkTexture>(texture);

            if (x != 0 || y != 0)
            {
                throw new NotImplementedException();
            }

            // First, create a staging texture.
            CreateImage(
                _gd.Device,
                _gd.PhysicalDeviceMemProperties,
                _gd.MemoryManager,
                width,
                height,
                depth,
                1,
                VkFormats.VdToVkPixelFormat(tex.Format),
                VkImageTiling.Linear,
                VkImageUsageFlags.TransferSrc,
                VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                out VkImage stagingImage,
                out VkMemoryBlock stagingMemory);

            VkImageSubresource subresource = new VkImageSubresource();
            subresource.aspectMask = VkImageAspectFlags.Color;
            subresource.mipLevel = 0;
            subresource.arrayLayer = 0;
            vkGetImageSubresourceLayout(_gd.Device, stagingImage, ref subresource, out VkSubresourceLayout stagingLayout);
            ulong rowPitch = stagingLayout.rowPitch;

            void* mappedPtr;
            VkResult result = vkMapMemory(_gd.Device, stagingMemory.DeviceMemory, stagingMemory.Offset, stagingLayout.size, 0, &mappedPtr);
            CheckResult(result);

            if (rowPitch == width)
            {
                System.Buffer.MemoryCopy(source.ToPointer(), mappedPtr, sizeInBytes, sizeInBytes);
            }
            else
            {
                uint pixelSizeInBytes = FormatHelpers.GetSizeInBytes(texture.Format);
                for (uint yy = 0; yy < height; yy++)
                {
                    byte* dstRowStart = ((byte*)mappedPtr) + (rowPitch * yy);
                    byte* srcRowStart = ((byte*)source.ToPointer()) + (width * yy * pixelSizeInBytes);
                    Unsafe.CopyBlock(dstRowStart, srcRowStart, width * pixelSizeInBytes);
                }
            }

            vkUnmapMemory(_gd.Device, stagingMemory.DeviceMemory);

            TransitionImageLayout(stagingImage, 0, 1, 0, 1, VkImageLayout.Preinitialized, VkImageLayout.TransferSrcOptimal);
            TransitionImageLayout(tex.DeviceImage, mipLevel, 1, 0, 1, tex.ImageLayouts[mipLevel], VkImageLayout.TransferDstOptimal);
            CopyImage(stagingImage, 0, tex.DeviceImage, mipLevel, width, height);
            TransitionImageLayout(tex.DeviceImage, mipLevel, 1, 0, 1, VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal);
            tex.ImageLayouts[mipLevel] = VkImageLayout.ShaderReadOnlyOptimal;

            if (_imagesToDestroy == null)
            {
                _imagesToDestroy = new List<VkImage>();
            }
            _imagesToDestroy.Add(stagingImage);

            if (_memoriesToFree == null)
            {
                _memoriesToFree = new List<VkMemoryBlock>();
            }
            _memoriesToFree.Add(stagingMemory);
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
            VkTexture vkTexCube = Util.AssertSubtype<Texture, VkTexture>(textureCube);

            if (x != 0 || y != 0)
            {
                throw new NotImplementedException();
            }

            // First, create a staging texture.
            CreateImage(
                _gd.Device,
                _gd.PhysicalDeviceMemProperties,
                _gd.MemoryManager,
                width,
                height,
                1,
                1,
                VkFormats.VdToVkPixelFormat(vkTexCube.Format),
                VkImageTiling.Linear,
                VkImageUsageFlags.TransferSrc,
                VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                out VkImage stagingImage,
                out VkMemoryBlock stagingMemory);

            VkImageSubresource subresource = new VkImageSubresource();
            subresource.aspectMask = VkImageAspectFlags.Color;
            subresource.mipLevel = 0;
            subresource.arrayLayer = 0;
            vkGetImageSubresourceLayout(_gd.Device, stagingImage, ref subresource, out VkSubresourceLayout stagingLayout);
            ulong rowPitch = stagingLayout.rowPitch;

            void* mappedPtr;
            VkResult result = vkMapMemory(_gd.Device, stagingMemory.DeviceMemory, stagingMemory.Offset, stagingLayout.size, 0, &mappedPtr);
            CheckResult(result);

            if (rowPitch == width)
            {
                System.Buffer.MemoryCopy(source.ToPointer(), mappedPtr, sizeInBytes, sizeInBytes);
            }
            else
            {
                uint pixelSizeInBytes = FormatHelpers.GetSizeInBytes(vkTexCube.Format);
                for (uint yy = 0; yy < height; yy++)
                {
                    byte* dstRowStart = ((byte*)mappedPtr) + (rowPitch * yy);
                    byte* srcRowStart = ((byte*)source.ToPointer()) + (width * yy * pixelSizeInBytes);
                    Unsafe.CopyBlock(dstRowStart, srcRowStart, width * pixelSizeInBytes);
                }
            }

            vkUnmapMemory(_gd.Device, stagingMemory.DeviceMemory);

            uint cubeArrayLayer = GetArrayLayer(face);

            // TODO: These transitions are sub-optimal.
            TransitionImageLayout(stagingImage, 0, 1, 0, 1, VkImageLayout.Preinitialized, VkImageLayout.TransferSrcOptimal);
            TransitionImageLayout(vkTexCube.DeviceImage, 0, 1, 0, 6, vkTexCube.ImageLayouts[0], VkImageLayout.TransferDstOptimal);
            CopyImage(stagingImage, 0, vkTexCube.DeviceImage, mipLevel, width, height, cubeArrayLayer);
            TransitionImageLayout(vkTexCube.DeviceImage, 0, 1, 0, 6, VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal);
            vkTexCube.ImageLayouts[0] = VkImageLayout.ShaderReadOnlyOptimal;

            if (_imagesToDestroy == null)
            {
                _imagesToDestroy = new List<VkImage>();
            }
            _imagesToDestroy.Add(stagingImage);

            if (_memoriesToFree == null)
            {
                _memoriesToFree = new List<VkMemoryBlock>();
            }
            _memoriesToFree.Add(stagingMemory);
        }

        private uint GetArrayLayer(CubeFace face)
        {
            switch (face)
            {
                case CubeFace.NegativeX:
                    return 1;
                case CubeFace.PositiveX:
                    return 0;
                case CubeFace.NegativeY:
                    return 3;
                case CubeFace.PositiveY:
                    return 2;
                case CubeFace.NegativeZ:
                    return 4;
                case CubeFace.PositiveZ:
                    return 5;
                default:
                    throw Illegal.Value<CubeFace>();
            }
        }

        protected void TransitionImageLayout(
            VkImage image,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageLayout oldLayout,
            VkImageLayout newLayout)
        {
            Debug.Assert(oldLayout != newLayout);
            VkImageMemoryBarrier barrier = VkImageMemoryBarrier.New();
            barrier.oldLayout = oldLayout;
            barrier.newLayout = newLayout;
            barrier.srcQueueFamilyIndex = QueueFamilyIgnored;
            barrier.dstQueueFamilyIndex = QueueFamilyIgnored;
            barrier.image = image;
            barrier.subresourceRange.aspectMask = VkImageAspectFlags.Color;
            barrier.subresourceRange.baseMipLevel = baseMipLevel;
            barrier.subresourceRange.levelCount = levelCount;
            barrier.subresourceRange.baseArrayLayer = baseArrayLayer;
            barrier.subresourceRange.layerCount = layerCount;

            VkPipelineStageFlags srcStageFlags = VkPipelineStageFlags.None;
            VkPipelineStageFlags dstStageFlags = VkPipelineStageFlags.None;

            if ((oldLayout == VkImageLayout.Undefined || oldLayout == VkImageLayout.Preinitialized) && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.None;
                barrier.dstAccessMask = VkAccessFlags.TransferWrite;
                srcStageFlags = VkPipelineStageFlags.TopOfPipe;
                dstStageFlags = VkPipelineStageFlags.Transfer;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags.TransferWrite;
                srcStageFlags = VkPipelineStageFlags.FragmentShader;
                dstStageFlags = VkPipelineStageFlags.Transfer;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.None;
                barrier.dstAccessMask = VkAccessFlags.TransferRead;
                srcStageFlags = VkPipelineStageFlags.TopOfPipe;
                dstStageFlags = VkPipelineStageFlags.Transfer;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.TransferWrite;
                barrier.dstAccessMask = VkAccessFlags.ShaderRead;
                srcStageFlags = VkPipelineStageFlags.Transfer;
                dstStageFlags = VkPipelineStageFlags.FragmentShader;
            }
            else
            {
                Debug.Fail("Invalid image layout transition.");
            }

            vkCmdPipelineBarrier(
                _cb,
                srcStageFlags,
                dstStageFlags,
                VkDependencyFlags.None,
                0, null,
                0, null,
                1, &barrier);
        }

        protected void CopyImage(
            VkImage srcImage,
            uint srcMipLevel,
            VkImage dstImage,
            uint dstMipLevel,
            uint width,
            uint height,
            uint baseArrayLayer = 0)
        {
            VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers();
            srcSubresource.mipLevel = srcMipLevel;
            srcSubresource.layerCount = 1;
            srcSubresource.aspectMask = VkImageAspectFlags.Color;
            srcSubresource.baseArrayLayer = 0;

            VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers();
            dstSubresource.mipLevel = dstMipLevel;
            dstSubresource.baseArrayLayer = baseArrayLayer;
            dstSubresource.layerCount = 1;
            dstSubresource.aspectMask = VkImageAspectFlags.Color;

            VkImageCopy region = new VkImageCopy();
            region.dstSubresource = dstSubresource;
            region.srcSubresource = srcSubresource;
            region.extent.width = width;
            region.extent.height = height;
            region.extent.depth = 1;

            vkCmdCopyImage(
                _cb,
                srcImage,
                VkImageLayout.TransferSrcOptimal,
                dstImage,
                VkImageLayout.TransferDstOptimal,
                1,
                ref region);
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposedCommandBuffer(this);
        }

        // Must only be called once the command buffer has fully executed.
        public void DestroyCommandPool()
        {
            vkDestroyCommandPool(_gd.Device, _pool, null);
        }
    }
}
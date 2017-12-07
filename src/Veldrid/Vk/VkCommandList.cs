using System;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System.Diagnostics;
using System.Collections.Generic;

namespace Veldrid.Vk
{
    internal unsafe class VkCommandList : CommandList
    {
        private readonly VkGraphicsDevice _gd;
        private VkCommandPool _pool;
        private VkCommandBuffer _cb;
        private bool _destroyed;
        private readonly HashSet<VkDeferredDisposal> _referencedResources = new HashSet<VkDeferredDisposal>();

        private bool _commandBufferBegun;
        private bool _commandBufferEnded;
        private VkRect2D[] _scissorRects = Array.Empty<VkRect2D>();

        private VkClearValue[] _clearValues = Array.Empty<VkClearValue>();
        private bool[] _validColorClearValues = Array.Empty<bool>();
        private VkClearValue? _depthClearValue;

        // Graphics State
        private VkFramebufferBase _currentFramebuffer;
        private bool _currentFramebufferEverActive;
        private VkRenderPass _activeRenderPass;
        private VkPipeline _currentGraphicsPipeline;
        private VkResourceSet[] _currentGraphicsResourceSets = Array.Empty<VkResourceSet>();
        private bool[] _graphicsResourceSetsChanged;
        private int _newGraphicsResourceSets;

        // Compute State
        private VkPipeline _currentComputePipeline;
        private VkResourceSet[] _currentComputeResourceSets = Array.Empty<VkResourceSet>();
        private bool[] _computeResourceSetsChanged;
        private int _newComputeResourceSets;
        private string _name;

        public VkCommandPool CommandPool => _pool;
        public VkCommandBuffer CommandBuffer => _cb;

        public HashSet<VkDeferredDisposal> ReferencedResources => _referencedResources;

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

            _referencedResources.Clear();
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            VkClearValue clearValue = new VkClearValue
            {
                color = new VkClearColorValue(clearColor.R, clearColor.G, clearColor.B, clearColor.A)
            };

            if (_activeRenderPass != VkRenderPass.Null)
            {
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
            else
            {
                // Queue up the clear value for the next RenderPass.
                _clearValues[index] = clearValue;
                _validColorClearValues[index] = true;
            }
        }

        public override void ClearDepthTarget(float depth)
        {
            VkClearValue clearValue = new VkClearValue { depthStencil = new VkClearDepthStencilValue(depth, 0) };

            if (_activeRenderPass != VkRenderPass.Null)
            {
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
            else
            {
                // Queue up the clear value for the next RenderPass.
                _depthClearValue = clearValue;
            }
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
            _referencedResources.Add(vkBuffer);
        }

        protected override void DrawIndexedIndirectCore(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(indirectBuffer);
            vkCmdDrawIndexedIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
            _referencedResources.Add(vkBuffer);
        }

        private void PreDrawCommand()
        {
            EnsureRenderPassActive();

            FlushNewResourceSets(
                _newGraphicsResourceSets,
                _currentGraphicsResourceSets,
                _graphicsResourceSetsChanged,
                VkPipelineBindPoint.Graphics,
                _currentGraphicsPipeline.PipelineLayout);
            _newGraphicsResourceSets = 0;

            if (!_currentGraphicsPipeline.ScissorTestEnabled)
            {
                SetFullScissorRects();
            }
        }

        private void FlushNewResourceSets(
            int newResourceSetsCount,
            VkResourceSet[] resourceSets,
            bool[] resourceSetsChanged,
            VkPipelineBindPoint bindPoint,
            VkPipelineLayout pipelineLayout)
        {
            if (newResourceSetsCount > 0)
            {
                int totalChanged = 0;
                uint currentSlot = 0;
                uint currentBatchIndex = 0;
                uint currentBatchFirstSet = 0;
                VkDescriptorSet* descriptorSets = stackalloc VkDescriptorSet[newResourceSetsCount];
                while (totalChanged < newResourceSetsCount)
                {
                    if (resourceSetsChanged[currentSlot])
                    {
                        resourceSetsChanged[currentSlot] = false;
                        descriptorSets[currentBatchIndex] = resourceSets[currentSlot].DescriptorSet;
                        totalChanged += 1;
                        currentBatchIndex += 1;
                        currentSlot += 1;
                    }
                    else
                    {
                        if (currentBatchIndex != 0)
                        {
                            // Flush current batch.
                            vkCmdBindDescriptorSets(
                                _cb,
                                bindPoint,
                                pipelineLayout,
                                currentBatchFirstSet,
                                currentBatchIndex,
                                descriptorSets,
                                0,
                                null);
                            currentBatchIndex = 0;
                        }

                        currentSlot += 1;
                        currentBatchFirstSet = currentSlot;
                    }
                }

                if (currentBatchIndex != 0)
                {
                    // Flush current batch.
                    vkCmdBindDescriptorSets(
                        _cb,
                        bindPoint,
                        pipelineLayout,
                        currentBatchFirstSet,
                        currentBatchIndex,
                        descriptorSets,
                        0,
                        null);
                }
            }
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            PreDispatchCommand();

            vkCmdDispatch(_cb, groupCountX, groupCountY, groupCountZ);
        }

        private void PreDispatchCommand()
        {
            EnsureNoRenderPass();

            FlushNewResourceSets(
                _newComputeResourceSets,
                _currentComputeResourceSets,
                _computeResourceSetsChanged,
                VkPipelineBindPoint.Compute,
                _currentComputePipeline.PipelineLayout);
            _newComputeResourceSets = 0;
        }

        protected override void DispatchIndirectCore(Buffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();

            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(indirectBuffer);
            vkCmdDispatchIndirect(_cb, vkBuffer.DeviceBuffer, offset);
            _referencedResources.Add(vkBuffer);
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
                vkSource.OptimalDeviceImage,
                vkSource.GetImageLayout(0, 0),
                vkDestination.OptimalDeviceImage,
                vkDestination.GetImageLayout(0, 0),
                1,
                ref region);

            _referencedResources.Add(vkSource);
            _referencedResources.Add(vkDestination);
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
            else if (!_currentFramebufferEverActive && _currentFramebuffer != null)
            {
                BeginCurrentRenderPass();
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
            _currentFramebufferEverActive = false;
            Util.EnsureArrayMinimumSize(ref _scissorRects, Math.Max(1, (uint)vkFB.ColorTargets.Count));
            uint clearValueCount = (uint)vkFB.ColorTargets.Count;
            Util.EnsureArrayMinimumSize(ref _clearValues, clearValueCount + 1); // Leave an extra space for the depth value (tracked separately).
            Util.ClearArray(_validColorClearValues);
            Util.EnsureArrayMinimumSize(ref _validColorClearValues, clearValueCount);
            _referencedResources.Add(vkFB);
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
            Debug.Assert(_currentFramebuffer != null);
            _currentFramebufferEverActive = true;

            uint attachmentCount = _currentFramebuffer.AttachmentCount;
            bool haveAnyAttachments = _framebuffer.ColorTargets.Count > 0 || _framebuffer.DepthTarget != null;
            bool haveAllClearValues = _depthClearValue.HasValue || _framebuffer.DepthTarget == null;
            bool haveAnyClearValues = _depthClearValue.HasValue;
            for (int i = 0; i < _currentFramebuffer.ColorTargets.Count; i++)
            {
                if (!_validColorClearValues[i])
                {
                    haveAllClearValues = false;
                    haveAnyClearValues = true;
                }
                else
                {
                    haveAnyClearValues = true;
                }
            }

            VkRenderPassBeginInfo renderPassBI = VkRenderPassBeginInfo.New();
            renderPassBI.renderArea = new VkRect2D(_currentFramebuffer.RenderableWidth, _currentFramebuffer.RenderableHeight);
            renderPassBI.framebuffer = _currentFramebuffer.CurrentFramebuffer;

            if (!haveAnyAttachments || !haveAllClearValues)
            {
                renderPassBI.renderPass = _currentFramebuffer.RenderPassNoClear;
                vkCmdBeginRenderPass(_cb, ref renderPassBI, VkSubpassContents.Inline);
                _activeRenderPass = _currentFramebuffer.RenderPassNoClear;

                if (haveAnyClearValues)
                {
                    if (_depthClearValue.HasValue)
                    {
                        ClearDepthTarget(_depthClearValue.Value.depthStencil.depth);
                        _depthClearValue = null;
                    }

                    for (uint i = 0; i < _currentFramebuffer.ColorTargets.Count; i++)
                    {
                        if (_validColorClearValues[i])
                        {
                            _validColorClearValues[i] = false;
                            VkClearValue vkClearValue = _clearValues[i];
                            RgbaFloat clearColor = new RgbaFloat(
                                vkClearValue.color.float32_0,
                                vkClearValue.color.float32_1,
                                vkClearValue.color.float32_2,
                                vkClearValue.color.float32_3);
                            ClearColorTarget(i, clearColor);
                        }
                    }
                }
            }
            else
            {
                // We have clear values for every attachment.
                renderPassBI.renderPass = _currentFramebuffer.RenderPassClear;
                fixed (VkClearValue* clearValuesPtr = &_clearValues[0])
                {
                    renderPassBI.clearValueCount = attachmentCount;
                    renderPassBI.pClearValues = clearValuesPtr;
                    if (_depthClearValue.HasValue)
                    {
                        _clearValues[_currentFramebuffer.ColorTargets.Count] = _depthClearValue.Value;
                    }
                    vkCmdBeginRenderPass(_cb, ref renderPassBI, VkSubpassContents.Inline);
                    _activeRenderPass = _currentFramebuffer.RenderPassClear;
                    Util.ClearArray(_validColorClearValues);
                }
            }
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
            _referencedResources.Add(vkBuffer);
        }

        protected override void SetIndexBufferCore(Buffer buffer, IndexFormat format)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(buffer);
            vkCmdBindIndexBuffer(_cb, vkBuffer.DeviceBuffer, 0, VkFormats.VdToVkIndexFormat(format));
            _referencedResources.Add(vkBuffer);
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _currentGraphicsPipeline != pipeline)
            {
                VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
                Util.EnsureArrayMinimumSize(ref _currentGraphicsResourceSets, vkPipeline.ResourceSetCount);
                Util.ClearArray(_currentGraphicsResourceSets);
                Util.EnsureArrayMinimumSize(ref _graphicsResourceSetsChanged, vkPipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Graphics, vkPipeline.DevicePipeline);
                _currentGraphicsPipeline = vkPipeline;
                _referencedResources.Add(vkPipeline);
            }
            else if (pipeline.IsComputePipeline && _currentComputePipeline != pipeline)
            {
                VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
                Util.EnsureArrayMinimumSize(ref _currentComputeResourceSets, vkPipeline.ResourceSetCount);
                Util.ClearArray(_currentComputeResourceSets);
                Util.EnsureArrayMinimumSize(ref _computeResourceSetsChanged, vkPipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Compute, vkPipeline.DevicePipeline);
                _currentComputePipeline = vkPipeline;
                _referencedResources.Add(vkPipeline);
            }
        }

        protected override void SetGraphicsResourceSetCore(uint slot, ResourceSet rs)
        {
            if (_currentGraphicsResourceSets[slot] != rs)
            {
                VkResourceSet vkRS = Util.AssertSubtype<ResourceSet, VkResourceSet>(rs);
                _currentGraphicsResourceSets[slot] = vkRS;
                _graphicsResourceSetsChanged[slot] = true;
                _newGraphicsResourceSets += 1;
                _referencedResources.Add(vkRS);
            }
        }

        protected override void SetComputeResourceSetCore(uint slot, ResourceSet rs)
        {
            if (_currentComputeResourceSets[slot] != rs)
            {
                VkResourceSet vkRS = Util.AssertSubtype<ResourceSet, VkResourceSet>(rs);
                _currentComputeResourceSets[slot] = vkRS;
                _computeResourceSetsChanged[slot] = true;
                _newComputeResourceSets += 1;
                _referencedResources.Add(vkRS);
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
            // TODO: This should grab a pooled staging buffer, upload the data to that, and then queue a copy from it to the true
            // destination buffer given. Then, it should queue up the buffer to be returned to the pool.
            // Calling GraphicsDevice.UpdateBuffer will not work correctly -- it doesn't queue up the copies into this VkCommandBuffer,
            // it just submits them immediately on a transient command buffer. Therefore, calling this function multiple times on
            // the same CommandList will cause earlier copies to be overwritten.
            _gd.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
            _referencedResources.Add(Util.AssertSubtype<Buffer, VkBuffer>(buffer));
        }

        protected override void CopyBufferCore(Buffer source, uint sourceOffset, Buffer destination, uint destinationOffset, uint sizeInBytes)
        {
            EnsureNoRenderPass();

            VkBuffer srcVkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(source);
            VkBuffer dstVkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(destination);

            VkBufferCopy region = new VkBufferCopy
            {
                srcOffset = sourceOffset,
                dstOffset = destinationOffset,
                size = sizeInBytes
            };

            vkCmdCopyBuffer(_cb, srcVkBuffer.DeviceBuffer, dstVkBuffer.DeviceBuffer, 1, ref region);

            _referencedResources.Add(srcVkBuffer);
            _referencedResources.Add(dstVkBuffer);

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
            EnsureNoRenderPass();

            bool sourceIsStaging = (source.Usage & TextureUsage.Staging) == TextureUsage.Staging;
            bool destIsStaging = (destination.Usage & TextureUsage.Staging) == TextureUsage.Staging;
            if ((destIsStaging || sourceIsStaging) && layerCount > 1)
            {
                // Need to issue one copy per array layer.
                throw new NotImplementedException();
            }

            VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers
            {
                aspectMask = VkImageAspectFlags.Color,
                layerCount = layerCount,
                mipLevel = sourceIsStaging ? 0 : srcMipLevel,
                baseArrayLayer = sourceIsStaging ? 0 : srcBaseArrayLayer
            };

            VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers
            {
                aspectMask = VkImageAspectFlags.Color,
                layerCount = layerCount,
                mipLevel = destIsStaging ? 0 : dstMipLevel,
                baseArrayLayer = destIsStaging ? 0 : dstBaseArrayLayer
            };

            VkImageCopy region = new VkImageCopy
            {
                srcOffset = new VkOffset3D { x = (int)srcX, y = (int)srcY, z = (int)srcZ },
                dstOffset = new VkOffset3D { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                srcSubresource = srcSubresource,
                dstSubresource = dstSubresource,
                extent = new VkExtent3D { width = width, height = height, depth = depth }
            };

            VkTexture srcVkTexture = Util.AssertSubtype<Texture, VkTexture>(source);
            VkTexture dstVkTexture = Util.AssertSubtype<Texture, VkTexture>(destination);

            srcVkTexture.TransitionImageLayout(
                _cb,
                srcMipLevel,
                1,
                srcBaseArrayLayer,
                layerCount,
                VkImageLayout.TransferSrcOptimal);

            dstVkTexture.TransitionImageLayout(
                _cb,
                dstMipLevel,
                1,
                dstBaseArrayLayer,
                layerCount,
                VkImageLayout.TransferDstOptimal);

            VkImage srcImage = sourceIsStaging
                ? srcVkTexture.GetStagingImage(source.CalculateSubresource(srcMipLevel, srcBaseArrayLayer))
                : srcVkTexture.OptimalDeviceImage;
            VkImage dstImage = destIsStaging
                ? dstVkTexture.GetStagingImage(destination.CalculateSubresource(dstMipLevel, dstBaseArrayLayer))
                : dstVkTexture.OptimalDeviceImage;

            vkCmdCopyImage(
                _cb,
                srcImage,
                VkImageLayout.TransferSrcOptimal,
                dstImage,
                VkImageLayout.TransferDstOptimal,
                1,
                ref region);

            _referencedResources.Add(srcVkTexture);
            _referencedResources.Add(dstVkTexture);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposedCommandBuffer(this);
        }

        // Must only be called once the command buffer has fully executed.
        public void DestroyCommandPool()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                vkDestroyCommandPool(_gd.Device, _pool, null);
            }
        }
    }
}
using System;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Veldrid.Vk
{
    internal unsafe class VkCommandList : CommandList
    {
        private readonly VkGraphicsDevice _gd;
        private VkCommandPool _pool;
        private VkCommandBuffer _cb;
        private bool _destroyed;

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

        private bool _newFramebuffer; // Render pass cycle state

        // Compute State
        private VkPipeline _currentComputePipeline;
        private VkResourceSet[] _currentComputeResourceSets = Array.Empty<VkResourceSet>();
        private bool[] _computeResourceSetsChanged;
        private int _newComputeResourceSets;
        private string _name;

        private readonly object _commandBufferListLock = new object();
        private readonly Queue<VkCommandBuffer> _availableCommandBuffers = new Queue<VkCommandBuffer>();
        private readonly List<VkCommandBuffer> _submittedCommandBuffers = new List<VkCommandBuffer>();

        private StagingResourceInfo _currentStagingInfo;
        private readonly object _stagingLock = new object();
        private readonly Dictionary<VkCommandBuffer, StagingResourceInfo> _submittedStagingInfos = new Dictionary<VkCommandBuffer, StagingResourceInfo>();
        private readonly List<StagingResourceInfo> _availableStagingInfos = new List<StagingResourceInfo>();
        private readonly List<VkBuffer> _availableStagingBuffers = new List<VkBuffer>();

        public VkCommandPool CommandPool => _pool;
        public VkCommandBuffer CommandBuffer => _cb;
        public uint SubmittedCommandBufferCount { get; private set; }

        public VkCommandList(VkGraphicsDevice gd, ref CommandListDescription description)
            : base(ref description, gd.Features)
        {
            _gd = gd;
            VkCommandPoolCreateInfo poolCI = VkCommandPoolCreateInfo.New();
            poolCI.flags = VkCommandPoolCreateFlags.ResetCommandBuffer;
            poolCI.queueFamilyIndex = gd.GraphicsQueueIndex;
            VkResult result = vkCreateCommandPool(_gd.Device, ref poolCI, null, out _pool);
            CheckResult(result);

            _cb = GetNextCommandBuffer();
        }

        private VkCommandBuffer GetNextCommandBuffer()
        {
            lock (_commandBufferListLock)
            {
                if (_availableCommandBuffers.Count > 0)
                {
                    VkCommandBuffer cachedCB = _availableCommandBuffers.Dequeue();
                    VkResult resetResult = vkResetCommandBuffer(cachedCB, VkCommandBufferResetFlags.None);
                    CheckResult(resetResult);
                    return cachedCB;
                }
            }

            VkCommandBufferAllocateInfo cbAI = VkCommandBufferAllocateInfo.New();
            cbAI.commandPool = _pool;
            cbAI.commandBufferCount = 1;
            cbAI.level = VkCommandBufferLevel.Primary;
            VkResult result = vkAllocateCommandBuffers(_gd.Device, ref cbAI, out VkCommandBuffer cb);
            CheckResult(result);
            return cb;
        }

        public void CommandBufferSubmitted(VkCommandBuffer cb)
        {
            SubmittedCommandBufferCount += 1;
            _submittedStagingInfos.Add(cb, _currentStagingInfo);
            _currentStagingInfo = null;
        }

        public void CommandBufferCompleted(VkCommandBuffer completedCB)
        {
            SubmittedCommandBufferCount -= 1;

            lock (_commandBufferListLock)
            {
                for (int i = 0; i < _submittedCommandBuffers.Count; i++)
                {
                    VkCommandBuffer submittedCB = _submittedCommandBuffers[i];
                    if (submittedCB == completedCB)
                    {
                        _availableCommandBuffers.Enqueue(completedCB);
                        _submittedCommandBuffers.RemoveAt(i);
                        i -= 1;
                    }
                }
            }

            lock (_stagingLock)
            {
                if (_submittedStagingInfos.TryGetValue(completedCB, out StagingResourceInfo info))
                {
                    RecycleStagingInfo(info);
                }
                _submittedStagingInfos.Remove(completedCB);
            }
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
                _cb = GetNextCommandBuffer();
                if (_currentStagingInfo != null)
                {
                    RecycleStagingInfo(_currentStagingInfo);
                }
            }

            _currentStagingInfo = GetStagingResourceInfo();

            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;
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

        protected override void ClearColorTargetCore(uint index, RgbaFloat clearColor)
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

        protected override void ClearDepthStencilCore(float depth, byte stencil)
        {
            VkClearValue clearValue = new VkClearValue { depthStencil = new VkClearDepthStencilValue(depth, stencil) };

            if (_activeRenderPass != VkRenderPass.Null)
            {
                VkImageAspectFlags aspect = FormatHelpers.IsStencilFormat(_currentFramebuffer.DepthTarget.Value.Target.Format)
                    ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                    : VkImageAspectFlags.Depth;
                VkClearAttachment clearAttachment = new VkClearAttachment
                {
                    aspectMask = aspect,
                    clearValue = clearValue
                };

                Texture depthTex = _currentFramebuffer.DepthTarget.Value.Target;
                uint renderableWidth = _currentFramebuffer.RenderableWidth;
                uint renderableHeight = _currentFramebuffer.RenderableHeight;
                if (renderableWidth > 0 && renderableHeight > 0)
                {
                    VkClearRect clearRect = new VkClearRect
                    {
                        baseArrayLayer = 0,
                        layerCount = 1,
                        rect = new VkRect2D(0, 0, renderableWidth, renderableHeight)
                    };

                    vkCmdClearAttachments(_cb, 1, ref clearAttachment, 1, ref clearRect);
                }
            }
            else
            {
                // Queue up the clear value for the next RenderPass.
                _depthClearValue = clearValue;
            }
        }

        protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDraw(_cb, vertexCount, instanceCount, vertexStart, instanceStart);
        }

        protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDrawIndexed(_cb, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            vkCmdDrawIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            vkCmdDrawIndexedIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
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

        protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();

            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
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
                ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                : VkImageAspectFlags.Color;
            VkImageResolve region = new VkImageResolve
            {
                extent = new VkExtent3D { width = source.Width, height = source.Height, depth = source.Depth },
                srcSubresource = new VkImageSubresourceLayers { layerCount = 1, aspectMask = aspectFlags },
                dstSubresource = new VkImageSubresourceLayers { layerCount = 1, aspectMask = aspectFlags }
            };

            vkSource.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.TransferSrcOptimal);
            vkDestination.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.TransferDstOptimal);

            vkCmdResolveImage(
                _cb,
                vkSource.OptimalDeviceImage,
                 VkImageLayout.TransferSrcOptimal,
                vkDestination.OptimalDeviceImage,
                VkImageLayout.TransferDstOptimal,
                1,
                ref region);

            if ((vkDestination.Usage & TextureUsage.Sampled) != 0)
            {
                vkDestination.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.ShaderReadOnlyOptimal);
            }
        }

        public override void End()
        {
            if (!_commandBufferBegun)
            {
                throw new VeldridException("CommandBuffer must have been started before End() may be called.");
            }

            _commandBufferBegun = false;
            _commandBufferEnded = true;

            if (!_currentFramebufferEverActive && _currentFramebuffer != null)
            {
                BeginCurrentRenderPass();
            }
            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
                _currentFramebuffer.TransitionToFinalLayout(_cb);
            }

            vkEndCommandBuffer(_cb);
            _submittedCommandBuffers.Add(_cb);
        }

        protected override void SetFramebufferCore(Framebuffer fb)
        {
            if (_activeRenderPass.Handle != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }
            else if (!_currentFramebufferEverActive && _currentFramebuffer != null)
            {
                // This forces any queued up texture clears to be emitted.
                BeginCurrentRenderPass();
                EndCurrentRenderPass();
            }

            if (_currentFramebuffer != null)
            {
                _currentFramebuffer.TransitionToFinalLayout(_cb);
            }

            VkFramebufferBase vkFB = Util.AssertSubtype<Framebuffer, VkFramebufferBase>(fb);
            _currentFramebuffer = vkFB;
            _currentFramebufferEverActive = false;
            _newFramebuffer = true;
            Util.EnsureArrayMinimumSize(ref _scissorRects, Math.Max(1, (uint)vkFB.ColorTargets.Count));
            uint clearValueCount = (uint)vkFB.ColorTargets.Count;
            Util.EnsureArrayMinimumSize(ref _clearValues, clearValueCount + 1); // Leave an extra space for the depth value (tracked separately).
            Util.ClearArray(_validColorClearValues);
            Util.EnsureArrayMinimumSize(ref _validColorClearValues, clearValueCount);
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
            bool haveAnyAttachments = _currentFramebuffer.ColorTargets.Count > 0 || _currentFramebuffer.DepthTarget != null;
            bool haveAllClearValues = _depthClearValue.HasValue || _currentFramebuffer.DepthTarget == null;
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
                renderPassBI.renderPass = _newFramebuffer
                    ? _currentFramebuffer.RenderPassNoClear_Init
                    : _currentFramebuffer.RenderPassNoClear_Load;
                vkCmdBeginRenderPass(_cb, ref renderPassBI, VkSubpassContents.Inline);
                _activeRenderPass = renderPassBI.renderPass;

                if (haveAnyClearValues)
                {
                    if (_depthClearValue.HasValue)
                    {
                        ClearDepthStencil(_depthClearValue.Value.depthStencil.depth, (byte)_depthClearValue.Value.depthStencil.stencil);
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
                        _depthClearValue = null;
                    }
                    vkCmdBeginRenderPass(_cb, ref renderPassBI, VkSubpassContents.Inline);
                    _activeRenderPass = _currentFramebuffer.RenderPassClear;
                    Util.ClearArray(_validColorClearValues);
                }
            }

            _newFramebuffer = false;
        }

        private void EndCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass != VkRenderPass.Null);
            vkCmdEndRenderPass(_cb);
            _currentFramebuffer.TransitionToIntermediateLayout(_cb);
            _activeRenderPass = VkRenderPass.Null;

            // Place a barrier between RenderPasses, so that color / depth outputs
            // can be read in subsequent passes.
            vkCmdPipelineBarrier(
                _cb,
                VkPipelineStageFlags.BottomOfPipe,
                VkPipelineStageFlags.TopOfPipe,
                VkDependencyFlags.None,
                0,
                null,
                0,
                null,
                0,
                null);
        }

        protected override void SetVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            Vulkan.VkBuffer deviceBuffer = vkBuffer.DeviceBuffer;
            ulong offset64 = offset;
            vkCmdBindVertexBuffers(_cb, index, 1, ref deviceBuffer, ref offset64);
        }

        protected override void SetIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            vkCmdBindIndexBuffer(_cb, vkBuffer.DeviceBuffer, offset, VkFormats.VdToVkIndexFormat(format));
        }

        protected override void SetPipelineCore(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _currentGraphicsPipeline != pipeline)
            {
                VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
                Util.EnsureArrayMinimumSize(ref _currentGraphicsResourceSets, vkPipeline.ResourceSetCount);
                Util.ClearArray(_currentGraphicsResourceSets);
                Util.EnsureArrayMinimumSize(ref _graphicsResourceSetsChanged, vkPipeline.ResourceSetCount);
                _newGraphicsResourceSets = 0;
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Graphics, vkPipeline.DevicePipeline);
                _currentGraphicsPipeline = vkPipeline;
            }
            else if (pipeline.IsComputePipeline && _currentComputePipeline != pipeline)
            {
                VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
                Util.EnsureArrayMinimumSize(ref _currentComputeResourceSets, vkPipeline.ResourceSetCount);
                Util.ClearArray(_currentComputeResourceSets);
                Util.EnsureArrayMinimumSize(ref _computeResourceSetsChanged, vkPipeline.ResourceSetCount);
                _newComputeResourceSets = 0;
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
                _newGraphicsResourceSets += 1;
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
            }
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            if (index == 0 || _gd.Features.MultipleViewports)
            {
                VkRect2D scissor = new VkRect2D((int)x, (int)y, (int)width, (int)height);
                if (_scissorRects[index] != scissor)
                {
                    _scissorRects[index] = scissor;
                    vkCmdSetScissor(_cb, index, 1, ref scissor);
                }
            }
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            if (index == 0 || _gd.Features.MultipleViewports)
            {
                float vpY = _gd.IsClipSpaceYInverted
                    ? viewport.Y
                    : viewport.Height + viewport.Y;
                float vpHeight = _gd.IsClipSpaceYInverted
                    ? viewport.Height
                    : -viewport.Height;

                VkViewport vkViewport = new VkViewport
                {
                    x = viewport.X,
                    y = vpY,
                    width = viewport.Width,
                    height = vpHeight,
                    minDepth = viewport.MinDepth,
                    maxDepth = viewport.MaxDepth
                };

                vkCmdSetViewport(_cb, index, 1, ref vkViewport);
            }
        }

        public override void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            VkBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);
            _gd.UpdateBuffer(stagingBuffer, 0, source, sizeInBytes);
            CopyBuffer(stagingBuffer, 0, buffer, bufferOffsetInBytes, sizeInBytes);
        }

        protected override void CopyBufferCore(
            DeviceBuffer source,
            uint sourceOffset,
            DeviceBuffer destination,
            uint destinationOffset,
            uint sizeInBytes)
        {
            EnsureNoRenderPass();

            VkBuffer srcVkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(source);
            VkBuffer dstVkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(destination);

            VkBufferCopy region = new VkBufferCopy
            {
                srcOffset = sourceOffset,
                dstOffset = destinationOffset,
                size = sizeInBytes
            };

            vkCmdCopyBuffer(_cb, srcVkBuffer.DeviceBuffer, dstVkBuffer.DeviceBuffer, 1, ref region);
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
            CopyTextureCore_VkCommandBuffer(
                _cb,
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth, layerCount);
        }

        internal static void CopyTextureCore_VkCommandBuffer(
            VkCommandBuffer cb,
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
            VkTexture srcVkTexture = Util.AssertSubtype<Texture, VkTexture>(source);
            VkTexture dstVkTexture = Util.AssertSubtype<Texture, VkTexture>(destination);

            bool sourceIsStaging = (source.Usage & TextureUsage.Staging) == TextureUsage.Staging;
            bool destIsStaging = (destination.Usage & TextureUsage.Staging) == TextureUsage.Staging;

            if (!sourceIsStaging && !destIsStaging)
            {
                VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = srcMipLevel,
                    baseArrayLayer = srcBaseArrayLayer
                };

                VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = dstMipLevel,
                    baseArrayLayer = dstBaseArrayLayer
                };

                VkImageCopy region = new VkImageCopy
                {
                    srcOffset = new VkOffset3D { x = (int)srcX, y = (int)srcY, z = (int)srcZ },
                    dstOffset = new VkOffset3D { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    srcSubresource = srcSubresource,
                    dstSubresource = dstSubresource,
                    extent = new VkExtent3D { width = width, height = height, depth = depth }
                };

                srcVkTexture.TransitionImageLayout(
                    cb,
                    srcMipLevel,
                    1,
                    srcBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferSrcOptimal);

                dstVkTexture.TransitionImageLayout(
                    cb,
                    dstMipLevel,
                    1,
                    dstBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferDstOptimal);

                vkCmdCopyImage(
                    cb,
                    srcVkTexture.OptimalDeviceImage,
                    VkImageLayout.TransferSrcOptimal,
                    dstVkTexture.OptimalDeviceImage,
                    VkImageLayout.TransferDstOptimal,
                    1,
                    ref region);
            }
            else if (sourceIsStaging && !destIsStaging)
            {
                Vulkan.VkBuffer srcBuffer = srcVkTexture.StagingBuffer;
                VkSubresourceLayout srcLayout = srcVkTexture.GetSubresourceLayout(
                    srcVkTexture.CalculateSubresource(srcMipLevel, srcBaseArrayLayer));
                VkImage dstImage = dstVkTexture.OptimalDeviceImage;
                dstVkTexture.TransitionImageLayout(
                    cb,
                    dstMipLevel,
                    1,
                    dstBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferDstOptimal);

                VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = dstMipLevel,
                    baseArrayLayer = dstBaseArrayLayer
                };

                Util.GetMipDimensions(srcVkTexture, srcMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                uint blockSize = FormatHelpers.IsCompressedFormat(srcVkTexture.Format) ? 4u : 1u;
                uint bufferRowLength = Math.Max(mipWidth, blockSize);
                uint bufferImageHeight = Math.Max(mipHeight, blockSize);
                uint compressedX = srcX / blockSize;
                uint compressedY = srcY / blockSize;
                uint blockSizeInBytes = blockSize == 1
                    ? FormatHelpers.GetSizeInBytes(srcVkTexture.Format)
                    : FormatHelpers.GetBlockSizeInBytes(srcVkTexture.Format);
                uint rowPitch = FormatHelpers.GetRowPitch(bufferRowLength, srcVkTexture.Format);
                uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, bufferImageHeight, srcVkTexture.Format);

                VkBufferImageCopy regions = new VkBufferImageCopy
                {
                    bufferOffset = srcLayout.offset
                        + (srcZ * depthPitch)
                        + (compressedY * rowPitch)
                        + (compressedX * blockSizeInBytes),
                    bufferRowLength = bufferRowLength,
                    bufferImageHeight = bufferImageHeight,
                    imageExtent = new VkExtent3D { width = width, height = height, depth = depth },
                    imageOffset = new VkOffset3D { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    imageSubresource = dstSubresource
                };

                vkCmdCopyBufferToImage(cb, srcBuffer, dstImage, VkImageLayout.TransferDstOptimal, 1, ref regions);
            }
            else if (!sourceIsStaging && destIsStaging)
            {
                VkImage srcImage = srcVkTexture.OptimalDeviceImage;
                srcVkTexture.TransitionImageLayout(
                    cb,
                    srcMipLevel,
                    1,
                    srcBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferSrcOptimal);

                Vulkan.VkBuffer dstBuffer = dstVkTexture.StagingBuffer;
                VkSubresourceLayout dstLayout = dstVkTexture.GetSubresourceLayout(
                    dstVkTexture.CalculateSubresource(dstMipLevel, dstBaseArrayLayer));
                VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = srcMipLevel,
                    baseArrayLayer = srcBaseArrayLayer
                };

                Util.GetMipDimensions(dstVkTexture, dstMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                VkBufferImageCopy region = new VkBufferImageCopy
                {
                    bufferRowLength = mipWidth,
                    bufferImageHeight = mipHeight,
                    bufferOffset = dstLayout.offset + (dstX * FormatHelpers.GetSizeInBytes(dstVkTexture.Format)),
                    imageExtent = new VkExtent3D { width = width, height = height, depth = depth },
                    imageOffset = new VkOffset3D { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    imageSubresource = srcSubresource
                };

                vkCmdCopyImageToBuffer(cb, srcImage, VkImageLayout.TransferSrcOptimal, dstBuffer, 1, ref region);
            }
            else
            {
                Debug.Assert(sourceIsStaging && destIsStaging);
                Vulkan.VkBuffer srcBuffer = srcVkTexture.StagingBuffer;
                VkSubresourceLayout srcLayout = srcVkTexture.GetSubresourceLayout(
                    srcVkTexture.CalculateSubresource(srcMipLevel, srcBaseArrayLayer));
                Vulkan.VkBuffer dstBuffer = dstVkTexture.StagingBuffer;
                VkSubresourceLayout dstLayout = dstVkTexture.GetSubresourceLayout(
                    dstVkTexture.CalculateSubresource(dstMipLevel, dstBaseArrayLayer));

                uint zLimit = Math.Max(depth, layerCount);
                if (!FormatHelpers.IsCompressedFormat(source.Format))
                {
                    uint pixelSize = FormatHelpers.GetSizeInBytes(srcVkTexture.Format);
                    for (uint zz = 0; zz < zLimit; zz++)
                    {
                        for (uint yy = 0; yy < height; yy++)
                        {
                            VkBufferCopy region = new VkBufferCopy
                            {
                                srcOffset = srcLayout.offset
                                    + srcLayout.depthPitch * (zz + srcZ)
                                    + srcLayout.rowPitch * (yy + srcY)
                                    + pixelSize * srcX,
                                dstOffset = dstLayout.offset
                                    + dstLayout.depthPitch * (zz + dstZ)
                                    + dstLayout.rowPitch * (yy + dstY)
                                    + pixelSize * dstX,
                                size = width * pixelSize,
                            };

                            vkCmdCopyBuffer(cb, srcBuffer, dstBuffer, 1, ref region);
                        }
                    }
                }
                else // IsCompressedFormat
                {
                    uint denseRowSize = FormatHelpers.GetRowPitch(width, source.Format);
                    uint numRows = FormatHelpers.GetNumRows(height, source.Format);
                    uint compressedSrcX = srcX / 4;
                    uint compressedSrcY = srcY / 4;
                    uint compressedDstX = dstX / 4;
                    uint compressedDstY = dstY / 4;
                    uint blockSizeInBytes = FormatHelpers.GetBlockSizeInBytes(source.Format);

                    for (uint zz = 0; zz < zLimit; zz++)
                    {
                        for (uint row = 0; row < numRows; row++)
                        {
                            VkBufferCopy region = new VkBufferCopy
                            {
                                srcOffset = srcLayout.offset
                                    + srcLayout.depthPitch * (zz + srcZ)
                                    + srcLayout.rowPitch * (row + compressedSrcY)
                                    + blockSizeInBytes * compressedSrcX,
                                dstOffset = dstLayout.offset
                                    + dstLayout.depthPitch * (zz + dstZ)
                                    + dstLayout.rowPitch * (row + compressedDstY)
                                    + blockSizeInBytes * compressedDstX,
                                size = denseRowSize,
                            };

                            vkCmdCopyBuffer(cb, srcBuffer, dstBuffer, 1, ref region);
                        }
                    }

                }
            }
        }

        protected override void GenerateMipmapsCore(Texture texture)
        {
            EnsureNoRenderPass();
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            vkTex.TransitionImageLayout(_cb, 0, 1, 0, vkTex.ArrayLayers, VkImageLayout.TransferSrcOptimal);
            vkTex.TransitionImageLayout(_cb, 1, vkTex.MipLevels - 1, 0, vkTex.ArrayLayers, VkImageLayout.TransferDstOptimal);

            VkImage deviceImage = vkTex.OptimalDeviceImage;

            uint blitCount = vkTex.MipLevels - 1;
            VkImageBlit* regions = stackalloc VkImageBlit[(int)blitCount];

            for (uint level = 1; level < vkTex.MipLevels; level++)
            {
                uint blitIndex = level - 1;

                regions[blitIndex].srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    baseArrayLayer = 0,
                    layerCount = vkTex.ArrayLayers,
                    mipLevel = 0
                };
                regions[blitIndex].srcOffsets_0 = new VkOffset3D();
                regions[blitIndex].srcOffsets_1 = new VkOffset3D { x = (int)vkTex.Width, y = (int)vkTex.Height, z = (int)vkTex.Depth };
                regions[blitIndex].dstOffsets_0 = new VkOffset3D();

                regions[blitIndex].dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    baseArrayLayer = 0,
                    layerCount = vkTex.ArrayLayers,
                    mipLevel = level
                };

                Util.GetMipDimensions(vkTex, level, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                regions[blitIndex].dstOffsets_1 = new VkOffset3D { x = (int)mipWidth, y = (int)mipHeight, z = (int)mipDepth };
            }

            vkCmdBlitImage(
                _cb,
                deviceImage, VkImageLayout.TransferSrcOptimal,
                deviceImage, VkImageLayout.TransferDstOptimal,
                blitCount, regions,
                _gd.GetFormatFilter(vkTex.VkFormat));

            if ((vkTex.Usage & TextureUsage.Sampled) != 0)
            {
                // This is somewhat ugly -- the transition logic does not handle different source layouts, so we do two batches.
                vkTex.TransitionImageLayout(_cb, 0, 1, 0, vkTex.ArrayLayers, VkImageLayout.ShaderReadOnlyOptimal);
                vkTex.TransitionImageLayout(_cb, 1, vkTex.MipLevels - 1, 0, vkTex.ArrayLayers, VkImageLayout.ShaderReadOnlyOptimal);
            }
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

        private VkBuffer GetStagingBuffer(uint size)
        {
            lock (_stagingLock)
            {
                VkBuffer ret = null;
                foreach (VkBuffer buffer in _availableStagingBuffers)
                {
                    if (buffer.SizeInBytes >= size)
                    {
                        ret = buffer;
                        _availableStagingBuffers.Remove(buffer);
                        break; ;
                    }
                }
                if (ret == null)
                {
                    ret = (VkBuffer)_gd.ResourceFactory.CreateBuffer(new BufferDescription(size, BufferUsage.Staging));
                }

                _currentStagingInfo.BuffersUsed.Add(ret);
                return ret;
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

                Debug.Assert(_submittedStagingInfos.Count == 0);

                foreach (VkBuffer buffer in _availableStagingBuffers)
                {
                    buffer.Dispose();
                }
            }
        }

        private class StagingResourceInfo
        {
            public List<VkBuffer> BuffersUsed { get; } = new List<VkBuffer>();
            public void Clear() => BuffersUsed.Clear();
        }

        private StagingResourceInfo GetStagingResourceInfo()
        {
            lock (_stagingLock)
            {
                StagingResourceInfo ret;
                int availableCount = _availableStagingInfos.Count;
                if (availableCount > 0)
                {
                    ret = _availableStagingInfos[availableCount - 1];
                    _availableStagingInfos.RemoveAt(availableCount - 1);
                }
                else
                {
                    ret = new StagingResourceInfo();
                }

                return ret;
            }
        }

        private void RecycleStagingInfo(StagingResourceInfo info)
        {
            lock (_stagingLock)
            {
                foreach (VkBuffer buffer in info.BuffersUsed)
                {
                    _availableStagingBuffers.Add(buffer);
                }
                info.Clear();

                _availableStagingInfos.Add(info);
            }
        }
    }
}

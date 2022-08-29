using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;
using VkImageLayout = TerraFX.Interop.Vulkan.VkImageLayout;
using VulkanBuffer = TerraFX.Interop.Vulkan.VkBuffer;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkCommandList : CommandList, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private VkCommandPool _pool;
        private VkCommandBuffer _cb;
        private bool _destroyed;

        private bool _commandBufferBegun;
        private bool _commandBufferEnded;

        private uint _viewportCount;
        private bool _viewportsChanged = false;
        private VkViewport[] _viewports = Array.Empty<VkViewport>();
        private bool _scissorRectsChanged = false;
        private VkRect2D[] _scissorRects = Array.Empty<VkRect2D>();

        private VkClearValue[] _clearValues = Array.Empty<VkClearValue>();
        private bool[] _validColorClearValues = Array.Empty<bool>();
        private VkClearValue? _depthClearValue;
        private readonly List<VkTexture> _preDrawSampledImages = new();

        // Graphics State
        private VkFramebufferBase? _currentFramebuffer;
        private bool _currentFramebufferEverActive;
        private VkRenderPass _activeRenderPass;
        private VkPipeline? _currentGraphicsPipeline;
        private BoundResourceSetInfo[] _currentGraphicsResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _graphicsResourceSetsChanged = Array.Empty<bool>();

        private bool _newFramebuffer; // Render pass cycle state

        private bool _vertexBindingsChanged = false;
        private uint _numVertexBindings = 0;
        private VulkanBuffer[] _vertexBindings = new VulkanBuffer[1];
        private ulong[] _vertexOffsets = new ulong[1];

        // Compute State
        private VkPipeline? _currentComputePipeline;
        private BoundResourceSetInfo[] _currentComputeResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _computeResourceSetsChanged = Array.Empty<bool>();
        private string? _name;

        private readonly object _commandBufferListLock = new();
        private readonly Stack<VkCommandBuffer> _availableCommandBuffers = new();
        private readonly List<VkCommandBuffer> _submittedCommandBuffers = new();

        private StagingResourceInfo _currentStagingInfo;
        private readonly Dictionary<VkCommandBuffer, StagingResourceInfo> _submittedStagingInfos = new();
        private readonly ConcurrentQueue<StagingResourceInfo> _availableStagingInfos = new();
        private readonly List<VkBuffer> _availableStagingBuffers = new();

        public VkCommandPool CommandPool => _pool;

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkCommandList(VkGraphicsDevice gd, in CommandListDescription description)
            : base(description, gd.Features, gd.UniformBufferMinOffsetAlignment, gd.StructuredBufferMinOffsetAlignment)
        {
            _gd = gd;
            VkCommandPoolCreateInfo poolCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
                flags = VkCommandPoolCreateFlags.VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
                queueFamilyIndex = gd.GraphicsQueueIndex
            };

            VkCommandPool pool;
            VkResult result = vkCreateCommandPool(_gd.Device, &poolCI, null, &pool);
            CheckResult(result);
            _pool = pool;

            _cb = GetNextCommandBuffer();
            RefCount = new ResourceRefCount(this);
        }

        private VkCommandBuffer GetNextCommandBuffer()
        {
            lock (_commandBufferListLock)
            {
                if (_availableCommandBuffers.TryPop(out VkCommandBuffer cachedCB))
                {
                    VkResult resetResult = vkResetCommandBuffer(cachedCB, 0);
                    CheckResult(resetResult);
                    return cachedCB;
                }
            }

            VkCommandBufferAllocateInfo cbAI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
                commandPool = _pool,
                commandBufferCount = 1,
                level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY
            };
            VkCommandBuffer cb;
            VkResult result = vkAllocateCommandBuffers(_gd.Device, &cbAI, &cb);
            CheckResult(result);
            return cb;
        }

        public VkCommandBuffer CommandBufferSubmitted()
        {
            RefCount.Increment();

            VkCommandBuffer cb = _cb;

            lock (_commandBufferListLock)
            {
                if (!_submittedStagingInfos.TryAdd(cb, _currentStagingInfo))
                {
                    throw new InvalidOperationException();
                }
                _submittedCommandBuffers.Add(cb);
            }

            _currentStagingInfo = default;
            _cb = default;

            return cb;
        }

        public void CommandBufferCompleted(VkCommandBuffer completedCB)
        {
            lock (_commandBufferListLock)
            {
                for (int i = 0; i < _submittedCommandBuffers.Count; i++)
                {
                    VkCommandBuffer submittedCB = _submittedCommandBuffers[i];
                    if (submittedCB == completedCB)
                    {
                        _availableCommandBuffers.Push(completedCB);
                        _submittedCommandBuffers.RemoveAt(i);
                        i -= 1;
                    }
                }

                if (!_submittedStagingInfos.Remove(completedCB, out StagingResourceInfo info))
                {
                    throw new InvalidOperationException();
                }
                RecycleStagingInfo(info);
            }

            RefCount.Decrement();
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

                if (_cb == VkCommandBuffer.NULL)
                {
                    _cb = GetNextCommandBuffer();
                }
                else
                {
                    VkResult resetResult = vkResetCommandBuffer(_cb, 0);
                    CheckResult(resetResult);
                }

                if (_currentStagingInfo.IsValid)
                {
                    RecycleStagingInfo(_currentStagingInfo);
                }
            }

            _currentStagingInfo = GetStagingResourceInfo();

            VkCommandBufferBeginInfo beginInfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
                flags = VkCommandBufferUsageFlags.VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT
            };
            VkResult result = vkBeginCommandBuffer(_cb, &beginInfo);
            CheckResult(result);
            _commandBufferBegun = true;

            ClearCachedState();
            _currentFramebuffer = null;
            _currentGraphicsPipeline = null;
            ClearSets(_currentGraphicsResourceSets);
            Util.ClearArray(_scissorRects);

            _numVertexBindings = 0;
            Util.ClearArray(_vertexBindings);
            Util.ClearArray(_vertexOffsets);

            _currentComputePipeline = null;
            ClearSets(_currentComputeResourceSets);
        }

        private protected override void ClearColorTargetCore(uint index, RgbaFloat clearColor)
        {
            VkClearValue clearValue = new();
            clearValue.color.float32[0] = clearColor.R;
            clearValue.color.float32[1] = clearColor.G;
            clearValue.color.float32[2] = clearColor.B;
            clearValue.color.float32[3] = clearColor.A;

            if (_activeRenderPass != VkRenderPass.NULL)
            {
                VkClearAttachment clearAttachment = new()
                {
                    colorAttachment = index,
                    aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                    clearValue = clearValue
                };

                Texture colorTex = _currentFramebuffer!.ColorTargets[(int)index].Target;

                VkClearRect clearRect = new()
                {
                    baseArrayLayer = 0,
                    layerCount = 1,
                    rect = new VkRect2D()
                    {
                        offset = new VkOffset2D(),
                        extent = new VkExtent2D() { width = colorTex.Width, height = colorTex.Height }
                    }
                };
                vkCmdClearAttachments(_cb, 1, &clearAttachment, 1, &clearRect);
            }
            else
            {
                // Queue up the clear value for the next RenderPass.
                _clearValues[index] = clearValue;
                _validColorClearValues[index] = true;
            }
        }

        private protected override void ClearDepthStencilCore(float depth, byte stencil)
        {
            VkClearValue clearValue = new()
            {
                depthStencil = new VkClearDepthStencilValue()
                {
                    depth = depth,
                    stencil = stencil
                }
            };

            if (_activeRenderPass != VkRenderPass.NULL)
            {
                VkImageAspectFlags aspect = FormatHelpers.IsStencilFormat(_currentFramebuffer!.DepthTarget!.Value.Target.Format)
                    ? VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT | VkImageAspectFlags.VK_IMAGE_ASPECT_STENCIL_BIT
                    : VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT;
                VkClearAttachment clearAttachment = new()
                {
                    aspectMask = aspect,
                    clearValue = clearValue
                };

                uint renderableWidth = _currentFramebuffer.RenderableWidth;
                uint renderableHeight = _currentFramebuffer.RenderableHeight;
                if (renderableWidth > 0 && renderableHeight > 0)
                {
                    VkClearRect clearRect = new()
                    {
                        baseArrayLayer = 0,
                        layerCount = 1,
                        rect = new VkRect2D()
                        {
                            offset = new VkOffset2D(),
                            extent = new VkExtent2D() { width = renderableWidth, height = renderableHeight }
                        }
                    };
                    vkCmdClearAttachments(_cb, 1, &clearAttachment, 1, &clearRect);
                }
            }
            else
            {
                // Queue up the clear value for the next RenderPass.
                _depthClearValue = clearValue;
            }
        }

        private protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDraw(_cb, vertexCount, instanceCount, vertexStart, instanceStart);
        }

        private protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDrawIndexed(_cb, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            _currentStagingInfo.AddResource(vkBuffer.RefCount);
            vkCmdDrawIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            _currentStagingInfo.AddResource(vkBuffer.RefCount);
            vkCmdDrawIndexedIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        private void PreDrawCommand()
        {
            if (_viewportsChanged)
            {
                _viewportsChanged = false;
                FlushViewports();
            }

            if (_scissorRectsChanged)
            {
                _scissorRectsChanged = false;
                FlushScissorRects();
            }

            if (_vertexBindingsChanged)
            {
                _vertexBindingsChanged = false;
                FlushVertexBindings();
            }

            TransitionImages(_preDrawSampledImages, VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
            _preDrawSampledImages.Clear();

            EnsureRenderPassActive();

            FlushNewResourceSets(
                _currentGraphicsResourceSets,
                _graphicsResourceSetsChanged,
                (int)_currentGraphicsPipeline!.ResourceSetCount,
                VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS,
                _currentGraphicsPipeline.PipelineLayout);
        }

        private void FlushVertexBindings()
        {
            fixed (VulkanBuffer* vertexBindings = _vertexBindings)
            fixed (ulong* vertexOffsets = _vertexOffsets)
            {
                vkCmdBindVertexBuffers(
                    _cb,
                    0, _numVertexBindings,
                    vertexBindings,
                    vertexOffsets);
            }
        }

        private void FlushViewports()
        {
            uint count = _viewportCount;
            if (count > 1 && !_gd.Features.MultipleViewports)
            {
                count = 1;
            }

            fixed (VkViewport* viewports = _viewports)
            {
                vkCmdSetViewport(_cb, 0, count, viewports);
            }
        }

        private void FlushScissorRects()
        {
            uint count = _viewportCount;
            if (count > 1 && !_gd.Features.MultipleViewports)
            {
                count = 1;
            }

            fixed (VkRect2D* scissorRects = _scissorRects)
            {
                vkCmdSetScissor(_cb, 0, count, scissorRects);
            }
        }

        private void FlushNewResourceSets(
            BoundResourceSetInfo[] resourceSets,
            bool[] resourceSetsChanged,
            int resourceSetCount,
            VkPipelineBindPoint bindPoint,
            VkPipelineLayout pipelineLayout)
        {
            VkPipeline pipeline = bindPoint == VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS
                ? _currentGraphicsPipeline!
                : _currentComputePipeline!;

            VkDescriptorSet* descriptorSets = stackalloc VkDescriptorSet[resourceSetCount];
            uint* dynamicOffsets = stackalloc uint[pipeline.DynamicOffsetsCount];
            uint currentBatchCount = 0;
            uint currentBatchFirstSet = 0;
            uint currentBatchDynamicOffsetCount = 0;

            Span<BoundResourceSetInfo> sets = resourceSets.AsSpan(0, resourceSetCount);
            Span<bool> setsChanged = resourceSetsChanged.AsSpan(0, resourceSetCount);

            for (int currentSlot = 0; currentSlot < resourceSetCount; currentSlot++)
            {
                bool batchEnded = !setsChanged[currentSlot] || currentSlot == resourceSetCount - 1;

                if (setsChanged[currentSlot])
                {
                    setsChanged[currentSlot] = false;
                    ref BoundResourceSetInfo resourceSet = ref sets[currentSlot];
                    VkResourceSet vkSet = Util.AssertSubtype<ResourceSet, VkResourceSet>(resourceSet.Set);
                    descriptorSets[currentBatchCount] = vkSet.DescriptorSet;
                    currentBatchCount += 1;

                    ref SmallFixedOrDynamicArray curSetOffsets = ref resourceSet.Offsets;
                    for (uint i = 0; i < curSetOffsets.Count; i++)
                    {
                        dynamicOffsets[currentBatchDynamicOffsetCount] = curSetOffsets.Get(i);
                        currentBatchDynamicOffsetCount += 1;
                    }

                    // Increment ref count on first use of a set.
                    _currentStagingInfo.AddResource(vkSet.RefCount);
                    for (int i = 0; i < vkSet.RefCounts.Count; i++)
                    {
                        _currentStagingInfo.AddResource(vkSet.RefCounts[i]);
                    }
                }

                if (batchEnded)
                {
                    if (currentBatchCount != 0)
                    {
                        // Flush current batch.
                        vkCmdBindDescriptorSets(
                            _cb,
                            bindPoint,
                            pipelineLayout,
                            currentBatchFirstSet,
                            currentBatchCount,
                            descriptorSets,
                            currentBatchDynamicOffsetCount,
                            dynamicOffsets);
                    }

                    currentBatchCount = 0;
                    currentBatchFirstSet = (uint)(currentSlot + 1);
                }
            }
        }

        private void TransitionImages(List<VkTexture> sampledTextures, VkImageLayout layout)
        {
            for (int i = 0; i < sampledTextures.Count; i++)
            {
                VkTexture tex = sampledTextures[i];
                tex.TransitionImageLayout(_cb, 0, tex.MipLevels, 0, tex.ActualArrayLayers, layout);
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

            for (uint currentSlot = 0; currentSlot < _currentComputePipeline!.ResourceSetCount; currentSlot++)
            {
                VkResourceSet vkSet = Util.AssertSubtype<ResourceSet, VkResourceSet>(
                    _currentComputeResourceSets[currentSlot].Set);

                TransitionImages(vkSet.SampledTextures, VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
                TransitionImages(vkSet.StorageTextures, VkImageLayout.VK_IMAGE_LAYOUT_GENERAL);
                for (int texIdx = 0; texIdx < vkSet.StorageTextures.Count; texIdx++)
                {
                    VkTexture storageTex = vkSet.StorageTextures[texIdx];
                    if ((storageTex.Usage & TextureUsage.Sampled) != 0)
                    {
                        _preDrawSampledImages.Add(storageTex);
                    }
                }
            }

            FlushNewResourceSets(
                _currentComputeResourceSets,
                _computeResourceSetsChanged,
                (int)_currentComputePipeline.ResourceSetCount,
                VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_COMPUTE,
                _currentComputePipeline.PipelineLayout);
        }

        protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();

            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            _currentStagingInfo.AddResource(vkBuffer.RefCount);
            vkCmdDispatchIndirect(_cb, vkBuffer.DeviceBuffer, offset);
        }

        protected override void ResolveTextureCore(Texture source, Texture destination)
        {
            if (_activeRenderPass != VkRenderPass.NULL)
            {
                EndCurrentRenderPass();
            }

            VkTexture vkSource = Util.AssertSubtype<Texture, VkTexture>(source);
            _currentStagingInfo.AddResource(vkSource.RefCount);
            VkTexture vkDestination = Util.AssertSubtype<Texture, VkTexture>(destination);
            _currentStagingInfo.AddResource(vkDestination.RefCount);

            VkImageAspectFlags aspectFlags = ((source.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
                ? VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT | VkImageAspectFlags.VK_IMAGE_ASPECT_STENCIL_BIT
                : VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT;
            VkImageResolve region = new()
            {
                extent = new VkExtent3D() { width = source.Width, height = source.Height, depth = source.Depth },
                srcSubresource = new VkImageSubresourceLayers() { layerCount = 1, aspectMask = aspectFlags },
                dstSubresource = new VkImageSubresourceLayers() { layerCount = 1, aspectMask = aspectFlags }
            };

            vkSource.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);
            vkDestination.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
             
            vkCmdResolveImage(
                _cb,
                vkSource.OptimalDeviceImage,
                VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                vkDestination.OptimalDeviceImage,
                VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                1,
                &region);

            TransitionBackFromTransfer(_cb, vkSource, 0, 1, 0, 1);
            TransitionBackFromTransfer(_cb, vkDestination, 0, 1, 0, 1);
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
            if (_activeRenderPass != VkRenderPass.NULL)
            {
                EndCurrentRenderPass();
                _currentFramebuffer!.TransitionToFinalLayout(_cb, false);
            }

            VkResult result = vkEndCommandBuffer(_cb);
            CheckResult(result);
        }

        protected override void SetFramebufferCore(Framebuffer fb)
        {
            if (_activeRenderPass != VkRenderPass.NULL)
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
                _currentFramebuffer.TransitionToFinalLayout(_cb, false);
            }

            VkFramebufferBase vkFB = Util.AssertSubtype<Framebuffer, VkFramebufferBase>(fb);
            _currentFramebuffer = vkFB;
            _currentFramebufferEverActive = false;
            _newFramebuffer = true;

            _viewportCount = Math.Max(1u, (uint)vkFB.ColorTargets.Length);
            Util.EnsureArrayMinimumSize(ref _viewports, _viewportCount);
            Util.ClearArray(_viewports);
            Util.EnsureArrayMinimumSize(ref _scissorRects, _viewportCount);
            Util.ClearArray(_scissorRects);

            uint clearValueCount = (uint)vkFB.ColorTargets.Length;
            Util.EnsureArrayMinimumSize(ref _clearValues, clearValueCount + 1); // Leave an extra space for the depth value (tracked separately).
            Util.ClearArray(_validColorClearValues);
            Util.EnsureArrayMinimumSize(ref _validColorClearValues, clearValueCount);
            _currentStagingInfo.AddResource(vkFB.RefCount);

            if (fb is VkSwapchainFramebuffer scFB)
            {
                _currentStagingInfo.AddResource(scFB.Swapchain.RefCount);
            }
        }

        private void EnsureRenderPassActive()
        {
            if (_activeRenderPass == VkRenderPass.NULL)
            {
                BeginCurrentRenderPass();
            }
        }

        private void EnsureNoRenderPass()
        {
            if (_activeRenderPass != VkRenderPass.NULL)
            {
                EndCurrentRenderPass();
            }
        }

        private void BeginCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass == VkRenderPass.NULL);
            Debug.Assert(_currentFramebuffer != null);
            _currentFramebufferEverActive = true;

            uint attachmentCount = _currentFramebuffer.AttachmentCount;
            int colorTargetCount = _currentFramebuffer.ColorTargets.Length;
            bool haveAnyAttachments = colorTargetCount > 0 || _currentFramebuffer.DepthTarget != null;
            bool haveAllClearValues = _depthClearValue.HasValue || _currentFramebuffer.DepthTarget == null;
            bool haveAnyClearValues = _depthClearValue.HasValue;
            for (int i = 0; i < colorTargetCount; i++)
            {
                if (!_validColorClearValues[i])
                {
                    haveAllClearValues = false;
                }
                else
                {
                    haveAnyClearValues = true;
                }
            }

            VkRenderPassBeginInfo renderPassBI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO,
                renderArea = new VkRect2D()
                {
                    offset = new VkOffset2D(),
                    extent = new VkExtent2D()
                    {
                        width = _currentFramebuffer.RenderableWidth,
                        height = _currentFramebuffer.RenderableHeight
                    }
                },
                framebuffer = _currentFramebuffer.CurrentFramebuffer
            };

            if (!haveAnyAttachments || !haveAllClearValues)
            {
                renderPassBI.renderPass = _newFramebuffer
                    ? _currentFramebuffer.RenderPassNoClear_Init
                    : _currentFramebuffer.RenderPassNoClear_Load;
                vkCmdBeginRenderPass(_cb, &renderPassBI, VkSubpassContents.VK_SUBPASS_CONTENTS_INLINE);
                _activeRenderPass = renderPassBI.renderPass;

                if (haveAnyClearValues)
                {
                    if (_depthClearValue.HasValue)
                    {
                        VkClearDepthStencilValue depthStencil = _depthClearValue.GetValueOrDefault().depthStencil;
                        ClearDepthStencilCore(depthStencil.depth, (byte)depthStencil.stencil);
                        _depthClearValue = null;
                    }

                    for (uint i = 0; i < colorTargetCount; i++)
                    {
                        if (_validColorClearValues[i])
                        {
                            _validColorClearValues[i] = false;
                            VkClearValue vkClearValue = _clearValues[i];
                            RgbaFloat clearColor = new(
                                vkClearValue.color.float32[0],
                                vkClearValue.color.float32[1],
                                vkClearValue.color.float32[2],
                                vkClearValue.color.float32[3]);
                            ClearColorTargetCore(i, clearColor);
                        }
                    }
                }
            }
            else
            {
                _currentFramebuffer.TransitionToFinalLayout(_cb, true);

                // We have clear values for every attachment.
                renderPassBI.renderPass = _currentFramebuffer.RenderPassClear;
                fixed (VkClearValue* clearValuesPtr = _clearValues)
                {
                    renderPassBI.clearValueCount = attachmentCount;
                    renderPassBI.pClearValues = clearValuesPtr;
                    if (_depthClearValue.HasValue)
                    {
                        _clearValues[colorTargetCount] = _depthClearValue.GetValueOrDefault();
                        _depthClearValue = null;
                    }
                    vkCmdBeginRenderPass(_cb, &renderPassBI, VkSubpassContents.VK_SUBPASS_CONTENTS_INLINE);
                    _activeRenderPass = renderPassBI.renderPass;
                    Util.ClearArray(_validColorClearValues);
                }
            }

            _newFramebuffer = false;
        }

        private void EndCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass != VkRenderPass.NULL);
            vkCmdEndRenderPass(_cb);
            _currentFramebuffer!.TransitionToIntermediateLayout(_cb);
            _activeRenderPass = default;

            // Place a barrier between RenderPasses, so that color / depth outputs
            // can be read in subsequent passes.
            vkCmdPipelineBarrier(
                _cb,
                VkPipelineStageFlags.VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT,
                VkPipelineStageFlags.VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
                0,
                0,
                null,
                0,
                null,
                0,
                null);
        }

        private protected override void SetVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            bool differentBuffer = _vertexBindings[index] != vkBuffer.DeviceBuffer;
            if (differentBuffer || _vertexOffsets[index] != offset)
            {
                _vertexBindingsChanged = true;
                if (differentBuffer)
                {
                    _currentStagingInfo.AddResource(vkBuffer.RefCount);
                    _vertexBindings[index] = vkBuffer.DeviceBuffer;
                }

                _vertexOffsets[index] = offset;
                _numVertexBindings = Math.Max(index + 1, _numVertexBindings);
            }
        }

        private protected override void SetIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            _currentStagingInfo.AddResource(vkBuffer.RefCount);
            vkCmdBindIndexBuffer(_cb, vkBuffer.DeviceBuffer, offset, VkFormats.VdToVkIndexFormat(format));
        }

        private protected override void SetPipelineCore(Pipeline pipeline)
        {
            VkPipeline vkPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
            if (!pipeline.IsComputePipeline && _currentGraphicsPipeline != pipeline)
            {
                Util.EnsureArrayMinimumSize(ref _currentGraphicsResourceSets, vkPipeline.ResourceSetCount);
                ClearSets(_currentGraphicsResourceSets);
                Util.EnsureArrayMinimumSize(ref _graphicsResourceSetsChanged, vkPipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS, vkPipeline.DevicePipeline);
                _currentGraphicsPipeline = vkPipeline;

                uint vertexBufferCount = vkPipeline.VertexLayoutCount;
                Util.EnsureArrayMinimumSize(ref _vertexBindings, vertexBufferCount);
                Util.EnsureArrayMinimumSize(ref _vertexOffsets, vertexBufferCount);
            }
            else if (pipeline.IsComputePipeline && _currentComputePipeline != pipeline)
            {
                Util.EnsureArrayMinimumSize(ref _currentComputeResourceSets, vkPipeline.ResourceSetCount);
                ClearSets(_currentComputeResourceSets);
                Util.EnsureArrayMinimumSize(ref _computeResourceSetsChanged, vkPipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_COMPUTE, vkPipeline.DevicePipeline);
                _currentComputePipeline = vkPipeline;
            }

            _currentStagingInfo.AddResource(vkPipeline.RefCount);
        }

        private static void ClearSets(Span<BoundResourceSetInfo> boundSets)
        {
            foreach (ref BoundResourceSetInfo boundSetInfo in boundSets)
            {
                boundSetInfo.Offsets.Dispose();
                boundSetInfo = default;
            }
        }

        protected override void SetGraphicsResourceSetCore(uint slot, ResourceSet rs, ReadOnlySpan<uint> dynamicOffsets)
        {
            ref BoundResourceSetInfo set = ref _currentGraphicsResourceSets[slot];
            if (!set.Equals(rs, dynamicOffsets))
            {
                set.Offsets.Dispose();
                set = new BoundResourceSetInfo(rs, dynamicOffsets);
                _graphicsResourceSetsChanged[slot] = true;
                Util.AssertSubtype<ResourceSet, VkResourceSet>(rs);
            }
        }

        protected override void SetComputeResourceSetCore(uint slot, ResourceSet rs, ReadOnlySpan<uint> dynamicOffsets)
        {
            ref BoundResourceSetInfo set = ref _currentComputeResourceSets[slot];
            if (!set.Equals(rs, dynamicOffsets))
            {
                set.Offsets.Dispose();
                set = new BoundResourceSetInfo(rs, dynamicOffsets);
                _computeResourceSetsChanged[slot] = true;
                Util.AssertSubtype<ResourceSet, VkResourceSet>(rs);
            }
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            VkRect2D scissor = new()
            {
                offset = new VkOffset2D() { x = (int)x, y = (int)y },
                extent = new VkExtent2D() { width = width, height = height }
            };

            VkRect2D[] scissorRects = _scissorRects;
            if (scissorRects[index].offset.x != scissor.offset.x ||
                scissorRects[index].offset.y != scissor.offset.y ||
                scissorRects[index].extent.width != scissor.extent.width ||
                scissorRects[index].extent.height != scissor.extent.height)
            {
                _scissorRectsChanged = true;
                scissorRects[index] = scissor;
            }
        }

        public override void SetViewport(uint index, in Viewport viewport)
        {
            bool yInverted = _gd.IsClipSpaceYInverted;
            float vpY = yInverted
                ? viewport.Y
                : viewport.Height + viewport.Y;
            float vpHeight = yInverted
                ? viewport.Height
                : -viewport.Height;

            _viewportsChanged = true;
            _viewports[index] = new VkViewport()
            {
                x = viewport.X,
                y = vpY,
                width = viewport.Width,
                height = vpHeight,
                minDepth = viewport.MinDepth,
                maxDepth = viewport.MaxDepth
            };
        }

        private protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            VkBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);
            _gd.UpdateBuffer(stagingBuffer, 0, source, sizeInBytes);
            CopyBuffer(stagingBuffer, 0, buffer, bufferOffsetInBytes, sizeInBytes);
        }

        protected override void CopyBufferCore(
            DeviceBuffer source,
            DeviceBuffer destination,
            ReadOnlySpan<BufferCopyCommand> commands)
        {
            EnsureNoRenderPass();

            VkBuffer srcVkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(source);
            _currentStagingInfo.AddResource(srcVkBuffer.RefCount);
            VkBuffer dstVkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(destination);
            _currentStagingInfo.AddResource(dstVkBuffer.RefCount);

            fixed (BufferCopyCommand* commandPtr = commands)
            {
                int offset = 0;
                int prevOffset = 0;

                while (offset < commands.Length)
                {
                    if (commands[offset].Length != 0)
                    {
                        offset++;
                        continue;
                    }

                    int count = offset - prevOffset;
                    if (count > 0)
                    {
                        vkCmdCopyBuffer(
                            _cb,
                            srcVkBuffer.DeviceBuffer,
                            dstVkBuffer.DeviceBuffer,
                            (uint)count,
                            (VkBufferCopy*)(commandPtr + prevOffset));
                    }

                    while (offset < commands.Length)
                    {
                        if (commands[offset].Length != 0)
                        {
                            break;
                        }
                        offset++;
                    }
                    prevOffset = offset;
                }

                {
                    int count = offset - prevOffset;
                    if (count > 0)
                    {
                        vkCmdCopyBuffer(
                            _cb,
                            srcVkBuffer.DeviceBuffer,
                            dstVkBuffer.DeviceBuffer,
                            (uint)count,
                            (VkBufferCopy*)(commandPtr + prevOffset));
                    }
                }
            }

            VkMemoryBarrier barrier = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_BARRIER,
                srcAccessMask = VkAccessFlags.VK_ACCESS_TRANSFER_WRITE_BIT,
                dstAccessMask = VkAccessFlags.VK_ACCESS_VERTEX_ATTRIBUTE_READ_BIT
            };

            vkCmdPipelineBarrier(
                _cb,
                VkPipelineStageFlags.VK_PIPELINE_STAGE_TRANSFER_BIT,
                VkPipelineStageFlags.VK_PIPELINE_STAGE_VERTEX_INPUT_BIT,
                0,
                1, &barrier,
                0, null,
                0, null);
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

            VkTexture srcVkTexture = Util.AssertSubtype<Texture, VkTexture>(source);
            _currentStagingInfo.AddResource(srcVkTexture.RefCount);
            VkTexture dstVkTexture = Util.AssertSubtype<Texture, VkTexture>(destination);
            _currentStagingInfo.AddResource(dstVkTexture.RefCount);
        }

        internal static VkImageLayout GetTransitionBackLayout(TextureUsage usage)
        {
            if ((usage & TextureUsage.Sampled) != 0)
            {
                return VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
            }
            else if ((usage & TextureUsage.RenderTarget) != 0)
            {
                return (usage & TextureUsage.DepthStencil) != 0
                    ? VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL
                    : VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
            }
            else
            {
                return VkImageLayout.VK_IMAGE_LAYOUT_GENERAL;
            }
        }

        internal static void TransitionBackFromTransfer(
            VkCommandBuffer cb,
            VkTexture texture,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount)
        {
            VkImageLayout layout = GetTransitionBackLayout(texture.Usage);

            texture.TransitionImageLayout(
                cb,
                baseMipLevel,
                levelCount,
                baseArrayLayer,
                layerCount,
                layout);
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
                VkImageSubresourceLayers srcSubresource = new()
                {
                    aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                    layerCount = layerCount,
                    mipLevel = srcMipLevel,
                    baseArrayLayer = srcBaseArrayLayer
                };

                VkImageSubresourceLayers dstSubresource = new()
                {
                    aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                    layerCount = layerCount,
                    mipLevel = dstMipLevel,
                    baseArrayLayer = dstBaseArrayLayer
                };

                VkImageCopy region = new()
                {
                    srcOffset = new VkOffset3D() { x = (int)srcX, y = (int)srcY, z = (int)srcZ },
                    dstOffset = new VkOffset3D() { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    srcSubresource = srcSubresource,
                    dstSubresource = dstSubresource,
                    extent = new VkExtent3D() { width = width, height = height, depth = depth }
                };

                srcVkTexture.TransitionImageLayout(
                    cb,
                    srcMipLevel,
                    1,
                    srcBaseArrayLayer,
                    layerCount,
                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);

                dstVkTexture.TransitionImageLayout(
                    cb,
                    dstMipLevel,
                    1,
                    dstBaseArrayLayer,
                    layerCount,
                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

                vkCmdCopyImage(
                    cb,
                    srcVkTexture.OptimalDeviceImage,
                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                    dstVkTexture.OptimalDeviceImage,
                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    1,
                    &region);

                TransitionBackFromTransfer(cb, srcVkTexture, srcMipLevel, 1, srcBaseArrayLayer, layerCount);

                TransitionBackFromTransfer(cb, dstVkTexture, dstMipLevel, 1, dstBaseArrayLayer, layerCount);
            }
            else if (sourceIsStaging && !destIsStaging)
            {
                VulkanBuffer srcBuffer = srcVkTexture.StagingBuffer;
                VkSubresourceLayout srcLayout = srcVkTexture.GetSubresourceLayout(srcMipLevel, srcBaseArrayLayer);
                VkImage dstImage = dstVkTexture.OptimalDeviceImage;
                dstVkTexture.TransitionImageLayout(
                    cb,
                    dstMipLevel,
                    1,
                    dstBaseArrayLayer,
                    layerCount,
                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

                VkImageSubresourceLayers dstSubresource = new()
                {
                    aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                    layerCount = layerCount,
                    mipLevel = dstMipLevel,
                    baseArrayLayer = dstBaseArrayLayer
                };

                Util.GetMipDimensions(srcVkTexture, srcMipLevel, out uint mipWidth, out uint mipHeight, out _);
                uint blockSize = FormatHelpers.IsCompressedFormat(srcVkTexture.Format) ? 4u : 1u;
                uint bufferRowLength = Math.Max(mipWidth, blockSize);
                uint bufferImageHeight = Math.Max(mipHeight, blockSize);
                uint compressedX = srcX / blockSize;
                uint compressedY = srcY / blockSize;
                uint blockSizeInBytes = blockSize == 1
                    ? FormatSizeHelpers.GetSizeInBytes(srcVkTexture.Format)
                    : FormatHelpers.GetBlockSizeInBytes(srcVkTexture.Format);
                uint rowPitch = FormatHelpers.GetRowPitch(bufferRowLength, srcVkTexture.Format);
                uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, bufferImageHeight, srcVkTexture.Format);

                uint copyWidth = Math.Min(width, mipWidth);
                uint copyheight = Math.Min(height, mipHeight);

                VkBufferImageCopy regions = new()
                {
                    bufferOffset = srcLayout.offset
                        + (srcZ * depthPitch)
                        + (compressedY * rowPitch)
                        + (compressedX * blockSizeInBytes),
                    bufferRowLength = bufferRowLength,
                    bufferImageHeight = bufferImageHeight,
                    imageExtent = new VkExtent3D() { width = copyWidth, height = copyheight, depth = depth },
                    imageOffset = new VkOffset3D() { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    imageSubresource = dstSubresource
                };

                vkCmdCopyBufferToImage(cb, srcBuffer, dstImage, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &regions);

                TransitionBackFromTransfer(cb, dstVkTexture, dstMipLevel, 1, dstBaseArrayLayer, layerCount);
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
                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);

                VulkanBuffer dstBuffer = dstVkTexture.StagingBuffer;

                VkImageAspectFlags aspect = (srcVkTexture.Usage & TextureUsage.DepthStencil) != 0
                    ? VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT
                    : VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT;

                Util.GetMipDimensions(dstVkTexture, dstMipLevel, out uint mipWidth, out uint mipHeight);
                uint blockSize = FormatHelpers.IsCompressedFormat(srcVkTexture.Format) ? 4u : 1u;
                uint bufferRowLength = Math.Max(mipWidth, blockSize);
                uint bufferImageHeight = Math.Max(mipHeight, blockSize);
                uint compressedDstX = dstX / blockSize;
                uint compressedDstY = dstY / blockSize;
                uint blockSizeInBytes = blockSize == 1
                    ? FormatSizeHelpers.GetSizeInBytes(dstVkTexture.Format)
                    : FormatHelpers.GetBlockSizeInBytes(dstVkTexture.Format);
                uint rowPitch = FormatHelpers.GetRowPitch(bufferRowLength, dstVkTexture.Format);
                uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, bufferImageHeight, dstVkTexture.Format);

                VkBufferImageCopy* layers = stackalloc VkBufferImageCopy[(int)layerCount];
                for (uint layer = 0; layer < layerCount; layer++)
                {
                    VkSubresourceLayout dstLayout = dstVkTexture.GetSubresourceLayout(dstMipLevel, dstBaseArrayLayer + layer);

                    VkImageSubresourceLayers srcSubresource = new()
                    {
                        aspectMask = aspect,
                        layerCount = 1,
                        mipLevel = srcMipLevel,
                        baseArrayLayer = srcBaseArrayLayer + layer
                    };

                    VkBufferImageCopy region = new()
                    {
                        bufferRowLength = bufferRowLength,
                        bufferImageHeight = bufferImageHeight,
                        bufferOffset = dstLayout.offset
                            + (dstZ * depthPitch)
                            + (compressedDstY * rowPitch)
                            + (compressedDstX * blockSizeInBytes),
                        imageExtent = new VkExtent3D { width = width, height = height, depth = depth },
                        imageOffset = new VkOffset3D { x = (int)srcX, y = (int)srcY, z = (int)srcZ },
                        imageSubresource = srcSubresource
                    };

                    layers[layer] = region;
                }

                vkCmdCopyImageToBuffer(cb, srcImage, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, dstBuffer, layerCount, layers);

                TransitionBackFromTransfer(cb, srcVkTexture, srcMipLevel, 1, srcBaseArrayLayer, layerCount);
            }
            else
            {
                Debug.Assert(sourceIsStaging && destIsStaging);
                VulkanBuffer srcBuffer = srcVkTexture.StagingBuffer;
                VkSubresourceLayout srcLayout = srcVkTexture.GetSubresourceLayout(srcMipLevel, srcBaseArrayLayer);
                VulkanBuffer dstBuffer = dstVkTexture.StagingBuffer;
                VkSubresourceLayout dstLayout = dstVkTexture.GetSubresourceLayout(dstMipLevel, dstBaseArrayLayer);

                uint zLimit = Math.Max(depth, layerCount);
                if (!FormatHelpers.IsCompressedFormat(source.Format))
                {
                    // TODO: batch BufferCopy

                    uint pixelSize = FormatSizeHelpers.GetSizeInBytes(srcVkTexture.Format);
                    for (uint zz = 0; zz < zLimit; zz++)
                    {
                        for (uint yy = 0; yy < height; yy++)
                        {
                            VkBufferCopy region = new()
                            {
                                srcOffset = srcLayout.offset
                                    + srcLayout.depthPitch * (zz + srcZ)
                                    + srcLayout.rowPitch * (yy + srcY)
                                    + pixelSize * srcX,
                                dstOffset = dstLayout.offset
                                    + dstLayout.depthPitch * (zz + dstZ)
                                    + dstLayout.rowPitch * (yy + dstY)
                                    + pixelSize * dstX,
                                size = width * pixelSize
                            };
                            vkCmdCopyBuffer(cb, srcBuffer, dstBuffer, 1, &region);
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

                    // TODO: batch BufferCopy

                    for (uint zz = 0; zz < zLimit; zz++)
                    {
                        for (uint row = 0; row < numRows; row++)
                        {
                            VkBufferCopy region = new()
                            {
                                srcOffset = srcLayout.offset
                                    + srcLayout.depthPitch * (zz + srcZ)
                                    + srcLayout.rowPitch * (row + compressedSrcY)
                                    + blockSizeInBytes * compressedSrcX,
                                dstOffset = dstLayout.offset
                                    + dstLayout.depthPitch * (zz + dstZ)
                                    + dstLayout.rowPitch * (row + compressedDstY)
                                    + blockSizeInBytes * compressedDstX,
                                size = denseRowSize
                            };
                            vkCmdCopyBuffer(cb, srcBuffer, dstBuffer, 1, &region);
                        }
                    }
                }
            }
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            EnsureNoRenderPass();
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            _currentStagingInfo.AddResource(vkTex.RefCount);

            uint layerCount = vkTex.ActualArrayLayers;

            VkImageBlit region;

            uint width = vkTex.Width;
            uint height = vkTex.Height;
            uint depth = vkTex.Depth;
            for (uint level = 1; level < vkTex.MipLevels; level++)
            {
                vkTex.TransitionImageLayoutNonmatching(_cb, level - 1, 1, 0, layerCount, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);
                vkTex.TransitionImageLayoutNonmatching(_cb, level, 1, 0, layerCount, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

                VkImage deviceImage = vkTex.OptimalDeviceImage;
                uint mipWidth = Math.Max(width >> 1, 1);
                uint mipHeight = Math.Max(height >> 1, 1);
                uint mipDepth = Math.Max(depth >> 1, 1);

                region.srcSubresource = new VkImageSubresourceLayers()
                {
                    aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                    baseArrayLayer = 0,
                    layerCount = layerCount,
                    mipLevel = level - 1
                };
                region.srcOffsets.e0 = new VkOffset3D();
                region.srcOffsets.e1 = new VkOffset3D() { x = (int)width, y = (int)height, z = (int)depth };
                region.dstOffsets.e0 = new VkOffset3D();

                region.dstSubresource = new VkImageSubresourceLayers()
                {
                    aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                    baseArrayLayer = 0,
                    layerCount = layerCount,
                    mipLevel = level
                };

                region.dstOffsets.e1 = new VkOffset3D() { x = (int)mipWidth, y = (int)mipHeight, z = (int)mipDepth };
                vkCmdBlitImage(
                    _cb,
                    deviceImage, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                    deviceImage, VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    1, &region,
                    _gd.GetFormatFilter(vkTex.VkFormat));

                width = mipWidth;
                height = mipHeight;
                depth = mipDepth;
            }

            VkImageLayout layout = GetTransitionBackLayout(vkTex.Usage);
            vkTex.TransitionImageLayoutNonmatching(_cb, 0, vkTex.MipLevels, 0, layerCount, layout);
        }

        [Conditional("DEBUG")]
        private void DebugFullPipelineBarrier()
        {
            VkMemoryBarrier memoryBarrier = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_BARRIER,
                srcAccessMask =
                    VkAccessFlags.VK_ACCESS_INDIRECT_COMMAND_READ_BIT |
                    VkAccessFlags.VK_ACCESS_INDEX_READ_BIT |
                    VkAccessFlags.VK_ACCESS_VERTEX_ATTRIBUTE_READ_BIT |
                    VkAccessFlags.VK_ACCESS_UNIFORM_READ_BIT |
                    VkAccessFlags.VK_ACCESS_INPUT_ATTACHMENT_READ_BIT |
                    VkAccessFlags.VK_ACCESS_SHADER_READ_BIT |
                    VkAccessFlags.VK_ACCESS_SHADER_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_READ_BIT |
                    VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT |
                    VkAccessFlags.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_TRANSFER_READ_BIT |
                    VkAccessFlags.VK_ACCESS_TRANSFER_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_HOST_READ_BIT |
                    VkAccessFlags.VK_ACCESS_HOST_WRITE_BIT,
                dstAccessMask =
                    VkAccessFlags.VK_ACCESS_INDIRECT_COMMAND_READ_BIT |
                    VkAccessFlags.VK_ACCESS_INDEX_READ_BIT |
                    VkAccessFlags.VK_ACCESS_VERTEX_ATTRIBUTE_READ_BIT |
                    VkAccessFlags.VK_ACCESS_UNIFORM_READ_BIT |
                    VkAccessFlags.VK_ACCESS_INPUT_ATTACHMENT_READ_BIT |
                    VkAccessFlags.VK_ACCESS_SHADER_READ_BIT |
                    VkAccessFlags.VK_ACCESS_SHADER_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_READ_BIT |
                    VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT |
                    VkAccessFlags.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_TRANSFER_READ_BIT |
                    VkAccessFlags.VK_ACCESS_TRANSFER_WRITE_BIT |
                    VkAccessFlags.VK_ACCESS_HOST_READ_BIT |
                    VkAccessFlags.VK_ACCESS_HOST_WRITE_BIT
            };

            vkCmdPipelineBarrier(
                _cb,
                VkPipelineStageFlags.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, // srcStageMask
                VkPipelineStageFlags.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, // dstStageMask
                0,
                1,                                  // memoryBarrierCount
                &memoryBarrier,                     // pMemoryBarriers
                0, null,
                0, null);
        }

        public override string? Name
        {
            get => _name;
            set
            {
                _name = value;

                if (_gd.DebugMarkerEnabled)
                {
                    SetDebugMarkerName(_name);
                }
            }
        }

        [SkipLocalsInit]
        private void SetDebugMarkerName(string? name)
        {
            void SetName(VkCommandBuffer cb, ReadOnlySpan<byte> nameUtf8)
            {
                _gd.SetDebugMarkerName(
                    VkDebugReportObjectTypeEXT.VK_DEBUG_REPORT_OBJECT_TYPE_COMMAND_BUFFER_EXT,
                    (ulong)cb.Value,
                    nameUtf8);
            }

            Span<byte> utf8Buffer = stackalloc byte[1024];
            Util.GetNullTerminatedUtf8(name, ref utf8Buffer);

            lock (_commandBufferListLock)
            {
                foreach (VkCommandBuffer cb in _submittedCommandBuffers)
                {
                    SetName(cb, utf8Buffer);
                }
                foreach (VkCommandBuffer cb in _availableCommandBuffers)
                {
                    SetName(cb, utf8Buffer);
                }
            }

            VkCommandBuffer currentCb = _cb;
            if (currentCb != VkCommandBuffer.NULL)
            {
                SetName(currentCb, utf8Buffer);
            }

            _gd.SetDebugMarkerName(
                VkDebugReportObjectTypeEXT.VK_DEBUG_REPORT_OBJECT_TYPE_COMMAND_POOL_EXT,
                CommandPool.Value,
                utf8Buffer);
        }

        private VkBuffer GetStagingBuffer(uint size)
        {
            VkBuffer? ret = null;

            lock (_availableStagingBuffers)
            {
                foreach (VkBuffer buffer in _availableStagingBuffers)
                {
                    if (buffer.SizeInBytes >= size)
                    {
                        ret = buffer;
                        _availableStagingBuffers.Remove(buffer);
                        break;
                    }
                }
            }
            if (ret == null)
            {
                ret = (VkBuffer)_gd.ResourceFactory.CreateBuffer(
                    new BufferDescription(size, BufferUsage.StagingWrite));
                ret.Name = $"Staging Buffer (CommandList {_name})";
            }

            _currentStagingInfo.BuffersUsed.Add(ret);
            return ret;
        }

        [SkipLocalsInit]
        private protected override void PushDebugGroupCore(ReadOnlySpan<char> name)
        {
            Span<byte> byteBuffer = stackalloc byte[1024];

            vkCmdDebugMarkerBeginEXT_t? func = _gd.MarkerBegin;
            if (func == null)
            {
                return;
            }

            Util.GetNullTerminatedUtf8(name, ref byteBuffer);
            fixed (byte* utf8Ptr = byteBuffer)
            {
                VkDebugMarkerMarkerInfoEXT markerInfo = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_DEBUG_MARKER_MARKER_INFO_EXT,
                    pMarkerName = (sbyte*)utf8Ptr
                };
                func(_cb, &markerInfo);
            }
        }

        private protected override void PopDebugGroupCore()
        {
            vkCmdDebugMarkerEndEXT_t? func = _gd.MarkerEnd;
            if (func == null)
            {
                return;
            }

            func(_cb);
        }

        [SkipLocalsInit]
        private protected override void InsertDebugMarkerCore(ReadOnlySpan<char> name)
        {
            Span<byte> byteBuffer = stackalloc byte[1024];

            vkCmdDebugMarkerInsertEXT_t? func = _gd.MarkerInsert;
            if (func == null)
            {
                return;
            }

            Util.GetNullTerminatedUtf8(name, ref byteBuffer);
            fixed (byte* utf8Ptr = byteBuffer)
            {
                VkDebugMarkerMarkerInfoEXT markerInfo = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_DEBUG_MARKER_MARKER_INFO_EXT,
                    pMarkerName = (sbyte*)utf8Ptr
                };
                func(_cb, &markerInfo);
            }
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        void IResourceRefCountTarget.RefZeroed()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                vkDestroyCommandPool(_gd.Device, _pool, null);

                Debug.Assert(_submittedStagingInfos.Count == 0);

                if (_currentStagingInfo.IsValid)
                {
                    RecycleStagingInfo(_currentStagingInfo);
                }

                foreach (VkBuffer buffer in _availableStagingBuffers)
                {
                    buffer.Dispose();
                }
            }
        }

        private readonly struct StagingResourceInfo
        {
            public List<VkBuffer> BuffersUsed { get; }
            public HashSet<ResourceRefCount> Resources { get; }

            public bool IsValid => BuffersUsed != null;

            public StagingResourceInfo()
            {
                BuffersUsed = new List<VkBuffer>();
                Resources = new HashSet<ResourceRefCount>();
            }

            public void AddResource(ResourceRefCount count)
            {
                if (Resources.Add(count))
                {
                    count.Increment();
                }
            }

            public void Clear()
            {
                BuffersUsed.Clear();
                Resources.Clear();
            }
        }

        private StagingResourceInfo GetStagingResourceInfo()
        {
            if (!_availableStagingInfos.TryDequeue(out StagingResourceInfo ret))
            {
                ret = new StagingResourceInfo();
            }
            return ret;
        }

        private void RecycleStagingInfo(StagingResourceInfo info)
        {
            lock (_availableStagingBuffers)
            {
                foreach (VkBuffer buffer in info.BuffersUsed)
                {
                    _availableStagingBuffers.Add(buffer);
                }
            }

            foreach (ResourceRefCount rrc in info.Resources)
            {
                rrc.Decrement();
            }

            info.Clear();

            _availableStagingInfos.Enqueue(info);
        }
    }
}

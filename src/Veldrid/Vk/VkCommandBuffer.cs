using System;
using System.Diagnostics;
using System.Text;
using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;
using static Vulkan.RawConstants;
using System.Collections.Generic;

namespace Veldrid.Vk
{
    internal unsafe class VulkanCommandBuffer : CommandBuffer
    {
        private readonly VkGraphicsDevice _gd;
        private readonly CommandBufferFlags _flags;
        private readonly VkCommandPool _pool;
        private readonly List<ResourceRefCount> _refCounts = new List<ResourceRefCount>();
        private string _name;
        private RecordingState _state = RecordingState.Initial;
        private VkCommandBuffer _cb;

        private VkPipeline _currentGraphicsPipeline;
        private VkFramebuffer _currentFB;
        private VkPipeline _currentComputePipeline;
        private bool _disposed;

        public ResourceRefCount RefCount { get; }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        public VulkanCommandBuffer(VkGraphicsDevice gd, ref CommandBufferDescription description)
            : base(gd.Features)
        {
            _gd = gd;
            _flags = description.Flags;

            VkCommandPoolCreateInfo poolCI = VkCommandPoolCreateInfo.New();
            poolCI.flags = VkCommandPoolCreateFlags.ResetCommandBuffer;
            poolCI.queueFamilyIndex = _gd.UniversalQueueIndex;
            VkResult result = vkCreateCommandPool(_gd.Device, ref poolCI, null, out _pool);
            CheckResult(result);

            RefCount = new ResourceRefCount(DisposeCore);
        }

        public void OnSubmitted()
        {
            RefCount.Increment();
        }

        public void OnCompleted()
        {
            RefCount.Decrement();
            if ((_flags & CommandBufferFlags.Reusable) == 0)
            {
                // ClearRefCounts();
            }
        }

        private void IncrementRef(ResourceRefCount refCount)
        {
            refCount.Increment();
            _refCounts.Add(refCount);
        }

        private void ClearRefCounts()
        {
            foreach (ResourceRefCount rc in _refCounts)
            {
                rc.Decrement();
            }
            _refCounts.Clear();
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            if (!_disposed)
            {
                _disposed = true;
                vkDestroyCommandPool(_gd.Device, _pool, null);

                ClearRefCounts();
            }
        }

        private void BeginIfNeeded()
        {
            if (_state == RecordingState.Recording) { return; }
            if (_state == RecordingState.Initial)
            {
                Debug.Assert(_cb.Handle == IntPtr.Zero);
                VkCommandBufferAllocateInfo cbAI = VkCommandBufferAllocateInfo.New();
                cbAI.commandPool = _pool;
                cbAI.commandBufferCount = 1;
                cbAI.level = VkCommandBufferLevel.Primary;
                VkResult allocateResult = vkAllocateCommandBuffers(_gd.Device, ref cbAI, out _cb);
                CheckResult(allocateResult);
            }

            VkCommandBufferBeginInfo bi = VkCommandBufferBeginInfo.New();
            if ((_flags & CommandBufferFlags.Reusable) != 0)
            {
                bi.flags = VkCommandBufferUsageFlags.SimultaneousUse;
            }
            else
            {
                bi.flags = VkCommandBufferUsageFlags.OneTimeSubmit;
            }
            VkResult beginResult = vkBeginCommandBuffer(_cb, ref bi);
            CheckResult(beginResult);

            ClearRefCounts();

            _state = RecordingState.Recording;
        }

        internal override void BeginRenderPassCore(in RenderPassDescription rpd)
        {
            BeginIfNeeded();

            if (rpd.Framebuffer is VkSwapchainFramebuffer)
            {
                throw new VeldridException(
                    "BeginRenderPass cannot be called on a Swapchain's Framebuffer directly.");
            }
            _currentFB = Util.AssertSubtype<Framebuffer, VkFramebuffer>(rpd.Framebuffer);
            IncrementRef(_currentFB.RefCount);
            foreach (FramebufferAttachment colorTarget in _currentFB.ColorTargets)
            {
                IncrementRef(Util.AssertSubtype<Texture, VkTexture>(colorTarget.Target).RefCount);
            }
            if (_currentFB.DepthTarget != null)
            {
                IncrementRef(Util.AssertSubtype<Texture, VkTexture>(_currentFB.DepthTarget.Value.Target).RefCount);
            }

            VkRenderPassBeginInfo rpBI = VkRenderPassBeginInfo.New();
            rpBI.renderPass = _currentFB.GetRenderPass(rpd);
            rpBI.framebuffer = _currentFB.CurrentFramebuffer;
            rpBI.renderArea = new VkRect2D(0, 0, rpd.Framebuffer.Width, rpd.Framebuffer.Height);

            rpBI.clearValueCount += (uint)rpd.Framebuffer.ColorTargets.Count;
            if (rpd.Framebuffer.DepthTarget != null) { rpBI.clearValueCount += 1; }
            VkClearValue* clears = stackalloc VkClearValue[(int)rpBI.clearValueCount];
            rpBI.pClearValues = clears;

            for (uint i = 0; i < rpd.Framebuffer.ColorTargets.Count; i++)
            {
                rpd.GetColorAttachment(i, out LoadAction loadAction, out _, out RgbaFloat clearRgba);
                if (loadAction == LoadAction.Clear)
                {
                    VkClearValue clearColor = new VkClearValue
                    {
                        color = new VkClearColorValue(clearRgba.R, clearRgba.G, clearRgba.B, clearRgba.A)
                    };
                    clears[i] = clearColor;
                }
            }
            if (rpd.Framebuffer.DepthTarget != null)
            {
                clears[rpd.Framebuffer.ColorTargets.Count] = new VkClearValue
                {
                    depthStencil = new VkClearDepthStencilValue(rpd.ClearDepth, rpd.ClearStencil)
                };
            }

            vkCmdBeginRenderPass(_cb, &rpBI, VkSubpassContents.Inline);

            SetViewportCore(0, new Viewport(0, 0, _currentFB.Width, _currentFB.Height, 0f, 1f));

            // TODO: Multiple scissors.
            vkCmdSetScissor(_cb, 0, 1, &rpBI.renderArea);
        }

        private protected override void EndRenderPassCore()
        {
            vkCmdEndRenderPass(_cb);
        }

        private protected override void MemoryBarrierCore(
            ShaderStages sourceStage,
            ShaderStages destinationStage)
        {
            throw new NotImplementedException();
        }


        private protected override void MemoryBarrierCore(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage)
        {
            BeginIfNeeded();
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            IncrementRef(vkTex.RefCount);
            VkImageMemoryBarrier barrier = VkImageMemoryBarrier.New();
            barrier.srcAccessMask = VkFormats.GetConservativeSrcAccessFlags(texture, sourceStage);
            barrier.dstAccessMask = VkFormats.GetConservativeDstAccessFlags(texture, sourceStage);
            barrier.subresourceRange.baseArrayLayer = baseArrayLayer;
            barrier.subresourceRange.layerCount = layerCount;
            barrier.subresourceRange.baseMipLevel = baseMipLevel;
            barrier.subresourceRange.levelCount = levelCount;
            barrier.subresourceRange.aspectMask = FormatHelpers.IsDepthStencilFormat(texture.Format)
                ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                : VkImageAspectFlags.Color;
            barrier.image = vkTex.OptimalDeviceImage;

            barrier.oldLayout = GetFinalLayout(vkTex);
            barrier.newLayout = GetFinalLayout(vkTex);

            vkCmdPipelineBarrier(
                _cb,
                VkFormats.GetConservativeSrcStageFlags(texture, sourceStage),
                VkFormats.GetConservativeDstStageFlags(texture, destinationStage),
                VkDependencyFlags.None,
                0, null,
                0, null,
                1, &barrier);
        }

        [Conditional("DEBUG")]
        private void DebugFullPipelineBarrier()
        {
            VkMemoryBarrier memoryBarrier = VkMemoryBarrier.New();
            memoryBarrier.srcAccessMask = VK_ACCESS_INDIRECT_COMMAND_READ_BIT |
                   VK_ACCESS_INDEX_READ_BIT |
                   VK_ACCESS_VERTEX_ATTRIBUTE_READ_BIT |
                   VK_ACCESS_UNIFORM_READ_BIT |
                   VK_ACCESS_INPUT_ATTACHMENT_READ_BIT |
                   VK_ACCESS_SHADER_READ_BIT |
                   VK_ACCESS_SHADER_WRITE_BIT |
                   VK_ACCESS_COLOR_ATTACHMENT_READ_BIT |
                   VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT |
                   VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT |
                   VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT |
                   VK_ACCESS_TRANSFER_READ_BIT |
                   VK_ACCESS_TRANSFER_WRITE_BIT |
                   VK_ACCESS_HOST_READ_BIT |
                   VK_ACCESS_HOST_WRITE_BIT;
            memoryBarrier.dstAccessMask = VK_ACCESS_INDIRECT_COMMAND_READ_BIT |
                   VK_ACCESS_INDEX_READ_BIT |
                   VK_ACCESS_VERTEX_ATTRIBUTE_READ_BIT |
                   VK_ACCESS_UNIFORM_READ_BIT |
                   VK_ACCESS_INPUT_ATTACHMENT_READ_BIT |
                   VK_ACCESS_SHADER_READ_BIT |
                   VK_ACCESS_SHADER_WRITE_BIT |
                   VK_ACCESS_COLOR_ATTACHMENT_READ_BIT |
                   VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT |
                   VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT |
                   VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT |
                   VK_ACCESS_TRANSFER_READ_BIT |
                   VK_ACCESS_TRANSFER_WRITE_BIT |
                   VK_ACCESS_HOST_READ_BIT |
                   VK_ACCESS_HOST_WRITE_BIT;

            vkCmdPipelineBarrier(
                _cb,
                VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, // srcStageMask
                VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, // dstStageMask
                VkDependencyFlags.None,
                1,                                  // memoryBarrierCount
                &memoryBarrier,                     // pMemoryBarriers
                0, null,
                0, null);
        }

        private protected override void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            IncrementRef(vkBuffer.RefCount);
            vkCmdBindIndexBuffer(_cb, vkBuffer.DeviceBuffer, offset, VkFormats.VdToVkIndexFormat(format));
        }

        private protected override void BindPipelineCore(Pipeline pipeline)
        {
            VkPipeline vkPipeline;
            if (pipeline.IsComputePipeline)
            {
                BeginIfNeeded();
                vkPipeline = _currentComputePipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
            }
            else
            {
                vkPipeline = _currentGraphicsPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
            }
            IncrementRef(vkPipeline.RefCount);

            vkCmdBindPipeline(
                _cb,
                pipeline.IsComputePipeline ? VkPipelineBindPoint.Compute : VkPipelineBindPoint.Graphics,
                vkPipeline.DevicePipeline);
        }

        private protected override void BindGraphicsResourceSetCore(
            uint slot,
            ResourceSet resourceSet,
            Span<uint> dynamicOffsets)
        {
            VkResourceSet vkSet = Util.AssertSubtype<ResourceSet, VkResourceSet>(resourceSet);
            IncrementRef(vkSet.RefCount);
            foreach (ResourceRefCount refCount in vkSet.RefCounts)
            {
                IncrementRef(refCount);
            }

            VkDescriptorSet descriptorSet = vkSet.DescriptorSet;
            fixed (uint* dynamicOffsetsPtr = dynamicOffsets)
            {
                vkCmdBindDescriptorSets(
                    _cb,
                    VkPipelineBindPoint.Graphics,
                    _currentGraphicsPipeline.PipelineLayout,
                    slot, 1,
                    &descriptorSet,
                    (uint)dynamicOffsets.Length, dynamicOffsetsPtr);
            }
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            IncrementRef(vkBuffer.RefCount);
            Vulkan.VkBuffer deviceBuffer = vkBuffer.DeviceBuffer;
            ulong offset64 = offset;
            vkCmdBindVertexBuffers(_cb, index, 1, &deviceBuffer, &offset64);
        }

        private protected override void DrawCore(
            uint vertexCount,
            uint instanceCount,
            uint vertexStart,
            uint instanceStart)
        {
            vkCmdDraw(_cb, vertexCount, instanceCount, vertexStart, instanceStart);
        }

        private protected override void DrawIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            IncrementRef(vkBuffer.RefCount);
            vkCmdDrawIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        private protected override void DrawIndexedIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            IncrementRef(vkBuffer.RefCount);
            vkCmdDrawIndexedIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        private protected override void DrawIndexedCore(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart)
        {
            vkCmdDrawIndexed(_cb, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        public override void InsertDebugMarker(string name)
        {
            vkCmdDebugMarkerInsertEXT_t func = _gd.MarkerInsert;
            if (func == null) { return; }

            VkDebugMarkerMarkerInfoEXT markerInfo = VkDebugMarkerMarkerInfoEXT.New();

            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* utf8Ptr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
            }
            utf8Ptr[byteCount] = 0;

            markerInfo.pMarkerName = utf8Ptr;

            func(_cb, &markerInfo);
        }

        public override void PushDebugGroup(string name)
        {
            BeginIfNeeded();
            vkCmdDebugMarkerBeginEXT_t func = _gd.MarkerBegin;
            if (func == null) { return; }

            VkDebugMarkerMarkerInfoEXT markerInfo = VkDebugMarkerMarkerInfoEXT.New();

            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* utf8Ptr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
            }
            utf8Ptr[byteCount] = 0;

            markerInfo.pMarkerName = utf8Ptr;

            func(_cb, &markerInfo);
        }

        public override void PopDebugGroup()
        {
            vkCmdDebugMarkerEndEXT_t func = _gd.MarkerEnd;
            if (func == null) { return; }

            func(_cb);
        }

        private protected override void SetScissorRectCore(uint index, uint x, uint y, uint width, uint height)
        {
            VkRect2D scissor = new VkRect2D((int)x, (int)y, width, height);
            vkCmdSetScissor(_cb, index, 1, &scissor);
        }

        private protected override void SetViewportCore(uint index, Viewport viewport)
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

            vkCmdSetViewport(_cb, 0, 1, &vkViewport);
            // TODO: Multiple viewports.
        }

        internal VkCommandBuffer GetSubmissionCB()
        {
            EndIfNeeded();
            return _cb;
        }

        private void EndIfNeeded()
        {
            if (_state != RecordingState.Ended
                || (_flags & CommandBufferFlags.Reusable) == 0)
            {
                BeginIfNeeded();
                vkEndCommandBuffer(_cb);
                _state = RecordingState.Ended;
            }
        }

        private protected override void BindComputeResourceSetCore(
            uint slot,
            ResourceSet resourceSet,
            Span<uint> dynamicOffsets)
        {
            BeginIfNeeded();
            VkResourceSet vkSet = Util.AssertSubtype<ResourceSet, VkResourceSet>(resourceSet);
            IncrementRef(vkSet.RefCount);
            foreach (ResourceRefCount refCount in vkSet.RefCounts)
            {
                IncrementRef(refCount);
            }

            VkDescriptorSet descriptorSet = vkSet.DescriptorSet;
            fixed (uint* dynamicOffsetsPtr = &dynamicOffsets[0])
            {
                vkCmdBindDescriptorSets(
                    _cb,
                    VkPipelineBindPoint.Compute,
                    _currentComputePipeline.PipelineLayout,
                    slot, 1,
                    &descriptorSet,
                    (uint)dynamicOffsets.Length, dynamicOffsetsPtr);
            }
        }

        private protected override void DispatchCore(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            vkCmdDispatch(_cb, groupCountX, groupCountY, groupCountZ);
        }

        private protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            IncrementRef(vkBuffer.RefCount);
            vkCmdDispatchIndirect(_cb, vkBuffer.DeviceBuffer, offset);
        }

        private protected override void CopyBufferCore(
            DeviceBuffer source, uint sourceOffset,
            DeviceBuffer destination, uint destinationOffset,
            uint sizeInBytes)
        {
            BeginIfNeeded();
            VkBuffer vkSrc = (VkBuffer)source;
            IncrementRef(vkSrc.RefCount);
            VkBuffer vkDst = (VkBuffer)destination;
            IncrementRef(vkDst.RefCount);
            VkBufferCopy region = new VkBufferCopy();
            region.srcOffset = sourceOffset;
            region.dstOffset = destinationOffset;
            region.size = sizeInBytes;
            vkCmdCopyBuffer(_cb, vkSrc.DeviceBuffer, vkDst.DeviceBuffer, 1, &region);
        }

        private protected override void CopyTextureCore(
            Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer,
            Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            BeginIfNeeded();

            VkTexture vkSrc = Util.AssertSubtype<Texture, VkTexture>(source);
            IncrementRef(vkSrc.RefCount);
            VkTexture vkDst = Util.AssertSubtype<Texture, VkTexture>(destination);
            IncrementRef(vkDst.RefCount);

            VkCommandList.CopyTextureCore_VkCommandBuffer(
                _cb,
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth, layerCount);
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            BeginIfNeeded();
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            IncrementRef(vkTex.RefCount);
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

        private protected override void UpdateBufferCore(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes)
        {
            BeginIfNeeded();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            IncrementRef(vkBuffer.RefCount);
            vkCmdUpdateBuffer(_cb, vkBuffer.DeviceBuffer, bufferOffsetInBytes, sizeInBytes, source.ToPointer());
        }

        private protected override void BlitTextureCore(
            Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight,
            Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight,
            bool linearFilter)
        {
            BeginIfNeeded();
            VkTexture vkSrc = Util.AssertSubtype<Texture, VkTexture>(source);
            IncrementRef(vkSrc.RefCount);
            VkTexture vkDst = Util.AssertSubtype<Texture, VkTexture>(destination.ColorTargets[0].Target);
            IncrementRef(vkDst.RefCount);

            vkSrc.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.TransferSrcOptimal);
            vkDst.TransitionImageLayout(_cb, 0, 1, destination.ColorTargets[0].ArrayLayer, 1, VkImageLayout.TransferDstOptimal);

            VkImageBlit region = new VkImageBlit();
            region.srcSubresource.aspectMask = FormatHelpers.IsDepthStencilFormat(vkSrc.Format) ? VkImageAspectFlags.Depth : VkImageAspectFlags.Color;
            region.srcSubresource.mipLevel = 0;
            region.srcSubresource.baseArrayLayer = 0;
            region.srcSubresource.layerCount = 1;
            region.srcOffsets_0.x = (int)srcX;
            region.srcOffsets_0.y = (int)srcY;
            region.srcOffsets_1.x = (int)(srcX + srcWidth);
            region.srcOffsets_1.y = (int)(srcY + srcHeight);
            region.srcOffsets_1.z = 1;

            region.dstSubresource.aspectMask = FormatHelpers.IsDepthStencilFormat(vkDst.Format) ? VkImageAspectFlags.Depth : VkImageAspectFlags.Color;
            region.dstSubresource.mipLevel = 0;
            region.dstSubresource.baseArrayLayer = destination.ColorTargets[0].ArrayLayer;
            region.dstSubresource.layerCount = 1;
            region.dstOffsets_0.x = (int)dstX;
            region.dstOffsets_0.y = (int)dstY;
            region.dstOffsets_1.x = (int)(dstX + dstWidth);
            region.dstOffsets_1.y = (int)(dstY + dstHeight);
            region.dstOffsets_1.z = 1;

            vkCmdBlitImage(
                _cb,
                vkSrc.OptimalDeviceImage, VkImageLayout.TransferSrcOptimal,
                vkDst.OptimalDeviceImage, VkImageLayout.TransferDstOptimal,
                1, ref region,
                linearFilter ? VkFilter.Linear : VkFilter.Nearest);

            vkSrc.TransitionImageLayout(_cb, 0, 1, 0, 1, GetFinalLayout(vkSrc));
            vkDst.TransitionImageLayout(_cb, 0, 1, destination.ColorTargets[0].ArrayLayer, 1, GetFinalLayout(vkDst));
        }

        private enum RecordingState
        {
            Initial,
            Recording,
            Ended,
        }
    }
}

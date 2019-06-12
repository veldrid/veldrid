using System;
using System.Diagnostics;
using System.Text;
using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;
using static Vulkan.RawConstants;

namespace Veldrid.Vk
{
    internal unsafe class VulkanCommandBuffer : CommandBuffer
    {
        private readonly VkGraphicsDevice _gd;
        private readonly CommandBufferFlags _flags;
        private readonly VkCommandPool _pool;
        private string _name;
        private RecordingState _state = RecordingState.Initial;
        private VkCommandBuffer _cb;

        private VkPipeline _currentGraphicsPipeline;
        private VkFramebuffer _currentFB;
        private VkPipeline _currentComputePipeline;

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

            _state = RecordingState.Recording;
        }

        public override void Dispose()
        {
            // TODO: Ref-counted disposal.
            vkDestroyCommandPool(_gd.Device, _pool, null);
        }

        internal override void BeginRenderPassCore(in RenderPassDescription rpi)
        {
            if (rpi.Framebuffer is VkSwapchainFramebuffer)
            {
                throw new VeldridException(
                    "BeginRenderPass cannot be called on a Swapchain's Framebuffer directly.");
            }
            _currentFB = Util.AssertSubtype<Framebuffer, VkFramebuffer>(rpi.Framebuffer);

            BeginIfNeeded();

            VkRenderPassBeginInfo rpBI = VkRenderPassBeginInfo.New();
            rpBI.renderPass = _currentFB.GetRenderPass(rpi);
            rpBI.framebuffer = _currentFB.CurrentFramebuffer;
            rpBI.renderArea = new VkRect2D(0, 0, rpi.Framebuffer.Width, rpi.Framebuffer.Height);

            if (rpi.LoadAction == LoadAction.Clear)
            {
                rpBI.clearValueCount += (uint)rpi.Framebuffer.ColorTargets.Count;
                if (rpi.Framebuffer.DepthTarget != null) { rpBI.clearValueCount += 1; }
            }

            VkClearValue* clears = stackalloc VkClearValue[(int)rpBI.clearValueCount];

            if (rpi.LoadAction == LoadAction.Clear)
            {
                for (uint i = 0; i < rpi.Framebuffer.ColorTargets.Count; i++)
                {
                    VkClearValue clearColor = new VkClearValue
                    {
                        color = new VkClearColorValue(rpi.ClearColor.R, rpi.ClearColor.G, rpi.ClearColor.B, rpi.ClearColor.A)
                    };
                    clears[i] = clearColor;
                }
                if (rpi.Framebuffer.DepthTarget != null)
                {
                    clears[rpi.Framebuffer.ColorTargets.Count] = new VkClearValue
                    {
                        depthStencil = new VkClearDepthStencilValue(rpi.ClearDepth, 0)
                    };
                    rpBI.pClearValues = clears;
                }
            }

            vkCmdBeginRenderPass(_cb, &rpBI, VkSubpassContents.Inline);

            SetViewportCore(0, new Viewport(0, 0, _currentFB.Width, _currentFB.Height, 0f, 1f));

            // TODO: Multiple scissors.
            vkCmdSetScissor(_cb, 0, 1, &rpBI.renderArea);
        }

        private protected override void EndRenderPassCore()
        {
            vkCmdEndRenderPass(_cb);
            // DebugFullPipelineBarrier();
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
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
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
            vkCmdDrawIndirect(_cb, vkBuffer.DeviceBuffer, offset, drawCount, stride);
        }

        private protected override void DrawIndexedIndirectCore(
            DeviceBuffer indirectBuffer,
            uint offset,
            uint drawCount,
            uint stride)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
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

        internal override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(indirectBuffer);
            vkCmdDispatchIndirect(_cb, vkBuffer.DeviceBuffer, offset);
        }

        private protected override void CopyBufferCore(
            DeviceBuffer source, uint sourceOffset,
            DeviceBuffer destination, uint destinationOffset,
            uint sizeInBytes)
        {
            BeginIfNeeded();
            VkBuffer vkSrc = (VkBuffer)source;
            VkBuffer vkDst = (VkBuffer)destination;
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
            VkBuffer vkbuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            vkCmdUpdateBuffer(_cb, vkbuffer.DeviceBuffer, bufferOffsetInBytes, sizeInBytes, source.ToPointer());
        }

        private enum RecordingState
        {
            Initial,
            Recording,
            Ended,
        }
    }
}

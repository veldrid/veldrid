using System;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System.Text;
using System.Diagnostics;

namespace Veldrid.Vk
{
    internal unsafe class VulkanCommandBuffer : CommandBuffer
    {
        private readonly VkGraphicsDevice _gd;
        private readonly CommandBufferFlags _flags;
        private readonly VkCommandPool _pool;
        private RecordingState _state = RecordingState.Initial;
        private VkCommandBuffer _cb;

        private bool _renderPassActive;
        private VkPipeline _currentGraphicsPipeline;
        private VkFramebuffer _currentFB;

        public VulkanCommandBuffer(VkGraphicsDevice gd, ref CommandBufferDescription description)
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
            if (_state == RecordingState.Ended)
            {
                throw new NotImplementedException("Recording more stuff into a submitted CB isn't implemented yet.");
            }

            Debug.Assert(_cb.Handle == IntPtr.Zero);
            VkCommandBufferAllocateInfo cbAI = VkCommandBufferAllocateInfo.New();
            cbAI.commandPool = _pool;
            cbAI.commandBufferCount = 1;
            cbAI.level = VkCommandBufferLevel.Primary;
            VkResult allocateResult = vkAllocateCommandBuffers(_gd.Device, ref cbAI, out _cb);
            CheckResult(allocateResult);

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
            _renderPassActive = true;

            SetViewport(0, new Viewport(0, 0, _currentFB.Width, _currentFB.Height, 0f, 1f));

            // TODO: Multiple scissors.
            vkCmdSetScissor(_cb, 0, 1, &rpBI.renderArea);
        }

        public override void EndRenderPass()
        {
            RequireActiveRenderPass();
            vkCmdEndRenderPass(_cb);
            _renderPassActive = false;
        }

        private void RequireActiveRenderPass()
        {
            if (!_renderPassActive)
            {
                throw new VeldridException($"A render pass must be active to use this method.");
            }
        }

        public override void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            RequireActiveRenderPass();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            vkCmdBindIndexBuffer(_cb, vkBuffer.DeviceBuffer, offset, VkFormats.VdToVkIndexFormat(format));
        }

        public override void BindPipeline(Pipeline pipeline)
        {
            RequireActiveRenderPass();
            _currentGraphicsPipeline = Util.AssertSubtype<Pipeline, VkPipeline>(pipeline);
            vkCmdBindPipeline(
                _cb,
                pipeline.IsComputePipeline ? VkPipelineBindPoint.Compute : VkPipelineBindPoint.Graphics,
                _currentGraphicsPipeline.DevicePipeline);
        }

        public override void BindResourceSet(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            RequireActiveRenderPass();
            VkResourceSet vkSet = Util.AssertSubtype<ResourceSet, VkResourceSet>(resourceSet);
            VkDescriptorSet descriptorSet = vkSet.DescriptorSet;
            fixed (uint* dynamicOffsetsPtr = &dynamicOffsets[0])
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

        public override void BindVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
            RequireActiveRenderPass();
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            Vulkan.VkBuffer deviceBuffer = vkBuffer.DeviceBuffer;
            ulong offset64 = offset;
            vkCmdBindVertexBuffers(_cb, index, 1, &deviceBuffer, &offset64);
        }

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            RequireActiveRenderPass();
            vkCmdDraw(_cb, vertexCount, instanceCount, vertexStart, instanceStart);
        }

        public override void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart)
        {
            RequireActiveRenderPass();
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

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            RequireActiveRenderPass();
            VkRect2D scissor = new VkRect2D((int)x, (int)y, width, height);
            vkCmdSetScissor(_cb, index, 1, &scissor);
        }

        public override void SetViewport(uint index, Viewport viewport)
        {
            RequireActiveRenderPass();

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
            if (_state != RecordingState.Ended)
            {
                BeginIfNeeded();
                vkEndCommandBuffer(_cb);
                _state = RecordingState.Ended;
            }
        }

        private enum RecordingState
        {
            Initial,
            Recording,
            Ended,
        }
    }
}

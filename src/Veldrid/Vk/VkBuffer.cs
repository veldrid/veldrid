using System;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;
using VkBufferUsageFlags = TerraFX.Interop.Vulkan.VkBufferUsageFlags;
using VulkanBuffer = TerraFX.Interop.Vulkan.VkBuffer;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkBuffer : DeviceBuffer, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanBuffer _deviceBuffer;
        private readonly VkMemoryBlock _memory;
        private readonly VkMemoryRequirements _bufferMemoryRequirements;
        public ResourceRefCount RefCount { get; }
        private bool _destroyed;
        private string? _name;
        public override bool IsDisposed => _destroyed;

        public VulkanBuffer DeviceBuffer => _deviceBuffer;
        public VkMemoryBlock Memory => _memory;

        public VkMemoryRequirements BufferMemoryRequirements => _bufferMemoryRequirements;

        public VkBuffer(VkGraphicsDevice gd, in BufferDescription bd) : base(bd)
        {
            _gd = gd;

            VkBufferUsageFlags vkUsage = VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_SRC_BIT | VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_DST_BIT;
            if ((bd.Usage & BufferUsage.VertexBuffer) == BufferUsage.VertexBuffer)
            {
                vkUsage |= VkBufferUsageFlags.VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;
            }
            if ((bd.Usage & BufferUsage.IndexBuffer) == BufferUsage.IndexBuffer)
            {
                vkUsage |= VkBufferUsageFlags.VK_BUFFER_USAGE_INDEX_BUFFER_BIT;
            }
            if ((bd.Usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer)
            {
                vkUsage |= VkBufferUsageFlags.VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
            }
            if ((bd.Usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite
                || (bd.Usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly)
            {
                vkUsage |= VkBufferUsageFlags.VK_BUFFER_USAGE_STORAGE_BUFFER_BIT;
            }
            if ((bd.Usage & BufferUsage.IndirectBuffer) == BufferUsage.IndirectBuffer)
            {
                vkUsage |= VkBufferUsageFlags.VK_BUFFER_USAGE_INDIRECT_BUFFER_BIT;
            }

            VkBufferCreateInfo bufferCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,
                size = bd.SizeInBytes,
                usage = vkUsage
            };
            VulkanBuffer deviceBuffer;
            VkResult result = vkCreateBuffer(gd.Device, &bufferCI, null, &deviceBuffer);
            CheckResult(result);
            _deviceBuffer = deviceBuffer;

            bool prefersDedicatedAllocation;
            if (_gd.GetBufferMemoryRequirements2 != null)
            {
                VkBufferMemoryRequirementsInfo2 memReqInfo2 = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_BUFFER_MEMORY_REQUIREMENTS_INFO_2,
                    buffer = _deviceBuffer
                };
                VkMemoryDedicatedRequirements dedicatedReqs = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_DEDICATED_REQUIREMENTS
                };
                VkMemoryRequirements2 memReqs2 = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_REQUIREMENTS_2,
                    pNext = &dedicatedReqs
                };
                _gd.GetBufferMemoryRequirements2(_gd.Device, &memReqInfo2, &memReqs2);
                _bufferMemoryRequirements = memReqs2.memoryRequirements;
                prefersDedicatedAllocation = dedicatedReqs.prefersDedicatedAllocation || dedicatedReqs.requiresDedicatedAllocation;
            }
            else
            {
                VkMemoryRequirements bufferMemoryRequirements;
                vkGetBufferMemoryRequirements(gd.Device, _deviceBuffer, &bufferMemoryRequirements);
                _bufferMemoryRequirements = bufferMemoryRequirements;
                prefersDedicatedAllocation = false;
            }

            bool isStaging = (bd.Usage & BufferUsage.StagingReadWrite) != 0;
            bool isDynamic = (bd.Usage & BufferUsage.DynamicReadWrite) != 0;
            bool hostVisible = isStaging || isDynamic;

            VkMemoryPropertyFlags memoryPropertyFlags = hostVisible
                ? VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT
                : VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;

            if (isDynamic)
            {
                memoryPropertyFlags |= VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
            }

            if ((bd.Usage & BufferUsage.StagingRead) != 0)
            {
                // Use "host cached" memory for staging when available, for better performance of GPU -> CPU transfers
                bool hostCachedAvailable = TryFindMemoryType(
                    gd.PhysicalDeviceMemProperties,
                    _bufferMemoryRequirements.memoryTypeBits,
                    memoryPropertyFlags | VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_CACHED_BIT,
                    out _);

                if (hostCachedAvailable)
                {
                    memoryPropertyFlags |= VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_CACHED_BIT;
                }
            }

            VkMemoryBlock memoryToken = gd.MemoryManager.Allocate(
                gd.PhysicalDeviceMemProperties,
                _bufferMemoryRequirements.memoryTypeBits,
                memoryPropertyFlags,
                hostVisible,
                _bufferMemoryRequirements.size,
                _bufferMemoryRequirements.alignment,
                prefersDedicatedAllocation,
                default,
                _deviceBuffer);
            _memory = memoryToken;
            result = vkBindBufferMemory(gd.Device, _deviceBuffer, _memory.DeviceMemory, _memory.Offset);
            CheckResult(result);

            RefCount = new ResourceRefCount(this);

            if (bd.InitialData != IntPtr.Zero)
            {
                gd.UpdateBuffer(this, 0, bd.InitialData, bd.SizeInBytes);
            }
        }

        public override string? Name
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
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                vkDestroyBuffer(_gd.Device, _deviceBuffer, null);
                _gd.MemoryManager.Free(_memory);
            }
        }

        void IResourceRefCountTarget.RefZeroed()
        {
            DisposeCore();
        }
    }
}

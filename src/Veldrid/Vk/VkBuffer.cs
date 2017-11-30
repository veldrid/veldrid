using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkBuffer : Buffer, VkDeferredDisposal
    {
        private readonly VkGraphicsDevice _gd;
        private readonly Vulkan.VkBuffer _deviceBuffer;
        private readonly VkMemoryBlock _memory;
        private readonly VkMemoryRequirements _bufferMemoryRequirements;

        public override uint SizeInBytes { get; }
        public override BufferUsage Usage { get; }

        public ReferenceTracker ReferenceTracker { get; } = new ReferenceTracker();

        public Vulkan.VkBuffer DeviceBuffer => _deviceBuffer;
        public VkMemoryBlock Memory => _memory;

        public VkMemoryRequirements BufferMemoryRequirements => _bufferMemoryRequirements;

        public VkBuffer(VkGraphicsDevice gd, uint sizeInBytes, BufferUsage usage)
        {
            _gd = gd;
            SizeInBytes = sizeInBytes;
            Usage = usage;

            VkBufferUsageFlags vkUsage = VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst;
            if ((usage & BufferUsage.VertexBuffer) == BufferUsage.VertexBuffer)
            {
                vkUsage |= VkBufferUsageFlags.VertexBuffer;
            }
            if ((usage & BufferUsage.IndexBuffer) == BufferUsage.IndexBuffer)
            {
                vkUsage |= VkBufferUsageFlags.IndexBuffer;
            }
            if ((usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer)
            {
                vkUsage |= VkBufferUsageFlags.UniformBuffer;
            }
            if ((usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite
                || (usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly)
            {
                vkUsage |= VkBufferUsageFlags.StorageBuffer;
            }
            if ((usage & BufferUsage.IndirectBuffer) == BufferUsage.IndirectBuffer)
            {
                vkUsage |= VkBufferUsageFlags.IndirectBuffer;
            }

            VkBufferCreateInfo bufferCI = VkBufferCreateInfo.New();
            bufferCI.size = sizeInBytes;
            bufferCI.usage = vkUsage;
            VkResult result = vkCreateBuffer(gd.Device, ref bufferCI, null, out _deviceBuffer);
            CheckResult(result);

            vkGetBufferMemoryRequirements(gd.Device, _deviceBuffer, out _bufferMemoryRequirements);

            bool hostVisible = (usage & BufferUsage.Dynamic) == BufferUsage.Dynamic
                || (usage & BufferUsage.Staging) == BufferUsage.Staging;

            VkMemoryPropertyFlags memoryPropertyFlags =
                hostVisible
                ? VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent
                : VkMemoryPropertyFlags.DeviceLocal;

            VkMemoryBlock memoryToken = gd.MemoryManager.Allocate(
                gd.PhysicalDeviceMemProperties,
                _bufferMemoryRequirements.memoryTypeBits,
                memoryPropertyFlags,
                hostVisible,
                _bufferMemoryRequirements.size,
                _bufferMemoryRequirements.alignment);
            _memory = memoryToken;
            result = vkBindBufferMemory(gd.Device, _deviceBuffer, _memory.DeviceMemory, _memory.Offset);
            CheckResult(result);
        }

        public override void Dispose()
        {
            _gd.DeferredDisposal(this);
        }

        public void DestroyResources()
        {
            vkDestroyBuffer(_gd.Device, _deviceBuffer, null);
            _gd.MemoryManager.Free(Memory);
        }
    }
}

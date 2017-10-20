using Vulkan;
using static Vd2.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Vd2.Vk
{
    internal unsafe class VkBuffer : Buffer
    {
        private readonly VkGraphicsDevice _gd;
        private readonly Vulkan.VkBuffer _deviceBuffer;
        private readonly VkMemoryBlock _memory;

        public ulong SizeInBytes { get; }

        public Vulkan.VkBuffer DeviceBuffer => _deviceBuffer;
        public VkMemoryBlock Memory => _memory;

        public VkBuffer(VkGraphicsDevice gd, ulong sizeInBytes, VkBufferUsageFlags usage)
        {
            _gd = gd;
            SizeInBytes = sizeInBytes;
            usage |= VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst;

            VkBufferCreateInfo bufferCI = VkBufferCreateInfo.New();
            bufferCI.size = sizeInBytes;
            bufferCI.usage = usage;
            VkResult result = vkCreateBuffer(gd.Device, ref bufferCI, null, out _deviceBuffer);
            CheckResult(result);

            vkGetBufferMemoryRequirements(gd.Device, _deviceBuffer, out VkMemoryRequirements bufferMemoryRequirements);

            VkMemoryPropertyFlags memoryProperties = VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent;
            uint memoryType = FindMemoryType(gd.PhysicalDevice, bufferMemoryRequirements.memoryTypeBits, memoryProperties);
            VkMemoryBlock memoryToken = gd.MemoryManager.Allocate(
                memoryType,
                bufferMemoryRequirements.size,
                bufferMemoryRequirements.alignment);
            _memory = memoryToken;
            vkBindBufferMemory(gd.Device, _deviceBuffer, _memory.DeviceMemory, _memory.Offset);
        }

        public void Dispose()
        {
            vkDestroyBuffer(_gd.Device, _deviceBuffer, null);
            _gd.MemoryManager.Free(Memory);
        }
    }
}

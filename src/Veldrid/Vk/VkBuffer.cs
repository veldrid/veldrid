using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkBuffer : Buffer
    {
        private readonly VkGraphicsDevice _gd;
        private readonly Vulkan.VkBuffer _deviceBuffer;
        private readonly VkMemoryBlock _memory;

        public ulong SizeInBytes { get; }

        public Vulkan.VkBuffer DeviceBuffer => _deviceBuffer;
        public VkMemoryBlock Memory => _memory;

        public VkBuffer(VkGraphicsDevice gd, ulong sizeInBytes, bool dynamic, VkBufferUsageFlags usage)
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

            VkMemoryBlock memoryToken = gd.MemoryManager.Allocate(
                gd.PhysicalDeviceMemProperties,
                bufferMemoryRequirements.memoryTypeBits,
                VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                dynamic,
                bufferMemoryRequirements.size,
                bufferMemoryRequirements.alignment);
            _memory = memoryToken;
            result = vkBindBufferMemory(gd.Device, _deviceBuffer, _memory.DeviceMemory, _memory.Offset);
            CheckResult(result);
        }

        public void Dispose()
        {
            vkDestroyBuffer(_gd.Device, _deviceBuffer, null);
            _gd.MemoryManager.Free(Memory);
        }
    }
}

using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;

namespace Veldrid.Vk
{
    internal unsafe class VkTextureCube : TextureCube
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkImage _deviceImage;
        private readonly VkMemoryBlock _memory;

        public override uint Width { get; }

        public override uint Height { get; }

        public override PixelFormat Format { get; }

        public override uint MipLevels { get; }

        public override uint ArrayLayers { get; }

        public override TextureUsage Usage { get; }

        public VkFormat VkFormat { get; }

        public VkImageLayout ImageLayout { get; set; }

        public VkImage DeviceImage => _deviceImage;

        public VkTextureCube(VkGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;

            Width = description.Width;
            Height = description.Height;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers * 6;
            Format = description.Format;
            VkFormat = VkFormats.VdToVkPixelFormat(Format);
            Usage = description.Usage;

            vkGetPhysicalDeviceImageFormatProperties(
                _gd.PhysicalDevice,
                VkFormat,
                VkImageType.Image2D,
                VkImageTiling.Linear,
                VkImageUsageFlags.Sampled,
                VkImageCreateFlags.CubeCompatible,
                out VkImageFormatProperties linearProps);

            vkGetPhysicalDeviceImageFormatProperties(
                _gd.PhysicalDevice,
                VkFormat,
                VkImageType.Image2D,
                VkImageTiling.Optimal,
                VkImageUsageFlags.Sampled,
                VkImageCreateFlags.CubeCompatible,
                out VkImageFormatProperties optimalProps);

            bool useSingleStagingBuffer = linearProps.maxArrayLayers >= 6;

            VkImageCreateInfo imageCI = VkImageCreateInfo.New();
            imageCI.imageType = VkImageType.Image2D;
            imageCI.flags = VkImageCreateFlags.CubeCompatible;
            imageCI.format = VkFormat;
            imageCI.extent.width = Width;
            imageCI.extent.height = Height;
            imageCI.extent.depth = 1;
            imageCI.mipLevels = 1;
            imageCI.arrayLayers = 6;
            imageCI.samples = VkSampleCountFlags.Count1;
            imageCI.tiling = useSingleStagingBuffer ? VkImageTiling.Linear : VkImageTiling.Optimal;
            imageCI.usage = VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst;
            imageCI.initialLayout = VkImageLayout.Preinitialized;

            VkResult result = vkCreateImage(_gd.Device, ref imageCI, null, out _deviceImage);
            CheckResult(result);
            ImageLayout = VkImageLayout.Preinitialized;

            vkGetImageMemoryRequirements(_gd.Device, _deviceImage, out VkMemoryRequirements memReqs);
            _memory = _gd.MemoryManager.Allocate(
                _gd.PhysicalDeviceMemProperties,
                memReqs.memoryTypeBits,
                VkMemoryPropertyFlags.DeviceLocal,
                false,
                memReqs.size,
                memReqs.alignment);
            vkBindImageMemory(_gd.Device, _deviceImage, _memory.DeviceMemory, _memory.Offset);
        }

        public override void Dispose()
        {
            vkDestroyImage(_gd.Device, _deviceImage, null);
            _gd.MemoryManager.Free(_memory);
        }
    }
}
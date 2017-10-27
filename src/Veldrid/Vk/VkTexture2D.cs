using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;

namespace Veldrid.Vk
{
    internal unsafe class VkTexture2D : Texture2D
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkImage _image;
        private readonly VkMemoryBlock _memory;

        public override uint Width { get; }

        public override uint Height { get; }

        public override PixelFormat Format { get; }

        public override uint MipLevels { get; }

        public override uint ArrayLayers { get; }

        public override TextureUsage Usage { get; }

        public VkImage DeviceImage => _image;
        public VkMemoryBlock MemoryBlock => _memory;
        public VkFormat VkFormat { get; }

        public VkImageLayout[] ImageLayouts { get; internal set; }

        internal VkTexture2D(VkGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;
            Width = description.Width;
            Height = description.Height;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Format = description.Format;
            Usage = description.Usage;
            if ((description.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                if (Format != PixelFormat.R16_UNorm && Format != PixelFormat.R32_Float)
                {
                    throw new NotImplementedException("The only supported depth texture formats are R16_UInt and R32_Float.");
                }
                if (Format == PixelFormat.R16_UNorm)
                {
                    VkFormat = VkFormat.D16Unorm;
                }
                else
                {
                    VkFormat = VkFormat.D32Sfloat;
                }
            }
            else
            {
                VkFormat = VkFormats.VdToVkPixelFormat(Format);
            }

            VkImageCreateInfo imageCI = VkImageCreateInfo.New();
            imageCI.mipLevels = MipLevels;
            imageCI.arrayLayers = description.ArrayLayers;
            imageCI.imageType = VkImageType.Image2D;
            imageCI.extent.width = Width;
            imageCI.extent.height = Height;
            imageCI.extent.depth = 1;
            imageCI.initialLayout = VkImageLayout.Preinitialized;
            imageCI.usage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.Sampled;
            bool isDepthStencil = (description.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil;
            if ((description.Usage & TextureUsage.Sampled) == TextureUsage.Sampled)
            {
                imageCI.usage |= VkImageUsageFlags.Sampled;
            }
            if (isDepthStencil)
            {
                imageCI.usage |= VkImageUsageFlags.DepthStencilAttachment;
            }
            imageCI.tiling = VkImageTiling.Optimal;
            imageCI.format = VkFormat;

            imageCI.samples = VkSampleCountFlags.Count1;

            VkResult result = vkCreateImage(gd.Device, ref imageCI, null, out _image);
            CheckResult(result);
            ImageLayouts = new VkImageLayout[MipLevels];
            for (int i = 0; i < MipLevels; i++)
            {
                ImageLayouts[i] = VkImageLayout.Preinitialized;
            }

            vkGetImageMemoryRequirements(gd.Device, _image, out VkMemoryRequirements memoryRequirements);

            VkMemoryBlock memoryToken = gd.MemoryManager.Allocate(
                gd.PhysicalDeviceMemProperties,
                memoryRequirements.memoryTypeBits,
                VkMemoryPropertyFlags.DeviceLocal,
                false,
                memoryRequirements.size,
                memoryRequirements.alignment);
            _memory = memoryToken;
            vkBindImageMemory(gd.Device, _image, _memory.DeviceMemory, _memory.Offset);
        }

        internal VkTexture2D(
            VkGraphicsDevice gd,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            VkFormat vkFormat,
            TextureUsage usage,
            VkImage existingImage)
        {
            _gd = gd;
            MipLevels = mipLevels;
            Width = width;
            Height = height;
            VkFormat = vkFormat;
            Format = VkFormats.VkToVdPixelFormat(VkFormat);
            ArrayLayers = arrayLayers;
            Usage = usage;
            _image = existingImage;
        }

        public override void Dispose()
        {
            vkDestroyImage(_gd.Device, _image, null);
            if (_memory != null)
            {
                _gd.MemoryManager.Free(_memory);
            }
        }
    }
}

using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Diagnostics;

namespace Veldrid.Vk
{
    internal unsafe class VkTexture : Texture
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkImage _image;
        private readonly VkMemoryBlock _memory;
        private bool _disposed;

        public override uint Width { get; }

        public override uint Height { get; }

        public override uint Depth { get; }

        public override PixelFormat Format { get; }

        public override uint MipLevels { get; }

        public override uint ArrayLayers { get; }

        public override TextureUsage Usage { get; }

        public override TextureSampleCount SampleCount { get; }

        public VkImage DeviceImage => _image;
        public VkMemoryBlock MemoryBlock => _memory;
        public VkFormat VkFormat { get; }
        public VkSampleCountFlags VkSampleCount { get; }

        public VkImageLayout[] ImageLayouts { get; internal set; }

        internal VkTexture(VkGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;
            Width = description.Width;
            Height = description.Height;
            Depth = description.Depth;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Format = description.Format;
            Usage = description.Usage;
            SampleCount = description.SampleCount;
            VkSampleCount = VkFormats.VdToVkSampleCount(SampleCount);
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
            imageCI.imageType = Depth == 1 ? VkImageType.Image2D : VkImageType.Image3D;
            imageCI.extent.width = Width;
            imageCI.extent.height = Height;
            imageCI.extent.depth = Depth;
            imageCI.initialLayout = VkImageLayout.Preinitialized;
            imageCI.usage = VkImageUsageFlags.TransferDst;
            bool isDepthStencil = (description.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil;
            if ((description.Usage & TextureUsage.Sampled) == TextureUsage.Sampled)
            {
                imageCI.usage |= VkImageUsageFlags.Sampled;
            }
            if (isDepthStencil)
            {
                imageCI.usage |= VkImageUsageFlags.DepthStencilAttachment;
            }
            if ((description.Usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget)
            {
                imageCI.usage |= VkImageUsageFlags.ColorAttachment;
            }
            if ((description.Usage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                imageCI.usage |= VkImageUsageFlags.Storage;
            }

            imageCI.tiling = VkImageTiling.Optimal;
            imageCI.format = VkFormat;

            imageCI.samples = VkSampleCount;
            if ((Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                imageCI.flags = VkImageCreateFlags.CubeCompatible;
                imageCI.arrayLayers *= 6;
            }

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

        internal VkTexture(
            VkGraphicsDevice gd,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            VkFormat vkFormat,
            TextureUsage usage,
            TextureSampleCount sampleCount,
            VkImage existingImage)
        {
            Debug.Assert(width > 0 && height > 0);
            _gd = gd;
            MipLevels = mipLevels;
            Width = width;
            Height = height;
            Depth = 1;
            VkFormat = vkFormat;
            Format = VkFormats.VkToVdPixelFormat(VkFormat);
            ArrayLayers = arrayLayers;
            Usage = usage;
            SampleCount = sampleCount;
            VkSampleCount = VkFormats.VdToVkSampleCount(sampleCount);
            _image = existingImage;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                vkDestroyImage(_gd.Device, _image, null);
                if (_memory != null)
                {
                    _gd.MemoryManager.Free(_memory);
                }
            }
        }

        internal void TransitionImageLayout(
            VkCommandBuffer cb,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageLayout newLayout)
        {
            if (ImageLayouts[baseMipLevel] != newLayout)
            {
                VulkanUtil.TransitionImageLayout(
                    cb,
                    DeviceImage,
                    baseMipLevel,
                    levelCount,
                    baseArrayLayer,
                    layerCount,
                    ImageLayouts[baseMipLevel],
                    newLayout);
                ImageLayouts[baseMipLevel] = newLayout;
            }
        }
    }
}

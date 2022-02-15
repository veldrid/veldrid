﻿using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkTextureView : TextureView
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkImageView _imageView;
        private bool _destroyed;
        private string _name;

        public VkImageView ImageView => _imageView;

        public new VkTexture Target => (VkTexture)base.Target;

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkTextureView(VkGraphicsDevice gd, ref TextureViewDescription description)
            : base(ref description)
        {
            _gd = gd;
            VkImageViewCreateInfo imageViewCI = VkImageViewCreateInfo.New();
            VkTexture tex = Util.AssertSubtype<Texture, VkTexture>(description.Target);
            imageViewCI.image = tex.OptimalDeviceImage;
            imageViewCI.format = VkFormats.VdToVkPixelFormat(Format, (Target.Usage & TextureUsage.DepthStencil) != 0);

            VkImageAspectFlags aspectFlags;
            if ((description.Target.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                aspectFlags = VkImageAspectFlags.Depth;
            }
            else
            {
                aspectFlags = VkImageAspectFlags.Color;
            }

            imageViewCI.subresourceRange = new VkImageSubresourceRange(
                aspectFlags,
                description.BaseMipLevel,
                description.MipLevels,
                description.BaseArrayLayer,
                description.ArrayLayers);

            switch (description.ViewType)
            {
                case TextureViewType.View1D:
                    imageViewCI.viewType = VkImageViewType.Image1D;
                    break;
                case TextureViewType.View1DArray:
                    imageViewCI.viewType = VkImageViewType.Image1DArray;
                    break;
                case TextureViewType.View2D:
                    imageViewCI.viewType = VkImageViewType.Image2D;
                    break;
                case TextureViewType.View2DArray:
                    imageViewCI.viewType = VkImageViewType.Image2DArray;
                    break;
                case TextureViewType.View3D:
                    imageViewCI.viewType = VkImageViewType.Image3D;
                    break;
                case TextureViewType.ViewCube:
                    imageViewCI.viewType = VkImageViewType.ImageCube;
                    imageViewCI.subresourceRange.layerCount *= 6;
                    break;
                
                case TextureViewType.ViewCubeArray:
                    imageViewCI.viewType = VkImageViewType.ImageCubeArray;
                    imageViewCI.subresourceRange.layerCount *= 6;
                    break;
            }

            vkCreateImageView(_gd.Device, ref imageViewCI, null, out _imageView);
            RefCount = new ResourceRefCount(DisposeCore);
        }

        public override string Name
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
                vkDestroyImageView(_gd.Device, ImageView, null);
            }
        }
    }
}

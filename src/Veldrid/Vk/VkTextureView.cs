using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkTextureView : TextureView
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkImageView _imageView;

        public VkImageView ImageView => _imageView;

        public VkTextureView(VkGraphicsDevice gd, ref TextureViewDescription description)
            : base(description.Target)
        {
            _gd = gd;
            VkImageViewCreateInfo imageViewCI = VkImageViewCreateInfo.New();
            VkTexture tex = Util.AssertSubtype<Texture, VkTexture>(description.Target);
            imageViewCI.image = tex.DeviceImage;
            imageViewCI.format = tex.VkFormat;

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
                0,
                description.Target.MipLevels,
                0,
                description.Target.ArrayLayers);

            if ((tex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                imageViewCI.viewType = tex.ArrayLayers == 1 ? VkImageViewType.ImageCube : VkImageViewType.ImageCubeArray;
                imageViewCI.subresourceRange.layerCount *= 6;
            }
            else if (tex.Depth == 1)
            {
                imageViewCI.viewType = tex.ArrayLayers == 1 ? VkImageViewType.Image2D : VkImageViewType.Image2DArray;
            }
            else
            {
                imageViewCI.viewType = VkImageViewType.Image3D;
            }

            vkCreateImageView(_gd.Device, ref imageViewCI, null, out _imageView);
        }

        public override void Dispose()
        {
            vkDestroyImageView(_gd.Device, ImageView, null);
        }
    }
}
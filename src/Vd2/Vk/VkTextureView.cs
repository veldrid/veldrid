using Vulkan;
using static Vd2.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Vd2.Vk
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
            if (description.Target is VkTexture2D vkTex2D)
            {
                imageViewCI.viewType = vkTex2D.ArrayLayers == 1 ? VkImageViewType.Image2D : VkImageViewType.Image2DArray;
                imageViewCI.image = vkTex2D.DeviceImage;
                imageViewCI.format = vkTex2D.VkFormat;
            }
            else if (description.Target is VkTextureCube vkTexCube)
            {
                imageViewCI.viewType = vkTexCube.ArrayLayers == 6 ? VkImageViewType.ImageCube : VkImageViewType.ImageCubeArray;
                imageViewCI.image = vkTexCube.DeviceImage;
                imageViewCI.format = vkTexCube.VkFormat;
            }

            VkImageAspectFlags aspectFlags;
            if ((description.Target.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                aspectFlags = VkImageAspectFlags.Depth;
            }
            else
            {
                aspectFlags = VkImageAspectFlags.Color;
            }

            imageViewCI.subresourceRange = new VkImageSubresourceRange(aspectFlags, 0, description.Target.MipLevels, 0, description.Target.ArrayLayers);

            vkCreateImageView(_gd.Device, ref imageViewCI, null, out _imageView);
        }

        public override void Dispose()
        {
            vkDestroyImageView(_gd.Device, ImageView, null);
        }
    }
}
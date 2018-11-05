using System;
using Veldrid.Vk;
using Vulkan;

namespace Veldrid
{
    public class BackendInfoVulkan
    {
        private readonly VkGraphicsDevice _gd;

        internal BackendInfoVulkan(VkGraphicsDevice gd)
        {
            _gd = gd;
        }

        public IntPtr Instance => _gd.Instance.Handle;
        public IntPtr Device => _gd.Device.Handle;
        public IntPtr PhysicalDevice => _gd.PhysicalDevice.Handle;
        public IntPtr GraphicsQueue => _gd.GraphicsQueue.Handle;
        public uint GraphicsQueueFamilyIndex => _gd.GraphicsQueueIndex;

        public void OverrideImageLayout(Texture texture, uint layout)
        {
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            for (uint layer = 0; layer < vkTex.ArrayLayers; layer++)
            {
                for (uint level = 0; level < vkTex.MipLevels; level++)
                {
                    vkTex.SetImageLayout(level, layer, (VkImageLayout)layout);
                }
            }
        }

        public ulong GetVkImage(Texture texture)
        {
            VkTexture vkTexture = Util.AssertSubtype<Texture, VkTexture>(texture);
            if ((vkTexture.Usage & TextureUsage.Staging) != 0)
            {
                throw new VeldridException(
                    $"{nameof(GetVkImage)} cannot be used if the {nameof(Texture)} " +
                    $"has {nameof(TextureUsage)}.{nameof(TextureUsage.Staging)}.");
            }

            return vkTexture.OptimalDeviceImage.Handle;
        }

        //public uint GetVkFormat(Texture texture) => (uint)Util.AssertSubtype<Texture, VkTexture>(texture).VkFormat;

        public void TransitionImageLayout(Texture texture, uint layout)
        {
            _gd.TransitionImageLayout(Util.AssertSubtype<Texture, VkTexture>(texture), (VkImageLayout)layout);
        }
    }
}

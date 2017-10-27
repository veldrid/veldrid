using System.Collections.Generic;
using Vulkan;

namespace Veldrid.Vk
{
    internal abstract class VkFramebufferBase : Framebuffer
    {
        public VkFramebufferBase(Texture2D depthTexture, IReadOnlyList<Texture2D> colorTextures)
            : base(depthTexture, colorTextures)
        {
        }

        public VkFramebufferBase() { }

        public abstract Vulkan.VkFramebuffer CurrentFramebuffer { get; }
        public abstract VkRenderPass RenderPass { get; }
    }
}

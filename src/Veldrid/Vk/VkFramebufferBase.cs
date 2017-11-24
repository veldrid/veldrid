using System.Collections.Generic;
using Vulkan;

namespace Veldrid.Vk
{
    internal abstract class VkFramebufferBase : Framebuffer
    {
        public VkFramebufferBase(
            FramebufferAttachmentDescription? depthTexture,
            IReadOnlyList<FramebufferAttachmentDescription> colorTextures)
            : base(depthTexture, colorTextures)
        {
            if (depthTexture != null)
            {
                AttachmentCount += 1;
            }
            AttachmentCount += (uint)colorTextures.Count;
        }

        public abstract uint RenderableWidth { get; }
        public abstract uint RenderableHeight { get; }

        public VkFramebufferBase() { }

        public abstract Vulkan.VkFramebuffer CurrentFramebuffer { get; }
        public abstract VkRenderPass RenderPassNoClear { get; }
        public abstract VkRenderPass RenderPassClear { get; }
        public uint AttachmentCount { get; }
    }
}

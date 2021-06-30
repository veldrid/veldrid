using System.Collections.Generic;
using Vulkan;

namespace Veldrid.Vk
{
    internal abstract class VkFramebufferBase : Framebuffer
    {
        internal readonly VkGraphicsDevice _gd;

        public VkFramebufferBase(
            VkGraphicsDevice gd,
            FramebufferAttachmentDescription? depthTexture,
            IReadOnlyList<FramebufferAttachmentDescription> colorTextures)
            : base(depthTexture, colorTextures)
        {
            _gd = gd;
            RefCountId = _gd.RefCountManager.Register(DisposeCore);
        }

        public VkFramebufferBase(VkGraphicsDevice gd)
        {
            _gd = gd;
            RefCountId = _gd.RefCountManager.Register(DisposeCore);
        }

        public uint RefCountId { get; }

        public abstract uint RenderableWidth { get; }
        public abstract uint RenderableHeight { get; }

        public override void Dispose()
        {
            _gd.RefCountManager.Decrement(RefCountId);
        }

        protected abstract void DisposeCore();

        public abstract Vulkan.VkFramebuffer CurrentFramebuffer { get; }
        public abstract VkRenderPass RenderPassNoClear_Init { get; }
        public abstract VkRenderPass RenderPassNoClear_Load { get; }
        public abstract VkRenderPass RenderPassClear { get; }
        public abstract uint AttachmentCount { get; }
        public abstract void TransitionToIntermediateLayout(VkCommandBuffer cb);
        public abstract void TransitionToFinalLayout(VkCommandBuffer cb);
    }
}

using System;
using TerraFX.Interop.Vulkan;

namespace Veldrid.Vulkan
{
    internal abstract class VkFramebufferBase : Framebuffer, IResourceRefCountTarget
    {
        public VkFramebufferBase(
            FramebufferAttachmentDescription? depthTexture,
            ReadOnlySpan<FramebufferAttachmentDescription> colorTextures)
            : base(depthTexture, colorTextures)
        {
            RefCount = new ResourceRefCount(this);
        }

        public VkFramebufferBase()
        {
            RefCount = new ResourceRefCount(this);
        }

        public ResourceRefCount RefCount { get; }

        public abstract VkExtent2D RenderableExtent { get; }

        public override void Dispose()
        {
            RefCount.DecrementDispose();
        }

        protected abstract void DisposeCore();

        public abstract TerraFX.Interop.Vulkan.VkFramebuffer CurrentFramebuffer { get; }
        public abstract VkRenderPass RenderPassNoClear_Init { get; }
        public abstract VkRenderPass RenderPassNoClear_Load { get; }
        public abstract VkRenderPass RenderPassClear { get; }

        public uint AttachmentCount { get; protected set; }

        public override bool IsDisposed => RefCount.IsDisposed;

        public abstract void TransitionToIntermediateLayout(VkCommandBuffer cb);
        public abstract void TransitionToFinalLayout(VkCommandBuffer cb, bool attachment);

        public FramebufferAttachment[] ColorTargetArray => _colorTargets;

        void IResourceRefCountTarget.RefZeroed()
        {
            DisposeCore();
        }
    }
}

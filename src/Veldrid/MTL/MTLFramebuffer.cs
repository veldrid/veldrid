using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal sealed class MTLFramebuffer : MTLFramebufferBase
    {
        public override bool IsRenderable => true;
        private bool _disposed;

        public MTLFramebuffer(MTLGraphicsDevice gd, in FramebufferDescription description)
            : base(gd, description)
        {
        }

        public override MTLRenderPassDescriptor CreateRenderPassDescriptor()
        {
            MTLRenderPassDescriptor ret = MTLRenderPassDescriptor.New();

            ReadOnlySpan<FramebufferAttachment> colorTargets = ColorTargets;
            for (int i = 0; i < colorTargets.Length; i++)
            {
                FramebufferAttachment colorTarget = colorTargets[i];
                MTLTexture mtlTarget = Util.AssertSubtype<Texture, MTLTexture>(colorTarget.Target);
                MTLRenderPassColorAttachmentDescriptor colorDescriptor = ret.colorAttachments[(uint)i];
                colorDescriptor.texture = mtlTarget.DeviceTexture;
                colorDescriptor.loadAction = MTLLoadAction.Load;
                colorDescriptor.slice = (UIntPtr)colorTarget.ArrayLayer;
                colorDescriptor.level = (UIntPtr)colorTarget.MipLevel;
            }

            if (DepthTarget != null)
            {
                FramebufferAttachment depthTarget = DepthTarget.GetValueOrDefault();

                MTLTexture mtlDepthTarget = Util.AssertSubtype<Texture, MTLTexture>(depthTarget.Target);
                MTLRenderPassDepthAttachmentDescriptor depthDescriptor = ret.depthAttachment;
                depthDescriptor.loadAction = MTLLoadAction.Load;
                depthDescriptor.storeAction = MTLStoreAction.Store;
                depthDescriptor.texture = mtlDepthTarget.DeviceTexture;
                depthDescriptor.slice = (UIntPtr)depthTarget.ArrayLayer;
                depthDescriptor.level = (UIntPtr)depthTarget.MipLevel;

                if (FormatHelpers.IsStencilFormat(mtlDepthTarget.Format))
                {
                    MTLRenderPassStencilAttachmentDescriptor stencilDescriptor = ret.stencilAttachment;
                    stencilDescriptor.loadAction = MTLLoadAction.Load;
                    stencilDescriptor.storeAction = MTLStoreAction.Store;
                    stencilDescriptor.texture = mtlDepthTarget.DeviceTexture;
                    stencilDescriptor.slice = (UIntPtr)depthTarget.ArrayLayer;
                }
            }

            return ret;
        }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}

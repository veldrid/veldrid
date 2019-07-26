using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal abstract class MTLFramebufferBase : Framebuffer
    {
        protected abstract MetalBindings.MTLTexture GetMtlTexture(uint target);

        public MTLRenderPassDescriptor CreateRenderPassDescriptor(in RenderPassDescription rpd)
        {
            MTLRenderPassDescriptor ret = MTLRenderPassDescriptor.New();
            for (uint i = 0; i < ColorTargets.Count; i++)
            {
                rpd.GetColorAttachment(
                    i,
                    out LoadAction loadAction,
                    out StoreAction storeAction,
                    out RgbaFloat clearColor);

                bool hasResolveTex = ResolveTargets.Count > i && ResolveTargets[(int)i].Target != null;
                FramebufferAttachment colorTarget = ColorTargets[(int)i];
                MTLRenderPassColorAttachmentDescriptor colorDescriptor = ret.colorAttachments[i];
                colorDescriptor.texture = GetMtlTexture(i);
                colorDescriptor.loadAction = MTLFormats.VdToMTLLoadAction(loadAction);
                colorDescriptor.storeAction = MTLFormats.VdToMTLStoreAction(storeAction, hasResolveTex);
                colorDescriptor.slice = (UIntPtr)colorTarget.ArrayLayer;
                colorDescriptor.level = (UIntPtr)colorTarget.MipLevel;

                if (hasResolveTex)
                {
                    FramebufferAttachment resolveAttachment = ResolveTargets[(int)i];
                    MTLTexture mtlResolveTex = Util.AssertSubtype<Texture, MTLTexture>(resolveAttachment.Target);
                    colorDescriptor.resolveTexture = mtlResolveTex.DeviceTexture;
                    colorDescriptor.resolveLevel = (UIntPtr)resolveAttachment.MipLevel;
                    if (mtlResolveTex.Type == TextureType.Texture2D)
                    {
                        colorDescriptor.resolveSlice = (UIntPtr)resolveAttachment.ArrayLayer;
                    }
                    else
                    {
                        colorDescriptor.resolveDepthPlane = (UIntPtr)resolveAttachment.ArrayLayer;
                    }
                }

                if (loadAction == LoadAction.Clear)
                {
                    colorDescriptor.clearColor = new MTLClearColor(
                        clearColor.R,
                        clearColor.G,
                        clearColor.B,
                        clearColor.A);
                }
            }

            if (DepthTarget != null)
            {
                MTLTexture mtlDepthTarget = Util.AssertSubtype<Texture, MTLTexture>(DepthTarget.Value.Target);
                MTLRenderPassDepthAttachmentDescriptor depthDescriptor = ret.depthAttachment;
                depthDescriptor.loadAction = MTLFormats.VdToMTLLoadAction(rpd.DepthLoadAction);
                depthDescriptor.storeAction = MTLFormats.VdToMTLStoreAction(rpd.DepthStoreAction, false);
                depthDescriptor.texture = mtlDepthTarget.DeviceTexture;
                depthDescriptor.slice = (UIntPtr)DepthTarget.Value.ArrayLayer;
                depthDescriptor.level = (UIntPtr)DepthTarget.Value.MipLevel;

                if (rpd.DepthLoadAction == LoadAction.Clear)
                {
                    depthDescriptor.clearDepth = rpd.ClearDepth;
                }

                if (FormatHelpers.IsStencilFormat(mtlDepthTarget.Format))
                {
                    MTLRenderPassStencilAttachmentDescriptor stencilDescriptor = ret.stencilAttachment;
                    stencilDescriptor.loadAction = MTLFormats.VdToMTLLoadAction(rpd.StencilLoadAction);
                    stencilDescriptor.storeAction = MTLFormats.VdToMTLStoreAction(rpd.StencilStoreAction, false);
                    stencilDescriptor.texture = mtlDepthTarget.DeviceTexture;
                    stencilDescriptor.slice = (UIntPtr)DepthTarget.Value.ArrayLayer;

                    if (rpd.StencilLoadAction == LoadAction.Clear)
                    {
                        stencilDescriptor.clearStencil = rpd.ClearStencil;
                    }
                }
            }

            return ret;
        }

        public abstract bool IsRenderable { get; }

        public override string Name { get; set; }

        public MTLFramebufferBase(MTLGraphicsDevice gd, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets, description.ResolveTargets)
        {
        }

        public MTLFramebufferBase()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchainFramebuffer : MTLFramebufferBase
    {
        private readonly MTLGraphicsDevice _gd;
        private MTLTexture _colorTexture;
        private MTLTexture _depthTexture;
        private readonly MTLSwapchain _parentSwapchain;
        private readonly PixelFormat _colorFormat;
        private bool _disposed;

        public override uint Width => _colorTexture.Width;
        public override uint Height => _colorTexture.Height;

        public override OutputDescription OutputDescription { get; }

        private FramebufferAttachment[] _colorTargets;

        private readonly FramebufferAttachment? _depthTarget;
        private readonly PixelFormat? _depthFormat;

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override FramebufferAttachment? DepthTarget => _depthTarget;

        public override bool IsDisposed => _disposed;

        public MTLSwapchainFramebuffer(
            MTLGraphicsDevice gd,
            MTLSwapchain parent,
            PixelFormat? depthFormat,
            PixelFormat colorFormat)
            : base()
        {
            _gd = gd;
            _parentSwapchain = parent;
            _colorFormat = colorFormat;

            OutputAttachmentDescription? depthAttachment = null;
            if (depthFormat != null)
            {
                _depthFormat = depthFormat;
                depthAttachment = new OutputAttachmentDescription(depthFormat.Value);
            }
            OutputAttachmentDescription colorAttachment = new OutputAttachmentDescription(colorFormat);

            OutputDescription = new OutputDescription(depthAttachment, colorAttachment);
        }

        private void RecreateDepthTexture(uint width, uint height)
        {
            Debug.Assert(_depthFormat.HasValue);
            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
            }

            _depthTexture = Util.AssertSubtype<Texture, MTLTexture>(
                _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    width, height, 1, 1, _depthFormat.Value, TextureUsage.DepthStencil)));
        }

        public void UpdateTextures(CAMetalDrawable drawable, CGSize size)
        {
            _colorTexture = new MTLTexture(drawable, size, _colorFormat);
            _colorTargets = new[] { new FramebufferAttachment(_colorTexture, 0) };

            if (_depthFormat.HasValue)
                RecreateDepthTexture((uint)size.width, (uint)size.height);
        }

        public override bool IsRenderable => !_parentSwapchain.CurrentDrawable.IsNull;

        public override MTLRenderPassDescriptor CreateRenderPassDescriptor()
        {
            MTLRenderPassDescriptor ret = MTLRenderPassDescriptor.New();
            var color0 = ret.colorAttachments[0];
            color0.texture = _parentSwapchain.CurrentDrawable.texture;
            color0.loadAction = MTLLoadAction.Load;

            if (_depthTarget != null)
            {
                var depthAttachment = ret.depthAttachment;
                depthAttachment.texture = _depthTexture.DeviceTexture;
                depthAttachment.loadAction = MTLLoadAction.Load;
                depthAttachment.storeAction = MTLStoreAction.Store;

                if (FormatHelpers.IsStencilFormat(_depthTarget.Value.Target.Format))
                {
                    MTLRenderPassStencilAttachmentDescriptor stencilDescriptor = ret.stencilAttachment;
                    stencilDescriptor.loadAction = MTLLoadAction.Load;
                    stencilDescriptor.storeAction = MTLStoreAction.Store;
                    stencilDescriptor.texture = _depthTexture.DeviceTexture;
                    stencilDescriptor.slice = (UIntPtr)_depthTarget.Value.ArrayLayer;
                }
            }

            return ret;
        }

        public override void Dispose()
        {
            _depthTexture?.Dispose();
            _disposed = true;
        }
    }
}

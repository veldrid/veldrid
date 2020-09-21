using System;
using System.Collections.Generic;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchainFramebuffer : MTLFramebufferBase
    {
        private readonly MTLGraphicsDevice _gd;
        private readonly MTLPlaceholderTexture _placeholderTexture;
        private MTLTexture _depthTexture;
        private readonly MTLSwapchain _parentSwapchain;
        private bool _disposed;

        public override uint Width => _placeholderTexture.Width;
        public override uint Height => _placeholderTexture.Height;

        public override OutputDescription OutputDescription { get; }

        private readonly FramebufferAttachment[] _colorTargets;
        private FramebufferAttachment? _depthTarget;
        private readonly PixelFormat? _depthFormat;

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override IReadOnlyList<FramebufferAttachment> ResolveTargets => Array.Empty<FramebufferAttachment>();
        public override FramebufferAttachment? DepthTarget => _depthTarget;
        private CAMetalDrawable _drawable;

        public CAMetalDrawable Drawable => _drawable;

        public override bool IsDisposed => _disposed;

        public MTLSwapchainFramebuffer(
            MTLGraphicsDevice gd,
            MTLSwapchain parent,
            uint width,
            uint height,
            PixelFormat? depthFormat,
            PixelFormat colorFormat)
            : base()
        {
            _gd = gd;
            _parentSwapchain = parent;

            OutputAttachmentDescription? depthAttachment = null;
            if (depthFormat != null)
            {
                _depthFormat = depthFormat;
                depthAttachment = new OutputAttachmentDescription(depthFormat.Value);
                RecreateDepthTexture(width, height);
            }
            OutputAttachmentDescription colorAttachment = new OutputAttachmentDescription(colorFormat);

            OutputDescription = new OutputDescription(depthAttachment, colorAttachment);
            _placeholderTexture = new MTLPlaceholderTexture(colorFormat);
            _placeholderTexture.Resize(width, height);
            _colorTargets = new[] { new FramebufferAttachment(_placeholderTexture, 0) };
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
            _depthTarget = new FramebufferAttachment(_depthTexture, 0);
        }

        public void Resize(uint width, uint height)
        {
            _placeholderTexture.Resize(width, height);

            if (_depthFormat.HasValue)
            {
                RecreateDepthTexture(width, height);
            }
        }

        public override bool IsRenderable => !_drawable.IsNull;
        public void SetDrawable(CAMetalDrawable drawable)
        {
            if (!_drawable.IsNull)
            {
                ObjectiveCRuntime.objc_msgSend(_drawable.NativePtr, "release");
            }
            _drawable = drawable;
        }

        public override void Dispose()
        {
            if (!_drawable.IsNull)
            {
                ObjectiveCRuntime.objc_msgSend(_drawable.NativePtr, "release");
            }
            _depthTexture?.Dispose();
            _disposed = true;
        }

        protected override MetalBindings.MTLTexture GetMtlTexture(uint target) => _drawable.texture;
    }
}

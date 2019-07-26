using System;
using System.Collections.Generic;

namespace Veldrid.WebGL
{
    internal class WebGLSwapchainFramebuffer : Framebuffer
    {
        private readonly GraphicsDevice _gd;
        private readonly PixelFormat? _depthFormat;

        public override uint Width => _colorTexture.Width;
        public override uint Height => _colorTexture.Height;

        public override OutputDescription OutputDescription { get; }
        public override string Name { get; set; }

        private readonly WebGLPlaceholderTexture _colorTexture;
        private readonly WebGLPlaceholderTexture _depthTexture;

        private readonly FramebufferAttachment[] _colorTargets;
        private readonly FramebufferAttachment? _depthTarget;

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override IReadOnlyList<FramebufferAttachment> ResolveTargets => Array.Empty<FramebufferAttachment>();
        public override FramebufferAttachment? DepthTarget => _depthTarget;

        public bool DisableSrgbConversion { get; }

        // Only valid for secondary swapchains.
        public WebGLFramebuffer Framebuffer { get; private set; }

        internal WebGLSwapchainFramebuffer(
            GraphicsDevice gd,
            uint width, uint height,
            PixelFormat colorFormat,
            PixelFormat? depthFormat,
            bool disableSrgbConversion)
        {
            _gd = gd;
            _depthFormat = depthFormat;
            // This is wrong, but it's not really used.
            OutputAttachmentDescription? depthDesc = _depthFormat != null
                ? new OutputAttachmentDescription(_depthFormat.Value)
                : (OutputAttachmentDescription?)null;
            OutputDescription = new OutputDescription(
                depthDesc,
                new OutputAttachmentDescription(colorFormat));

            _colorTexture = new WebGLPlaceholderTexture(
                width,
                height,
                colorFormat,
                TextureUsage.RenderTarget,
                TextureSampleCount.Count1);
            _colorTargets = new[] { new FramebufferAttachment(_colorTexture, 0) };

            if (_depthFormat != null)
            {
                _depthTexture = new WebGLPlaceholderTexture(
                    width,
                    height,
                    depthFormat.Value,
                    TextureUsage.DepthStencil,
                    TextureSampleCount.Count1);
                _depthTarget = new FramebufferAttachment(_depthTexture, 0);
            }

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);

            DisableSrgbConversion = disableSrgbConversion;
        }

        public void Resize(uint width, uint height)
        {
            _colorTexture.Resize(width, height);
            _depthTexture?.Resize(width, height);
        }

        public override void Dispose()
        {
        }
    }
}

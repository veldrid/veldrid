using System;
using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchainFramebuffer : Framebuffer
    {
        private readonly PixelFormat? _depthFormat;

        public override uint Width => _colorTexture.Width;
        public override uint Height => _colorTexture.Height;

        public override OutputDescription OutputDescription { get; }
        public override string Name { get; set; }

        private readonly OpenGLPlaceholderTexture _colorTexture;
        private readonly OpenGLPlaceholderTexture _depthTexture;

        private readonly FramebufferAttachment[] _colorTargets;
        private readonly FramebufferAttachment? _depthTarget;

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override FramebufferAttachment? DepthTarget => _depthTarget;

        internal OpenGLSwapchainFramebuffer(uint width, uint height, PixelFormat colorFormat, PixelFormat? depthFormat)
        {
            _depthFormat = depthFormat;
            // This is wrong, but it's not really used.
            OutputAttachmentDescription? depthDesc = _depthFormat != null
                ? new OutputAttachmentDescription(_depthFormat.Value)
                : (OutputAttachmentDescription?)null;
            OutputDescription = new OutputDescription(
                depthDesc,
                new OutputAttachmentDescription(colorFormat));

            _colorTexture = new OpenGLPlaceholderTexture(
                width,
                height,
                colorFormat,
                TextureUsage.RenderTarget,
                TextureSampleCount.Count1);
            _colorTargets = new[] { new FramebufferAttachment(_colorTexture, 0) };

            if (_depthFormat != null)
            {
                _depthTexture = new OpenGLPlaceholderTexture(
                    width,
                    height,
                    depthFormat.Value,
                    TextureUsage.DepthStencil,
                    TextureSampleCount.Count1);
                _depthTarget = new FramebufferAttachment(_depthTexture, 0);
            }

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
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

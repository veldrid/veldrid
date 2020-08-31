using System;
using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchainFramebuffer : Framebuffer
    {
        private readonly double _dpiScale;
        private readonly PixelFormat? _depthFormat;
        private bool _disposed;

        public override uint Width => _colorTexture.Width;
        public override uint Height => _colorTexture.Height;

        public override OutputDescription OutputDescription { get; }
        public override string Name { get; set; }
        public override bool IsDisposed => _disposed;

        private readonly OpenGLPlaceholderTexture _colorTexture;
        private readonly OpenGLPlaceholderTexture _depthTexture;

        private readonly FramebufferAttachment[] _colorTargets;
        private readonly FramebufferAttachment? _depthTarget;

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override FramebufferAttachment? DepthTarget => _depthTarget;

        public bool DisableSrgbConversion { get; }

        internal OpenGLSwapchainFramebuffer(
            uint width, uint height,
            double dpiScale,
            PixelFormat colorFormat,
            PixelFormat? depthFormat,
            bool disableSrgbConversion)
        {
            _dpiScale = dpiScale;
            _depthFormat = depthFormat;
            // This is wrong, but it's not really used.
            OutputAttachmentDescription? depthDesc = _depthFormat != null
                ? new OutputAttachmentDescription(_depthFormat.Value)
                : (OutputAttachmentDescription?)null;
            OutputDescription = new OutputDescription(
                depthDesc,
                new OutputAttachmentDescription(colorFormat));

            _colorTexture = new OpenGLPlaceholderTexture(
                (uint)(width * _dpiScale),
                (uint)(height * _dpiScale),
                colorFormat,
                TextureUsage.RenderTarget,
                TextureSampleCount.Count1);
            _colorTargets = new[] { new FramebufferAttachment(_colorTexture, 0) };

            if (_depthFormat != null)
            {
                _depthTexture = new OpenGLPlaceholderTexture(
                    (uint)(width * _dpiScale),
                    (uint)(height * _dpiScale),
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
            _colorTexture.Resize((uint)(width * _dpiScale), (uint)(height * _dpiScale));
            _depthTexture?.Resize((uint)(width * _dpiScale), (uint)(height * _dpiScale));
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}

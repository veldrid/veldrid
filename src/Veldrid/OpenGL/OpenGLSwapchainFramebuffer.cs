using System.Collections.Generic;
using System.Diagnostics;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchainFramebuffer : Framebuffer
    {
        private readonly GraphicsDevice _gd;
        private readonly PixelFormat? _depthFormat;
        public bool IsSecondarySwapchain { get; }

        public override uint Width => _colorTexture.Width;
        public override uint Height => _colorTexture.Height;

        public override OutputDescription OutputDescription { get; }
        public override string Name { get; set; }

        private readonly OpenGLPlaceholderTexture _colorTexture;
        private readonly OpenGLPlaceholderTexture _depthTexture;

        private readonly FramebufferAttachment[] _colorTargets;
        private readonly FramebufferAttachment? _depthTarget;
        private bool _needsResize;

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override FramebufferAttachment? DepthTarget => _depthTarget;

        // Only valid for secondary swapchains.
        public OpenGLFramebuffer Framebuffer { get; private set; }

        internal OpenGLSwapchainFramebuffer(
            GraphicsDevice gd,
            uint width, uint height,
            PixelFormat colorFormat,
            PixelFormat? depthFormat,
            bool isSecondary)
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

            IsSecondarySwapchain = isSecondary;
            _needsResize = IsSecondarySwapchain;
        }

        public void Resize(uint width, uint height)
        {
            _colorTexture.Resize(width, height);
            _depthTexture?.Resize(width, height);
            _needsResize = IsSecondarySwapchain;
        }

        public void FlushChanges()
        {
            if (_needsResize)
            {
                _needsResize = false;
                if (Framebuffer != null)
                {
                    DestroyFramebuffer();
                }

                uint width = _colorTexture.Width;
                uint height = _colorTexture.Height;

                TextureDescription colorDesc = TextureDescription.Texture2D(
                    width, height,
                    1, 1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.RenderTarget);
                Texture colorTex = _gd.ResourceFactory.CreateTexture(ref colorDesc);

                Texture depthTex = null;
                if (_depthFormat != null)
                {
                    TextureDescription depthDesc = TextureDescription.Texture2D(
                        width, height,
                        1, 1,
                        _depthFormat.Value,
                        TextureUsage.DepthStencil);
                    depthTex = _gd.ResourceFactory.CreateTexture(ref depthDesc);
                }

                Framebuffer = Util.AssertSubtype<Framebuffer, OpenGLFramebuffer>(
                    _gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthTex, colorTex)));
            }
        }

        public override void Dispose()
        {
            Debug.Assert(IsSecondarySwapchain);
            DestroyFramebuffer();
        }

        private void DestroyFramebuffer()
        {
            foreach (FramebufferAttachment colorTarget in Framebuffer.ColorTargets)
            {
                colorTarget.Target.Dispose();
            }
            Framebuffer.DepthTarget?.Target.Dispose();
            Framebuffer.Dispose();
        }
    }
}

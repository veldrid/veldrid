using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchainFramebuffer : Framebuffer
    {
        private uint _width;
        private uint _height;
        private readonly PixelFormat? _depthFormat;

        public override uint Width => _width;
        public override uint Height => _height;

        public override OutputDescription OutputDescription { get; }
        public override string Name { get; set; }

        internal OpenGLSwapchainFramebuffer(uint width, uint height, PixelFormat? depthFormat)
            : base(null, Array.Empty<FramebufferAttachment>())
        {
            _width = width;
            _height = height;
            _depthFormat = depthFormat;
            // This is wrong, but it's not really used.
            OutputAttachmentDescription? depthDesc = _depthFormat != null
                ? new OutputAttachmentDescription(_depthFormat.Value)
                : (OutputAttachmentDescription?)null;
            OutputDescription = new OutputDescription(
                depthDesc,
                new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));
        }

        public void Resize(uint width, uint height)
        {
            _width = width;
            _height = height;
        }

        public override void Dispose()
        {
        }
    }
}

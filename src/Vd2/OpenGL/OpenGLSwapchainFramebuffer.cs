using System;

namespace Vd2.OpenGL
{
    internal class OpenGLSwapchainFramebuffer : Framebuffer
    {
        private uint _width;
        private uint _height;

        public override uint Width => _width;
        public override uint Height => _height;

        public override OutputDescription OutputDescription { get; }

        internal OpenGLSwapchainFramebuffer(uint width, uint height) : base(null, Array.Empty<Texture2D>())
        {
            _width = width;
            _height = height;
            // This is wrong, but it's not really used.
            OutputDescription = new OutputDescription(
                new OutputAttachmentDescription(PixelFormat.R16_UNorm),
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

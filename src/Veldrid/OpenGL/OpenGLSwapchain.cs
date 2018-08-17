using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchain : Swapchain
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly OpenGLSwapchainFramebuffer _framebuffer;
        private readonly Action<uint, uint> _resizeAction;

        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank { get => _gd.SyncToVerticalBlank; set => _gd.SyncToVerticalBlank = value; }
        public override string Name { get; set; } = "OpenGL Context Swapchain";

        public OpenGLSwapchain(
            OpenGLGraphicsDevice gd,
            uint width,
            uint height,
            PixelFormat? depthFormat,
            Action<uint, uint> resizeAction)
        {
            _gd = gd;
            _framebuffer = new OpenGLSwapchainFramebuffer(
                _gd,
                width,
                height,
                PixelFormat.B8_G8_R8_A8_UNorm,
                depthFormat,
                false);
            _resizeAction = resizeAction;
        }

        public override void Resize(uint width, uint height)
        {
            _framebuffer.Resize(width, height);
            _resizeAction?.Invoke(width, height);
        }

        public override void Dispose()
        {
        }
    }
}

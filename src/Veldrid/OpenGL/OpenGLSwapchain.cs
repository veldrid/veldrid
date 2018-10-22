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
            OpenGLSwapchainFramebuffer framebuffer,
            Action<uint, uint> resizeAction)
        {
            _gd = gd;
            PixelFormat? depthFormat = framebuffer.DepthTarget?.Target.Format ?? null;

            _framebuffer = new OpenGLSwapchainFramebuffer(
                framebuffer.Width,
                framebuffer.Height,
                framebuffer.ColorTargets[0].Target.Format,
                depthFormat);
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

using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchain : Swapchain
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly OpenGLSwapchainFramebuffer _framebuffer;
        private readonly Action<uint, uint> _resizeAction;
        private readonly SwapchainSource _swapchainSource;

        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank { get => _gd.SyncToVerticalBlank; set => _gd.SyncToVerticalBlank = value; }
        public override string Name { get; set; } = "OpenGL Context Swapchain";

        public override Framebuffer[] Framebuffers { get; }

        public OpenGLSwapchain(
            OpenGLGraphicsDevice gd,
            OpenGLSwapchainFramebuffer framebuffer,
            Action<uint, uint> resizeAction,
            SwapchainSource swapchainSource)
        {
            _gd = gd;
            _framebuffer = framebuffer;
            Framebuffers = new[] { _framebuffer, _framebuffer };
            _resizeAction = resizeAction;
            _swapchainSource = swapchainSource;
        }

        public override void Resize(uint width, uint height)
        {
            _framebuffer.Resize(width, height);
            _resizeAction?.Invoke(width, height);
        }

        public override void Resize()
        {
            _swapchainSource.GetSize(out uint width, out uint height);
            Resize(width, height);
        }

        public override void Dispose()
        {
        }
    }
}

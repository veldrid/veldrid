using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLSwapchain : Swapchain
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly OpenGLSwapchainFramebuffer _framebuffer;
        private readonly Action<uint, uint> _resizeAction;
        private readonly SwapchainSource _swapchainSource;

        private uint _lastAcquiredImage;
        private bool _disposed;

        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank { get => _gd.SyncToVerticalBlank; set => _gd.SyncToVerticalBlank = value; }
        public override string Name { get; set; } = "OpenGL Context Swapchain";
        public override bool IsDisposed => _disposed;

        public override Framebuffer[] Framebuffers { get; }
        public override uint LastAcquiredImage => _lastAcquiredImage;
        public OpenGLSwapchain(
            OpenGLGraphicsDevice gd,
            OpenGLSwapchainFramebuffer framebuffer,
            Action<uint, uint> resizeAction,
            SwapchainSource swapchainSource)
        {
            _gd = gd;
            _framebuffer = framebuffer;
            Framebuffers = new[] { _framebuffer, _framebuffer };
            _lastAcquiredImage = BufferCount - 1;
            _resizeAction = resizeAction;
            _swapchainSource = swapchainSource;
        }

        public override void Resize(uint width, uint height)
        {
            _framebuffer.Resize(width, height);
            _resizeAction?.Invoke(width, height);
            _lastAcquiredImage = BufferCount - 1;
        }

        public void AcquireNextImage()
        {
            _lastAcquiredImage = (LastAcquiredImage + 1) % BufferCount;
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}

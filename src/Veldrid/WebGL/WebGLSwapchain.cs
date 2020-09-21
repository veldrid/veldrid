namespace Veldrid.WebGL
{
    internal class WebGLSwapchain : Swapchain
    {
        private WebGLGraphicsDevice _gd;
        private uint _lastAcquiredImage;
        private bool _disposed;

        public WebGLSwapchain(WebGLGraphicsDevice gd)
        {
            _gd = gd;
            Framebuffer = new WebGLSwapchainFramebuffer(
                gd,
                (uint)(int)gd.Canvas.GetObjectProperty("width"),
                (uint)(int)gd.Canvas.GetObjectProperty("height"),
                PixelFormat.R8_G8_B8_A8_UNorm,
                PixelFormat.R16_UNorm,
                false);
            Framebuffers = new[] { Framebuffer };
        }

        public override Framebuffer Framebuffer { get; }

        public override Framebuffer[] Framebuffers { get; }

        public override bool SyncToVerticalBlank { get; set; }
        public override string Name { get; set; }

        public override uint LastAcquiredImage => _lastAcquiredImage;

        public override bool IsDisposed => _disposed;

        public override void Resize(uint width, uint height)
        {
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}

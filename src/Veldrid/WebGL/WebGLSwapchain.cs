namespace Veldrid.WebGL
{
    internal class WebGLSwapchain : Swapchain
    {
        private WebGLGraphicsDevice _gd;

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

        public override bool SyncToVerticalBlank { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public override string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override void Resize(uint width, uint height)
        {
            throw new System.NotImplementedException();
        }

    }
}

namespace Veldrid.WebGL
{
    internal class WebGLSwapchain : Swapchain
    {
        private WebGLGraphicsDevice webGLGraphicsDevice;

        public WebGLSwapchain(WebGLGraphicsDevice webGLGraphicsDevice)
        {
            this.webGLGraphicsDevice = webGLGraphicsDevice;
            Framebuffer = new WebGLFramebuffer(this);
        }

        public override Framebuffer Framebuffer { get; }

        public override Framebuffer[] Framebuffers => throw new System.NotImplementedException();

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

        public override void Resize()
        {
            throw new System.NotImplementedException();
        }
    }
}

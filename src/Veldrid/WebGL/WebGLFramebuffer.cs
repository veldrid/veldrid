namespace Veldrid.WebGL
{
    internal class WebGLFramebuffer : Framebuffer
    {
        private readonly WebGLSwapchain _swapchain;

        public override string Name { get; set; }
        public WebGLDotNET.WebGLFramebuffer WglFramebuffer { get; }
        public bool IsSwapchainFramebuffer => _swapchain != null;

        public WebGLFramebuffer(WebGLGraphicsDevice gd, ref FramebufferDescription description)
        {
            WglFramebuffer = gd.Ctx.CreateFramebuffer();
        }

        public override void Dispose()
        {
            WglFramebuffer.Dispose();
        }
    }
}

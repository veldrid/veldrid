namespace Veldrid.WebGL
{
    internal class WebGLFramebuffer : Framebuffer
    {
        private WebGLSwapchain webGLSwapchain;

        public WebGLFramebuffer(WebGLSwapchain webGLSwapchain)
        {
            this.webGLSwapchain = webGLSwapchain;
        }

        public override string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace Veldrid.WebGL
{
    internal class WebGLResourceFactory : ResourceFactory
    {
        private readonly WebGLGraphicsDevice _gd;

        public WebGLResourceFactory(WebGLGraphicsDevice gd)
            : base(gd.Features)
        {
            _gd = gd;
        }

        public override GraphicsBackend BackendType => GraphicsBackend.WebGL;

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            throw new System.NotImplementedException();
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            throw new System.NotImplementedException();
        }

        public override Fence CreateFence(bool signaled)
        {
            return new WebGLFence(_gd, signaled);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            throw new System.NotImplementedException();
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            throw new System.NotImplementedException();
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            throw new System.NotImplementedException();
        }

        public override Swapchain CreateSwapchain(ref SwapchainDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override DeviceBuffer CreateBufferCore(ref BufferDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override CommandBuffer CreateCommandBufferCore(ref CommandBufferDescription description)
        {
            return new WebGLCommandBuffer(_gd, ref description);
        }

        protected override Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override Sampler CreateSamplerCore(ref SamplerDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override Semaphore CreateSemaphoreCore()
        {
            throw new System.NotImplementedException();
        }

        protected override Shader CreateShaderCore(ref ShaderDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            throw new System.NotImplementedException();
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            throw new System.NotImplementedException();
        }
    }
}

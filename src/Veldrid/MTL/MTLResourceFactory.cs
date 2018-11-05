using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLResourceFactory : ResourceFactory
    {
        private readonly MTLGraphicsDevice _gd;

        public MTLResourceFactory(MTLGraphicsDevice gd)
            : base(gd.Features)
        {
            _gd = gd;
        }

        public override GraphicsBackend BackendType => GraphicsBackend.Metal;

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            return new MTLCommandList(ref description, _gd);
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            return new MTLPipeline(ref description, _gd);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new MTLFramebuffer(_gd, ref description);
        }

        protected override Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
        {
            return new MTLPipeline(ref description, _gd);
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new MTLResourceLayout(ref description, _gd);
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            ValidationHelpers.ValidateResourceSet(_gd, ref description);
            return new MTLResourceSet(ref description, _gd);
        }

        protected override Sampler CreateSamplerCore(ref SamplerDescription description)
        {
            return new MTLSampler(ref description, _gd);
        }

        protected override Shader CreateShaderCore(ref ShaderDescription description)
        {
            return new MTLShader(ref description, _gd);
        }

        protected override DeviceBuffer CreateBufferCore(ref BufferDescription description)
        {
            return new MTLBuffer(ref description, _gd);
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            return new MTLTexture(ref description, _gd);
        }

        protected override Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
        {
            return new MTLTexture(nativeTexture, ref description);
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            return new MTLTextureView(ref description, _gd);
        }

        public override Fence CreateFence(bool signaled)
        {
            return new MTLFence(signaled);
        }

        public override Swapchain CreateSwapchain(ref SwapchainDescription description)
        {
            return new MTLSwapchain(_gd, ref description);
        }
    }
}

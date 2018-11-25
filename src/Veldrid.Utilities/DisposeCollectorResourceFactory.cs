namespace Veldrid.Utilities
{
    public class DisposeCollectorResourceFactory : ResourceFactory
    {
        public ResourceFactory Factory { get; }
        public DisposeCollector DisposeCollector { get; }

        public DisposeCollectorResourceFactory(ResourceFactory factory)
            : this(factory, new DisposeCollector())
        {
        }

        public DisposeCollectorResourceFactory(ResourceFactory factory, DisposeCollector disposeCollector)
            : base(factory.Features)
        {
            Factory = factory;
            DisposeCollector = disposeCollector;
        }

        public override GraphicsBackend BackendType => Factory.BackendType;

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            CommandList cl = Factory.CreateCommandList(ref description);
            DisposeCollector.Add(cl);
            return cl;
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            Framebuffer fb = Factory.CreateFramebuffer(ref description);
            DisposeCollector.Add(fb);
            return fb;
        }

        protected override DeviceBuffer CreateBufferCore(ref BufferDescription description)
        {
            DeviceBuffer buffer = Factory.CreateBuffer(ref description);
            DisposeCollector.Add(buffer);
            return buffer;
        }

        protected override Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
        {
            Pipeline pipeline = Factory.CreateGraphicsPipeline(ref description);
            DisposeCollector.Add(pipeline);
            return pipeline;
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            Pipeline pipeline = Factory.CreateComputePipeline(ref description);
            DisposeCollector.Add(pipeline);
            return pipeline;
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            ResourceLayout layout = Factory.CreateResourceLayout(ref description);
            DisposeCollector.Add(layout);
            return layout;
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            ResourceSet rs = Factory.CreateResourceSet(ref description);
            DisposeCollector.Add(rs);
            return rs;
        }

        protected override Sampler CreateSamplerCore(ref SamplerDescription description)
        {
            Sampler sampler = Factory.CreateSampler(ref description);
            DisposeCollector.Add(sampler);
            return sampler;
        }

        protected override Shader CreateShaderCore(ref ShaderDescription description)
        {
            Shader shader = Factory.CreateShader(ref description);
            DisposeCollector.Add(shader);
            return shader;
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            Texture tex = Factory.CreateTexture(ref description);
            DisposeCollector.Add(tex);
            return tex;
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            TextureView texView = Factory.CreateTextureView(ref description);
            DisposeCollector.Add(texView);
            return texView;
        }

        public override Fence CreateFence(bool signaled)
        {
            Fence f = Factory.CreateFence(signaled);
            DisposeCollector.Add(f);
            return f;
        }

        public override Swapchain CreateSwapchain(ref SwapchainDescription description)
        {
            Swapchain sc = Factory.CreateSwapchain(ref description);
            DisposeCollector.Add(sc);
            return sc;
        }

        protected override Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
        {
            Texture tex = Factory.CreateTexture(nativeTexture, ref description);
            DisposeCollector.Add(tex);
            return tex;
        }
    }
}

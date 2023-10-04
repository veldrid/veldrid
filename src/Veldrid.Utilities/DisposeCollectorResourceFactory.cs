using System;

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

        public override CommandList CreateCommandList(in CommandListDescription description)
        {
            CommandList cl = Factory.CreateCommandList(description);
            DisposeCollector.Add(cl);
            return cl;
        }

        public override Framebuffer CreateFramebuffer(in FramebufferDescription description)
        {
            Framebuffer fb = Factory.CreateFramebuffer(description);
            DisposeCollector.Add(fb);
            return fb;
        }

        public override DeviceBuffer CreateBuffer(in BufferDescription description)
        {
            DeviceBuffer buffer = Factory.CreateBuffer(description);
            DisposeCollector.Add(buffer);
            return buffer;
        }

        public override Pipeline CreateGraphicsPipeline(in GraphicsPipelineDescription description)
        {
            Pipeline pipeline = Factory.CreateGraphicsPipeline(description);
            DisposeCollector.Add(pipeline);
            return pipeline;
        }

        public override Pipeline CreateComputePipeline(in ComputePipelineDescription description)
        {
            Pipeline pipeline = Factory.CreateComputePipeline(description);
            DisposeCollector.Add(pipeline);
            return pipeline;
        }

        public override ResourceLayout CreateResourceLayout(in ResourceLayoutDescription description)
        {
            ResourceLayout layout = Factory.CreateResourceLayout(description);
            DisposeCollector.Add(layout);
            return layout;
        }

        public override ResourceSet CreateResourceSet(in ResourceSetDescription description)
        {
            ResourceSet rs = Factory.CreateResourceSet(description);
            DisposeCollector.Add(rs);
            return rs;
        }

        public override Sampler CreateSampler(in SamplerDescription description)
        {
            Sampler sampler = Factory.CreateSampler(description);
            DisposeCollector.Add(sampler);
            return sampler;
        }

        public override Shader CreateShader(in ShaderDescription description)
        {
            Shader shader = Factory.CreateShader(description);
            DisposeCollector.Add(shader);
            return shader;
        }

        public override Texture CreateTexture(in TextureDescription description)
        {
            Texture tex = Factory.CreateTexture(description);
            DisposeCollector.Add(tex);
            return tex;
        }

        public override TextureView CreateTextureView(in TextureViewDescription description)
        {
            TextureView texView = Factory.CreateTextureView(description);
            DisposeCollector.Add(texView);
            return texView;
        }

        public override Fence CreateFence(bool signaled)
        {
            Fence f = Factory.CreateFence(signaled);
            DisposeCollector.Add(f);
            return f;
        }

        public override Swapchain CreateSwapchain(in SwapchainDescription description)
        {
            Swapchain sc = Factory.CreateSwapchain(description);
            DisposeCollector.Add(sc);
            return sc;
        }

        public override Texture CreateTexture(ulong nativeTexture, in TextureDescription description)
        {
            Texture tex = Factory.CreateTexture(nativeTexture, description);
            DisposeCollector.Add(tex);
            return tex;
        }
    }
}

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

        public override IndexBuffer CreateIndexBuffer(ref IndexBufferDescription description)
        {
            IndexBuffer ib = Factory.CreateIndexBuffer(ref description);
            DisposeCollector.Add(ib);
            return ib;
        }

        public override Pipeline CreatePipeline(ref PipelineDescription description)
        {
            Pipeline pipeline = Factory.CreatePipeline(ref description);
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

        public override Sampler CreateSampler(ref SamplerDescription description)
        {
            Sampler sampler = Factory.CreateSampler(ref description);
            DisposeCollector.Add(sampler);
            return sampler;
        }

        public override Shader CreateShader(ref ShaderDescription description)
        {
            Shader shader = Factory.CreateShader(ref description);
            DisposeCollector.Add(shader);
            return shader;
        }

        public override Texture2D CreateTexture2D(ref TextureDescription description)
        {
            Texture2D tex2D = Factory.CreateTexture2D(ref description);
            DisposeCollector.Add(tex2D);
            return tex2D;
        }

        public override TextureCube CreateTextureCube(ref TextureDescription description)
        {
            TextureCube texCube = Factory.CreateTextureCube(ref description);
            DisposeCollector.Add(texCube);
            return texCube;
        }

        public override TextureView CreateTextureView(ref TextureViewDescription description)
        {
            TextureView texView = Factory.CreateTextureView(ref description);
            DisposeCollector.Add(texView);
            return texView;
        }

        public override UniformBuffer CreateUniformBuffer(ref BufferDescription description)
        {
            UniformBuffer ub = Factory.CreateUniformBuffer(ref description);
            DisposeCollector.Add(ub);
            return ub;
        }

        public override VertexBuffer CreateVertexBuffer(ref BufferDescription description)
        {
            VertexBuffer vb = Factory.CreateVertexBuffer(ref description);
            DisposeCollector.Add(vb);
            return vb;
        }
    }
}
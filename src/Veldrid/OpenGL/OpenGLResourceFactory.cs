namespace Veldrid.OpenGL
{
    internal class OpenGLResourceFactory : ResourceFactory
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly StagingMemoryPool _pool = new StagingMemoryPool();

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public OpenGLResourceFactory(OpenGLGraphicsDevice gd)
        {
            _gd = gd;
        }

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            return new OpenGLCommandList(ref description);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new OpenGLFramebuffer(_gd, ref description);
        }

        public override IndexBuffer CreateIndexBuffer(ref IndexBufferDescription description)
        {
            return new OpenGLIndexBuffer(ref description);
        }

        public override Pipeline CreatePipeline(ref PipelineDescription description)
        {
            return new OpenGLPipeline(_gd, ref description);
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new OpenGLResourceLayout(ref description);
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            return new OpenGLResourceSet(ref description);
        }

        public override Sampler CreateSampler(ref SamplerDescription description)
        {
            return new OpenGLSampler(_gd, ref description);
        }

        public override Shader CreateShader(ref ShaderDescription description)
        {
            StagingBlock stagingBlock = _pool.Stage(description.ShaderBytes);
            return new OpenGLShader(_gd, description.Stage, stagingBlock);
        }

        public override Texture CreateTexture(ref TextureDescription description)
        {
            return new OpenGLTexture(_gd, ref description);
        }

        public override TextureView CreateTextureView(ref TextureViewDescription description)
        {
            return new OpenGLTextureView(ref description);
        }

        public override UniformBuffer CreateUniformBuffer(ref BufferDescription description)
        {
            return new OpenGLUniformBuffer(ref description);
        }

        public override VertexBuffer CreateVertexBuffer(ref BufferDescription description)
        {
            return new OpenGLVertexBuffer(ref description);
        }
    }
}
namespace Veldrid
{
    public abstract class ResourceFactory
    {
        public abstract GraphicsBackend BackendType { get; }

        public Pipeline CreatePipeline(PipelineDescription description) => CreatePipeline(ref description);
        public abstract Pipeline CreatePipeline(ref PipelineDescription description);
        public Framebuffer CreateFramebuffer(FramebufferDescription description) => CreateFramebuffer(ref description);
        public abstract Framebuffer CreateFramebuffer(ref FramebufferDescription description);
        public Texture2D CreateTexture2D(TextureDescription description) => CreateTexture2D(ref description);
        public abstract Texture2D CreateTexture2D(ref TextureDescription description);
        public TextureCube CreateTextureCube(TextureDescription description) => CreateTextureCube(ref description);
        public abstract TextureCube CreateTextureCube(ref TextureDescription description);
        public TextureView CreateTextureView(Texture target) => CreateTextureView(new TextureViewDescription(target));
        public TextureView CreateTextureView(TextureViewDescription description) => CreateTextureView(ref description);
        public abstract TextureView CreateTextureView(ref TextureViewDescription description);
        public VertexBuffer CreateVertexBuffer(BufferDescription description) => CreateVertexBuffer(ref description);
        public abstract VertexBuffer CreateVertexBuffer(ref BufferDescription description);
        public IndexBuffer CreateIndexBuffer(IndexBufferDescription description) => CreateIndexBuffer(ref description);
        public abstract IndexBuffer CreateIndexBuffer(ref IndexBufferDescription description);
        public UniformBuffer CreateUniformBuffer(BufferDescription description) => CreateUniformBuffer(ref description);
        public abstract UniformBuffer CreateUniformBuffer(ref BufferDescription description);
        public Sampler CreateSampler(SamplerDescription description) => CreateSampler(ref description);
        public abstract Sampler CreateSampler(ref SamplerDescription description);
        public Shader CreateShader(ShaderDescription description) => CreateShader(ref description);
        public abstract Shader CreateShader(ref ShaderDescription description);
        public CommandList CreateCommandList() => CreateCommandList(new CommandListDescription());
        public CommandList CreateCommandList(CommandListDescription description) => CreateCommandList(ref description);
        public abstract CommandList CreateCommandList(ref CommandListDescription description);
        public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description) => CreateResourceLayout(ref description);
        public abstract ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description);
        public ResourceSet CreateResourceSet(ResourceSetDescription description) => CreateResourceSet(ref description);
        public abstract ResourceSet CreateResourceSet(ref ResourceSetDescription description);
    }
}

namespace Veldrid
{
    /// <summary>
    /// A device object responsible for the creation of graphics resources.
    /// </summary>
    public abstract class ResourceFactory
    {
        /// <summary>
        /// Gets the <see cref="GraphicsBackend"/> of this instance.
        /// </summary>
        public abstract GraphicsBackend BackendType { get; }

        /// <summary>
        /// Creates a new <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/>.</returns>
        public Pipeline CreatePipeline(PipelineDescription description) => CreatePipeline(ref description);
        /// <summary>
        /// Creates a new <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/>.</returns>
        public abstract Pipeline CreatePipeline(ref PipelineDescription description);

        /// <summary>
        /// Creates a new <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Framebuffer"/>.</returns>
        public Framebuffer CreateFramebuffer(FramebufferDescription description) => CreateFramebuffer(ref description);
        /// <summary>
        /// Creates a new <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Framebuffer"/>.</returns>
        public abstract Framebuffer CreateFramebuffer(ref FramebufferDescription description);

        /// <summary>
        /// Creates a new <see cref="Texture"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Texture"/>.</returns>
        public Texture CreateTexture(TextureDescription description) => CreateTexture(ref description);
        /// <summary>
        /// Creates a new <see cref="Texture"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Texture"/>.</returns>
        public abstract Texture CreateTexture(ref TextureDescription description);

        /// <summary>
        /// Creates a new <see cref="TextureView"/>.
        /// </summary>
        /// <param name="target">The target <see cref="Texture"/> used in the new view.</param>
        /// <returns>A new <see cref="TextureView"/>.</returns>
        public TextureView CreateTextureView(Texture target) => CreateTextureView(new TextureViewDescription(target));
        /// <summary>
        /// Creates a new <see cref="TextureView"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="TextureView"/>.</returns>
        public TextureView CreateTextureView(TextureViewDescription description) => CreateTextureView(ref description);
        /// <summary>
        /// Creates a new <see cref="TextureView"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="TextureView"/>.</returns>
        public abstract TextureView CreateTextureView(ref TextureViewDescription description);

        /// <summary>
        /// Creates a new <see cref="Buffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Buffer"/>.</returns>
        public Buffer CreateBuffer(BufferDescription description) => CreateBuffer(ref description);
        /// <summary>
        /// Creates a new <see cref="Buffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Buffer"/>.</returns>
        public abstract Buffer CreateBuffer(ref BufferDescription description);

        /// <summary>
        /// Creates a new <see cref="Sampler"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Sampler"/>.</returns>
        public Sampler CreateSampler(SamplerDescription description) => CreateSampler(ref description);
        /// <summary>
        /// Creates a new <see cref="Sampler"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Sampler"/>.</returns>
        public abstract Sampler CreateSampler(ref SamplerDescription description);

        /// <summary>
        /// Creates a new <see cref="Shader"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Shader"/>.</returns>
        public Shader CreateShader(ShaderDescription description) => CreateShader(ref description);
        /// <summary>
        /// Creates a new <see cref="Shader"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Shader"/>.</returns>
        public abstract Shader CreateShader(ref ShaderDescription description);

        /// <summary>
        /// Creates a new <see cref="CommandList"/>.
        /// </summary>
        /// <returns>A new <see cref="CommandList"/>.</returns>
        public CommandList CreateCommandList() => CreateCommandList(new CommandListDescription());
        /// <summary>
        /// Creates a new <see cref="CommandList"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="CommandList"/>.</returns>
        public CommandList CreateCommandList(CommandListDescription description) => CreateCommandList(ref description);
        /// <summary>
        /// Creates a new <see cref="CommandList"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="CommandList"/>.</returns>
        public abstract CommandList CreateCommandList(ref CommandListDescription description);

        /// <summary>
        /// Creates a new <see cref="ResourceLayout"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceLayout"/>.</returns>
        public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description) => CreateResourceLayout(ref description);
        /// <summary>
        /// Creates a new <see cref="ResourceLayout"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceLayout"/>.</returns>
        public abstract ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description);

        /// <summary>
        /// Creates a new <see cref="ResourceSet"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceSet"/>.</returns>
        public ResourceSet CreateResourceSet(ResourceSetDescription description) => CreateResourceSet(ref description);
        /// <summary>
        /// Creates a new <see cref="ResourceSet"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceSet"/>.</returns>
        public abstract ResourceSet CreateResourceSet(ref ResourceSetDescription description);

    }
}

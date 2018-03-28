namespace Veldrid
{
    /// <summary>
    /// A device object responsible for the creation of graphics resources.
    /// </summary>
    public abstract class ResourceFactory
    {
        public ResourceFactory(GraphicsDeviceFeatures features)
        {
            Features = features;
        }

        /// <summary>
        /// Gets the <see cref="GraphicsBackend"/> of this instance.
        /// </summary>
        public abstract GraphicsBackend BackendType { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsDeviceFeatures"/> this instance was created with.
        /// </summary>
        public GraphicsDeviceFeatures Features { get; }

        /// <summary>
        /// Creates a new <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/>.</returns>
        public Pipeline CreateGraphicsPipeline(GraphicsPipelineDescription description) => CreateGraphicsPipeline(ref description);
        /// <summary>
        /// Creates a new <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/> which, when bound to a CommandList, is used to dispatch draw commands.</returns>
        public Pipeline CreateGraphicsPipeline(ref GraphicsPipelineDescription description)
        {
#if VALIDATE_USAGE
            if (!description.RasterizerState.DepthClipEnabled && !Features.DepthClipDisable)
            {
                throw new VeldridException(
                    "RasterizerState.DepthClipEnabled must be true if GraphicsDeviceFeatures.DepthClipDisable is not supported.");
            }
            if (description.RasterizerState.FillMode == PolygonFillMode.Wireframe && !Features.FillModeWireframe)
            {
                throw new VeldridException(
                    "PolygonFillMode.Wireframe requires GraphicsDeviceFeatures.FillModeWireframe.");
            }
#endif
            return CreateGraphicsPipelineCore(ref description);
        }

        protected abstract Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description);

        /// <summary>
        /// Creates a new compute <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desirede properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/> which, when bound to a CommandList, is used to dispatch compute commands.</returns>
        public Pipeline CreateComputePipeline(ComputePipelineDescription description) => CreateComputePipeline(ref description);

        /// <summary>
        /// Creates a new compute <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desirede properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/> which, when bound to a CommandList, is used to dispatch compute commands.</returns>
        public abstract Pipeline CreateComputePipeline(ref ComputePipelineDescription description);

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
        public Texture CreateTexture(ref TextureDescription description)
        {
#if VALIDATE_USAGE
            if (description.Width == 0 || description.Height == 0 || description.Depth == 0)
            {
                throw new VeldridException("Width, Height, and Depth must be non-zero.");
            }
            if ((description.Format == PixelFormat.D24_UNorm_S8_UInt || description.Format == PixelFormat.D32_Float_S8_UInt)
                && (description.Usage & TextureUsage.DepthStencil) == 0)
            {
                throw new VeldridException("The givel PixelFormat can only be used in a Texture with DepthStencil usage.");
            }
            if ((description.Type == TextureType.Texture1D || description.Type == TextureType.Texture3D)
                && description.SampleCount != TextureSampleCount.Count1)
            {
                throw new VeldridException(
                    $"1D and 3D Textures must use {nameof(TextureSampleCount)}.{nameof(TextureSampleCount.Count1)}.");
            }
            if (description.Type == TextureType.Texture1D && !Features.Texture1D)
            {
                throw new VeldridException($"1D Textures are not supported by this device.");
            }
#endif
            return CreateTextureCore(ref description);
        }

        // TODO: private protected
        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected abstract Texture CreateTextureCore(ref TextureDescription description);

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
        public TextureView CreateTextureView(ref TextureViewDescription description)
        {
#if VALIDATE_USAGE
            if (description.MipLevels == 0 || description.ArrayLayers == 0
                || (description.BaseMipLevel + description.MipLevels) > description.Target.MipLevels
                || (description.BaseArrayLayer + description.ArrayLayers) > description.Target.ArrayLayers)
            {
                throw new VeldridException(
                    "TextureView mip level and array layer range must be contained in the target Texture.");
            }
            if ((description.Target.Usage & TextureUsage.Sampled) == 0
                && (description.Target.Usage & TextureUsage.Storage) == 0)
            {
                throw new VeldridException(
                    "To create a TextureView, the target texture must have either Sampled or Storage usage flags.");
            }
#endif

            return CreateTextureViewCore(ref description);
        }

        // TODO: private protected
        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected abstract TextureView CreateTextureViewCore(ref TextureViewDescription description);

        /// <summary>
        /// Creates a new <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="DeviceBuffer"/>.</returns>
        public DeviceBuffer CreateBuffer(BufferDescription description) => CreateBuffer(ref description);
        /// <summary>
        /// Creates a new <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="DeviceBuffer"/>.</returns>
        public DeviceBuffer CreateBuffer(ref BufferDescription description)
        {
#if VALIDATE_USAGE
            if ((description.Usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly
                || (description.Usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                if (description.StructureByteStride == 0)
                {
                    throw new VeldridException("Structured Buffer objects must have a non-zero StructureByteStride.");
                }
            }
            else if (description.StructureByteStride != 0)
            {
                throw new VeldridException("Non-structured Buffers must have a StructureByteStride of zero.");
            }
            if ((description.Usage & BufferUsage.Staging) != 0 && description.Usage != BufferUsage.Staging)
            {
                throw new VeldridException("Buffers with Staging Usage must not specify any other Usage flags.");
            }
            if ((description.Usage & BufferUsage.UniformBuffer) != 0 && (description.SizeInBytes % 16) != 0)
            {
                throw new VeldridException($"Uniform buffer size must be a multiple of 16 bytes.");
            }
#endif
            return CreateBufferCore(ref description);
        }

        // TODO: private protected
        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected abstract DeviceBuffer CreateBufferCore(ref BufferDescription description);

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
        public Sampler CreateSampler(ref SamplerDescription description)
        {
#if VALIDATE_USAGE
            if (!Features.SamplerLodBias && description.LodBias != 0)
            {
                throw new VeldridException(
                    "GraphicsDevice does not support Sampler LOD bias. SamplerDescription.LodBias must be 0.");
            }
            if (!Features.SamplerAnisotropy && description.Filter == SamplerFilter.Anisotropic)
            {
                throw new VeldridException(
                    "SamplerFilter.Anisotropic cannot be used unless GraphicsDeviceFeatures.SamplerAnisotropy is supported.");
            }
#endif

            return CreateSamplerCore(ref description);
        }

        protected abstract Sampler CreateSamplerCore(ref SamplerDescription description);

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
        public Shader CreateShader(ref ShaderDescription description)
        {
#if VALIDATE_USAGE
            if (!Features.ComputeShader && description.Stage == ShaderStages.Compute)
            {
                throw new VeldridException("GraphicsDevice does not support Compute Shaders.");
            }
            if (!Features.GeometryShader && description.Stage == ShaderStages.Geometry)
            {
                throw new VeldridException("GraphicsDevice does not support Compute Shaders.");
            }
            if (!Features.TessellationShaders
                && (description.Stage == ShaderStages.TessellationControl
                    || description.Stage == ShaderStages.TessellationEvaluation))
            {
                throw new VeldridException("GraphicsDevice does not support Tessellation Shaders.");
            }
#endif
            return CreateShaderCore(ref description);
        }

        protected abstract Shader CreateShaderCore(ref ShaderDescription description);

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

        /// <summary>
        /// Creates a new <see cref="Fence"/> in the given state.
        /// </summary>
        /// <param name="signaled">A value indicating whether the Fence should be in the signaled state when created.</param>
        /// <returns>A new <see cref="Fence"/>.</returns>
        public abstract Fence CreateFence(bool signaled);

        /// <summary>
        /// Creates a new <see cref="Swapchain"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Swapchain"/>.</returns>
        public Swapchain CreateSwapchain(SwapchainDescription description) => CreateSwapchain(ref description);
        /// <summary>
        /// Creates a new <see cref="Swapchain"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Swapchain"/>.</returns>
        public abstract Swapchain CreateSwapchain(ref SwapchainDescription description);
    }
}

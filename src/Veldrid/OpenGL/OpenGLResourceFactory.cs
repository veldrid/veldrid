using System;

namespace Veldrid.OpenGL
{
    internal sealed class OpenGLResourceFactory : ResourceFactory
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly StagingMemoryPool _pool;

        public override GraphicsBackend BackendType => _gd.BackendType;

        public unsafe OpenGLResourceFactory(OpenGLGraphicsDevice gd)
            : base(gd.Features)
        {
            _gd = gd;
            _pool = gd.StagingMemoryPool;
        }

        public override CommandList CreateCommandList(in CommandListDescription description)
        {
            return new OpenGLCommandList(_gd, description);
        }

        public override Framebuffer CreateFramebuffer(in FramebufferDescription description)
        {
            return new OpenGLFramebuffer(_gd, description);
        }

        public override Pipeline CreateGraphicsPipeline(in GraphicsPipelineDescription description)
        {
            ValidateGraphicsPipeline(description);
            OpenGLPipeline pipeline = new(_gd, description);
            _gd.EnsureResourceInitialized(pipeline);
            return pipeline;
        }

        public override Pipeline CreateComputePipeline(in ComputePipelineDescription description)
        {
            OpenGLPipeline pipeline = new(_gd, description);
            _gd.EnsureResourceInitialized(pipeline);
            return pipeline;
        }

        public override ResourceLayout CreateResourceLayout(in ResourceLayoutDescription description)
        {
            return new OpenGLResourceLayout(description);
        }

        public override ResourceSet CreateResourceSet(in ResourceSetDescription description)
        {
            ValidationHelpers.ValidateResourceSet(_gd, description);
            return new OpenGLResourceSet(description);
        }

        public override Sampler CreateSampler(in SamplerDescription description)
        {
            ValidateSampler(description);
            return new OpenGLSampler(_gd, description);
        }

        public override Shader CreateShader(in ShaderDescription description)
        {
            ValidateShader(description);
            StagingBlock stagingBlock = _pool.Stage(description.ShaderBytes);
            OpenGLShader shader = new(_gd, description.Stage, stagingBlock, description.EntryPoint);
            _gd.EnsureResourceInitialized(shader);
            return shader;
        }

        public override Texture CreateTexture(in TextureDescription description)
        {
            ValidateTexture(description);
            return new OpenGLTexture(_gd, description);
        }

        public override Texture CreateTexture(ulong nativeTexture, in TextureDescription description)
        {
            return new OpenGLTexture(_gd, (uint)nativeTexture, description);
        }

        public override TextureView CreateTextureView(in TextureViewDescription description)
        {
            ValidateTextureView(description);
            return new OpenGLTextureView(_gd, description);
        }

        public override DeviceBuffer CreateBuffer(in BufferDescription description)
        {
            ValidateBuffer(description);
            return new OpenGLBuffer(_gd, description);
        }

        public override Fence CreateFence(bool signaled)
        {
            return new OpenGLFence(signaled);
        }

        public override Swapchain CreateSwapchain(in SwapchainDescription description)
        {
            throw new NotSupportedException("OpenGL does not support creating Swapchain objects.");
        }
    }
}

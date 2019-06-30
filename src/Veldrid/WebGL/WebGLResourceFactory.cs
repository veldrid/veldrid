using System;

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
            throw new NotImplementedException();
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            throw new NotSupportedException();
        }

        public override Fence CreateFence(bool signaled)
        {
            return new WebGLFence(_gd, signaled);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new WebGLFramebuffer(_gd, ref description);
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new WebGLResourceLayout(ref description);
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            return new WebGLResourceSet(_gd, ref description);
        }

        public override Swapchain CreateSwapchain(ref SwapchainDescription description)
        {
            throw new VeldridException("WebGL does not support multiple swapchains.");
        }

        protected override DeviceBuffer CreateBufferCore(ref BufferDescription description)
        {
            return new WebGLBuffer(_gd, ref description);
        }

        protected override CommandBuffer CreateCommandBufferCore(ref CommandBufferDescription description)
        {
            return new WebGLCommandBuffer(_gd, ref description);
        }

        protected override Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
        {
            return new WebGLPipeline(_gd, ref description);
        }

        protected override Sampler CreateSamplerCore(ref SamplerDescription description)
        {
            return new WebGLSampler(_gd, ref description);
        }

        protected override Semaphore CreateSemaphoreCore()
        {
            return new WebGLSemaphore();
        }

        protected override Shader CreateShaderCore(ref ShaderDescription description)
        {
            return new WebGLShader(_gd, ref description);
        }

        protected override Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
        {
            throw new NotImplementedException();
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            return new WebGLTexture(_gd, ref description);
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            return new WebGLTextureView(_gd, description);
        }
    }
}

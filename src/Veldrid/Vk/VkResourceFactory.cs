using Vulkan;

namespace Veldrid.Vk
{
    internal class VkResourceFactory : ResourceFactory
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkDevice _device;

        public VkResourceFactory(VkGraphicsDevice vkGraphicsDevice)
        {
            _gd = vkGraphicsDevice;
            _device = vkGraphicsDevice.Device;
        }

        public override GraphicsBackend BackendType => GraphicsBackend.Vulkan;

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            return new VkCommandList(_gd, ref description);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new VkFramebuffer(_gd, ref description, false);
        }

        public override Pipeline CreateGraphicsPipeline(ref GraphicsPipelineDescription description)
        {
            return new VkPipeline(_gd, ref description);
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            return new VkPipeline(_gd, ref description);
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new VkResourceLayout(_gd, ref description);
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            return new VkResourceSet(_gd, ref description);
        }

        public override Sampler CreateSampler(ref SamplerDescription description)
        {
            return new VkSampler(_device, ref description);
        }

        public override Shader CreateShader(ref ShaderDescription description)
        {
            return new VkShader(_gd, ref description);
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            return new VkTexture(_gd, ref description);
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            return new VkTextureView(_gd, ref description);
        }

        protected override Buffer CreateBufferCore(ref BufferDescription description)
        {
            return new VkBuffer(_gd, description.SizeInBytes, description.Usage);
        }
    }
}
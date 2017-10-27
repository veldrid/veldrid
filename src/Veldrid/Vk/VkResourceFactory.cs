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
            return new VkFramebuffer(_gd, ref description);
        }

        public override IndexBuffer CreateIndexBuffer(ref IndexBufferDescription description)
        {
            return new VkIndexBuffer(_gd, ref description);
        }

        public override Pipeline CreatePipeline(ref PipelineDescription description)
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

        public override Texture2D CreateTexture2D(ref TextureDescription description)
        {
            return new VkTexture2D(_gd, ref description);
        }

        public override TextureCube CreateTextureCube(ref TextureDescription description)
        {
            return new VkTextureCube(_gd, ref description);
        }

        public override TextureView CreateTextureView(ref TextureViewDescription description)
        {
            return new VkTextureView(_gd, ref description);
        }

        public override UniformBuffer CreateUniformBuffer(ref BufferDescription description)
        {
            return new VkUniformBuffer(_gd, ref description);
        }

        public override VertexBuffer CreateVertexBuffer(ref BufferDescription description)
        {
            return new VkVertexBuffer(_gd, ref description);
        }
    }
}
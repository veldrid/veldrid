using System;
using TerraFX.Interop.Vulkan;

namespace Veldrid.Vulkan
{
    internal sealed class VkResourceFactory : ResourceFactory
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkDevice _device;

        public VkResourceFactory(VkGraphicsDevice vkGraphicsDevice)
            : base(vkGraphicsDevice.Features)
        {
            _gd = vkGraphicsDevice;
            _device = vkGraphicsDevice.Device;
        }

        public override GraphicsBackend BackendType => GraphicsBackend.Vulkan;

        public override CommandList CreateCommandList(in CommandListDescription description)
        {
            return new VkCommandList(_gd, description);
        }

        public override Framebuffer CreateFramebuffer(in FramebufferDescription description)
        {
            return new VkFramebuffer(_gd, description, false);
        }

        public override Pipeline CreateGraphicsPipeline(in GraphicsPipelineDescription description)
        {
            ValidateGraphicsPipeline(description);
            return new VkPipeline(_gd, description);
        }

        public override Pipeline CreateComputePipeline(in ComputePipelineDescription description)
        {
            return new VkPipeline(_gd, description);
        }

        public override ResourceLayout CreateResourceLayout(in ResourceLayoutDescription description)
        {
            return new VkResourceLayout(_gd, description);
        }

        public override ResourceSet CreateResourceSet(in ResourceSetDescription description)
        {
            ValidationHelpers.ValidateResourceSet(_gd, description);
            return new VkResourceSet(_gd, description);
        }

        public override Sampler CreateSampler(in SamplerDescription description)
        {
            ValidateSampler(description);
            return new VkSampler(_gd, description);
        }

        public override Shader CreateShader(in ShaderDescription description)
        {
            ValidateShader(description);
            return new VkShader(_gd, description);
        }

        public override Texture CreateTexture(in TextureDescription description)
        {
            ValidateTexture(description);
            return new VkTexture(_gd, description);
        }

        public override Texture CreateTexture(ulong nativeTexture, in TextureDescription description)
        {
            return new VkTexture(
                _gd,
                description.Width, description.Height,
                description.MipLevels, description.ArrayLayers,
                VkFormats.VdToVkPixelFormat(description.Format, (description.Usage & TextureUsage.DepthStencil) != 0),
                description.Usage,
                description.SampleCount,
                new VkImage(nativeTexture));
        }

        public override TextureView CreateTextureView(in TextureViewDescription description)
        {
            ValidateTextureView(description);
            return new VkTextureView(_gd, description);
        }

        public override DeviceBuffer CreateBuffer(in BufferDescription description)
        {
            ValidateBuffer(description);
            return new VkBuffer(_gd, description);
        }

        public override Fence CreateFence(bool signaled)
        {
            return new VkFence(_gd, signaled);
        }

        public override Swapchain CreateSwapchain(in SwapchainDescription description)
        {
            return new VkSwapchain(_gd, description);
        }
    }
}

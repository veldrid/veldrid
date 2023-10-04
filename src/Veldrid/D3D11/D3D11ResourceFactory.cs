using Vortice.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal sealed class D3D11ResourceFactory : ResourceFactory, IDisposable
    {
        private readonly D3D11GraphicsDevice _gd;
        private readonly ID3D11Device _device;
        private readonly D3D11ResourceCache _cache;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public D3D11ResourceFactory(D3D11GraphicsDevice gd)
            : base(gd.Features)
        {
            _gd = gd;
            _device = gd.Device;
            _cache = new D3D11ResourceCache(_device);
        }

        public override CommandList CreateCommandList(in CommandListDescription description)
        {
            return new D3D11CommandList(_gd, description);
        }

        public override Framebuffer CreateFramebuffer(in FramebufferDescription description)
        {
            return new D3D11Framebuffer(_device, description);
        }

        public override Pipeline CreateGraphicsPipeline(in GraphicsPipelineDescription description)
        {
            ValidateGraphicsPipeline(description);
            return new D3D11Pipeline(_cache, description);
        }

        public override Pipeline CreateComputePipeline(in ComputePipelineDescription description)
        {
            return new D3D11Pipeline(_cache, description);
        }

        public override ResourceLayout CreateResourceLayout(in ResourceLayoutDescription description)
        {
            return new D3D11ResourceLayout(description);
        }

        public override ResourceSet CreateResourceSet(in ResourceSetDescription description)
        {
            ValidationHelpers.ValidateResourceSet(_gd, description);
            return new D3D11ResourceSet(description);
        }

        public override Sampler CreateSampler(in SamplerDescription description)
        {
            ValidateSampler(description);
            return new D3D11Sampler(_device, description);
        }

        public override Shader CreateShader(in ShaderDescription description)
        {
            ValidateShader(description);
            return new D3D11Shader(_device, description);
        }

        public override Texture CreateTexture(in TextureDescription description)
        {
            ValidateTexture(description);
            return new D3D11Texture(_device, description);
        }

        public override Texture CreateTexture(ulong nativeTexture, in TextureDescription description)
        {
            ID3D11Texture2D existingTexture = new((IntPtr)nativeTexture);
            return new D3D11Texture(existingTexture, description.Type, description.Format);
        }

        public override TextureView CreateTextureView(in TextureViewDescription description)
        {
            ValidateTextureView(description);
            return new D3D11TextureView(_gd, description);
        }

        public override DeviceBuffer CreateBuffer(in BufferDescription description)
        {
            ValidateBuffer(description);
            return new D3D11Buffer(_device, description);
        }

        public override Fence CreateFence(bool signaled)
        {
            return new D3D11Fence(signaled);
        }

        public override Swapchain CreateSwapchain(in SwapchainDescription description)
        {
            return new D3D11Swapchain(_gd, description);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}

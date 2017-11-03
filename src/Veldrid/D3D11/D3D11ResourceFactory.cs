using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11ResourceFactory : ResourceFactory
    {
        private readonly D3D11GraphicsDevice _gd;
        private readonly Device _device;
        private readonly D3D11ResourceCache _cache;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public D3D11ResourceFactory(D3D11GraphicsDevice gd)
        {
            _gd = gd;
            _device = gd.Device;
            _cache = new D3D11ResourceCache(_device);
        }

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            return new D3D11CommandList(_gd, ref description);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new D3D11Framebuffer(_device, ref description);
        }

        public override IndexBuffer CreateIndexBuffer(ref IndexBufferDescription description)
        {
            return new D3D11IndexBuffer(_device, ref description);
        }

        public override Pipeline CreatePipeline(ref PipelineDescription description)
        {
            return new D3D11Pipeline(_cache, ref description);
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new D3D11ResourceLayout(ref description);
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            return new D3D11ResourceSet(ref description);
        }

        public override Sampler CreateSampler(ref SamplerDescription description)
        {
            return new D3D11Sampler(_device, ref description);
        }

        public override Shader CreateShader(ref ShaderDescription description)
        {
            return new D3D11Shader(_device, description);
        }

        public override Texture CreateTexture(ref TextureDescription description)
        {
            return new D3D11Texture(_device, ref description);
        }

        public override TextureView CreateTextureView(ref TextureViewDescription description)
        {
            return new D3D11TextureView(_device, ref description);
        }

        public override UniformBuffer CreateUniformBuffer(ref BufferDescription description)
        {
            return new D3D11UniformBuffer(_device, ref description);
        }

        public override VertexBuffer CreateVertexBuffer(ref BufferDescription description)
        {
            return new D3D11VertexBuffer(_device, ref description);
        }
    }
}
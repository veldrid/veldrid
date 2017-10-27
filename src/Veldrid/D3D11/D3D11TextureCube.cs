using System;
using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11TextureCube : TextureCube
    {
        public override uint Width { get; }
        public override uint Height { get; }
        public override uint MipLevels { get; }
        public override uint ArrayLayers { get; }
        public override PixelFormat Format { get; }
        public override TextureUsage Usage { get; }

        public SharpDX.Direct3D11.Texture2D DeviceTexture { get; private set; }

        public D3D11TextureCube(Device device, ref TextureDescription description)
        {
            Width = description.Width;
            Height = description.Height;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Format = description.Format;
            Usage = description.Usage;

            BindFlags bindFlags = BindFlags.None;
            if ((description.Usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget)
            {
                bindFlags |= BindFlags.RenderTarget;
            }
            if ((description.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                bindFlags |= BindFlags.DepthStencil;
            }
            if ((description.Usage & TextureUsage.Sampled) == TextureUsage.Sampled)
            {
                bindFlags |= BindFlags.ShaderResource;
            }

            Texture2DDescription deviceDescription = new Texture2DDescription()
            {
                Width = (int)description.Width,
                Height = (int)description.Height,
                MipLevels = (int)description.MipLevels,
                ArraySize = (int)description.ArrayLayers * 6,
                Format = D3D11Formats.ToDxgiFormat(description.Format, false),
                BindFlags = bindFlags,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            };

            DeviceTexture = new SharpDX.Direct3D11.Texture2D(device, deviceDescription);
        }

        public override void Dispose()
        {
            DeviceTexture.Dispose();
        }
    }
}
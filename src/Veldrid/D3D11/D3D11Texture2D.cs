using System;
using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11Texture2D : Texture2D
    {
        public override uint Width { get; }
        public override uint Height { get; }
        public override uint MipLevels { get; }
        public override uint ArrayLayers { get; }
        public override PixelFormat Format { get; }
        public override TextureUsage Usage { get; }

        public SharpDX.Direct3D11.Texture2D DeviceTexture { get; }

        public D3D11Texture2D(Device device, ref TextureDescription description)
        {
            Width = description.Width;
            Height = description.Height;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Format = description.Format;
            Usage = description.Usage;

            SharpDX.DXGI.Format dxgiFormat = D3D11Formats.ToDxgiFormat(
                description.Format, 
                (description.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil);

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
                ArraySize = (int)description.ArrayLayers,
                Format = dxgiFormat,
                BindFlags = bindFlags,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            };

            DeviceTexture = new SharpDX.Direct3D11.Texture2D(device, deviceDescription);
        }

        public D3D11Texture2D(SharpDX.Direct3D11.Texture2D existingTexture)
        {
            DeviceTexture = existingTexture;
            Width = (uint)existingTexture.Description.Width;
            Height = (uint)existingTexture.Description.Height;
            MipLevels = (uint)existingTexture.Description.MipLevels;
            ArrayLayers = (uint)existingTexture.Description.ArraySize;
            Format = D3D11Formats.ToVdFormat(existingTexture.Description.Format);
        }

        public override void Dispose()
        {
            DeviceTexture.Dispose();
        }
    }
}
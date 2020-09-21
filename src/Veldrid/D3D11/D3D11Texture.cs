using System;
using System.Diagnostics;
using Vortice.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11Texture : Texture
    {
        private readonly ID3D11Device _device;
        private string _name;

        public override uint Width { get; }
        public override uint Height { get; }
        public override uint Depth { get; }
        public override uint MipLevels { get; }
        public override uint ArrayLayers { get; }
        public override PixelFormat Format { get; }
        public override TextureUsage Usage { get; }
        public override TextureType Type { get; }
        public override TextureSampleCount SampleCount { get; }
        public override bool IsDisposed => DeviceTexture.IsDisposed;

        public ID3D11Resource DeviceTexture { get; }
        public Vortice.DXGI.Format DxgiFormat { get; }
        public Vortice.DXGI.Format TypelessDxgiFormat { get; }

        public D3D11Texture(ID3D11Device device, ref TextureDescription description)
        {
            _device = device;
            Width = description.Width;
            Height = description.Height;
            Depth = description.Depth;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Format = description.Format;
            Usage = description.Usage;
            Type = description.Type;
            SampleCount = description.SampleCount;

            DxgiFormat = D3D11Formats.ToDxgiFormat(
                description.Format,
                (description.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil);
            TypelessDxgiFormat = D3D11Formats.GetTypelessFormat(DxgiFormat);

            CpuAccessFlags cpuFlags = CpuAccessFlags.None;
            Usage resourceUsage = Vortice.Direct3D11.Usage.Default;
            BindFlags bindFlags = BindFlags.None;
            ResourceOptionFlags optionFlags = ResourceOptionFlags.None;

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
            if ((description.Usage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                bindFlags |= BindFlags.UnorderedAccess;
            }
            if ((description.Usage & TextureUsage.Staging) == TextureUsage.Staging)
            {
                cpuFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
                resourceUsage = Vortice.Direct3D11.Usage.Staging;
            }

            if ((description.Usage & TextureUsage.GenerateMipmaps) != 0)
            {
                bindFlags |= BindFlags.RenderTarget | BindFlags.ShaderResource;
                optionFlags |= ResourceOptionFlags.GenerateMips;
            }

            int arraySize = (int)description.ArrayLayers;
            if ((description.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                optionFlags = ResourceOptionFlags.TextureCube;
                arraySize *= 6;
            }

            int roundedWidth = (int)description.Width;
            int roundedHeight = (int)description.Height;
            if (FormatHelpers.IsCompressedFormat(description.Format))
            {
                roundedWidth = ((roundedWidth + 3) / 4) * 4;
                roundedHeight = ((roundedHeight + 3) / 4) * 4;
            }

            if (Type == TextureType.Texture1D)
            {
                Texture1DDescription desc1D = new Texture1DDescription()
                {
                    Width = roundedWidth,
                    MipLevels = (int)description.MipLevels,
                    ArraySize = arraySize,
                    Format = TypelessDxgiFormat,
                    BindFlags = bindFlags,
                    CpuAccessFlags = cpuFlags,
                    Usage = resourceUsage,
                    OptionFlags = optionFlags,
                };

                DeviceTexture = device.CreateTexture1D(desc1D);
            }
            else if (Type == TextureType.Texture2D)
            {
                Texture2DDescription deviceDescription = new Texture2DDescription()
                {
                    Width = roundedWidth,
                    Height = roundedHeight,
                    MipLevels = (int)description.MipLevels,
                    ArraySize = arraySize,
                    Format = TypelessDxgiFormat,
                    BindFlags = bindFlags,
                    CpuAccessFlags = cpuFlags,
                    Usage = resourceUsage,
                    SampleDescription = new Vortice.DXGI.SampleDescription((int)FormatHelpers.GetSampleCountUInt32(SampleCount), 0),
                    OptionFlags = optionFlags,
                };

                DeviceTexture = device.CreateTexture2D(deviceDescription);
            }
            else
            {
                Debug.Assert(Type == TextureType.Texture3D);
                Texture3DDescription desc3D = new Texture3DDescription()
                {
                    Width = roundedWidth,
                    Height = roundedHeight,
                    Depth = (int)description.Depth,
                    MipLevels = (int)description.MipLevels,
                    Format = TypelessDxgiFormat,
                    BindFlags = bindFlags,
                    CpuAccessFlags = cpuFlags,
                    Usage = resourceUsage,
                    OptionFlags = optionFlags,
                };

                DeviceTexture = device.CreateTexture3D(desc3D);
            }
        }

        public D3D11Texture(ID3D11Texture2D existingTexture, TextureType type, PixelFormat format)
        {
            _device = existingTexture.Device;
            DeviceTexture = existingTexture;
            Width = (uint)existingTexture.Description.Width;
            Height = (uint)existingTexture.Description.Height;
            Depth = 1;
            MipLevels = (uint)existingTexture.Description.MipLevels;
            ArrayLayers = (uint)existingTexture.Description.ArraySize;
            Format = format;
            SampleCount = FormatHelpers.GetSampleCount((uint)existingTexture.Description.SampleDescription.Count);
            Type = type;
            Usage = D3D11Formats.GetVdUsage(
                existingTexture.Description.BindFlags,
                existingTexture.Description.CpuAccessFlags,
                existingTexture.Description.OptionFlags);

            DxgiFormat = D3D11Formats.ToDxgiFormat(
                format,
                (Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil);
            TypelessDxgiFormat = D3D11Formats.GetTypelessFormat(DxgiFormat);
        }

        private protected override TextureView CreateFullTextureView(GraphicsDevice gd)
        {
            TextureViewDescription desc = new TextureViewDescription(this);
            D3D11GraphicsDevice d3d11GD = Util.AssertSubtype<GraphicsDevice, D3D11GraphicsDevice>(gd);
            return new D3D11TextureView(d3d11GD, ref desc);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                DeviceTexture.DebugName = value;
            }
        }

        private protected override void DisposeCore()
        {
            DeviceTexture.Dispose();
        }
    }
}

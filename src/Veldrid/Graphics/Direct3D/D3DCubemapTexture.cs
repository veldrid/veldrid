using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DCubemapTexture : D3DTexture, CubemapTexture
    {
        public override Texture2D DeviceTexture { get; }
        public override int Width { get; }
        public override int Height { get; }

        public D3DCubemapTexture(
            Device device,
            IntPtr pixelsFront,
            IntPtr pixelsBack,
            IntPtr pixelsLeft,
            IntPtr pixelsRight,
            IntPtr pixelsTop,
            IntPtr pixelsBottom,
            int width,
            int height,
            int pixelSizeInBytes,
            PixelFormat format)
        {
            Width = width;
            Height = height;
            int stride = width * pixelSizeInBytes;

            DataRectangle[] dataRectangles = new DataRectangle[]
            {
                new DataRectangle(pixelsRight, stride),
                new DataRectangle(pixelsLeft, stride),
                new DataRectangle(pixelsTop, stride),
                new DataRectangle(pixelsBottom, stride),
                new DataRectangle(pixelsBack, stride),
                new DataRectangle(pixelsFront, stride),
            };

            DeviceTexture = new Texture2D(
                device,
                new Texture2DDescription()
                {
                    Format = D3DFormats.VeldridToD3DPixelFormat(format),
                    ArraySize = 6,
                    MipLevels = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    Width = width,
                    Height = height,
                    OptionFlags = ResourceOptionFlags.TextureCube
                },
                dataRectangles);
        }

        public override ShaderResourceViewDescription GetShaderResourceViewDescription()
        {
            ShaderResourceViewDescription srvd = new ShaderResourceViewDescription();
            srvd.Format = D3DFormats.MapFormatForShaderResourceView(DeviceTexture.Description.Format);
            srvd.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
            srvd.TextureCube.MipLevels = 1;
            srvd.TextureCube.MostDetailedMip = 0;

            return srvd;
        }
    }
}
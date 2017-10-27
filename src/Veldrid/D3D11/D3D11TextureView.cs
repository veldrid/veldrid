using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11TextureView : TextureView
    {
        public ShaderResourceView ShaderResourceView { get; }

        public D3D11TextureView(Device device, ref TextureViewDescription description)
            : base(description.Target)
        {
            // TODO: This is stupid.
            if (description.Target is D3D11Texture2D d3dTex2d)
            {
                SharpDX.DXGI.Format dxgiFormat = D3D11Formats.GetViewFormat(d3dTex2d.DeviceTexture.Description.Format);
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    Format = dxgiFormat,
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource
                    {
                        MipLevels = (int)d3dTex2d.MipLevels,
                        MostDetailedMip = 0
                    }
                };

                ShaderResourceView = new ShaderResourceView(device, d3dTex2d.DeviceTexture, srvDesc);
            }
            else if (description.Target is D3D11TextureCube d3dTexCube)
            {
                SharpDX.DXGI.Format dxgiFormat = D3D11Formats.GetViewFormat(d3dTexCube.DeviceTexture.Description.Format);
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    Format = dxgiFormat,
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube,
                    TextureCube = new ShaderResourceViewDescription.TextureCubeResource
                    {
                        MipLevels = (int)d3dTexCube.MipLevels,
                        MostDetailedMip = 0
                    }
                };

                ShaderResourceView = new ShaderResourceView(device, d3dTexCube.DeviceTexture, srvDesc);
            }
            else
            {
                throw new VeldridException("Invalid texture type used in D3D11 Texture View: " + description.Target.GetType().FullName);
            }
        }

        public override void Dispose()
        {
            ShaderResourceView.Dispose();
        }
    }
}
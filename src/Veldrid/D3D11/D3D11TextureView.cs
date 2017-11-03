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
            D3D11Texture d3dTex = Util.AssertSubtype<Texture, D3D11Texture>(description.Target);
            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = D3D11Formats.GetViewFormat(d3dTex.DeviceTexture.Description.Format);

            if ((d3dTex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                if (d3dTex.ArrayLayers == 1)
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
                    srvDesc.TextureCube.MipLevels = (int)d3dTex.MipLevels;
                }
                else
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
                    srvDesc.TextureCubeArray.MipLevels = (int)d3dTex.MipLevels;
                    srvDesc.TextureCubeArray.CubeCount = (int)d3dTex.ArrayLayers;
                }
            }
            else if (d3dTex.Depth == 1)
            {
                if (d3dTex.ArrayLayers == 1)
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture2D.MipLevels = (int)d3dTex.MipLevels;
                }
                else
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;
                    srvDesc.Texture2DArray.MipLevels = (int)d3dTex.MipLevels;
                    srvDesc.Texture2DArray.ArraySize = (int)d3dTex.ArrayLayers;
                }
            }
            else
            {
                srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
                srvDesc.Texture3D.MipLevels = (int)d3dTex.MipLevels;
            }

            ShaderResourceView = new ShaderResourceView(device, d3dTex.DeviceTexture, srvDesc);
        }

        public override void Dispose()
        {
            ShaderResourceView.Dispose();
        }
    }
}
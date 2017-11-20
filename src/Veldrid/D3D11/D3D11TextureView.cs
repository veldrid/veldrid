using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11TextureView : TextureView
    {
        public ShaderResourceView ShaderResourceView { get; }
        public UnorderedAccessView UnorderedAccessView { get; }

        public D3D11TextureView(Device device, ref TextureViewDescription description)
            : base(ref description)
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

            if ((d3dTex.Usage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription();
                uavDesc.Format = D3D11Formats.GetViewFormat(d3dTex.DeviceTexture.Description.Format);

                if ((d3dTex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
                {
                    throw new NotSupportedException();
                }
                else if (d3dTex.Depth == 1)
                {
                    if (d3dTex.ArrayLayers == 1)
                    {
                        uavDesc.Dimension = UnorderedAccessViewDimension.Texture2D;
                        uavDesc.Texture2D.MipSlice = 0;
                    }
                    else
                    {
                        uavDesc.Dimension = UnorderedAccessViewDimension.Texture2DArray;
                        uavDesc.Texture2DArray.MipSlice = 0;
                        uavDesc.Texture2DArray.ArraySize = (int)d3dTex.ArrayLayers;
                    }
                }
                else
                {
                    uavDesc.Dimension = UnorderedAccessViewDimension.Texture3D;
                    uavDesc.Texture3D.MipSlice = 0;
                    uavDesc.Texture3D.WSize = (int)d3dTex.Depth;
                }

                UnorderedAccessView = new UnorderedAccessView(device, d3dTex.DeviceTexture, uavDesc);
            }
        }

        public override void Dispose()
        {
            ShaderResourceView.Dispose();
        }
    }
}
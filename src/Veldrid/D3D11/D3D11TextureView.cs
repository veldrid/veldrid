using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11TextureView : TextureView
    {
        private string _name;

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
                    srvDesc.TextureCube.MostDetailedMip = (int)description.BaseMipLevel;
                    srvDesc.TextureCube.MipLevels = (int)description.MipLevels;
                }
                else
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
                    srvDesc.TextureCubeArray.MostDetailedMip = (int)description.BaseMipLevel;
                    srvDesc.TextureCubeArray.MipLevels = (int)description.MipLevels;
                    srvDesc.TextureCubeArray.First2DArrayFace = (int)description.BaseArrayLayer;
                    srvDesc.TextureCubeArray.CubeCount = (int)d3dTex.ArrayLayers;
                }
            }
            else if (d3dTex.Depth == 1)
            {
                if (d3dTex.ArrayLayers == 1)
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture2D.MostDetailedMip = (int)description.BaseMipLevel;
                    srvDesc.Texture2D.MipLevels = (int)description.MipLevels;
                }
                else
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;
                    srvDesc.Texture2DArray.MostDetailedMip = (int)description.BaseMipLevel;
                    srvDesc.Texture2DArray.MipLevels = (int)description.MipLevels;
                    srvDesc.Texture2DArray.FirstArraySlice = (int)description.BaseArrayLayer;
                    srvDesc.Texture2DArray.ArraySize = (int)description.ArrayLayers;
                }
            }
            else
            {
                srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
                srvDesc.Texture3D.MostDetailedMip = (int)description.BaseMipLevel;
                srvDesc.Texture3D.MipLevels = (int)description.MipLevels;
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
                        uavDesc.Texture2D.MipSlice = (int)description.BaseMipLevel;
                    }
                    else
                    {
                        uavDesc.Dimension = UnorderedAccessViewDimension.Texture2DArray;
                        uavDesc.Texture2DArray.MipSlice = (int)description.BaseMipLevel;
                        uavDesc.Texture2DArray.FirstArraySlice = (int)description.BaseArrayLayer;
                        uavDesc.Texture2DArray.ArraySize = (int)description.ArrayLayers;
                    }
                }
                else
                {
                    uavDesc.Dimension = UnorderedAccessViewDimension.Texture3D;
                    uavDesc.Texture3D.MipSlice = (int)description.BaseMipLevel;
                    uavDesc.Texture3D.FirstWSlice = (int)description.BaseArrayLayer;
                    uavDesc.Texture3D.WSize = (int)description.ArrayLayers;
                }

                UnorderedAccessView = new UnorderedAccessView(device, d3dTex.DeviceTexture, uavDesc);
            }
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (ShaderResourceView != null)
                {
                    ShaderResourceView.DebugName = value + "_SRV";
                }
                if (UnorderedAccessView != null)
                {
                    UnorderedAccessView.DebugName = value + "_UAV";
                }
            }
        }

        public override void Dispose()
        {
            ShaderResourceView.Dispose();
        }
    }
}
using Vortice.Direct3D11;

namespace Veldrid.D3D11
{
    internal static class D3D11Util
    {
        public static int ComputeSubresource(uint mipLevel, uint mipLevelCount, uint arrayLayer)
        {
            return (int)((arrayLayer * mipLevelCount) + mipLevel);
        }

        internal static ShaderResourceViewDescription GetSrvDesc(
            D3D11Texture tex,
            TextureViewType viewType,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            PixelFormat format)
        {
            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = D3D11Formats.GetViewFormat(
                D3D11Formats.ToDxgiFormat(format, (tex.Usage & TextureUsage.DepthStencil) != 0));

            switch (viewType)
            {
                case TextureViewType.View1D:
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture1D;
                    srvDesc.Texture1D.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.Texture1D.MipLevels = (int)levelCount;
                    break;
                case TextureViewType.View1DArray:
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture1DArray;
                    srvDesc.Texture1DArray.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.Texture1DArray.MipLevels = (int)levelCount;
                    srvDesc.Texture1DArray.FirstArraySlice = (int)baseArrayLayer;
                    srvDesc.Texture1DArray.ArraySize = (int)layerCount;
                    break;

                case TextureViewType.View2D:
                    if (tex.SampleCount == TextureSampleCount.Count1)
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2D;
                        srvDesc.Texture2D.MostDetailedMip = (int)baseMipLevel;
                        srvDesc.Texture2D.MipLevels = (int)levelCount;
                    }
                    else
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled;
                    }
                    break;
                case TextureViewType.View2DArray:
                    if (tex.SampleCount == TextureSampleCount.Count1)
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2DArray;
                        srvDesc.Texture2DArray.MostDetailedMip = (int)baseMipLevel;
                        srvDesc.Texture2DArray.MipLevels = (int)levelCount;
                        srvDesc.Texture2DArray.FirstArraySlice = (int)baseArrayLayer;
                        srvDesc.Texture2DArray.ArraySize = (int)layerCount;
                    }
                    else
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2DMultisampledArray;
                        srvDesc.Texture2DMSArray.FirstArraySlice = (int)baseArrayLayer;
                        srvDesc.Texture2DMSArray.ArraySize = (int)layerCount;
                    }

                    break;

                case TextureViewType.View3D:
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture3D;
                    srvDesc.Texture3D.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.Texture3D.MipLevels = (int)levelCount;
                    break;
                case TextureViewType.ViewCube:
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.TextureCube;
                    srvDesc.TextureCube.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.TextureCube.MipLevels = (int)levelCount;
                    break;
                
                case TextureViewType.ViewCubeArray:
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
                    srvDesc.TextureCubeArray.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.TextureCubeArray.MipLevels = (int)levelCount;
                    srvDesc.TextureCubeArray.First2DArrayFace = (int)baseArrayLayer;
                    srvDesc.TextureCubeArray.NumCubes = (int)tex.ArrayLayers;
                    break;
            }

            return srvDesc;
        }

        internal static int GetSyncInterval(bool syncToVBlank)
        {
            return syncToVBlank ? 1 : 0;
        }
    }
}

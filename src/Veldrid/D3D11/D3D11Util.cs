using System;
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
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            PixelFormat format)
        {
            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = D3D11Formats.GetViewFormat(
                D3D11Formats.ToDxgiFormat(format, (tex.Usage & TextureUsage.DepthStencil) != 0));

            if ((tex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                if (tex.ArrayLayers == 1)
                {
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.TextureCube;
                    srvDesc.TextureCube.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.TextureCube.MipLevels = (int)levelCount;
                }
                else
                {
                    srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
                    srvDesc.TextureCubeArray.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.TextureCubeArray.MipLevels = (int)levelCount;
                    srvDesc.TextureCubeArray.First2DArrayFace = (int)baseArrayLayer;
                    srvDesc.TextureCubeArray.NumCubes = (int)tex.ArrayLayers;
                }
            }
            else if (tex.Depth == 1)
            {
                if (tex.ArrayLayers == 1)
                {
                    if (tex.Type == TextureType.Texture1D)
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture1D;
                        srvDesc.Texture1D.MostDetailedMip = (int)baseMipLevel;
                        srvDesc.Texture1D.MipLevels = (int)levelCount;
                    }
                    else
                    {
                        if (tex.SampleCount == TextureSampleCount.Count1)
                            srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2D;
                        else
                            srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled;
                        srvDesc.Texture2D.MostDetailedMip = (int)baseMipLevel;
                        srvDesc.Texture2D.MipLevels = (int)levelCount;
                    }
                }
                else
                {
                    if (tex.Type == TextureType.Texture1D)
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture1DArray;
                        srvDesc.Texture1DArray.MostDetailedMip = (int)baseMipLevel;
                        srvDesc.Texture1DArray.MipLevels = (int)levelCount;
                        srvDesc.Texture1DArray.FirstArraySlice = (int)baseArrayLayer;
                        srvDesc.Texture1DArray.ArraySize = (int)layerCount;
                    }
                    else
                    {
                        srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2DArray;
                        srvDesc.Texture2DArray.MostDetailedMip = (int)baseMipLevel;
                        srvDesc.Texture2DArray.MipLevels = (int)levelCount;
                        srvDesc.Texture2DArray.FirstArraySlice = (int)baseArrayLayer;
                        srvDesc.Texture2DArray.ArraySize = (int)layerCount;
                    }
                }
            }
            else
            {
                srvDesc.ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture3D;
                srvDesc.Texture3D.MostDetailedMip = (int)baseMipLevel;
                srvDesc.Texture3D.MipLevels = (int)levelCount;
            }

            return srvDesc;
        }

        internal static void SetUnorderedAccessViewsKeepRTV(this ID3D11DeviceContext context, int startSlot, int numBuffers, IntPtr unorderedAccessBuffer, IntPtr uavCount)
        {
            context.OMSetRenderTargetsAndUnorderedAccessViews(
                ID3D11DeviceContext.KeepRenderTargetsAndDepthStencil,
                IntPtr.Zero, null, startSlot, numBuffers,
                unorderedAccessBuffer, uavCount);
        }

        internal static unsafe void SetUnorderedAccessViews(this ID3D11DeviceContext context, int startSlot, ID3D11UnorderedAccessView[] unorderedAccessViews, int[] uavInitialCounts)
        {
            IntPtr* unorderedAccessViewsOut_ = (IntPtr*)0;
            if (unorderedAccessViews != null)
            {
                IntPtr* unorderedAccessViewsOut__ = stackalloc IntPtr[unorderedAccessViews.Length];
                unorderedAccessViewsOut_ = unorderedAccessViewsOut__;
                for (int i = 0; i < unorderedAccessViews.Length; i++)
                    unorderedAccessViewsOut_[i] = (unorderedAccessViews[i] == null) ? IntPtr.Zero : unorderedAccessViews[i].NativePointer;
            }
            fixed (void* puav = uavInitialCounts)
                context.SetUnorderedAccessViewsKeepRTV(startSlot, unorderedAccessViews != null ? unorderedAccessViews.Length : 0, (IntPtr)unorderedAccessViewsOut_, (IntPtr)puav);
        }
    }
}

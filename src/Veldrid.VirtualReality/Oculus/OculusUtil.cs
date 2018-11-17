using System;

namespace Veldrid.VirtualReality.Oculus
{
    internal static class OculusUtil
    {
        public static TextureDescription GetVeldridTextureDescription(ovrTextureSwapChainDesc desc)
        {
            return TextureDescription.Texture2D(
                (uint)desc.Width, (uint)desc.Height,
                (uint)desc.MipLevels,
                (uint)desc.ArraySize,
                GetVeldridPixelFormat(desc.Format),
                GetVeldridTextureUsage(desc.BindFlags),
                GetSampleCount(desc.SampleCount));
        }

        public static TextureDescription GetVeldridTextureDescription(ovrMirrorTextureDesc desc)
        {
            return TextureDescription.Texture2D(
                (uint)desc.Width, (uint)desc.Height,
                1,
                1,
                GetVeldridPixelFormat(desc.Format),
                TextureUsage.Sampled,
                TextureSampleCount.Count1);
        }

        public static PixelFormat GetVeldridPixelFormat(ovrTextureFormat format)
        {
            switch (format)
            {
                case ovrTextureFormat.R8G8B8A8_UNORM:
                    return PixelFormat.R8_G8_B8_A8_UNorm;
                case ovrTextureFormat.R8G8B8A8_UNORM_SRGB:
                    return PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
                case ovrTextureFormat.B8G8R8A8_UNORM:
                    return PixelFormat.B8_G8_R8_A8_UNorm;
                case ovrTextureFormat.B8G8R8A8_UNORM_SRGB:
                    return PixelFormat.B8_G8_R8_A8_UNorm_SRgb;
                case ovrTextureFormat.R16G16B16A16_FLOAT:
                    return PixelFormat.R16_G16_B16_A16_Float;
                case ovrTextureFormat.R11G11B10_FLOAT:
                    return PixelFormat.R11_G11_B10_Float;
                case ovrTextureFormat.D16_UNORM:
                    return PixelFormat.R16_UNorm;
                case ovrTextureFormat.D24_UNORM_S8_UINT:
                    return PixelFormat.D24_UNorm_S8_UInt;
                case ovrTextureFormat.D32_FLOAT:
                    return PixelFormat.R32_Float;
                case ovrTextureFormat.D32_FLOAT_S8X24_UINT:
                    return PixelFormat.D32_Float_S8_UInt;
                case ovrTextureFormat.BC1_UNORM:
                    return PixelFormat.BC1_Rgba_UNorm;
                case ovrTextureFormat.BC2_UNORM:
                    return PixelFormat.BC2_UNorm;
                case ovrTextureFormat.BC3_UNORM:
                    return PixelFormat.BC3_UNorm;
                case ovrTextureFormat.BC7_UNORM:
                    return PixelFormat.BC7_UNorm;
                default:
                    throw new VeldridException($"Unsupported Oculus texture format: {format}");
            }
        }

        public static TextureUsage GetVeldridTextureUsage(ovrTextureBindFlags bindFlags)
        {
            TextureUsage ret = TextureUsage.Sampled;
            if ((bindFlags & ovrTextureBindFlags.DX_DepthStencil) != 0)
            {
                ret |= TextureUsage.DepthStencil;
            }
            if ((bindFlags & ovrTextureBindFlags.DX_RenderTarget) != 0)
            {
                ret |= TextureUsage.RenderTarget;
            }
            if ((bindFlags & ovrTextureBindFlags.DX_UnorderedAccess) != 0)
            {
                ret |= TextureUsage.Storage;
            }

            return ret;
        }

        public static TextureSampleCount GetSampleCount(int sampleCount)
        {
            switch (sampleCount)
            {
                case 1:
                    return TextureSampleCount.Count1;
                case 2:
                    return TextureSampleCount.Count2;
                case 4:
                    return TextureSampleCount.Count4;
                case 8:
                    return TextureSampleCount.Count8;
                case 16:
                    return TextureSampleCount.Count16;
                case 32:
                    return TextureSampleCount.Count32;
                default:
                    throw new VeldridException($"Unsupported sample count: {sampleCount}");
            }
        }

        public static bool IsSrgbFormat(PixelFormat format)
        {
            return format == PixelFormat.B8_G8_R8_A8_UNorm_SRgb || format == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
        }
    }
}

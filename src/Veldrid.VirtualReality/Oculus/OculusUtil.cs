
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
            return format switch
            {
                ovrTextureFormat.R8G8B8A8_UNORM => PixelFormat.R8_G8_B8_A8_UNorm,
                ovrTextureFormat.R8G8B8A8_UNORM_SRGB => PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                ovrTextureFormat.B8G8R8A8_UNORM => PixelFormat.B8_G8_R8_A8_UNorm,
                ovrTextureFormat.B8G8R8A8_UNORM_SRGB => PixelFormat.B8_G8_R8_A8_UNorm_SRgb,
                ovrTextureFormat.R16G16B16A16_FLOAT => PixelFormat.R16_G16_B16_A16_Float,
                ovrTextureFormat.R11G11B10_FLOAT => PixelFormat.R11_G11_B10_Float,
                ovrTextureFormat.D16_UNORM => PixelFormat.R16_UNorm,
                ovrTextureFormat.D24_UNORM_S8_UINT => PixelFormat.D24_UNorm_S8_UInt,
                ovrTextureFormat.D32_FLOAT => PixelFormat.R32_Float,
                ovrTextureFormat.D32_FLOAT_S8X24_UINT => PixelFormat.D32_Float_S8_UInt,
                ovrTextureFormat.BC1_UNORM => PixelFormat.BC1_Rgba_UNorm,
                ovrTextureFormat.BC2_UNORM => PixelFormat.BC2_UNorm,
                ovrTextureFormat.BC3_UNORM => PixelFormat.BC3_UNorm,
                ovrTextureFormat.BC7_UNORM => PixelFormat.BC7_UNorm,
                _ => throw new VeldridException($"Unsupported Oculus texture format: {format}"),
            };
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
            return sampleCount switch
            {
                1 => TextureSampleCount.Count1,
                2 => TextureSampleCount.Count2,
                4 => TextureSampleCount.Count4,
                8 => TextureSampleCount.Count8,
                16 => TextureSampleCount.Count16,
                32 => TextureSampleCount.Count32,
                _ => throw new VeldridException($"Unsupported sample count: {sampleCount}"),
            };
        }

        public static bool IsSrgbFormat(PixelFormat format)
        {
            return format == PixelFormat.B8_G8_R8_A8_UNorm_SRgb || format == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
        }
    }
}

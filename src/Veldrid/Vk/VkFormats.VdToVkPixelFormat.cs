using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.VkFormat;

namespace Veldrid.Vulkan
{
    internal static partial class VkFormats
    {
        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static VkFormat VdToVkPixelFormat(PixelFormat format, bool toDepthFormat = false)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return VK_FORMAT_R8_UNORM;
                case PixelFormat.R8_SNorm:
                    return VK_FORMAT_R8_SNORM;
                case PixelFormat.R8_UInt:
                    return VK_FORMAT_R8_UINT;
                case PixelFormat.R8_SInt:
                    return VK_FORMAT_R8_SINT;

                case PixelFormat.R16_UNorm:
                    return toDepthFormat ? VK_FORMAT_D16_UNORM : VK_FORMAT_R16_UNORM;
                case PixelFormat.R16_SNorm:
                    return VK_FORMAT_R16_SNORM;
                case PixelFormat.R16_UInt:
                    return VK_FORMAT_R16_UINT;
                case PixelFormat.R16_SInt:
                    return VK_FORMAT_R16_SINT;
                case PixelFormat.R16_Float:
                    return VK_FORMAT_R16_SFLOAT;

                case PixelFormat.R32_UInt:
                    return VK_FORMAT_R32_UINT;
                case PixelFormat.R32_SInt:
                    return VK_FORMAT_R32_SINT;
                case PixelFormat.R32_Float:
                    return toDepthFormat ? VK_FORMAT_D32_SFLOAT : VK_FORMAT_R32_SFLOAT;

                case PixelFormat.R8_G8_UNorm:
                    return VK_FORMAT_R8G8_UNORM;
                case PixelFormat.R8_G8_SNorm:
                    return VK_FORMAT_R8G8_SNORM;
                case PixelFormat.R8_G8_UInt:
                    return VK_FORMAT_R8G8_UINT;
                case PixelFormat.R8_G8_SInt:
                    return VK_FORMAT_R8G8_SINT;

                case PixelFormat.R16_G16_UNorm:
                    return VK_FORMAT_R16G16_UNORM;
                case PixelFormat.R16_G16_SNorm:
                    return VK_FORMAT_R16G16_SNORM;
                case PixelFormat.R16_G16_UInt:
                    return VK_FORMAT_R16G16_UINT;
                case PixelFormat.R16_G16_SInt:
                    return VK_FORMAT_R16G16_SINT;
                case PixelFormat.R16_G16_Float:
                    return VK_FORMAT_R16G16B16A16_SFLOAT;

                case PixelFormat.R32_G32_UInt:
                    return VK_FORMAT_R32G32_UINT;
                case PixelFormat.R32_G32_SInt:
                    return VK_FORMAT_R32G32_SINT;
                case PixelFormat.R32_G32_Float:
                    return VK_FORMAT_R32G32B32A32_SFLOAT;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return VK_FORMAT_R8G8B8A8_UNORM;
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                    return VK_FORMAT_R8G8B8A8_SRGB;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return VK_FORMAT_B8G8R8A8_UNORM;
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return VK_FORMAT_B8G8R8A8_SRGB;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return VK_FORMAT_R8G8B8A8_SNORM;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return VK_FORMAT_R8G8B8A8_UINT;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return VK_FORMAT_R8G8B8A8_SINT;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                    return VK_FORMAT_R16G16B16A16_UNORM;
                case PixelFormat.R16_G16_B16_A16_SNorm:
                    return VK_FORMAT_R16G16B16A16_SNORM;
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return VK_FORMAT_R16G16B16A16_UINT;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return VK_FORMAT_R16G16B16A16_SINT;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return VK_FORMAT_R16G16B16A16_SFLOAT;

                case PixelFormat.R32_G32_B32_A32_UInt:
                    return VK_FORMAT_R32G32B32A32_UINT;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return VK_FORMAT_R32G32B32A32_SINT;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return VK_FORMAT_R32G32B32A32_SFLOAT;

                case PixelFormat.BC1_Rgb_UNorm:
                    return VK_FORMAT_BC1_RGB_UNORM_BLOCK;
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                    return VK_FORMAT_BC1_RGB_SRGB_BLOCK;
                case PixelFormat.BC1_Rgba_UNorm:
                    return VK_FORMAT_BC1_RGBA_UNORM_BLOCK;
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                    return VK_FORMAT_BC1_RGBA_SRGB_BLOCK;
                case PixelFormat.BC2_UNorm:
                    return VK_FORMAT_BC2_UNORM_BLOCK;
                case PixelFormat.BC2_UNorm_SRgb:
                    return VK_FORMAT_BC2_SRGB_BLOCK;
                case PixelFormat.BC3_UNorm:
                    return VK_FORMAT_BC3_UNORM_BLOCK;
                case PixelFormat.BC3_UNorm_SRgb:
                    return VK_FORMAT_BC3_SRGB_BLOCK;
                case PixelFormat.BC4_UNorm:
                    return VK_FORMAT_BC4_UNORM_BLOCK;
                case PixelFormat.BC4_SNorm:
                    return VK_FORMAT_BC4_SNORM_BLOCK;
                case PixelFormat.BC5_UNorm:
                    return VK_FORMAT_BC5_UNORM_BLOCK;
                case PixelFormat.BC5_SNorm:
                    return VK_FORMAT_BC5_SNORM_BLOCK;
                case PixelFormat.BC7_UNorm:
                    return VK_FORMAT_BC7_UNORM_BLOCK;
                case PixelFormat.BC7_UNorm_SRgb:
                    return VK_FORMAT_BC7_SRGB_BLOCK;

                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                    return VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK;
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                    return VK_FORMAT_ETC2_R8G8B8A1_UNORM_BLOCK;
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK;

                case PixelFormat.D32_Float_S8_UInt:
                    return VK_FORMAT_D32_SFLOAT_S8_UINT;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return VK_FORMAT_D24_UNORM_S8_UINT;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return VK_FORMAT_A2B10G10R10_UNORM_PACK32;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return VK_FORMAT_A2B10G10R10_UINT_PACK32;
                case PixelFormat.R11_G11_B10_Float:
                    return VK_FORMAT_B10G11R11_UFLOAT_PACK32;

                default:
                    throw new VeldridException($"Invalid {nameof(PixelFormat)}: {format}");
            }
        }
    }
}

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal static class OpenGLFormats
    {
        internal static DrawElementsType VdToGLDrawElementsType(IndexFormat format)
        {
            return format switch
            {
                IndexFormat.UInt16 => DrawElementsType.UnsignedShort,
                IndexFormat.UInt32 => DrawElementsType.UnsignedInt,
                _ => throw Illegal.Value<IndexFormat>(),
            };
        }

        internal static ShaderType VdToGLShaderType(ShaderStages stage)
        {
            return stage switch
            {
                ShaderStages.Vertex => ShaderType.VertexShader,
                ShaderStages.Geometry => ShaderType.GeometryShader,
                ShaderStages.TessellationControl => ShaderType.TessControlShader,
                ShaderStages.TessellationEvaluation => ShaderType.TessEvaluationShader,
                ShaderStages.Fragment => ShaderType.FragmentShader,
                ShaderStages.Compute => ShaderType.ComputeShader,
                _ => throw Illegal.Value<ShaderStages>(),
            };
        }

        internal static PixelInternalFormat VdToGLPixelInternalFormat(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.R8_UNorm => PixelInternalFormat.R8,
                PixelFormat.R8_SNorm => PixelInternalFormat.R8Snorm,
                PixelFormat.R8_UInt => PixelInternalFormat.R8ui,
                PixelFormat.R8_SInt => PixelInternalFormat.R8i,
                PixelFormat.R16_UNorm => PixelInternalFormat.R16,
                PixelFormat.R16_SNorm => PixelInternalFormat.R16Snorm,
                PixelFormat.R16_UInt => PixelInternalFormat.R16ui,
                PixelFormat.R16_SInt => PixelInternalFormat.R16i,
                PixelFormat.R16_Float => PixelInternalFormat.R16f,
                PixelFormat.R32_UInt => PixelInternalFormat.R32ui,
                PixelFormat.R32_SInt => PixelInternalFormat.R32i,
                PixelFormat.R32_Float => PixelInternalFormat.R32f,
                PixelFormat.R8_G8_UNorm => PixelInternalFormat.Rg8,
                PixelFormat.R8_G8_SNorm => PixelInternalFormat.Rg8Snorm,
                PixelFormat.R8_G8_UInt => PixelInternalFormat.Rg8ui,
                PixelFormat.R8_G8_SInt => PixelInternalFormat.Rg8i,
                PixelFormat.R16_G16_UNorm => PixelInternalFormat.Rg16,
                PixelFormat.R16_G16_SNorm => PixelInternalFormat.Rg16Snorm,
                PixelFormat.R16_G16_UInt => PixelInternalFormat.Rg16ui,
                PixelFormat.R16_G16_SInt => PixelInternalFormat.Rg16i,
                PixelFormat.R16_G16_Float => PixelInternalFormat.Rg16f,
                PixelFormat.R32_G32_UInt => PixelInternalFormat.Rg32ui,
                PixelFormat.R32_G32_SInt => PixelInternalFormat.Rg32i,
                PixelFormat.R32_G32_Float => PixelInternalFormat.Rg32f,
                PixelFormat.R8_G8_B8_A8_UNorm => PixelInternalFormat.Rgba8,
                PixelFormat.R8_G8_B8_A8_UNorm_SRgb => PixelInternalFormat.Srgb8Alpha8,
                PixelFormat.R8_G8_B8_A8_SNorm => PixelInternalFormat.Rgba8Snorm,
                PixelFormat.R8_G8_B8_A8_UInt => PixelInternalFormat.Rgba8ui,
                PixelFormat.R8_G8_B8_A8_SInt => PixelInternalFormat.Rgba8i,
                PixelFormat.R16_G16_B16_A16_UNorm => PixelInternalFormat.Rgba16,
                PixelFormat.R16_G16_B16_A16_SNorm => PixelInternalFormat.Rgba16Snorm,
                PixelFormat.R16_G16_B16_A16_UInt => PixelInternalFormat.Rgba16ui,
                PixelFormat.R16_G16_B16_A16_SInt => PixelInternalFormat.Rgba16i,
                PixelFormat.R16_G16_B16_A16_Float => PixelInternalFormat.Rgba16f,
                PixelFormat.R32_G32_B32_A32_Float => PixelInternalFormat.Rgba32f,
                PixelFormat.R32_G32_B32_A32_UInt => PixelInternalFormat.Rgba32ui,
                PixelFormat.R32_G32_B32_A32_SInt => PixelInternalFormat.Rgba32i,
                PixelFormat.B8_G8_R8_A8_UNorm => PixelInternalFormat.Rgba,
                PixelFormat.B8_G8_R8_A8_UNorm_SRgb => PixelInternalFormat.Srgb8Alpha8,
                PixelFormat.BC1_Rgb_UNorm => PixelInternalFormat.CompressedRgbS3tcDxt1Ext,
                PixelFormat.BC1_Rgb_UNorm_SRgb => PixelInternalFormat.CompressedSrgbS3tcDxt1Ext,
                PixelFormat.BC1_Rgba_UNorm => PixelInternalFormat.CompressedRgbaS3tcDxt1Ext,
                PixelFormat.BC1_Rgba_UNorm_SRgb => PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext,
                PixelFormat.BC2_UNorm => PixelInternalFormat.CompressedRgbaS3tcDxt3Ext,
                PixelFormat.BC2_UNorm_SRgb => PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext,
                PixelFormat.BC3_UNorm => PixelInternalFormat.CompressedRgbaS3tcDxt5Ext,
                PixelFormat.BC3_UNorm_SRgb => PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext,
                PixelFormat.BC4_UNorm => PixelInternalFormat.CompressedRedRgtc1,
                PixelFormat.BC4_SNorm => PixelInternalFormat.CompressedSignedRedRgtc1,
                PixelFormat.BC5_UNorm => PixelInternalFormat.CompressedRgRgtc2,
                PixelFormat.BC5_SNorm => PixelInternalFormat.CompressedSignedRgRgtc2,
                PixelFormat.BC7_UNorm => PixelInternalFormat.CompressedRgbaBptcUnorm,
                PixelFormat.BC7_UNorm_SRgb => PixelInternalFormat.CompressedSrgbAlphaBptcUnorm,
                PixelFormat.ETC2_R8_G8_B8_UNorm => PixelInternalFormat.CompressedRgb8Etc2,
                PixelFormat.ETC2_R8_G8_B8_A1_UNorm => PixelInternalFormat.CompressedRgb8PunchthroughAlpha1Etc2,
                PixelFormat.ETC2_R8_G8_B8_A8_UNorm => PixelInternalFormat.CompressedRgba8Etc2Eac,
                PixelFormat.D32_Float_S8_UInt => PixelInternalFormat.Depth32fStencil8,
                PixelFormat.D24_UNorm_S8_UInt => PixelInternalFormat.Depth24Stencil8,
                PixelFormat.R10_G10_B10_A2_UNorm => PixelInternalFormat.Rgb10A2,
                PixelFormat.R10_G10_B10_A2_UInt => PixelInternalFormat.Rgb10A2ui,
                PixelFormat.R11_G11_B10_Float => PixelInternalFormat.R11fG11fB10f,
                _ => throw Illegal.Value<PixelFormat>(),
            };
        }

        internal static TextureWrapMode VdToGLTextureWrapMode(SamplerAddressMode mode)
        {
            return mode switch
            {
                SamplerAddressMode.Wrap => TextureWrapMode.Repeat,
                SamplerAddressMode.Mirror => TextureWrapMode.MirroredRepeat,
                SamplerAddressMode.Clamp => TextureWrapMode.ClampToEdge,
                SamplerAddressMode.Border => TextureWrapMode.ClampToBorder,
                _ => throw Illegal.Value<SamplerAddressMode>(),
            };
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static GLPixelFormat VdToGLPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_Float:
                case PixelFormat.R32_Float:
                case PixelFormat.BC4_UNorm:
                    return GLPixelFormat.Red;

                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_SInt:
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_SInt:
                case PixelFormat.R32_UInt:
                case PixelFormat.R32_SInt:
                case PixelFormat.BC4_SNorm:
                    return GLPixelFormat.RedInteger;

                case PixelFormat.R8_G8_UNorm:
                case PixelFormat.R16_G16_UNorm:
                case PixelFormat.R16_G16_Float:
                case PixelFormat.R32_G32_Float:
                case PixelFormat.BC5_UNorm:
                    return GLPixelFormat.Rg;

                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R8_G8_UInt:
                case PixelFormat.R8_G8_SInt:
                case PixelFormat.R16_G16_SNorm:
                case PixelFormat.R16_G16_UInt:
                case PixelFormat.R16_G16_SInt:
                case PixelFormat.R32_G32_UInt:
                case PixelFormat.R32_G32_SInt:
                case PixelFormat.BC5_SNorm:
                    return GLPixelFormat.RgInteger;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                case PixelFormat.R16_G16_B16_A16_UNorm:
                case PixelFormat.R16_G16_B16_A16_Float:
                case PixelFormat.R32_G32_B32_A32_Float:
                    return GLPixelFormat.Rgba;

                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return GLPixelFormat.Bgra;

                case PixelFormat.R8_G8_B8_A8_SNorm:
                case PixelFormat.R8_G8_B8_A8_UInt:
                case PixelFormat.R8_G8_B8_A8_SInt:
                case PixelFormat.R16_G16_B16_A16_SNorm:
                case PixelFormat.R16_G16_B16_A16_UInt:
                case PixelFormat.R16_G16_B16_A16_SInt:
                case PixelFormat.R32_G32_B32_A32_UInt:
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return GLPixelFormat.RgbaInteger;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                    return GLPixelFormat.Rgb;
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return GLPixelFormat.Rgba;

                case PixelFormat.D24_UNorm_S8_UInt:
                    return GLPixelFormat.DepthStencil;
                case PixelFormat.D32_Float_S8_UInt:
                    return GLPixelFormat.DepthStencil;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return GLPixelFormat.Rgba;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return GLPixelFormat.RgbaInteger;
                case PixelFormat.R11_G11_B10_Float:
                    return GLPixelFormat.Rgb;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static GLPixelType VdToGLPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_G8_UNorm:
                case PixelFormat.R8_G8_UInt:
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                case PixelFormat.R8_G8_B8_A8_UInt:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return GLPixelType.UnsignedByte;
                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_SInt:
                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R8_G8_SInt:
                case PixelFormat.R8_G8_B8_A8_SNorm:
                case PixelFormat.R8_G8_B8_A8_SInt:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_SNorm:
                    return GLPixelType.Byte;
                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_G16_UNorm:
                case PixelFormat.R16_G16_UInt:
                case PixelFormat.R16_G16_B16_A16_UNorm:
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return GLPixelType.UnsignedShort;
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_SInt:
                case PixelFormat.R16_G16_SNorm:
                case PixelFormat.R16_G16_SInt:
                case PixelFormat.R16_G16_B16_A16_SNorm:
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return GLPixelType.Short;
                case PixelFormat.R32_UInt:
                case PixelFormat.R32_G32_UInt:
                case PixelFormat.R32_G32_B32_A32_UInt:
                    return GLPixelType.UnsignedInt;
                case PixelFormat.R32_SInt:
                case PixelFormat.R32_G32_SInt:
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return GLPixelType.Int;
                case PixelFormat.R16_Float:
                case PixelFormat.R16_G16_Float:
                case PixelFormat.R16_G16_B16_A16_Float:
                    return GLPixelType.HalfFloat;
                case PixelFormat.R32_Float:
                case PixelFormat.R32_G32_Float:
                case PixelFormat.R32_G32_B32_A32_Float:
                    return GLPixelType.Float;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return GLPixelType.UnsignedByte; // ?

                case PixelFormat.D32_Float_S8_UInt:
                    return GLPixelType.Float32UnsignedInt248Rev;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return GLPixelType.UnsignedInt248;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return GLPixelType.UnsignedInt1010102;
                case PixelFormat.R11_G11_B10_Float:
                    return GLPixelType.UnsignedInt10F11F11FRev;

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static SizedInternalFormat VdToGLSizedInternalFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return SizedInternalFormat.R8;
                case PixelFormat.R8_SNorm:
                    return SizedInternalFormat.R8i;
                case PixelFormat.R8_UInt:
                    return SizedInternalFormat.R8ui;
                case PixelFormat.R8_SInt:
                    return SizedInternalFormat.R8i;

                case PixelFormat.R16_UNorm:
                    return depthFormat ? (SizedInternalFormat)PixelInternalFormat.DepthComponent16 : SizedInternalFormat.R16;
                case PixelFormat.R16_SNorm:
                    return SizedInternalFormat.R16i;
                case PixelFormat.R16_UInt:
                    return SizedInternalFormat.R16ui;
                case PixelFormat.R16_SInt:
                    return SizedInternalFormat.R16i;
                case PixelFormat.R16_Float:
                    return SizedInternalFormat.R16f;

                case PixelFormat.R32_UInt:
                    return SizedInternalFormat.R32ui;
                case PixelFormat.R32_SInt:
                    return SizedInternalFormat.R32i;
                case PixelFormat.R32_Float:
                    return depthFormat ? (SizedInternalFormat)PixelInternalFormat.DepthComponent32f : SizedInternalFormat.R32f;

                case PixelFormat.R8_G8_UNorm:
                    return SizedInternalFormat.Rg8;
                case PixelFormat.R8_G8_SNorm:
                    return SizedInternalFormat.Rg8i;
                case PixelFormat.R8_G8_UInt:
                    return SizedInternalFormat.Rg8ui;
                case PixelFormat.R8_G8_SInt:
                    return SizedInternalFormat.Rg8i;

                case PixelFormat.R16_G16_UNorm:
                    return SizedInternalFormat.Rg16;
                case PixelFormat.R16_G16_SNorm:
                    return SizedInternalFormat.Rg16i;
                case PixelFormat.R16_G16_UInt:
                    return SizedInternalFormat.Rg16ui;
                case PixelFormat.R16_G16_SInt:
                    return SizedInternalFormat.Rg16i;
                case PixelFormat.R16_G16_Float:
                    return SizedInternalFormat.Rg16f;

                case PixelFormat.R32_G32_UInt:
                    return SizedInternalFormat.Rg32ui;
                case PixelFormat.R32_G32_SInt:
                    return SizedInternalFormat.Rg32i;
                case PixelFormat.R32_G32_Float:
                    return SizedInternalFormat.Rg32f;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return SizedInternalFormat.Rgba8;
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.Srgb8Alpha8;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return SizedInternalFormat.Rgba8i;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return SizedInternalFormat.Rgba8ui;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return SizedInternalFormat.Rgba8i;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return SizedInternalFormat.Rgba8;
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.Srgb8Alpha8;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                    return SizedInternalFormat.Rgba16;
                case PixelFormat.R16_G16_B16_A16_SNorm:
                    return SizedInternalFormat.Rgba16i;
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return SizedInternalFormat.Rgba16ui;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return SizedInternalFormat.Rgba16i;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return SizedInternalFormat.Rgba16f;

                case PixelFormat.R32_G32_B32_A32_UInt:
                    return SizedInternalFormat.Rgba32ui;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return SizedInternalFormat.Rgba32i;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return SizedInternalFormat.Rgba32f;

                case PixelFormat.BC1_Rgb_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSrgbS3tcDxt1Ext;
                case PixelFormat.BC1_Rgba_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext;
                case PixelFormat.BC2_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                case PixelFormat.BC2_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                case PixelFormat.BC3_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                case PixelFormat.BC3_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                case PixelFormat.BC4_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRedRgtc1;
                case PixelFormat.BC4_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSignedRedRgtc1;
                case PixelFormat.BC5_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgRgtc2;
                case PixelFormat.BC5_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSignedRgRgtc2;
                case PixelFormat.BC7_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgbaBptcUnorm;
                case PixelFormat.BC7_UNorm_SRgb:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedSrgbAlphaBptcUnorm;

                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgb8Etc2;
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgb8PunchthroughAlpha1Etc2;
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgba8Etc2Eac;

                case PixelFormat.D32_Float_S8_UInt:
                    Debug.Assert(depthFormat);
                    return (SizedInternalFormat)PixelInternalFormat.Depth32fStencil8;
                case PixelFormat.D24_UNorm_S8_UInt:
                    Debug.Assert(depthFormat);
                    return (SizedInternalFormat)PixelInternalFormat.Depth24Stencil8;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.Rgb10A2;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return (SizedInternalFormat)PixelInternalFormat.Rgb10A2ui;
                case PixelFormat.R11_G11_B10_Float:
                    return (SizedInternalFormat)PixelInternalFormat.R11fG11fB10f;

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static void VdToGLTextureMinMagFilter(SamplerFilter filter, bool mip, out TextureMinFilter min, out TextureMagFilter mag)
        {
            switch (filter)
            {
                case SamplerFilter.Anisotropic:
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    min = mip ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    min = mip ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    min = mip ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Linear;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    min = mip ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    min = mip ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    min = mip ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    min = mip ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    min = mip ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Linear;
                    break;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static BufferAccessMask VdToGLMapMode(MapMode mode)
        {
            return mode switch
            {
                MapMode.Read => BufferAccessMask.Read,
                MapMode.Write => BufferAccessMask.Write | BufferAccessMask.InvalidateBuffer | BufferAccessMask.InvalidateRange,
                MapMode.ReadWrite => BufferAccessMask.Read | BufferAccessMask.Write,
                _ => throw Illegal.Value<MapMode>(),
            };
        }

        internal static VertexAttribPointerType VdToGLVertexAttribPointerType(
            VertexElementFormat format,
            out bool normalized,
            out bool isInteger)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.Float2:
                case VertexElementFormat.Float3:
                case VertexElementFormat.Float4:
                    normalized = false;
                    isInteger = false;
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Half1:
                case VertexElementFormat.Half2:
                case VertexElementFormat.Half4:
                    normalized = false;
                    isInteger = false;
                    return VertexAttribPointerType.HalfFloat;
                case VertexElementFormat.Byte2_Norm:
                case VertexElementFormat.Byte4_Norm:
                    normalized = true;
                    isInteger = true;
                    return VertexAttribPointerType.UnsignedByte;
                case VertexElementFormat.Byte2:
                case VertexElementFormat.Byte4:
                    normalized = false;
                    isInteger = true;
                    return VertexAttribPointerType.UnsignedByte;
                case VertexElementFormat.SByte2_Norm:
                case VertexElementFormat.SByte4_Norm:
                    normalized = true;
                    isInteger = true;
                    return VertexAttribPointerType.Byte;
                case VertexElementFormat.SByte2:
                case VertexElementFormat.SByte4:
                    normalized = false;
                    isInteger = true;
                    return VertexAttribPointerType.Byte;
                case VertexElementFormat.UShort2_Norm:
                case VertexElementFormat.UShort4_Norm:
                    normalized = true;
                    isInteger = true;
                    return VertexAttribPointerType.UnsignedShort;
                case VertexElementFormat.UShort2:
                case VertexElementFormat.UShort4:
                    normalized = false;
                    isInteger = true;
                    return VertexAttribPointerType.UnsignedShort;
                case VertexElementFormat.Short2_Norm:
                case VertexElementFormat.Short4_Norm:
                    normalized = true;
                    isInteger = true;
                    return VertexAttribPointerType.Short;
                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                    normalized = false;
                    isInteger = true;
                    return VertexAttribPointerType.Short;
                case VertexElementFormat.UInt1:
                case VertexElementFormat.UInt2:
                case VertexElementFormat.UInt3:
                case VertexElementFormat.UInt4:
                    normalized = false;
                    isInteger = true;
                    return VertexAttribPointerType.UnsignedInt;
                case VertexElementFormat.Int1:
                case VertexElementFormat.Int2:
                case VertexElementFormat.Int3:
                case VertexElementFormat.Int4:
                    normalized = false;
                    isInteger = true;
                    return VertexAttribPointerType.Int;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static bool IsFormatSupported(OpenGLExtensions extensions, PixelFormat format, GraphicsBackend backend)
        {
            switch (format)
            {
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return extensions.GLESVersion(3, 0) || extensions.GLVersion(4, 3);

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                    return extensions.IsExtensionSupported("GL_EXT_texture_compression_s3tc");

                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                    return extensions.GLVersion(3, 0) || extensions.IsExtensionSupported("GL_ARB_texture_compression_rgtc");

                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                    return extensions.GLVersion(4, 2) || extensions.IsExtensionSupported("GL_ARB_texture_compression_bptc")
                        || extensions.IsExtensionSupported("GL_EXT_texture_compression_bptc");

                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                case PixelFormat.R10_G10_B10_A2_UInt:
                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return backend == GraphicsBackend.OpenGL;

                default:
                    return true;
            }
        }

        internal static DepthFunction VdToGLDepthFunction(ComparisonKind value)
        {
            return value switch
            {
                ComparisonKind.Never => DepthFunction.Never,
                ComparisonKind.Less => DepthFunction.Less,
                ComparisonKind.Equal => DepthFunction.Equal,
                ComparisonKind.LessEqual => DepthFunction.Lequal,
                ComparisonKind.Greater => DepthFunction.Greater,
                ComparisonKind.NotEqual => DepthFunction.Notequal,
                ComparisonKind.GreaterEqual => DepthFunction.Gequal,
                ComparisonKind.Always => DepthFunction.Always,
                _ => throw Illegal.Value<ComparisonKind>(),
            };
        }

        internal static BlendingFactorSrc VdToGLBlendFactorSrc(BlendFactor factor)
        {
            return factor switch
            {
                BlendFactor.Zero => BlendingFactorSrc.Zero,
                BlendFactor.One => BlendingFactorSrc.One,
                BlendFactor.SourceAlpha => BlendingFactorSrc.SrcAlpha,
                BlendFactor.InverseSourceAlpha => BlendingFactorSrc.OneMinusSrcAlpha,
                BlendFactor.DestinationAlpha => BlendingFactorSrc.DstAlpha,
                BlendFactor.InverseDestinationAlpha => BlendingFactorSrc.OneMinusDstAlpha,
                BlendFactor.SourceColor => BlendingFactorSrc.SrcColor,
                BlendFactor.InverseSourceColor => BlendingFactorSrc.OneMinusSrcColor,
                BlendFactor.DestinationColor => BlendingFactorSrc.DstColor,
                BlendFactor.InverseDestinationColor => BlendingFactorSrc.OneMinusDstColor,
                BlendFactor.BlendFactor => BlendingFactorSrc.ConstantColor,
                BlendFactor.InverseBlendFactor => BlendingFactorSrc.OneMinusConstantColor,
                _ => throw Illegal.Value<BlendFactor>(),
            };
        }

        internal static BlendEquationMode VdToGLBlendEquationMode(BlendFunction function)
        {
            return function switch
            {
                BlendFunction.Add => BlendEquationMode.FuncAdd,
                BlendFunction.Subtract => BlendEquationMode.FuncSubtract,
                BlendFunction.ReverseSubtract => BlendEquationMode.FuncReverseSubtract,
                BlendFunction.Minimum => BlendEquationMode.Min,
                BlendFunction.Maximum => BlendEquationMode.Max,
                _ => throw Illegal.Value<BlendFunction>(),
            };
        }

        internal static PolygonMode VdToGLPolygonMode(PolygonFillMode fillMode)
        {
            return fillMode switch
            {
                PolygonFillMode.Solid => PolygonMode.Fill,
                PolygonFillMode.Wireframe => PolygonMode.Line,
                _ => throw Illegal.Value<PolygonFillMode>(),
            };
        }

        internal static StencilFunction VdToGLStencilFunction(ComparisonKind comparison)
        {
            return comparison switch
            {
                ComparisonKind.Never => StencilFunction.Never,
                ComparisonKind.Less => StencilFunction.Less,
                ComparisonKind.Equal => StencilFunction.Equal,
                ComparisonKind.LessEqual => StencilFunction.Lequal,
                ComparisonKind.Greater => StencilFunction.Greater,
                ComparisonKind.NotEqual => StencilFunction.Notequal,
                ComparisonKind.GreaterEqual => StencilFunction.Gequal,
                ComparisonKind.Always => StencilFunction.Always,
                _ => throw Illegal.Value<ComparisonKind>(),
            };
        }

        internal static StencilOp VdToGLStencilOp(StencilOperation op)
        {
            return op switch
            {
                StencilOperation.Keep => StencilOp.Keep,
                StencilOperation.Zero => StencilOp.Zero,
                StencilOperation.Replace => StencilOp.Replace,
                StencilOperation.IncrementAndClamp => StencilOp.Incr,
                StencilOperation.DecrementAndClamp => StencilOp.Decr,
                StencilOperation.Invert => StencilOp.Invert,
                StencilOperation.IncrementAndWrap => StencilOp.IncrWrap,
                StencilOperation.DecrementAndWrap => StencilOp.DecrWrap,
                _ => throw Illegal.Value<StencilOperation>(),
            };
        }

        internal static CullFaceMode VdToGLCullFaceMode(FaceCullMode cullMode)
        {
            return cullMode switch
            {
                FaceCullMode.Back => CullFaceMode.Back,
                FaceCullMode.Front => CullFaceMode.Front,
                _ => throw Illegal.Value<FaceCullMode>(),
            };
        }

        internal static PrimitiveType VdToGLPrimitiveType(PrimitiveTopology primitiveTopology)
        {
            return primitiveTopology switch
            {
                PrimitiveTopology.TriangleList => PrimitiveType.Triangles,
                PrimitiveTopology.TriangleStrip => PrimitiveType.TriangleStrip,
                PrimitiveTopology.LineList => PrimitiveType.Lines,
                PrimitiveTopology.LineStrip => PrimitiveType.LineStrip,
                PrimitiveTopology.PointList => PrimitiveType.Points,
                _ => throw Illegal.Value<PrimitiveTopology>(),
            };
        }

        internal static FrontFaceDirection VdToGLFrontFaceDirection(FrontFace frontFace)
        {
            return frontFace switch
            {
                FrontFace.Clockwise => FrontFaceDirection.Cw,
                FrontFace.CounterClockwise => FrontFaceDirection.Ccw,
                _ => throw Illegal.Value<FrontFace>(),
            };
        }

        internal static BlendingFactorDest VdToGLBlendFactorDest(BlendFactor factor)
        {
            return factor switch
            {
                BlendFactor.Zero => BlendingFactorDest.Zero,
                BlendFactor.One => BlendingFactorDest.One,
                BlendFactor.SourceAlpha => BlendingFactorDest.SrcAlpha,
                BlendFactor.InverseSourceAlpha => BlendingFactorDest.OneMinusSrcAlpha,
                BlendFactor.DestinationAlpha => BlendingFactorDest.DstAlpha,
                BlendFactor.InverseDestinationAlpha => BlendingFactorDest.OneMinusDstAlpha,
                BlendFactor.SourceColor => BlendingFactorDest.SrcColor,
                BlendFactor.InverseSourceColor => BlendingFactorDest.OneMinusSrcColor,
                BlendFactor.DestinationColor => BlendingFactorDest.DstColor,
                BlendFactor.InverseDestinationColor => BlendingFactorDest.OneMinusDstColor,
                BlendFactor.BlendFactor => BlendingFactorDest.ConstantColor,
                BlendFactor.InverseBlendFactor => BlendingFactorDest.OneMinusConstantColor,
                _ => throw Illegal.Value<BlendFactor>(),
            };
        }
    }
}

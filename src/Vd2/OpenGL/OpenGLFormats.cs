using System;
using Vd2.OpenGLBinding;

namespace Vd2.OpenGL
{
    internal static class OpenGLFormats
    {
        internal static DrawElementsType VdToGLDrawElementsType(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return DrawElementsType.UnsignedShort;
                case IndexFormat.UInt32:
                    return DrawElementsType.UnsignedInt;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static ShaderType VdToGLShaderType(ShaderStages stage)
        {
            switch (stage)
            {
                case ShaderStages.Vertex:
                    return ShaderType.VertexShader;
                case ShaderStages.Geometry:
                    return ShaderType.GeometryShader;
                case ShaderStages.TessellationControl:
                    return ShaderType.TessControlShader;
                case ShaderStages.TessellationEvaluation:
                    return ShaderType.TessEvaluationShader;
                case ShaderStages.Fragment:
                    return ShaderType.FragmentShader;
                default:
                    throw Illegal.Value<ShaderStages>();
            }
        }

        internal static TextureWrapMode VdToGLTextureWrapMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return TextureWrapMode.Repeat;
                case SamplerAddressMode.Mirror:
                    return TextureWrapMode.MirroredRepeat;
                case SamplerAddressMode.Clamp:
                    return TextureWrapMode.Clamp;
                case SamplerAddressMode.Border:
                    return TextureWrapMode.ClampToBorder;
                default:
                    throw Illegal.Value<SamplerAddressMode>();
            }
        }

        internal static GLPixelFormat VdToGLPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return GLPixelFormat.Rgba;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return GLPixelFormat.Bgra;
                case PixelFormat.R8_UNorm:
                    return GLPixelFormat.RedInteger;
                case PixelFormat.R16_UNorm:
                    return GLPixelFormat.RedInteger;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return GLPixelFormat.Rgba;
                case PixelFormat.R32_Float:
                    return GLPixelFormat.Red;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static GLPixelType VdToGLPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.R8_UNorm:
                    return GLPixelType.UnsignedByte;
                case PixelFormat.R16_UNorm:
                    return GLPixelType.UnsignedShort;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return GLPixelType.Float;
                case PixelFormat.R32_Float:
                    return GLPixelType.Float;
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

        internal static DepthFunction VdToGLDepthFunction(DepthComparisonKind value)
        {
            switch (value)
            {
                case DepthComparisonKind.Never:
                    return DepthFunction.Never;
                case DepthComparisonKind.Less:
                    return DepthFunction.Less;
                case DepthComparisonKind.Equal:
                    return DepthFunction.Equal;
                case DepthComparisonKind.LessEqual:
                    return DepthFunction.Lequal;
                case DepthComparisonKind.Greater:
                    return DepthFunction.Greater;
                case DepthComparisonKind.NotEqual:
                    return DepthFunction.Notequal;
                case DepthComparisonKind.GreaterEqual:
                    return DepthFunction.Gequal;
                case DepthComparisonKind.Always:
                    return DepthFunction.Always;
                default:
                    throw Illegal.Value<DepthComparisonKind>();
            }
        }
    }
}